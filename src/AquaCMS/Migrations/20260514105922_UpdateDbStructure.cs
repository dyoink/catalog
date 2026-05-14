using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AquaCMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,")
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "banners",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    subtitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    link_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banners", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chat_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    guest_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    unread_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_message = table.Column<string>(type: "text", nullable: true),
                    last_seen_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "page_views",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entity_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    referrer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    viewed_at = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_page_views", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "partner_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "site_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    company_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    logo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    facebook = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    show_facebook = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    zalo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    show_zalo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    youtube = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    show_youtube = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tiktok = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    show_tiktok = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    telegram = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    show_telegram = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    show_hotline = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    bank_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    bank_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bank_owner = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    background_color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    primary_color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    footer_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    show_footer = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_banners = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_categories = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_featured_products = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_latest_posts = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_partners = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    featured_products_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 8),
                    latest_posts_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 6),
                    show_nav_products = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_nav_knowledge = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_nav_partners = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_nav_cart = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_nav_about = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    about_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    about_content = table.Column<string>(type: "text", nullable: true),
                    about_image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    default_meta_title = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    default_meta_description = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    default_og_image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    google_analytics_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    facebook_pixel_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    footer_about_text = table.Column<string>(type: "text", nullable: true),
                    copyright_text = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    smtp_host = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    smtp_port = table.Column<int>(type: "integer", nullable: false, defaultValue: 587),
                    smtp_use_ssl = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    smtp_user = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    smtp_password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    smtp_from_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    smtp_from_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    notification_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    chat_auto_reply_message = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    avatar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    short_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_from_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_chat_messages_chat_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "chat_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    short_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    excerpt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    author = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    knowledge_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    read_time = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    published_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    view_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    meta_title = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    meta_desc = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.id);
                    table.ForeignKey(
                        name: "FK_posts_knowledge_categories_knowledge_category_id",
                        column: x => x.knowledge_category_id,
                        principalTable: "knowledge_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "partners",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    short_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    detailed_description = table.Column<string>(type: "text", nullable: true),
                    partner_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    since = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partners", x => x.id);
                    table.ForeignKey(
                        name: "FK_partners_partner_categories_partner_category_id",
                        column: x => x.partner_category_id,
                        principalTable: "partner_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "activity_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Info"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_activity_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "product_contents",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    content_blocks = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    video_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_contents", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_product_contents_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_finances",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,0)", nullable: true),
                    show_price = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_finances", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_product_finances_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_metadata",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    meta_title = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    meta_desc = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_metadata", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_product_metadata_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_statistics",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    view_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_statistics", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_product_statistics_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_created_at",
                table: "activity_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_entity_type_entity_id",
                table: "activity_logs",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_user_id",
                table: "activity_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_session_id",
                table: "chat_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_sessions_guest_id",
                table: "chat_sessions",
                column: "guest_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_categories_slug",
                table: "knowledge_categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_page_views_path_viewed_at",
                table: "page_views",
                columns: new[] { "path", "viewed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_page_views_viewed_at",
                table: "page_views",
                column: "viewed_at");

            migrationBuilder.CreateIndex(
                name: "IX_partner_categories_slug",
                table: "partner_categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_partners_partner_category_id",
                table: "partners",
                column: "partner_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_partners_short_id",
                table: "partners",
                column: "short_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_partners_slug",
                table: "partners",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_posts_knowledge_category_id",
                table: "posts",
                column: "knowledge_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_posts_short_id",
                table: "posts",
                column: "short_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_posts_slug",
                table: "posts",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_metadata_slug",
                table: "product_metadata",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_short_id",
                table: "products",
                column: "short_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_sku",
                table: "products",
                column: "sku",
                unique: true,
                filter: "sku IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_logs");

            migrationBuilder.DropTable(
                name: "banners");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "page_views");

            migrationBuilder.DropTable(
                name: "partners");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DropTable(
                name: "product_contents");

            migrationBuilder.DropTable(
                name: "product_finances");

            migrationBuilder.DropTable(
                name: "product_metadata");

            migrationBuilder.DropTable(
                name: "product_statistics");

            migrationBuilder.DropTable(
                name: "site_settings");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "chat_sessions");

            migrationBuilder.DropTable(
                name: "partner_categories");

            migrationBuilder.DropTable(
                name: "knowledge_categories");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
