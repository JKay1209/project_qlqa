using QLQA.Constants;
using QLQA.Utilities;

namespace QLQA.Protocol;

/// <summary>
/// framing: biến Packet thành byte trên dây và ngược lại.
///   Gửi:  [Header (13B)][Payload (N B)]
///   Nhận: đọc đúng 13B header -> biết N -> đọc đúng N B payload.
/// </summary>

public static class PacketCodec
{
    // Gói -> 1 mảng byte duy nhất (header + payload).
    public static byte[] Encode(Packet packet)
    {
        var header = new PacketHeader(NetworkConstants.ProtocolVersion, packet.Type, packet.RequestId, packet.Payload.Length);

        if (packet.Payload.Length > NetworkConstants.MaxPayloadSize)
            throw new InvalidOperationException($"Payload {packet.Payload.Length}B vượt giới hạn {NetworkConstants.MaxPayloadSize}B.");

        var frame = new byte[NetworkConstants.HeaderSize + packet.Payload.Length];
        header.WriteTo(frame);
        packet.Payload.CopyTo(frame, NetworkConstants.HeaderSize);
        return frame;
    }

    /// Ghi gói lên stream bằng 1 lần WriteAsync
    public static async Task WriteAsync(Stream stream, Packet packet, CancellationToken ct = default)
    {
        byte[] frame = Encode(packet);
        await stream.WriteAsync(frame, ct).ConfigureAwait(false);
        await stream.FlushAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Đọc một gói đầy đủ. Trả về null nếu đối phương đóng kết nối
    /// Trả về InvalidDataException nếu dữ liệu sai giao thức, EndOfStreamException nếu đứt giữa chừng.
    /// </summary>
    public static async Task<Packet?> ReadAsync(Stream stream, CancellationToken ct = default)
    {
        var headerBuf = new byte[NetworkConstants.HeaderSize];
        if (!await stream.ReadExactAsync(headerBuf, ct).ConfigureAwait(false))
            return null;

        PacketHeader header = PacketHeader.Parse(headerBuf);

        var payload = new byte[header.PayloadLength];
        if (header.PayloadLength > 0)
        {
            // Đã nhận header mà payload không đến -> coi là đứt giữa chừng, không phải đóng sạch
            if (!await stream.ReadExactAsync(payload, ct).ConfigureAwait(false))
                throw new EndOfStreamException("Kết nối đóng sau header, thiếu payload.");
        }

        return new Packet(header.Type, header.RequestId, payload);
    }
}
