namespace Terraria.Projectile;

public readonly record struct ProjectilePacket29DecodeResult(
  ProjectilePacket29DecodeStatus Status,
  ProjectileNetworkTerminateCommand? Command);
