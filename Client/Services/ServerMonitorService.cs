using Shared.Constants;
using Shared.DTOs.Auth;
using Shared.DTOs.Monitor;

namespace Client.Services
{
    // Gọi API giám sát Socket Server (chỉ Admin/Manager).
    internal class ServerMonitorService : ApiService
    {
        public Task<SocketServerStatusDto> GetStatusAsync(CancellationToken ct = default)
            => GetAsync<SocketServerStatusDto>(ApiRoutes.SocketServerStatus, ct);

        public Task<List<ConnectedClientDto>> GetClientsAsync(CancellationToken ct = default)
            => GetAsync<List<ConnectedClientDto>>(ApiRoutes.SocketServerClients, ct);

        public Task<MessageResponse> DisconnectClientAsync(int clientId, CancellationToken ct = default)
            => DeleteAsync($"{ApiRoutes.SocketServerClients}/{clientId}", ct);
    }
}