using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成已放置湖泊的位置历史。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：LakeX（第 256 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1525 行。</para>
/// </remarks>
public sealed class LakePlacementHistoryComponent
{
  private readonly int[] _lakeX =
    new int[LakePlacementCapacityDefinition.Capacity];

  public LakePlacementHistoryComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int Count { get; private set; }

  public bool TryAppend(int lakeX)
  {
    if (Count >= LakePlacementCapacityDefinition.EffectiveEntryLimit)
    {
      return false;
    }

    _lakeX[Count] = lakeX;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public LakePlacementHistorySnapshot CreateSnapshot()
  {
    int[] copy = new int[Count];
    Array.Copy(_lakeX, copy, Count);
    return new LakePlacementHistorySnapshot(
      GenerationId,
      LakePlacementCapacityDefinition.Capacity,
      LakePlacementCapacityDefinition.EffectiveEntryLimit,
      Count,
      Array.AsReadOnly(copy));
  }
}
