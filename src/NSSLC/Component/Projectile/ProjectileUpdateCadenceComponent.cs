namespace Terraria.Projectile;

public record struct ProjectileUpdateCadenceComponent(
  int ExtraUpdates = 0)
{
  public int MaxUpdates
  {
    readonly get => ExtraUpdates + 1;
    set => ExtraUpdates = value - 1;
  }
}
