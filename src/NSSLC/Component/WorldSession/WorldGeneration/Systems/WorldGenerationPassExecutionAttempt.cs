using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public sealed class WorldGenerationPassExecutionAttempt
{
  private WorldGenerationPassExecutionAttempt(
    WorldGenerationPassStateComponent state,
    WorldGenerationPassResultComponent? passResult,
    Exception? failure)
  {
    State = state;
    PassResult = passResult;
    Failure = failure;
  }

  public WorldGenerationPassStateComponent State { get; }

  public WorldGenerationPassResultComponent? PassResult { get; }

  public Exception? Failure { get; }

  public bool Succeeded => PassResult is not null && Failure is null;

  internal static WorldGenerationPassExecutionAttempt Completed(
    WorldGenerationPassStateComponent state,
    WorldGenerationPassResultComponent passResult)
  {
    ArgumentNullException.ThrowIfNull(passResult);
    return new WorldGenerationPassExecutionAttempt(state, passResult, failure: null);
  }

  internal static WorldGenerationPassExecutionAttempt Failed(
    WorldGenerationPassStateComponent state,
    Exception failure)
  {
    ArgumentNullException.ThrowIfNull(failure);
    return new WorldGenerationPassExecutionAttempt(state, passResult: null, failure);
  }
}
