namespace Terraria.WorldGeneration.Dungeon.Styles;

public static class DungeonStyleSetCatalog
{
  public static DungeonStyleCatalog CreateDefault()
  {
    var entries = new List<DungeonStyleMaterialDefinition>();
    foreach (DungeonStyleId style in Enum.GetValues<DungeonStyleId>())
    {
      int styleValue = (int)style + 1;
      entries.Add(new DungeonStyleMaterialDefinition(
        Style: style,
        BrickTileType: styleValue,
        BrickGrassTileType: styleValue + 100,
        BrickCrackedTileType: styleValue + 200,
        BrickWallType: styleValue + 300,
        WindowGlassWallType: styleValue + 400,
        WindowClosedGlassWallType: styleValue + 500,
        WindowEdgeWallType: styleValue + 600,
        PitTrapTileType: styleValue + 700,
        LiquidType: 0,
        UnbreakableWallProgressionTier: (int)style / 3,
        EdgeDither: style is DungeonStyleId.Shimmer or DungeonStyleId.Crystal));
    }

    return new DungeonStyleCatalog(entries);
  }
}
