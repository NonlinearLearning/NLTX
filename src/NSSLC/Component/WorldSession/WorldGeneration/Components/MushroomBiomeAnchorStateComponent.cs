using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成的蘑菇群落锚点集合。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：numMushroomBiomes（第 244 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1399 行。</para>
/// </remarks>
public sealed class MushroomBiomeAnchorStateComponent
{
  private readonly TilePosition[] _positions =
    new TilePosition[MushroomBiomeCapacityDefinition.Capacity];

  public MushroomBiomeAnchorStateComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int Count { get; private set; }

  public bool TryAppend(TilePosition position)
  {
    if (Count >= MushroomBiomeCapacityDefinition.Capacity)
    {
      return false;
    }

    _positions[Count] = position;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public MushroomBiomeAnchorStateSnapshot CreateSnapshot()
  {
    TilePosition[] copy = new TilePosition[Count];
    Array.Copy(_positions, copy, Count);
    return new MushroomBiomeAnchorStateSnapshot(
      GenerationId,
      MushroomBiomeCapacityDefinition.Capacity,
      Count,
      Array.AsReadOnly(copy));
  }
}
