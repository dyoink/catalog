# Hướng Dẫn Triển Khai AquaCMS Bằng Docker Compose Trên VPS Linux (Cách Dễ Nhất)

Đây là hướng dẫn từng bước (Step-by-Step) để đưa dự án AquaCMS của bạn lên môi trường Internet sử dụng **Docker Compose** và **Nginx**. Cách này đảm bảo code chạy ổn định, tự động cấu hình Database và dễ dàng cập nhật sau này.

---

## 🚀 Bước 1: Chuẩn bị VPS & Tên miền
1. **Thuê 1 VPS Linux**: Khuyên dùng hệ điều hành **Ubuntu 22.04 LTS** hoặc **24.04 LTS**. Cấu hình tối thiểu: 1 vCPU, 2GB RAM.
2. **Trỏ Tên miền (Domain)**: Truy cập trang quản lý tên miền của bạn, tạo một bản ghi `A` trỏ tên miền (ví dụ: `yourdomain.com` và `www.yourdomain.com`) về địa chỉ IP của VPS.

---

## 🛠 Bước 2: Truy cập VPS và Cài đặt Docker
Mở Terminal (Mac/Linux) hoặc Command Prompt/PowerShell (Windows) và gõ lệnh SSH để truy cập vào VPS:

```bash
ssh root@<IP_CUA_VPS>
```
*(Thay `<IP_CUA_VPS>` bằng địa chỉ IP VPS của bạn)*

Sau khi đăng nhập thành công, chạy lệnh dưới đây để tự động cài đặt Docker và Docker Compose:

```bash
curl -fsSL https://get.docker.com -o get-docker.sh && sudo sh get-docker.sh
```

---

## 📥 Bước 3: Đưa mã nguồn lên VPS
Nếu mã nguồn của bạn đang ở trên Github/Gitlab, cách dễ nhất là clone về (Lưu ý: Nếu repo private, bạn cần nhập username/password hoặc dùng SSH key):

```bash
# Tải mã nguồn về thư mục /opt/aquacms
git clone <LINK_GIT_CUA_BAN> /opt/aquacms

# Di chuyển vào thư mục dự án
cd /opt/aquacms
```

---

## ⚙️ Bước 4: Cấu hình Biến môi trường
Hệ thống cần 1 file `.env` để lưu mật khẩu Database và Tên miền. Tạo file này bằng lệnh:

```bash
nano .env
```

Dán nội dung sau vào (nhấp chuột phải để dán), nhớ thay đổi thông tin của bạn:

```env
# Mật khẩu mạnh cho Database PostgreSQL (Không dùng ký tự đặc biệt như $ hay & nếu không rành)
DB_PASSWORD=MatKhauSieuKho123!

# Tên miền chính thức của bạn (Dùng cho cấu hình SEO, Sitemap, Email)
SITE_URL=https://yourdomain.com
```

Bấm `Ctrl + O` -> `Enter` để lưu. Bấm `Ctrl + X` để thoát.

---

## 🚀 Bước 5: Khởi chạy Ứng dụng
Bây giờ, hãy để Docker lo phần còn lại (Build code .NET, setup Database, kết nối chúng với nhau). Chạy lệnh:

```bash
docker compose up -d --build
```
*Đợi khoảng 3-5 phút để hệ thống tải .NET SDK, Node.js (để build CSS Tailwind) và khởi chạy.*

Kiểm tra xem hệ thống đã chạy chưa:
```bash
docker compose ps
```
Nếu bạn thấy `aquacms-app` và `aquacms-db` có trạng thái `Up`, xin chúc mừng! Web của bạn đang chạy ở cổng `8080`.

---

## 🌐 Bước 6: Cài đặt Nginx & HTTPS (Ổ khóa xanh)
Để người dùng truy cập qua tên miền (port 80/443) thay vì phải gõ cổng `8080`, ta cần Nginx làm cầu nối.

**1. Cài Nginx và Let's Encrypt Certbot:**
```bash
sudo apt update
sudo apt install -y nginx certbot python3-certbot-nginx
```

**2. Tạo cấu hình Nginx cho trang web:**
```bash
sudo nano /etc/nginx/sites-available/aquacms
```

Dán nội dung sau vào (Thay `yourdomain.com` bằng tên miền thật):
```nginx
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;
    
    # Cho phép upload file tối đa 25MB (Phải khớp với cấu hình trong appsettings)
    client_max_body_size 25M; 

    # Cấu hình WebSockets cho chức năng Chat (SignalR)
    location /hubs/ {
        proxy_pass http://127.0.0.1:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 86400s;
        proxy_send_timeout 86400s;
    }

    # Cấu hình web thông thường
    location / {
        proxy_pass http://127.0.0.1:8080;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```
Lưu và thoát (`Ctrl+O`, `Enter`, `Ctrl+X`).

**3. Kích hoạt Nginx và lấy chứng chỉ SSL:**
```bash
# Kích hoạt cấu hình
sudo ln -s /etc/nginx/sites-available/aquacms /etc/nginx/sites-enabled/

# Kiểm tra lỗi cú pháp
sudo nginx -t

# Khởi động lại Nginx
sudo systemctl reload nginx

# Chạy Certbot để lấy SSL (Làm theo hướng dẫn trên màn hình, nhập Email, chọn Y)
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com
```

---

## 🔄 Bước 7: Cách cập nhật Code khi có thay đổi
Mỗi khi bạn sửa code trên máy tính và push lên Git, hãy truy cập vào VPS và chạy 3 lệnh sau để cập nhật (quá trình này không làm mất ảnh hay database):

```bash
cd /opt/aquacms
git pull
docker compose build app
docker compose up -d app
```

---

## 🛡️ Bước 8: Kiểm tra Bảo mật cuối cùng (Quan trọng)
Sau khi web đã truy cập được bằng domain, hãy:
1. Đăng nhập ngay vào Admin bằng tài khoản mặc định (`admin@aquacms.com` / `Admin@123`).
2. Vào phần Quản lý Người Dùng -> **Đổi ngay mật khẩu Admin**.
3. Thử upload 1 tấm ảnh xem có bị lỗi dung lượng không.
4. Thử tính năng Chat xem tin nhắn có realtime không (để kiểm tra SignalR WebSocket).

🎉 Hoàn tất! Website AquaCMS của bạn đã sẵn sàng đón khách.