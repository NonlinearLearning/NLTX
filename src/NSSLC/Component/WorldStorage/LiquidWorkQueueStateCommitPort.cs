using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

public sealed class LiquidWorkQueueStateCommitPort : ILiquidWorkQueueCommitPort
{
  private const int MaximumRepresentableWorkValue = byte.MaxValue;

  private readonly LiquidWorkQueueState _state;

  public LiquidWorkQueueStateCommitPort(LiquidWorkQueueState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (state.Capacity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(state.Capacity));
    }

    _state = state;
  }

  public LiquidWorkQueueOperationResult Enqueue(
    in LiquidCellWorkItemStateCommand command)
  {
    LiquidWorkQueueOperationResult validation = ValidateRepresentable(command);
    if (!validation.Succeeded)
    {
      return validation;
    }

    if (!_state.ScheduledCoordinates.Add(command.Coordinate))
    {
      return LiquidWorkQueueOperationResult.Duplicate;
    }

    if (!_state.HasCapacity)
    {
      _state.ScheduledCoordinates.Remove(command.Coordinate);
      return LiquidWorkQueueOperationResult.Rejected("capacity");
    }

    _state.ActiveEntries.Enqueue(ToEntry(command));
    return LiquidWorkQueueOperationResult.Accepted(changed: true);
  }

  public LiquidWorkQueueOperationResult Requeue(
    in LiquidCellWorkItemStateCommand command)
  {
    LiquidWorkQueueOperationResult validation = ValidateRepresentable(command);
    if (!validation.Succeeded)
    {
      return validation;
    }

    if (!_state.ScheduledCoordinates.Contains(command.Coordinate))
    {
      return LiquidWorkQueueOperationResult.Rejected("not-scheduled");
    }

    List<LiquidWorkEntry> replacement = new(_state.ActiveEntries.Count);
    bool replaced = false;
    foreach (LiquidWorkEntry entry in _state.ActiveEntries)
    {
      if (entry.Coordinate != command.Coordinate)
      {
        replacement.Add(entry);
        continue;
      }

      if (!replaced)
      {
        replacement.Add(ToEntry(command));
        replaced = true;
      }
    }

    if (!replaced)
    {
      return LiquidWorkQueueOperationResult.Rejected("state-mismatch");
    }

    _state.ActiveEntries.Clear();
    foreach (LiquidWorkEntry entry in replacement)
    {
      _state.ActiveEntries.Enqueue(entry);
    }

    return LiquidWorkQueueOperationResult.Accepted(changed: true);
  }

  public LiquidWorkQueueOperationResult Remove(TileCoordinate coordinate)
  {
    bool removedFromIndex = _state.ScheduledCoordinates.Remove(coordinate);
    List<LiquidWorkEntry> remaining = new(_state.ActiveEntries.Count);
    bool removedFromQueue = false;
    foreach (LiquidWorkEntry entry in _state.ActiveEntries)
    {
      if (entry.Coordinate == coordinate)
      {
        removedFromQueue = true;
        continue;
      }

      remaining.Add(entry);
    }

    if (!removedFromIndex && !removedFromQueue)
    {
      return LiquidWorkQueueOperationResult.Rejected("not-found");
    }

    _state.ActiveEntries.Clear();
    foreach (LiquidWorkEntry entry in remaining)
    {
      _state.ActiveEntries.Enqueue(entry);
    }

    return LiquidWorkQueueOperationResult.Removed;
  }

  public LiquidWorkQueueOperationResult Reset()
  {
    bool changed = _state.ActiveEntries.Count > 0 ||
      _state.BufferedEntries.Count > 0 ||
      _state.ScheduledCoordinates.Count > 0;
    _state.ActiveEntries.Clear();
    _state.BufferedEntries.Clear();
    _state.ScheduledCoordinates.Clear();
    return LiquidWorkQueueOperationResult.Accepted(changed);
  }

  public IReadOnlyList<LiquidCellWorkItemStateCommand> Snapshot()
  {
    List<LiquidCellWorkItemStateCommand> snapshot = new(_state.ActiveEntries.Count);
    foreach (LiquidWorkEntry entry in _state.ActiveEntries)
    {
      snapshot.Add(new LiquidCellWorkItemStateCommand(
        entry.Coordinate.X,
        entry.Coordinate.Y,
        entry.KillCount,
        entry.DelayTicks));
    }

    return snapshot.AsReadOnly();
  }

  private static LiquidWorkQueueOperationResult ValidateRepresentable(
    in LiquidCellWorkItemStateCommand command)
  {
    return command.Kill > MaximumRepresentableWorkValue ||
      command.Delay > MaximumRepresentableWorkValue
      ? LiquidWorkQueueOperationResult.Rejected("byte-range")
      : LiquidWorkQueueOperationResult.Accepted(changed: false);
  }

  private static LiquidWorkEntry ToEntry(
    in LiquidCellWorkItemStateCommand command)
  {
    return new LiquidWorkEntry(
      command.Coordinate,
      (byte)command.Delay,
      (byte)command.Kill);
  }
}
