namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HousingStinkbugSpawnPolicy
{
  public static HousingStinkbugSpawnDecision Evaluate(
    bool hasStinkbug,
    bool hasEchoStinkbug,
    bool hasTownPetRoom)
  {
    bool isBlocked = (hasStinkbug || hasEchoStinkbug) && !hasTownPetRoom;
    return new HousingStinkbugSpawnDecision(isBlocked, hasStinkbug, hasEchoStinkbug);
  }
}
