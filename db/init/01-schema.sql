-- ============================================================
-- CatalogaWeb — Database Schema (PostgreSQL 16)
-- Chạy tự động khi docker compose up lần đầu
-- ============================================================

-- 1. Extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";   -- Hỗ trợ uuid_generate_v4()
CREATE EXTENSION IF NOT EXISTS "pg_trgm";     -- Hỗ trợ tìm kiếm ILIKE nhanh với GIN index
CREATE EXTENSION IF NOT EXISTS "unaccent";    -- Bỏ dấu tiếng Việt khi search

-- 2. ENUM types
CREATE TYPE user_role      AS ENUM ('SUPER_ADMIN', 'MANAGER', 'EDITOR', 'SALE');
CREATE TYPE product_status AS ENUM ('available', 'out_of_stock', 'hidden');

-- ============================================================
-- 3. TABLES
-- ============================================================

-- 3.1 Users (Quản trị viên — không có user khách hàng)
CREATE TABLE users (
    id             UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name           VARCHAR(100) NOT NULL,
    email          VARCHAR(255) NOT NULL UNIQUE,
    password_hash  VARCHAR(255) NOT NULL,           -- Argon2id hash
    role           user_role NOT NULL DEFAULT 'EDITOR',
    avatar         VARCHAR(500),
    is_active      BOOLEAN NOT NULL DEFAULT true,
    last_login_at  TIMESTAMPTZ,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.2 Site Settings (singleton — chỉ 1 row)
CREATE TABLE site_settings (
    id               UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_name     VARCHAR(255) NOT NULL DEFAULT 'AquaCMS',
    logo             VARCHAR(500),
    address          TEXT,
    phone            VARCHAR(30),
    email            VARCHAR(255),
    -- Social + floating toggles
    facebook         VARCHAR(500),
    show_facebook    BOOLEAN NOT NULL DEFAULT true,
    zalo             VARCHAR(500),
    show_zalo        BOOLEAN NOT NULL DEFAULT true,
    youtube          VARCHAR(500),
    show_youtube     BOOLEAN NOT NULL DEFAULT false,
    tiktok           VARCHAR(500),
    show_tiktok      BOOLEAN NOT NULL DEFAULT false,
    telegram         VARCHAR(500),
    show_telegram    BOOLEAN NOT NULL DEFAULT false,
    show_hotline     BOOLEAN NOT NULL DEFAULT true,
    -- Payment
    bank_name        VARCHAR(255),
    bank_number      VARCHAR(50),
    bank_owner       VARCHAR(255),
    -- UI
    background_color VARCHAR(20) DEFAULT '#F9F9F9',
    primary_color    VARCHAR(20) DEFAULT '#55B3D9',
    navbar_color     VARCHAR(20) DEFAULT '#2563eb',
    footer_color     VARCHAR(20) DEFAULT '#1F2937',
    footer_text      VARCHAR(500),
    show_footer      BOOLEAN NOT NULL DEFAULT true,
    hero_background_image VARCHAR(500),
    -- CMS homepage modules
    show_banners           BOOLEAN NOT NULL DEFAULT true,
    show_categories        BOOLEAN NOT NULL DEFAULT true,
    show_featured_products BOOLEAN NOT NULL DEFAULT true,
    show_latest_posts      BOOLEAN NOT NULL DEFAULT true,
    show_partners          BOOLEAN NOT NULL DEFAULT true,
    featured_products_count INT NOT NULL DEFAULT 8,
    latest_posts_count      INT NOT NULL DEFAULT 6,
    -- CMS navbar toggles
    show_nav_products   BOOLEAN NOT NULL DEFAULT true,
    show_nav_knowledge  BOOLEAN NOT NULL DEFAULT true,
    show_nav_partners   BOOLEAN NOT NULL DEFAULT true,
    show_nav_cart       BOOLEAN NOT NULL DEFAULT true,
    -- CMS — Hero / Intro
    hero_title              VARCHAR(255),
    hero_subtitle           VARCHAR(255),
    hero_description        TEXT,
    hero_button_text        VARCHAR(100),
    hero_button_url         VARCHAR(500),
    -- CMS — About
    about_title             VARCHAR(255),
    about_content           TEXT,
    about_image             VARCHAR(500),
    -- SEO defaults
    default_meta_title      VARCHAR(70),
    default_meta_description VARCHAR(160),
    default_og_image        VARCHAR(500),
    google_analytics_id     VARCHAR(50),
    facebook_pixel_id       VARCHAR(50),
    -- Footer extras
    footer_about_text       TEXT,
    copyright_text          VARCHAR(255),
    -- Email / SMTP
    email_enabled           BOOLEAN NOT NULL DEFAULT FALSE,
    smtp_host               VARCHAR(255),
    smtp_port               INTEGER NOT NULL DEFAULT 587,
    smtp_use_ssl            BOOLEAN NOT NULL DEFAULT TRUE,
    smtp_user               VARCHAR(255),
    smtp_password           VARCHAR(255),
    smtp_from_email         VARCHAR(255),
    smtp_from_name          VARCHAR(255),
    notification_email      VARCHAR(255),
    updated_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.13 Activity Logs (Truy vết thao tác admin)
CREATE TABLE activity_logs (
    id           UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id      UUID REFERENCES users(id) ON DELETE SET NULL,
    user_name    VARCHAR(100) NOT NULL,
    action       VARCHAR(50) NOT NULL,
    entity_type  VARCHAR(50) NOT NULL,
    entity_id    VARCHAR(100),
    description  TEXT,
    ip_address   VARCHAR(45),
    user_agent   VARCHAR(500),
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.3 Categories (Danh mục sản phẩm — flat, không tree)
CREATE TABLE categories (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name        VARCHAR(100) NOT NULL,
    slug        VARCHAR(120) NOT NULL UNIQUE,
    image       VARCHAR(500),
    sort_order  INT NOT NULL DEFAULT 0,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.4 Products
CREATE TABLE products (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    short_id        BIGSERIAL UNIQUE,                -- Mã số ngắn cho URL SEO: /san-pham/{slug}-{short_id}
    slug            VARCHAR(200) NOT NULL UNIQUE,
    name            VARCHAR(255) NOT NULL,
    sku             VARCHAR(100) UNIQUE,
    category_id     UUID REFERENCES categories(id) ON DELETE SET NULL,
    price           DECIMAL(18,0),                  -- NULL = "Liên hệ"
    description     TEXT,                            -- Mô tả ngắn cho listing
    image           VARCHAR(500),                    -- Ảnh chính
    video_url       VARCHAR(500),
    status          product_status NOT NULL DEFAULT 'available',
    content_blocks  JSONB NOT NULL DEFAULT '[]',     -- Mảng ContentBlock
    view_count      INT NOT NULL DEFAULT 0,
    meta_title      VARCHAR(70),                     -- SEO
    meta_desc       VARCHAR(160),                    -- SEO
    is_featured     BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.5 Knowledge Categories (Danh mục bài viết)
CREATE TABLE knowledge_categories (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name        VARCHAR(100) NOT NULL,
    slug        VARCHAR(120) NOT NULL UNIQUE,
    sort_order  INT NOT NULL DEFAULT 0,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.6 Posts (Bài viết kiến thức)
CREATE TABLE posts (
    id                     UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    short_id               BIGSERIAL UNIQUE,            -- Mã số ngắn cho URL SEO
    slug                   VARCHAR(200) NOT NULL UNIQUE,
    title                  VARCHAR(255) NOT NULL,
    excerpt                TEXT,                        -- Tóm tắt
    content                TEXT NOT NULL,               -- HTML content
    image                  VARCHAR(500),
    author                 VARCHAR(100) NOT NULL DEFAULT 'Admin',
    knowledge_category_id  UUID REFERENCES knowledge_categories(id) ON DELETE SET NULL,
    read_time              VARCHAR(20),                 -- "8 phút"
    is_published           BOOLEAN NOT NULL DEFAULT false,
    published_at           TIMESTAMPTZ,
    view_count             INT NOT NULL DEFAULT 0,
    meta_title             VARCHAR(70),
    meta_desc              VARCHAR(160),
    created_at             TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at             TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.7 Partner Categories
CREATE TABLE partner_categories (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name        VARCHAR(100) NOT NULL,
    slug        VARCHAR(120) NOT NULL UNIQUE,
    sort_order  INT NOT NULL DEFAULT 0,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.8 Partners
CREATE TABLE partners (
    id                    UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    short_id              BIGSERIAL UNIQUE,            -- Mã số ngắn cho URL SEO
    slug                  VARCHAR(200) NOT NULL UNIQUE,
    name                  VARCHAR(255) NOT NULL,
    description           TEXT,
    detailed_description  TEXT,                        -- HTML chi tiết
    partner_category_id   UUID REFERENCES partner_categories(id) ON DELETE SET NULL,
    location              VARCHAR(255),
    since                 VARCHAR(10),                  -- "2022"
    image                 VARCHAR(500),
    contact_email         VARCHAR(255),
    contact_phone         VARCHAR(30),
    website               VARCHAR(500),
    is_active             BOOLEAN NOT NULL DEFAULT true,
    sort_order            INT NOT NULL DEFAULT 0,
    created_at            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at            TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.9 Banners (Trang chủ)
CREATE TABLE banners (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title       VARCHAR(255) NOT NULL,
    subtitle    VARCHAR(255),
    description TEXT,
    image       VARCHAR(500) NOT NULL,
    color       VARCHAR(100),                          -- Background color
    link_url    VARCHAR(500),
    sort_order  INT NOT NULL DEFAULT 0,
    is_active   BOOLEAN NOT NULL DEFAULT true,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.10 Chat Sessions (Phiên chat khách vãng lai)
CREATE TABLE chat_sessions (
    id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    guest_id      VARCHAR(50) NOT NULL UNIQUE,         -- Client-generated ID
    unread_count  INT NOT NULL DEFAULT 0,
    last_message  TEXT,
    last_seen_at  TIMESTAMPTZ,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.11 Chat Messages
CREATE TABLE chat_messages (
    id             UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    session_id     UUID NOT NULL REFERENCES chat_sessions(id) ON DELETE CASCADE,
    sender_id      VARCHAR(50) NOT NULL,               -- guestId hoặc "admin"
    is_from_admin  BOOLEAN NOT NULL DEFAULT false,
    text           TEXT NOT NULL,
    is_read        BOOLEAN NOT NULL DEFAULT false,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.12 Page Views (Analytics tracking)
CREATE TABLE page_views (
    id           BIGSERIAL PRIMARY KEY,
    path         VARCHAR(500) NOT NULL,
    entity_id    UUID,                                 -- ProductId hoặc PostId
    entity_type  VARCHAR(20),                          -- 'product' | 'post'
    ip_address   VARCHAR(45),
    user_agent   VARCHAR(500),
    viewed_at    DATE NOT NULL DEFAULT CURRENT_DATE
);

-- ============================================================
-- 4. INDEXES
-- ============================================================

-- Users
CREATE INDEX idx_users_email      ON users(email);
CREATE INDEX idx_users_role       ON users(role);

-- Categories
CREATE INDEX idx_categories_slug  ON categories(slug);

-- Products
CREATE INDEX idx_products_slug        ON products(slug);
CREATE INDEX idx_products_short_id    ON products(short_id);
CREATE INDEX idx_products_status      ON products(status);
CREATE INDEX idx_products_category    ON products(category_id);
CREATE INDEX idx_products_featured    ON products(is_featured) WHERE is_featured = true;
CREATE INDEX idx_products_name_trgm   ON products USING GIN (name gin_trgm_ops);
CREATE INDEX idx_products_content     ON products USING GIN (content_blocks);

-- Posts
CREATE INDEX idx_posts_slug           ON posts(slug);
CREATE INDEX idx_posts_short_id       ON posts(short_id);
CREATE INDEX idx_posts_published      ON posts(is_published, published_at DESC);
CREATE INDEX idx_posts_category       ON posts(knowledge_category_id);
CREATE INDEX idx_posts_title_trgm     ON posts USING GIN (title gin_trgm_ops);

-- Partners
CREATE INDEX idx_partners_slug        ON partners(slug);
CREATE INDEX idx_partners_short_id    ON partners(short_id);
CREATE INDEX idx_partners_category    ON partners(partner_category_id);

-- Activity Logs
CREATE INDEX idx_activity_logs_user      ON activity_logs(user_id, created_at DESC);
CREATE INDEX idx_activity_logs_created   ON activity_logs(created_at DESC);
CREATE INDEX idx_activity_logs_entity    ON activity_logs(entity_type, entity_id);
CREATE INDEX idx_partners_active      ON partners(is_active) WHERE is_active = true;

-- Banners
CREATE INDEX idx_banners_active       ON banners(is_active, sort_order);

-- Chat
CREATE INDEX idx_chat_sessions_guest  ON chat_sessions(guest_id);
CREATE INDEX idx_chat_messages_session ON chat_messages(session_id, created_at DESC);

-- Page Views
CREATE INDEX idx_pageviews_date       ON page_views(viewed_at DESC);
CREATE INDEX idx_pageviews_entity     ON page_views(entity_id) WHERE entity_id IS NOT NULL;

-- ============================================================
-- 5. TRIGGER — Auto update updated_at
-- ============================================================

CREATE OR REPLACE FUNCTION fn_update_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_users_updated    BEFORE UPDATE ON users         FOR EACH ROW EXECUTE FUNCTION fn_update_timestamp();
CREATE TRIGGER trg_products_updated BEFORE UPDATE ON products      FOR EACH ROW EXECUTE FUNCTION fn_update_timestamp();
CREATE TRIGGER trg_posts_updated    BEFORE UPDATE ON posts         FOR EACH ROW EXECUTE FUNCTION fn_update_timestamp();
CREATE TRIGGER trg_partners_updated BEFORE UPDATE ON partners      FOR EACH ROW EXECUTE FUNCTION fn_update_timestamp();
CREATE TRIGGER trg_settings_updated BEFORE UPDATE ON site_settings FOR EACH ROW EXECUTE FUNCTION fn_update_timestamp();
CREATE TRIGGER trg_chat_sessions_updated BEFORE UPDATE ON chat_sessions FOR EACH ROW EXECUTE FUNCTION fn_update_timestamp();

-- ============================================================
-- 6. SEED DATA
-- ============================================================

-- Admin mặc định (password: Admin@123 — hash bằng Argon2id, sẽ được app seed lại)
INSERT INTO users (name, email, password_hash, role) VALUES
('System Admin', 'admin@cataloga.com', 'PLACEHOLDER_WILL_BE_SEEDED_BY_APP', 'SUPER_ADMIN');

-- Settings mặc định
INSERT INTO site_settings (company_name, phone, email, primary_color, background_color, footer_text, show_footer) VALUES
('CatalogaWeb', '0353785710', 'contact@cataloga.com', '#55B3D9', '#F9F9F9', 'Precision Engineering.', true);

-- Danh mục mẫu
INSERT INTO categories (name, slug, image, sort_order) VALUES
('Máy cho ăn',   'may-cho-an',   '/uploads/categories/may-cho-an.jpg',   0),
('Máy sục khí',  'may-suc-khi',  '/uploads/categories/may-suc-khi.jpg',  1),
('Khung ao',     'khung-ao',     '/uploads/categories/khung-ao.jpg',     2),
('Phụ kiện',     'phu-kien',     '/uploads/categories/phu-kien.jpg',     3);

-- Danh mục kiến thức mẫu
INSERT INTO knowledge_categories (name, slug, sort_order) VALUES
('Tin tức',       'tin-tuc',    0),
('Hướng dẫn',    'huong-dan',  1),
('Kỹ thuật',     'ky-thuat',   2);

-- Danh mục đối tác mẫu
INSERT INTO partner_categories (name, slug, sort_order) VALUES
('Doanh nghiệp', 'doanh-nghiep', 0),
('Hộ nuôi',      'ho-nuoi',      1);
