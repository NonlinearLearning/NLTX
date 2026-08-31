using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class TreeTopStyleSnapshot
{
  public const int AreaCount = 13;

  public TreeTopStyleSnapshot(IReadOnlyList<int> styles)
  {
    ArgumentNullException.ThrowIfNull(styles);
    if (styles.Count != AreaCount)
    {
      throw new ArgumentException("Exactly 13 tree-top styles are required.", nameof(styles));
    }

    Styles = new List<int>(styles).AsReadOnly();
  }

  public IReadOnlyList<int> Styles { get; }
}
