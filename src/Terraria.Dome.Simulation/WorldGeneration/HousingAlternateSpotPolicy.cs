namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HousingAlternateSpotPolicy
{
  public static HousingAlternateSpotDecision Evaluate(
    bool hasAlternateSpot,
    bool currentlyTryingToUseAlternateHousingSpot)
  {
    bool canTryAlternateSpot = hasAlternateSpot && !currentlyTryingToUseAlternateHousingSpot;
    return new HousingAlternateSpotDecision(
      hasAlternateSpot,
      currentlyTryingToUseAlternateHousingSpot,
      canTryAlternateSpot);
  }
}
