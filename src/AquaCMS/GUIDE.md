# Hướng dẫn Sử dụng Quản trị viên AquaCMS

Chào mừng bạn đến với hệ thống quản trị AquaCMS. Tài liệu này hướng dẫn chi tiết các chức năng có trong khu vực Admin.

## 1. Truy cập Admin
- **Đường dẫn:** `http://your-domain.com/dang-nhap`
- **Tài khoản mặc định:** `admin@aquacms.com` / `Admin@123`

---

## 2. Các chức năng chính

### 2.1 Dashboard (Bảng điều khiển)
- Xem tổng quan số lượng sản phẩm, bài viết, tin nhắn mới.
- Theo dõi biểu đồ lượt xem trang trong 7 ngày gần nhất.
- Phân tích nguồn truy cập (Google, Facebook, Zalo...) và thiết bị (Mobile, Desktop).

### 2.2 Quản lý Sản phẩm
- **Danh mục:** Phân loại sản phẩm (Máy cho ăn, Máy sục khí...). Có thể chỉnh thứ tự hiển thị.
- **Sản phẩm:** 
    - Thêm/Sửa/Xóa sản phẩm.
    - Cấu hình giá, hình ảnh, trạng thái (Còn hàng/Hết hàng/Ẩn).
    - Đánh dấu "Sản phẩm nổi bật" để hiện ra trang chủ.
    - SEO: Tùy chỉnh Meta Title, Meta Description cho từng sản phẩm.

### 2.3 Quản lý Kiến thức (Blog/Tin tức)
- **Danh mục bài viết:** Phân loại kiến thức kỹ thuật, tin tức ngành.
- **Bài viết:**
    - Soạn thảo nội dung với trình chỉnh sửa trực quan.
    - Cấu hình ảnh đại diện, tóm tắt và thời gian đọc dự kiến.

### 2.4 Quản lý Đối tác
- Hiển thị danh sách các nhà phân phối hoặc đối tác công nghệ.
- Giúp khách hàng tin tưởng hơn vào hệ sinh thái của doanh nghiệp.

### 2.5 Quản lý Banner (Slider)
- Thay đổi hình ảnh chạy ở đầu trang chủ.
- Mỗi banner có thể đính kèm tiêu đề, mô tả và nút bấm (Link) dẫn đến sản phẩm khuyến mãi.

### 2.6 Tin nhắn & Chat
- **Chat trực tuyến:** Giao tiếp thời gian thực với khách đang truy cập web.
- **Tin nhắn:** Xem lại lịch sử các cuộc trò chuyện của khách hàng.

### 2.7 Cài đặt hệ thống (Quan trọng)
Đây là nơi cấu hình toàn bộ "linh hồn" của website:
- **Thông tin doanh nghiệp:** Tên công ty, Logo, Địa chỉ, Hotline, Email.
- **Mạng xã hội:** Nhập link Facebook, Zalo, Telegram. Bật/Tắt các nút nổi ở góc màn hình.
- **Giao diện:** Thay đổi màu sắc chủ đạo (Primary Color) toàn trang.
- **Module trang chủ:** Bật/Tắt các khối (Banner, Danh mục, Sản phẩm nổi bật...) tùy theo nhu cầu.
- **SEO:** Cấu hình mã Google Analytics, Facebook Pixel.
- **Email/SMTP:** Cấu hình để hệ thống tự động gửi email thông báo (Xem hướng dẫn chi tiết bên dưới).

---

## 3. Hướng dẫn cấu hình Email (SMTP)
Để web gửi được mail, bạn cần:
1. **SMTP Host:** `smtp.gmail.com`
2. **Port:** `587`
3. **SMTP User:** Địa chỉ Gmail của bạn.
4. **SMTP Password:** Mật khẩu ứng dụng (App Password) 16 ký tự từ tài khoản Google.
5. **Dùng SSL/TLS:** Tích chọn.
6. **Bật gửi email:** Tích chọn và nhấn Lưu.
7. **Test:** Dùng chức năng "Gửi email test" để kiểm tra ngay lập tức.

---

## 4. Lưu ý chung
- **Dung lượng ảnh:** Nên dùng ảnh dưới 2MB để trang web tải nhanh.
- **Bảo mật:** Hãy đổi mật khẩu ngay sau khi đăng nhập lần đầu.
- **Xóa Cache:** Sau khi lưu cài đặt hệ thống, hãy F5 trang chủ để thấy thay đổi mới nhất.
