namespace Terraria.Dome.Simulation.Physics.Systems;

public static class TileCollisionActivationRuleSystem
{
  public static bool ShouldParticipate(bool isActive, bool isInactive)
  {
    return isActive && !isInactive;
  }
}
