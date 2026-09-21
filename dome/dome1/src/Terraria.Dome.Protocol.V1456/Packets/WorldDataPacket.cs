using System;

namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct WorldDataPacket(ReadOnlyMemory<byte> Payload)
{
  public static WorldDataPacket CreateDefault()
  {
    return new WorldDataPacket(TerrariaPacketCodec.CreateDefaultWorldDataPayload());
  }
}
