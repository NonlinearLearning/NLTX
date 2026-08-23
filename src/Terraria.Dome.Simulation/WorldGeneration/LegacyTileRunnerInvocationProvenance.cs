using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerInvocationProvenance(
  string PassName,
  string RecipeName,
  int InvocationIndex,
  int SourceLine,
  LegacyTileRunnerRequest Request,
  LegacyTileRunnerCommandBatch Commands);

public static class LegacyTileRunnerInvocationProvenanceFactory
{
  public static LegacyTileRunnerInvocationProvenance Create(
    LegacyTileRunnerPassInvocation invocation,
    LegacyTileRunnerCommandBatch commands,
    int invocationIndex,
    int sourceLine)
  {
    ArgumentNullException.ThrowIfNull(invocation);
    ArgumentNullException.ThrowIfNull(commands);
    if (invocationIndex < 0 || sourceLine <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(invocationIndex));
    }

    return new LegacyTileRunnerInvocationProvenance(
      invocation.Recipe.PassName,
      invocation.Recipe.RecipeName,
      invocationIndex,
      sourceLine,
      invocation.Request,
      commands);
  }
}
