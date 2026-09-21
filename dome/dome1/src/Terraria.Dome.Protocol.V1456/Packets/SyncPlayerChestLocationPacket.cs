namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct SyncPlayerChestLocationPacket(
  byte PlayerId,
  short X,
  short Y,
  byte Type);
