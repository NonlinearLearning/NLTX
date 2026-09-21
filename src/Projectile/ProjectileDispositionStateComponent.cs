namespace Terraria.Projectile;

public struct ProjectileDispositionStateComponent
{
  public ProjectileDispositionStateComponent(
    bool friendly = false,
    bool hostile = false)
  {
    Friendly = friendly;
    Hostile = hostile;
  }

  public bool Friendly;

  public bool Hostile;
}
