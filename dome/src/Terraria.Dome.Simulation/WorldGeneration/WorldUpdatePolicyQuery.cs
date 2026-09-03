namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WorldUpdatePolicyQuery
{
  public static WorldUpdatePolicySnapshot Evaluate(
    bool hardMode,
    bool remixWorld,
    bool getGoodWorld,
    bool tenthAnniversaryWorld,
    bool stopBiomeSpreadPowerEnabled,
    bool notTheBeesWorld,
    WorldUpdatePhase phase)
  {
    bool hardModeWorldUpdates =
      hardMode || (remixWorld && getGoodWorld && !tenthAnniversaryWorld);
    bool allowedToSpreadInfections = !stopBiomeSpreadPowerEnabled;
    bool growGrassUnderground = phase == WorldUpdatePhase.Underground &&
      (remixWorld || (!remixWorld && notTheBeesWorld));
    return new WorldUpdatePolicySnapshot(
      hardModeWorldUpdates,
      allowedToSpreadInfections,
      growGrassUnderground);
  }
}
