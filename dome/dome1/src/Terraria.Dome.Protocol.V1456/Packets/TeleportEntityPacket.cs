namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct TeleportEntityPacket(
  byte Flags,
  short TargetId,
  float PositionX,
  float PositionY,
  byte Style,
  int? ExtraInfo);
