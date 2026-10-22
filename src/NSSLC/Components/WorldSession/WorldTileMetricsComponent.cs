using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界方块扫描进度、待发布统计和最近发布的统计快照。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：tileCounts（第 4117 行）。</para>
/// <para>
/// 用途：扫描流程分批统计神圣、腐化、猩红和实体方块数量，再发布供世界状态显示或其他规则读取的比例快照。
/// </para>
/// <para>
/// 状态变化：Add 系列方法只累加待发布计数，PublishPendingCounts 才更新公开快照。比例按类别数量除以实体方块数量并四舍五入，非零类别算得零时提升为 
/// 1。
/// </para>
/// <para>
/// 当前实现：CreateSnapshot 在首次发布前返回默认值，TryGetPublishedSnapshot 可同时判断是否已发布。修改方块类型容量会重置扫描和快照状
/// 态；实际方块遍历由外部扫描流程执行。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C202，L2（通用可选玩法）。分级理由：生态采样、结构保留区、地层统计、扫描和地形变换适用于可编辑／动态世界，固定小地图可减少。
/// </para>
/// </remarks>
public sealed class WorldTileMetricsComponent
{
  private int[] _tileCounts = Array.Empty<int>();

  /// <summary>
  /// 内部按方块类型索引的计数数组。
  /// </summary>
  internal int[] TileCounts => _tileCounts;

  /// <summary>
  /// 方块扫描调度使用的 tick 计数。
  /// </summary>
  internal int TileScanTickCount { get; set; }

  /// <summary>
  /// 下一次扫描的方块列横坐标。
  /// </summary>
  internal int NextTileColumnX { get; set; }

  /// <summary>
  /// 已调度的方块列横坐标，默认 -1。
  /// </summary>
  internal int ScheduledTileColumnX { get; set; } = -1;

  private int _pendingGoodCount;
  private int _pendingEvilCount;
  private int _pendingBloodCount;
  private int _pendingSolidCount;
  private WorldTileMetricsSnapshot _publishedSnapshot;
  private bool _hasPublishedSnapshot;

  /// <summary>
  /// 返回最近发布的统计快照；首次发布前返回默认值。
  /// </summary>
  public WorldTileMetricsSnapshot CreateSnapshot()
  {
    return _publishedSnapshot;
  }

  /// <summary>
  /// 读取最近发布的快照，并返回是否已经发生过发布。
  /// </summary>
  public bool TryGetPublishedSnapshot(out WorldTileMetricsSnapshot snapshot)
  {
    snapshot = _publishedSnapshot;
    return _hasPublishedSnapshot;
  }

  /// <summary>
  /// 把待发布计数换算成比例快照，不修改已发布结果。
  /// </summary>
  internal WorldTileMetricsSnapshot CreatePendingSnapshot()
  {
    byte goodPercent = GetPercent(_pendingGoodCount, _pendingSolidCount);
    byte evilPercent = GetPercent(_pendingEvilCount, _pendingSolidCount);
    byte bloodPercent = GetPercent(_pendingBloodCount, _pendingSolidCount);

    if (goodPercent == 0 && _pendingGoodCount > 0)
    {
      goodPercent = 1;
    }

    if (evilPercent == 0 && _pendingEvilCount > 0)
    {
      evilPercent = 1;
    }

    if (bloodPercent == 0 && _pendingBloodCount > 0)
    {
      bloodPercent = 1;
    }

    return new WorldTileMetricsSnapshot(
      _pendingGoodCount,
      _pendingEvilCount,
      _pendingBloodCount,
      _pendingSolidCount,
      goodPercent,
      evilPercent,
      bloodPercent);
  }

  /// <summary>
  /// 清空方块计数、扫描游标、待发布统计和已发布快照状态。
  /// </summary>
  internal void Reset()
  {
    Array.Clear(_tileCounts, 0, _tileCounts.Length);
    TileScanTickCount = 0;
    NextTileColumnX = 0;
    ScheduledTileColumnX = -1;
    ClearPendingCounts();
    _publishedSnapshot = default;
    _hasPublishedSnapshot = false;
  }

  /// <summary>
  /// 要求正容量；容量变化时重新分配计数数组并重置全部扫描状态。
  /// </summary>
  internal void EnsureTileTypeCount(int tileTypeCount)
  {
    if (tileTypeCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileTypeCount));
    }

    if (_tileCounts.Length == tileTypeCount)
    {
      return;
    }

    _tileCounts = new int[tileTypeCount];
    Reset();
  }

  /// <summary>
  /// 累加待发布的神圣方块数量。
  /// </summary>
  internal void AddGoodCount(int count)
  {
    _pendingGoodCount += count;
  }

  /// <summary>
  /// 累加待发布的腐化方块数量。
  /// </summary>
  internal void AddEvilCount(int count)
  {
    _pendingEvilCount += count;
  }

  /// <summary>
  /// 累加待发布的猩红方块数量。
  /// </summary>
  internal void AddBloodCount(int count)
  {
    _pendingBloodCount += count;
  }

  /// <summary>
  /// 累加作为比例分母的实体方块数量。
  /// </summary>
  internal void AddSolidCount(int count)
  {
    _pendingSolidCount += count;
  }

  /// <summary>
  /// 只清零四种待发布统计计数。
  /// </summary>
  internal void ClearPendingCounts()
  {
    _pendingGoodCount = 0;
    _pendingEvilCount = 0;
    _pendingBloodCount = 0;
    _pendingSolidCount = 0;
  }

  /// <summary>
  /// 计算并提交待发布统计快照，标记已经发布。
  /// </summary>
  internal void PublishPendingCounts()
  {
    _publishedSnapshot = CreatePendingSnapshot();
    _hasPublishedSnapshot = true;
  }

  private static byte GetPercent(int count, int solidCount)
  {
    if (solidCount == 0)
    {
      return 0;
    }

    return (byte)Math.Round((double)count / (double)solidCount * 100.0);
  }
}
