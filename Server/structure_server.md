Restaurant.Server/
│
├── Program.cs                         // Điểm khởi chạy Server
│                                      // - Khởi tạo ASP.NET Core Web API
│                                      // - Đăng ký Dependency Injection
│                                      // - Đăng ký Controllers
│                                      // - Đăng ký SignalR
│                                      // - Đăng ký DbContext
│                                      // - Đăng ký Services và Repositories
│                                      // - Cấu hình JWT Authentication
│                                      // - Cấu hình Authorization
│                                      // - Cấu hình Middleware
│                                      // - Khởi chạy Web Server
│
├── Controllers/                       // Tiếp nhận và xử lý HTTP Request
│   │
│   ├── AuthController.cs              // API xác thực tài khoản
│   │                                  // - POST /api/auth/login
│   │                                  // - POST /api/auth/logout
│   │                                  // - POST /api/auth/refresh
│   │                                  // - GET /api/auth/me
│   │                                  // - Nhận thông tin đăng nhập
│   │                                  // - Trả về Access Token
│   │
│   ├── TableController.cs              // API quản lý bàn ăn
│   │                                  // - GET /api/tables
│   │                                  // - GET /api/tables/{id}
│   │                                  // - POST /api/tables
│   │                                  // - PUT /api/tables/{id}
│   │                                  // - DELETE /api/tables/{id}
│   │                                  // - POST /api/tables/{id}/open
│   │                                  // - POST /api/tables/{id}/merge
│   │                                  // - POST /api/tables/{id}/transfer
│   │
│   ├── MenuController.cs               // API quản lý thực đơn
│   │                                  // - GET /api/menu
│   │                                  // - GET /api/menu/{id}
│   │                                  // - POST /api/menu
│   │                                  // - PUT /api/menu/{id}
│   │                                  // - DELETE /api/menu/{id}
│   │                                  // - Tìm kiếm món ăn
│   │                                  // - Lọc món theo danh mục
│   │                                  // - Cập nhật trạng thái món
│   │
│   ├── OrderController.cs              // API quản lý đơn hàng
│   │                                  // - GET /api/orders
│   │                                  // - GET /api/orders/{id}
│   │                                  // - POST /api/orders
│   │                                  // - PUT /api/orders/{id}
│   │                                  // - PUT /api/orders/{id}/status
│   │                                  // - POST /api/orders/{id}/cancel
│   │                                  // - Kiểm tra dữ liệu request
│   │                                  // - Gọi OrderService xử lý nghiệp vụ
│   │                                  // - Trả kết quả về Client
│   │
│   ├── PaymentController.cs            // API quản lý thanh toán
│   │                                  // - POST /api/payments
│   │                                  // - GET /api/payments/{id}
│   │                                  // - GET /api/payments/order/{orderId}
│   │                                  // - Xử lý yêu cầu thanh toán
│   │                                  // - Lấy thông tin hóa đơn
│   │                                  // - Trả kết quả thanh toán
│   │
│   └── EmployeeController.cs           // API quản lý nhân viên
│                                      // - GET /api/employees
│                                      // - GET /api/employees/{id}
│                                      // - POST /api/employees
│                                      // - PUT /api/employees/{id}
│                                      // - DELETE /api/employees/{id}
│                                      // - Quản lý thông tin nhân viên
│                                      // - Quản lý tài khoản nhân viên
│
├── Hubs/                              // Xử lý giao tiếp thời gian thực
│   │
│   ├── RestaurantHub.cs               // Hub giao tiếp chung của nhà hàng
│   │                                  // - Quản lý kết nối Client
│   │                                  // - Quản lý ngắt kết nối
│   │                                  // - Quản lý các Group
│   │                                  // - Join Group theo vai trò
│   │                                  // - Join Group theo khu vực/bộ phận
│   │                                  // - Gửi thông báo đến các Client
│   │
│   ├── KitchenHub.cs                   // Hub dành cho màn hình bếp
│   │                                  // - Quản lý kết nối nhân viên bếp
│   │                                  // - Tham gia Group Kitchen
│   │                                  // - Nhận yêu cầu liên quan đến bếp
│   │                                  // - Gửi thông báo đơn hàng mới
│   │                                  // - Gửi cập nhật trạng thái chế biến
│   │
│   └── NotificationHub.cs              // Hub gửi thông báo
│                                      // - Gửi thông báo đến nhân viên
│                                      // - Gửi thông báo đến nhóm người dùng
│                                      // - Gửi thông báo thanh toán
│                                      // - Gửi thông báo đơn hàng
│                                      // - Gửi thông báo trạng thái bàn
│
├── Services/                          // Xử lý nghiệp vụ của ứng dụng
│   │
│   ├── AuthService.cs                  // Nghiệp vụ xác thực
│   │                                  // - Kiểm tra username/password
│   │                                  // - Tìm tài khoản trong Database
│   │                                  // - Kiểm tra tài khoản bị khóa
│   │                                  // - Xác thực mật khẩu
│   │                                  // - Yêu cầu JwtService tạo Token
│   │                                  // - Kiểm tra quyền đăng nhập
│   │
│   ├── TableService.cs                 // Nghiệp vụ quản lý bàn
│   │                                  // - Lấy danh sách bàn
│   │                                  // - Kiểm tra bàn tồn tại
│   │                                  // - Mở bàn
│   │                                  // - Đóng bàn
│   │                                  // - Chuyển bàn
│   │                                  // - Gộp bàn
│   │                                  // - Kiểm tra trạng thái bàn
│   │                                  // - Phát sự kiện khi bàn thay đổi
│   │
│   ├── MenuService.cs                  // Nghiệp vụ quản lý thực đơn
│   │                                  // - Lấy danh sách món ăn
│   │                                  // - Thêm món ăn
│   │                                  // - Cập nhật món ăn
│   │                                  // - Xóa hoặc ẩn món ăn
│   │                                  // - Quản lý danh mục món
│   │                                  // - Kiểm tra giá bán
│   │                                  // - Kiểm tra món còn kinh doanh
│   │
│   ├── OrderService.cs                 // Nghiệp vụ quản lý đơn hàng
│   │                                  // - Tạo đơn hàng
│   │                                  // - Thêm món vào đơn hàng
│   │                                  // - Xóa món khỏi đơn hàng
│   │                                  // - Cập nhật số lượng
│   │                                  // - Tính tổng tiền
│   │                                  // - Kiểm tra bàn có hợp lệ
│   │                                  // - Kiểm tra món còn phục vụ
│   │                                  // - Cập nhật trạng thái đơn
│   │                                  // - Hủy đơn hàng
│   │                                  // - Lưu dữ liệu vào Database
│   │                                  // - Phát sự kiện OrderCreated
│   │                                  // - Phát sự kiện OrderStatusChanged
│   │
│   ├── PaymentService.cs               // Nghiệp vụ thanh toán
│   │                                  // - Tính tổng tiền cần thanh toán
│   │                                  // - Tính giảm giá
│   │                                  // - Tính thuế/phí nếu có
│   │                                  // - Tạo hóa đơn
│   │                                  // - Kiểm tra đơn đã thanh toán chưa
│   │                                  // - Ghi nhận phương thức thanh toán
│   │                                  // - Cập nhật trạng thái đơn
│   │                                  // - Cập nhật trạng thái bàn
│   │                                  // - Phát sự kiện PaymentCompleted
│   │
│   └── NotificationService.cs          // Nghiệp vụ thông báo
│                                      // - Tạo nội dung thông báo
│                                      // - Gửi thông báo qua SignalR
│                                      // - Thông báo đơn hàng mới
│                                      // - Thông báo thanh toán
│                                      // - Thông báo thay đổi trạng thái bàn
│
├── Repositories/                      // Truy xuất dữ liệu Database
│   │
│   ├── TableRepository.cs              // Truy vấn dữ liệu bàn
│   │                                  // - Lấy danh sách bàn
│   │                                  // - Lấy bàn theo ID
│   │                                  // - Thêm bàn
│   │                                  // - Cập nhật bàn
│   │                                  // - Xóa bàn
│   │                                  // - Kiểm tra bàn tồn tại
│   │
│   ├── MenuRepository.cs               // Truy vấn dữ liệu thực đơn
│   │                                  // - Lấy danh sách món
│   │                                  // - Tìm món theo ID
│   │                                  // - Tìm món theo tên
│   │                                  // - Lọc theo danh mục
│   │                                  // - Thêm/sửa/xóa món
│   │
│   ├── OrderRepository.cs              // Truy vấn dữ liệu đơn hàng
│   │                                  // - Lấy danh sách đơn hàng
│   │                                  // - Lấy đơn theo ID
│   │                                  // - Lấy đơn theo bàn
│   │                                  // - Lấy chi tiết đơn hàng
│   │                                  // - Thêm đơn hàng
│   │                                  // - Cập nhật đơn hàng
│   │                                  // - Lưu chi tiết đơn
│   │
│   └── EmployeeRepository.cs           // Truy vấn dữ liệu nhân viên
│                                      // - Tìm nhân viên theo ID
│                                      // - Tìm tài khoản theo username
│                                      // - Lấy danh sách nhân viên
│                                      // - Thêm/sửa/xóa nhân viên
│
├── Models/                            // Mô hình dữ liệu phía Server
│   │
│   ├── Entities/                      // Các Entity ánh xạ với Database
│   │   │
│   │   ├── User.cs                    // Thông tin tài khoản đăng nhập
│   │   │                              // - UserId
│   │   │                              // - Username
│   │   │                              // - PasswordHash
│   │   │                              // - RoleId
│   │   │                              // - IsActive
│   │   │
│   │   ├── Table.cs                   // Thông tin bàn ăn
│   │   │                              // - TableId
│   │   │                              // - TableName
│   │   │                              // - Capacity
│   │   │                              // - TableStatus
│   │   │
│   │   ├── MenuItem.cs                // Thông tin món ăn
│   │   │                              // - MenuItemId
│   │   │                              // - Name
│   │   │                              // - Price
│   │   │                              // - CategoryId
│   │   │                              // - IsAvailable
│   │   │
│   │   ├── Order.cs                   // Thông tin đơn hàng
│   │   │                              // - OrderId
│   │   │                              // - TableId
│   │   │                              // - EmployeeId
│   │   │                              // - CreatedAt
│   │   │                              // - TotalAmount
│   │   │                              // - OrderStatus
│   │   │
│   │   └── OrderDetail.cs             // Chi tiết từng món trong đơn
│   │                                  // - OrderDetailId
│   │                                  // - OrderId
│   │                                  // - MenuItemId
│   │                                  // - Quantity
│   │                                  // - UnitPrice
│   │                                  // - Note
│   │                                  // - ItemStatus
│   │
│   └── Database/                      // Cấu hình truy cập Database
│       │
│       └── RestaurantDbContext.cs     // DbContext của Entity Framework Core
│                                      // - Khai báo DbSet<User>
│                                      // - Khai báo DbSet<Table>
│                                      // - Khai báo DbSet<MenuItem>
│                                      // - Khai báo DbSet<Order>
│                                      // - Khai báo DbSet<OrderDetail>
│                                      // - Cấu hình khóa chính
│                                      // - Cấu hình khóa ngoại
│                                      // - Cấu hình quan hệ giữa các bảng
│                                      // - Cấu hình kiểu dữ liệu và độ dài
│
├── Middleware/                        // Xử lý các request xuyên suốt hệ thống
│   │
│   ├── ExceptionMiddleware.cs          // Xử lý ngoại lệ tập trung
│   │                                  // - Bắt lỗi không được xử lý
│   │                                  // - Ghi log lỗi
│   │                                  // - Trả HTTP 400/404/500 phù hợp
│   │                                  // - Không để lộ thông tin nội bộ
│   │
│   └── LoggingMiddleware.cs            // Ghi nhật ký HTTP Request/Response
│                                      // - Ghi HTTP Method
│                                      // - Ghi Endpoint
│                                      // - Ghi thời gian xử lý
│                                      // - Ghi HTTP Status Code
│                                      // - Theo dõi request phục vụ debug
│
├── Security/                          // Các thành phần bảo mật
│   │
│   ├── JwtService.cs                  // Tạo và quản lý JWT
│   │                                  // - Tạo Access Token
│   │                                  // - Gắn UserId vào Token
│   │                                  // - Gắn Role vào Token
│   │                                  // - Thiết lập thời hạn Token
│   │                                  // - Tạo Refresh Token nếu sử dụng
│   │
│   ├── PasswordHasher.cs              // Mã hóa/băm mật khẩu
│   │                                  // - Hash mật khẩu khi đăng ký
│   │                                  // - Verify mật khẩu khi đăng nhập
│   │                                  // - Sử dụng thuật toán băm mật khẩu
│   │                                  //   chuyên dụng có salt
│   │
│   └── PermissionService.cs            // Kiểm tra quyền truy cập
│                                      // - Kiểm tra vai trò người dùng
│                                      // - Kiểm tra quyền sử dụng chức năng
│                                      // - Kiểm tra quyền quản lý bàn
│                                      // - Kiểm tra quyền quản lý món
│                                      // - Kiểm tra quyền thanh toán
│                                      // - Ngăn người dùng truy cập trái phép
│
└── Configuration/                     // Các cấu hình của Server
    │
    └── DatabaseConfiguration.cs       // Cấu hình kết nối SQL Server
                                       // - Đọc Connection String
                                       // - Cấu hình DbContext
                                       // - Cấu hình SQL Server Provider
                                       // - Cấu hình thời gian chờ kết nối