using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldObjects.Chest.Systems;

public sealed class ChestIndexSystem
{
  private readonly Dictionary<(int X, int Y), int> _chestIds = new();

  public bool CanAdd(ChestComponent chest)
  {
    ArgumentNullException.ThrowIfNull(chest);
    return !_chestIds.ContainsKey((chest.TileX, chest.TileY));
  }

  public bool TryAdd(ChestComponent chest)
  {
    ArgumentNullException.ThrowIfNull(chest);
    return _chestIds.TryAdd((chest.TileX, chest.TileY), chest.ChestId);
  }

  public bool TryGetChestId(int tileX, int tileY, out int chestId)
  {
    return _chestIds.TryGetValue((tileX, tileY), out chestId);
  }

  public bool TryRemove(ChestComponent chest)
  {
    ArgumentNullException.ThrowIfNull(chest);
    (int X, int Y) coordinate = (chest.TileX, chest.TileY);
    if (!_chestIds.TryGetValue(coordinate, out int chestId) || chestId != chest.ChestId)
    {
      return false;
    }

    return _chestIds.Remove(coordinate);
  }
}
