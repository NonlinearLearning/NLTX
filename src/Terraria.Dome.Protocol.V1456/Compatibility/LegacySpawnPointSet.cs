using System;
using System.Collections.Generic;

namespace Terraria.Dome.Protocol.V1456.Compatibility;

public sealed class LegacySpawnPointSet
{
  private readonly LegacySpawnPoint[] _points;

  public LegacySpawnPointSet(IReadOnlyList<LegacySpawnPoint> points)
  {
    ArgumentNullException.ThrowIfNull(points);
    if (points.Count > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(points));
    }

    _points = new LegacySpawnPoint[points.Count];
    for (int index = 0; index < points.Count; index++)
    {
      _points[index] = points[index];
    }
  }

  public int Count => _points.Length;

  public LegacySpawnPoint this[int index] => _points[index];
}
