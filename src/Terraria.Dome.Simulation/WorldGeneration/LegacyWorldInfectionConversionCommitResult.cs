using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyWorldInfectionConversionCommitResult(
  bool Succeeded,
  int AppliedTileCount,
  int AppliedWallCount,
  int NoOpCount,
  IReadOnlyList<LegacyWorldInfectionConversionDeferred> Deferred,
  string? FailureReason)
{
  public int AppliedCount => AppliedTileCount + AppliedWallCount;

  public int DeferredCount => Deferred.Count;

  public static LegacyWorldInfectionConversionCommitResult Failed(string reason)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reason);
    return new LegacyWorldInfectionConversionCommitResult(
      false,
      0,
      0,
      0,
      Array.Empty<LegacyWorldInfectionConversionDeferred>(),
      reason);
  }
}
