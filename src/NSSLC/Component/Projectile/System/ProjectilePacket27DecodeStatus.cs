namespace Terraria.Projectile;

public enum ProjectilePacket27DecodeStatus : byte
{
  Decoded,
  Truncated,
  Invalid,
  TrailingBytes,
}
