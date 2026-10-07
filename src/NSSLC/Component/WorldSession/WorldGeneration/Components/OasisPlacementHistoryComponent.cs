using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成的绿洲中心和宽度历史。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：numOasis（第 260 行）； oasisPosition（第 262 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1528 行。</para>
/// </remarks>
public sealed class OasisPlacementHistoryComponent
{
  private readonly TilePosition[] _centers =
    new TilePosition[OasisPlacementCapacityDefinition.Capacity];
  private readonly int[] _widths =
    new int[OasisPlacementCapacityDefinition.Capacity];

  public OasisPlacementHistoryComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int Count { get; private set; }

  public bool TryAppend(TilePosition center, int width)
  {
    if (Count >= OasisPlacementCapacityDefinition.Capacity || width < 45 || width > 60)
    {
      return false;
    }

    _centers[Count] = center;
    _widths[Count] = width;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public OasisPlacementHistorySnapshot CreateSnapshot()
  {
    TilePosition[] centerCopy = new TilePosition[Count];
    int[] widthCopy = new int[Count];
    Array.Copy(_centers, centerCopy, Count);
    Array.Copy(_widths, widthCopy, Count);
    return new OasisPlacementHistorySnapshot(
      GenerationId,
      OasisPlacementCapacityDefinition.Capacity,
      Count,
      Array.AsReadOnly(centerCopy),
      Array.AsReadOnly(widthCopy));
  }
}
