using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Terrain.TreeTops;

public sealed class WorldTreeTopsStateSnapshot
{
  public WorldTreeTopsStateSnapshot(IReadOnlyList<int> variations)
  {
    ArgumentNullException.ThrowIfNull(variations);
    if (variations.Count != WorldTreeTopsStateComponent.TreeTopsAreaCount)
    {
      throw new ArgumentException(
        $"Tree tops snapshots require {WorldTreeTopsStateComponent.TreeTopsAreaCount} areas.",
        nameof(variations));
    }

    int[] copy = new int[variations.Count];
    for (int i = 0; i < variations.Count; i++)
    {
      copy[i] = variations[i];
    }

    Variations = Array.AsReadOnly(copy);
  }

  public IReadOnlyList<int> Variations { get; }
}
