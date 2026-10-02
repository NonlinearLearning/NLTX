namespace Terraria.Projectile;

public struct ProjectileSentryCapabilityComponent
{
  public ProjectileSentryCapabilityComponent(
    bool isSentry = false)
  {
    IsSentry = isSentry;
  }

  public bool IsSentry;
}
