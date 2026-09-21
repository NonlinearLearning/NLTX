namespace Terraria.Projectile;

public readonly record struct ProjectileUpdateCadenceComponent(
  int ExtraUpdates = 0)
{
  public int MaxUpdates => ExtraUpdates + 1;
}
