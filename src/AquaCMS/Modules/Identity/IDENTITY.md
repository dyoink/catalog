# Identity Module

## Mục đích
Quản lý xác thực (Authentication) và phân quyền (Authorization) cho hệ thống AquaCMS.

## Trách nhiệm
- Đăng nhập / Đăng xuất (Cookie-based, stateful)
- Quản lý user (CRUD, toggle active)
- Phân quyền theo role (4 cấp)
- Password hashing (Argon2id)
- Session management
- CSRF protection

## Roles & Permissions

| Role | Dashboard | Products | Knowledge | Partners | Banners | Messages | Users | Settings |
|------|-----------|----------|-----------|----------|---------|----------|-------|----------|
| SUPER_ADMIN | ✅ | ✅ CRUD | ✅ CRUD | ✅ CRUD | ✅ CRUD | ✅ | ✅ CRUD | ✅ |
| MANAGER | ✅ | ✅ CRUD | ✅ CRUD | ✅ CRUD | ✅ CRUD | ✅ | ✅ View | ❌ |
| EDITOR | ✅ | ✅ CRUD | ✅ CRUD | ✅ CRUD | ✅ CRUD | ✅ | ❌ | ❌ |
| SALE | ✅ | ✅ View | ✅ View | ✅ View | ✅ View | ✅ | ❌ | ❌ |

## Authorization Policies
| Policy | Roles cho phép |
|--------|---------------|
| `SuperAdmin` | SUPER_ADMIN |
| `ManagerUp` | SUPER_ADMIN, MANAGER |
| `EditorUp` | SUPER_ADMIN, MANAGER, EDITOR |
| `AnyAdmin` | Tất cả 4 roles |

## Entity: User
| Field | Type | Mô tả |
|-------|------|--------|
| Id | Guid | UUID primary key |
| Name | string | Tên hiển thị |
| Email | string | Email đăng nhập (unique) |
| PasswordHash | string | Argon2id hash |
| Role | UserRole | Enum: SUPER_ADMIN, MANAGER, EDITOR, SALE |
| Avatar | string? | URL ảnh đại diện |
| IsActive | bool | Kích hoạt/vô hiệu hóa |
| LastLoginAt | DateTime? | Lần đăng nhập cuối |

## Service: IAuthService
| Method | Mô tả |
|--------|--------|
| `ValidateCredentials(email, password)` | Kiểm tra email + Argon2 verify |
| `HashPassword(password)` | Hash mật khẩu mới |
| `UpdateLastLogin(userId)` | Cập nhật LastLoginAt |

## Controllers
| Controller | Route | Mô tả |
|------------|-------|--------|
| `AccountController` | `/dang-nhap`, `/dang-xuat` | Login/Logout public |
| `Admin/UsersController` | `/admin/users` | CRUD users (ManagerUp) |

## Security
- Cookie: `HttpOnly`, `SameSite=Lax`, 24h expiry, sliding renewal
- Argon2id: memory=65536, iterations=4, parallelism=8
- CSRF: `[ValidateAntiForgeryToken]` trên mọi POST
- Không cho phép user tự xóa/vô hiệu hóa chính mình
- Rate limiting trên login endpoint (planned)

## Changelog
| Ngày | Thay đổi |
|------|----------|
| 2026-04-24 | Khởi tạo module, Cookie auth + Argon2id |
