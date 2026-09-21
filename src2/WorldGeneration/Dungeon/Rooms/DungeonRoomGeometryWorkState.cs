using System.Collections.ObjectModel;
using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Rooms;

public sealed class DungeonRoomGeometryWorkState
{
  private IReadOnlyList<DungeonTilePoint> _positions = Array.Empty<DungeonTilePoint>();

  public DungeonBoundsRectangle InnerBounds { get; private set; }

  public DungeonBoundsRectangle OuterBounds { get; private set; }

  public IReadOnlyList<DungeonTilePoint> Positions => _positions;

  public void Replace(
    DungeonBoundsRectangle innerBounds,
    DungeonBoundsRectangle outerBounds,
    IEnumerable<DungeonTilePoint> positions)
  {
    ArgumentNullException.ThrowIfNull(positions);
    if (innerBounds.IsEmpty || outerBounds.IsEmpty)
    {
      throw new ArgumentException("Room geometry bounds must be non-empty.");
    }

    InnerBounds = innerBounds;
    OuterBounds = outerBounds;
    _positions = new ReadOnlyCollection<DungeonTilePoint>(positions.ToArray());
  }

  public void Clear()
  {
    InnerBounds = default;
    OuterBounds = default;
    _positions = Array.Empty<DungeonTilePoint>();
  }
}
