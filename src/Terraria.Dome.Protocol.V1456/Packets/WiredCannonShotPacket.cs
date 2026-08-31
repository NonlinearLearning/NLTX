namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct WiredCannonShotPacket(
  short Damage,
  float Knockback,
  short X,
  short Y,
  short Angle,
  short Ammo,
  byte PlayerId);
