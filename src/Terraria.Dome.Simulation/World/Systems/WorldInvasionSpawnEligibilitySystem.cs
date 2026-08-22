namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class WorldInvasionSpawnEligibilitySystem
{
  public bool CanSpawn(int invasionType, int invasionSize, int invasionDelayTicks)
  {
    return invasionType > 0 && invasionSize > 0 && invasionDelayTicks == 0;
  }
}
