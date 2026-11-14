# 🍽️ project_qlqa – Hệ thống quản lý nhà hàng

Ứng dụng quản lý nhà hàng theo mô hình **Client – Server**: ứng dụng desktop Windows Forms cho nhân viên (phục vụ, thu ngân, bếp, quản lý) kết nối tới một Web API xây dựng bằng ASP.NET Core.

> **Trạng thái:** đang phát triển. Hiện đã hoàn thiện phần **xác thực tài khoản** (đăng nhập, đăng ký, quên mật khẩu qua OTP email). Các module bàn, thực đơn, đơn hàng, thanh toán, bếp và SignalR mới ở mức khung (file đã tạo, chưa có nội dung).

---

## Mục lục

- [Tính năng](#tính-năng)
- [Công nghệ](#công-nghệ)
- [Cấu trúc dự án](#cấu-trúc-dự-án)
- [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
- [Cài đặt và chạy](#cài-đặt-và-chạy)
- [Cấu hình](#cấu-hình)
- [API](#api)
- [Phân quyền](#phân-quyền)
- [Dữ liệu mẫu](#dữ-liệu-mẫu)
- [Tiến độ](#tiến-độ)
- [Lưu ý bảo mật](#lưu-ý-bảo-mật)

---

## Tính năng

**Đã có**

- Đăng nhập, trả về JWT (hết hạn sau 480 phút, cấu hình được).
- Đăng ký tài khoản. Người dùng công khai luôn được cấp vai trò `Waiter`; chỉ `Admin` đã đăng nhập mới chọn được vai trò khác.
- Quên mật khẩu: gửi mã OTP qua email → xác thực OTP → đặt lại mật khẩu (giới hạn số lần nhập sai).
- Lấy thông tin người dùng hiện tại (`/api/auth/me`).
- Băm mật khẩu, middleware bắt lỗi tập trung.
- Tự động seed tài khoản admin, 10 bàn, và thực đơn mẫu khi chạy lần đầu.
- TCP socket server nền (cổng `9000`) – bản thử nghiệm: `PING` → `PONG`, còn lại trả `ECHO: <nội dung>`.

**Dự kiến** (đã có khung thư mục, xem file `structure_*.md`)

- Quản lý bàn: mở, gộp, chuyển bàn.
- Quản lý thực đơn và danh mục.
- Gọi món, gửi đơn xuống bếp, theo dõi trạng thái món.
- Thanh toán (tiền mặt / thẻ / chuyển khoản).
- Màn hình bếp và thông báo thời gian thực qua SignalR.
- Quản lý nhân viên.

## Công nghệ

| Thành phần | Công nghệ |
|---|---|
| Ngôn ngữ / Runtime | C#, **.NET 10** |
| Server | ASP.NET Core Web API, Entity Framework Core 10 (SQL Server) |
| Xác thực | JWT Bearer |
| Real-time | SignalR (đã đăng ký, chưa dùng), TCP Socket |
| Client | Windows Forms (`net10.0-windows`) |
| Email | SMTP (mặc định Gmail) |
| Cơ sở dữ liệu | Microsoft SQL Server / SQL Server Express |

## Cấu trúc dự án

```
project_qlqa-master/
├── Client/                 # Ứng dụng WinForms
│   ├── Layout/             # Các form giao diện (Login, Register, ForgotPassword, VerifyOtp, ResetPassword, Main...)
│   ├── LayoutLogics/       # Logic cho từng màn hình (AuthLogic, ...)
│   ├── Network/            # HttpClientManager, ApiException, ConnectionMonitor, HubConnectionManager
│   ├── Security/           # SessionManager, TokenManager
│   ├── Services/           # ApiService, AuthService, SignalRService, NotificationService
│   ├── Helpers/            # DialogHelper, ValidationHelper
│   └── structure_client.md # Thiết kế cấu trúc phía Client
│
├── Server/                 # ASP.NET Core Web API
│   ├── Config/             # Database, JWT, Email options
│   ├── Controllers/        # AuthController (đã làm) + Table/Menu/Order/Payment/Employee (khung)
│   ├── Hubs/               # KitchenHub, NotificationHub, RestaurantHub (khung)
│   ├── Middleware/         # ExceptionMiddleware, LoggingMiddleware
│   ├── Migrations/         # EF Core migrations
│   ├── Models/             # Database (DbContext, DbSeeder) và Entities
│   ├── Network/            # TcpSocketServer
│   ├── Repositories/       # EmployeeRepository (+ khung các repository khác)
│   ├── Security/           # JwtService, PasswordHasher, PermissionService
│   ├── Services/           # AuthService, EmailService (+ khung các service khác)
│   ├── appsettings.json
│   └── structure_server.md # Thiết kế cấu trúc phía Server
│
├── Shared/                 # Thư viện dùng chung Client ↔ Server
│   ├── Constants/          # ApiRoutes, HubEvents
│   ├── DTOs/               # Auth, Menu, Order, Payment, Table
│   ├── Enums/              # UserRole, OrderStatus, ItemStatus, TableStatus, PaymentMethod, ErrorCode
│   ├── Serialization/      # JsonDefaults
│   └── structure_shared.md
│
└── project_qlqa.slnx       # Solution
```

## Yêu cầu hệ thống

- Windows 10/11 (Client dùng Windows Forms)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ (khuyến nghị, hỗ trợ file `.slnx`) hoặc `dotnet` CLI
- SQL Server hoặc SQL Server Express
- Tài khoản email có App Password nếu muốn dùng chức năng quên mật khẩu (OTP)

## Cài đặt và chạy

### 1. Lấy mã nguồn

```bash
git clone <url-repo>
cd project_qlqa-master
```

### 2. Cấu hình Server

Mở `Server/appsettings.json` và sửa các mục sau (xem chi tiết ở phần [Cấu hình](#cấu-hình)):

- `ConnectionStrings:Default` – trỏ tới SQL Server của bạn.
- `Jwt:Key` – đổi sang khóa bí mật riêng (tối thiểu 32 ký tự).
- `Email:*` – thông tin SMTP nếu dùng OTP.

### 3. Tạo cơ sở dữ liệu

```bash
cd Server
dotnet ef database update
```

> Nếu chưa có công cụ EF: `dotnet tool install --global dotnet-ef`

### 4. Chạy Server

```bash
dotnet run --project Server
```

Mặc định server lắng nghe tại:

- `https://localhost:51279`
- `http://localhost:51280`
- TCP socket: cổng `9000`

Lần chạy đầu tiên, server tự tạo dữ liệu mẫu (xem [Dữ liệu mẫu](#dữ-liệu-mẫu)).

### 5. Cấu hình địa chỉ Server cho Client

Mở `Client/Network/HttpClientManager.cs` và đổi `BaseAddress` cho khớp với server của bạn. Hiện đang là một địa chỉ ngrok:

```csharp
BaseAddress = new Uri("https://reawake-judiciary-thread.ngrok-free.dev/")
```

Khi chạy local, đổi thành `https://localhost:51279/` (hoặc `http://localhost:51280/`).

### 6. Chạy Client

```bash
dotnet run --project Client
```

Hoặc trong Visual Studio: chọn nhiều startup project (`Server` và `Client`) rồi nhấn **F5**. Repo có sẵn file `project_qlqa.slnLaunch.user` cho cấu hình này.

## Cấu hình

`Server/appsettings.json`:

| Khóa | Mô tả |
|---|---|
| `ConnectionStrings:Default` | Chuỗi kết nối SQL Server |
| `Jwt:Key` | Khóa ký JWT (≥ 32 ký tự) |
| `Jwt:Issuer` / `Jwt:Audience` | Mặc định `Server` / `Client` |
| `Jwt:ExpiresMinutes` | Thời gian sống của token (mặc định `480`) |
| `Seed:AdminPassword` | Mật khẩu tài khoản admin mặc định |
| `Seed:AdminEmail` | Email của tài khoản admin mặc định |
| `Email:Host` / `Port` / `EnableSsl` | Máy chủ SMTP (mặc định `smtp.gmail.com:587`) |
| `Email:User` / `Password` / `FromAddress` / `FromName` | Thông tin tài khoản gửi mail |

## API

Các route được khai báo tại `Shared/Constants/ApiRoutes.cs`.

| Method | Endpoint | Quyền | Mô tả |
|---|---|---|---|
| POST | `/api/auth/login` | Công khai | Đăng nhập, nhận JWT |
| POST | `/api/auth/register` | Công khai (Admin mới chọn được vai trò) | Đăng ký tài khoản |
| GET | `/api/auth/me` | Đã đăng nhập | Thông tin người dùng hiện tại |
| POST | `/api/auth/forgot-password` | Công khai | Gửi OTP về email |
| POST | `/api/auth/verify-otp` | Công khai | Xác thực OTP |
| POST | `/api/auth/reset-password` | Công khai | Đặt lại mật khẩu |

Các nhóm route `/api/tables`, `/api/menu`, `/api/orders`, `/api/payments`, `/api/employees` đã được định nghĩa nhưng controller chưa triển khai.

Gọi API có xác thực bằng header:

```
Authorization: Bearer <token>
```

Enum được serialize dạng chuỗi (ví dụ `"Waiter"`) ở cả Server lẫn Client.

## Phân quyền

| Vai trò | RoleId |
|---|---|
| Admin | 1 |
| Manager | 2 |
| Cashier | 3 |
| Kitchen | 4 |
| Waiter | 5 |

Giá trị trùng với bảng `Roles` trong cơ sở dữ liệu.

## Dữ liệu mẫu

Khi bảng còn trống, server tự seed:

- **Tài khoản:** `admin` / mật khẩu lấy từ `Seed:AdminPassword` (mặc định `Admin@123`). **Hãy đổi ngay sau lần đăng nhập đầu tiên.**
- **Bàn:** 10 bàn (`Bàn 01` – `Bàn 10`), bàn 1–6 có 4 chỗ, bàn 7–10 có 6 chỗ.
- **Thực đơn:** 4 danh mục (Khai vị, Món chính, Đồ uống, Tráng miệng) với các món mẫu.

## Tiến độ

- [x] Cấu trúc solution Client / Server / Shared
- [x] Cơ sở dữ liệu và migration (users, roles, tables, menu, orders, payments, password reset)
- [x] Đăng nhập, đăng ký, JWT
- [x] Quên mật khẩu bằng OTP email
- [x] TCP socket server (bản thử nghiệm)
- [ ] Quản lý bàn
- [ ] Quản lý thực đơn
- [ ] Gọi món và đơn hàng
- [ ] Thanh toán
- [ ] Màn hình bếp
- [ ] Thông báo real-time (SignalR)
- [ ] Quản lý nhân viên và phân quyền chi tiết

## Lưu ý bảo mật

Trước khi đưa repo lên nơi công khai hoặc triển khai thật:

- `Server/appsettings.json` hiện chứa **JWT key** và **mật khẩu admin mặc định** viết thẳng trong file. Hãy đổi khóa, và chuyển các giá trị nhạy cảm sang biến môi trường hoặc [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (`dotnet user-secrets`).
- Không commit mật khẩu SMTP; dùng App Password của Gmail thay vì mật khẩu thật.
- Đổi mật khẩu `admin` ngay lần đầu đăng nhập.

## Ghi chú cho người đóng góp

- File `.gitignore` đang còn dấu xung đột merge (`<<<<<<<`, `=======`, `>>>>>>>`), nên cần dọn lại để `bin/` và `obj/` được bỏ qua đúng cách.
- Các file `*.resx` của form rất nặng (hàng MB do nhúng ảnh nền); nên cân nhắc nén hoặc đưa ảnh ra ngoài.
- Thiết kế chi tiết từng tầng nằm trong `Client/structure_client.md`, `Server/structure_server.md`, `Shared/structure_shared.md`.
