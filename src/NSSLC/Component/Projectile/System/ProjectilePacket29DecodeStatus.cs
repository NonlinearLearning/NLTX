namespace Terraria.Projectile;

public enum ProjectilePacket29DecodeStatus : byte
{
  Decoded,
  Truncated,
  TrailingBytes,
  Invalid,
}
