-- ============================================================
-- Migration 04: Add referrer column + page_views indexes
-- ============================================================
-- Cho phép tracking traffic source trong dashboard analytics nâng cao.

ALTER TABLE page_views
    ADD COLUMN IF NOT EXISTS referrer VARCHAR(500);

CREATE INDEX IF NOT EXISTS idx_page_views_path
    ON page_views(path, viewed_at DESC);

CREATE INDEX IF NOT EXISTS idx_page_views_viewed_at
    ON page_views(viewed_at DESC);

CREATE INDEX IF NOT EXISTS idx_page_views_ip
    ON page_views(ip_address, viewed_at);
