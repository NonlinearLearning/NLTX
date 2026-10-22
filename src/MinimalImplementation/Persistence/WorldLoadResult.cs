using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Persistence;

/// <summary>Stable result for a candidate load; the caller publishes it only after readiness.</summary>
public sealed class WorldLoadResult
{
  private WorldLoadResult(
    WorldDocumentRecoveryResult recovery,
    long candidateWorldScope,
    WorldLoadStage stage,
    IReadOnlyList<string> preparedParticipants,
    WorldStorageFailure failure,
    WorldStorageFailure cleanupFailure)
  {
    Recovery = recovery;
    CandidateWorldScope = candidateWorldScope;
    Stage = stage;
    PreparedParticipants = Array.AsReadOnly(preparedParticipants.ToArray());
    Failure = failure;
    CleanupFailure = cleanupFailure;
  }

  public WorldDocumentRecoveryResult Recovery { get; }

  public long CandidateWorldScope { get; }

  public WorldLoadStage Stage { get; }

  public IReadOnlyList<string> PreparedParticipants { get; }

  public WorldStorageFailure Failure { get; }

  public WorldStorageFailure CleanupFailure { get; }

  public bool CandidateReady => Stage == WorldLoadStage.CandidateReady &&
    Failure.Kind == WorldStorageFailureKind.None;

  public static WorldLoadResult Candidate(
    WorldDocumentRecoveryResult recovery,
    long candidateWorldScope,
    IReadOnlyList<string> preparedParticipants)
  {
    ArgumentNullException.ThrowIfNull(recovery);
    ArgumentNullException.ThrowIfNull(preparedParticipants);
    return new(
      recovery,
      candidateWorldScope,
      WorldLoadStage.CandidateReady,
      preparedParticipants,
      WorldStorageFailure.None,
      WorldStorageFailure.None);
  }

  public static WorldLoadResult Rejected(
    WorldDocumentRecoveryResult recovery,
    long candidateWorldScope,
    WorldLoadStage stage,
    IReadOnlyList<string> preparedParticipants,
    WorldStorageFailure failure,
    WorldStorageFailure cleanupFailure = default)
  {
    ArgumentNullException.ThrowIfNull(recovery);
    ArgumentNullException.ThrowIfNull(preparedParticipants);
    if (stage == WorldLoadStage.CandidateReady)
    {
      throw new ArgumentOutOfRangeException(nameof(stage));
    }

    WorldStorageFailure actualFailure = failure.Kind == WorldStorageFailureKind.None
      ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
      : failure;
    return new(
      recovery,
      candidateWorldScope,
      stage,
      preparedParticipants,
      actualFailure,
      cleanupFailure);
  }
}
