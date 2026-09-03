namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct TemporaryAnimationPacket(
  short AnimationType,
  ushort TileType,
  short TileX,
  short TileY);
