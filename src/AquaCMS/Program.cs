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
// Server-rendered cho SEO tối ưu, cookie-based auth.
// Serilog structured logging, rate limiting, security headers.
// ============================================================

// Cấu hình Serilog — structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/aquacms-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{

var builder = WebApplication.CreateBuilder(args);

// Dùng Serilog thay default logger
builder.Host.UseSerilog();

// ===== 1. Database — EF Core + PostgreSQL =====
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql =>
        {
            // Map C# enum sang PostgreSQL enum
            // Sử dụng nameTranslator để giữ nguyên chữ HOA (SUPER_ADMIN) khớp với DB script
            npgsql.MapEnum<UserRole>("user_role", nameTranslator: new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
            npgsql.MapEnum<ProductStatus>("product_status");
        });
});

// ===== 2. Authentication — Cookie-based (không JWT) =====
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/dang-nhap";           // Redirect khi chưa đăng nhập
        options.AccessDeniedPath = "/khong-co-quyen"; // Redirect khi không có quyền
        options.Cookie.Name = "AquaCMS.Auth";
        options.Cookie.HttpOnly = true;              // Chống XSS đọc cookie
        options.Cookie.SameSite = SameSiteMode.Lax;  // CSRF protection
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);  // Session 24 giờ
        options.SlidingExpiration = true;            // Tự gia hạn khi còn hoạt động
    });

// ===== 3. Authorization Policies =====
builder.Services.AddAuthorization(options =>
{
    // Policy cho SUPER_ADMIN — toàn quyền
    options.AddPolicy("SuperAdmin", policy =>
        policy.RequireClaim(ClaimTypes.Role, "SUPER_ADMIN"));

    // Policy cho Manager trở lên — quản lý sản phẩm, đối tác, settings
    options.AddPolicy("ManagerUp", policy =>
        policy.RequireClaim(ClaimTypes.Role, "SUPER_ADMIN", "MANAGER"));

    // Policy cho Editor trở lên — quản lý bài viết
    options.AddPolicy("EditorUp", policy =>
        policy.RequireClaim(ClaimTypes.Role, "SUPER_ADMIN", "MANAGER", "EDITOR"));

    // Policy cho tất cả admin — xem được mọi thứ
    options.AddPolicy("AnyAdmin", policy =>
        policy.RequireClaim(ClaimTypes.Role, "SUPER_ADMIN", "MANAGER", "EDITOR", "SALE"));
});

// ===== 4. Services — Dependency Injection =====
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

// ===== 5. Memory Cache — dùng cho settings, sitemap =====
builder.Services.AddMemoryCache();

// ===== 6. MVC + Razor Views =====
builder.Services.AddControllersWithViews();

// ===== 7. SignalR — real-time chat =====
builder.Services.AddSignalR();

// ===== 8. Rate Limiting — chống brute-force, DDoS =====
builder.Services.AddRateLimiter(options =>
{
    // Policy cho login — 5 attempts / 1 phút / IP
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    // Policy cho API — 60 requests / 1 phút / IP
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 2;
    });

    // Global — 200 requests / 1 phút / IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ============================================================
// BUILD APP
// ============================================================

var app = builder.Build();

// ===== Seed data khi khởi động =====
await DataSeeder.SeedAsync(app.Services);
Log.Information("✅ Seeding completed, setting up middleware...");

// ===== Middleware Pipeline — thứ tự quan trọng! =====

// Global exception handler — ĐẦU TIÊN, bắt mọi lỗi
app.UseGlobalExceptionHandler();
Log.Information("✅ Global exception handler registered");

// Security headers — CSP, X-Frame-Options, etc.
app.UseSecurityHeaders();
Log.Information("✅ Security headers registered");

// Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "{RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000}ms";
});
Log.Information("✅ Serilog request logging registered");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}
Log.Information("✅ Environment middleware registered");

// Rate limiting — chống brute force
app.UseRateLimiter();
Log.Information("✅ Rate limiter registered");

// Static files (wwwroot: CSS, JS, images, uploads)
app.UseStaticFiles();
Log.Information("✅ Static files registered");

// Page view tracking — analytics dashboard (chỉ track sau static files)
app.UsePageViewTracking();
Log.Information("✅ Page view tracking registered");

// Status code pages — custom 404/500 with re-execute (giu URL gốc)
app.UseStatusCodePagesWithReExecute("/loi/{0}");
Log.Information("✅ Status code pages registered");

app.UseRouting();
Log.Information("✅ Routing registered");

// Auth middleware — phải đặt SAU routing, TRƯỚC authorization
app.UseAuthentication();
app.UseAuthorization();

// ===== Route mapping =====

// Admin Area — phải map trước default route
app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" });

// SEO-friendly routes cho từng module (tiếng Việt không dấu)
// URL dạng: /san-pham/may-cho-tom-an-12345  (slug + dash + shortId)
app.MapControllerRoute(
    name: "product-detail",
    pattern: "san-pham/{slugAndId}",
    defaults: new { controller = "Product", action = "Detail" });

app.MapControllerRoute(
    name: "product-list",
    pattern: "san-pham",
    defaults: new { controller = "Product", action = "Index" });

app.MapControllerRoute(
    name: "category-products",
    pattern: "danh-muc/{slug}",
    defaults: new { controller = "Product", action = "Category" });

app.MapControllerRoute(
    name: "knowledge-detail",
    pattern: "kien-thuc/{slugAndId}",
    defaults: new { controller = "Knowledge", action = "Detail" });

app.MapControllerRoute(
    name: "knowledge-list",
    pattern: "kien-thuc",
    defaults: new { controller = "Knowledge", action = "Index" });

app.MapControllerRoute(
    name: "partner-detail",
    pattern: "doi-tac/{slugAndId}",
    defaults: new { controller = "Partner", action = "Detail" });

app.MapControllerRoute(
    name: "partner-list",
    pattern: "doi-tac",
    defaults: new { controller = "Partner", action = "Index" });

app.MapControllerRoute(
    name: "cart",
    pattern: "gio-hang",
    defaults: new { controller = "Cart", action = "Index" });

app.MapControllerRoute(
    name: "login",
    pattern: "dang-nhap",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "logout",
    pattern: "dang-xuat",
    defaults: new { controller = "Account", action = "Logout" });

// SEO: sitemap.xml + robots.txt + error pages
app.MapControllerRoute(
    name: "sitemap",
    pattern: "sitemap.xml",
    defaults: new { controller = "Seo", action = "Sitemap" });

app.MapControllerRoute(
    name: "robots",
    pattern: "robots.txt",
    defaults: new { controller = "Seo", action = "Robots" });

app.MapControllerRoute(
    name: "error-status",
    pattern: "loi/{code:int}",
    defaults: new { controller = "Home", action = "Error" });

// API search suggestions
app.MapControllerRoute(
    name: "api-search",
    pattern: "api/search/{action}",
    defaults: new { controller = "SearchApi" });

// Access denied
app.MapControllerRoute(
    name: "access-denied",
    pattern: "khong-co-quyen",
    defaults: new { controller = "Account", action = "AccessDenied" });

// Default fallback route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// SignalR hub endpoint
app.MapHub<AquaCMS.Hubs.ChatHub>("/hubs/chat");

Log.Information("✅ All routes and hubs mapped successfully! Starting server...");
app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "AquaCMS terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
