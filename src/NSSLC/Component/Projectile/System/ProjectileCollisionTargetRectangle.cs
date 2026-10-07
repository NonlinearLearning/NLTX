namespace Terraria.Projectile;

/// <summary>
/// Target rectangle captured for one projectile collision decision.
/// </summary>
public readonly record struct ProjectileCollisionTargetRectangle(
  int X,
  int Y,
  int Width,
  int Height)
{
  public bool Intersects(in ProjectileDamageHitbox other)
  {
    return X < other.X + other.Width &&
      other.X < X + Width &&
      Y < other.Y + other.Height &&
      other.Y < Y + Height;
  }

  public bool Intersects(in ProjectileCollisionTargetRectangle other)
  {
    return X < other.X + other.Width &&
      other.X < X + Width &&
      Y < other.Y + other.Height &&
      other.Y < Y + Height;
  }
}
