# Hướng dẫn Deploy AquaCMS

> Tài liệu này mô tả nhiều cách deploy từ **đơn giản nhất** (chạy trực tiếp trên 1 VPS) cho tới **production-grade** (Docker + Nginx + HTTPS + backup).
> Chọn cấp độ phù hợp với quy mô + ngân sách của bạn.

---

## Tổng quan kiến trúc

```
┌─────────┐     HTTPS     ┌────────────┐    TCP 5432   ┌──────────────┐
│ Khách   │ ─────────────▶│  Reverse   │──────────────▶│ PostgreSQL   │
│ trình   │               │  proxy     │               │ 16           │
│ duyệt   │◀───────────── │  (Nginx /  │◀──────────────│              │
└─────────┘     wss://    │  Caddy /   │               └──────────────┘
                          │   IIS)     │
                          └─────┬──────┘
                                │ HTTP 5000 (loopback)
                          ┌─────▼──────┐
                          │ AquaCMS    │
                          │ (Kestrel,  │
                          │  .NET 9)   │
                          └─────┬──────┘
                                │
                       wwwroot/uploads (volume / bind mount)
                       logs/                (volume / bind mount)
                       DataProtection-Keys/ (volume / bind mount)
```

**Yêu cầu chạy production**:
- Linux (khuyến nghị Ubuntu 22.04 / 24.04 LTS) hoặc Windows Server 2019+.
- ≥ 1 vCPU, ≥ 2 GB RAM, ≥ 20 GB disk (SSD).
- PostgreSQL 16 (cùng host hoặc managed service).
- Domain + DNS A record trỏ về VPS.
- Cổng 80/443 mở public.

**Ba thư mục PHẢI persist** (đừng xóa khi redeploy):
| Thư mục | Mục đích |
|---|---|
| `wwwroot/uploads/` | Ảnh sản phẩm / banner / kiến thức / đối tác |
| `logs/` | Serilog rolling daily |
| `DataProtection-Keys/` | Khóa bảo vệ cookie auth — XÓA = đăng xuất toàn bộ user |

---

## Tier 0 — Chuẩn bị chung (mọi cách deploy)

### 0.1 Build artifact

Trên máy dev (hoặc CI):

```bash
cd src/AquaCMS

# Tailwind production CSS (minified)
npm install
npm run build:css       # ghi vào wwwroot/css/site.css

# Publish .NET (linux-x64 self-contained KHÔNG cần .NET runtime trên server)
dotnet publish -c Release -r linux-x64 --self-contained false -o ./publish

# Hoặc self-contained (file lớn ~80 MB nhưng không cần cài runtime)
# dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish
```

> **Đừng quên** chạy `npm run build:css` TRƯỚC `dotnet publish`, nếu không trang sẽ trống style.

### 0.2 Cấu hình Production

Tạo `src/AquaCMS/appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=aquacms;Username=aquacms;Password=ĐỔI_MẬT_KHẨU_MẠNH"
  },
  "Site": {
    "Url": "https://example.com",
    "MaxUploadMb": 5
  },
  "Logging": {
    "LogLevel": { "Default": "Warning", "AquaCMS": "Information" }
  },
  "AllowedHosts": "example.com;www.example.com"
}
```

> **Bảo mật**: KHÔNG commit file này. Đưa secret qua env var hoặc User Secrets:
> `ConnectionStrings__DefaultConnection`, `Site__Url`...

### 0.3 Init database

```bash
sudo -u postgres psql
CREATE USER aquacms WITH PASSWORD 'mật-khẩu-mạnh';
CREATE DATABASE aquacms OWNER aquacms;
\c aquacms
\i db/init/01-schema.sql
\i db/init/02-add-email-settings.sql
\i db/init/03-add-activity-severity.sql
\i db/init/04-add-pageview-referrer.sql
\q
```

---

## Tier 1 — Đơn giản nhất: 1 VPS, không HTTPS, dùng test

> Phù hợp: demo nội bộ / staging.

```bash
# trên VPS Ubuntu
sudo apt update && sudo apt install -y postgresql-16
# (xem mục 0.3 để init DB)

# copy folder publish/ lên VPS, ví dụ /opt/aquacms
scp -r ./publish user@vps:/opt/aquacms

ssh user@vps
cd /opt/aquacms
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
./AquaCMS         # nếu self-contained
# hoặc:  dotnet AquaCMS.dll
```

Truy cập `http://VPS_IP:5000`. Đóng SSH = app dừng. Chỉ dùng để test nhanh.

---

## Tier 2 — VPS Linux + systemd + Nginx + HTTPS (KHUYẾN NGHỊ)

> Phù hợp: production thực sự cho 1 site, lưu lượng nhỏ–trung bình.

### 2.1 Cài runtime trên VPS

```bash
# .NET 9 runtime (ASP.NET Core)
curl -sSL https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh
sudo bash dotnet-install.sh --channel 9.0 --runtime aspnetcore --install-dir /usr/share/dotnet
sudo ln -s /usr/share/dotnet/dotnet /usr/local/bin/dotnet

# PostgreSQL 16
sudo apt install -y postgresql-16

# Nginx + Certbot
sudo apt install -y nginx certbot python3-certbot-nginx
```

### 2.2 Tạo user hệ thống + thư mục

```bash
sudo useradd -r -s /usr/sbin/nologin aquacms
sudo mkdir -p /var/www/aquacms /var/www/aquacms/wwwroot/uploads /var/www/aquacms/logs /var/www/aquacms/DataProtection-Keys
sudo chown -R aquacms:aquacms /var/www/aquacms
```

Copy `publish/` vào `/var/www/aquacms/`:

```bash
sudo rsync -av ./publish/ /var/www/aquacms/
sudo chown -R aquacms:aquacms /var/www/aquacms
```

### 2.3 systemd unit

Tạo `/etc/systemd/system/aquacms.service`:

```ini
[Unit]
Description=AquaCMS .NET 9 web app
After=network.target postgresql.service

[Service]
WorkingDirectory=/var/www/aquacms
ExecStart=/usr/bin/dotnet /var/www/aquacms/AquaCMS.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=aquacms
User=aquacms
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=ConnectionStrings__DefaultConnection=Host=127.0.0.1;Port=5432;Database=aquacms;Username=aquacms;Password=ĐỔI_TÔI

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now aquacms
sudo systemctl status aquacms      # phải Active: running
sudo journalctl -u aquacms -f      # xem log realtime
```

### 2.4 Nginx reverse proxy

Tạo `/etc/nginx/sites-available/aquacms`:

```nginx
server {
    listen 80;
    server_name example.com www.example.com;

    client_max_body_size 25M;   # >= MaxUploadMb cho phép

    # Static files trực tiếp từ disk (nhanh hơn)
    location /uploads/ {
        alias /var/www/aquacms/wwwroot/uploads/;
        expires 30d;
        add_header Cache-Control "public, immutable";
    }
    location /css/ { alias /var/www/aquacms/wwwroot/css/; expires 7d; }
    location /js/  { alias /var/www/aquacms/wwwroot/js/;  expires 7d; }
    location /lib/ { alias /var/www/aquacms/wwwroot/lib/; expires 30d; }

    # SignalR cần WebSocket
    location /hubs/ {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade           $http_upgrade;
        proxy_set_header   Connection        "upgrade";
        proxy_set_header   Host              $host;
        proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_read_timeout 1h;
        proxy_send_timeout 1h;
    }

    location / {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Host              $host;
        proxy_set_header   X-Real-IP         $remote_addr;
        proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_set_header   X-Forwarded-Host  $host;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/aquacms /etc/nginx/sites-enabled/
sudo nginx -t && sudo systemctl reload nginx
```

### 2.5 HTTPS bằng Let's Encrypt

```bash
sudo certbot --nginx -d example.com -d www.example.com
# Certbot tự sửa config nginx + tạo cron renewal
sudo certbot renew --dry-run
```

### 2.6 Forwarded Headers (quan trọng cho HTTPS đúng IP client)

Nếu IP client trong audit log đang là `127.0.0.1`, cần thêm vào `Program.cs` (trước `app.UseRouting`):

```csharp
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownProxies = { System.Net.IPAddress.Parse("127.0.0.1") }
});
```

### 2.7 Firewall

```bash
sudo ufw allow 22
sudo ufw allow 80
sudo ufw allow 443
sudo ufw enable
# KHÔNG mở port 5432 ra public — DB chỉ truy cập qua localhost.
```

---

## Tier 3 — Docker Compose (1 lệnh = up cả app + DB)

> Phù hợp: muốn đóng gói gọn, dễ chuyển server, dễ rollback.

### 3.1 Dockerfile cho app

Tạo `src/AquaCMS/Dockerfile`:

```dockerfile
# === Build stage ===
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Cài Node.js để build Tailwind
RUN apt-get update && apt-get install -y curl \
 && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
 && apt-get install -y nodejs \
 && rm -rf /var/lib/apt/lists/*

# Restore .NET
COPY AquaCMS.csproj .
RUN dotnet restore

# Build CSS + publish
COPY . .
RUN npm install && npm run build:css
RUN dotnet publish -c Release -o /app /p:UseAppHost=false

# === Runtime stage ===
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Tạo user non-root
RUN useradd -r -u 1000 aquacms && chown -R aquacms:aquacms /app
USER aquacms

ENV ASPNETCORE_URLS=http://0.0.0.0:5000 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_PRINT_TELEMETRY_MESSAGE=false

EXPOSE 5000
ENTRYPOINT ["dotnet", "AquaCMS.dll"]
```

### 3.2 Mở rộng `docker-compose.yml`

Thay thế file `docker-compose.yml` ở root bằng:

```yaml
services:
  postgres:
    image: postgres:16-alpine
    container_name: aquacms-db
    restart: unless-stopped
    environment:
      POSTGRES_DB: aquacms
      POSTGRES_USER: aquacms
      POSTGRES_PASSWORD: ${DB_PASSWORD:?set DB_PASSWORD in .env}
    volumes:
      - pgdata:/var/lib/postgresql/data
      - ./db/init:/docker-entrypoint-initdb.d:ro
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U aquacms"]
      interval: 5s
      timeout: 5s
      retries: 10

  app:
    build:
      context: ./src/AquaCMS
      dockerfile: Dockerfile
    container_name: aquacms-app
    restart: unless-stopped
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=aquacms;Username=aquacms;Password=${DB_PASSWORD}"
      Site__Url: "${SITE_URL:-http://localhost:8080}"
      ASPNETCORE_FORWARDEDHEADERS_ENABLED: "true"
    volumes:
      - uploads:/app/wwwroot/uploads
      - logs:/app/logs
      - dpkeys:/app/DataProtection-Keys
    ports:
      - "8080:5000"

volumes:
  pgdata:
  uploads:
  logs:
  dpkeys:
```

Tạo `.env` (đừng commit):

```env
DB_PASSWORD=mật-khẩu-thực-sự-mạnh
SITE_URL=https://example.com
```

### 3.3 Chạy

```bash
docker compose up -d --build
docker compose logs -f app
docker compose ps
```

Truy cập `http://server:8080` rồi đặt Nginx reverse proxy ở Tier 2.4 trỏ vào `127.0.0.1:8080`.

### 3.4 Update lên version mới

```bash
git pull
docker compose build app
docker compose up -d app          # chỉ replace container app, DB vẫn chạy
```

### 3.5 Backup volume

```bash
# Backup DB
docker exec aquacms-db pg_dump -U aquacms aquacms | gzip > backup-$(date +%F).sql.gz

# Backup uploads
docker run --rm -v catalogaweb_uploads:/data -v $PWD:/backup alpine \
  tar czf /backup/uploads-$(date +%F).tar.gz -C /data .
```

---

## Tier 4 — Windows Server + IIS

> Phù hợp: hạ tầng đã có Windows Server, IT team quen IIS.

### 4.1 Cài đặt

1. Cài **.NET 9 Hosting Bundle**: https://dotnet.microsoft.com/download/dotnet/9.0 → "ASP.NET Core Runtime 9.x — Hosting Bundle" → Restart IIS:
   ```powershell
   net stop was /y
   net start w3svc
   ```
2. Cài **PostgreSQL 16** (https://www.postgresql.org/download/windows/) hoặc dùng managed DB.

### 4.2 Publish + copy

Trên máy dev:
```powershell
cd src\AquaCMS
npm run build:css
dotnet publish -c Release -r win-x64 --self-contained false -o .\publish
```

Copy `publish\` → `C:\inetpub\wwwroot\aquacms\` trên server. Tạo thêm các thư mục:
- `C:\inetpub\wwwroot\aquacms\wwwroot\uploads`
- `C:\inetpub\wwwroot\aquacms\logs`
- `C:\inetpub\wwwroot\aquacms\DataProtection-Keys`

Cấp quyền **Modify** cho `IIS AppPool\AquaCMSPool` lên 3 thư mục đó.

### 4.3 IIS site

1. **IIS Manager** → Application Pools → Add Pool: `AquaCMSPool`, .NET CLR = **No Managed Code**, Identity = ApplicationPoolIdentity.
2. Add Website:
   - Site name: `AquaCMS`
   - Physical path: `C:\inetpub\wwwroot\aquacms`
   - Binding: `https`, port 443, hostname `example.com`, chọn cert SSL.
   - App Pool: `AquaCMSPool`.
3. File `web.config` đã được .NET publish tự sinh — KHÔNG sửa thủ công trừ khi cần thêm env var:

   ```xml
   <aspNetCore processPath="dotnet" arguments=".\AquaCMS.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout">
     <environmentVariables>
       <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
       <environmentVariable name="ConnectionStrings__DefaultConnection"
         value="Host=localhost;Port=5432;Database=aquacms;Username=aquacms;Password=ĐỔI_TÔI" />
     </environmentVariables>
   </aspNetCore>
   ```

4. Tăng giới hạn upload (mặc định IIS 30 MB):
   ```xml
   <system.webServer>
     <security>
       <requestFiltering>
         <requestLimits maxAllowedContentLength="26214400" /> <!-- 25 MB -->
       </requestFiltering>
     </security>
   </system.webServer>
   ```

5. **Bật WebSocket** cho SignalR:
   - Server Manager → Add Roles → Web Server → Application Development → **WebSocket Protocol**.

### 4.4 SSL

Dùng `win-acme` (https://www.win-acme.com/) cài Let's Encrypt tự động cho IIS:
```powershell
.\wacs.exe --target iis --siteid <ID>
```

---

## Tier 5 — Cloud managed (AWS / Azure / DigitalOcean App Platform)

### Azure App Service (đơn giản nhất với .NET)

1. Tạo **App Service** Linux, runtime stack = **.NET 9**.
2. Tạo **Azure Database for PostgreSQL Flexible Server** cùng region.
3. App Service → Configuration → Application Settings:
   - `ConnectionStrings__DefaultConnection` (loại Custom): `Host=...postgres.database.azure.com;...`
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `WEBSITE_RUN_FROM_PACKAGE=1`
4. Deploy:
   ```bash
   az webapp deploy --resource-group rg --name aquacms-app --src-path publish.zip
   ```
5. **Persist storage**: bật **Azure Files** mount vào `/home/site/wwwroot/uploads` (vì App Service container ephemeral).
6. **DataProtection keys**: dùng Azure Blob Storage + Key Vault (xem `Microsoft.AspNetCore.DataProtection.AzureStorage`).

### DigitalOcean App Platform

1. Push code lên GitHub.
2. Apps → Create → connect repo → chọn Dockerfile mode (dùng Tier 3.1 Dockerfile).
3. Add Component → Database → PostgreSQL 16 (managed).
4. Env var: `ConnectionStrings__DefaultConnection=${db.DATABASE_URL}` (DO inject sẵn).
5. Volumes: gắn 5GB volume vào `/app/wwwroot/uploads` + `/app/DataProtection-Keys`.

### AWS

- **EC2** + Tier 2 setup ở trên (rẻ nhất).
- **ECS Fargate** + RDS PostgreSQL: dùng image Tier 3.1, mount EFS cho uploads.
- **Elastic Beanstalk** .NET on Linux platform.

---

## Backup & Disaster Recovery

### Backup hàng ngày (cron)

`/etc/cron.daily/aquacms-backup`:

```bash
#!/bin/bash
set -e
BACKUP_DIR=/var/backups/aquacms
mkdir -p $BACKUP_DIR
DATE=$(date +%F)

# Database
sudo -u postgres pg_dump aquacms | gzip > $BACKUP_DIR/db-$DATE.sql.gz

# Uploads
tar czf $BACKUP_DIR/uploads-$DATE.tar.gz -C /var/www/aquacms/wwwroot uploads

# DataProtection keys
tar czf $BACKUP_DIR/dpkeys-$DATE.tar.gz -C /var/www/aquacms DataProtection-Keys

# Xoá backup > 30 ngày
find $BACKUP_DIR -mtime +30 -delete
```

```bash
sudo chmod +x /etc/cron.daily/aquacms-backup
```

### Sync offsite

```bash
# Rclone lên S3 / Google Drive / Dropbox
rclone copy /var/backups/aquacms remote:aquacms-backup
```

### Restore

```bash
gunzip -c db-2026-04-24.sql.gz | sudo -u postgres psql aquacms
tar xzf uploads-2026-04-24.tar.gz -C /var/www/aquacms/wwwroot/
sudo systemctl restart aquacms
```

---

## Update / Zero-downtime deploy (Tier 2)

```bash
# Trên dev:
dotnet publish -c Release -o ./publish
rsync -avz ./publish/ user@vps:/var/www/aquacms-new/

# Trên VPS:
sudo systemctl stop aquacms
sudo rsync -av --delete \
  --exclude=wwwroot/uploads --exclude=logs --exclude=DataProtection-Keys \
  /var/www/aquacms-new/ /var/www/aquacms/
sudo chown -R aquacms:aquacms /var/www/aquacms
sudo systemctl start aquacms
```

> Nếu có migration mới (file `db/init/0X-*.sql`), chạy `psql aquacms < db/init/0X-*.sql` TRƯỚC khi start app.

Zero-downtime thực sự: dùng 2 instance phía sau Nginx + `proxy_next_upstream`, hoặc rolling update với Docker Swarm / Kubernetes.

---

## Security checklist trước khi go-live

- [ ] Đổi password admin mặc định (`admin@aquacms.com / Admin@123`).
- [ ] Đổi password PostgreSQL khỏi giá trị mẫu `AquaCMS@2025`.
- [ ] `appsettings.Production.json` KHÔNG commit vào git.
- [ ] HTTPS bắt buộc (Let's Encrypt hoặc cert thật).
- [ ] `AllowedHosts` chỉ liệt kê domain thật, KHÔNG để `*`.
- [ ] Firewall chỉ mở 22/80/443; DB cổng 5432 chỉ loopback.
- [ ] SSH key-only (disable password auth).
- [ ] Backup tự động + đã test restore ít nhất 1 lần.
- [ ] Cấu hình SMTP thật trong Admin → Cài đặt → Email.
- [ ] CSP/CSRF/Rate-limit middleware đang chạy (mặc định ON).
- [ ] Folder `DataProtection-Keys/` được persist + backup.
- [ ] Set `Site:Url` đúng domain HTTPS để OG/canonical/sitemap đúng.
- [ ] Bật `UseForwardedHeaders` nếu sau reverse proxy (mục 2.6).
- [ ] Limit upload size cả ở Nginx (`client_max_body_size`) và `MaxUploadMb` đồng bộ.

---

## Monitoring & Logs

| Mục | Lệnh / công cụ |
|---|---|
| Live log app | `journalctl -u aquacms -f` (systemd) hoặc `docker compose logs -f app` |
| Log file | `/var/www/aquacms/logs/aquacms-YYYYMMDD.log` (Serilog rolling) |
| Audit nội bộ | Admin → Nhật ký hoạt động |
| Lượt truy cập | Admin → Dashboard → Analytics |
| Healthcheck | `curl -I https://example.com` (200 OK) |
| DB size | `psql -c "SELECT pg_size_pretty(pg_database_size('aquacms'));"` |
| Disk | `df -h /var/www` |

Tích hợp thêm (tuỳ chọn):
- **UptimeRobot** / **BetterStack** — ping HTTPS mỗi 5 phút.
- **Grafana + Loki** — agg log Serilog.
- **pgAdmin** — quản trị DB qua web (đặt sau VPN, KHÔNG public).

---

## Troubleshooting

| Triệu chứng | Nguyên nhân thường gặp | Fix |
|---|---|---|
| 502 Bad Gateway | App crash / chưa start | `journalctl -u aquacms -n 100` |
| 504 Gateway Timeout SignalR | Nginx chưa cấu hình WebSocket | Xem block `/hubs/` mục 2.4 |
| CSS/JS lỗi MIME | Static files thiếu | Chạy `npm run build:css` rồi publish lại |
| Upload ảnh báo 413 | `client_max_body_size` Nginx nhỏ | Tăng lên 25M |
| Đăng nhập tự logout liên tục | Volume `DataProtection-Keys` không persist | Mount volume cố định |
| IP client trong audit log = 127.0.0.1 | Thiếu `UseForwardedHeaders` | Mục 2.6 |
| Migration column not exist | Quên chạy file SQL trong `db/init/` | Chạy theo thứ tự 01→04 |
| Lệnh `dotnet` not found trên IIS | Chưa cài Hosting Bundle (không phải SDK) | Mục 4.1 |
| Container OOM killed | RAM < 1GB | Nâng RAM hoặc thêm swap |

---

## Tổng kết — chọn cách nào?

| Quy mô | Khuyến nghị |
|---|---|
| Demo / chạy thử | **Tier 1** |
| 1 site sản xuất, traffic ≤ 10k/ngày | **Tier 2** (VPS + systemd + Nginx) |
| Cần đóng gói + dễ migrate | **Tier 3** (Docker Compose) |
| Hạ tầng Windows có sẵn | **Tier 4** (IIS) |
| Không muốn quản trị server | **Tier 5** (Azure App Service / DO App Platform) |

> Bắt đầu từ **Tier 2**, scale lên Tier 3/5 khi cần.
