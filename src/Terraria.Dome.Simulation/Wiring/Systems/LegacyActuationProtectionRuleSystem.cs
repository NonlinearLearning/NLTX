namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class LegacyActuationProtectionRuleSystem
{
  public static bool PreventsActuationUnder(ushort tileType)
  {
    return tileType is 21 or 467 or 26 or 77 or 88 or 470 or 475 or 237 or 597 or 441 or 468;
  }
}
