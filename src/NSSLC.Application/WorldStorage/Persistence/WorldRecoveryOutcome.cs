using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldRecoveryOutcome
{
  public WorldRecoveryOutcome(
    WorldRecoveryStatus status,
    int attempts,
    WorldStorageFailure failure,
    WorldLoadApiExecutionResult? apiExecution = null)
  {
    if (!Enum.IsDefined(status))
    {
      throw new ArgumentOutOfRangeException(nameof(status));
    }

    if (attempts < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(attempts));
    }

    bool reportsSuccess = status is WorldRecoveryStatus.Loaded or
      WorldRecoveryStatus.RecoveredFromBackup;
    if (reportsSuccess &&
        (failure.Kind != WorldStorageFailureKind.None ||
         apiExecution is null ||
         !apiExecution.Succeeded))
    {
      throw new ArgumentException(
        "A successful recovery outcome requires a successful API execution and no failure.",
        nameof(failure));
    }

    if (!reportsSuccess && apiExecution?.Succeeded == true)
    {
      throw new ArgumentException(
        "A failed recovery outcome cannot contain a successful API execution.",
        nameof(apiExecution));
    }

    if (!reportsSuccess && failure.Kind == WorldStorageFailureKind.None)
    {
      throw new ArgumentException(
        "A failed recovery outcome requires a failure.",
        nameof(failure));
    }

    Status = status;
    Attempts = attempts;
    Failure = failure;
    ApiExecution = apiExecution;
  }

  public WorldRecoveryStatus Status { get; }

  /// <summary>
  /// Gets the number of file reads performed by this coordinator call, not lifecycle attempts.
  /// </summary>
  public int Attempts { get; }

  public WorldStorageFailure Failure { get; }

  public WorldLoadApiExecutionResult? ApiExecution { get; }

  public bool RequiresWorldReset => ApiExecution?.Stage == WorldLoadApiStage.Commit;

  public bool CanPublishWorldLoaded =>
    (Status is WorldRecoveryStatus.Loaded or WorldRecoveryStatus.RecoveredFromBackup) &&
    ApiExecution?.Succeeded == true &&
    Failure.Kind == WorldStorageFailureKind.None;
}
