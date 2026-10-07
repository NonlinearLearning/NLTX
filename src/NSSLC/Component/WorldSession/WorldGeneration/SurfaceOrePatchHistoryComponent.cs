using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成的地表矿脉位置历史。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：numOrePatch（第 238 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1248 行。</para>
/// </remarks>
public sealed class SurfaceOrePatchHistoryComponent
{
  public const int Capacity = SurfaceOrePatchHistoryDefinition.Capacity;

  public const int EffectiveEntryLimit =
    SurfaceOrePatchHistoryDefinition.EffectiveAppendCapacity;

  private readonly int[] _patchX =
    new int[SurfaceOrePatchHistoryDefinition.Capacity];

  public SurfaceOrePatchHistoryComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int Count { get; private set; }

  internal bool TryAppend(int x)
  {
    if (Count >= SurfaceOrePatchHistoryDefinition.EffectiveAppendCapacity)
    {
      return false;
    }

    _patchX[Count] = x;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public SurfaceOrePatchHistorySnapshot CreateSnapshot()
  {
    int[] copy = new int[Count];
    Array.Copy(_patchX, copy, Count);
    return new SurfaceOrePatchHistorySnapshot(
      GenerationId,
      SurfaceOrePatchHistoryDefinition.Capacity,
      SurfaceOrePatchHistoryDefinition.EffectiveAppendCapacity,
      Count,
      Array.AsReadOnly(copy));
  }
}
