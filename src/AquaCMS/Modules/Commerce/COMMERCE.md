# Commerce Module (Cart & Orders)

## Mục đích
Module thương mại — giỏ hàng, đặt hàng qua Zalo.

## Trách nhiệm
- Giỏ hàng (localStorage-based, client-side)
- Tạo order summary để gửi qua Zalo
- Hiển thị trang giỏ hàng

## Cơ chế
- **Không cần server-side cart** — sản phẩm lưu localStorage
- **Checkout qua Zalo** — tạo message template, redirect đến Zalo
- **Không có payment gateway** — liên hệ trực tiếp

## Cart Data Structure (localStorage)
```json
[
  {
    "id": "uuid",
    "name": "Tên sản phẩm",
    "price": 1500000,
    "image": "/uploads/...",
    "quantity": 2
  }
]
```

## Cart Operations (JS)
| Function | Mô tả |
|----------|--------|
| `Cart.getItems()` | Lấy danh sách items |
| `Cart.add(product)` | Thêm vào giỏ (tăng qty nếu đã có) |
| `Cart.remove(id)` | Xóa item |
| `Cart.updateQuantity(id, qty)` | Cập nhật số lượng |
| `Cart.clear()` | Xóa toàn bộ |
| `Cart.getTotalCount()` | Tổng số items |
| `Cart.updateBadge()` | Cập nhật badge trên navbar |

## Routes
| Route | Controller | Mô tả |
|-------|-----------|--------|
| `/gio-hang` | CartController | Trang giỏ hàng |

## Checkout Flow
1. User thêm sản phẩm vào giỏ → localStorage
2. Xem giỏ hàng → `/gio-hang`
3. Click "Đặt hàng qua Zalo" → redirect Zalo với message template
4. Nhân viên xử lý đơn hàng qua Zalo

## Changelog
| Ngày | Thay đổi |
|------|----------|
| 2026-04-24 | Khởi tạo module, localStorage cart |
