namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct TeleportNpcThroughPortalPacket(
  ushort NpcId,
  short PortalColor,
  float PositionX,
  float PositionY,
  float VelocityX,
  float VelocityY);
