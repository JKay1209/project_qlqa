namespace QLQA.Protocol;

/// <summary>
/// Mã loại thông điệp (2 byte). Chia theo nhóm để dễ mở rộng:
/// 0x00xx hệ thống, 0x01xx xác thực, 0x02xx bàn, 0x03xx order, 0x04xx menu, 0x05xx thanh toán.
/// Quy ước: Request/Result đi theo cặp; Event là server chủ động đẩy xuống.
/// </summary>

public enum MessageType : ushort
{
    // ---- Hệ thống ----
    Connect = 0x0001,
    ConnectAck = 0x0002,
    Error = 0x0003,

    // ---- Xác thực ----
    LoginRequest = 0x0100,
    LoginResult = 0x0101,

    // space for add logout code

    // ---- Bàn ----
    GetTablesRequest = 0x0200,
    GetTablesResult = 0x0201,
    TableChangedEvent = 0x0202,

    // ---- Order ----
    CreateOrderRequest = 0x0300,
    CreateOrderResult = 0x0301,
    ItemStatusChangedEvent = 0x0302,

    // ---- Menu ----
    GetMenuRequest = 0x0400,
    GetMenuResult = 0x0401,

    // ---- Thanh toán ----
    PaymentRequest = 0x0500,
    PaymentResult = 0x0501
}
