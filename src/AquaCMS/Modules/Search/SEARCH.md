# Search Module

## Mục đích
Tìm kiếm & lọc sản phẩm, bài viết — tối ưu trải nghiệm người dùng với search nhanh, chính xác.

## Trách nhiệm
- Full-text search PostgreSQL (pg_trgm + unaccent)
- Search suggestions (debounced, AJAX)
- Filter theo danh mục, giá, trạng thái
- Kết quả search highlight
- Search analytics (planned)

## Chiến lược tìm kiếm

### PostgreSQL Extensions (đã cài trong schema)
- `pg_trgm` — Trigram similarity search (fuzzy matching)
- `unaccent` — Bỏ dấu tiếng Việt khi search

### Indexes đã có
```sql
CREATE INDEX idx_products_name_trgm ON products USING gin (name gin_trgm_ops);
CREATE INDEX idx_products_search ON products USING gin (
    (name || ' ' || COALESCE(sku, '') || ' ' || COALESCE(description, ''))
    gin_trgm_ops
);
CREATE INDEX idx_posts_title_trgm ON posts USING gin (title gin_trgm_ops);
```

### Search Flow
1. User gõ keyword → debounce 300ms
2. Request AJAX `GET /api/search?q=...&type=products`
3. Server: `unaccent(LOWER(name)) LIKE unaccent(LOWER(%term%))` + trigram
4. Response: top 5 suggestions (JSON)
5. Full search: redirect to `/san-pham?search=...`

## API Endpoints (planned)
| Route | Method | Mô tả |
|-------|--------|--------|
| `/api/search/suggest` | GET | Search suggestions (top 5) |
| `/api/search/products` | GET | Full product search (paginated) |
| `/api/search/posts` | GET | Full post search (paginated) |

## Service: ISearchService (planned)
| Method | Mô tả |
|--------|--------|
| `SuggestAsync(query, type, limit)` | Gợi ý search nhanh |
| `SearchProductsAsync(query, filters, page)` | Tìm sản phẩm đầy đủ |
| `SearchPostsAsync(query, page)` | Tìm bài viết |

## UX Requirements
- Debounce 300ms khi gõ
- Minimum 2 ký tự mới search
- Hiển thị "Đang tìm kiếm..." loading state
- Empty state: "Không tìm thấy kết quả"
- Search term highlight trong kết quả

## Changelog
| Ngày | Thay đổi |
|------|----------|
| 2026-04-24 | Khởi tạo module, định nghĩa chiến lược search |
