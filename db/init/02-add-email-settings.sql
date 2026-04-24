-- Migration: thêm cột SMTP/Email vào site_settings
-- Áp dụng nếu DB đã tồn tại trước phase Email/Notifications.
-- Idempotent: dùng IF NOT EXISTS để có thể chạy nhiều lần.

ALTER TABLE site_settings
    ADD COLUMN IF NOT EXISTS email_enabled       BOOLEAN     NOT NULL DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS smtp_host           VARCHAR(255),
    ADD COLUMN IF NOT EXISTS smtp_port           INTEGER     NOT NULL DEFAULT 587,
    ADD COLUMN IF NOT EXISTS smtp_use_ssl        BOOLEAN     NOT NULL DEFAULT TRUE,
    ADD COLUMN IF NOT EXISTS smtp_user           VARCHAR(255),
    ADD COLUMN IF NOT EXISTS smtp_password       VARCHAR(255),
    ADD COLUMN IF NOT EXISTS smtp_from_email     VARCHAR(255),
    ADD COLUMN IF NOT EXISTS smtp_from_name      VARCHAR(255),
    ADD COLUMN IF NOT EXISTS notification_email  VARCHAR(255);
