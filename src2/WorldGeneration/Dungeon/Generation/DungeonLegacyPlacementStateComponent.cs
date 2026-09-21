using Terraria.WorldGeneration.Dungeon.Bounds;
using Terraria.WorldGeneration.Dungeon.Styles;

namespace Terraria.WorldGeneration.Dungeon.Generation;

public sealed class DungeonLegacyPlacementStateComponent
{
  public int DungeonSide { get; private set; }

  public DungeonTilePoint DungeonLocation { get; private set; }

  public int DungeonColor { get; private set; }

  public int BrickTileType { get; private set; }

  public int BrickWallType { get; private set; }

  public int BrickCrackedTileType { get; private set; }

  public int WindowGlassWallType { get; private set; }

  public int WindowClosedGlassWallType { get; private set; }

  public int WindowEdgeWallType { get; private set; }

  public IReadOnlyList<int> WindowPlatformItemTypes { get; private set; } = Array.Empty<int>();

  public DungeonTilePoint GeneratingDungeonPosition { get; private set; }

  public DungeonTilePoint GeneratingDungeonTop { get; private set; }

  public int DungeonLootStyle { get; private set; }

  public DungeonBoundsRectangle OuterPotentialDungeonBounds { get; private set; }

  public DungeonBoundsRectangle InnerPotentialDungeonBounds { get; private set; }

  public DungeonTilePoint DungeonEntrancePosition { get; private set; }

  public void ApplyStyle(
    int dungeonSide,
    DungeonTilePoint dungeonLocation,
    int dungeonColor,
    DungeonStyleMaterialDefinition style,
    IEnumerable<int> windowPlatformItemTypes)
  {
    ArgumentNullException.ThrowIfNull(style);
    ArgumentNullException.ThrowIfNull(windowPlatformItemTypes);
    if (dungeonSide < 0 || dungeonColor < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(dungeonSide));
    }

    DungeonSide = dungeonSide;
    DungeonLocation = dungeonLocation;
    DungeonColor = dungeonColor;
    BrickTileType = style.BrickTileType;
    BrickWallType = style.BrickWallType;
    BrickCrackedTileType = style.BrickCrackedTileType;
    WindowGlassWallType = style.WindowGlassWallType;
    WindowClosedGlassWallType = style.WindowClosedGlassWallType;
    WindowEdgeWallType = style.WindowEdgeWallType;
    WindowPlatformItemTypes = Array.AsReadOnly(windowPlatformItemTypes.ToArray());
    DungeonLootStyle = (int)style.Style;
  }

  public void SetPlacement(
    DungeonTilePoint generatingDungeonPosition,
    DungeonTilePoint generatingDungeonTop,
    DungeonBoundsRectangle outerPotentialDungeonBounds,
    DungeonBoundsRectangle innerPotentialDungeonBounds,
    DungeonTilePoint dungeonEntrancePosition)
  {
    GeneratingDungeonPosition = generatingDungeonPosition;
    GeneratingDungeonTop = generatingDungeonTop;
    OuterPotentialDungeonBounds = outerPotentialDungeonBounds;
    InnerPotentialDungeonBounds = innerPotentialDungeonBounds;
    DungeonEntrancePosition = dungeonEntrancePosition;
  }
}
