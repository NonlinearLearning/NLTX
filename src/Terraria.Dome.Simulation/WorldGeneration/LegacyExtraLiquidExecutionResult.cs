using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyExtraLiquidExecutionResult(
  bool Succeeded,
  int AppliedTileCount,
  int AppliedLiquidCount,
  long NextSequence,
  IReadOnlyList<LegacyExtraLiquidBubbleSquare> Bubbles,
  string? FailureReason)
{
  public int BubbleCount => Bubbles.Count;

  public static LegacyExtraLiquidExecutionResult Failed(
    long nextSequence,
    string failureReason)
  {
    return new LegacyExtraLiquidExecutionResult(
      false,
      0,
      0,
      nextSequence,
      Array.Empty<LegacyExtraLiquidBubbleSquare>(),
      failureReason);
  }
}
