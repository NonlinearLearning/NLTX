using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成的地表隧道位置历史。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：numTunnels（第 232 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1245 行。</para>
/// </remarks>
public sealed class SurfaceTunnelHistoryComponent
{
  public const int Capacity = SurfaceTunnelHistoryDefinition.Capacity;

  public const int EffectiveEntryLimit =
    SurfaceTunnelHistoryDefinition.EffectiveAppendCapacity;

  private readonly int[] _centerX =
    new int[SurfaceTunnelHistoryDefinition.Capacity];

  public SurfaceTunnelHistoryComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int Count { get; private set; }

  internal bool TryAppend(int centerX)
  {
    if (Count >= SurfaceTunnelHistoryDefinition.EffectiveAppendCapacity)
    {
      return false;
    }

    _centerX[Count] = centerX;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public SurfaceTunnelHistorySnapshot CreateSnapshot()
  {
    int[] copy = new int[Count];
    Array.Copy(_centerX, copy, Count);
    return new SurfaceTunnelHistorySnapshot(
      GenerationId,
      SurfaceTunnelHistoryDefinition.Capacity,
      SurfaceTunnelHistoryDefinition.EffectiveAppendCapacity,
      Count,
      Array.AsReadOnly(copy));
  }
}
