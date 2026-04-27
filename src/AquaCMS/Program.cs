using System.Security.Claims;
using System.Threading.RateLimiting;
using AquaCMS.Data;
using AquaCMS.Data.Seed;
using AquaCMS.Models.Entities;
using AquaCMS.Modules.Core.Middleware;
using AquaCMS.Services;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ============================================================
// AquaCMS — ASP.NET Core MVC Application
// ============================================================

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/aquacms-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // 1. Database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    builder.Services.AddDbContext<AppDbContext>(options => {
        options.UseNpgsql(connectionString);
        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.ManyServiceProvidersCreatedWarning));
    });

    // 2. Auth
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options => {
            options.LoginPath = "/dang-nhap";
            options.AccessDeniedPath = "/khong-co-quyen";
            options.Cookie.Name = "AquaCMS.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromHours(24);
        });

    builder.Services.AddAuthorization(options => {
        options.AddPolicy("SuperAdmin", p => p.RequireClaim(ClaimTypes.Role, "SUPER_ADMIN"));
        options.AddPolicy("ManagerUp", p => p.RequireClaim(ClaimTypes.Role, "SUPER_ADMIN", "MANAGER"));
        options.AddPolicy("EditorUp", p => p.RequireClaim(ClaimTypes.Role, "SUPER_ADMIN", "MANAGER", "EDITOR"));
        options.AddPolicy("AnyAdmin", p => p.RequireClaim(ClaimTypes.Role, "SUPER_ADMIN", "MANAGER", "EDITOR", "SALE"));
    });

    // 3. Services
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<ISettingsService, SettingsService>();
    builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
    builder.Services.AddScoped<IPartnerService, PartnerService>();
    builder.Services.AddScoped<IBannerService, BannerService>();
    builder.Services.AddScoped<IFileUploadService, FileUploadService>();
    builder.Services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
    builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
    builder.Services.AddScoped<ISearchService, SearchService>();
    builder.Services.AddScoped<ISitemapService, SitemapService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<ILocalizationService, LocalizationService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddMemoryCache();
    builder.Services.AddControllersWithViews();
    builder.Services.AddSignalR();

    // 4. Rate Limiter
    builder.Services.AddRateLimiter(options => {
        options.AddFixedWindowLimiter("login", opt => {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(1);
        });
    });

    var app = builder.Build();

    // Seed Data
    await DataSeeder.SeedAsync(app.Services);

    app.UseGlobalExceptionHandler();
    app.UseSecurityHeaders();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
    app.UseStaticFiles();
    app.UsePageViewTracking();
    app.UseStatusCodePagesWithReExecute("/loi/{0}");
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // Route Mapping
    // 1. Auth routes (Priority)
    app.MapControllerRoute(name: "login", pattern: "dang-nhap", defaults: new { controller = "Account", action = "Login" });
    app.MapControllerRoute(name: "logout", pattern: "dang-xuat", defaults: new { controller = "Account", action = "Logout" });

    // 2. Admin Area
    app.MapControllerRoute(name: "admin", pattern: "admin/{controller=Dashboard}/{action=Index}/{id?}", defaults: new { area = "Admin" });

    // 3. SEO / Friendly routes
    app.MapControllerRoute(name: "product-detail", pattern: "san-pham/{slugAndId}", defaults: new { controller = "Product", action = "Detail" });
    app.MapControllerRoute(name: "product-list", pattern: "san-pham", defaults: new { controller = "Product", action = "Index" });
    app.MapControllerRoute(name: "category-products", pattern: "danh-muc/{slug}", defaults: new { controller = "Product", action = "Category" });
    app.MapControllerRoute(name: "knowledge-detail", pattern: "kien-thuc/{slugAndId}", defaults: new { controller = "Knowledge", action = "Detail" });
    app.MapControllerRoute(name: "knowledge-list", pattern: "kien-thuc", defaults: new { controller = "Knowledge", action = "Index" });
    app.MapControllerRoute(name: "partner-detail", pattern: "doi-tac/{slugAndId}", defaults: new { controller = "Partner", action = "Detail" });
    app.MapControllerRoute(name: "partner-list", pattern: "doi-tac", defaults: new { controller = "Partner", action = "Index" });
    app.MapControllerRoute(name: "cart", pattern: "gio-hang", defaults: new { controller = "Cart", action = "Index" });
    
    // 4. Utilities
    app.MapControllerRoute(name: "sitemap", pattern: "sitemap.xml", defaults: new { controller = "Seo", action = "Sitemap" });
    app.MapControllerRoute(name: "robots", pattern: "robots.txt", defaults: new { controller = "Seo", action = "Robots" });
    app.MapControllerRoute(name: "error-status", pattern: "loi/{code:int}", defaults: new { controller = "Home", action = "Error" });
    app.MapControllerRoute(name: "api-search", pattern: "api/search/{action}", defaults: new { controller = "SearchApi" });

    // 5. Default
    app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
    app.MapHub<AquaCMS.Hubs.ChatHub>("/hubs/chat");

    app.Run();
}
catch (Exception ex) { Log.Fatal(ex, "Terminated"); }
finally { Log.CloseAndFlush(); }
