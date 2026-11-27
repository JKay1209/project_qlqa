namespace Server.Network.Routing;

// Ghi nhớ orderId đã xử lý để order không bị nhân đôi.
// Lưu ra file nên Server khởi động lại vẫn nhớ. Chỉ worker định tuyến gọi (1 luồng).
public sealed class ProcessedOrderLog
{
    private readonly string _path;
    private readonly HashSet<Guid> _ids = [];

    public ProcessedOrderLog(string path)
    {
        _path = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

        if (!File.Exists(_path)) return;
        foreach (var line in File.ReadLines(_path))
            if (Guid.TryParseExact(line, "N", out var id))
                _ids.Add(id);
    }

    public bool Contains(Guid id) => _ids.Contains(id);

    public void Add(Guid id)
    {
        if (_ids.Add(id))
            File.AppendAllText(_path, id.ToString("N") + "\n");
    }
}