-- ============================================================
-- Migration 03: Add severity column + index to activity_logs
-- ============================================================
-- Mức độ hoạt động: Info (mặc định), Success, Warning, Error
-- Dùng cho audit log viewer trong admin với màu sắc + filter.

ALTER TABLE activity_logs
    ADD COLUMN IF NOT EXISTS severity VARCHAR(20) NOT NULL DEFAULT 'Info';

CREATE INDEX IF NOT EXISTS idx_activity_logs_severity
    ON activity_logs(severity, created_at DESC);

CREATE INDEX IF NOT EXISTS idx_activity_logs_action
    ON activity_logs(action, created_at DESC);
