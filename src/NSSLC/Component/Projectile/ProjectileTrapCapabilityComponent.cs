namespace Terraria.Projectile;

public struct ProjectileTrapCapabilityComponent
{
  public ProjectileTrapCapabilityComponent(
    bool isTrap = false)
  {
    IsTrap = isTrap;
  }

  public bool IsTrap;
}
