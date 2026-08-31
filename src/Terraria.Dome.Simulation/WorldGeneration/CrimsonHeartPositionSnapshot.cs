using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class CrimsonHeartPositionSnapshot
{
  public const int MaximumCount = 100;

  public CrimsonHeartPositionSnapshot(IReadOnlyList<(int X, int Y)> positions)
  {
    ArgumentNullException.ThrowIfNull(positions);
    if (positions.Count > MaximumCount)
    {
      throw new ArgumentOutOfRangeException(nameof(positions));
    }

    Positions = new List<(int X, int Y)>(positions).AsReadOnly();
  }

  public IReadOnlyList<(int X, int Y)> Positions { get; }
}
