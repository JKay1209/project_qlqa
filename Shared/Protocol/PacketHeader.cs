using System.Buffers.Binary;
using QLQA.Constants;

namespace QLQA.Protocol;

/// <summary>
/// Header cố định 13 byte, big-endian (network byte order):
///
///  offset  size  field
///  0       2     Magic         0x5253 ("RS")
///  2       1     Version       1
///  3       2     MessageType   ushort
///  5       4     RequestId     int  (client đặt; server trả lại cùng số để ghép Request-Result)
///  9       4     PayloadLength int  (số byte JSON phía sau)
/// </summary>
public readonly record struct PacketHeader(byte Version, MessageType Type, int RequestId, int PayloadLength)
{
    public void WriteTo(Span<byte> dest)
    {
        BinaryPrimitives.WriteUInt16BigEndian(dest[0..2], NetworkConstants.Magic);
        dest[2] = Version;
        BinaryPrimitives.WriteUInt16BigEndian(dest[3..5], (ushort)Type);
        BinaryPrimitives.WriteInt32BigEndian(dest[5..9], RequestId);
        BinaryPrimitives.WriteInt32BigEndian(dest[9..13], PayloadLength);
    }

    /// <summary>Đọc và KIỂM TRA header. Dữ liệu từ mạng không bao giờ được tin tưởng.</summary>
    public static PacketHeader Parse(ReadOnlySpan<byte> src)
    {
        ushort magic = BinaryPrimitives.ReadUInt16BigEndian(src[0..2]);
        if (magic != NetworkConstants.Magic)
            throw new InvalidDataException($"Sai magic 0x{magic:X4}: không phải gói tin của hệ thống.");

        byte version = src[2];
        if (version != NetworkConstants.ProtocolVersion)
            throw new InvalidDataException($"Phiên bản giao thức {version} không được hỗ trợ.");

        var type = (MessageType)BinaryPrimitives.ReadUInt16BigEndian(src[3..5]);
        int requestId = BinaryPrimitives.ReadInt32BigEndian(src[5..9]);
        int length = BinaryPrimitives.ReadInt32BigEndian(src[9..13]);

        if (length < 0 || length > NetworkConstants.MaxPayloadSize)
            throw new InvalidDataException($"PayloadLength {length} không hợp lệ (tối đa {NetworkConstants.MaxPayloadSize}).");

        return new PacketHeader(version, type, requestId, length);
    }
}
