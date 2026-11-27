namespace Shared.Protocol;

// Giao thức dạng dòng trên TCP (UTF-8, kết thúc bằng '\n'):  LỆNH|tham số|...
public static class Protocol
{
    public const char Separator = '|';

    // ---- Client -> Server ----
    public const string Auth = "AUTH";          // AUTH|{jwt}
    public const string Ping = "PING";          // PING
    public const string Sync = "SYNC";          // SYNC  (xin lại toàn bộ bàn + order đang mở)
    public const string Order = "ORDER";        // ORDER|{orderId:N}|{CreateOrderRequest}
    public const string Item = "ITEM";          // ITEM|{requestId:N}|{ItemStatusChangedEvent}
    public const string Table = "TABLE";        // TABLE|{requestId:N}|{TableChangedEvent}

    // ---- Server -> Client ----
    public const string Pong = "PONG";
    public const string AuthOk = "AUTH_OK";         // AUTH_OK|{role}
    public const string AuthFail = "AUTH_FAIL";     // AUTH_FAIL|{ErrorResponse}
    public const string Ack = "ACK";                // ACK|{id:N}                  đã xử lý
    public const string Nak = "NAK";                // NAK|{id:N}|{ErrorResponse}  từ chối hẳn
    public const string Retry = "RETRY";            // RETRY|{id:N}                lỗi tạm thời, gửi lại sau
    public const string Error = "ERROR";            // ERROR|{ErrorResponse}
    public const string OrderNew = "ORDER_NEW";     // ORDER_NEW|{OrderDto}  (Items = các món VỪA gửi)
    public const string ItemStatus = "ITEM_STATUS"; // ITEM_STATUS|{ItemStatusChangedEvent}
    public const string TableStatus = "TABLE_STATUS"; // TABLE_STATUS|{TableChangedEvent}
    public const string Tables = "TABLES";          // TABLES|[TableDto]
    public const string Orders = "ORDERS";          // ORDERS|[OrderDto]
}