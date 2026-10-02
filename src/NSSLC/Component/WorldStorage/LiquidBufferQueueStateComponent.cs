using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

public sealed class LiquidBufferQueueStateComponent
{
  public const int DefaultReservedSlots = 2;

  private readonly Queue<LiquidBufferEntry> _entries = new();
  private readonly HashSet<TileCoordinate> _scheduledCoordinates = new();

  public LiquidBufferQueueStateComponent(
    LiquidFlowBudgetPolicy policy,
    int reservedSlots = DefaultReservedSlots)
  {
    ArgumentNullException.ThrowIfNull(policy);
    if (reservedSlots < 0 || policy.MaximumBufferLength <= reservedSlots)
    {
      throw new ArgumentOutOfRangeException(nameof(reservedSlots));
    }

    MaximumLength = policy.MaximumBufferLength;
    ReservedSlots = reservedSlots;
  }

  public int MaximumLength { get; }

  public int ReservedSlots { get; }

  public int MaximumEnqueueCount => MaximumLength - ReservedSlots;

  public int Count => _entries.Count;

  public bool HasCapacity => Count < MaximumEnqueueCount;

  public IReadOnlyList<LiquidBufferEntry> Snapshot()
  {
    return new List<LiquidBufferEntry>(_entries).AsReadOnly();
  }

  internal bool Contains(TileCoordinate coordinate)
  {
    return _scheduledCoordinates.Contains(coordinate);
  }

  internal bool TryEnqueue(TileCoordinate coordinate)
  {
    if (_scheduledCoordinates.Contains(coordinate) || !HasCapacity)
    {
      return false;
    }

    _scheduledCoordinates.Add(coordinate);
    _entries.Enqueue(new LiquidBufferEntry(coordinate));
    return true;
  }

  internal bool TryRelease(
    TileCoordinate coordinate,
    out LiquidBufferEntry releasedEntry)
  {
    releasedEntry = default;
    if (_entries.Count == 0 || _entries.Peek().Coordinate != coordinate)
    {
      return false;
    }

    releasedEntry = _entries.Dequeue();
    _scheduledCoordinates.Remove(coordinate);
    return true;
  }

  internal bool TryPeek(out LiquidBufferEntry entry)
  {
    if (_entries.Count == 0)
    {
      entry = default;
      return false;
    }

    entry = _entries.Peek();
    return true;
  }

  internal bool Reset()
  {
    bool changed = _entries.Count != 0 || _scheduledCoordinates.Count != 0;
    _entries.Clear();
    _scheduledCoordinates.Clear();
    return changed;
  }
}
