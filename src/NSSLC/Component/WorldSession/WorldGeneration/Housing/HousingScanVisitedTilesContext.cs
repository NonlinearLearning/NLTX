using System.Collections.Generic;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Housing;

public sealed class HousingScanVisitedTilesContext
{
  private readonly HashSet<TilePosition> _countedTiles = new();

  public HousingScanVisitedTilesContext(HousingScanBudgetDefinition budget)
  {
    Budget = budget;
  }

  public HousingScanBudgetDefinition Budget { get; }

  public int NumTileCount { get; private set; }

  public int LavaCount { get; private set; }

  public int IceCount { get; private set; }

  public int SandCount { get; private set; }

  public int RockCount { get; private set; }

  public int ShroomCount { get; private set; }

  public int CountedTileCount => _countedTiles.Count;

  public bool IsAtTileLimit => NumTileCount >= Budget.MaxTileCount;

  public bool Contains(TilePosition position)
  {
    return _countedTiles.Contains(position);
  }

  public bool TryAdd(TilePosition position)
  {
    if (IsAtTileLimit || !_countedTiles.Add(position))
    {
      return false;
    }

    NumTileCount++;
    return true;
  }

  public void AddLava()
  {
    LavaCount++;
  }

  public void AddIce()
  {
    IceCount++;
  }

  public void AddSand()
  {
    SandCount++;
  }

  public void AddRock()
  {
    RockCount++;
  }

  public void AddShroom()
  {
    ShroomCount++;
  }

  public void Clear()
  {
    _countedTiles.Clear();
    NumTileCount = 0;
    LavaCount = 0;
    IceCount = 0;
    SandCount = 0;
    RockCount = 0;
    ShroomCount = 0;
  }
}
