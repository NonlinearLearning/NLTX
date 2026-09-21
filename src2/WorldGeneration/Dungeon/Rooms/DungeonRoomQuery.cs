using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Rooms;

public static class DungeonRoomQuery
{
  public static DungeonTilePoint Center(DungeonRoomGeometryWorkState geometry)
  {
    ArgumentNullException.ThrowIfNull(geometry);
    DungeonBoundsRectangle bounds = geometry.InnerBounds;
    return new DungeonTilePoint(
      bounds.X + bounds.Width / 2,
      bounds.Y + bounds.Height / 2);
  }

  public static bool Contains(
    DungeonRoomGeometryWorkState geometry,
    DungeonTilePoint point)
  {
    ArgumentNullException.ThrowIfNull(geometry);
    return geometry.InnerBounds.Contains(point);
  }
}
