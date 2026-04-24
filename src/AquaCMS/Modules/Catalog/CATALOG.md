# Catalog Module (Product & Category)

## Mục đích
Quản lý catalog sản phẩm — danh mục, sản phẩm, giá, content blocks, hình ảnh, video.

## Trách nhiệm
- CRUD sản phẩm (Admin)
- CRUD danh mục (Admin)
- Hiển thị public: listing, detail, filter theo danh mục
- Content Blocks (JSONB) — nội dung chi tiết linh hoạt
- SEO: slug URL, meta tags, JSON-LD Product structured data
- Lượt xem (view count)
- Sản phẩm nổi bật, liên quan

## Entities

### Category
| Field | Type | Mô tả |
|-------|------|--------|
| Id | Guid | UUID PK |
| Name | string | Tên danh mục |
| Slug | string | URL-friendly slug |
| Image | string? | Ảnh đại diện |
| SortOrder | int | Thứ tự hiển thị |

### Product
| Field | Type | Mô tả |
|-------|------|--------|
| Id | Guid | UUID PK |
| Slug | string | URL slug (unique) |
| Name | string | Tên sản phẩm |
| Sku | string? | Mã sản phẩm |
| CategoryId | Guid? | FK → Category |
| Price | decimal? | Giá (null = "Liên hệ báo giá") |
| Description | string? | Mô tả ngắn |
| Image | string? | Ảnh chính |
| VideoUrl | string? | Link video YouTube |
| Status | ProductStatus | Available / OutOfStock / Hidden |
| ContentBlocks | JsonDocument? | JSONB nội dung chi tiết |
| ViewCount | int | Lượt xem |
| MetaTitle | string? | SEO meta title |
| MetaDesc | string? | SEO meta description |
| IsFeatured | bool | Sản phẩm nổi bật |

### Content Block Types (JSONB)
```json
[
  { "type": "heading", "content": "Thông số kỹ thuật" },
  { "type": "paragraph", "content": "..." },
  { "type": "image", "url": "...", "caption": "..." },
  { "type": "list", "items": ["item1", "item2"] },
  { "type": "table", "headers": [...], "rows": [[...]] }
]
```

## Services
| Service | Method | Mô tả |
|---------|--------|--------|
| IProductService | GetPublicProductsAsync | Listing có search, filter, phân trang |
| IProductService | GetBySlugAsync | Chi tiết theo slug |
| IProductService | GetFeaturedProductsAsync | Sản phẩm nổi bật (trang chủ) |
| IProductService | GetRelatedProductsAsync | Sản phẩm cùng danh mục |
| IProductService | IncrementViewCountAsync | +1 view (atomic SQL) |
| IProductService | GetAdminProductsAsync | Admin listing (kể cả hidden) |
| IProductService | CreateAsync / UpdateAsync / DeleteAsync | CRUD |
| IProductService | BulkActionAsync | Ẩn/hiện/xóa nhiều cùng lúc |
| ICategoryService | GetAllWithCountAsync | Danh mục + số sản phẩm |
| ICategoryService | GetBySlugAsync | Tìm danh mục theo slug |
| ICategoryService | CRUD | Admin quản lý danh mục |

## Routes (Public)
| Route | Controller | Action |
|-------|-----------|--------|
| `/san-pham` | ProductController | Index |
| `/san-pham/{slug}` | ProductController | Detail |
| `/danh-muc/{slug}` | ProductController | Category |

## Routes (Admin)
| Route | Controller | Action |
|-------|-----------|--------|
| `/admin/products` | Admin/ProductsController | Index |
| `/admin/products/create` | Admin/ProductsController | Create |
| `/admin/products/edit/{id}` | Admin/ProductsController | Edit |
| `/admin/products/delete/{id}` | Admin/ProductsController | Delete (POST) |
| `/admin/products/bulk` | Admin/ProductsController | Bulk (POST) |

## SEO
- Product detail: JSON-LD `Product` schema, breadcrumb
- Product listing: Breadcrumb JSON-LD, canonical URL
- Vietnamese slug: `may-suc-khi-ao-tom-750w`

## Changelog
| Ngày | Thay đổi |
|------|----------|
| 2026-04-24 | Khởi tạo module với đầy đủ CRUD + SEO |
