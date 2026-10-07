using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Housing;

public sealed class HousingRoomTileClassificationContext
{
  private readonly bool[] _classifiedTileTypes;

  public HousingRoomTileClassificationContext(int tileTypeCount)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileTypeCount);
    _classifiedTileTypes = new bool[tileTypeCount];
  }

  public int TileTypeCount => _classifiedTileTypes.Length;

  public int ClassifiedTileTypeCount { get; private set; }

  public IReadOnlyList<int> CreateSnapshot()
  {
    int[] tileTypes = new int[ClassifiedTileTypeCount];
    int snapshotIndex = 0;
    for (int tileType = 0; tileType < _classifiedTileTypes.Length; tileType++)
    {
      if (_classifiedTileTypes[tileType])
      {
        tileTypes[snapshotIndex] = tileType;
        snapshotIndex++;
      }
    }

    return Array.AsReadOnly(tileTypes);
  }

  public bool Contains(int tileType)
  {
    ValidateTileType(tileType);
    return _classifiedTileTypes[tileType];
  }

  public void Mark(int tileType)
  {
    ValidateTileType(tileType);
    if (_classifiedTileTypes[tileType])
    {
      return;
    }

    _classifiedTileTypes[tileType] = true;
    ClassifiedTileTypeCount++;
  }

  public void Clear()
  {
    Array.Clear(_classifiedTileTypes);
    ClassifiedTileTypeCount = 0;
  }

  private void ValidateTileType(int tileType)
  {
    if (tileType < 0 || tileType >= _classifiedTileTypes.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(tileType));
    }
  }
}
