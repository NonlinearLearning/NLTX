using System;

namespace Terraria.Dome.Protocol.V1456.Packets;

public sealed record NetModulePacket(ushort ModuleId, ReadOnlyMemory<byte> Payload)
{
  public byte[]? ResponseFrame { get; init; }
}
