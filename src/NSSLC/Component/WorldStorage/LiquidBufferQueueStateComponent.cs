using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

/// <summary>
/// 保存液体待执行缓冲队列及入队容量限制。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.LiquidBuffer。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/LiquidBuffer.cs。</para>
/// <para>主要源成员：numLiquidBuffer（第 5 行）； x（第 7 行）； y（第 9 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 182 行。</para>
/// </remarks>
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
