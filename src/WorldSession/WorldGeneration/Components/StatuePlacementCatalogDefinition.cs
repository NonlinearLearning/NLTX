using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public sealed class StatuePlacementCatalogDefinition
{
  public StatuePlacementCatalogDefinition(IReadOnlyList<StatuePlacementOption> options)
  {
    ArgumentNullException.ThrowIfNull(options);
    if (options.Count == 0)
    {
      throw new ArgumentException("The statue placement catalog cannot be empty.", nameof(options));
    }

    StatuePlacementOption[] copy = new StatuePlacementOption[options.Count];
    for (int index = 0; index < options.Count; index++)
    {
      copy[index] = options[index];
    }

    Options = Array.AsReadOnly(copy);
  }

  public IReadOnlyList<StatuePlacementOption> Options { get; }
}
