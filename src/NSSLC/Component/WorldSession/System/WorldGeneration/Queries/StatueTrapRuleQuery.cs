using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

public static class StatueTrapRuleQuery
{
  public static bool Contains(
    StatueTrapSelectionDefinition definition,
    int statueIndex)
  {
    ArgumentNullException.ThrowIfNull(definition);
    for (int index = 0; index < definition.StatueIndexes.Count; index++)
    {
      if (definition.StatueIndexes[index] == statueIndex)
      {
        return true;
      }
    }

    return false;
  }
}
