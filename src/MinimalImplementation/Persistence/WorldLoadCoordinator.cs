using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Persistence;

/// <summary>
/// Reads a detached document and runs all owner prepare/commit operations on one commit boundary.
/// It returns an unpublished candidate; host admission and active-world publication remain outside.
/// </summary>
public sealed class WorldLoadCoordinator
{
  private readonly WorldDocumentRecoveryLoader _loader;
  private readonly IWorldCommitQueue _commitQueue;
  private readonly IWorldStateParticipant[] _participants;
  private readonly Func<long> _candidateScopeFactory;

  public WorldLoadCoordinator(
    WorldDocumentRecoveryLoader loader,
    IWorldCommitQueue commitQueue,
    IReadOnlyCollection<IWorldStateParticipant> participants,
    Func<long> candidateScopeFactory)
  {
    ArgumentNullException.ThrowIfNull(loader);
    ArgumentNullException.ThrowIfNull(commitQueue);
    ArgumentNullException.ThrowIfNull(participants);
    ArgumentNullException.ThrowIfNull(candidateScopeFactory);
    if (participants.Count == 0)
    {
      throw new ArgumentException("At least one world participant is required.", nameof(participants));
    }

    if (participants.Any(participant => participant is null))
    {
      throw new ArgumentException("World participants cannot contain null values.", nameof(participants));
    }

    if (participants.Select(participant => participant.Name).Distinct(StringComparer.Ordinal).Count() !=
        participants.Count)
    {
      throw new ArgumentException("World participant names must be unique.", nameof(participants));
    }

    _loader = loader;
    _commitQueue = commitQueue;
    _participants = participants.ToArray();
    _candidateScopeFactory = candidateScopeFactory;
  }

  public async Task<WorldLoadResult> LoadCandidateAsync(
    string primaryPath,
    int maximumFileBytes,
    CancellationToken cancellationToken = default)
  {
    WorldDocumentRecoveryResult recovery = await _loader.ReadBestAvailableAsync(
      primaryPath,
      maximumFileBytes,
      cancellationToken).ConfigureAwait(false);
    if (!recovery.Succeeded || recovery.Document is null)
    {
      WorldStorageFailure failure = recovery.PrimaryFailure.Kind != WorldStorageFailureKind.None
        ? recovery.PrimaryFailure
        : recovery.BackupFailure;
      return WorldLoadResult.Rejected(
        recovery,
        candidateWorldScope: 0,
        WorldLoadStage.Read,
        Array.Empty<string>(),
        failure.Kind == WorldStorageFailureKind.None
          ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
          : failure);
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return WorldLoadResult.Rejected(
        recovery,
        candidateWorldScope: 0,
        WorldLoadStage.Read,
        Array.Empty<string>(),
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled));
    }

    long candidateWorldScope;
    try
    {
      candidateWorldScope = _candidateScopeFactory();
      if (candidateWorldScope <= 0)
      {
        throw new InvalidDataException("The candidate world scope must be positive.");
      }
    }
    catch (Exception exception) when (IsBoundaryFailure(exception))
    {
      return WorldLoadResult.Rejected(
        recovery,
        candidateWorldScope: 0,
        WorldLoadStage.Prepare,
        Array.Empty<string>(),
        Classify(exception));
    }

    var execution = new LoadExecution(_participants);
    try
    {
      await _commitQueue.ExecuteAsync(
        () => ExecuteOnCommitBoundary(execution, candidateWorldScope, recovery.Document, cancellationToken),
        cancellationToken).ConfigureAwait(false);
    }
    catch (Exception exception) when (IsBoundaryFailure(exception))
    {
      return WorldLoadResult.Rejected(
        recovery,
        candidateWorldScope,
        execution.Stage == WorldLoadStage.None ? WorldLoadStage.Prepare : execution.Stage,
        execution.PreparedParticipantNames,
        Classify(exception),
        execution.CleanupFailure);
    }

    if (execution.Failure.Kind != WorldStorageFailureKind.None)
    {
      return WorldLoadResult.Rejected(
        recovery,
        candidateWorldScope,
        execution.Stage,
        execution.PreparedParticipantNames,
        execution.Failure,
        execution.CleanupFailure);
    }

    return WorldLoadResult.Candidate(
      recovery,
      candidateWorldScope,
      execution.PreparedParticipantNames);
  }

  private static void ExecuteOnCommitBoundary(
    LoadExecution execution,
    long candidateWorldScope,
    WorldPersistenceDocument document,
    CancellationToken cancellationToken)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();
      var context = new WorldLoadContext(candidateWorldScope, document, cancellationToken);
      foreach (IWorldStateParticipant participant in execution.Participants)
      {
        cancellationToken.ThrowIfCancellationRequested();
        WorldLoadPreparation preparation = participant.PrepareLoad(context);
        if (!string.Equals(
              preparation.ParticipantName,
              participant.Name,
              StringComparison.Ordinal))
        {
          throw new InvalidDataException(
            $"Participant '{participant.Name}' returned preparation for " +
            $"'{preparation.ParticipantName}'.");
        }

        if (!preparation.Succeeded)
        {
          execution.Stage = WorldLoadStage.Prepare;
          execution.Failure = preparation.Failure.Kind == WorldStorageFailureKind.None
            ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
            : preparation.Failure;
          AbortPrepared(execution);
          return;
        }

        execution.Prepared.Add(new PreparedParticipant(participant, preparation.PreparedState!));
      }

      execution.Stage = WorldLoadStage.Commit;
      foreach (PreparedParticipant prepared in execution.Prepared)
      {
        cancellationToken.ThrowIfCancellationRequested();
        prepared.Participant.CommitPrepared(prepared.State);
      }

      execution.Stage = WorldLoadStage.CandidateReady;
    }
    catch (Exception exception) when (IsBoundaryFailure(exception))
    {
      if (execution.Failure.Kind == WorldStorageFailureKind.None)
      {
        execution.Failure = Classify(exception);
      }

      if (execution.Stage == WorldLoadStage.None)
      {
        execution.Stage = WorldLoadStage.Prepare;
      }

      AbortPrepared(execution);
    }
  }

  private static void AbortPrepared(LoadExecution execution)
  {
    execution.Stage = execution.Stage == WorldLoadStage.Prepare
      ? WorldLoadStage.Abort
      : execution.Stage;
    for (int index = execution.Prepared.Count - 1; index >= 0; index--)
    {
      try
      {
        PreparedParticipant prepared = execution.Prepared[index];
        prepared.Participant.AbortPrepared(prepared.State);
      }
      catch (Exception exception) when (IsBoundaryFailure(exception))
      {
        execution.CleanupFailure = execution.CleanupFailure.Kind == WorldStorageFailureKind.None
          ? Classify(exception)
          : WorldStorageFailure.Create(
            WorldStorageFailureKind.Unknown,
            $"Multiple candidate cleanup failures; latest: {exception.Message}");
      }
    }
  }

  private static WorldStorageFailure Classify(Exception exception)
  {
    WorldStorageFailureKind kind = exception switch
    {
      OperationCanceledException => WorldStorageFailureKind.Canceled,
      UnauthorizedAccessException or System.Security.SecurityException =>
        WorldStorageFailureKind.PermissionDenied,
      FileNotFoundException or DirectoryNotFoundException => WorldStorageFailureKind.Missing,
      InvalidDataException => WorldStorageFailureKind.InvalidData,
      IOException => WorldStorageFailureKind.IoFailure,
      ArgumentException or NotSupportedException => WorldStorageFailureKind.InvalidPath,
      _ => WorldStorageFailureKind.Unknown
    };
    return WorldStorageFailure.Create(kind, exception.Message);
  }

  private static bool IsBoundaryFailure(Exception exception) => exception is
    OperationCanceledException or IOException or UnauthorizedAccessException or
    ArgumentException or InvalidDataException or NotSupportedException or
    System.Security.SecurityException;

  private sealed class LoadExecution
  {
    public LoadExecution(IWorldStateParticipant[] participants)
    {
      Participants = participants;
    }

    public IWorldStateParticipant[] Participants { get; }

    public List<PreparedParticipant> Prepared { get; } = [];

    public WorldLoadStage Stage { get; set; }

    public WorldStorageFailure Failure { get; set; } = WorldStorageFailure.None;

    public WorldStorageFailure CleanupFailure { get; set; } = WorldStorageFailure.None;

    public IReadOnlyList<string> PreparedParticipantNames => Prepared
      .Select(item => item.Participant.Name)
      .ToArray();
  }

  private readonly record struct PreparedParticipant(
    IWorldStateParticipant Participant,
    IWorldPreparedState State);
}
