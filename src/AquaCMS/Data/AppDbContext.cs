using System.Text.Json;
using AquaCMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Data;

/// <summary>
/// DbContext chính — kết nối EF Core với PostgreSQL.
/// Sử dụng snake_case naming convention cho tên bảng/cột (chuẩn PostgreSQL).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ===== DbSets — mỗi DbSet tương ứng 1 bảng =====
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductMetadata> ProductMetadata => Set<ProductMetadata>();
    public DbSet<ProductContent> ProductContents => Set<ProductContent>();
    public DbSet<ProductFinance> ProductFinances => Set<ProductFinance>();
    public DbSet<ProductStatistic> ProductStatistics => Set<ProductStatistic>();
    public DbSet<KnowledgeCategory> KnowledgeCategories => Set<KnowledgeCategory>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PartnerCategory> PartnerCategories => Set<PartnerCategory>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<PageView> PageViews => Set<PageView>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===================================================
        // PostgreSQL: Extensions
        // ===================================================
        modelBuilder.HasPostgresExtension("uuid-ossp");
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.HasPostgresExtension("unaccent");

        // ===================================================
        // User
        // ===================================================
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasColumnName("role")
                  .HasConversion<string>()
                  .IsRequired();
            entity.Property(e => e.Avatar).HasColumnName("avatar").HasMaxLength(500);

            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ===================================================
        // Category
        // ===================================================
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(120).IsRequired();
            entity.Property(e => e.Image).HasColumnName("image").HasMaxLength(500);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // ===================================================
        // Product (Core)
        // ===================================================
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.ShortId).HasColumnName("short_id").UseSerialColumn();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(100);
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Status).HasColumnName("status")
                  .HasConversion(
                      v => v == ProductStatus.Available ? "available" :
                           v == ProductStatus.OutOfStock ? "out_of_stock" : "hidden",
                      v => v == "available" ? ProductStatus.Available :
                           v == "out_of_stock" ? ProductStatus.OutOfStock : ProductStatus.Hidden)
                  .IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Sku).IsUnique().HasFilter("sku IS NOT NULL");
            entity.HasIndex(e => e.ShortId).IsUnique();

            // Relationship: Product → Category (nhiều-một)
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Relationships (1:1)
            entity.HasOne(e => e.Metadata)
                  .WithOne(m => m.Product)
                  .HasForeignKey<ProductMetadata>(m => m.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Content)
                  .WithOne(c => c.Product)
                  .HasForeignKey<ProductContent>(c => c.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Finance)
                  .WithOne(f => f.Product)
                  .HasForeignKey<ProductFinance>(f => f.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Statistic)
                  .WithOne(s => s.Product)
                  .HasForeignKey<ProductStatistic>(s => s.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================================================
        // Product Sub-tables
        // ===================================================
        modelBuilder.Entity<ProductMetadata>(entity =>
        {
            entity.ToTable("product_metadata");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(200).IsRequired();
            entity.Property(e => e.MetaTitle).HasColumnName("meta_title").HasMaxLength(70);
            entity.Property(e => e.MetaDesc).HasColumnName("meta_desc").HasMaxLength(160);
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<ProductContent>(entity =>
        {
            entity.ToTable("product_contents");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ContentBlocks).HasColumnName("content_blocks")
                  .HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
            entity.Property(e => e.Image).HasColumnName("image").HasMaxLength(500);
            entity.Property(e => e.VideoUrl).HasColumnName("video_url").HasMaxLength(500);
        });

        modelBuilder.Entity<ProductFinance>(entity =>
        {
            entity.ToTable("product_finances");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(18,0)");
            entity.Property(e => e.ShowPrice).HasColumnName("show_price").HasDefaultValue(true);
            entity.Property(e => e.IsFeatured).HasColumnName("is_featured").HasDefaultValue(false);
        });

        modelBuilder.Entity<ProductStatistic>(entity =>
        {
            entity.ToTable("product_statistics");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ViewCount).HasColumnName("view_count").HasDefaultValue(0);
        });

        // ===================================================
        // KnowledgeCategory
        // ===================================================
        modelBuilder.Entity<KnowledgeCategory>(entity =>
        {
            entity.ToTable("knowledge_categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(120).IsRequired();
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // ===================================================
        // Post
        // ===================================================
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("posts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.ShortId).HasColumnName("short_id").UseSerialColumn();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Excerpt).HasColumnName("excerpt");
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.Image).HasColumnName("image").HasMaxLength(500);
            entity.Property(e => e.Author).HasColumnName("author").HasMaxLength(100);
            entity.Property(e => e.KnowledgeCategoryId).HasColumnName("knowledge_category_id");
            entity.Property(e => e.ReadTime).HasColumnName("read_time").HasMaxLength(20);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(false);
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.ViewCount).HasColumnName("view_count").HasDefaultValue(0);
            entity.Property(e => e.MetaTitle).HasColumnName("meta_title").HasMaxLength(70);
            entity.Property(e => e.MetaDesc).HasColumnName("meta_desc").HasMaxLength(160);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Slug).IsUnique();

            entity.HasOne(e => e.KnowledgeCategory)
                  .WithMany(c => c.Posts)
                  .HasForeignKey(e => e.KnowledgeCategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ShortId).IsUnique();
        });

        // ===================================================
        // PartnerCategory
        // ===================================================
        modelBuilder.Entity<PartnerCategory>(entity =>
        {
            entity.ToTable("partner_categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(120).IsRequired();
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // ===================================================
        // Partner
        // ===================================================
        modelBuilder.Entity<Partner>(entity =>
        {
            entity.ToTable("partners");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.ShortId).HasColumnName("short_id").UseSerialColumn();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DetailedDescription).HasColumnName("detailed_description");
            entity.Property(e => e.PartnerCategoryId).HasColumnName("partner_category_id");
            entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(255);
            entity.Property(e => e.Since).HasColumnName("since").HasMaxLength(10);
            entity.Property(e => e.Image).HasColumnName("image").HasMaxLength(500);
            entity.Property(e => e.ContactEmail).HasColumnName("contact_email").HasMaxLength(255);
            entity.Property(e => e.ContactPhone).HasColumnName("contact_phone").HasMaxLength(30);
            entity.Property(e => e.Website).HasColumnName("website").HasMaxLength(500);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Slug).IsUnique();

            entity.HasOne(e => e.PartnerCategory)
                  .WithMany(c => c.Partners)
                  .HasForeignKey(e => e.PartnerCategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ShortId).IsUnique();
        });

        // ===================================================
        // Banner
        // ===================================================
        modelBuilder.Entity<Banner>(entity =>
        {
            entity.ToTable("banners");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Subtitle).HasColumnName("subtitle").HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Image).HasColumnName("image").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Color).HasColumnName("color").HasMaxLength(100);
            entity.Property(e => e.LinkUrl).HasColumnName("link_url").HasMaxLength(500);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        // ===================================================
        // SiteSettings (singleton — 1 row)
        // ===================================================
        modelBuilder.Entity<SiteSettings>(entity =>
        {
            entity.ToTable("site_settings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.CompanyName).HasColumnName("company_name").HasMaxLength(255);
            entity.Property(e => e.Logo).HasColumnName("logo").HasMaxLength(500);
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(30);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            // Social
            entity.Property(e => e.Facebook).HasColumnName("facebook").HasMaxLength(500);
            entity.Property(e => e.ShowFacebook).HasColumnName("show_facebook").HasDefaultValue(true);
            entity.Property(e => e.Zalo).HasColumnName("zalo").HasMaxLength(500);
            entity.Property(e => e.ShowZalo).HasColumnName("show_zalo").HasDefaultValue(true);
            entity.Property(e => e.Youtube).HasColumnName("youtube").HasMaxLength(500);
            entity.Property(e => e.ShowYoutube).HasColumnName("show_youtube").HasDefaultValue(false);
            entity.Property(e => e.Tiktok).HasColumnName("tiktok").HasMaxLength(500);
            entity.Property(e => e.ShowTiktok).HasColumnName("show_tiktok").HasDefaultValue(false);
            entity.Property(e => e.Telegram).HasColumnName("telegram").HasMaxLength(500);
            entity.Property(e => e.ShowTelegram).HasColumnName("show_telegram").HasDefaultValue(false);
            entity.Property(e => e.ShowHotline).HasColumnName("show_hotline").HasDefaultValue(true);
            // Payment
            entity.Property(e => e.BankName).HasColumnName("bank_name").HasMaxLength(255);
            entity.Property(e => e.BankNumber).HasColumnName("bank_number").HasMaxLength(50);
            entity.Property(e => e.BankOwner).HasColumnName("bank_owner").HasMaxLength(255);
            // UI
            entity.Property(e => e.BackgroundColor).HasColumnName("background_color").HasMaxLength(20);
            entity.Property(e => e.PrimaryColor).HasColumnName("primary_color").HasMaxLength(20);
            entity.Property(e => e.FooterText).HasColumnName("footer_text").HasMaxLength(500);
            entity.Property(e => e.ShowFooter).HasColumnName("show_footer").HasDefaultValue(true);
            entity.Property(e => e.HeroBackgroundImage).HasColumnName("hero_background_image").HasMaxLength(500);
            // CMS module toggles
            entity.Property(e => e.ShowBanners).HasColumnName("show_banners").HasDefaultValue(true);
            entity.Property(e => e.ShowCategories).HasColumnName("show_categories").HasDefaultValue(true);
            entity.Property(e => e.ShowFeaturedProducts).HasColumnName("show_featured_products").HasDefaultValue(true);
            entity.Property(e => e.ShowLatestPosts).HasColumnName("show_latest_posts").HasDefaultValue(true);
            entity.Property(e => e.ShowPartners).HasColumnName("show_partners").HasDefaultValue(true);
            entity.Property(e => e.FeaturedProductsCount).HasColumnName("featured_products_count").HasDefaultValue(8);
            entity.Property(e => e.LatestPostsCount).HasColumnName("latest_posts_count").HasDefaultValue(6);
            // Navbar toggles
            entity.Property(e => e.ShowNavProducts).HasColumnName("show_nav_products").HasDefaultValue(true);
            entity.Property(e => e.ShowNavKnowledge).HasColumnName("show_nav_knowledge").HasDefaultValue(true);
            entity.Property(e => e.ShowNavPartners).HasColumnName("show_nav_partners").HasDefaultValue(true);
            entity.Property(e => e.ShowNavCart).HasColumnName("show_nav_cart").HasDefaultValue(true);
            // Hero / Intro
            entity.Property(e => e.HeroTitle).HasColumnName("hero_title").HasMaxLength(255);
            entity.Property(e => e.HeroSubtitle).HasColumnName("hero_subtitle").HasMaxLength(255);
            entity.Property(e => e.HeroDescription).HasColumnName("hero_description");
            entity.Property(e => e.HeroButtonText).HasColumnName("hero_button_text").HasMaxLength(100);
            entity.Property(e => e.HeroButtonUrl).HasColumnName("hero_button_url").HasMaxLength(500);
            // About
            entity.Property(e => e.AboutTitle).HasColumnName("about_title").HasMaxLength(255);
            entity.Property(e => e.AboutContent).HasColumnName("about_content");
            entity.Property(e => e.AboutImage).HasColumnName("about_image").HasMaxLength(500);
            // SEO
            entity.Property(e => e.DefaultMetaTitle).HasColumnName("default_meta_title").HasMaxLength(70);
            entity.Property(e => e.DefaultMetaDescription).HasColumnName("default_meta_description").HasMaxLength(160);
            entity.Property(e => e.DefaultOgImage).HasColumnName("default_og_image").HasMaxLength(500);
            entity.Property(e => e.GoogleAnalyticsId).HasColumnName("google_analytics_id").HasMaxLength(50);
            entity.Property(e => e.FacebookPixelId).HasColumnName("facebook_pixel_id").HasMaxLength(50);
            // Footer extras
            entity.Property(e => e.FooterAboutText).HasColumnName("footer_about_text");
            entity.Property(e => e.CopyrightText).HasColumnName("copyright_text").HasMaxLength(255);
            // Email / SMTP
            entity.Property(e => e.EmailEnabled).HasColumnName("email_enabled").HasDefaultValue(false);
            entity.Property(e => e.SmtpHost).HasColumnName("smtp_host").HasMaxLength(255);
            entity.Property(e => e.SmtpPort).HasColumnName("smtp_port").HasDefaultValue(587);
            entity.Property(e => e.SmtpUseSsl).HasColumnName("smtp_use_ssl").HasDefaultValue(true);
            entity.Property(e => e.SmtpUser).HasColumnName("smtp_user").HasMaxLength(255);
            entity.Property(e => e.SmtpPassword).HasColumnName("smtp_password").HasMaxLength(255);
            entity.Property(e => e.SmtpFromEmail).HasColumnName("smtp_from_email").HasMaxLength(255);
            entity.Property(e => e.SmtpFromName).HasColumnName("smtp_from_name").HasMaxLength(255);
            entity.Property(e => e.NotificationEmail).HasColumnName("notification_email").HasMaxLength(255);
            entity.Property(e => e.ChatAutoReplyMessage).HasColumnName("chat_auto_reply_message");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ===================================================
        // ActivityLog — truy vết hành động admin
        // ===================================================
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("activity_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserName).HasColumnName("user_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
            entity.Property(e => e.EntityType).HasColumnName("entity_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.EntityId).HasColumnName("entity_id").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(e => e.Severity).HasColumnName("severity").HasMaxLength(20).HasDefaultValue("Info");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });        // ===================================================
        // ChatSession
        // ===================================================
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.ToTable("chat_sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.GuestId).HasColumnName("guest_id").HasMaxLength(50).IsRequired();
            entity.Property(e => e.UnreadCount).HasColumnName("unread_count").HasDefaultValue(0);
            entity.Property(e => e.LastMessage).HasColumnName("last_message");
            entity.Property(e => e.LastSeenAt).HasColumnName("last_seen_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.GuestId).IsUnique();
        });

        // ===================================================
        // ChatMessage
        // ===================================================
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("chat_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.SessionId).HasColumnName("session_id").IsRequired();
            entity.Property(e => e.SenderId).HasColumnName("sender_id").HasMaxLength(50).IsRequired();
            entity.Property(e => e.IsFromAdmin).HasColumnName("is_from_admin").HasDefaultValue(false);
            entity.Property(e => e.Text).HasColumnName("text").IsRequired();
            entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Session)
                  .WithMany(s => s.Messages)
                  .HasForeignKey(e => e.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================================================
        // PageView
        // ===================================================
        modelBuilder.Entity<PageView>(entity =>
        {
            entity.ToTable("page_views");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Path).HasColumnName("path").HasMaxLength(500).IsRequired();
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType).HasColumnName("entity_type").HasMaxLength(20);
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(e => e.Referrer).HasColumnName("referrer").HasMaxLength(500);
            entity.Property(e => e.ViewedAt).HasColumnName("viewed_at").HasDefaultValueSql("CURRENT_DATE");

            entity.HasIndex(e => e.ViewedAt);
            entity.HasIndex(e => new { e.Path, e.ViewedAt });
        });
    }
}
