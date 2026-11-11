# Kiến trúc TCP của dự án Quản lý quán ăn

Client (WinForms) và Server nói chuyện qua **một kết nối TCP giữ mở**, dùng giao thức gói tin tự định nghĩa.
Phần nghiệp vụ và dữ liệu (Service, Repository, EF Core) giữ nguyên, chỉ thay "cửa vào" từ HTTP Controller sang TCP.

```
Client (WinForms)                                   Server (ASP.NET host)
─────────────────                                   ─────────────────────────────────────────────
Layout (Form)                                       TcpSocketServer   socket→bind→listen→accept
   │                                                    │ mỗi client 1 Task
LayoutLogic (AuthLogic)                                 ▼
   │                                                ClientSession     đọc gói → trả lời, giữ phiên đăng nhập
Services (AuthService)                                  │
   │ RequestAsync(Command, body)                        ▼
TcpConnection  ══ TCP, gói tin Header+JSON ══════►  PacketDispatcher  Command → hàm xử lý
   ▲ ReadLoop: ghép Response theo RequestId             │ scope riêng cho mỗi yêu cầu
   └ PacketReceived: gói Server đẩy xuống               ▼
                                                    Service (AuthService…) → Repository → DbContext → SQL Server
                                                    ClientRegistry   danh sách client, broadcast theo vai trò
```

## 1. Shared/Protocol (hai bên dùng chung)

| File | Vai trò |
|---|---|
| `CommandType.cs` | Enum kiểu lệnh (2 byte): Ping/Pong/Response, Login/Register/Me/Logout, ForgotPassword/VerifyOtp/ResetPassword. Thêm lệnh mới ở đây. |
| `Packet.cs` | Một gói: `Command`, `RequestId`, `Payload` (JSON UTF-8). `Create<T>()`, `ReadBody<T>()`. |
| `PacketCodec.cs` | Ghi/đọc gói trên stream. Đọc đủ byte, kiểm tra Magic/Version/lệnh/kích thước. |
| `PacketException.cs` | Gói sai định dạng, Server sẽ ngắt kết nối. |
| `SocketResponse.cs` | Body của gói Response: `Ok`, `Code` (ErrorCode), `Message`, `Data`. Thay cho mã HTTP + ErrorResponse. |
| `Enums/ErrorCode.cs` | Thêm `Forbidden = 7`. |

### Định dạng gói tin (Header 13 byte, big-endian)

| Byte | Trường | Giá trị |
|---|---|---|
| 0–1 | Magic | `0x514C` ("QL") |
| 2 | Version | `1` |
| 3–4 | Command | `CommandType` |
| 5–8 | RequestId | Client đặt, Server chép lại vào Response. `0` = Server tự đẩy |
| 9–12 | Length | Độ dài Payload, tối đa `MaxPayloadBytes` (1 MB) |
| 13… | Payload | JSON UTF-8 (camelCase, enum dạng chữ, dùng `JsonDefaults`) |

Lý do cần Header: TCP là luồng byte, không giữ ranh giới giữa các gói. Length cho biết đọc bao nhiêu byte, Magic/Version phát hiện luồng lệch hoặc client lạ.

## 2. Server/Network

| File | Vai trò |
|---|---|
| `TcpSocketServer.cs` | Task 05. `BackgroundService` làm đủ 4 bước socket/bind/listen/accept, mỗi client giao cho một `ClientSession`. Cổng/backlog đọc từ cấu hình. |
| `SocketServerOptions.cs` | Mục `Socket` trong appsettings: `Port` 9000, `Backlog` 10, `MaxPayloadBytes` 1 MB. |
| `ClientSession.cs` | Một kết nối: vòng lặp đọc gói → `PacketDispatcher` → ghi Response. Giữ `User` (phiên đăng nhập). Có khóa ghi để nhiều luồng cùng gửi không lẫn byte. Mọi lỗi của client này bị cô lập tại đây. |
| `PacketDispatcher.cs` | "Controller" của TCP. Chọn hàm theo `Command`, tạo scope DI cho mỗi yêu cầu, gọi `AuthService`, đóng gói `SocketResponse`. Kiểm tra quyền (Register chỉ Admin) bằng phiên trên kết nối. |
| `ClientRegistry.cs` | Danh sách client đang kết nối. `BroadcastAsync` / `BroadcastToRolesAsync` để đẩy realtime (Task 11, 12). |
| `SessionUser.cs` | UserId, Username, Role của người đang đăng nhập trên kết nối. |
| `Server/Validation/AuthValidator.cs` | Quy tắc kiểm tra đầu vào (username, mật khẩu mạnh, email), tách riêng từ `AuthController`. |
| `Program.cs`, `appsettings.json` | Đăng ký `SocketServerOptions`, `ClientRegistry`, `PacketDispatcher`, `TcpSocketServer`. |

Giữ nguyên: `AuthService`, `EmployeeRepository`, `PasswordHasher`, `JwtService`, `EmailService`, `RestaurantDbContext`.

## 3. Client

| File | Vai trò |
|---|---|
| `Network/TcpConnection.cs` | Một kết nối dùng chung (`Instance`). Tự kết nối ở yêu cầu đầu. Mỗi yêu cầu có `RequestId` và chờ Response tương ứng, timeout 10 s. Vòng đọc nền ghép Response, gói Server đẩy xuống phát qua `PacketReceived`, mất kết nối phát `Disconnected`. |
| `Services/AuthService.cs` | Viết lại qua TCP (trước đây chỉ Login qua HTTP, còn lại là hàm rỗng). Lỗi Server báo được đổi thành `ApiException` với mã HTTP tương ứng nên **`AuthLogic` và các Form không phải sửa**. |
| `Network/ApiException.cs` | Giữ nguyên. |
| `Network/HttpClientManager.cs`, `Services/ApiService.cs` | Không còn được dùng cho Auth, có thể xóa khi chắc chắn không dùng REST nữa. |

Địa chỉ Server nằm ở `TcpConnection.Host/Port` (mặc định `127.0.0.1:9000`). Task 14 (UDP discovery) sẽ gán hai giá trị này.

## 4. Luồng ví dụ: đăng nhập

1. `main.cs` → `AuthLogic.LoginAsync` → `AuthService.LoginAsync`.
2. `TcpConnection` kết nối (nếu chưa), gửi gói `Login` (RequestId = 1, Payload = `{"username":…,"password":…}`).
3. `ClientSession` đọc gói, `PacketDispatcher` tạo scope, gọi `AuthService.LoginAsync` của Server.
4. Thành công: `session.User` được gán, Server trả `Response` (cùng RequestId) chứa `LoginResponse`. Sai: `Response` với `Ok=false`, `Code=InvalidCredentials`.
5. `ReadLoop` phía Client ghép Response theo RequestId, `AuthService` trả `LoginResponse` hoặc ném `ApiException(401)`.

## 5. Còn lại theo timeline (đã có chỗ để cắm)

| Task | Gắn vào đâu |
|---|---|
| 8 Socket Options, đa client | `TcpSocketServer` (SetSocketOption), `ClientSession` |
| 9 Reconnect, lưu order chưa gửi | `TcpConnection` (`Disconnected`, hàng đợi gửi bù). Reconnect phải đăng nhập lại vì phiên nằm trên kết nối |
| 10 Phân quyền | `PacketDispatcher` (kiểm `session.User.Role` trước mỗi lệnh) |
| 11, 12 Định tuyến order, broadcast bàn | `ClientRegistry.BroadcastToRolesAsync`, `TcpConnection.PacketReceived` + `Control.Invoke` |
| 13 AES | `PacketCodec` (mã hóa Payload sau khi serialize, giải mã trước khi đọc), thêm cờ vào Header |
| 17 Giới hạn kết nối | `TcpSocketServer` (đếm qua `ClientRegistry`, từ chối ở accept) |
| 18 Kiểm tra gói | Đã có: Magic, Version, lệnh, kích thước tối đa, ngắt kết nối khi sai |
| 19 Heartbeat | `Ping/Pong` đã có lệnh; thêm bộ đếm thời gian trong `ClientSession` |
| 26, 27 Rate limit, chống sập | `ClientSession`/`TcpSocketServer`. Cô lập lỗi từng client đã có |

## 6. Cần biết

- Server hiện vẫn mở REST (`AuthController`) song song. Có thể giữ để test bằng Postman hoặc xóa khi nhóm chốt chỉ dùng TCP.
- `AuthController` và `AuthValidator` đang trùng logic kiểm tra. Nên cho Controller gọi `AuthValidator` để chỉ còn một nơi.
- Mỗi client xử lý tuần tự từng yêu cầu. Đủ cho giờ, Task 8 có thể chuyển sang xử lý song song.
- `MainForm.btnLogout_Click` chỉ xóa phiên phía Client. Nên gọi thêm `AuthService.LogoutAsync()` để Server xóa phiên trên kết nối (Server đã tự xóa phiên cũ khi có Login mới).
- Chưa build được trong môi trường của mình (không có .NET SDK), cần build và chạy thử trên máy bạn.
