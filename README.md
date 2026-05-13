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

### Ảnh minh họa
### I. User

### 1. Trang chủ
*  Banner 
*  Danh mục sản phẩm 
*  Các sản phẩm theo danh mục
*  Các bài viết 
<img width="1905" height="945" alt="Image" src="https://github.com/user-attachments/assets/4d9ae2d3-0dc0-4e81-9556-980bc18d5dbb" />
<img width="1903" height="945" alt="Image" src="https://github.com/user-attachments/assets/a31f0c8b-cbb2-46fd-85a8-c1850641fdcd" />
<img width="1905" height="947" alt="Image" src="https://github.com/user-attachments/assets/7f28c05a-144d-4524-b350-624be6ddc1ec" />

### 2. Sản phẩm
<img width="1903" height="944" alt="Image" src="https://github.com/user-attachments/assets/14bf7180-d6b0-4edc-a2a3-84e7d252abcc" />

### 2.1. Chi tiết sản phẩm
<img width="1904" height="942" alt="Image" src="https://github.com/user-attachments/assets/59dce096-5d44-4f7b-bbef-da84d57e7249" />
<img width="1904" height="943" alt="Image" src="https://github.com/user-attachments/assets/3518d164-21d0-4be0-ac71-838cb83b8d38" />

### 3. Kiến thức
<img width="1919" height="946" alt="Image" src="https://github.com/user-attachments/assets/fa0ee5af-2422-4086-a6eb-ef3a12a6252c" />
<img width="1902" height="943" alt="Image" src="https://github.com/user-attachments/assets/ed4283d9-26a7-4bf5-b0d6-539abbc873a2" />

### 4. Đối tác
<img width="1902" height="945" alt="Image" src="https://github.com/user-attachments/assets/194b8c24-5b9a-4fc0-91e9-07e073989597" />
<img width="1918" height="940" alt="Image" src="https://github.com/user-attachments/assets/6e27103c-4228-4381-bc04-3773674bbec6" />

### 5. Giỏ hàng
<img width="1918" height="943" alt="Image" src="https://github.com/user-attachments/assets/00347543-e261-4bbb-b878-bb09fcbc44b3" />

### II. ADMIN

### 1. Tổng quan

<img width="1917" height="936" alt="Image" src="https://github.com/user-attachments/assets/a038f10c-e29f-40e3-9d8e-4a59d9aca4ae" />
<img width="1904" height="944" alt="Image" src="https://github.com/user-attachments/assets/98a2d233-dc05-45cc-b6ae-cf3a12f1dab1" />
<img width="1908" height="944" alt="Image" src="https://github.com/user-attachments/assets/8f5b177a-0419-4fdd-bdcd-302cc1061bc9" />

### 2. Sản phẩm

### 2.1. Danh sách sản phẩm
<img width="1918" height="943" alt="Image" src="https://github.com/user-attachments/assets/cb38f67f-0e1b-4c23-a1ab-13ad9ef6f429" />

### 2.2. Cập nhật sản phẩm
<img width="1914" height="945" alt="Image" src="https://github.com/user-attachments/assets/4e013351-d9eb-4eeb-92d1-bbcb2fad2b3e" />
<img width="1906" height="945" alt="Image" src="https://github.com/user-attachments/assets/ea67a3bb-e858-4c12-a95c-9e95902c37d0" />

### 2.3. Thêm sản phẩm mới
<img width="1908" height="942" alt="Image" src="https://github.com/user-attachments/assets/413e0cbb-a65a-43d7-8b25-58ebf91897ed" />
<img width="1905" height="946" alt="Image" src="https://github.com/user-attachments/assets/4c84530a-b2a8-456b-995e-2b92d7c8b7ec" />

### 2.4. Danh sách  danh mục sản phẩm
<img width="1915" height="942" alt="Image" src="https://github.com/user-attachments/assets/1dfbfb2f-d2bd-4adc-a2b7-f5b71bbceab9" />

### 3. Danh sách bài viết
<img width="1915" height="944" alt="Image" src="https://github.com/user-attachments/assets/497a0677-aba8-4e76-b832-25e04cfb9143" />

### 4. Danh sách đối tác
<img width="1912" height="941" alt="Image" src="https://github.com/user-attachments/assets/85ede6d6-00a4-4a96-b8dd-272a62a2f0c3" />
<img width="1914" height="945" alt="Image" src="https://github.com/user-attachments/assets/92c345ae-845e-499a-8ed0-8c00229a146d" />

### 5. Banner
<img width="1912" height="944" alt="Image" src="https://github.com/user-attachments/assets/ab17e75c-54e2-47ba-a320-331b2d476e22" />

### 6. Tin nhắn với khách hàng
<img width="1918" height="946" alt="Image" src="https://github.com/user-attachments/assets/4ff50062-0b2e-472a-8e9b-5e3ab76ad086" />

### 7. Quản lí tài khoản ( Chỉ áp dụng với Super Admin )
<img width="1916" height="946" alt="Image" src="https://github.com/user-attachments/assets/8d90015c-d444-4712-a165-c96f831b30a3" />

### 8. Nhật kí hoạt động
<img width="1915" height="940" alt="Image" src="https://github.com/user-attachments/assets/006d3102-6c25-4805-b03b-3fccf99bdb0c" />

### 9. Cài đạt thông tin tài khoản
<img width="1916" height="945" alt="Image" src="https://github.com/user-attachments/assets/91858f1d-e10b-4a71-9935-4a11b1651064" />

### 10. Cài đặt hệ thống
<img width="1910" height="942" alt="Image" src="https://github.com/user-attachments/assets/ad00b4b8-2820-4c2a-8e66-31c2208d5f0f" />
<img width="1915" height="947" alt="Image" src="https://github.com/user-attachments/assets/16a4ce0e-e2f8-4852-b7a2-bb2f3ebe0a34" />
<img width="1907" height="943" alt="Image" src="https://github.com/user-attachments/assets/99fa448b-3a49-47c3-8154-d32433123ed4" />
<img width="1901" height="938" alt="Image" src="https://github.com/user-attachments/assets/e51516b9-cbdf-4c8e-845c-c5c5822a8a2a" />
<img width="1907" height="945" alt="Image" src="https://github.com/user-attachments/assets/03de12d7-48ca-47f5-b466-a60e02c9c0e7" />
<img width="1905" height="944" alt="Image" src="https://github.com/user-attachments/assets/5b252b27-0a03-4c49-9ea2-713bd90367fa" />
<img width="1902" height="944" alt="Image" src="https://github.com/user-attachments/assets/ddfe7b7b-9622-4748-9e04-52a462c05332" />

### 11. Đăng nhập admin
<img width="1898" height="941" alt="Image" src="https://github.com/user-attachments/assets/470c79fd-b271-4425-aed3-c9d69ff61b41" />

@Author : Trần Chí Đức
