namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct TeleportPlayerThroughPortalPacket(
  byte PlayerSlot,
  short PortalColor,
  float PositionX,
  float PositionY,
  float VelocityX,
  float VelocityY);
