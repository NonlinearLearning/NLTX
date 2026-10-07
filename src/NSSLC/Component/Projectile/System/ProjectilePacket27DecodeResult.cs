namespace Terraria.Projectile;

public readonly record struct ProjectilePacket27DecodeResult(
  ProjectilePacket27DecodeStatus Status,
  ProjectileNetworkApplyCommand? Command);
