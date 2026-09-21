namespace Terraria.WorldGeneration.Dungeon.Bounds;

public sealed class DungeonGeometryRuleCatalog
{
  public DungeonGeometryRuleCatalog(
    int hallwayDoorPlacementVariance,
    int hallInnerAreaDepth,
    int hallOuterWallDepth,
    int roomInnerAreaDepth,
    int roomOuterWallDepth)
  {
    int[] values =
    [
      hallwayDoorPlacementVariance,
      hallInnerAreaDepth,
      hallOuterWallDepth,
      roomInnerAreaDepth,
      roomOuterWallDepth
    ];
    if (values.Any(value => value < 0))
    {
      throw new ArgumentOutOfRangeException(nameof(hallwayDoorPlacementVariance));
    }

    HallwayDoorPlacementVariance = hallwayDoorPlacementVariance;
    HallInnerAreaDepth = hallInnerAreaDepth;
    HallOuterWallDepth = hallOuterWallDepth;
    RoomInnerAreaDepth = roomInnerAreaDepth;
    RoomOuterWallDepth = roomOuterWallDepth;
  }

  public int HallwayDoorPlacementVariance { get; }

  public int HallInnerAreaDepth { get; }

  public int HallOuterWallDepth { get; }

  public int RoomInnerAreaDepth { get; }

  public int RoomOuterWallDepth { get; }
}
