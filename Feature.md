# language: vi
# encoding: utf-8
# CatalogaWeb — Feature Specifications (Gherkin/BDD)
# Ngày tạo: 2026-04-24

# ============================================================
# MODULE 1: AUTHENTICATION & AUTHORIZATION
# ============================================================

Tính năng: Xác thực và Phân quyền
  Mô tả: Quản trị viên đăng nhập, hệ thống xác thực JWT và phân quyền dựa trên Role.

  Bối cảnh:
    Cho rằng hệ thống có các tài khoản sau:
      | email              | role        | is_active |
      | admin@cataloga.com | SUPER_ADMIN | true      |
      | mgr@cataloga.com   | MANAGER     | true      |
      | editor@cataloga.com| EDITOR      | true      |
      | sale@cataloga.com  | SALE        | true      |
      | locked@cataloga.com| MANAGER     | false     |

  # --- Đăng nhập ---

  Kịch bản: Đăng nhập thành công
    Khi gửi POST "/api/auth/login" với body:
      | email              | password    |
      | admin@cataloga.com | Admin@123   |
    Thì nhận status 200
    Và response có "token" không rỗng
    Và response có "user.role" bằng "SUPER_ADMIN"
    Và response không chứa "passwordHash"

  Kịch bản: Đăng nhập sai mật khẩu
    Khi gửi POST "/api/auth/login" với body:
      | email              | password   |
      | admin@cataloga.com | sai_mk_123 |
    Thì nhận status 401
    Và response có "message" bằng "Email hoặc mật khẩu không đúng"

  Kịch bản: Đăng nhập tài khoản bị khóa
    Khi gửi POST "/api/auth/login" với body:
      | email               | password   |
      | locked@cataloga.com  | Pass@123  |
    Thì nhận status 401
    Và response có "message" chứa "bị khóa"

  Kịch bản: Lấy thông tin user hiện tại
    Cho rằng đã đăng nhập với role "SUPER_ADMIN"
    Khi gửi GET "/api/auth/me"
    Thì nhận status 200
    Và response có "data.email" bằng "admin@cataloga.com"
    Và response không chứa "passwordHash"

  Kịch bản: Truy cập không có token
    Khi gửi GET "/api/auth/me" không có header Authorization
    Thì nhận status 401

  # --- Phân quyền ---

  Lược đồ kịch bản: Kiểm tra quyền truy cập theo Role
    Cho rằng đã đăng nhập với role "<role>"
    Khi gửi <method> "<endpoint>"
    Thì nhận status <expected_status>

    Ví dụ:
      | role        | method | endpoint               | expected_status |
      | SUPER_ADMIN | GET    | /api/admin/users       | 200             |
      | MANAGER     | GET    | /api/admin/users       | 403             |
      | EDITOR      | GET    | /api/admin/users       | 403             |
      | SALE        | GET    | /api/admin/users       | 403             |
      | MANAGER     | POST   | /api/admin/products    | 201             |
      | EDITOR      | POST   | /api/admin/products    | 403             |
      | SALE        | POST   | /api/admin/products    | 403             |
      | EDITOR      | POST   | /api/admin/knowledge/posts | 201         |
      | SALE        | POST   | /api/admin/knowledge/posts | 403         |

  # --- Đổi mật khẩu ---

  Kịch bản: Đổi mật khẩu thành công
    Cho rằng đã đăng nhập với role "SUPER_ADMIN"
    Khi gửi PUT "/api/auth/change-password" với body:
      | currentPassword | newPassword  |
      | Admin@123       | NewPass@456  |
    Thì nhận status 200

  Kịch bản: Đổi mật khẩu sai mật khẩu cũ
    Cho rằng đã đăng nhập với role "SUPER_ADMIN"
    Khi gửi PUT "/api/auth/change-password" với body:
      | currentPassword | newPassword  |
      | SaiMK@123       | NewPass@456  |
    Thì nhận status 400
    Và response có "message" chứa "không đúng"

# ============================================================
# MODULE 2: PRODUCT MANAGEMENT
# ============================================================

Tính năng: Quản lý Sản phẩm
  Mô tả: CRUD sản phẩm, lọc theo danh mục, tìm kiếm, thao tác hàng loạt.

  Bối cảnh:
    Cho rằng có danh mục "Máy cho ăn" với slug "may-cho-an"
    Và có danh mục "Máy sục khí" với slug "may-suc-khi"
    Và có sản phẩm:
      | name                     | slug              | sku         | category    | price | status    |
      | Máy Cho Tôm Ăn FEED 360 | may-cho-tom-360   | FEED-360    | Máy cho ăn  | null  | available |
      | Máy Sục Khí Oxy Pro      | may-suc-khi-pro   | OXY-PRO     | Máy sục khí | 5000000 | available |
      | Sản phẩm ẩn              | san-pham-an       | HIDDEN-01   | Máy cho ăn  | null  | hidden    |

  # --- Public Endpoints ---

  Kịch bản: Khách xem danh sách sản phẩm - không hiện sản phẩm ẩn
    Khi gửi GET "/api/products"
    Thì nhận status 200
    Và danh sách có 2 sản phẩm
    Và không có sản phẩm nào có status "hidden"
    Và mỗi sản phẩm không có field "contentBlocks"

  Kịch bản: Khách lọc sản phẩm theo danh mục
    Khi gửi GET "/api/products?categorySlug=may-cho-an"
    Thì nhận status 200
    Và danh sách có 1 sản phẩm
    Và sản phẩm đầu tiên có name "Máy Cho Tôm Ăn FEED 360"

  Kịch bản: Khách tìm kiếm sản phẩm
    Khi gửi GET "/api/products?search=oxy"
    Thì nhận status 200
    Và danh sách có 1 sản phẩm
    Và sản phẩm đầu tiên có sku "OXY-PRO"

  Kịch bản: Khách xem chi tiết sản phẩm
    Khi gửi GET "/api/products/may-cho-tom-360"
    Thì nhận status 200
    Và response có "data.contentBlocks" là mảng
    Và response có "data.price" là null
    Và lượt xem sản phẩm tăng 1

  Kịch bản: Giá null hiển thị "Liên hệ"
    Khi gửi GET "/api/products/may-cho-tom-360"
    Thì nhận status 200
    Và response có "data.price" là null

  Kịch bản: Xem sản phẩm không tồn tại
    Khi gửi GET "/api/products/khong-ton-tai-xyz"
    Thì nhận status 404

  # --- Admin Endpoints ---

  Kịch bản: Admin xem tất cả sản phẩm kể cả ẩn
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi GET "/api/admin/products"
    Thì nhận status 200
    Và danh sách có 3 sản phẩm
    Và có sản phẩm với status "hidden"

  Kịch bản: Admin tạo sản phẩm mới
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi POST "/api/admin/products" với body:
      | name              | sku       | categoryId | price   | status    |
      | Máy Bơm Nước V2   | PUMP-V2  | <uuid>     | 3000000 | available |
    Thì nhận status 201
    Và response có "data.slug" không rỗng
    Và response có "data.id" không rỗng

  Kịch bản: Admin tạo sản phẩm - slug tự generate từ name
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi POST "/api/admin/products" với body không có field slug:
      | name              |
      | Máy Bơm Nước V2   |
    Thì nhận status 201
    Và response có "data.slug" bằng "may-bom-nuoc-v2"

  Kịch bản: Admin tạo sản phẩm - slug trùng tự append số
    Cho rằng đã đăng nhập với role "MANAGER"
    Và đã có sản phẩm với slug "may-cho-tom-360"
    Khi gửi POST "/api/admin/products" với body:
      | name                     | slug            |
      | Máy Cho Tôm Ăn FEED 360 | may-cho-tom-360 |
    Thì nhận status 201
    Và response có "data.slug" bằng "may-cho-tom-360-2"

  Kịch bản: Admin cập nhật sản phẩm
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi PUT "/api/admin/products/<product_id>" với body:
      | price   |
      | 6000000 |
    Thì nhận status 200
    Và response có "data.price" bằng 6000000

  Kịch bản: Admin xóa sản phẩm
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi DELETE "/api/admin/products/<product_id>"
    Thì nhận status 200
    Và sản phẩm không còn trong database

  Kịch bản: Admin ẩn nhiều sản phẩm cùng lúc
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi POST "/api/admin/products/bulk" với body:
      | ids                        | action |
      | ["<id1>", "<id2>"]         | hide   |
    Thì nhận status 200
    Và response có "affected" bằng 2
    Và cả 2 sản phẩm có status "hidden"

  Kịch bản: Validation - tên sản phẩm bắt buộc
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi POST "/api/admin/products" với body:
      | name | sku    |
      |      | NO-NAM |
    Thì nhận status 400
    Và response có "errors" chứa thông báo về "name"

# ============================================================
# MODULE 3: CATEGORY MANAGEMENT
# ============================================================

Tính năng: Quản lý Danh mục
  Mô tả: CRUD danh mục sản phẩm, đếm số sản phẩm mỗi danh mục.

  Kịch bản: Lấy danh sách danh mục kèm số lượng sản phẩm
    Khi gửi GET "/api/categories"
    Thì nhận status 200
    Và mỗi danh mục có field "productCount"
    Và danh mục "Máy cho ăn" có "productCount" >= 1

  Kịch bản: Admin tạo danh mục mới
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi POST "/api/admin/categories" với body:
      | name         | slug         |
      | Phụ kiện     | phu-kien     |
    Thì nhận status 201

  Kịch bản: Admin xóa danh mục - sản phẩm con trở thành chưa phân loại
    Cho rằng đã đăng nhập với role "MANAGER"
    Và danh mục "Máy sục khí" có 1 sản phẩm
    Khi gửi DELETE "/api/admin/categories/<category_id>"
    Thì nhận status 200
    Và sản phẩm "Máy Sục Khí Oxy Pro" có categoryId là null

# ============================================================
# MODULE 4: KNOWLEDGE (BLOG/ARTICLES)
# ============================================================

Tính năng: Quản lý Kiến thức
  Mô tả: Bài viết chia sẻ kiến thức, phân theo danh mục, chỉ hiện bài đã xuất bản.

  Bối cảnh:
    Cho rằng có danh mục kiến thức "Kỹ thuật" slug "ky-thuat"
    Và có bài viết:
      | title                    | slug                | is_published | category |
      | Cách tối ưu hóa Oxy     | cach-toi-uu-oxy     | true         | Kỹ thuật |
      | Bài nháp chưa xong       | bai-nhap            | false        | Kỹ thuật |

  Kịch bản: Khách xem danh sách bài viết - chỉ thấy bài đã xuất bản
    Khi gửi GET "/api/knowledge/posts"
    Thì nhận status 200
    Và danh sách có 1 bài viết
    Và bài viết đầu tiên có title "Cách tối ưu hóa Oxy"

  Kịch bản: Khách xem chi tiết bài viết
    Khi gửi GET "/api/knowledge/posts/cach-toi-uu-oxy"
    Thì nhận status 200
    Và response có "data.content" là chuỗi HTML
    Và response có "data.readTime" không rỗng

  Kịch bản: Admin xem tất cả bài viết kể cả nháp
    Cho rằng đã đăng nhập với role "EDITOR"
    Khi gửi GET "/api/admin/knowledge/posts"
    Thì nhận status 200
    Và danh sách có 2 bài viết

  Kịch bản: Admin tạo bài viết mới
    Cho rằng đã đăng nhập với role "EDITOR"
    Khi gửi POST "/api/admin/knowledge/posts" với body:
      | title                | content              | knowledgeCategoryId | isPublished |
      | Hướng dẫn cho ăn     | <p>Nội dung...</p>  | <uuid>              | true        |
    Thì nhận status 201
    Và response có "data.slug" bằng "huong-dan-cho-an"

  Kịch bản: Xóa danh mục kiến thức - bài viết giữ nguyên với category null
    Cho rằng đã đăng nhập với role "EDITOR"
    Khi gửi DELETE "/api/admin/knowledge/categories/<category_id>"
    Thì nhận status 200
    Và bài viết "Cách tối ưu hóa Oxy" có knowledgeCategoryId là null

# ============================================================
# MODULE 5: PARTNERS
# ============================================================

Tính năng: Quản lý Đối tác
  Mô tả: Thông tin đối tác kinh doanh, phân loại, hiển thị cho khách.

  Bối cảnh:
    Cho rằng có danh mục đối tác "Doanh nghiệp" slug "doanh-nghiep"
    Và có đối tác:
      | name            | slug             | is_active | category     |
      | Tập đoàn ABC    | tap-doan-abc     | true      | Doanh nghiệp |
      | Công ty XYZ     | cong-ty-xyz      | false     | Doanh nghiệp |

  Kịch bản: Khách xem danh sách đối tác - chỉ thấy active
    Khi gửi GET "/api/partners"
    Thì nhận status 200
    Và danh sách có 1 đối tác
    Và đối tác đầu tiên có name "Tập đoàn ABC"

  Kịch bản: Khách xem chi tiết đối tác
    Khi gửi GET "/api/partners/tap-doan-abc"
    Thì nhận status 200
    Và response có "data.detailedDescription"
    Và response có "data.contactEmail"

  Kịch bản: Admin CRUD đối tác
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi POST "/api/admin/partners" với body:
      | name         | description    | partnerCategoryId | isActive |
      | Đối tác Mới  | Mô tả ngắn    | <uuid>            | true     |
    Thì nhận status 201

# ============================================================
# MODULE 6: BANNERS
# ============================================================

Tính năng: Quản lý Banner trang chủ
  Mô tả: Banner hiển thị trên trang chủ, chỉ hiện banner active, sắp xếp theo thứ tự.

  Bối cảnh:
    Cho rằng có banner:
      | title           | is_active | sort_order |
      | Banner Chính    | true      | 0          |
      | Banner Phụ      | true      | 1          |
      | Banner Ẩn       | false     | 2          |

  Kịch bản: Khách xem banner trang chủ - chỉ thấy active
    Khi gửi GET "/api/banners"
    Thì nhận status 200
    Và danh sách có 2 banner
    Và banner đầu tiên có title "Banner Chính"
    Và banner được sắp xếp theo sortOrder tăng dần

  Kịch bản: Admin xem tất cả banner kể cả ẩn
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi GET "/api/admin/banners"
    Thì nhận status 200
    Và danh sách có 3 banner

  Kịch bản: Admin tạo banner mới
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi POST "/api/admin/banners" với body:
      | title       | image                | isActive |
      | Banner Mới  | /uploads/banner.jpg  | true     |
    Thì nhận status 201

  Kịch bản: Đồng bộ banner từ GA4
    Cho rằng đã đăng nhập với role "MANAGER"
    Và GA4 đã được cấu hình
    Khi gửi POST "/api/admin/banners/sync-ga4"
    Thì nhận status 200
    Và response có "message" chứa "đồng bộ"

  Kịch bản: Đồng bộ GA4 khi chưa cấu hình
    Cho rằng đã đăng nhập với role "MANAGER"
    Và GA4 chưa được cấu hình
    Khi gửi POST "/api/admin/banners/sync-ga4"
    Thì nhận status 400
    Và response có "message" chứa "chưa cấu hình"

# ============================================================
# MODULE 7: SETTINGS
# ============================================================

Tính năng: Cấu hình hệ thống
  Mô tả: Quản lý thông tin doanh nghiệp, giao diện website.

  Kịch bản: Khách lấy settings public
    Khi gửi GET "/api/settings"
    Thì nhận status 200
    Và response có "data.companyName"
    Và response có "data.phone"
    Và response có "data.primaryColor"
    Và response không chứa thông tin nhạy cảm

  Kịch bản: Admin cập nhật settings
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi PUT "/api/admin/settings" với body:
      | companyName    | phone       |
      | Tên Mới        | 0999888777  |
    Thì nhận status 200
    Và response có "data.companyName" bằng "Tên Mới"
    Và response có "data.phone" bằng "0999888777"
    Và các field không gửi vẫn giữ nguyên giá trị cũ

  Kịch bản: EDITOR không được sửa settings
    Cho rằng đã đăng nhập với role "EDITOR"
    Khi gửi PUT "/api/admin/settings" với body:
      | companyName |
      | Hack        |
    Thì nhận status 403

# ============================================================
# MODULE 8: LIVE CHAT (REAL-TIME)
# ============================================================

Tính năng: Tin nhắn trực tuyến
  Mô tả: Khách vãng lai chat ẩn danh với admin qua WebSocket (SignalR).

  Bối cảnh:
    Cho rằng khách có guestId "abc-123-xyz"

  # --- REST Endpoints ---

  Kịch bản: Khách tạo phiên chat
    Khi gửi POST "/api/chat/sessions" với body:
      | guestId     |
      | abc-123-xyz |
    Thì nhận status 200
    Và response có "data.sessionId" không rỗng

  Kịch bản: Khách lấy lịch sử tin nhắn
    Cho rằng khách đã gửi 5 tin nhắn trước đó
    Khi gửi GET "/api/chat/sessions/abc-123-xyz/messages"
    Thì nhận status 200
    Và danh sách có 5 tin nhắn
    Và tin nhắn được sắp xếp theo thời gian

  Kịch bản: Admin xem danh sách hội thoại
    Cho rằng đã đăng nhập với role "SALE"
    Khi gửi GET "/api/admin/chat/sessions"
    Thì nhận status 200
    Và mỗi session có "unreadCount"
    Và mỗi session có "lastMessage"

  Kịch bản: Admin đánh dấu đã đọc
    Cho rằng đã đăng nhập với role "SALE"
    Khi gửi PUT "/api/admin/chat/sessions/<session_id>/read"
    Thì nhận status 200
    Và session có "unreadCount" bằng 0

  # --- SignalR Hub ---

  Kịch bản: Khách gửi tin nhắn qua SignalR
    Cho rằng khách đã kết nối SignalR hub "/hubs/chat" với guestId "abc-123-xyz"
    Khi khách gọi "SendMessage" với text "Xin chào"
    Thì tin nhắn được lưu vào database
    Và khách nhận event "ReceiveMessage" xác nhận đã gửi
    Và tất cả admin kết nối nhận "NewSessionAlert"
    Và "UnreadCount" của session tăng 1

  Kịch bản: Admin phản hồi tin nhắn qua SignalR
    Cho rằng admin đã kết nối SignalR và join session "abc-123-xyz"
    Khi admin gọi "SendMessage" với guestId "abc-123-xyz" và text "Chào bạn!"
    Thì tin nhắn được lưu với isFromAdmin = true
    Và khách nhận event "ReceiveMessage" với nội dung "Chào bạn!"

# ============================================================
# MODULE 9: FILE UPLOAD
# ============================================================

Tính năng: Tải ảnh lên hệ thống
  Mô tả: Upload ảnh cho sản phẩm, banner, đối tác, bài viết. Tự động resize và convert WebP.

  Kịch bản: Upload ảnh thành công
    Cho rằng đã đăng nhập với role "EDITOR"
    Khi gửi POST "/api/upload" multipart/form-data:
      | file         | folder   |
      | image.jpg    | products |
    Thì nhận status 201
    Và response có "data.url" chứa "/uploads/products/"
    Và response có "data.url" kết thúc bằng ".webp"

  Kịch bản: Upload file không phải ảnh
    Cho rằng đã đăng nhập với role "EDITOR"
    Khi gửi POST "/api/upload" với file "document.pdf"
    Thì nhận status 400
    Và response có "message" chứa "chỉ chấp nhận"

  Kịch bản: Upload file vượt quá 5MB
    Cho rằng đã đăng nhập với role "EDITOR"
    Khi gửi POST "/api/upload" với file ảnh 10MB
    Thì nhận status 413

  Kịch bản: Upload không có token
    Khi gửi POST "/api/upload" không có Authorization header
    Thì nhận status 401

# ============================================================
# MODULE 10: DASHBOARD & ANALYTICS
# ============================================================

Tính năng: Bảng điều khiển thống kê
  Mô tả: Thống kê tổng quan cho admin, tracking lượt xem trang.

  Kịch bản: Admin xem dashboard thống kê
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi GET "/api/admin/dashboard"
    Thì nhận status 200
    Và response có "data.totalVisits"
    Và response có "data.newMessages"
    Và response có "data.totalProducts"
    Và response có "data.weeklyVisits" là mảng 7 phần tử

  Kịch bản: Ghi nhận lượt xem trang sản phẩm
    Khi gửi POST "/api/admin/dashboard/track-view" với body:
      | path                          | entityId | entityType |
      | /product/may-cho-tom-360      | <uuid>   | product    |
    Thì nhận status 200

  Kịch bản: Rate limit tracking - chặn spam
    Khi gửi POST "/api/admin/dashboard/track-view" 15 lần liên tiếp từ cùng IP
    Thì lần thứ 11 trở đi nhận status 429

# ============================================================
# MODULE 11: USER MANAGEMENT
# ============================================================

Tính năng: Quản lý tài khoản quản trị viên
  Mô tả: Chỉ SUPER_ADMIN được CRUD tài khoản. Không được tự xóa mình.

  Bối cảnh:
    Cho rằng đã đăng nhập với role "SUPER_ADMIN" có id "<admin_id>"

  Kịch bản: Super Admin xem danh sách users
    Khi gửi GET "/api/admin/users"
    Thì nhận status 200
    Và không user nào có field "passwordHash"

  Kịch bản: Super Admin tạo user mới
    Khi gửi POST "/api/admin/users" với body:
      | name        | email            | password   | role    |
      | Nhân viên A | nva@cataloga.com | Pass@123   | EDITOR  |
    Thì nhận status 201
    Và response có "data.role" bằng "EDITOR"
    Và response không chứa "passwordHash"

  Kịch bản: Tạo user - email trùng
    Khi gửi POST "/api/admin/users" với body:
      | email              | password | role   |
      | admin@cataloga.com | Pass@123 | EDITOR |
    Thì nhận status 409
    Và response có "message" chứa "đã tồn tại"

  Kịch bản: Tạo user - mật khẩu yếu
    Khi gửi POST "/api/admin/users" với body:
      | email          | password | role   |
      | new@test.com   | 123      | EDITOR |
    Thì nhận status 400
    Và response có "errors" chứa thông báo về mật khẩu

  Kịch bản: Super Admin cập nhật role user
    Khi gửi PUT "/api/admin/users/<user_id>" với body:
      | role    |
      | MANAGER |
    Thì nhận status 200

  Kịch bản: Super Admin không được xóa chính mình
    Khi gửi DELETE "/api/admin/users/<admin_id>"
    Thì nhận status 400
    Và response có "message" chứa "không thể xóa"

  Kịch bản: Super Admin vô hiệu hóa tài khoản
    Khi gửi PUT "/api/admin/users/<user_id>" với body:
      | isActive |
      | false    |
    Thì nhận status 200
    Và user không thể đăng nhập được nữa

# ============================================================
# MODULE 12: SHOPPING CART & ZALO CHECKOUT
# ============================================================

Tính năng: Giỏ hàng và Đặt hàng qua Zalo
  Mô tả: Giỏ hàng lưu trên client (localStorage), checkout redirect qua Zalo.

  Kịch bản: Thêm sản phẩm vào giỏ hàng
    Cho rằng giỏ hàng trống
    Khi người dùng thêm sản phẩm "Máy Cho Tôm Ăn FEED 360" vào giỏ
    Thì giỏ hàng có 1 sản phẩm
    Và số lượng là 1

  Kịch bản: Thêm sản phẩm đã có - cộng dồn số lượng
    Cho rằng giỏ hàng có sản phẩm "Máy Cho Tôm Ăn FEED 360" với số lượng 1
    Khi người dùng thêm lại sản phẩm "Máy Cho Tôm Ăn FEED 360"
    Thì giỏ hàng vẫn có 1 sản phẩm
    Và số lượng là 2

  Kịch bản: Cập nhật số lượng sản phẩm
    Cho rằng giỏ hàng có sản phẩm với số lượng 3
    Khi người dùng giảm số lượng xuống 1
    Thì số lượng sản phẩm là 2

  Kịch bản: Xóa sản phẩm khi số lượng về 0
    Cho rằng giỏ hàng có sản phẩm với số lượng 1
    Khi người dùng giảm số lượng xuống 1
    Thì sản phẩm bị xóa khỏi giỏ hàng

  Kịch bản: Checkout chuyển hướng Zalo
    Cho rằng giỏ hàng có 2 sản phẩm
    Khi người dùng bấm "Đặt hàng qua Zalo"
    Thì hệ thống tạo chuỗi tin nhắn chứa tên và SKU từng sản phẩm
    Và chuyển hướng đến URL Zalo của doanh nghiệp

  Kịch bản: Xóa toàn bộ giỏ hàng
    Cho rằng giỏ hàng có 3 sản phẩm
    Khi người dùng bấm "Xóa tất cả"
    Thì giỏ hàng trống
    Và badge trên navbar hiện 0

# ============================================================
# MODULE 13: SEO
# ============================================================

Tính năng: Tối ưu SEO
  Mô tả: Sitemap, robots.txt, meta tags, structured data.

  Kịch bản: Sitemap XML được generate động
    Khi gửi GET "/sitemap.xml"
    Thì nhận status 200
    Và Content-Type là "application/xml"
    Và XML chứa URL của tất cả sản phẩm visible
    Và XML chứa URL của tất cả bài viết published
    Và XML chứa URL của tất cả đối tác active
    Và XML không chứa URL "/admin"

  Kịch bản: Robots.txt chặn admin và API
    Khi gửi GET "/robots.txt"
    Thì nhận status 200
    Và nội dung chứa "Disallow: /admin"
    Và nội dung chứa "Disallow: /api/"
    Và nội dung chứa "Sitemap:"

  Kịch bản: Trang sản phẩm có đủ meta tags
    Khi truy cập trang chi tiết sản phẩm "Máy Cho Tôm Ăn FEED 360"
    Thì trang có thẻ meta "og:title" chứa tên sản phẩm
    Và trang có thẻ meta "og:description"
    Và trang có thẻ meta "og:image"
    Và trang có script JSON-LD type "Product"

  Kịch bản: Trang bài viết có structured data Article
    Khi truy cập trang chi tiết bài viết "Cách tối ưu hóa Oxy"
    Thì trang có script JSON-LD type "Article"
    Và JSON-LD có "headline" bằng title bài viết
    Và JSON-LD có "datePublished"

# ============================================================
# MODULE 14: CONTENT BLOCKS (SẢN PHẨM)
# ============================================================

Tính năng: Nội dung động sản phẩm (Content Blocks)
  Mô tả: Sản phẩm có nhiều khối nội dung động: text, tech-grid, feature, gallery.

  Kịch bản: Sản phẩm có khối text
    Cho rằng sản phẩm có contentBlock type "text"
    Khi xem chi tiết sản phẩm
    Thì hiển thị tiêu đề khối và nội dung HTML

  Kịch bản: Sản phẩm có khối tech-grid
    Cho rằng sản phẩm có contentBlock type "tech-grid" với items:
      | key       | value  |
      | Công suất | 2000W  |
      | Trọng lượng| 22.5kg|
    Khi xem chi tiết sản phẩm
    Thì hiển thị bảng thông số 2 cột key-value

  Kịch bản: Sản phẩm có khối gallery
    Cho rằng sản phẩm có contentBlock type "gallery" với 4 ảnh
    Khi xem chi tiết sản phẩm
    Thì hiển thị lưới 4 ảnh
    Và click vào ảnh mở lightbox xem phóng to

  Kịch bản: Sản phẩm có khối feature
    Cho rằng sản phẩm có contentBlock type "feature" với image và content
    Khi xem chi tiết sản phẩm
    Thì hiển thị ảnh bên cạnh nội dung mô tả

  Kịch bản: Admin sắp xếp content blocks
    Cho rằng đã đăng nhập với role "MANAGER"
    Và sản phẩm có 3 content blocks theo thứ tự [A, B, C]
    Khi admin di chuyển block C lên đầu
    Thì thứ tự mới là [C, A, B]
    Và khi khách xem sản phẩm thì thấy thứ tự [C, A, B]

  Kịch bản: Admin thêm content block mới cho sản phẩm
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi admin thêm block type "gallery" vào sản phẩm
    Thì sản phẩm có thêm 1 content block
    Và block mới có id unique

  Kịch bản: Validation JSONB content blocks
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi PUT sản phẩm với contentBlocks thiếu field "type"
    Thì nhận status 422
    Và response có "message" chứa "type"

# ============================================================
# MODULE 15: PAGINATION & CACHING
# ============================================================

Tính năng: Phân trang và Cache
  Mô tả: API hỗ trợ phân trang chuẩn, response có cache headers.

  Kịch bản: Phân trang sản phẩm
    Cho rằng có 25 sản phẩm visible
    Khi gửi GET "/api/products?page=1&pageSize=10"
    Thì nhận status 200
    Và danh sách có 10 sản phẩm
    Và meta.total bằng 25
    Và meta.totalPages bằng 3
    Và meta.page bằng 1

  Kịch bản: Phân trang - trang cuối cùng
    Cho rằng có 25 sản phẩm visible
    Khi gửi GET "/api/products?page=3&pageSize=10"
    Thì nhận status 200
    Và danh sách có 5 sản phẩm

  Kịch bản: Phân trang - vượt quá số trang
    Khi gửi GET "/api/products?page=999&pageSize=10"
    Thì nhận status 200
    Và danh sách rỗng

  Kịch bản: Cache headers cho public endpoints
    Khi gửi GET "/api/settings"
    Thì response header "Cache-Control" chứa "public, max-age=300"

  Kịch bản: No-cache cho admin endpoints
    Cho rằng đã đăng nhập với role "MANAGER"
    Khi gửi GET "/api/admin/products"
    Thì response header "Cache-Control" chứa "no-store"
