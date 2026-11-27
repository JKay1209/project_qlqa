using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Server.Config;
using Server.Network;
using Shared.DTOs.Auth;
using Shared.DTOs.Monitor;

namespace Server.Controllers;

// API giám sát Socket Server. Chỉ Chủ quán (Admin) và Quản lý được dùng.
[ApiController]
[Route("api/server")]
[Authorize(Roles = "Admin,Manager")]
public class ServerController(ClientRegistry registry, IOptions<SocketServerOptions> options) : ControllerBase
{
    // GET /api/server/status
    [HttpGet("status")]
    public ActionResult<SocketServerStatusDto> GetStatus()
    {
        var o = options.Value;
        return Ok(new SocketServerStatusDto(
            registry.IsListening,
            registry.Port,
            registry.StartedAtUtc,
            registry.Count,
            registry.TotalAccepted,
            o.ReuseAddress,
            o.KeepAlive,
            o.KeepAliveTimeSeconds,
            o.KeepAliveIntervalSeconds,
            o.KeepAliveRetryCount,
            o.NoDelay));
    }

    // GET /api/server/clients
    [HttpGet("clients")]
    public ActionResult<List<ConnectedClientDto>> GetClients()
        => Ok(registry.Snapshot().Select(ToDto).ToList());

    // DELETE /api/server/clients/{id}  — ngắt một client
    [HttpDelete("clients/{id:int}")]
    public ActionResult<MessageResponse> Disconnect(int id)
        => registry.Disconnect(id)
            ? Ok(new MessageResponse($"Đã ngắt client #{id}."))
            : NotFound(new ErrorResponse($"Không tìm thấy client #{id}."));

    private static ConnectedClientDto ToDto(ClientSession s)
        => new(s.Id, s.RemoteEndPoint, s.ConnectedAtUtc, s.LastActivityUtc, s.BytesReceived, s.BytesSent);
}