# CMS Module (Content Management)

## Mục đích
Quản lý nội dung — bài viết kiến thức, banner, cài đặt hệ thống.

## Trách nhiệm
- **Knowledge (Bài viết)**: CRUD bài viết, danh mục kiến thức, SEO
- **Banners**: Quản lý banner trang chủ
- **Site Settings**: Cấu hình hệ thống (singleton pattern)

## Entities

### KnowledgeCategory
| Field | Type | Mô tả |
|-------|------|--------|
| Id | Guid | UUID PK |
| Name | string | Tên danh mục |
| Slug | string | URL slug |
| SortOrder | int | Thứ tự |

### Post
| Field | Type | Mô tả |
|-------|------|--------|
| Id | Guid | UUID PK |
| Slug | string | URL slug |
| Title | string | Tiêu đề |
| Excerpt | string? | Tóm tắt |
| Content | string? | Nội dung HTML |
| Image | string? | Ảnh đại diện |
| Author | string? | Tác giả |
| KnowledgeCategoryId | Guid? | FK → KnowledgeCategory |
| ReadTime | string? | "5 phút đọc" |
| IsPublished | bool | Đã xuất bản |
| PublishedAt | DateTime? | Ngày xuất bản |
| ViewCount | int | Lượt xem |
| MetaTitle | string? | SEO |
| MetaDesc | string? | SEO |

### Banner
| Field | Type | Mô tả |
|-------|------|--------|
| Id | Guid | UUID PK |
| Title | string | Tiêu đề chính |
| Subtitle | string? | Phụ đề |
| Description | string? | Mô tả |
| Image | string? | Ảnh banner |
| Color | string? | Màu nền |
| LinkUrl | string? | URL khi click |
| SortOrder | int | Thứ tự |
| IsActive | bool | Hiển thị/ẩn |

### SiteSettings (Singleton)
| Field | Type | Mô tả |
|-------|------|--------|
| CompanyName | string? | Tên công ty |
| Logo | string? | URL logo |
| Address, Phone, Email | string? | Thông tin liên hệ |
| Facebook, Youtube, Zalo, Tiktok | string? | Mạng xã hội |
| BankName, BankAccount, BankOwner | string? | Thông tin ngân hàng |
| PrimaryColor, BackgroundColor | string? | Theme |
| FooterText | string? | Nội dung footer |

## Services
| Service | Mô tả |
|---------|--------|
| IKnowledgeService | CRUD bài viết + danh mục, lấy published, tăng view |
| IBannerService | CRUD banner, lấy active banners |
| ISettingsService | Get/Update settings (cached 5 phút) |

## Routes (Public)
| Route | Controller | Action |
|-------|-----------|--------|
| `/kien-thuc` | KnowledgeController | Index |
| `/kien-thuc/{slug}` | KnowledgeController | Detail |

## Routes (Admin)
| Route | Controller | Action |
|-------|-----------|--------|
| `/admin/knowledge` | Admin/KnowledgeController | CRUD |
| `/admin/banners` | Admin/BannersController | CRUD |
| `/admin/settings` | Admin/SettingsController | View/Update (SuperAdmin) |

## Security
- HTML content (`Post.Content`) phải được sanitize trước khi lưu
- Settings chỉ SUPER_ADMIN được chỉnh sửa
- Banner/Knowledge: Editor trở lên được CRUD, Manager trở lên được xóa

## Changelog
| Ngày | Thay đổi |
|------|----------|
| 2026-04-24 | Khởi tạo module với Knowledge, Banner, Settings |
