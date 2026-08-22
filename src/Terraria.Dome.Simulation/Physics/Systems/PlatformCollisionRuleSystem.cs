namespace Terraria.Dome.Simulation.Physics.Systems;

public static class PlatformCollisionRuleSystem
{
  public static bool ShouldCollideFromAbove(
    bool isPlatform,
    bool isProperTopFrame,
    bool fallThrough,
    bool fall2,
    float legacyVelocityY)
  {
    if (!isPlatform || !isProperTopFrame)
    {
      return false;
    }

    return !(fallThrough && (legacyVelocityY <= 1.0f || fall2));
  }
}
