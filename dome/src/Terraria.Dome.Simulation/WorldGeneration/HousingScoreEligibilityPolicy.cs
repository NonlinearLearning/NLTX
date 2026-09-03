namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HousingScoreEligibilityPolicy
{
  public static HousingScoreEligibilityDecision Evaluate(int score)
  {
    bool canSpawn = score > 0;
    return new HousingScoreEligibilityDecision(score, canSpawn, !canSpawn);
  }
}
