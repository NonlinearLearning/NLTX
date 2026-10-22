using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

/// <summary>
/// 保存有容量限制且按方块坐标去重的液体缓冲队列。
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
/// <para>
/// 用途：暂时不能进入主液体工作集合的坐标先进入缓冲队列，调度流程再按入队顺序转移。
/// </para>
/// <para>
/// 状态变化：TryEnqueue 在坐标未重复且有剩余容量时入队；TryRelease 仅允许释放与队首坐标一致的记录。成功释放同时移除去重标记。
/// </para>
/// <para>
/// 当前实现：最大可入队数量等于策略容量减预留槽位，默认预留 2。Snapshot 复制队列，Reset 清空队列与去重表并返回是否改变状态；实际液体求解由外部流程执行。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C206，L2（通用可选玩法）。分级理由：液体缓冲、排序、工作项、脏区块、预算和流动状态只在启用液体传播时需要。
/// </para>
/// </remarks>
public sealed class LiquidBufferQueueStateComponent
{
  /// <summary>
  /// 默认预留槽位数 2。
  /// </summary>
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

  /// <summary>
  /// 策略指定的液体缓冲总容量。
  /// </summary>
  public int MaximumLength { get; }

  /// <summary>
  /// 不供普通入队使用的预留槽位数。
  /// </summary>
  public int ReservedSlots { get; }

  /// <summary>
  /// 普通入队上限，等于总容量减预留槽位。
  /// </summary>
  public int MaximumEnqueueCount => MaximumLength - ReservedSlots;

  /// <summary>
  /// 当前缓冲队列记录数。
  /// </summary>
  public int Count => _entries.Count;

  /// <summary>
  /// 当前记录数小于普通入队上限时返回 true。
  /// </summary>
  public bool HasCapacity => Count < MaximumEnqueueCount;

  /// <summary>
  /// 按队列顺序复制记录并返回只读快照。
  /// </summary>
  public IReadOnlyList<LiquidBufferEntry> Snapshot()
  {
    return new List<LiquidBufferEntry>(_entries).AsReadOnly();
  }

  /// <summary>
  /// 查询坐标是否已登记在缓冲队列的去重集合中。
  /// </summary>
  internal bool Contains(TileCoordinate coordinate)
  {
    return _scheduledCoordinates.Contains(coordinate);
  }

  /// <summary>
  /// 坐标不重复且有容量时入队，成功返回 true。
  /// </summary>
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

  /// <summary>
  /// 仅在指定坐标与队首一致时出队并删除去重标记。
  /// </summary>
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

  /// <summary>
  /// 读取队首而不出队；空队列返回 false。
  /// </summary>
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

  /// <summary>
  /// 清空队列和去重集合，返回清空前是否存在内容。
  /// </summary>
  internal bool Reset()
  {
    bool changed = _entries.Count != 0 || _scheduledCoordinates.Count != 0;
    _entries.Clear();
    _scheduledCoordinates.Clear();
    return changed;
  }
}
