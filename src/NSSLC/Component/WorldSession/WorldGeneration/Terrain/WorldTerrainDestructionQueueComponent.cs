using System.Collections.Generic;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Terrain;

/// <summary>
/// Stores terrain positions waiting for a later frame/network effect flush.
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
/// </remarks>
public sealed class WorldTerrainDestructionQueueComponent
{
  private readonly Queue<TilePosition> _pendingPositions = new();

  public int Count => _pendingPositions.Count;

  public bool IsEmpty => _pendingPositions.Count == 0;

  public void Enqueue(TilePosition position)
  {
    _pendingPositions.Enqueue(position);
  }

  public bool TryDequeue(out TilePosition position)
  {
    return _pendingPositions.TryDequeue(out position);
  }

  public void Clear()
  {
    _pendingPositions.Clear();
  }
}
