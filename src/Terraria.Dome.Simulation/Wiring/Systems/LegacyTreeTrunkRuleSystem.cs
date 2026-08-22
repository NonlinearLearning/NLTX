namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class LegacyTreeTrunkRuleSystem
{
  public static bool IsTreeTrunk(ushort tileType)
  {
    return tileType is 5 or 72 or 583 or 584 or 585 or 586 or 587 or 588 or 589 or 596 or 616 or 634;
  }
}
