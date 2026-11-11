using System.Buffers.Binary;

namespace Shared.Protocol;

/// <summary>
/// Đóng/mở gói tin trên luồng TCP (TCP là luồng byte, không có ranh giới gói nên phải tự đóng khung).
///
/// Header 13 byte, big-endian:
///   [0-1]  Magic      0x514C ("QL")
///   [2]    Version    1
///   [3-4]  Command    CommandType
///   [5-8]  RequestId  int
///   [9-12] Length     độ dài Payload (byte)
/// Theo sau là Payload.
/// </summary>
public static class PacketCodec
{
    public const ushort Magic = 0x514C;
    public const byte Version = 1;
    public const int HeaderSize = 13;
    public const int DefaultMaxPayload = 1024 * 1024;   // 1 MB

    public static async Task WriteAsync(Stream stream, Packet packet, CancellationToken ct = default)
    {
        var buf = new byte[HeaderSize + packet.Payload.Length];
        BinaryPrimitives.WriteUInt16BigEndian(buf.AsSpan(0), Magic);
        buf[2] = Version;
        BinaryPrimitives.WriteUInt16BigEndian(buf.AsSpan(3), (ushort)packet.Command);
        BinaryPrimitives.WriteInt32BigEndian(buf.AsSpan(5), packet.RequestId);
        BinaryPrimitives.WriteInt32BigEndian(buf.AsSpan(9), packet.Payload.Length);
        packet.Payload.CopyTo(buf, HeaderSize);

        await stream.WriteAsync(buf, ct);   // 1 lần ghi cả Header + Payload
    }

    /// <summary>Trả null nếu đối phương đóng kết nối đúng ranh giới gói. Ném PacketException nếu gói sai.</summary>
    public static async Task<Packet?> ReadAsync(Stream stream, int maxPayload = DefaultMaxPayload, CancellationToken ct = default)
    {
        var header = new byte[HeaderSize];
        if (!await ReadExactAsync(stream, header, ct, allowEofAtStart: true))
            return null;

        if (BinaryPrimitives.ReadUInt16BigEndian(header.AsSpan(0)) != Magic)
            throw new PacketException("Sai Magic Number.");
        if (header[2] != Version)
            throw new PacketException($"Sai phiên bản giao thức: {header[2]}.");

        var command = (CommandType)BinaryPrimitives.ReadUInt16BigEndian(header.AsSpan(3));
        if (!Enum.IsDefined(command))
            throw new PacketException($"Lệnh không hợp lệ: {(ushort)command}.");

        int requestId = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(5));
        int length = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(9));
        if (length < 0 || length > maxPayload)
            throw new PacketException($"Kích thước gói không hợp lệ: {length} byte (tối đa {maxPayload}).");

        var payload = new byte[length];
        if (length > 0)
            await ReadExactAsync(stream, payload, ct, allowEofAtStart: false);

        return new Packet(command, requestId, payload);
    }

    // Đọc đủ đúng buffer.Length byte (một lần Read có thể chỉ trả về một phần).
    private static async Task<bool> ReadExactAsync(Stream stream, byte[] buffer, CancellationToken ct, bool allowEofAtStart)
    {
        int read = 0;
        while (read < buffer.Length)
        {
            int n = await stream.ReadAsync(buffer.AsMemory(read), ct);
            if (n == 0)
            {
                if (read == 0 && allowEofAtStart) return false;
                throw new EndOfStreamException("Kết nối bị đóng giữa chừng một gói tin.");
            }
            read += n;
        }
        return true;
    }
}
