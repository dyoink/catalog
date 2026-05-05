# 🌊 AquaCMS - Hệ Thống Quản Trị Nội Dung & Bán Hàng Thủy Sản

AquaCMS là giải pháp website chuyên nghiệp được thiết kế tối ưu cho ngành nuôi trồng thủy sản. Dưới đây là chi tiết các chức năng của hệ thống được tổng hợp từ dữ liệu vận hành thực tế.

---

## 🌍 I. TRANG KHÁCH HÀNG (PUBLIC SITE)

### 1. Trang Chủ (Home Page)
- **Banner Hero Slider:** Hiển thị các chiến dịch quảng bá, sản phẩm mới với hiệu ứng chuyển động mượt mà.
- **Thanh Danh Mục Swiper:** Hệ thống icon danh mục (Máy cho ăn, sục khí, lọc nước...) dạng slide, giúp người dùng lướt và truy cập nhanh các nhóm sản phẩm.
- **Sản phẩm Nổi bật:** Tự động hiển thị các sản phẩm được đánh giá cao hoặc đang khuyến mãi.
- **Sản phẩm theo Danh mục:** Tự động phân loại và hiển thị sản phẩm theo từng nhóm ngành riêng biệt ngay tại trang chủ.
- **Kiến thức mới nhất:** Hiển thị các bài viết hướng dẫn kỹ thuật, tin tức mới nhất từ chuyên mục Blog.

### 2. Quản Lý Sản Phẩm (Product Listing)
- **Lọc Sản phẩm:** Bộ lọc thông minh theo danh mục tại thanh bên trái.
- **Tìm kiếm AJAX:** Thanh tìm kiếm thông minh, gợi ý kết quả (hình ảnh, tên, giá) ngay khi khách hàng đang nhập.
- **Chi tiết Sản phẩm:** Hiển thị đầy đủ thông số kỹ thuật, mô tả chi tiết, giá bán và các sản phẩm liên quan.
- **Đặt hàng qua Zalo:** Nút liên hệ trực tiếp dẫn đến Zalo cá nhân/OA của cửa hàng để chốt đơn nhanh chóng.

### 3. Trang Kiến Thức (Knowledge/Blog)
- **Phân loại chuyên mục:** Tin tức ngành, hướng dẫn kỹ thuật, kinh nghiệm nuôi trồng.
- **Giao diện đọc bài:** Tối ưu hóa cho trải nghiệm đọc, hiển thị ngày đăng, tác giả và lượt xem.

### 4. Công Cụ Hỗ Trợ (Floating Widgets)
- **Nút Zalo/Hotline:** Luôn hiển thị ở góc màn hình để hỗ trợ khách hàng mọi lúc.
- **Chat trực tuyến:** Khung chat realtime kết nối trực tiếp khách hàng với nhân viên quản trị.

---

## 🔐 II. TRANG QUẢN TRỊ (ADMIN PANEL)

### 1. Tổng Quan (Dashboard)
- **Thống kê Lượt xem:** Biểu đồ đường hiển thị lưu lượng truy cập trong 7 ngày gần nhất.
- **Phân tích Nguồn truy cập:** Thống kê khách đến từ Google, Facebook, Zalo hoặc truy cập trực tiếp.
- **Thống kê Thiết bị:** Biểu đồ tròn phân tích tỉ lệ người dùng sử dụng Mobile, Tablet hay Desktop.
- **Thông báo nhanh:** Hiển thị tổng số sản phẩm, bài viết, tin nhắn chưa đọc và tỉ lệ chuyển đổi.

### 2. Quản Lý Nội Dung (CRUD)
- **Quản lý Sản phẩm:** Thêm, sửa, xóa sản phẩm. Hỗ trợ thay đổi trạng thái (Còn hàng/Hết hàng/Ẩn).
- **Quản lý Danh mục:** Tùy chỉnh danh mục sản phẩm và thứ tự hiển thị của chúng trên Slide trang chủ.
- **Quản lý Bài viết:** Hệ thống soạn thảo văn bản phong phú (Rich Text Editor) giúp trình bày bài viết chuyên nghiệp.
- **Quản lý Đối tác:** Quản lý logo và thông tin các đối tác chiến lược.
- **Quản lý Banner:** Cập nhật hình ảnh chạy ở trang chủ, hỗ trợ gắn link điều hướng.

### 3. Công Cụ Nâng Cao
- **Cắt ảnh (Image Cropper):** Tích hợp công cụ cắt ảnh theo tỉ lệ 1:1 hoặc 16:9 trước khi tải lên, giúp giao diện luôn đồng nhất.
- **Nhật ký hoạt động (Audit Log):** Ghi lại chi tiết ai đã thực hiện thao tác gì, vào thời gian nào để bảo mật hệ thống.

### 4. Cài Đặt Hệ Thống (Site Settings)
- **Thông tin Công ty:** Thay đổi Tên công ty, Logo, Địa chỉ, Email và Hotline hiển thị trên toàn web.
- **Cấu hình Chat:** Cài đặt tin nhắn tự động (Auto-reply) khi khách hàng bắt đầu chat.
- **Cấu hình Email (SMTP):** Thiết lập máy chủ gửi mail để nhận thông báo đặt hàng hoặc tin nhắn mới.
- **SEO Mặc định:** Cài đặt Meta Title, Meta Description và hình ảnh chia sẻ (OG Image) để tối ưu hiển thị trên mạng xã hội.
- **Tùy biến Giao diện:** Thay đổi màu sắc chủ đạo của toàn bộ hệ thống (Primary Color) phù hợp với thương hiệu.
- **Quản lý Module:** Chủ động bật/tắt các thành phần trên trang chủ (như ẩn khối Đối tác hoặc khối Banner).

---
*Hệ thống được xây dựng trên nền tảng .NET 10 hiện đại, đảm bảo tốc độ tải trang nhanh và bảo mật tuyệt đối.*
