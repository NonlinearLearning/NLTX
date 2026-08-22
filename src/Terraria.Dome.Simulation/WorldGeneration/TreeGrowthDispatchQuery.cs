namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeGrowthDispatchQuery
{
  public static bool TryGet(int treeTileType, out TreeGrowthDispatch dispatch)
  {
    switch (treeTileType)
    {
      case 5:
        dispatch = new TreeGrowthDispatch(TreeGrowthDispatchKind.Ordinary, null);
        return true;
      case 323:
        dispatch = new TreeGrowthDispatch(TreeGrowthDispatchKind.Palm, null);
        return true;
      case 583:
        return TryCreateProfile(LegacyTreeProfileKind.GemTreeTopaz, out dispatch);
      case 584:
        return TryCreateProfile(LegacyTreeProfileKind.GemTreeAmethyst, out dispatch);
      case 585:
        return TryCreateProfile(LegacyTreeProfileKind.GemTreeSapphire, out dispatch);
      case 586:
        return TryCreateProfile(LegacyTreeProfileKind.GemTreeEmerald, out dispatch);
      case 587:
        return TryCreateProfile(LegacyTreeProfileKind.GemTreeRuby, out dispatch);
      case 588:
        return TryCreateProfile(LegacyTreeProfileKind.GemTreeDiamond, out dispatch);
      case 589:
        return TryCreateProfile(LegacyTreeProfileKind.GemTreeAmber, out dispatch);
      case 596:
        return TryCreateProfile(LegacyTreeProfileKind.VanityTreeSakura, out dispatch);
      case 616:
        return TryCreateProfile(LegacyTreeProfileKind.VanityTreeWillow, out dispatch);
      case 634:
        return TryCreateProfile(LegacyTreeProfileKind.TreeAsh, out dispatch);
      default:
        dispatch = default;
        return false;
    }
  }

  private static bool TryCreateProfile(
    LegacyTreeProfileKind profileKind,
    out TreeGrowthDispatch dispatch)
  {
    dispatch = new TreeGrowthDispatch(TreeGrowthDispatchKind.Profile, profileKind);
    return true;
  }
}
