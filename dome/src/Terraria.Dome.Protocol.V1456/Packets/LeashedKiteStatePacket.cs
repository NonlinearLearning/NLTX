namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct LeashedKiteStatePacket(
  int? ProjectileType,
  float PositionX,
  float PositionY,
  uint PackedVelocity,
  byte Rotation,
  float WindTarget,
  float CloudAlpha,
  float TimeCounter);
