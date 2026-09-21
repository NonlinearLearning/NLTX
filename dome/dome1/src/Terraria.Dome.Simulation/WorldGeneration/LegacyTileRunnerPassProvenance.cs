using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerPassProvenance(
  string PassName,
  IReadOnlyList<LegacyTileRunnerInvocationProvenance> Invocations,
  int TileCommandCount,
  int LiquidCommandCount);

public static class LegacyTileRunnerPassProvenanceFactory
{
  public static LegacyTileRunnerPassProvenance Create(
    string passName,
    IReadOnlyCollection<LegacyTileRunnerInvocationProvenance> invocations)
  {
    ArgumentException.ThrowIfNullOrEmpty(passName);
    ArgumentNullException.ThrowIfNull(invocations);
    List<LegacyTileRunnerInvocationProvenance> ordered = invocations
      .OrderBy(invocation => invocation.InvocationIndex)
      .ToList();
    for (int index = 0; index < ordered.Count; index++)
    {
      LegacyTileRunnerInvocationProvenance invocation = ordered[index];
      if (invocation is null || invocation.PassName != passName ||
          invocation.InvocationIndex != index ||
          (index > 0 && invocation.SourceLine < ordered[index - 1].SourceLine))
      {
        throw new ArgumentException(
          "TileRunner pass provenance was not contiguous or belonged to another pass.",
          nameof(invocations));
      }
    }

    return new LegacyTileRunnerPassProvenance(
      passName,
      ordered.AsReadOnly(),
      ordered.Count(invocation => invocation.Commands.TileCommand.HasValue),
      ordered.Count(invocation => invocation.Commands.LiquidCommand.HasValue));
  }
}
