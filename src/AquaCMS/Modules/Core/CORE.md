# Core Module

## Mục đích
Module nền tảng (infrastructure) — cung cấp các thành phần dùng chung cho toàn bộ hệ thống AquaCMS.

## Trách nhiệm
- **Database Context** (`AppDbContext`) — EF Core PostgreSQL, Fluent API config
- **Data Seeding** (`DataSeeder`) — Tạo dữ liệu mặc định (admin, settings)
- **Middleware** — Security headers, exception handling, request logging
- **Helpers** — Slug generation (Vietnamese), price formatting
- **Tag Helpers** — SEO, Active route
- **Pagination** — `PaginatedList<T>` generic
- **Error Models** — `ErrorViewModel`

## Thành phần

### Database
| File | Mô tả |
|------|--------|
| `Data/AppDbContext.cs` | DbContext chính, Fluent API config tất cả entities |
| `Data/Seed/DataSeeder.cs` | Seed SUPER_ADMIN user + default SiteSettings |

### Helpers
| File | Mô tả |
|------|--------|
| `Helpers/SlugHelper.cs` | Tạo slug từ tiếng Việt (bỏ dấu, lowercase, hyphen) |
| `Helpers/PriceHelper.cs` | Format giá VNĐ, null/0 → "Liên hệ báo giá" |

### Middleware
| File | Mô tả |
|------|--------|
| `Modules/Core/Middleware/SecurityHeadersMiddleware.cs` | CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy |
| `Modules/Core/Middleware/ExceptionHandlingMiddleware.cs` | Global exception handler — log lỗi Serilog, 404 redirect, JSON response cho AJAX |

### Models
| File | Mô tả |
|------|--------|
| `Models/Common/PaginatedList.cs` | Generic paginated list từ IQueryable |
| `Models/ErrorViewModel.cs` | Error page model |

## Cấu hình
- **Connection String**: `appsettings.json` → `ConnectionStrings:DefaultConnection`
- **PostgreSQL Enums**: `user_role`, `product_status` (mapped qua Npgsql)
- **Cache**: `IMemoryCache` — 5 phút cho SiteSettings

## Dependencies
- `Npgsql.EntityFrameworkCore.PostgreSQL` — PostgreSQL provider
- `Isopoh.Cryptography.Argon2` — Password hashing
- `Serilog.AspNetCore` — Structured logging (console + file rolling daily)
- `HtmlSanitizer` — XSS protection cho user-generated content

## Rate Limiting (built-in .NET 9)
| Policy | Giới hạn | Mô tả |
|--------|----------|--------|
| `login` | 5 req/phút/IP | Chống brute-force đăng nhập |
| `api` | 60 req/phút/IP | API endpoints |
| Global | 200 req/phút/IP | Tất cả requests |

## Changelog
| Ngày | Thay đổi |
|------|----------|
| 2026-04-24 | Khởi tạo module, thêm middleware layer |
| 2026-04-24 | Implement SecurityHeaders + ExceptionHandling middleware, Serilog, rate limiting |
