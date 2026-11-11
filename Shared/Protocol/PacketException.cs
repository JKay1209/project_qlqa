namespace Shared.Protocol;

/// <summary>Gói tin sai định dạng (sai Magic/Version/lệnh, quá lớn...). Luồng đã lệch nên phải đóng kết nối.</summary>
public class PacketException(string message) : Exception(message);
