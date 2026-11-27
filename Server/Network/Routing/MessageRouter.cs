using System.Text.Json;
using Shared.DTOs.Auth;
using Shared.DTOs.Order;
using Shared.DTOs.Table;
using Shared.Protocol;
using Shared.Serialization;

namespace Server.Network.Routing;

// Định tuyến: 1 dòng từ Client -> kiểm tra quyền -> nghiệp vụ -> danh sách dòng cần gửi đi.
// Được gọi TUẦN TỰ bởi một worker duy nhất, nên hai yêu cầu không bao giờ sửa dữ liệu chồng nhau.
public sealed class MessageRouter(BackendRouting backend, ProcessedOrderLog processed, ILogger<MessageRouter> logger)
{
    public async Task RouteAsync(ClientSession session, string line, Action<Outbound> emit, CancellationToken ct)
    {
        string[] parts = line.Split(Protocol.Separator, 3);
        string command = parts[0];

        void Reply(string text) => emit(new Outbound(text, TargetId: session.Id));

        if (command == Protocol.Auth)
        {
            await AuthenticateAsync(session, line[command.Length..].TrimStart(Protocol.Separator), Reply, ct);
            return;
        }

        // ORDER / ITEM / TABLE luôn có id để Client ghép phản hồi.
        Guid id = default;
        bool hasId = parts.Length == 3 && Guid.TryParseExact(parts[1], "N", out id);

        var user = session.User;
        if (user is not null && !backend.IsStillValid(user))
            session.User = user = null;

        if (user is null)
        {
            // Chưa đăng nhập: không NAK order (Client phải giữ lại, đăng nhập rồi gửi tiếp)
            if (hasId) Reply($"{Protocol.Retry}|{id:N}");
            Reply(Failure(Protocol.AuthFail, "Chưa đăng nhập hoặc phiên đã hết hạn."));
            return;
        }

        if (command == Protocol.Sync)
        {
            await SendSnapshotAsync(Reply, ct);
            return;
        }

        if (command is not (Protocol.Order or Protocol.Item or Protocol.Table) || !hasId)
        {
            Reply(Failure(Protocol.Error, "Lệnh không hợp lệ."));
            return;
        }

        try
        {
            switch (command)
            {
                case Protocol.Order: await OrderAsync(user, id, parts[2], emit, Reply, ct); break;
                case Protocol.Item: await ItemAsync(user, id, parts[2], emit, Reply, ct); break;
                case Protocol.Table: await TableAsync(user, id, parts[2], emit, Reply, ct); break;
            }
        }
        catch (RouteRejectedException ex)
        {
            Reply(Nak(id, ex.Message));
        }
        catch (JsonException)
        {
            Reply(Nak(id, "Dữ liệu không hợp lệ."));
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "Lỗi xử lý {Command} từ client #{Id}", command, session.Id);
            Reply($"{Protocol.Retry}|{id:N}");
        }
    }

    public static Outbound TableStatusMessage(TableChangedEvent e)
        => new(Message(Protocol.TableStatus, e), Audience: RoutingRules.ReceivesTableStatus);

    private async Task AuthenticateAsync(ClientSession session, string token, Action<string> reply, CancellationToken ct)
    {
        session.User = null;   // xác thực lại (kể cả thất bại) thì phiên cũ hết hiệu lực
        try
        {
            var user = await backend.AuthenticateAsync(token, ct);
            if (user is null || !RoutingRules.IsStaff(user.Role))
            {
                reply(Failure(Protocol.AuthFail, "Token không hợp lệ hoặc không có quyền."));
                return;
            }

            session.User = user;
            reply($"{Protocol.AuthOk}|{user.Role}");
            logger.LogInformation("Client #{Id} xác thực: user {UserId} ({Role})", session.Id, user.UserId, user.Role);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "Lỗi xác thực client #{Id}", session.Id);
            reply(Failure(Protocol.Error, "Lỗi hệ thống khi xác thực."));
            session.Disconnect();   // Client tự nối lại và xác thực lại
            return;
        }

        // Gửi ảnh chụp hiện tại để Client vừa vào / vừa nối lại không bị thiếu sự kiện
        await SendSnapshotAsync(reply, ct);
    }

    private async Task SendSnapshotAsync(Action<string> reply, CancellationToken ct)
    {
        try
        {
            var s = await backend.GetSnapshotAsync(ct);
            reply(Message(Protocol.Tables, s.Tables));
            reply(Message(Protocol.Orders, s.Orders));
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "Không đọc được danh sách bàn/order.");
            reply(Failure(Protocol.Error, "Chưa tải được danh sách bàn, hãy thử lại."));
        }
    }

    private async Task OrderAsync(SessionUser user, Guid id, string json,
        Action<Outbound> emit, Action<string> reply, CancellationToken ct)
    {
        if (!RoutingRules.CanCreateOrder(user.Role))
            throw new RouteRejectedException("Bạn không có quyền gửi order.");

        // Đã xử lý rồi (Client không nhận được ACK nên gửi lại): chỉ ACK
        if (processed.Contains(id))
        {
            reply($"{Protocol.Ack}|{id:N}");
            return;
        }

        var request = Read<CreateOrderRequest>(json);
        if (request.Items is not { Count: > 0 and <= 100 })
            throw new RouteRejectedException("Order phải có từ 1 đến 100 món.");
        if (request.Items.Any(i => i is null || i.Quantity is < 1 or > 100))
            throw new RouteRejectedException("Số lượng mỗi món phải từ 1 đến 100.");
        if (request.Items.Any(i => i.Note?.Length > 300))
            throw new RouteRejectedException("Ghi chú tối đa 300 ký tự.");

        var result = await backend.CreateOrderAsync(user, request, ct);

        try { processed.Add(id); }
        catch (IOException ex) { logger.LogError(ex, "Không ghi được nhật ký order {Id}", id); }

        reply($"{Protocol.Ack}|{id:N}");
        emit(new Outbound(Message(Protocol.OrderNew, result.NewItems), Audience: RoutingRules.ReceivesNewOrder));
        if (result.TableChange is not null)
            emit(TableStatusMessage(result.TableChange));
    }

    private async Task ItemAsync(SessionUser user, Guid id, string json,
        Action<Outbound> emit, Action<string> reply, CancellationToken ct)
    {
        var change = Read<ItemStatusChangedEvent>(json);
        if (!Enum.IsDefined(change.NewStatus))
            throw new RouteRejectedException("Trạng thái món không hợp lệ.");

        var result = await backend.ChangeItemStatusAsync(user, change, ct);

        reply($"{Protocol.Ack}|{id:N}");
        emit(new Outbound(Message(Protocol.ItemStatus, result.Event), Audience: RoutingRules.ReceivesItemStatus));
    }

    private async Task TableAsync(SessionUser user, Guid id, string json,
        Action<Outbound> emit, Action<string> reply, CancellationToken ct)
    {
        var change = Read<TableChangedEvent>(json);
        if (!Enum.IsDefined(change.NewStatus))
            throw new RouteRejectedException("Trạng thái bàn không hợp lệ.");

        var result = await backend.ChangeTableStatusAsync(user, change, ct);

        reply($"{Protocol.Ack}|{id:N}");
        emit(TableStatusMessage(result));
    }

    private static T Read<T>(string json) where T : class
        => JsonSerializer.Deserialize<T>(json, JsonDefaults.Options)
           ?? throw new RouteRejectedException("Dữ liệu không hợp lệ.");

    private static string Message<T>(string command, T body)
        => $"{command}|{JsonSerializer.Serialize(body, JsonDefaults.Options)}";

    private static string Failure(string command, string message)
        => Message(command, new ErrorResponse(message));

    private static string Nak(Guid id, string message)
        => $"{Protocol.Nak}|{id:N}|{JsonSerializer.Serialize(new ErrorResponse(message), JsonDefaults.Options)}";
}