namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileHousingRuleQuery
{
  public static TileHousingRuleSnapshot Create(bool preventInfiniteRopeFraming)
  {
    return new TileHousingRuleSnapshot(preventInfiniteRopeFraming);
  }
}
