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

        // ===== Seed Admin User =====
        logger.LogInformation("⏳ Checking for admin user...");
        var hasAdmin = await db.Users.AnyAsync(u => u.Role == UserRole.SUPER_ADMIN);
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
        // Test data seeding disabled to improve startup performance
        // To enable, remove the return statement below and rebuild
        logger.LogInformation("⏭️  Test data seeding skipped (disabled for performance)");
        return;

        // ===== Categories =====
        var categories = new[]
        {
            new Category { Name = "Máy cho tôm ăn", Slug = "may-cho-tom-an", Image = "https://placehold.co/400x300/55B3D9/FFF?text=M%C3%A1y+cho+t%C3%B4m+%C4%83n", SortOrder = 1 },
            new Category { Name = "Máy sục khí", Slug = "may-suc-khi", Image = "https://placehold.co/400x300/2196F3/FFF?text=M%C3%A1y+s%E1%BB%A5c+kh%C3%AD", SortOrder = 2 },
            new Category { Name = "Thiết bị đo lường", Slug = "thiet-bi-do-luong", Image = "https://placehold.co/400x300/4CAF50/FFF?text=%C4%90o+l%C6%B0%E1%BB%9Dng", SortOrder = 3 },
            new Category { Name = "Hệ thống lọc nước", Slug = "he-thong-loc-nuoc", Image = "https://placehold.co/400x300/FF9800/FFF?text=L%E1%BB%8Dc+n%C6%B0%E1%BB%9Bc", SortOrder = 4 },
            new Category { Name = "Phụ kiện ao nuôi", Slug = "phu-kien-ao-nuoi", Image = "https://placehold.co/400x300/9C27B0/FFF?text=Ph%E1%BB%A5+ki%E1%BB%87n", SortOrder = 5 },
            new Category { Name = "Hóa chất xử lý", Slug = "hoa-chat-xu-ly", Image = "https://placehold.co/400x300/F44336/FFF?text=H%C3%B3a+ch%E1%BA%A5t", SortOrder = 6 },
        };
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        // ===== Products =====
        var products = new List<Product>();
        var productData = new[]
        {
            ("Máy cho tôm ăn tự động 360°", categories[0].Id, 12500000m, "Máy cho tôm ăn tự động xoay 360 độ, phân phối đều thức ăn khắp ao nuôi."),
            ("Máy cho ăn mini 180°", categories[0].Id, 6800000m, "Phiên bản mini cho ao nuôi nhỏ, xoay 180 độ, tiết kiệm điện."),
            ("Máy cho ăn thông minh IoT", categories[0].Id, 25000000m, "Tích hợp IoT, điều khiển qua app điện thoại, hẹn giờ tự động."),
            ("Máy sục khí 2HP", categories[1].Id, 8500000m, "Máy sục khí công suất 2HP, tạo oxy cho ao nuôi 1000m²."),
            ("Máy sục khí 5HP", categories[1].Id, 15000000m, "Máy sục khí công suất lớn 5HP, cho ao nuôi công nghiệp."),
            ("Máy thổi khí Roots Blower", categories[1].Id, 35000000m, "Roots Blower nhập khẩu, bền bỉ, hiệu suất cao."),
            ("Bộ đo pH/DO cầm tay", categories[2].Id, 3200000m, "Máy đo pH và oxy hòa tan cầm tay, kết quả nhanh chính xác."),
            ("Sensor nhiệt độ WiFi", categories[2].Id, 1500000m, "Cảm biến nhiệt độ nước kết nối WiFi, cảnh báo realtime."),
            ("Bộ test nước 7 chỉ tiêu", categories[2].Id, 890000m, "Bộ kit test nhanh 7 chỉ tiêu: pH, DO, NH3, NO2, kiềm, cứng, Cl."),
            ("Hệ thống lọc tuần hoàn RAS", categories[3].Id, 85000000m, "Hệ thống lọc tuần hoàn khép kín RAS cho nuôi tôm siêu thâm canh."),
            ("Bộ lọc drum filter", categories[3].Id, 45000000m, "Lọc thùng quay tự động, loại bỏ cặn lơ lửng hiệu quả."),
            ("Lọc sinh học moving bed", categories[3].Id, 12000000m, "Hệ thống lọc sinh học giá thể di động, xử lý ammonia."),
            ("Bạt HDPE lót ao", categories[4].Id, 35000m, "Bạt HDPE 0.5mm lót ao nuôi tôm, chống thấm tuyệt đối. Giá/m²."),
            ("Ống nước PVC phi 60", categories[4].Id, 45000m, "Ống PVC phi 60mm, chịu áp lực tốt. Giá/mét."),
            ("Lưới che nắng 70%", categories[4].Id, 28000m, "Lưới che nắng 70%, bảo vệ ao khỏi nhiệt độ cao. Giá/m²."),
            ("Chlorine Ca(OCl)₂ 70%", categories[5].Id, 280000m, "Chlorine dạng bột, hàm lượng 70%, khử trùng ao nuôi. Giá/kg."),
            ("Vi sinh xử lý đáy ao", categories[5].Id, 350000m, "Vi sinh Bacillus đậm đặc, xử lý đáy ao, giảm khí độc. Giá/lít."),
            ("Khoáng tổng hợp cho tôm", categories[5].Id, 180000m, "Khoáng đa vi lượng bổ sung cho tôm, tăng cứng vỏ. Giá/kg."),
        };

        foreach (var (name, catId, price, desc) in productData)
        {
            products.Add(new Product
            {
                Name = name,
                Slug = SlugHelper.GenerateSlug(name),
                CategoryId = catId,
                Price = price,
                Description = desc,
                Image = $"https://placehold.co/600x400/55B3D9/FFF?text={Uri.EscapeDataString(name.Split(' ')[0])}",
                Status = ProductStatus.Available,
                IsFeatured = products.Count < 8,
                ContentBlocks = JsonDocument.Parse("[]"),
                Sku = $"SP-{products.Count + 1:D4}",
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 60)),
                UpdatedAt = DateTime.UtcNow
            });
        }
        db.Products.AddRange(products);
        await db.SaveChangesAsync();

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
            },
            new Post
            {
                Title = "Cách sử dụng máy cho tôm ăn hiệu quả",
                Slug = "cach-su-dung-may-cho-tom-an-hieu-qua",
                Excerpt = "Tối ưu lượng thức ăn, giảm FCR với máy cho ăn tự động — kinh nghiệm thực tế.",
                Content = "<h2>Nguyên tắc cho ăn</h2><p>Chia nhỏ bữa ăn thành 4-6 lần/ngày. Máy cho ăn tự động giúp phân phối đều, giảm lãng phí thức ăn.</p><h2>Cài đặt thời gian</h2><p>Sáng: 6h, 9h. Trưa: 12h. Chiều: 15h, 18h. Tối: 21h (tùy giai đoạn).</p><h2>Điều chỉnh lượng ăn</h2><p>Theo dõi sàng ăn, điều chỉnh tăng/giảm 5-10% mỗi lần. FCR mục tiêu: 1.1-1.3.</p>",
                Author = "Admin",
                KnowledgeCategoryId = knowledgeCats[0].Id,
                ReadTime = "4 phút",
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-3),
                Image = "https://placehold.co/800x450/2196F3/FFF?text=M%C3%A1y+cho+%C4%83n",
            },
            new Post
            {
                Title = "Quản lý oxy hòa tan trong ao nuôi tôm",
                Slug = "quan-ly-oxy-hoa-tan-trong-ao-nuoi-tom",
                Excerpt = "DO thấp là nguyên nhân hàng đầu gây chết tôm. Hướng dẫn quản lý oxy hiệu quả.",
                Content = "<h2>Tầm quan trọng của DO</h2><p>Oxy hòa tan (DO) cần duy trì > 4mg/L. Dưới 3mg/L tôm bắt đầu nổi đầu, dưới 2mg/L có thể chết hàng loạt.</p><h2>Giải pháp</h2><p>Sử dụng máy sục khí, quạt nước, hoặc Roots Blower tùy quy mô ao. Bật quạt 24/24 vào giai đoạn cuối vụ.</p>",
                Author = "Admin",
                KnowledgeCategoryId = knowledgeCats[1].Id,
                ReadTime = "3 phút",
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-1),
                Image = "https://placehold.co/800x450/FF9800/FFF?text=Oxy+DO",
            },
            new Post
            {
                Title = "Xu hướng nuôi tôm công nghệ cao 2025",
                Slug = "xu-huong-nuoi-tom-cong-nghe-cao-2025",
                Excerpt = "Cập nhật các xu hướng nuôi tôm ứng dụng công nghệ IoT, AI, và tự động hóa.",
                Content = "<h2>IoT trong thủy sản</h2><p>Sensor giám sát pH, DO, nhiệt độ realtime, cảnh báo qua app. Giảm rủi ro, tăng năng suất.</p><h2>AI dự đoán dịch bệnh</h2><p>Các hệ thống AI phân tích dữ liệu ao nuôi, dự đoán và cảnh báo sớm dịch bệnh.</p>",
                Author = "Admin",
                KnowledgeCategoryId = knowledgeCats[2].Id,
                ReadTime = "6 phút",
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-10),
                Image = "https://placehold.co/800x450/9C27B0/FFF?text=C%C3%B4ng+ngh%E1%BB%87",
            },
            new Post
            {
                Title = "So sánh các loại máy sục khí cho ao nuôi",
                Slug = "so-sanh-cac-loai-may-suc-khi-cho-ao-nuoi",
                Excerpt = "Phân tích ưu nhược điểm của quạt nước, máy sục khí, Roots Blower.",
                Content = "<h2>Quạt nước</h2><p>Chi phí thấp, phù hợp ao nhỏ. Nhược: tốn điện, tạo sóng mạnh.</p><h2>Máy sục khí</h2><p>Hiệu quả trung bình, giá vừa phải. Phù hợp ao 500-2000m².</p><h2>Roots Blower</h2><p>Hiệu suất cao nhất, tiết kiệm điện dài hạn. Đầu tư ban đầu lớn.</p>",
                Author = "Admin",
                KnowledgeCategoryId = knowledgeCats[0].Id,
                ReadTime = "5 phút",
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-7),
                Image = "https://placehold.co/800x450/F44336/FFF?text=S%E1%BB%A5c+kh%C3%AD",
            },
            new Post
            {
                Title = "Bí quyết giảm FCR trong nuôi tôm",
                Slug = "bi-quyet-giam-fcr-trong-nuoi-tom",
                Excerpt = "Hệ số chuyển đổi thức ăn (FCR) ảnh hưởng trực tiếp đến lợi nhuận.",
                Content = "<h2>FCR là gì?</h2><p>FCR = Tổng thức ăn / Tổng trọng lượng tôm thu hoạch. FCR tốt: 1.1-1.3.</p><h2>Cách giảm FCR</h2><p>1. Sử dụng máy cho ăn tự động. 2. Kiểm tra sàng ăn thường xuyên. 3. Cho ăn đúng giờ, đúng lượng. 4. Quản lý chất lượng nước tốt.</p>",
                Author = "Admin",
                KnowledgeCategoryId = knowledgeCats[1].Id,
                ReadTime = "4 phút",
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-15),
                Image = "https://placehold.co/800x450/607D8B/FFF?text=FCR",
            },
        };
        db.Posts.AddRange(posts);
        await db.SaveChangesAsync();

        // ===== Partner Categories =====
        var partnerCats = new[]
        {
            new PartnerCategory { Name = "Nhà phân phối", Slug = "nha-phan-phoi", SortOrder = 1 },
            new PartnerCategory { Name = "Đối tác công nghệ", Slug = "doi-tac-cong-nghe", SortOrder = 2 },
        };
        db.Set<PartnerCategory>().AddRange(partnerCats);
        await db.SaveChangesAsync();

        // ===== Partners =====
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
            },
            new Partner
            {
                Name = "Aquatech Solutions",
                Slug = "aquatech-solutions",
                Description = "Đối tác công nghệ IoT cho thủy sản thông minh.",
                Location = "TP.HCM",
                Since = "2020",
                PartnerCategoryId = partnerCats[1].Id,
                IsActive = true,
                SortOrder = 2,
                Image = "https://placehold.co/200x200/2196F3/FFF?text=AT",
            },
            new Partner
            {
                Name = "Đại lý Thắng Lợi",
                Slug = "dai-ly-thang-loi",
                Description = "Đại lý phân phối khu vực Bạc Liêu, Sóc Trăng.",
                Location = "Bạc Liêu",
                Since = "2019",
                PartnerCategoryId = partnerCats[0].Id,
                IsActive = true,
                SortOrder = 3,
                Image = "https://placehold.co/200x200/4CAF50/FFF?text=TL",
            },
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
            },
            new Banner
            {
                Title = "Hệ thống RAS tuần hoàn khép kín",
                Subtitle = "GIẢI PHÁP NUÔI THÂM CANH",
                Description = "Nuôi tôm mật độ cao 300 con/m², tiết kiệm nước 90%, không xả thải ra môi trường.",
                Image = "https://placehold.co/800x500/2196F3/FFF?text=RAS+System",
                LinkUrl = "/danh-muc/he-thong-loc-nuoc",
                SortOrder = 2,
                IsActive = true,
            },
        };
        db.Banners.AddRange(banners);
        await db.SaveChangesAsync();

        logger.LogInformation("✅ Seed test data hoàn tất: {Products} sản phẩm, {Posts} bài viết, {Partners} đối tác, {Banners} banners",
            products.Count, posts.Length, partners.Length, banners.Length);
    }
}
