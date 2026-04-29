-- ============================================================
-- CatalogaWeb — Database Schema (PostgreSQL 16)
-- Đã cập nhật theo kiến trúc Product Normalization
-- Đã hợp nhất tất cả các migration (02, 03, 04, 05) vào bản gốc
-- ============================================================

-- 1. Extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
CREATE EXTENSION IF NOT EXISTS "unaccent";

-- 2. ENUM types
CREATE TYPE user_role      AS ENUM ('SUPER_ADMIN', 'MANAGER', 'EDITOR', 'SALE');
CREATE TYPE product_status AS ENUM ('available', 'out_of_stock', 'hidden');

-- ============================================================
-- 3. TABLES
-- ============================================================

-- 3.1 Users (Quản trị viên)
CREATE TABLE users (
    id             UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name           VARCHAR(100) NOT NULL,
    email          VARCHAR(255) NOT NULL UNIQUE,
    password_hash  VARCHAR(255) NOT NULL,
    role           user_role NOT NULL DEFAULT 'EDITOR',
    avatar         VARCHAR(500),
    is_active      BOOLEAN NOT NULL DEFAULT true,
    last_login_at  TIMESTAMPTZ,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.2 Site Settings (singleton - Bao gồm SMTP từ migration 02)
CREATE TABLE site_settings (
    id               UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_name     VARCHAR(255) NOT NULL DEFAULT 'AquaCMS',
    logo             VARCHAR(500),
    address          TEXT,
    phone            VARCHAR(30),
    email            VARCHAR(255),
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
    bank_name        VARCHAR(255),
    bank_number      VARCHAR(50),
    bank_owner       VARCHAR(255),
    background_color VARCHAR(20) DEFAULT '#F9F9F9',
    primary_color    VARCHAR(20) DEFAULT '#55B3D9',
    navbar_color     VARCHAR(20) DEFAULT '#2563eb',
    footer_color     VARCHAR(20) DEFAULT '#1F2937',
    footer_text      VARCHAR(500),
    show_footer      BOOLEAN NOT NULL DEFAULT true,
    hero_background_image VARCHAR(500),
    show_banners           BOOLEAN NOT NULL DEFAULT true,
    show_categories        BOOLEAN NOT NULL DEFAULT true,
    show_featured_products BOOLEAN NOT NULL DEFAULT true,
    show_latest_posts      BOOLEAN NOT NULL DEFAULT true,
    show_partners          BOOLEAN NOT NULL DEFAULT true,
    featured_products_count INT NOT NULL DEFAULT 8,
    latest_posts_count      INT NOT NULL DEFAULT 6,
    show_nav_products   BOOLEAN NOT NULL DEFAULT true,
    show_nav_knowledge  BOOLEAN NOT NULL DEFAULT true,
    show_nav_partners   BOOLEAN NOT NULL DEFAULT true,
    show_nav_cart       BOOLEAN NOT NULL DEFAULT true,
    hero_title              VARCHAR(255),
    hero_subtitle           VARCHAR(255),
    hero_description        TEXT,
    hero_button_text        VARCHAR(100),
    hero_button_url         VARCHAR(500),
    about_title             VARCHAR(255),
    about_content           TEXT,
    about_image             VARCHAR(500),
    default_meta_title      VARCHAR(70),
    default_meta_description VARCHAR(160),
    default_og_image        VARCHAR(500),
    google_analytics_id     VARCHAR(50),
    facebook_pixel_id       VARCHAR(50),
    footer_about_text       TEXT,
    copyright_text          VARCHAR(255),
    -- Email / SMTP (Migration 02)
    email_enabled           BOOLEAN NOT NULL DEFAULT FALSE,
    smtp_host               VARCHAR(255),
    smtp_port               INTEGER NOT NULL DEFAULT 587,
    smtp_use_ssl            BOOLEAN NOT NULL DEFAULT TRUE,
    smtp_user               VARCHAR(255),
    smtp_password           VARCHAR(255),
    smtp_from_email         VARCHAR(255),
    smtp_from_name          VARCHAR(255),
    notification_email      VARCHAR(255),
    chat_auto_reply_message TEXT,
    updated_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.3 Categories
CREATE TABLE categories (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name        VARCHAR(100) NOT NULL,
    slug        VARCHAR(120) NOT NULL UNIQUE,
    image       VARCHAR(500),
    sort_order  INT NOT NULL DEFAULT 0,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.4 Products (CORE)
CREATE TABLE products (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    short_id        BIGSERIAL UNIQUE,                
    sku             VARCHAR(100) UNIQUE,
    name            VARCHAR(255) NOT NULL,
    category_id     UUID REFERENCES categories(id) ON DELETE SET NULL,
    status          VARCHAR(20) NOT NULL DEFAULT 'hidden',
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.4a Product Metadata (SEO & Slugs)
CREATE TABLE product_metadata (
    product_id      UUID PRIMARY KEY REFERENCES products(id) ON DELETE CASCADE,
    slug            VARCHAR(200) NOT NULL UNIQUE,
    meta_title      VARCHAR(70),
    meta_desc       VARCHAR(160)
);

-- 3.4b Product Contents (Heavy Data)
CREATE TABLE product_contents (
    product_id      UUID PRIMARY KEY REFERENCES products(id) ON DELETE CASCADE,
    description     TEXT,
    content_blocks  JSONB NOT NULL DEFAULT '[]',
    image           VARCHAR(500),
    video_url       VARCHAR(500)
);

-- 3.4c Product Finances (Pricing - Bao gồm show_price từ migration 05)
CREATE TABLE product_finances (
    product_id      UUID PRIMARY KEY REFERENCES products(id) ON DELETE CASCADE,
    price           DECIMAL(18,0),
    show_price      BOOLEAN NOT NULL DEFAULT TRUE,
    is_featured     BOOLEAN NOT NULL DEFAULT FALSE
);

-- 3.4d Product Statistics (Analytics)
CREATE TABLE product_statistics (
    product_id      UUID PRIMARY KEY REFERENCES products(id) ON DELETE CASCADE,
    view_count      INT NOT NULL DEFAULT 0
);

-- 3.5 Knowledge Categories
CREATE TABLE knowledge_categories (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name        VARCHAR(100) NOT NULL,
    slug        VARCHAR(120) NOT NULL UNIQUE,
    sort_order  INT NOT NULL DEFAULT 0,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.6 Posts
CREATE TABLE posts (
    id                     UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    short_id               BIGSERIAL UNIQUE,
    slug                   VARCHAR(200) NOT NULL UNIQUE,
    title                  VARCHAR(255) NOT NULL,
    excerpt                TEXT,
    content                TEXT NOT NULL,
    image                  VARCHAR(500),
    author                 VARCHAR(100) NOT NULL DEFAULT 'Admin',
    knowledge_category_id  UUID REFERENCES knowledge_categories(id) ON DELETE SET NULL,
    read_time              VARCHAR(20),
    is_published           BOOLEAN NOT NULL DEFAULT false,
    published_at           TIMESTAMPTZ,
    view_count             INT NOT NULL DEFAULT 0,
    meta_title             VARCHAR(70),
    meta_desc              VARCHAR(160),
    created_at             TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at             TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.7 Partner Categories & Partners
CREATE TABLE partner_categories (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name        VARCHAR(100) NOT NULL,
    slug        VARCHAR(120) NOT NULL UNIQUE,
    sort_order  INT NOT NULL DEFAULT 0,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE partners (
    id                    UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    short_id              BIGSERIAL UNIQUE,
    slug                  VARCHAR(200) NOT NULL UNIQUE,
    name                  VARCHAR(255) NOT NULL,
    description           TEXT,
    detailed_description  TEXT,
    partner_category_id   UUID REFERENCES partner_categories(id) ON DELETE SET NULL,
    location              VARCHAR(255),
    since                 VARCHAR(10),
    image                 VARCHAR(500),
    contact_email         VARCHAR(255),
    contact_phone         VARCHAR(30),
    website               VARCHAR(500),
    is_active             BOOLEAN NOT NULL DEFAULT true,
    sort_order            INT NOT NULL DEFAULT 0,
    created_at            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at            TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.9 Banners
CREATE TABLE banners (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title       VARCHAR(255) NOT NULL,
    subtitle    VARCHAR(255),
    description TEXT,
    image       VARCHAR(500) NOT NULL,
    color       VARCHAR(100),
    link_url    VARCHAR(500),
    sort_order  INT NOT NULL DEFAULT 0,
    is_active   BOOLEAN NOT NULL DEFAULT true,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.10 Chat Sessions
CREATE TABLE chat_sessions (
    id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    guest_id      VARCHAR(50) NOT NULL UNIQUE,
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
    sender_id      VARCHAR(50) NOT NULL,
    is_from_admin  BOOLEAN NOT NULL DEFAULT false,
    text           TEXT NOT NULL,
    is_read        BOOLEAN NOT NULL DEFAULT false,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3.12 Page Views (Bao gồm referrer từ migration 04)
CREATE TABLE page_views (
    id           BIGSERIAL PRIMARY KEY,
    path         VARCHAR(500) NOT NULL,
    entity_id    UUID,
    entity_type  VARCHAR(20),
    ip_address   VARCHAR(45),
    user_agent   VARCHAR(500),
    referrer     VARCHAR(500),
    viewed_at    DATE NOT NULL DEFAULT CURRENT_DATE
);

-- 3.13 Activity Logs (Bao gồm severity từ migration 03)
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
    severity     VARCHAR(20) NOT NULL DEFAULT 'Info',
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ============================================================
-- 4. INDEXES
-- ============================================================

CREATE INDEX idx_users_email      ON users(email);
CREATE INDEX idx_categories_slug  ON categories(slug);

-- Product Indexes
CREATE INDEX idx_products_sku         ON products(sku);
CREATE INDEX idx_products_category    ON products(category_id);
CREATE INDEX idx_products_status      ON products(status);
CREATE INDEX idx_product_metadata_slug ON product_metadata(slug);
CREATE INDEX idx_product_finances_featured ON product_finances(is_featured) WHERE is_featured = true;
CREATE INDEX idx_products_name_trgm   ON products USING GIN (name gin_trgm_ops);

-- Others
CREATE INDEX idx_posts_slug           ON posts(slug);
CREATE INDEX idx_posts_category       ON posts(knowledge_category_id);
CREATE INDEX idx_partners_slug        ON partners(slug);
CREATE INDEX idx_activity_logs_created   ON activity_logs(created_at DESC);
CREATE INDEX idx_activity_logs_severity  ON activity_logs(severity, created_at DESC);
CREATE INDEX idx_page_views_viewed_at ON page_views(viewed_at DESC);
CREATE INDEX idx_page_views_path      ON page_views(path, viewed_at DESC);

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

-- Admin & Settings
INSERT INTO users (name, email, password_hash, role) VALUES
('System Admin', 'admin@cataloga.com', 'PLACEHOLDER_WILL_BE_SEEDED_BY_APP', 'SUPER_ADMIN');

INSERT INTO site_settings (company_name, phone, email, primary_color, background_color, footer_text, show_footer) VALUES
('CatalogaWeb', '0353785710', 'contact@cataloga.com', '#55B3D9', '#F9F9F9', 'Precision Engineering.', true);

-- Categories
INSERT INTO categories (id, name, slug, image, sort_order) VALUES
('c1000000-0000-0000-0000-000000000001', 'Máy cho ăn',   'may-cho-an',   '/uploads/categories/may-cho-an.jpg',   0),
('c1000000-0000-0000-0000-000000000002', 'Máy sục khí',  'may-suc-khi',  '/uploads/categories/may-suc-khi.jpg',  1),
('c1000000-0000-0000-0000-000000000003', 'Khung ao',     'khung-ao',     '/uploads/categories/khung-ao.jpg',     2),
('c1000000-0000-0000-0000-000000000004', 'Phụ kiện',     'phu-kien',     '/uploads/categories/phu-kien.jpg',     3);

-- Knowledge & Partner Categories
INSERT INTO knowledge_categories (name, slug, sort_order) VALUES
('Tin tức',       'tin-tuc',    0),
('Hướng dẫn',    'huong-dan',  1),
('Kỹ thuật',     'ky-thuat',   2);

INSERT INTO partner_categories (name, slug, sort_order) VALUES
('Doanh nghiệp', 'doanh-nghiep', 0),
('Hộ nuôi',      'ho-nuoi',      1);

