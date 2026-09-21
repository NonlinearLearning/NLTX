using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Placement;

public sealed class DungeonDoorPlacementRequest
{
  public DungeonDoorPlacementRequest(DungeonTilePoint position)
  {
    Position = position;
  }

  public DungeonTilePoint Position { get; }

  public int? OverrideBrickTileType { get; init; }

  public int? OverrideBrickWallType { get; init; }

  public int? OverrideStyle { get; init; }

  public DungeonTilePoint Direction { get; init; }

  public bool InAHallway { get; init; }

  public int OverrideWidthFluff { get; init; }

  public bool SkipOtherDoorsCheck { get; init; }

  public bool SkipSpaceCheck { get; init; }

  public bool AlwaysClearArea { get; init; }
}
