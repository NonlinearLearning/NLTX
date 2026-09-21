namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct TeleportPylonModulePacket(
  byte SubPacketType,
  short TileX,
  short TileY,
  byte PylonType);
