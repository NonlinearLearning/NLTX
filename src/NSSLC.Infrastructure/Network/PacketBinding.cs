using System.Buffers.Binary;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace NSSLC.Infrastructure.Network;

public abstract class PacketBinding {
  public byte MessageId { get; }
  public PacketDirection Direction { get; }
  public Type PacketType { get; }

  protected PacketBinding(byte messageId, PacketDirection direction, Type packetType) {
    MessageId = messageId;
    Direction = direction;
    PacketType = packetType;
  }

  public abstract object Decode(ReadOnlyMemory<byte> body);
  public abstract byte[] Encode(object packet);
}

public sealed class PacketBinding<TPacket> : PacketBinding {
  private readonly Func<ReadOnlyMemory<byte>, PacketReadResult<TPacket>> _decode;
  private readonly Func<TPacket, MemoryStream?> _encode;

  public PacketBinding(byte messageId, PacketDirection direction,
      Func<ReadOnlyMemory<byte>, PacketReadResult<TPacket>> decode,
      Func<TPacket, MemoryStream?> encode) : base(messageId, direction, typeof(TPacket)) {
    ArgumentNullException.ThrowIfNull(decode);
    ArgumentNullException.ThrowIfNull(encode);
    _decode = decode;
    _encode = encode;
  }

  public override object Decode(ReadOnlyMemory<byte> body) {
    PacketReadResult<TPacket> result = _decode.Invoke(body);
    if (!result.Success) {
      throw new PacketProtocolException(result.Error!.Code.ToString(), MessageId,
          result.Error.Offset);
    }
    if (result.Consumed != body.Length) {
      throw new PacketProtocolException("TrailingBytes", MessageId, result.Consumed);
    }
    return result.Packet!;
  }

  public override byte[] Encode(object packet) {
    if (packet.GetType() != typeof(TPacket)) {
      throw new PacketEncodingException("Packet type does not match the registered format.");
    }
    using MemoryStream? body = _encode.Invoke((TPacket)packet);
    if (body is null || body.Length > ushort.MaxValue - 3) {
      throw new PacketEncodingException("Encoding rejected or body exceeds the frame limit.");
    }
    byte[] frame = new byte[checked((int)body.Length + 3)];
    BinaryPrimitives.WriteUInt16LittleEndian(frame, (ushort)frame.Length);
    frame[2] = MessageId;
    body.Position = 0;
    body.ReadExactly(frame.AsSpan(3));
    return frame;
  }
}
