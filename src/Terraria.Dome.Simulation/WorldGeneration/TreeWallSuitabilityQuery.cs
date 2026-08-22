namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeWallSuitabilityQuery
{
  public static bool IsSuitable(
    LegacyTreeProfileKind treeProfileKind,
    TreeWallDefinition wall)
  {
    return wall.AllowsPlantsToGrow ||
      IsGemTreeProfile(treeProfileKind) && IsGemTreeWall(wall.WallType);
  }

  private static bool IsGemTreeProfile(LegacyTreeProfileKind treeProfileKind)
  {
    return treeProfileKind is
      LegacyTreeProfileKind.GemTreeTopaz or
      LegacyTreeProfileKind.GemTreeAmethyst or
      LegacyTreeProfileKind.GemTreeSapphire or
      LegacyTreeProfileKind.GemTreeEmerald or
      LegacyTreeProfileKind.GemTreeRuby or
      LegacyTreeProfileKind.GemTreeDiamond or
      LegacyTreeProfileKind.GemTreeAmber;
  }

  private static bool IsGemTreeWall(ushort wallType)
  {
    return wallType is
      2 or
      54 or
      55 or
      56 or
      57 or
      58 or
      59 or
      61 or
      185 or
      196 or
      197 or
      198 or
      199 or
      208 or
      209 or
      210 or
      211 or
      212 or
      213 or
      214 or
      215;
  }
}
