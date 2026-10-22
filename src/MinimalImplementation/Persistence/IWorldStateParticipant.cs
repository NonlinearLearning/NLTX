using System;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Persistence;

/// <summary>
/// Owner boundary used by P08 to prepare and install a world without publishing partial state.
/// </summary>
public interface IWorldStateParticipant
{
  string Name { get; }

  WorldLoadPreparation PrepareLoad(WorldLoadContext context);

  void CommitPrepared(IWorldPreparedState preparedState);

  /// <summary>
  /// Releases or rolls back a prepared state. Implementations must make this idempotent because
  /// it can be called after a participant has reached its commit boundary.
  /// </summary>
  void AbortPrepared(IWorldPreparedState preparedState);
}

public interface IWorldPreparedState
{
}

public sealed record WorldLoadPreparation(
  string ParticipantName,
  IWorldPreparedState? PreparedState,
  WorldStorageFailure Failure)
{
  public bool Succeeded => PreparedState is not null &&
    Failure.Kind == WorldStorageFailureKind.None;

  public static WorldLoadPreparation Prepared(
    string participantName,
    IWorldPreparedState preparedState)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(participantName);
    ArgumentNullException.ThrowIfNull(preparedState);
    return new(participantName, preparedState, WorldStorageFailure.None);
  }

  public static WorldLoadPreparation Rejected(
    string participantName,
    WorldStorageFailure failure)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(participantName);
    WorldStorageFailure actualFailure = failure.Kind == WorldStorageFailureKind.None
      ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
      : failure;
    return new(participantName, null, actualFailure);
  }
}
