# Hướng Dẫn Cài Đặt và Chạy Database (PostgreSQL) Với Docker Desktop

Dự án AquaCMS đã được cấu hình sẵn `docker-compose.yml` để chạy cơ sở dữ liệu PostgreSQL. Điều này giúp bạn không cần phải cài đặt PostgreSQL trực tiếp lên máy tính của mình.

## Yêu cầu
- Đã cài đặt [Docker Desktop](https://www.docker.com/products/docker-desktop/) trên máy tính.
- Đảm bảo ứng dụng Docker Desktop đang được mở và chạy (Icon Docker hiển thị trạng thái "Running" ở khay hệ thống).

## Bước 1: Khởi động Database với Docker

1. Mở Terminal (Command Prompt, PowerShell hoặc Terminal trong VS Code/Visual Studio).
2. Di chuyển đến thư mục gốc của dự án (nơi chứa file `docker-compose.yml`).
3. Chạy lệnh sau để khởi động container database trong chế độ chạy nền (background):

```bash
docker-compose up -d
```

> **Lưu ý:** Trong lần chạy đầu tiên, Docker sẽ tải image `postgres:16-alpine` về máy (có thể mất vài phút). 
> Các script khởi tạo database (trong thư mục `db/init`) sẽ tự động được thực thi để tạo sẵn cấu trúc bảng cho dự án.

## Bước 2: Kiểm tra trạng thái Container (Tùy chọn)

Để đảm bảo database đang hoạt động tốt, bạn có thể chạy lệnh:

```bash
docker-compose ps
```

Nếu bạn thấy `aquacms-db` có trạng thái (Status) là `Up` (hoặc `healthy`), thì database đã sẵn sàng.

## Bước 3: Chạy ứng dụng AquaCMS

Database hiện đang chạy ở địa chỉ `localhost:5432` với tài khoản mặc định được định nghĩa trong file `src/AquaCMS/appsettings.json`:
- **Database:** `aquacms`
- **Username:** `aquacms`
- **Password:** `AquaCMS@2025`

Bây giờ bạn có thể mở dự án `AquaCMS.sln` bằng Visual Studio hoặc chạy trực tiếp bằng lệnh:

```bash
cd src/AquaCMS
dotnet run
```

Ứng dụng sẽ tự động kết nối với database đang chạy trong Docker.

---

## Quản lý Database bằng Docker

### Tắt Database
Khi không làm việc với dự án nữa, bạn có thể dừng container để tiết kiệm tài nguyên máy tính:

```bash
docker-compose stop
```

### Xóa Container (nhưng giữ lại dữ liệu)
Nếu bạn muốn xóa container:

```bash
docker-compose down
```
*(Dữ liệu database của bạn vẫn được giữ lại do đã cấu hình `volumes: pgdata` trong file docker-compose).*

### Xóa toàn bộ dữ liệu (Reset Database)
Nếu bạn muốn xóa sạch dữ liệu và khởi tạo lại từ đầu, hãy chạy lệnh sau:

```bash
docker-compose down -v
```
*Cảnh báo: Lệnh này sẽ xóa toàn bộ dữ liệu trong database của bạn!*
