using System;

namespace Terraria.WorldStorage;

public sealed class WorldLoadApiExecutionResult
{
  private WorldLoadApiExecutionResult(
    bool succeeded,
    WorldLoadApiStage stage,
    string? apiId,
    string? ownerId,
    WorldLoadApiFailure? failure,
    Exception? exception,
    Exception? cleanupException)
  {
    Succeeded = succeeded;
    Stage = stage;
    ApiId = apiId;
    OwnerId = ownerId;
    Failure = failure;
    Exception = exception;
    CleanupException = cleanupException;
  }

  public bool Succeeded { get; }

  public WorldLoadApiStage Stage { get; }

  public string? ApiId { get; }

  public string? OwnerId { get; }

  public WorldLoadApiFailure? Failure { get; }

  public Exception? Exception { get; }

  public Exception? CleanupException { get; }

  public static WorldLoadApiExecutionResult Completed()
  {
    return new WorldLoadApiExecutionResult(
      true,
      WorldLoadApiStage.None,
      null,
      null,
      null,
      null,
      null);
  }

  public static WorldLoadApiExecutionResult Failed(
    WorldLoadApiStage stage,
    string? apiId,
    string? ownerId,
    WorldLoadApiFailure failure,
    Exception? exception = null,
    Exception? cleanupException = null)
  {
    if (stage is not (
        WorldLoadApiStage.Binding or
        WorldLoadApiStage.Preparation or
        WorldLoadApiStage.Commit))
    {
      throw new ArgumentOutOfRangeException(nameof(stage));
    }

    if (!failure.IsValid)
    {
      throw new ArgumentException(
        "A failed execution result requires a valid failure.",
        nameof(failure));
    }

    return new WorldLoadApiExecutionResult(
      false,
      stage,
      apiId,
      ownerId,
      failure,
      exception,
      cleanupException);
  }
}
