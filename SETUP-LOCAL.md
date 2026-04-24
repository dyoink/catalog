# Hướng dẫn Cài đặt & Chạy Dự án AquaCMS Local

Tài liệu này hướng dẫn chi tiết cách thiết lập môi trường và khởi chạy dự án AquaCMS trên máy tính cá nhân (Local). Dự án là sự kết hợp giữa **.NET 10/9** (Backend & Frontend) và **PostgreSQL** (Database).

---

## 1. Yêu cầu Hệ thống
Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã cài đặt các công cụ sau:
- **.NET SDK 9.0 hoặc 10.0**: [Tải về tại đây](https://dotnet.microsoft.com/download)
- **Docker Desktop**: Dùng để chạy Database PostgreSQL. [Tải về tại đây](https://www.docker.com/products/docker-desktop/)
- **Node.js & npm**: Dùng để biên dịch Tailwind CSS. [Tải về tại đây](https://nodejs.org/)

---

## 2. Khởi động Database (PostgreSQL)

Dự án sử dụng Docker Compose để quản lý Database. 

1. Mở Terminal (PowerShell hoặc CMD) tại thư mục gốc của dự án.
2. Chạy lệnh:
   ```powershell
   docker-compose up -d
   ```
3. Kiểm tra xem container `aquacms-db` đã chạy chưa bằng lệnh: `docker ps`.

**Lưu ý quan trọng:** Do giới hạn của một số driver, kiểu dữ liệu Enum trong Database đã được chuyển sang kiểu `TEXT`. Nếu bạn khởi tạo Database mới hoàn toàn từ script SQL gốc, hãy chạy lệnh sau để đảm bảo tương thích:
```powershell
docker exec -i aquacms-db psql -U aquacms -d aquacms -c "ALTER TABLE users ALTER COLUMN role TYPE text USING role::text; ALTER TABLE products ALTER COLUMN status TYPE text USING status::text;"
```

---

## 3. Cấu hình Ứng dụng

Kiểm tra tệp `src/AquaCMS/appsettings.Development.json` để đảm bảo chuỗi kết nối chính xác:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=aquacms;Username=aquacms;Password=AquaCMS@2025"
}
```

---

## 4. Biên dịch Frontend (Tailwind CSS)

Mặc dù ở chế độ Development ứng dụng có sử dụng CDN, bạn vẫn nên biên dịch CSS để giao diện hiển thị chuẩn nhất:

1. Di chuyển vào thư mục dự án:
   ```powershell
   cd src/AquaCMS
   ```
2. Cài đặt các gói node:
   ```powershell
   npm install
   ```
3. Biên dịch CSS:
   ```powershell
   npm run build:css
   ```

---

## 5. Chạy Ứng dụng

Tại thư mục `src/AquaCMS`, chạy lệnh:

```powershell
dotnet run
```

Sau khi Terminal thông báo `✅ All routes and hubs mapped successfully! Starting server...`, bạn có thể truy cập:

- **Frontend:** [http://localhost:5088](http://localhost:5088)
- **Giỏ hàng:** [http://localhost:5088/gio-hang](http://localhost:5088/gio-hang)
- **Đăng nhập Admin:** [http://localhost:5088/dang-nhap](http://localhost:5088/dang-nhap)

---

## 6. Thông tin Quản trị viên (Mặc định)

Sau khi chạy lần đầu, hệ thống sẽ tự động tạo (Seed) dữ liệu mẫu:
- **Email:** `admin@aquacms.com`
- **Mật khẩu:** `Admin@123`

---

## 7. Các lưu ý khi phát triển
- **Sửa giao diện (.cshtml):** Không cần build lại, chỉ cần F5 trình duyệt.
- **Sửa logic (.cs):** Cần dừng (`Ctrl + C`) và chạy lại `dotnet run`.
- **Lỗi 404:** Nếu gặp lỗi 404 ở các trang như Giỏ hàng, hãy kiểm tra lại cấu hình Route trong `Program.cs`.
- **Màu sắc Navbar:** Đang được ép cứng màu xanh Blue 600 tại `_Layout.cshtml`. Để thay đổi theo cấu hình Admin, hãy xóa thuộc tính `!important` trong inline style của file đó.

---
*Tài liệu được cập nhật lần cuối vào: 24/04/2026*
