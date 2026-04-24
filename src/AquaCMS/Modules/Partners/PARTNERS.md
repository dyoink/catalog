# Partners Module

## Mục đích
Quản lý mạng lưới đối tác — nhà cung cấp, đại lý.

## Trách nhiệm
- CRUD đối tác (Admin)
- CRUD danh mục đối tác (Admin)
- Hiển thị public: listing, detail

## Entities

### PartnerCategory
| Field | Type | Mô tả |
|-------|------|--------|
| Id | Guid | UUID PK |
| Name | string | Tên danh mục |
| Slug | string | URL slug |
| SortOrder | int | Thứ tự |

### Partner
| Field | Type | Mô tả |
|-------|------|--------|
| Id | Guid | UUID PK |
| Slug | string | URL slug |
| Name | string | Tên đối tác |
| Description | string? | Mô tả ngắn |
| DetailedDescription | string? | Giới thiệu HTML |
| PartnerCategoryId | Guid? | FK → PartnerCategory |
| Location | string? | Địa điểm |
| Since | string? | Năm hợp tác |
| Image | string? | Logo/ảnh |
| ContactEmail, ContactPhone, Website | string? | Liên hệ |
| IsActive | bool | Hiển thị |
| SortOrder | int | Thứ tự |

## Routes
| Route | Controller | Mô tả |
|-------|-----------|--------|
| `/doi-tac` | PartnerController | Listing |
| `/doi-tac/{slug}` | PartnerController | Detail |
| `/admin/partners` | Admin/PartnersController | CRUD |

## Changelog
| Ngày | Thay đổi |
|------|----------|
| 2026-04-24 | Khởi tạo module |
