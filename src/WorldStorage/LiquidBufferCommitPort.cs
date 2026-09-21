using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

public sealed class LiquidBufferCommitPort : ILiquidBufferCommitPort
{
  private readonly LiquidBufferQueueStateComponent _state;

  public LiquidBufferCommitPort(LiquidBufferQueueStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    _state = state;
  }

  public LiquidBufferCommitResult Commit(
    in LiquidBufferCommand command,
    bool tileIsCheckingLiquid)
  {
    return command.Kind switch
    {
      LiquidBufferCommandKind.Enqueue => Enqueue(
        command,
        tileIsCheckingLiquid),
      LiquidBufferCommandKind.Release => Release(command),
      LiquidBufferCommandKind.Reset => LiquidBufferCommitResult.Reset(
        _state.Reset()),
      _ => LiquidBufferCommitResult.Rejected("invalid-command")
    };
  }

  public bool TryPeek(out LiquidBufferEntry entry)
  {
    return _state.TryPeek(out entry);
  }

  public IReadOnlyList<LiquidBufferEntry> Snapshot()
  {
    return _state.Snapshot();
  }

  private LiquidBufferCommitResult Enqueue(
    in LiquidBufferCommand command,
    bool tileIsCheckingLiquid)
  {
    TileCoordinate coordinate = command.Coordinate;
    if (tileIsCheckingLiquid || _state.Contains(coordinate))
    {
      return LiquidBufferCommitResult.Duplicate(coordinate);
    }

    if (!_state.HasCapacity)
    {
      return LiquidBufferCommitResult.Rejected("capacity");
    }

    _state.TryEnqueue(coordinate);
    return LiquidBufferCommitResult.Accepted(coordinate);
  }

  private LiquidBufferCommitResult Release(
    in LiquidBufferCommand command)
  {
    TileCoordinate coordinate = command.Coordinate;
    return _state.TryRelease(coordinate, out _)
      ? LiquidBufferCommitResult.Released(coordinate)
      : LiquidBufferCommitResult.Rejected("not-head");
  }
}
