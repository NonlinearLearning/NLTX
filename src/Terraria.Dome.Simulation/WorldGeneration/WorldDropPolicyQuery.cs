namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WorldDropPolicyQuery
{
  public static bool ShouldDropItems(bool stopDrops, bool effectOnly, bool noItem)
  {
    return !stopDrops && !effectOnly && !noItem;
  }

  public static bool ShouldDropEffects(bool stopDrops, bool noItem)
  {
    return !stopDrops && !noItem;
  }
}
