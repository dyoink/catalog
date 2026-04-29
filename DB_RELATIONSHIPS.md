# Tài liệu Quan hệ và Cấu trúc Cơ sở Dữ liệu (Database Schema & Relationships)

Dự án sử dụng PostgreSQL 16. Dưới đây là chi tiết các thực thể, thuộc tính và mối quan hệ trong hệ thống.

## 1. Sơ đồ Quan hệ Chính

### Quản lý Sản phẩm (Catalog)
*   **Categories (1) <--- (n) Products**: Mỗi sản phẩm thuộc về một danh mục.
    *   Khóa ngoại: `products.category_id` -> `categories.id` (ON DELETE SET NULL)

### Quản lý Kiến thức / Tin tức (CMS)
*   **Knowledge Categories (1) <--- (n) Posts**: Mỗi bài viết thuộc về một danh mục kiến thức.
    *   Khóa ngoại: `posts.knowledge_category_id` -> `knowledge_categories.id` (ON DELETE SET NULL)

### Quản lý Đối tác (Partners)
*   **Partner Categories (1) <--- (n) Partners**: Mỗi đối tác thuộc về một danh mục đối tác.
    *   Khóa ngoại: `partners.partner_category_id` -> `partner_categories.id` (ON DELETE SET NULL)

### Hệ thống Chat (Messaging)
*   **Chat Sessions (1) <--- (n) Chat Messages**: Một phiên chat chứa nhiều tin nhắn.
    *   Khóa ngoại: `chat_messages.session_id` -> `chat_sessions.id` (ON DELETE CASCADE)

### Nhật ký Hoạt động (Audit Logs)
*   **Users (1) <--- (n) Activity Logs**: Lưu vết thao tác của admin.
    *   Khóa ngoại: `activity_logs.user_id` -> `users.id` (ON DELETE SET NULL)

---

## 2. Chi tiết các Bảng và Thuộc tính

### 2.1. Hệ thống & Admin
#### Bảng `users` (Quản trị viên)
- `id`: UUID (PK)
- `name`: VARCHAR(100) - Tên hiển thị
- `email`: VARCHAR(255) (Unique) - Email đăng nhập
- `password_hash`: VARCHAR(255) - Hash mật khẩu (Argon2id)
- `role`: user_role (Enum) - Quyền: SUPER_ADMIN, MANAGER, EDITOR, SALE
- `avatar`: VARCHAR(500) - Đường dẫn ảnh đại diện
- `is_active`: BOOLEAN - Trạng thái hoạt động
- `last_login_at`: TIMESTAMPTZ - Lần đăng nhập cuối
- `created_at`, `updated_at`: TIMESTAMPTZ

#### Bảng `site_settings` (Cấu hình hệ thống - Singleton)
- Lưu trữ toàn bộ thông tin công ty, cấu hình SEO, màu sắc UI, thông tin ngân hàng, cấu hình SMTP/Email, và các nút bật/tắt module trên trang chủ/navbar.

#### Bảng `activity_logs` (Nhật ký thao tác)
- `id`: UUID (PK)
- `user_id`: UUID (FK)
- `user_name`: VARCHAR(100)
- `action`: VARCHAR(50) - Hành động (Create, Update, Delete...)
- `entity_type`: VARCHAR(50) - Loại thực thể bị tác động
- `entity_id`: VARCHAR(100)
- `description`: TEXT - Chi tiết thao tác
- `severity`: VARCHAR(20) - Mức độ: Info, Success, Warning, Error
- `ip_address`, `user_agent`: Thông tin kết nối
- `created_at`: TIMESTAMPTZ

### 2.2. Module Catalog (Sản phẩm)
#### Bảng `categories` (Danh mục sản phẩm)
- `id`: UUID (PK)
- `name`: VARCHAR(100)
- `slug`: VARCHAR(120) (Unique)
- `image`: VARCHAR(500)
- `sort_order`: INT
- `created_at`: TIMESTAMPTZ

#### Bảng `products` (Sản phẩm)
- `id`: UUID (PK)
- `short_id`: BIGSERIAL (Unique) - Dùng cho SEO URL
- `slug`: VARCHAR(200) (Unique)
- `name`: VARCHAR(255)
- `sku`: VARCHAR(100)
- `category_id`: UUID (FK)
- `price`: DECIMAL(18,0) - Giá (NULL = Liên hệ)
- `show_price`: BOOLEAN - Bật/tắt hiển thị giá
- `description`: TEXT - Mô tả ngắn
- `image`: VARCHAR(500) - Ảnh chính
- `video_url`: VARCHAR(500)
- `status`: product_status (Enum) - available, out_of_stock, hidden
- `content_blocks`: JSONB - Chứa nội dung chi tiết dạng block
- `is_featured`: BOOLEAN - Sản phẩm nổi bật
- `view_count`: INT
- `meta_title`, `meta_desc`: SEO
- `created_at`, `updated_at`: TIMESTAMPTZ

### 2.3. Module CMS (Kiến thức)
#### Bảng `knowledge_categories` (Danh mục bài viết)
- `id`, `name`, `slug`, `sort_order`, `created_at`

#### Bảng `posts` (Bài viết)
- `id`: UUID (PK)
- `short_id`: BIGSERIAL
- `slug`: VARCHAR(200) (Unique)
- `title`: VARCHAR(255)
- `excerpt`: TEXT - Tóm tắt
- `content`: TEXT (HTML) - Nội dung chi tiết
- `image`: VARCHAR(500)
- `author`: VARCHAR(100)
- `knowledge_category_id`: UUID (FK)
- `read_time`: VARCHAR(20)
- `is_published`: BOOLEAN
- `published_at`: TIMESTAMPTZ
- `view_count`: INT
- `meta_title`, `meta_desc`: SEO
- `created_at`, `updated_at`: TIMESTAMPTZ

### 2.4. Module Partners (Đối tác)
#### Bảng `partner_categories`, `partners`
- Tương tự cấu trúc bài viết nhưng bổ sung các trường: `location`, `since` (năm thành lập), `contact_email`, `contact_phone`, `website`.

### 2.5. Module Messaging & Analytics
#### Bảng `chat_sessions` & `chat_messages`
- Quản lý phiên chat của khách (guest_id) và nội dung tin nhắn giữa khách và admin.

#### Bảng `page_views` (Thống kê truy cập)
- `id`: BIGSERIAL (PK)
- `path`: VARCHAR(500)
- `entity_id`: UUID - ID của Sản phẩm/Bài viết
- `entity_type`: VARCHAR(20) - 'product' hoặc 'post'
- `referrer`: VARCHAR(500) - Nguồn dẫn đến
- `ip_address`, `user_agent`: Thông tin khách truy cập
- `viewed_at`: DATE

---

## 3. Kiểu Dữ liệu Đặc biệt (Enums)
- **user_role**: `SUPER_ADMIN`, `MANAGER`, `EDITOR`, `SALE`.
- **product_status**: `available`, `out_of_stock`, `hidden`.
