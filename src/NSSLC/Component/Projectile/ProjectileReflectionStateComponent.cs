namespace Terraria.Projectile;

public struct ProjectileReflectionStateComponent
{
  public ProjectileReflectionStateComponent(bool reflected = false)
  {
    Reflected = reflected;
  }

  public bool Reflected;
}
