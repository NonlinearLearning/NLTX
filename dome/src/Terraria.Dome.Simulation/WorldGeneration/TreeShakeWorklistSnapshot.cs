using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class TreeShakeWorklistSnapshot
{
  public const int MaximumCount = 500;

  public TreeShakeWorklistSnapshot(IReadOnlyList<TreeShakeWorkItem> items)
  {
    ArgumentNullException.ThrowIfNull(items);
    if (items.Count > MaximumCount)
    {
      throw new ArgumentOutOfRangeException(nameof(items));
    }

    Items = new List<TreeShakeWorkItem>(items).AsReadOnly();
  }

  public IReadOnlyList<TreeShakeWorkItem> Items { get; }
}
