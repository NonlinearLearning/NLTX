using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileTypeCountSnapshot
{
  private readonly IReadOnlyList<int>? _counts;

  public TileTypeCountSnapshot(IReadOnlyList<int> counts)
  {
    ArgumentNullException.ThrowIfNull(counts);
    if (counts.Count != TileDefinitionRegistry.Version4TileCount)
    {
      throw new ArgumentException(
        "Tile type counts must match the Version4 tile registry.",
        nameof(counts));
    }

    int[] copy = new int[counts.Count];
    for (int index = 0; index < counts.Count; index++)
    {
      if (counts[index] < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(counts));
      }

      copy[index] = counts[index];
    }

    _counts = Array.AsReadOnly(copy);
  }

  public IReadOnlyList<int> Counts => _counts ?? Array.Empty<int>();

  public int TotalCount
  {
    get
    {
      int total = 0;
      for (int index = 0; index < Counts.Count; index++)
      {
        total = checked(total + Counts[index]);
      }

      return total;
    }
  }
}
