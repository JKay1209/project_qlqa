Restaurant.Shared/
│
├── DTOs/                               // Data Transfer Objects
│   │                                  // Định nghĩa dữ liệu gửi/nhận qua API
│   │
│   ├── Auth/                           // DTO liên quan đến xác thực
│   │   │
│   │   ├── LoginRequest.cs             // Dữ liệu Client gửi khi đăng nhập
│   │   │                              // - Username
│   │   │                              // - Password
│   │   │
│   │   ├── LoginResponse.cs            // Dữ liệu Server trả về sau đăng nhập
│   │   │                              // - UserId
│   │   │                              // - Username
│   │   │                              // - Role
│   │   │                              // - AccessToken
│   │   │                              // - ExpiresAt
│   │   │
│   │   └── UserDto.cs                  // Thông tin người dùng gửi về Client
│   │                                  // - UserId
│   │                                  // - Username
│   │                                  // - DisplayName
│   │                                  // - Role
│   │
│   ├── Table/                          // DTO liên quan đến bàn
│   │   │
│   │   ├── TableDto.cs                 // Thông tin bàn trả về Client
│   │   │                              // - TableId
│   │   │                              // - TableName
│   │   │                              // - Capacity
│   │   │                              // - TableStatus
│   │   │                              // - CurrentOrderId (nếu có)
│   │   │
│   │   ├── CreateTableRequest.cs       // Dữ liệu yêu cầu tạo bàn
│   │   │                              // - TableName
│   │   │                              // - Capacity
│   │   │
│   │   └── UpdateTableRequest.cs       // Dữ liệu yêu cầu cập nhật bàn
│   │                                  // - TableName
│   │                                  // - Capacity
│   │                                  // - TableStatus
│   │
│   ├── Order/                          // DTO liên quan đến đơn hàng
│   │   │
│   │   ├── CreateOrderRequest.cs       // Dữ liệu Client gửi để tạo đơn
│   │   │                              // - TableId
│   │   │                              // - Items
│   │   │                              // - Ghi chú đơn hàng
│   │   │                              // - IdempotencyKey (nếu sử dụng)
│   │   │
│   │   ├── OrderDto.cs                 // Thông tin đơn hàng trả về Client
│   │   │                              // - OrderId
│   │   │                              // - TableId
│   │   │                              // - EmployeeId
│   │   │                              // - CreatedAt
│   │   │                              // - TotalAmount
│   │   │                              // - OrderStatus
│   │   │                              // - Danh sách món ăn
│   │   │
│   │   ├── OrderDetailDto.cs           // Thông tin từng món trong đơn
│   │   │                              // - MenuItemId
│   │   │                              // - MenuItemName
│   │   │                              // - Quantity
│   │   │                              // - UnitPrice
│   │   │                              // - Note
│   │   │                              // - ItemStatus
│   │   │
│   │   └── UpdateOrderStatusRequest.cs // Dữ liệu cập nhật trạng thái đơn
│   │                                  // - OrderStatus
│   │
│   ├── Menu/                           // DTO liên quan đến thực đơn
│   │   │
│   │   ├── MenuItemDto.cs              // Thông tin món ăn trả về Client
│   │   │                              // - MenuItemId
│   │   │                              // - Name
│   │   │                              // - Description
│   │   │                              // - Price
│   │   │                              // - CategoryId
│   │   │                              // - ImageUrl
│   │   │                              // - IsAvailable
│   │   │
│   │   ├── CreateMenuItemRequest.cs   // Dữ liệu thêm món ăn
│   │   │                              // - Name
│   │   │                              // - Description
│   │   │                              // - Price
│   │   │                              // - CategoryId
│   │   │                              // - ImageUrl
│   │   │
│   │   └── UpdateMenuItemRequest.cs   // Dữ liệu cập nhật món ăn
│   │                                  // - Name
│   │                                  // - Price
│   │                                  // - CategoryId
│   │                                  // - IsAvailable
│   │
│   └── Payment/                        // DTO liên quan đến thanh toán
│       │
│       ├── CreatePaymentRequest.cs     // Dữ liệu yêu cầu thanh toán
│       │                              // - OrderId
│       │                              // - PaymentMethod
│       │                              // - Amount
│       │
│       └── PaymentDto.cs               // Thông tin thanh toán trả về Client
│                                      // - PaymentId
│                                      // - OrderId
│                                      // - Amount
│                                      // - PaymentMethod
│                                      // - PaymentStatus
│                                      // - PaidAt
│
├── Enums/                             // Các kiểu dữ liệu liệt kê dùng chung
│   │
│   ├── OrderStatus.cs                 // Các trạng thái đơn hàng
│   │                                  // - Pending: Chờ xử lý
│   │                                  // - Preparing: Đang chế biến
│   │                                  // - Completed: Hoàn thành
│   │                                  // - Cancelled: Đã hủy
│   │
│   ├── TableStatus.cs                 // Các trạng thái bàn
│   │                                  // - Available: Bàn trống
│   │                                  // - Occupied: Đang sử dụng
│   │                                  // - Reserved: Đã đặt trước
│   │                                  // - Cleaning: Đang dọn dẹp
│   │
│   ├── UserRole.cs                    // Các vai trò người dùng
│   │                                  // - Admin
│   │                                  // - Manager
│   │                                  // - Cashier
│   │                                  // - Kitchen
│   │                                  // - Waiter
│   │
│   ├── PaymentMethod.cs               // Phương thức thanh toán
│   │                                  // - Cash
│   │                                  // - BankTransfer
│   │                                  // - Card
│   │
│   ├── PaymentStatus.cs               // Trạng thái thanh toán
│   │                                  // - Pending
│   │                                  // - Completed
│   │                                  // - Failed
│   │                                  // - Refunded
│   │
│   └── ItemStatus.cs                  // Trạng thái từng món ăn
│                                      // - Pending
│                                      // - Preparing
│                                      // - Completed
│                                      // - Cancelled
│
├── Messages/                          // Các cấu trúc dữ liệu SignalR
│   │                                  // Dùng để trao đổi sự kiện Real-time
│   │
│   ├── OrderCreatedMessage.cs         // Thông báo có đơn hàng mới
│   │                                  // - OrderId
│   │                                  // - TableId
│   │                                  // - CreatedAt
│   │                                  // - Items
│   │
│   ├── OrderUpdatedMessage.cs         // Thông báo đơn hàng được cập nhật
│   │                                  // - OrderId
│   │                                  // - UpdatedAt
│   │                                  // - Các thông tin thay đổi
│   │
│   ├── OrderStatusChangedMessage.cs   // Thông báo trạng thái đơn thay đổi
│   │                                  // - OrderId
│   │                                  // - OldStatus
│   │                                  // - NewStatus
│   │                                  // - ChangedAt
│   │
│   ├── TableStatusChangedMessage.cs   // Thông báo trạng thái bàn thay đổi
│   │                                  // - TableId
│   │                                  // - OldStatus
│   │                                  // - NewStatus
│   │
│   ├── PaymentCompletedMessage.cs     // Thông báo thanh toán thành công
│   │                                  // - PaymentId
│   │                                  // - OrderId
│   │                                  // - Amount
│   │                                  // - PaidAt
│   │
│   └── NotificationMessage.cs         // Cấu trúc thông báo chung
│                                      // - Title
│                                      // - Content
│                                      // - NotificationType
│                                      // - CreatedAt
│
├── Constants/                         // Các hằng số dùng chung
│   │
│   ├── ApiRoutes.cs                   // Khai báo đường dẫn REST API
│   │                                  // - Auth = /api/auth
│   │                                  // - Tables = /api/tables
│   │                                  // - Menu = /api/menu
│   │                                  // - Orders = /api/orders
│   │                                  // - Payments = /api/payments
│   │                                  // - Employees = /api/employees
│   │
│   └── HubEvents.cs                   // Khai báo tên sự kiện SignalR
│                                      // - OrderCreated
│                                      // - OrderUpdated
│                                      // - OrderStatusChanged
│                                      // - TableStatusChanged
│                                      // - PaymentCompleted
│                                      // - NewNotification