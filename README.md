# AquaCMS

**Hệ thống quản trị nội dung (CMS) & bán hàng** — xây dựng cho doanh nghiệp thiết bị nuôi trồng thủy sản.
Server-side rendered (SSR) cho SEO tối ưu, quản trị đầy đủ, responsive trên mọi thiết bị.

---

## Tính năng chính

### Trang khách hàng (Public)
- **Trang chủ**: Banner slider, sản phẩm nổi bật, bài viết mới, đối tác
- **Sản phẩm**: Danh sách sản phẩm, lọc theo danh mục, tìm kiếm, phân trang
- **Chi tiết sản phẩm**: Content blocks (JSONB), SEO meta tags, JSON-LD, sản phẩm liên quan
- **Kiến thức**: Blog bài viết, phân loại, tìm kiếm
- **Đối tác**: Danh sách & chi tiết đối tác
- **Giỏ hàng**: Cart localStorage, đặt hàng qua Zalo
- **Tìm kiếm**: Debounced search, suggestions AJAX, full-text PostgreSQL (pg_trgm + unaccent)
- **Chat trực tuyến**: SignalR realtime giữa khách – admin
- **SEO**: Vietnamese slug URLs, meta tags, Open Graph, canonical, JSON-LD structured data
- **Responsive**: Mobile-first, Tailwind CSS
- **Floating contacts**: Zalo, hotline, scroll-to-top
- **Đa ngôn ngữ (VI / EN)**: Chuyển ngữ navbar / footer / labels qua cookie `AquaCMS.Lang` (LocalizationService)

### Trang quản trị (Admin)
- **Dashboard**: Thống kê tổng quan, biểu đồ lượt xem 7 ngày (Chart.js), top sản phẩm, bài viết gần đây
  - **Analytics nâng cao**: Traffic source (Google / Facebook / Zalo / Direct…), Device breakdown (doughnut), Conversion rate (unique → cart/checkout), Top pages (horizontal bar)
- **Sản phẩm**: CRUD đầy đủ, search/filter, bulk actions (ẩn/hiện/xóa), phân trang
  - **Import / Export Excel (.xlsx)** — cùng 1 format cho xuất/nhập, sắp xếp khoa học, có sheet hướng dẫn
  - **Crop ảnh trước khi upload** (Cropper.js modal — 1:1 cho sản phẩm/đối tác, 16:9 cho banner/bài viết)
- **Danh mục SP**: CRUD danh mục sản phẩm
- **Kiến thức**: CRUD bài viết với **Quill** rich text editor, xuất bản/nháp
- **Đối tác**: CRUD đối tác, rich text mô tả
- **Banner**: CRUD banner trang chủ, preview grid
- **Tin nhắn (Chat)**: Trả lời khách realtime qua SignalR
- **Cài đặt**: Thông tin công ty, mạng xã hội, thanh toán, giao diện, **email SMTP (MailKit)**
- **Người dùng**: Quản lý tài khoản admin, phân quyền 4 roles
- **Nhật ký hoạt động (Audit Log)**: Bảng audit có **màu sắc theo mức độ** (Info / Success / Warning / Error), filter theo entity, action, severity, search
- **Dark mode**: Toggle trong sidebar admin (lưu `localStorage`, không flash khi tải)
- **Responsive**: Admin sidebar responsive, mobile overlay

---

## Công nghệ

| Layer | Công nghệ |
|-------|-----------|
| **Runtime** | .NET 9.0 (ASP.NET Core MVC) |
| **Database** | PostgreSQL 16 + EF Core 9.0 (Npgsql) |
| **Auth** | Cookie-based, Argon2id password hashing |
| **CSS** | Tailwind CSS (CLI build → wwwroot/css/site.css) |
| **Icons** | Lucide Icons (CDN) |
| **Rich Editor** | Quill 2 (CDN) |
| **Image Crop** | Cropper.js |
| **Charts** | Chart.js 4 (CDN) |
| **Realtime** | ASP.NET Core SignalR (chat) |
| **Email** | MailKit (SMTP) |
| **Excel I/O** | ClosedXML (.xlsx import/export) |
| **AJAX** | HTMX 2.0 |
| **Reactivity** | Alpine.js 3 |
| **Logging** | Serilog (console + file rolling daily) |
| **Security** | CSP headers, rate limiting, CSRF, XSS protection (HtmlSanitizer) |
| **Search** | PostgreSQL pg_trgm + unaccent extensions |
| **IDs** | UUID (uuid_generate_v4) |
| **Content** | JSONB content blocks cho sản phẩm |

---

## Kiến trúc

```
Controller → Service → DbContext → PostgreSQL
    ↓
Razor View (SSR) → HTML + Tailwind CSS
```

- **MVC Pattern**: Controllers chỉ điều hướng, logic nằm trong Services
- **Service Layer**: Interface + Implementation, DI qua constructor
- **Vietnamese SEO Routes**: `/san-pham`, `/kien-thuc`, `/doi-tac`, `/gio-hang`

---

## Phân quyền

| Role | Dashboard | Sản phẩm | Bài viết | Đối tác | Banner | Cài đặt | Người dùng | Audit log |
|------|-----------|----------|----------|---------|--------|---------|------------|-----------|
| **SUPER_ADMIN** | ✅ | ✅ Full | ✅ Full | ✅ Full | ✅ Full | ✅ | ✅ Full | ✅ |
| **MANAGER** | ✅ | ✅ Full | ✅ Full | ✅ Full | ✅ Full | ❌ | ✅ Xem | ✅ |
| **EDITOR** | ✅ | ✅ CRUD | ✅ CRUD | ✅ CRUD | ✅ CRUD | ❌ | ❌ | ❌ |
| **SALE** | ✅ | 👁 Xem | 👁 Xem | 👁 Xem | 👁 Xem | ❌ | ❌ | ❌ |

---

## Bảo mật

- **Authentication**: Cookie-based (HttpOnly, SameSite=Lax, Secure)
- **Password**: Argon2id hashing (chống GPU attack)
- **CSRF**: `[ValidateAntiForgeryToken]` trên tất cả POST
- **Rate Limiting**: Login (5/min/IP), API (60/min/IP), Global (200/min/IP)
- **Security Headers**: CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy
- **XSS**: HtmlSanitizer cho user-generated content
- **Audit Trail**: Mọi thao tác CRUD admin được log với severity (Info/Success/Warning/Error)
- **Error Handling**: Global exception middleware, không leak stack trace
- **Logging**: Serilog structured logging với request context

---

## Cài đặt nhanh

```bash
# 1. Clone
git clone <repo-url> && cd CatalogaWeb

# 2. Setup database
psql -U postgres -c "CREATE DATABASE aquacms_db;"
psql -U postgres -d aquacms_db -f db/init/01-schema.sql
psql -U postgres -d aquacms_db -f db/init/02-add-email-settings.sql
psql -U postgres -d aquacms_db -f db/init/03-add-activity-severity.sql

# 3. Cấu hình connection string
# Sửa src/AquaCMS/appsettings.json

# 4. Build & Run
cd src/AquaCMS
dotnet restore && dotnet run
```

Xem chi tiết tại [GETTING-STARTED.md](GETTING-STARTED.md).

---

## Import / Export sản phẩm bằng Excel

1. Vào **Admin → Sản phẩm**.
2. Bấm **Xuất Excel** để tải về toàn bộ danh sách (đã zebra, tô màu theo trạng thái, freeze header, autofilter, kèm sheet "Hướng dẫn").
3. Bấm **Tải template Excel** nếu chưa có dữ liệu.
4. Chỉnh sửa trực tiếp trong Excel (giá, mô tả, tên, SEO, trạng thái, nổi bật, ...).
5. Bấm **Nhập Excel** + chọn file `.xlsx` để cập nhật hàng loạt.

**Quy tắc import:**
- Match theo cột **Slug** → đã có thì update, chưa có thì tạo mới.
- Slug bỏ trống → tự sinh từ Tên (loại dấu tiếng Việt).
- **Ảnh** KHÔNG nằm trong Excel — admin upload trực tiếp trên web.
- Trạng thái: `Available` / `Hidden` / `OutOfStock`.
- Nổi bật: `Có` / `Không` / `true` / `false` / `1` / `0`.

## Audit Log (Nhật ký hoạt động)

- Vào **Admin → Nhật ký hoạt động** (Manager+).
- Mỗi dòng có màu theo **Severity**:
  - 🔵 **Info** — thao tác xem/tham khảo
  - 🟢 **Success** — CRUD thành công
  - 🟠 **Warning** — DELETE, IMPORT lỗi 1 phần
  - 🔴 **Error** — exception, thất bại
- Filter linh hoạt theo **Entity** (Product/Post/...), **Action** (CREATE/UPDATE/...), **Severity**, **Search** (user / mô tả / ID).
- Click vào card thống kê đầu trang để filter nhanh.

---

## License

Private — Internal use only.
