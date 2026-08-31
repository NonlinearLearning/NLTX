namespace Terraria.Dome.Simulation.Projectile.Definitions;

public readonly record struct ProjectileOnDespawnAreaDamage(float Width, float Height)
{
  public bool IsEnabled => Width > 0.0f && Height > 0.0f;
}
