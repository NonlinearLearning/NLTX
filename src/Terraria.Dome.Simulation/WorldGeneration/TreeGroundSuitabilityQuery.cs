namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeGroundSuitabilityQuery
{
  private const ushort CorruptGrassTileType = 23;
  private const ushort CrimsonGrassTileType = 199;
  private const ushort AshGrassTileType = 633;

  public static bool IsSuitable(
    LegacyTreeProfileKind treeProfileKind,
    TreeGroundTileDefinition ground)
  {
    return treeProfileKind switch
    {
      LegacyTreeProfileKind.GemTreeTopaz or
      LegacyTreeProfileKind.GemTreeAmethyst or
      LegacyTreeProfileKind.GemTreeSapphire or
      LegacyTreeProfileKind.GemTreeEmerald or
      LegacyTreeProfileKind.GemTreeRuby or
      LegacyTreeProfileKind.GemTreeDiamond or
      LegacyTreeProfileKind.GemTreeAmber => ground.IsStone || ground.IsMoss,
      LegacyTreeProfileKind.VanityTreeSakura or
      LegacyTreeProfileKind.VanityTreeWillow =>
        ground.IsGrass &&
        ground.TileType != CorruptGrassTileType &&
        ground.TileType != CrimsonGrassTileType,
      LegacyTreeProfileKind.TreeAsh => ground.TileType == AshGrassTileType,
      _ => false
    };
  }
}
