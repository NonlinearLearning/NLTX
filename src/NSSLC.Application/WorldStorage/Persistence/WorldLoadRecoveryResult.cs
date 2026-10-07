using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldLoadRecoveryResult
{
  internal WorldLoadRecoveryResult(
    WorldLoadRecoveryAction terminalAction,
    int loadAttemptCount,
    WorldRecoveryOutcome? lastLoadOutcome,
    WorldStorageFailure failure,
    WorldStorageFailure cleanupFailure,
    bool requiresWorldReset,
    bool worldCleared)
  {
    if (!Enum.IsDefined(terminalAction))
    {
      throw new ArgumentOutOfRangeException(nameof(terminalAction));
    }

    if (terminalAction is not (
        WorldLoadRecoveryAction.NotifyWorldLoaded or
        WorldLoadRecoveryAction.ReportNoBackupFailure or
        WorldLoadRecoveryAction.ReportLoadFailure))
    {
      throw new ArgumentOutOfRangeException(nameof(terminalAction));
    }

    if (loadAttemptCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(loadAttemptCount));
    }

    if (terminalAction == WorldLoadRecoveryAction.NotifyWorldLoaded &&
        (lastLoadOutcome?.CanPublishWorldLoaded != true ||
         failure.Kind != WorldStorageFailureKind.None ||
         cleanupFailure.Kind != WorldStorageFailureKind.None ||
         requiresWorldReset))
    {
      throw new ArgumentException(
        "A completed world load requires a successful outcome and no pending cleanup.",
        nameof(lastLoadOutcome));
    }

    if (terminalAction != WorldLoadRecoveryAction.NotifyWorldLoaded &&
        failure.Kind == WorldStorageFailureKind.None)
    {
      throw new ArgumentException(
        "A failed world-load recovery requires a failure.",
        nameof(failure));
    }

    TerminalAction = terminalAction;
    LoadAttemptCount = loadAttemptCount;
    LastLoadOutcome = lastLoadOutcome;
    Failure = failure;
    CleanupFailure = cleanupFailure;
    RequiresWorldReset = requiresWorldReset;
    WorldCleared = worldCleared;
  }

  public WorldLoadRecoveryAction TerminalAction { get; }

  /// <summary>
  /// Gets the lifecycle's scheduled load-attempt count, including a scheduled attempt canceled
  /// before its file read begins.
  /// </summary>
  public int LoadAttemptCount { get; }

  public WorldRecoveryOutcome? LastLoadOutcome { get; }

  public WorldStorageFailure Failure { get; }

  public WorldStorageFailure CleanupFailure { get; }

  public bool RequiresWorldReset { get; }

  public bool WorldCleared { get; }

  public bool Succeeded => TerminalAction == WorldLoadRecoveryAction.NotifyWorldLoaded;
}
