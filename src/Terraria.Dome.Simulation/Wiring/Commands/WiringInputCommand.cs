using System.Collections.Generic;
using Terraria.Dome.Simulation.Wiring.Components;

namespace Terraria.Dome.Simulation.Wiring.Commands;

public readonly record struct WiringInputCommand(
  long Sequence,
  PlayerHandle Player,
  int X,
  int Y,
  WireColor Color);

public sealed class WiringInputCommandComparer : IComparer<WiringInputCommand>
{
  public static WiringInputCommandComparer Instance { get; } = new();

  public int Compare(WiringInputCommand left, WiringInputCommand right)
  {
    int sequenceComparison = left.Sequence.CompareTo(right.Sequence);
    if (sequenceComparison != 0)
    {
      return sequenceComparison;
    }

    int playerComparison = left.Player.Value.CompareTo(right.Player.Value);
    if (playerComparison != 0)
    {
      return playerComparison;
    }

    int xComparison = left.X.CompareTo(right.X);
    if (xComparison != 0)
    {
      return xComparison;
    }

    int yComparison = left.Y.CompareTo(right.Y);
    if (yComparison != 0)
    {
      return yComparison;
    }

    return left.Color.CompareTo(right.Color);
  }
}
