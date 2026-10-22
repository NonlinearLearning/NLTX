using System.Collections.Generic;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Terrain;

/// <summary>
/// 保存等待地形破坏处理的方块坐标队列。
/// </summary>
/// <remarks>
/// <para>职责：保存世界地形待销毁位置队列。</para>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：ExploitDestroyQueue（第 4342 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-design.md。
/// </para>
/// <para>依据位置：第 978 行。</para>
/// <para>
/// 用途：产生破坏请求的流程把坐标入队，地形处理流程按先进先出顺序取出并执行破坏。
/// </para>
/// <para>
/// 当前实现：只维护内存队列，允许重复坐标。TryDequeue 在空队列时返回 false，Clear 丢弃全部待处理坐标；实际方块修改由外部地形流程执行。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C204，L2（通用可选玩法）。分级理由：生态采样、结构保留区、地层统计、扫描和地形变换适用于可编辑／动态世界，固定小地图可减少。
/// </para>
/// </remarks>
public sealed class WorldTerrainDestructionQueueComponent
{
  private readonly Queue<TilePosition> _pendingPositions = new();

  /// <summary>
  /// 当前待处理的破坏坐标数量。
  /// </summary>
  public int Count => _pendingPositions.Count;

  /// <summary>
  /// 待处理坐标队列为空时返回 true。
  /// </summary>
  public bool IsEmpty => _pendingPositions.Count == 0;

  /// <summary>
  /// 把方块坐标加入队尾，保留重复坐标。
  /// </summary>
  public void Enqueue(TilePosition position)
  {
    _pendingPositions.Enqueue(position);
  }

  /// <summary>
  /// 取出并移除队首坐标；队列为空时返回 false。
  /// </summary>
  public bool TryDequeue(out TilePosition position)
  {
    return _pendingPositions.TryDequeue(out position);
  }

  /// <summary>
  /// 清空全部待处理破坏坐标。
  /// </summary>
  public void Clear()
  {
    _pendingPositions.Clear();
  }
}
