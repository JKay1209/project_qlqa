Restaurant.Client/
│
├── Program.cs                         // Điểm khởi chạy ứng dụng Client
│                                      // - Khởi tạo Application
│                                      // - Đăng ký Dependency Injection (DI)
│                                      // - Khởi tạo HttpClient
│                                      // - Khởi tạo SignalR connection
│                                      // - Mở màn hình LoginLayout
│
├── Layout/                             // Chứa các màn hình giao diện
│   │
│   ├── LoginLayout.cs                   // Màn hình đăng nhập
│   │                                  // - Nhập username, password
│   │                                  // - Kiểm tra dữ liệu đầu vào
│   │                                  // - Gọi LoginLogic để đăng nhập
│   │                                  // - Hiển thị lỗi đăng nhập
│   │
│   ├── MainLayout.cs                    // Màn hình chính sau khi đăng nhập
│   │                                  // - Hiển thị menu chức năng
│   │                                  // - Điều hướng giữa các màn hình
│   │                                  // - Hiển thị thông tin nhân viên
│   │                                  // - Hiển thị trạng thái kết nối Server
│   │
│   ├── TableLayout.cs                   // Màn hình quản lý bàn
│   │                                  // - Hiển thị danh sách bàn
│   │                                  // - Hiển thị trạng thái bàn
│   │                                  // - Mở bàn, gộp bàn, chuyển bàn
│   │                                  // - Chọn bàn để tạo đơn hàng
│   │
│   ├── OrderLayout.cs                   // Màn hình gọi món / quản lý đơn hàng
│   │                                  // - Hiển thị danh sách món ăn
│   │                                  // - Thêm món vào đơn hàng
│   │                                  // - Thay đổi số lượng món
│   │                                  // - Thêm ghi chú món ăn
│   │                                  // - Xóa món khỏi đơn
│   │                                  // - Gửi đơn hàng đến Server
│   │
│   ├── MenuLayout.cs                    // Màn hình quản lý thực đơn
│   │                                  // - Hiển thị danh sách món ăn
│   │                                  // - Tìm kiếm món ăn
│   │                                  // - Lọc theo danh mục
│   │                                  // - Xem giá và trạng thái món
│   │                                  // - Thêm/sửa/xóa món (nếu có quyền)
│   │
│   └── KitchenLayout.cs                 // Màn hình dành cho nhân viên bếp
│                                      // - Hiển thị đơn hàng mới
│                                      // - Hiển thị các món cần chế biến
│                                      // - Cập nhật trạng thái chế biến
│                                      // - Nhận thông báo đơn mới qua SignalR
│                                      // - Cập nhật giao diện khi có sự kiện mới
│
├── LayoutLogics/                        // Xử lý logic giao diện theo mô hình MVVM
│   │
│   ├── LoginLogic.cs              // Logic màn hình đăng nhập
│   │                                  // - Nhận username/password từ Layout
│   │                                  // - Gọi AuthService.LoginAsync()
│   │                                  // - Nhận Access Token
│   │                                  // - Lưu thông tin phiên đăng nhập
│   │                                  // - Điều hướng đến MainLayout
│   │
│   ├── TableLogic.cs              // Logic quản lý bàn
│   │                                  // - Lấy danh sách bàn từ API
│   │                                  // - Tìm kiếm và lọc bàn
│   │                                  // - Gọi API mở/chuyển/gộp bàn
│   │                                  // - Nhận sự kiện TableStatusChanged
│   │                                  // - Cập nhật trạng thái bàn trên giao diện
│   │
│   ├── OrderLogic.cs              // Logic quản lý đơn hàng
│   │                                  // - Lấy danh sách món ăn
│   │                                  // - Thêm/xóa món trong đơn
│   │                                  // - Tính tạm tính trên giao diện
│   │                                  // - Kiểm tra dữ liệu trước khi gửi
│   │                                  // - Gọi API tạo đơn hàng
│   │                                  // - Nhận trạng thái đơn hàng mới
│   │
│   └── KitchenLogic.cs            // Logic màn hình bếp
│                                      // - Lấy danh sách đơn cần chế biến
│                                      // - Nhận OrderCreated từ SignalR
│                                      // - Nhận OrderUpdated từ SignalR
│                                      // - Gửi yêu cầu cập nhật trạng thái
│                                      // - Phân loại đơn theo trạng thái
│
├── Services/                          // Các dịch vụ xử lý phía Client
│   │
│   ├── ApiService.cs                  // Dịch vụ giao tiếp REST API tổng quát
│   │                                  // - GET: Lấy dữ liệu từ Server
│   │                                  // - POST: Gửi dữ liệu lên Server
│   │                                  // - PUT: Cập nhật dữ liệu
│   │                                  // - DELETE: Xóa dữ liệu
│   │                                  // - Chuyển đổi JSON thành đối tượng C#
│   │                                  // - Xử lý HTTP Status Code
│   │                                  // - Đính kèm Access Token vào request
│   │
│   ├── AuthService.cs                  // Dịch vụ xác thực người dùng
│   │                                  // - Gửi yêu cầu đăng nhập
│   │                                  // - Gửi yêu cầu đăng xuất
│   │                                  // - Lấy thông tin tài khoản hiện tại
│   │                                  // - Làm mới Access Token
│   │                                  // - Kiểm tra trạng thái đăng nhập
│   │
│   ├── SignalRService.cs              // Dịch vụ giao tiếp thời gian thực
│   │                                  // - Kết nối đến SignalR Hub
│   │                                  // - Đăng ký lắng nghe các Event
│   │                                  // - Nhận OrderCreated
│   │                                  // - Nhận OrderStatusChanged
│   │                                  // - Nhận TableStatusChanged
│   │                                  // - Gửi yêu cầu lên Hub nếu cần
│   │                                  // - Ngắt kết nối khi đăng xuất
│   │
│   └── NotificationService.cs         // Dịch vụ thông báo
│                                      // - Nhận dữ liệu thông báo từ SignalR
│                                      // - Hiển thị thông báo trên giao diện
│                                      // - Thông báo có đơn hàng mới
│                                      // - Thông báo món đã hoàn thành
│                                      // - Thông báo lỗi kết nối
│
├── Network/                           // Quản lý kết nối mạng
│   │
│   ├── HttpClientManager.cs           // Quản lý HttpClient
│   │                                  // - Khởi tạo HttpClient dùng chung
│   │                                  // - Thiết lập BaseAddress
│   │                                  // - Thiết lập Timeout
│   │                                  // - Cấu hình Header mặc định
│   │                                  // - Tránh tạo HttpClient liên tục
│   │
│   ├── HubConnectionManager.cs        // Quản lý kết nối SignalR
│   │                                  // - Khởi tạo HubConnection
│   │                                  // - Kết nối đến Hub
│   │                                  // - Ngắt kết nối
│   │                                  // - Quản lý trạng thái Connected/
│   │                                  //   Disconnected/Reconnecting
│   │
│   └── ConnectionMonitor.cs           // Theo dõi trạng thái mạng
│                                      // - Kiểm tra trạng thái kết nối
│                                      // - Phát hiện mất kết nối Server
│                                      // - Theo dõi quá trình kết nối lại
│                                      // - Thông báo cho giao diện
│
├── Security/                          // Bảo mật phía Client
│   │
│   ├── TokenManager.cs                // Quản lý Access Token
│   │                                  // - Lưu Access Token trong phiên
│   │                                  // - Cung cấp Token cho ApiService
│   │                                  // - Cập nhật Token mới
│   │                                  // - Xóa Token khi đăng xuất
│   │
│   └── SessionManager.cs              // Quản lý phiên đăng nhập
│                                      // - Lưu thông tin người dùng hiện tại
│                                      // - Lưu UserId, Username, Role
│                                      // - Kiểm tra phiên đăng nhập
│                                      // - Kết thúc phiên khi đăng xuất
│
└── Helpers/                           // Các hàm tiện ích hỗ trợ
    │
    ├── DialogHelper.cs                // Hỗ trợ hiển thị hộp thoại
    │                                  // - Thông báo thành công
    │                                  // - Thông báo lỗi
    │                                  // - Xác nhận xóa hoặc hủy đơn
    │
    └── ValidationHelper.cs            // Kiểm tra dữ liệu đầu vào
                                       // - Kiểm tra trường bắt buộc
                                       // - Kiểm tra số lượng món
                                       // - Kiểm tra định dạng dữ liệu
                                       // - Kiểm tra dữ liệu trước khi gửi API