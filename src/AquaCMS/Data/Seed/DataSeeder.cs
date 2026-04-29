using AquaCMS.Helpers;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AquaCMS.Data.Seed;

/// <summary>
/// Seed dữ liệu ban đầu + test data khi database trống.
/// Chạy 1 lần duy nhất khi khởi động app lần đầu.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // Đợi DB sẵn sàng
        var retries = 5;
        while (retries > 0)
        {
            try
            {
                if (await db.Database.CanConnectAsync())
                {
                    logger.LogInformation("✅ Kết nối Database thành công.");
                    break;
                }
            }
            catch (Exception ex)
            {
                retries--;
                logger.LogWarning("⚠️ Không kết nối được DB: {Message}. Thử lại sau 2 giây... (còn {Retries} lần)", ex.Message, retries);
                await Task.Delay(2000);
            }
        }

        // Tự động tạo Schema nếu chưa có (vì không dùng EF Migrations)
        logger.LogInformation("⏳ Đang kiểm tra cấu trúc Database...");
        
        // Kiểm tra xem đã có bảng nào chưa (tránh lỗi khi đã có ENUM nhưng chưa có TABLE)
        var hasTables = false;
        try
        {
            // Thử query một bảng bất kỳ
            await db.Users.AnyAsync();
            hasTables = true;
        }
        catch
        {
            // Bảng chưa tồn tại
            hasTables = false;
        }

        if (!hasTables)
        {
            logger.LogInformation("Empty database detected. Creating schema...");
            await db.Database.EnsureCreatedAsync();
            logger.LogInformation("✅ Cấu trúc Database đã được khởi tạo.");
        }
        else
        {
            logger.LogInformation("✅ Cấu trúc Database đã sẵn sàng.");
        }

        // ===== Create Implicit Casts for Enums (EF Core String Mapping) =====
        try
        {
            await db.Database.ExecuteSqlRawAsync(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_type WHERE typname = 'user_role') AND NOT EXISTS (
                        SELECT 1 FROM pg_cast c
                        JOIN pg_type s ON c.castsource = s.oid
                        JOIN pg_type t ON c.casttarget = t.oid
                        WHERE s.typname = 'text' AND t.typname = 'user_role'
                    ) THEN
                        CREATE CAST (text AS user_role) WITH INOUT AS IMPLICIT;
                    END IF;
                END $$;");
            logger.LogInformation("✅ Checked/Created implicit cast for user_role");
        }
        catch (Exception ex)
        {
            logger.LogWarning("⚠️ Could not create cast for user_role: {Message}", ex.Message);
        }

        try
        {
            await db.Database.ExecuteSqlRawAsync(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_type WHERE typname = 'product_status') AND NOT EXISTS (
                        SELECT 1 FROM pg_cast c
                        JOIN pg_type s ON c.castsource = s.oid
                        JOIN pg_type t ON c.casttarget = t.oid
                        WHERE s.typname = 'text' AND t.typname = 'product_status'
                    ) THEN
                        CREATE CAST (text AS product_status) WITH INOUT AS IMPLICIT;
                    END IF;
                END $$;");
            logger.LogInformation("✅ Checked/Created implicit cast for product_status");
        }
        catch (Exception ex)
        {
            logger.LogWarning("⚠️ Could not create cast for product_status: {Message}", ex.Message);
        }

        // ===== Seed Admin User =====
        logger.LogInformation("⏳ Checking for admin user...");
        var hasAdmin = await db.Users.AnyAsync(u => u.Email == "admin@aquacms.com");
        logger.LogInformation("✅ Admin check completed");
        if (!hasAdmin)
        {
            logger.LogInformation("⏳ Creating admin user...");
            db.Users.Add(new User
            {
                Name = "System Admin",
                Email = "admin@aquacms.com",
                PasswordHash = authService.HashPassword("Admin@123"),
                Role = UserRole.SUPER_ADMIN,
                IsActive = true
            });
            await db.SaveChangesAsync();
            logger.LogInformation("✅ Seed admin: admin@aquacms.com / Admin@123");
        }
        else
        {
            logger.LogInformation("✅ Admin user already exists");
        }

        // ===== Cập nhật password hash cho user có PLACEHOLDER =====
        logger.LogInformation("⏳ Checking for placeholder users...");
        try
        {
            var placeholderUsers = await db.Users
                .Where(u => u.PasswordHash == "PLACEHOLDER_WILL_BE_SEEDED_BY_APP")
                .ToListAsync();

            foreach (var user in placeholderUsers)
            {
                user.PasswordHash = authService.HashPassword("Admin@123");
                logger.LogInformation("✅ Cập nhật password cho: {Email}", user.Email);
            }
            if (placeholderUsers.Count > 0) await db.SaveChangesAsync();
            logger.LogInformation("✅ Placeholder users check completed");
        }
        catch (Exception ex)
        {
            logger.LogWarning("⚠️ Failed to check placeholder users: {Message}", ex.Message);
        }

        // ===== Seed SiteSettings =====
        logger.LogInformation("⏳ Checking SiteSettings...");
        try
        {
            if (!await db.SiteSettings.AnyAsync())
            {
                db.SiteSettings.Add(new SiteSettings
                {
                    CompanyName = "AquaCMS",
                    Phone = "0353785710",
                    Email = "contact@aquacms.com",
                    Address = "123 Đường Nguyễn Huệ, Q.1, TP.HCM",
                    PrimaryColor = "#55B3D9",
                    BackgroundColor = "#F9F9F9",
                    FooterText = "Giải pháp thiết bị nuôi trồng thủy sản hàng đầu.",
                    ShowFooter = true,
                    ShowZalo = true,
                    ShowFacebook = true,
                    ShowHotline = true,
                    ShowBanners = true,
                    ShowCategories = true,
                    ShowFeaturedProducts = true,
                    ShowLatestPosts = true,
                    ShowPartners = true,
                    FeaturedProductsCount = 8,
                    LatestPostsCount = 6
                });
                await db.SaveChangesAsync();
                logger.LogInformation("✅ Seed SiteSettings mặc định");
            }
            else
            {
                logger.LogInformation("✅ SiteSettings already exists");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("⚠️ Failed to seed SiteSettings: {Message}", ex.Message);
        }

        // ===== Seed Test Data =====
        logger.LogInformation("⏳ Checking for test data...");
        try
        {
            await SeedTestDataAsync(db, logger);
            logger.LogInformation("✅ Test data check completed");
        }
        catch (Exception ex)
        {
            logger.LogWarning("⚠️ Failed to seed test data: {Message}", ex.Message);
        }

        logger.LogInformation("✅ Database seeding completed successfully!");
    }

    private static async Task SeedTestDataAsync(AppDbContext db, ILogger logger)
    {
        // Kiểm tra xem đã có dữ liệu sản phẩm chưa
        if (await db.Products.AnyAsync())
        {
            logger.LogInformation("⏭️ Test data already exists, skipping seeding.");
            return;
        }

        logger.LogInformation("⏭️  Test data seeding started...");

        // ===== Categories =====
        var categoriesData = new[]
        {
            new Category { Name = "Máy cho tôm ăn", Slug = "may-cho-tom-an", Image = "https://placehold.co/400x300/55B3D9/FFF?text=M%C3%A1y+cho+t%C3%B4m+%C4%83n", SortOrder = 1 },
            new Category { Name = "Máy sục khí", Slug = "may-suc-khi", Image = "https://placehold.co/400x300/2196F3/FFF?text=M%C3%A1y+s%E1%BB%A5c+kh%C3%AD", SortOrder = 2 },
            new Category { Name = "Thiết bị đo lường", Slug = "thiet-bi-do-luong", Image = "https://placehold.co/400x300/4CAF50/FFF?text=%C4%90o+l%C6%B0%E1%BB%9Dng", SortOrder = 3 },
            new Category { Name = "Hệ thống lọc nước", Slug = "he-thong-loc-nuoc", Image = "https://placehold.co/400x300/FF9800/FFF?text=L%E1%BB%8Dc+n%C6%B0%E1%BB%9Bc", SortOrder = 4 },
            new Category { Name = "Phụ kiện ao nuôi", Slug = "phu-kien-ao-nuoi", Image = "https://placehold.co/400x300/9C27B0/FFF?text=Ph%E1%BB%A5+ki%E1%BB%87n", SortOrder = 5 },
            new Category { Name = "Hóa chất xử lý", Slug = "hoa-chat-xu-ly", Image = "https://placehold.co/400x300/F44336/FFF?text=H%C3%B3a+ch%E1%BA%A5t", SortOrder = 6 },
        };

        foreach (var cat in categoriesData)
        {
            if (!await db.Categories.AnyAsync(c => c.Slug == cat.Slug))
            {
                db.Categories.Add(cat);
            }
        }
        await db.SaveChangesAsync();

        // Get all categories for product reference
        var categories = await db.Categories.ToListAsync();
        var catAn = categories.FirstOrDefault(c => c.Slug == "may-cho-tom-an")?.Id ?? categories[0].Id;

        // ===== Products =====
        var productData = new[]
        {
            ("Máy cho tôm ăn tự động 360°", catAn, 12500000m, "Máy cho tôm ăn tự động xoay 360 độ, phân phối đều thức ăn khắp ao nuôi."),
            ("Máy cho ăn mini 180°", catAn, 6800000m, "Phiên bản mini cho ao nuôi nhỏ, xoay 180 độ, tiết kiệm điện."),
            ("Máy cho ăn thông minh IoT", catAn, 25000000m, "Tích hợp IoT, điều khiển qua app điện thoại, hẹn giờ tự động."),
        };

        var count = 0;
        foreach (var (name, catId, price, desc) in productData)
        {
            count++;
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = name,
                Sku = $"SP-{count:D4}",
                CategoryId = catId,
                Status = ProductStatus.Available,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 60)),
                UpdatedAt = DateTime.UtcNow
            };

            db.Products.Add(product);

            db.ProductMetadata.Add(new ProductMetadata
            {
                ProductId = productId,
                Slug = SlugHelper.GenerateSlug(name),
                MetaTitle = name
            });

            db.ProductContents.Add(new ProductContent
            {
                ProductId = productId,
                Description = desc,
                Image = $"https://placehold.co/600x400/55B3D9/FFF?text={Uri.EscapeDataString(name.Split(' ')[0])}",
                ContentBlocks = JsonDocument.Parse("[]")
            });

            db.ProductFinances.Add(new ProductFinance
            {
                ProductId = productId,
                Price = price,
                IsFeatured = count == 1,
                ShowPrice = true
            });

            db.ProductStatistics.Add(new ProductStatistic
            {
                ProductId = productId,
                ViewCount = Random.Shared.Next(100, 1000)
            });
        }
        await db.SaveChangesAsync();

        // (Rest of the seeding for Knowledge, Posts, Partners, Banners stays same as they aren't affected by Product splitting)
        // ... (I'll keep them to ensure the file remains complete)
        
        // ===== Knowledge Categories =====
        var knowledgeCats = new[]
        {
            new KnowledgeCategory { Name = "Kỹ thuật nuôi tôm", Slug = "ky-thuat-nuoi-tom", SortOrder = 1 },
            new KnowledgeCategory { Name = "Quản lý ao nuôi", Slug = "quan-ly-ao-nuoi", SortOrder = 2 },
            new KnowledgeCategory { Name = "Tin tức ngành", Slug = "tin-tuc-nganh", SortOrder = 3 },
        };
        db.Set<KnowledgeCategory>().AddRange(knowledgeCats);
        await db.SaveChangesAsync();

        // ===== Posts =====
        var posts = new[]
        {
            new Post
            {
                Title = "5 bước chuẩn bị ao nuôi tôm đạt chuẩn",
                Slug = "5-buoc-chuan-bi-ao-nuoi-tom-dat-chuan",
                Excerpt = "Hướng dẫn chi tiết quy trình chuẩn bị ao nuôi tôm từ A-Z, đảm bảo tỷ lệ thành công cao nhất.",
                Content = "<h2>Bước 1: Cải tạo ao</h2><p>Sau mỗi vụ nuôi, ao cần được cải tạo kỹ lưỡng. Bao gồm tháo cạn nước, phơi đáy ao 7-10 ngày, bón vôi CaO với liều lượng 100-150 kg/1000m².</p><h2>Bước 2: Xử lý nước</h2><p>Cấp nước qua túi lọc, xử lý bằng Chlorine 30ppm, chạy quạt 2-3 ngày để bay hết dư lượng Chlorine.</p><h2>Bước 3: Gây màu nước</h2><p>Sử dụng vi sinh và phân bón sinh học để gây màu nước đạt độ trong 30-40cm.</p><h2>Bước 4: Kiểm tra chỉ tiêu</h2><p>pH: 7.5-8.5, Kiềm: 120-150 mg/L, DO > 5mg/L, NH3 < 0.1mg/L.</p><h2>Bước 5: Thả giống</h2><p>Thả giống vào sáng sớm hoặc chiều mát, mật độ 100-150 con/m².</p>",
                Author = "Admin",
                KnowledgeCategoryId = knowledgeCats[0].Id,
                ReadTime = "5 phút",
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-5),
                Image = "https://placehold.co/800x450/4CAF50/FFF?text=Chu%E1%BA%A9n+b%E1%BB%8B+ao",
            }
        };
        db.Posts.AddRange(posts);
        await db.SaveChangesAsync();

        // ===== Partners =====
        var partnerCats = new[]
        {
            new PartnerCategory { Name = "Nhà phân phối", Slug = "nha-phan-phoi", SortOrder = 1 },
        };
        db.Set<PartnerCategory>().AddRange(partnerCats);
        await db.SaveChangesAsync();

        var partners = new[]
        {
            new Partner
            {
                Name = "Công ty TNHH Thủy sản Miền Tây",
                Slug = "cong-ty-thuy-san-mien-tay",
                Description = "Nhà phân phối thiết bị nuôi trồng thủy sản lớn nhất ĐBSCL.",
                Location = "Cà Mau",
                Since = "2018",
                PartnerCategoryId = partnerCats[0].Id,
                IsActive = true,
                SortOrder = 1,
                Image = "https://placehold.co/200x200/55B3D9/FFF?text=MT",
            }
        };
        db.Partners.AddRange(partners);
        await db.SaveChangesAsync();

        // ===== Banners =====
        var banners = new[]
        {
            new Banner
            {
                Title = "Máy cho tôm ăn thế hệ mới",
                Subtitle = "SẢN PHẨM MỚI 2025",
                Description = "Công nghệ xoay 360°, IoT tích hợp, điều khiển qua smartphone. Tiết kiệm 30% thức ăn.",
                Image = "https://placehold.co/800x500/55B3D9/FFF?text=M%C3%A1y+cho+%C4%83n+360%C2%B0",
                LinkUrl = "/san-pham",
                SortOrder = 1,
                IsActive = true,
            }
        };
        db.Banners.AddRange(banners);
        await db.SaveChangesAsync();

        logger.LogInformation("✅ Seed test data hoàn tất");
    }
}
