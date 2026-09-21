using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Halls;

public static class DungeonHallQuery
{
  public static bool Contains(
    DungeonHallGeometryWorkState geometry,
    DungeonTilePoint point)
  {
    ArgumentNullException.ThrowIfNull(geometry);
    return geometry.Bounds.Contains(point);
  }
}
