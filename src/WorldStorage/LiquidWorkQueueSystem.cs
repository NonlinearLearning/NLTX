using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

public sealed class LiquidWorkQueueSystem
{
  private readonly ILiquidWorkQueueCommitPort _commitPort;
  private readonly int _killThreshold;

  public LiquidWorkQueueSystem(
    ILiquidWorkQueueCommitPort commitPort,
    int killThreshold = 8)
  {
    ArgumentNullException.ThrowIfNull(commitPort);
    if (killThreshold < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(killThreshold));
    }

    _commitPort = commitPort;
    _killThreshold = killThreshold;
  }

  public LiquidWorkQueueOperationResult Enqueue(
    in LiquidCellWorkItemStateCommand command,
    int worldWidth,
    int worldHeight)
  {
    if (!LiquidWorkItemReadyQuery.IsWithinBounds(
      command,
      worldWidth,
      worldHeight))
    {
      return LiquidWorkQueueOperationResult.Rejected("bounds");
    }

    return _commitPort.Enqueue(in command);
  }

  public LiquidWorkQueueDrainResult DrainNext(
    int worldWidth,
    int worldHeight)
  {
    ValidateWorldDimensions(worldWidth, worldHeight);
    IReadOnlyList<LiquidCellWorkItemStateCommand> snapshot =
      _commitPort.Snapshot();
    if (snapshot.Count == 0)
    {
      return LiquidWorkQueueDrainResult.Empty;
    }

    LiquidCellWorkItemStateCommand command = snapshot[0];
    if (!LiquidWorkItemReadyQuery.IsWithinBounds(
      command,
      worldWidth,
      worldHeight))
    {
      return LiquidWorkQueueDrainResult.Rejected("bounds");
    }

    LiquidWorkItemReadiness readiness = LiquidWorkItemReadyQuery.Evaluate(
      in command,
      _killThreshold);
    if (readiness.ShouldRemove)
    {
      LiquidWorkQueueOperationResult removeResult = _commitPort.Remove(
        command.Coordinate);
      return removeResult.Succeeded
        ? LiquidWorkQueueDrainResult.Removed(command)
        : LiquidWorkQueueDrainResult.Rejected(
          removeResult.FailureReason ?? "remove-failed");
    }

    if (readiness.ShouldDecrementDelay)
    {
      LiquidCellWorkItemStateCommand deferred =
        LiquidWorkItemReadyQuery.DecrementDelay(in command);
      LiquidWorkQueueOperationResult requeueResult = _commitPort.Requeue(
        in deferred);
      return requeueResult.Succeeded
        ? LiquidWorkQueueDrainResult.Deferred(deferred)
        : LiquidWorkQueueDrainResult.Rejected(
          requeueResult.FailureReason ?? "requeue-failed");
    }

    LiquidWorkQueueOperationResult dequeueResult = _commitPort.Remove(
      command.Coordinate);
    return dequeueResult.Succeeded
      ? LiquidWorkQueueDrainResult.Ready(command)
      : LiquidWorkQueueDrainResult.Rejected(
        dequeueResult.FailureReason ?? "dequeue-failed");
  }

  public LiquidWorkQueueOperationResult Reset()
  {
    return _commitPort.Reset();
  }

  public IReadOnlyList<LiquidCellWorkItemStateCommand> Snapshot()
  {
    return _commitPort.Snapshot();
  }

  private static void ValidateWorldDimensions(int worldWidth, int worldHeight)
  {
    if (worldWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    if (worldHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldHeight));
    }
  }
}
