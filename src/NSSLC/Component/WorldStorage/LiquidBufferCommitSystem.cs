using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

public sealed class LiquidBufferCommitSystem
{
  private readonly ILiquidBufferCommitPort _bufferPort;
  private readonly ILiquidWorkQueueCommitPort _activeQueuePort;

  public LiquidBufferCommitSystem(
    ILiquidBufferCommitPort bufferPort,
    ILiquidWorkQueueCommitPort activeQueuePort)
  {
    ArgumentNullException.ThrowIfNull(bufferPort);
    ArgumentNullException.ThrowIfNull(activeQueuePort);
    _bufferPort = bufferPort;
    _activeQueuePort = activeQueuePort;
  }

  public LiquidBufferCommitResult Enqueue(
    int x,
    int y,
    int worldWidth,
    int worldHeight,
    bool tileIsCheckingLiquid)
  {
    ValidateWorldDimensions(worldWidth, worldHeight);
    if (!IsWithinBounds(x, y, worldWidth, worldHeight))
    {
      return LiquidBufferCommitResult.Rejected("bounds");
    }

    LiquidBufferCommand command = LiquidBufferCommand.Enqueue(x, y);
    return _bufferPort.Commit(in command, tileIsCheckingLiquid);
  }

  public LiquidBufferDrainResult DrainNext(
    int worldWidth,
    int worldHeight)
  {
    ValidateWorldDimensions(worldWidth, worldHeight);
    if (!_bufferPort.TryPeek(out LiquidBufferEntry entry))
    {
      return LiquidBufferDrainResult.Empty;
    }

    TileCoordinate coordinate = entry.Coordinate;
    if (!IsWithinBounds(
      coordinate.X,
      coordinate.Y,
      worldWidth,
      worldHeight))
    {
      return LiquidBufferDrainResult.Rejected(
        entry,
        "bounds",
        LiquidWorkQueueOperationResult.Rejected("bounds"));
    }

    LiquidCellWorkItemStateCommand activeCommand =
      new(coordinate.X, coordinate.Y, kill: 0, delay: 0);
    LiquidWorkQueueOperationResult activeQueueResult =
      _activeQueuePort.Enqueue(in activeCommand);
    if (!activeQueueResult.Succeeded)
    {
      return LiquidBufferDrainResult.Rejected(
        entry,
        activeQueueResult.FailureReason ?? "active-queue-rejected",
        activeQueueResult);
    }

    LiquidBufferCommand releaseCommand = LiquidBufferCommand.Release(
      coordinate.X,
      coordinate.Y);
    LiquidBufferCommitResult releaseResult = _bufferPort.Commit(
      in releaseCommand,
      tileIsCheckingLiquid: true);
    if (!releaseResult.Succeeded)
    {
      return LiquidBufferDrainResult.StateConflict(
        entry,
        activeCommand,
        activeQueueResult,
        releaseResult.FailureReason ?? "buffer-release-failed");
    }

    return LiquidBufferDrainResult.Enqueued(
      entry,
      activeCommand,
      activeQueueResult,
      releaseResult.CheckingIntent);
  }

  public LiquidBufferCommitResult Reset()
  {
    LiquidBufferCommand command = LiquidBufferCommand.Reset();
    return _bufferPort.Commit(in command, tileIsCheckingLiquid: false);
  }

  public IReadOnlyList<LiquidBufferEntry> Snapshot()
  {
    return _bufferPort.Snapshot();
  }

  private static bool IsWithinBounds(
    int x,
    int y,
    int worldWidth,
    int worldHeight)
  {
    return x >= 0 && x < worldWidth && y >= 0 && y < worldHeight;
  }

  private static void ValidateWorldDimensions(
    int worldWidth,
    int worldHeight)
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
