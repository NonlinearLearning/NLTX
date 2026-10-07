using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public sealed class StatueTrapSelectionDefinition
{
  public StatueTrapSelectionDefinition(IReadOnlyList<int> statueIndexes)
  {
    ArgumentNullException.ThrowIfNull(statueIndexes);

    int[] copy = new int[statueIndexes.Count];
    for (int index = 0; index < statueIndexes.Count; index++)
    {
      int statueIndex = statueIndexes[index];
      if (statueIndex < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(statueIndexes));
      }

      copy[index] = statueIndex;
    }

    StatueIndexes = Array.AsReadOnly(copy);
  }

  public IReadOnlyList<int> StatueIndexes { get; }
}
