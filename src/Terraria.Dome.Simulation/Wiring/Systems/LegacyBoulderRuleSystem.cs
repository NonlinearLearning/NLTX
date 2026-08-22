namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class LegacyBoulderRuleSystem
{
  public static bool IsBoulder(ushort tileType)
  {
    return tileType is 138 or 484 or 664 or 665 or 711 or 712 or 713 or 714 or 715 or 716;
  }
}
