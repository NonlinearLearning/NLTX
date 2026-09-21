namespace Terraria.WorldSession.Components;

public static class WorldSavedOreTierRepairQuery
{
  public static OreTierState Calculate(
    OreTierState loadedState,
    in WorldSavedOreTierTileCounts tileCounts)
  {
    if (!RequiresLowTierRepair(loadedState))
    {
      return loadedState;
    }

    return new OreTierState(
      SelectTier(
        WorldSavedOreTierDefaults.Copper,
        166,
        tileCounts.CopperVanillaCount,
        tileCounts.CopperAlternateCount),
      SelectTier(
        WorldSavedOreTierDefaults.Iron,
        167,
        tileCounts.IronVanillaCount,
        tileCounts.IronAlternateCount),
      SelectTier(
        WorldSavedOreTierDefaults.Silver,
        168,
        tileCounts.SilverVanillaCount,
        tileCounts.SilverAlternateCount),
      SelectTier(
        WorldSavedOreTierDefaults.Gold,
        169,
        tileCounts.GoldVanillaCount,
        tileCounts.GoldAlternateCount),
      loadedState.Cobalt,
      loadedState.Mythril,
      loadedState.Adamantite);
  }

  public static bool RequiresLowTierRepair(OreTierState state)
  {
    return state.Copper == -1 ||
      state.Iron == -1 ||
      state.Silver == -1 ||
      state.Gold == -1;
  }

  private static int SelectTier(
    int vanillaTileType,
    int alternateTileType,
    int vanillaCount,
    int alternateCount)
  {
    return vanillaCount > alternateCount
      ? vanillaTileType
      : alternateTileType;
  }
}
