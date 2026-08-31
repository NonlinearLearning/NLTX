using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class DungeonStyleLookupEntry
{
  public DungeonStyleLookupEntry(
    DungeonStyleDefinition style,
    IReadOnlyList<DungeonStyleDefinition>? subStyles = null)
  {
    Style = style;
    if (subStyles is null || subStyles.Count == 0)
    {
      SubStyles = Array.Empty<DungeonStyleDefinition>();
      return;
    }

    DungeonStyleDefinition[] copiedSubStyles = new DungeonStyleDefinition[subStyles.Count];
    for (int index = 0; index < copiedSubStyles.Length; index++)
    {
      copiedSubStyles[index] = subStyles[index];
    }

    SubStyles = Array.AsReadOnly(copiedSubStyles);
  }

  public DungeonStyleDefinition Style { get; }

  public IReadOnlyList<DungeonStyleDefinition> SubStyles { get; }
}
