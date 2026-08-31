namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct BugReleasingPacket(
  int TileX,
  int TileY,
  short NpcType,
  byte Style);
