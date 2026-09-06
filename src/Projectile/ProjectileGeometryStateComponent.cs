namespace Terraria.Projectile;

public struct ProjectileGeometryStateComponent
{
  public ProjectileGeometryStateComponent(
    float scale = 1.0f,
    bool reflected = false)
  {
    Scale = scale;
    Reflected = reflected;
  }

  public float Scale;
  public bool Reflected;
}
