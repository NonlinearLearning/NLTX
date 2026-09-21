using System.Collections.ObjectModel;
using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Halls;

public sealed class DungeonHallGeometryWorkState
{
  private IReadOnlyList<DungeonTilePoint> _potentialPlatformPoints = Array.Empty<DungeonTilePoint>();

  public DungeonBoundsRectangle Bounds { get; private set; }

  public IReadOnlyList<DungeonTilePoint> PotentialPlatformPoints => _potentialPlatformPoints;

  public void Replace(
    DungeonBoundsRectangle bounds,
    IEnumerable<DungeonTilePoint> potentialPlatformPoints)
  {
    ArgumentNullException.ThrowIfNull(potentialPlatformPoints);
    if (bounds.IsEmpty)
    {
      throw new ArgumentException("Hall bounds must be non-empty.", nameof(bounds));
    }

    Bounds = bounds;
    _potentialPlatformPoints = new ReadOnlyCollection<DungeonTilePoint>(
      potentialPlatformPoints.ToArray());
  }

  public void Clear()
  {
    Bounds = default;
    _potentialPlatformPoints = Array.Empty<DungeonTilePoint>();
  }
}
