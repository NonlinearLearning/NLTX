namespace Terraria.Projectile;

public struct ProjectileCounterweightCapabilityComponent
{
  public ProjectileCounterweightCapabilityComponent(
    bool isCounterweight = false)
  {
    IsCounterweight = isCounterweight;
  }

  public bool IsCounterweight;
}
