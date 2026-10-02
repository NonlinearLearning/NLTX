using System.Collections.ObjectModel;
using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Spawn;

public sealed class ExtraSpawnPointState
{
  private readonly List<DungeonTilePoint> _points = new();

  public IReadOnlyList<DungeonTilePoint> Points => new ReadOnlyCollection<DungeonTilePoint>(_points.ToArray());

  public IReadOnlyList<DungeonTilePoint> LandmassCandidates { get; private set; } =
    Array.Empty<DungeonTilePoint>();

  public void ReplaceLandmassCandidates(IEnumerable<DungeonTilePoint> candidates)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    LandmassCandidates = Array.AsReadOnly(candidates.ToArray());
  }

  public void SetTeamSpawn(int team, DungeonTilePoint point)
  {
    if (team < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(team));
    }

    while (_points.Count <= team)
    {
      _points.Add(default);
    }

    _points[team] = point;
  }

  public bool TryGetTeamSpawn(int team, out DungeonTilePoint point)
  {
    if (team < 0 || team >= _points.Count)
    {
      point = default;
      return false;
    }

    point = _points[team];
    return true;
  }

  public void Clear()
  {
    _points.Clear();
    LandmassCandidates = Array.Empty<DungeonTilePoint>();
  }
}
