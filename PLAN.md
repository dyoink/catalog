# AquaCMS — Kế hoạch phát triển (PLAN)

> Cập nhật lần cuối: 28/04/2026

---

## Đã hoàn thành (Done)

### Nền tảng
- [x] Cấu trúc dự án .NET 9.0 MVC + Razor Views
- [x] PostgreSQL 16 + EF Core 9.0 (Npgsql)
- [x] 12 Entity models (User, Product, Category, Post, KnowledgeCategory, Partner, PartnerCategory, Banner, SiteSettings, ChatSession, ChatMessage, PageView)
- [x] AppDbContext với Fluent API configuration đầy đủ
- [x] Data Seeder (admin user + default settings)
- [x] PaginatedList<T> generic cho phân trang
- [x] SlugHelper (Vietnamese slug generation, bỏ dấu)
- [x] PriceHelper (VNĐ formatting)

### Authentication & Authorization
- [x] Cookie-based authentication (không JWT)
- [x] Argon2id password hashing
- [x] 4 roles: SUPER_ADMIN, MANAGER, EDITOR, SALE
- [x] 4 authorization policies: SuperAdmin, ManagerUp, EditorUp, AnyAdmin
- [x] Rate limiting trên login (5 attempts/phút/IP)
- [x] CSRF protection (ValidateAntiForgeryToken)

### Security
- [x] SecurityHeadersMiddleware (CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy)
- [x] ExceptionHandlingMiddleware (global error handler, JSON response cho AJAX)
- [x] Rate limiting: Login (5/min), API (60/min), Global (200/min per IP)
- [x] Serilog structured logging (console + file rolling daily)
- [x] HtmlSanitizer package (Ganss.Xss) — đã integrate vào save flow của Post/Partner/SiteSettings rich text

### SEO nâng cao (P0 — Hoàn thành)
- [x] **URL có ShortId**: `/san-pham/{slug}-{shortId}`, `/kien-thuc/{slug}-{shortId}`, `/doi-tac/{slug}-{shortId}`
- [x] Canonical 301 redirect khi slug thay đổi
- [x] Sitemap.xml động (SitemapService + IMemoryCache 1h, `/sitemap.xml`)
- [x] Robots.txt (`/robots.txt` với link sitemap)
- [x] Meta Title/Description fallback từ SiteSettings (DefaultMetaTitle/Description/OgImage)
- [x] Twitter Card meta tags
- [x] Google Analytics 4 + Facebook Pixel injection (settings-controlled)
- [x] JSON-LD Organization structured data

### File Upload (P0 — Hoàn thành)
- [x] FileUploadService: lưu vào wwwroot/uploads/{folder}/, validate type & size (max 20MB)
- [x] Auto-generate slug-based filename, delete old file khi update/xoá
- [x] Tích hợp vào: Products, Banners, Knowledge, Partners, SiteSettings (logo/hero/about/og)

### Admin mở rộng (P1 — Hoàn thành)
- [x] **KnowledgeCategory CRUD** (`/admin/knowledgecategories`)
- [x] **PartnerCategory CRUD** (`/admin/partnercategories`)
- [x] **Admin Profile + Đổi mật khẩu** (`/admin/profile`)
- [x] **Activity Log Viewer** (`/admin/activity`) — filter theo entity type, paginated
- [x] **ActivityLogService** — log mọi thao tác CREATE/UPDATE/DELETE/CHANGE_PASSWORD với UserId, IP, UserAgent
- [x] **Admin Messages** (`/admin/messages`) — list ChatSessions + chat detail UI
- [x] **Search API** (`/api/search/suggest`) — HTMX dropdown gợi ý sản phẩm + bài viết

### CMS — Tất cả nội dung trang chủ động (P0 — Hoàn thành)
- [x] SiteSettings mở rộng: HeroTitle/Subtitle/Description/ButtonText/ButtonUrl/HeroBackgroundImage
- [x] AboutTitle/AboutContent/AboutImage (section About trên trang chủ)
- [x] FooterAboutText (rich text), CopyrightText
- [x] DefaultMetaTitle/DefaultMetaDescription/DefaultOgImage
- [x] GoogleAnalyticsId, FacebookPixelId
- [x] Trang chủ + Footer dynamic theo settings

### Error Handling & Validation (P0 — Hoàn thành)
- [x] Custom Error pages: 404/403/500 (Views/Home/Error.cshtml)
- [x] Try/catch + ILogger toàn bộ admin controllers
- [x] Server-side validation (ModelState) cho admin forms
- [x] Null safety check ở navigation properties

### Rich text & Editors (Phase 4 — Hoàn thành)
- [x] **Quill 2.0** (free, MIT) thay TinyMCE — không cần API key
- [x] Auto-init mọi `<textarea class="rich-editor">` qua `_AdminLayout`
- [x] **Content Blocks Visual Editor** (Alpine.js): thêm/xóa/sắp xếp blocks (heading/paragraph/image/list/video) + chế độ JSON thô
- [x] **Image preview** instant cho mọi `input[type=file][data-preview]` (Products/Knowledge/Partners/Banners/Settings)
- [x] **Cropper.js** (free, MIT) included — sẵn sàng dung khi cần crop

### Email / Notifications (Phase 4 — Hoàn thành)
- [x] **EmailService** (MailKit 4.16, free, MIT) — cho phép gửi qua SMTP bất kỳ
- [x] Cơ chế cô lập: mọi exception gửi email bị nuốt + log, KHÔNG ảnh hưởng business logic
- [x] **Admin UI cấu hình SMTP**: Host, Port, User, Password, From, SSL/TLS, Notification email
- [x] **Nút gửi email test** trong trang Settings (`/admin/settings/test-email`)
- [x] Tự động notify admin khi có chat mới (fire-and-forget, isolated)

### Live Chat — SignalR (Phase 4 — Hoàn thành)
- [x] **ChatHub** (`/hubs/chat`): JoinAsGuest, JoinAsAdmin, SendFromGuest, SendFromAdmin
- [x] Mọi method bao try/catch + log — không kill connection
- [x] **Chat widget công khai** (Views/Shared/_ChatWidget.cshtml): floating button + panel
- [x] GuestId persistent qua localStorage
- [x] Auto-reconnect (0/2s/5s/10s/30s)
- [x] **Admin chat realtime** (Messages/Detail.cshtml): trả lời trực tiếp qua SignalR
- [x] Email notification cho admin khi có tin mới (fire-and-forget)

### Bulk Import/Export CSV (Phase 4 — Hoàn thành)
- [x] **CsvHelper** package (free, MS-PL/Apache)
- [x] `ProductImportExportController`: Export, Import, Template (download mẫu)
- [x] Match theo Slug — cập nhật nếu đã tồn tại, tạo mới nếu chưa
- [x] Báo cáo lỗi theo dòng, log vào ActivityLog
- [x] UI buttons trong `/admin/products`: Xuất CSV / Tải template / Nhập CSV
- [x] Encoding UTF-8 BOM → mở đúng tiếng Việt trong Excel

### Production CSS Build (Tech debt — Hoàn thành)
- [x] Tailwind CLI setup: `package.json`, `tailwind.config.js`, `src/tailwind.css`
- [x] Plugins: `@tailwindcss/typography`, `@tailwindcss/forms`
- [x] Layouts sử dụng `<environment>` tag helper:
  - **Production**: `~/css/tailwind.css` (file build sẵn)
  - **Development/Staging**: CDN runtime (cho hot reload)
- [x] Build command: `cd src/AquaCMS && npm install && npm run build:css`

### Trang Public (SSR)
- [x] Layout chính: Navbar, Footer, Floating contacts, Mobile menu
- [x] Trang chủ: Banners, danh mục, sản phẩm nổi bật, bài viết, JSON-LD
- [x] Sản phẩm: Listing (search, filter, phân trang), Detail (content blocks, SEO)
- [x] Kiến thức: Listing, Detail
- [x] Đối tác: Listing, Detail
- [x] Giỏ hàng: Cart page (localStorage-based)
- [x] Đăng nhập / Đăng xuất
- [x] SEO: Vietnamese slug URLs, meta tags, Open Graph, canonical

### Admin Panel
- [x] Admin Layout: Sidebar responsive, user info, role-based menu
- [x] Dashboard: Stats cards, traffic overview (hôm nay/tuần/tháng), biểu đồ 7 ngày (Chart.js), top sản phẩm, bài viết gần đây, thao tác nhanh
- [x] Sản phẩm: CRUD, search/filter, bulk actions, content blocks (JSON), phân trang
- [x] Danh mục SP: CRUD đầy đủ
- [x] Kiến thức: CRUD + TinyMCE rich text editor, xuất bản/nháp
- [x] Đối tác: CRUD + TinyMCE cho mô tả chi tiết
- [x] Banner: CRUD, preview grid
- [x] Cài đặt: Form đầy đủ (công ty, MXH, thanh toán, giao diện)
- [x] Người dùng: CRUD, toggle active, role-based access

### UI/UX
- [x] Tailwind CSS (CDN dev, file build cho production)
- [x] Lucide Icons (CDN)
- [x] **Quill 2.0** rich text editor (free, MIT) — đã thay TinyMCE
- [x] **Cropper.js** ảnh preview/crop (free, MIT)
- [x] Chart.js 4 cho dashboard analytics
- [x] HTMX 2.0 cho AJAX search
- [x] Alpine.js 3 cho reactive components (visual content blocks editor)
- [x] **SignalR client 8.0** cho live chat
- [x] Toast notifications (CSS animations)

### Documentation
- [x] Module docs: CORE.md, IDENTITY.md, CATALOG.md, CMS.md, SEARCH.md, COMMERCE.md, PARTNERS.md, MESSAGING.md
- [x] README.md — giới thiệu dự án
- [x] GETTING-STARTED.md — hướng dẫn cài đặt
- [x] PLAN.md — kế hoạch phát triển (file này)

---

## Chưa hoàn thành (Todo cho MVP)

### Ưu tiên cao (P0 — cần cho MVP)
_Tất cả P0 đã hoàn thành ✅_

### Ưu tiên trung bình (P1 — nên có)
_Tất cả P1 đã hoàn thành ✅ (image preview, content blocks editor, CSV import/export, category CRUD, activity log, password change, profile, sitemap)._

### Ưu tiên thấp (P2 — nice to have)
- [x] **Live Chat (SignalR)** — ĐÃ HOÀN THÀNH
- [x] **Email notifications (SMTP)** — ĐÃ HOÀN THÀNH
- [x] **Image crop modal** — ĐÃ HOÀN THÀNH (Cropper.js wired vào `_AdminLayout`, kích hoạt bằng `data-crop="16:9|1:1|free"`)
- [x] **Analytics dashboard nâng cao** — ĐÃ HOÀN THÀNH (traffic source, device breakdown, conversion rate, top pages)
- [x] **Multi-language EN/VI** — ĐÃ HOÀN THÀNH (LocalizationService cookie-based, language switcher trên navbar)
- [x] **Dark mode admin** — ĐÃ HOÀN THÀNH (toggle trong sidebar, lưu localStorage, không flash)
- [ ] **PWA**: Progressive Web App cho trang public
- [ ] **Cache layer**: Redis cache cho sản phẩm, bài viết (hiện chỉ memory cache cho settings)
- [ ] **API endpoints**: REST API cho mobile app
- [ ] **Webhooks**: Notify external systems khi có thay đổi
- [ ] **Backup**: Auto database backup scheduler

---

## Roadmap MVP

### Phase 1: Core CMS ✅ (Hoàn thành)
- Database schema + EF Core
- Authentication + Authorization
- Sản phẩm CRUD
- Bài viết CRUD (TinyMCE)
- Dashboard thống kê
- SEO-friendly routes

### Phase 2: Full Admin ✅ (Hoàn thành)
- Đối tác, Banner, Cài đặt, Người dùng CRUD
- Danh mục sản phẩm CRUD
- Security middleware
- Rate limiting + Serilog logging

### Phase 3: Polish ✅ (Hoàn thành)
- File upload system (FileUploadService + wwwroot/uploads)
- Input validation & HtmlSanitizer (Ganss.Xss)
- Search suggestions API (HTMX dropdown)
- Custom error pages (404/403/500)
- SEO URLs với ShortId + canonical 301
- Sitemap.xml + Robots.txt
- GA4 + Facebook Pixel
- KnowledgeCategory/PartnerCategory/Profile/Activity/Messages admin
- Toàn bộ trang chủ + footer dynamic theo SiteSettings

### Phase 4: Extended Features ✅ (Hoàn thành)
- Quill 2.0 thay TinyMCE (free, MIT)
- Content blocks visual editor (Alpine.js)
- Image preview cho tất cả forms upload
- Bulk CSV import/export sản phẩm (CsvHelper)
- Live chat realtime (SignalR ChatHub)
- Email service (MailKit) + admin UI cấu hình SMTP + nhận thông báo
- Production Tailwind build (npm + tailwindcss CLI)

### Phase 5: Future
- [x] Image crop modal UI (Cropper.js wired) ✅
- [x] Multi-language VI/EN ✅
- [x] Dark mode admin ✅
- [x] Analytics dashboard nâng cao (traffic source, device, conversion) ✅
- [ ] PWA
- [ ] Redis cache layer
- [ ] REST API cho mobile app

---

## Tech Debt / Cần cải thiện
- [x] Tailwind CSS — đã setup CLI build, production dùng file local thay CDN
- [x] TinyMCE — đã thay bằng Quill 2.0 (MIT, không cần API key)
- [ ] Chưa có unit tests
- [ ] Chưa có CI/CD pipeline
- [ ] Chưa có Docker Compose cho deployment
- [ ] Lucide Icons dùng `@latest` → nên pin version
