# 🎓 Cẩm Nang Phỏng Vấn Backend Intern - Dự Án AquaCMS

Tài liệu này tổng hợp các câu hỏi phỏng vấn thực tế kèm theo câu trả lời mẫu được tối ưu cho vị trí Backend Intern, dựa trên mã nguồn và kiến trúc của dự án AquaCMS.

---

## 🏗️ PHẦN 1: KIẾN TRÚC & .NET CORE

### Câu 1: Em hãy giải thích về cấu trúc thư mục của AquaCMS và tại sao em lại chia nhỏ thành các lớp (Layers) như vậy?
**Trả lời:**
Dự án được tổ chức theo mô hình **Separation of Concerns (Tách biệt các mối quan tâm)**:
- **Controllers:** Chỉ chịu trách nhiệm nhận yêu cầu (Request), điều phối và trả về kết quả (Response). Không chứa logic nghiệp vụ.
- **Services & Interfaces:** Đây là nơi chứa logic nghiệp vụ chính (Business Logic). Việc dùng Interface giúp code lỏng lẻo (Loose Coupling), dễ dàng thay thế hoặc nâng cấp và hỗ trợ Unit Test.
- **Models/Entities:** Đại diện cho cấu trúc dữ liệu trong Database.
- **Data (AppDbContext):** Lớp kết nối trực tiếp với Database thông qua Entity Framework Core.
- **Lợi ích:** Giúp code dễ đọc, dễ bảo trì và nhiều người có thể cùng làm việc trên các phần khác nhau mà ít bị xung đột.

### Câu 2: Dependency Injection (DI) là gì và em đã áp dụng nó như thế nào trong project?
**Trả lời:**
- **Định nghĩa:** DI là một Design Pattern giúp quản lý việc khởi tạo các đối tượng phụ thuộc. Thay vì tự khởi tạo (`new ProductService()`), chúng ta "yêu cầu" hệ thống cung cấp nó.
- **Áp dụng:** Em đăng ký các Service trong `Program.cs` (ví dụ: `builder.Services.AddScoped<IProductService, ProductService>();`).
- **Lợi ích:** Giúp quản lý vòng đời (Lifecycle) của đối tượng tốt hơn (Transient, Scoped, Singleton) và giúp code linh hoạt hơn.

### Câu 3: Tại sao em lại sử dụng Async/Await trong hầu hết các phương thức Service?
**Trả lời:**
- **Mục đích:** Để giải phóng luồng xử lý (Thread) trong khi chờ các tác vụ I/O (như truy vấn Database hoặc gửi Email) hoàn thành.
- **Lợi ích:** Giúp server xử lý được nhiều yêu cầu đồng thời hơn (tăng Scalability), tránh tình trạng "Thread Starvation" (đói luồng) khiến ứng dụng bị treo.

---

## 💾 PHẦN 2: CƠ SỞ DỮ LIỆU & EF CORE

### Câu 4: Em xử lý việc Phân trang (Pagination) như thế nào để hệ thống không bị chậm khi có hàng triệu sản phẩm?
**Trả lời:**
- Em sử dụng lớp `PaginatedList.cs`. Thay vì lấy toàn bộ dữ liệu về rồi mới chia trang (Client-side), em thực hiện phân trang ngay từ câu lệnh SQL (Server-side) bằng cách sử dụng `.Skip()` và `.Take()` của EF Core.
- **Ví dụ:** Nếu xem trang 2, mỗi trang 10 mục, em sẽ dùng `.Skip(10).Take(10)`.

### Câu 5: Migration trong EF Core là gì? Em làm gì nếu lỡ tay xóa mất database ở môi trường Production?
**Trả lời:**
- **Migration:** Là cách để đồng bộ hóa cấu trúc của các lớp C# (Entities) với cấu trúc bảng trong Database một cách có lịch sử.
- **Xử lý:** Nhờ có các file Migration đã tạo, em có thể chạy lệnh `dotnet ef database update` để tái cấu trúc lại DB từ đầu. Tuy nhiên, về dữ liệu, em cần phải có chiến lược Backup DB định kỳ (ví dụ dùng Cronjob để backup file SQL từ PostgreSQL).

---

## 💬 PHẦN 3: TÍNH NĂNG ĐẶC BIỆT (SIGNALR & SEO)

### Câu 6: Tính năng Chat Real-time sử dụng SignalR hoạt động theo cơ chế nào?
**Trả lời:**
- SignalR sử dụng cơ chế **Hub**. Khi Client kết nối, nó sẽ ưu tiên dùng **WebSockets** (kết nối 2 chiều liên tục).
- Khi khách hàng gửi tin nhắn, nó gọi một phương thức trên Hub, Hub sẽ lưu tin nhắn vào DB thông qua `ChatMessage.cs` và đồng thời đẩy (push) tin nhắn đó đến Admin ngay lập tức mà không cần Admin phải F5 lại trang.

### Câu 7: Làm sao em tối ưu SEO cho các sản phẩm trong AquaCMS?
**Trả lời:**
- Em xây dựng `SlugHelper` để tạo URL thân thiện (ví dụ: `san-pham/may-cho-an-tu-dong`).
- Mỗi sản phẩm/bài viết đều có các trường `MetaTitle`, `MetaDescription` trong DB.
- Em có một `SeoController` để tự động tạo file `sitemap.xml`, giúp Google Bot dễ dàng thu thập dữ liệu của website.

---

## 🐳 PHẦN 4: DOCKER & TRIỂN KHAI

### Câu 8: Tại sao em lại dùng Docker để đóng gói ứng dụng này?
**Trả lời:**
- **Tính nhất quán:** Đảm bảo "chạy được trên máy em là chạy được trên server", không lo thiếu môi trường (.NET SDK, Node.js, v.v.).
- **Dễ triển khai:** Chỉ cần một lệnh `docker-compose up -d` là cả ứng dụng và database đều sẵn sàng.

### Câu 9: Multi-stage Build trong Dockerfile của em có ý nghĩa gì?
**Trả lời:**
- Stage 1 (Build): Dùng SDK nặng để biên dịch code.
- Stage 2 (Runtime): Chỉ copy file thực thi sang một Image nhẹ hơn (ASP.NET Runtime).
- **Kết quả:** Image cuối cùng rất nhỏ gọn, bảo mật hơn vì không chứa mã nguồn và bộ công cụ build.

---

## 🛡️ PHẦN 5: BẢO MẬT

### Câu 10: Em làm thế nào để bảo mật khu vực Admin?
**Trả lời:**
- Em sử dụng **ASP.NET Core Identity** để quản lý tài khoản.
- Áp dụng Attribute `[Authorize(Roles = "Admin")]` cho các Controller trong khu vực Admin để chặn người dùng thường.
- Sử dụng `HtmlSanitizerService` để làm sạch các nội dung nhập từ Editor, tránh tấn công **XSS**.
- Mật khẩu được Identity tự động băm (Hash) bằng thuật toán mạnh (PBKDF2), không lưu mật khẩu dạng text thô.

---
*Ghi chú: Hãy luôn mở code thực tế để minh họa khi trả lời những câu hỏi này.*
