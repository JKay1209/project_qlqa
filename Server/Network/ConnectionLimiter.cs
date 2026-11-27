using System.Net;

namespace Server.Network;

// Giới hạn số kết nối: toàn Server (SemaphoreSlim) và trên mỗi IP.
// Mỗi lần TryAcquire thành công phải có đúng một lần Release khi đóng kết nối.
public sealed class ConnectionLimiter(int maxClients, int maxClientsPerIp)
{
    private readonly SemaphoreSlim _slots = new(maxClients, maxClients);
    private readonly Dictionary<IPAddress, int> _perIp = new();
    private readonly object _lock = new();

    // Không bao giờ chờ: hết chỗ thì từ chối ngay để vòng select không bị chặn.
    public bool TryAcquire(IPAddress ip, out string? reason)
    {
        lock (_lock)
        {
            int current = _perIp.GetValueOrDefault(ip);
            if (current >= maxClientsPerIp)
            {
                reason = $"Máy này đã mở tối đa {maxClientsPerIp} kết nối.";
                return false;
            }

            if (!_slots.Wait(0))
            {
                reason = $"Server đã đủ {maxClients} kết nối.";
                return false;
            }

            _perIp[ip] = current + 1;
            reason = null;
            return true;
        }
    }

    public void Release(IPAddress ip)
    {
        lock (_lock)
        {
            if (!_perIp.TryGetValue(ip, out int current)) return;

            if (current <= 1) _perIp.Remove(ip);  
            else _perIp[ip] = current - 1;

            _slots.Release();
        }
    }
}