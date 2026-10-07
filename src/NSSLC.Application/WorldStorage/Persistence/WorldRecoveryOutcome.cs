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

  /// <summary>
  /// Gets whether API execution reached commit and may have partially changed its owner. The
  /// recovery coordinator discards an unpublished candidate and resets the active world only when
  /// publication may have changed it.
  /// </summary>
  public bool RequiresWorldReset => ApiExecution?.Stage == WorldLoadApiStage.Commit;

  /// <summary>
  /// Gets whether failed execution left this candidate unsafe to reuse for a retry.
  /// </summary>
  /// <remarks>
  /// A commit failure may have changed owner state; a cleanup failure may have left prepared
  /// resources unreleased. Recovery discards an unpublished candidate and resets only a world
  /// whose publication may have changed the active state.
  /// </remarks>
  public bool RequiresCandidateDiscard =>
    RequiresWorldReset || ApiExecution?.CleanupException is not null;

  public bool CanPublishWorldLoaded =>
    (Status is WorldRecoveryStatus.Loaded or WorldRecoveryStatus.RecoveredFromBackup) &&
    ApiExecution?.Succeeded == true &&
    Failure.Kind == WorldStorageFailureKind.None;
}
