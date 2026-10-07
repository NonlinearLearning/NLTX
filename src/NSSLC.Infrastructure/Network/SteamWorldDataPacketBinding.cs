using System.Buffers.Binary;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>Adds Steam 326's dungeon coordinates to the generated Version4 world layout.</summary>
public sealed class SteamWorldDataPacketBinding : PacketBinding {
  private const int DungeonBytes = 4;
  private readonly PacketBinding _generated;

  public SteamWorldDataPacketBinding(PacketBinding generated)
      : base(7, generated?.Direction ?? throw new ArgumentNullException(nameof(generated)),
          typeof(WorldDataPacket)) {
    if (generated.MessageId != 7 || generated.PacketType != typeof(WorldDataPacket)) {
      throw new ArgumentException("The base binding must encode world data.", nameof(generated));
    }
    _generated = generated;
  }

  public override object Decode(ReadOnlyMemory<byte> body) {
    if (body.Length < DungeonBytes) {
      throw new PacketProtocolException("Truncated", MessageId, body.Length);
    }
    var packet = (WorldDataPacket)_generated.Decode(body[..^DungeonBytes]);
    ReadOnlySpan<byte> dungeon = body.Span[^DungeonBytes..];
    packet.DungeonX = BinaryPrimitives.ReadInt16LittleEndian(dungeon);
    packet.DungeonY = BinaryPrimitives.ReadInt16LittleEndian(dungeon[2..]);
    return packet;
  }

  public override byte[] Encode(object packet) {
    byte[] frame = _generated.Encode(packet);
    if (frame.Length > ushort.MaxValue - DungeonBytes) {
      throw new PacketEncodingException("Steam world data exceeds the frame limit.");
    }
    var world = (WorldDataPacket)packet;
    int originalLength = frame.Length;
    Array.Resize(ref frame, originalLength + DungeonBytes);
    BinaryPrimitives.WriteUInt16LittleEndian(frame, checked((ushort)frame.Length));
    BinaryPrimitives.WriteInt16LittleEndian(frame.AsSpan(originalLength), world.DungeonX);
    BinaryPrimitives.WriteInt16LittleEndian(frame.AsSpan(originalLength + 2), world.DungeonY);
    return frame;
  }
}
