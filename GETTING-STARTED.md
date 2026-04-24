# AquaCMS — Hướng dẫn cài đặt (Getting Started)

## Yêu cầu hệ thống

| Thành phần | Phiên bản tối thiểu |
|-----------|---------------------|
| **.NET SDK** | 9.0 trở lên |
| **PostgreSQL** | 15+ (khuyến nghị 16) |
| **Node.js** | 18+ (build Tailwind CSS qua CLI) |
| **OS** | Windows 10+, macOS, Linux |

## 1. Clone dự án

```bash
git clone <repository-url>
cd CatalogaWeb
```

## 2. Cài đặt PostgreSQL

### Windows
- Tải từ https://www.postgresql.org/download/windows/
- Cài đặt mặc định, ghi nhớ password cho user `postgres`

### macOS
```bash
brew install postgresql@16
brew services start postgresql@16
```

### Linux (Ubuntu/Debian)
```bash
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
```

## 3. Tạo Database

```bash
# Kết nối psql
psql -U postgres

# Trong psql:
CREATE DATABASE aquacms_db;
\c aquacms_db

# Chạy schema + các migration tăng dần
\i db/init/01-schema.sql
\i db/init/02-add-email-settings.sql
\i db/init/03-add-activity-severity.sql
\i db/init/04-add-pageview-referrer.sql
```

Schema sẽ tạo:
- 12+ bảng: `users`, `categories`, `products`, `posts`, `knowledge_categories`, `partners`, `partner_categories`, `banners`, `site_settings`, `chat_sessions`, `chat_messages`, `page_views`, `activity_logs`
- 2 PostgreSQL enums: `user_role`, `product_status`
- Extensions: `uuid-ossp`, `pg_trgm`, `unaccent`
- Indexes cho full-text search + audit log

> Migration `03` thêm cột `severity` (Info/Success/Warning/Error) cho `activity_logs` — bắt buộc cho trang Nhật ký hoạt động phiên bản mới.
> Migration `04` thêm cột `referrer` cho `page_views` + indexes — bắt buộc cho Analytics nâng cao (traffic source, top pages).

## 4. Cấu hình Connection String

Mở `src/AquaCMS/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=aquacms_db;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

> **Production**: dùng environment variable hoặc user-secrets:
> ```bash
> cd src/AquaCMS
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Password=..."
> ```

## 5. Restore NuGet packages

```bash
cd src/AquaCMS
dotnet restore
```

Các package chính sẽ được kéo về:
- `Microsoft.EntityFrameworkCore` 9 + `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.AspNetCore.SignalR` (chat realtime)
- `MailKit` (gửi email SMTP)
- `ClosedXML` (import/export Excel `.xlsx`)
- `HtmlSanitizer` (chống XSS)
- `Isopoh.Cryptography.Argon2` (hash password)
- `Serilog.AspNetCore` (logging)

## 6. Build CSS (Tailwind CLI)

Lần đầu hoặc khi thay đổi class Tailwind:

```bash
cd src/AquaCMS
npm install
npm run build:css       # build 1 lần
# hoặc
npm run watch:css       # auto rebuild khi save
```

Output: `wwwroot/css/site.css`.

## 7. Build dự án

```bash
dotnet build
```

Nếu build thành công:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## 8. Chạy ứng dụng

```bash
dotnet run
# hoặc dev hot-reload:
dotnet watch run
```

Truy cập:
- **Trang chủ**: http://localhost:5000
- **Admin**: http://localhost:5000/admin
- **Đăng nhập**: http://localhost:5000/dang-nhap

## 9. Đăng nhập Admin

Khi khởi động lần đầu, hệ thống tự seed tài khoản Super Admin:

| Field | Giá trị |
|-------|---------|
| Email | `admin@aquacms.com` |
| Password | `Admin@123` |
| Role | `SUPER_ADMIN` |

> **Quan trọng**: Đổi mật khẩu ngay sau khi đăng nhập lần đầu (Admin → Hồ sơ).

## 10. Cấu hình Email SMTP (tuỳ chọn)

Vào **Admin → Cài đặt → Email** để cấu hình SMTP host/port/user/password.
Hỗ trợ Gmail App Password, SendGrid, Mailtrap...

## 11. Cấu trúc thư mục

```
CatalogaWeb/
├── AquaCMS.sln
├── README.md
├── GETTING-STARTED.md
├── db/init/
│   ├── 01-schema.sql                 ← schema gốc
│   ├── 02-add-email-settings.sql     ← migration: email SMTP
│   ├── 03-add-activity-severity.sql  ← migration: audit severity
│   └── 04-add-pageview-referrer.sql  ← migration: pageview referrer + indexes
└── src/AquaCMS/
    ├── Program.cs
    ├── appsettings.json
    ├── package.json                  ← Tailwind CLI scripts
    ├── tailwind.config.js
    ├── Areas/Admin/
    │   ├── Controllers/
    │   │   ├── ProductsController.cs
    │   │   ├── ProductImportExportController.cs   ← Excel I/O
    │   │   ├── ActivityController.cs              ← Audit viewer
    │   │   ├── ChatController.cs
    │   │   └── ...
    │   └── Views/
    ├── Controllers/                  ← Public controllers
    ├── Hubs/                         ← SignalR ChatHub
    ├── Data/                         ← DbContext + Seed
    ├── Helpers/                      ← Slug, Price formatting
    ├── Models/                       ← Entities, ViewModels
    ├── Services/                     ← Business logic
    ├── Views/                        ← Razor Views (SSR)
    └── wwwroot/                      ← Static files
```

## 12. Workflow Import / Export sản phẩm bằng Excel

1. Vào **Admin → Sản phẩm**.
2. **Xuất Excel**: nút màu xanh — tải file `.xlsx` đã được format đẹp (zebra rows, freeze header, autofilter, color-coded status), kèm sheet **Hướng dẫn**.
3. **Tải template Excel**: nếu chưa có dữ liệu, tải template trống có 1 row demo.
4. Chỉnh sửa file Excel (đổi giá, tên, mô tả, slug, SEO, trạng thái, nổi bật...).
5. **Nhập Excel**: chọn file `.xlsx` đã sửa rồi bấm Nhập.
6. Hệ thống match theo **Slug**: đã tồn tại → update, chưa có → tạo mới.
7. **Ảnh không có trong Excel** — upload riêng trên web.

## 13. Audit Log (Nhật ký hoạt động)

- **Admin → Nhật ký hoạt động** (chỉ Manager trở lên).
- 5 stat card đầu trang: Tổng / Info / Success / Warning / Error — click để filter nhanh.
- Filter form: Search (user, mô tả, ID), Entity, Action, Severity.
- Mỗi row có **màu nền + viền trái** theo severity, có badge icon (info / check / warning / x-circle).
- Phân trang giới hạn ±3 quanh trang hiện tại.

## 14. Image Crop modal

- Khi upload ảnh ở các trang Tạo mới của **Sản phẩm**, **Banner**, **Bài viết (Kiến thức)**, **Đối tác**, một modal Cropper.js sẽ tự bật ra.
  - Sản phẩm / Đối tác → tỉ lệ **1:1**.
  - Banner / Kiến thức → tỉ lệ **16:9**.
- Toolbar: xoay ±90°, lật ngang/dọc, reset.
- Bấm **Áp dụng** để dùng ảnh đã crop (file `.png` thay thế trong input). Bấm **Huỷ** để giữ nguyên ảnh gốc.

## 15. Đa ngôn ngữ (VI / EN)

- Người dùng public bấm icon **🌐** trên navbar để chuyển VI ↔ EN.
- Cookie `AquaCMS.Lang` lưu lựa chọn (1 năm).
- Trang admin **luôn tiếng Việt** — thiết kế cho operator nội bộ.
- Thêm/sửa từ điển tại `src/AquaCMS/Services/LocalizationService.cs` (dictionary `_vi`, `_en`).

## 16. Dark mode (Admin)

- Bấm icon **mặt trăng / mặt trời** ở footer sidebar admin để đổi theme.
- Lưu vào `localStorage['aquacms-theme'] = 'dark' | 'light'`.
- Inline script ở `<head>` apply class `.dark` trước khi CSS load → không bị flash.

## 17. Analytics nâng cao (Dashboard)

Trang **Admin → Dashboard** hiển thị thêm:
- **Traffic Source** (top 6): phân loại referrer (Google / Facebook / Zalo / YouTube / Bing / Instagram / TikTok / Trực tiếp / Khác).
- **Device Breakdown**: doughnut chart (Desktop / Mobile / Tablet) dựa trên User-Agent.
- **Conversion Rate**: % unique IP có truy cập `/gio-hang` hoặc `/dat-hang`.
- **Top Pages** (top 8): horizontal bar chart các URL nhiều view nhất 30 ngày qua.

> Yêu cầu: middleware `UsePageViewTracking()` đang chạy + đã apply migration `04`.

## 18. Các lệnh hữu ích

```bash
# Build
dotnet build

# Chạy
dotnet run

# Hot reload
dotnet watch run

# Port khác
dotnet run --urls "http://localhost:5001"

# Tailwind watch
npm run watch:css

# Logs (rolling daily)
# src/AquaCMS/logs/aquacms-*.log
```

## Troubleshooting

### Lỗi "Connection refused" PostgreSQL
- `pg_isready` để kiểm tra service.
- Kiểm tra port 5432 + `pg_hba.conf`.

### Lỗi "relation does not exist" hoặc "column severity does not exist" hoặc "column referrer does not exist"
- Chưa chạy đủ migration. Chạy lại 4 file SQL theo thứ tự ở mục 3.

### Lỗi "password authentication failed"
- Sai password trong `appsettings.json` hoặc user-secrets.

### Lỗi build
- Kiểm tra .NET SDK: `dotnet --version` (cần 9.0+).
- `dotnet restore` để kéo lại packages.
- Nếu lỗi ClosedXML/SignalR: xóa `bin/`, `obj/` rồi restore lại.

### Import Excel không chạy
- File phải là `.xlsx` (không phải `.xls` hoặc `.csv`).
- Hàng tiêu đề (header) phải còn nguyên — đừng xóa.
- Xem TempData["Error"] hiển thị trên trang Sản phẩm để biết dòng nào lỗi.

### Chat realtime không kết nối
- Kiểm tra console browser xem có lỗi WebSocket không.
- CSP header phải allow `ws:` và `wss:` cho host hiện tại.
