using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Terrain;

public sealed class WorldStormSafeSpotScratch
{
  private readonly List<WorldStormSafeSpot> _safeSpots = new();

  public int Count => _safeSpots.Count;

  public void Add(WorldStormSafeSpot safeSpot)
  {
    _safeSpots.Add(safeSpot);
  }

  public void Clear()
  {
    _safeSpots.Clear();
  }

  public WorldStormSafeSpot Get(int index)
  {
    if ((uint)index >= _safeSpots.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    return _safeSpots[index];
  }
}
