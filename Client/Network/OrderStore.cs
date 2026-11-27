using System.Diagnostics;
using System.Text.Json;

namespace Client.Network;

// Một order chưa gửi được. Id sinh ở Client và gửi kèm lên Server
// để Server nhận ra order đã xử lý rồi (gửi bù không bị nhân đôi).
internal sealed record PendingOrder(Guid Id, DateTime CreatedAtUtc, string PayloadJson);

// Hàng đợi order chưa gửi, lưu ra file để tắt app / crash vẫn không mất.
// File: %LocalAppData%\QLQA\pending-orders.json
internal sealed class OrderStore
{
    private readonly object _lock = new();
    private readonly string _path;
    private readonly List<PendingOrder> _items;

    public OrderStore(string? path = null)
    {
        _path = path ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QLQA", "pending-orders.json");

        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        _items = Load();
    }

    public int Count
    {
        get { lock (_lock) return _items.Count; }
    }

    // Ghi xuống đĩa NGAY khi thêm, trước khi thử gửi (write-ahead).
    public PendingOrder Add(string payloadJson)
    {
        var order = new PendingOrder(Guid.NewGuid(), DateTime.UtcNow, payloadJson);
        lock (_lock)
        {
            _items.Add(order);
            Save();
        }
        return order;
    }

    public bool Contains(Guid id)
    {
        lock (_lock) return _items.Any(o => o.Id == id);
    }

    // Bản chụp theo thứ tự cũ -> mới, để gửi bù đúng thứ tự.
    public IReadOnlyList<PendingOrder> Snapshot()
    {
        lock (_lock) return _items.ToList();
    }

    public void Remove(Guid id)
    {
        lock (_lock)
        {
            if (_items.RemoveAll(o => o.Id == id) > 0)
                Save();
        }
    }

    // ===================== File =====================
    private List<PendingOrder> Load()
    {
        if (!File.Exists(_path)) return [];

        try
        {
            var items = JsonSerializer.Deserialize<List<PendingOrder>>(File.ReadAllText(_path));
            return items ?? [];
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            // File hỏng: giữ lại bản sao để cứu dữ liệu thay vì xóa im lặng
            Debug.WriteLine($"[OrderStore] File hỏng, chuyển sang .corrupt: {ex.Message}");
            try { File.Move(_path, _path + ".corrupt", overwrite: true); } catch { }
            return [];
        }
    }

    // Ghi file tạm rồi đổi tên -> mất điện giữa chừng cũng không làm hỏng file chính.
    private void Save()
    {
        string tmp = _path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(_items));
        File.Move(tmp, _path, overwrite: true);
    }
}