using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存地下沙漠幼虫放置坐标。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：larvaY（第 152 行）； larvaX（第 154 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 810 行。</para>
/// </remarks>
public sealed class UndergroundDesertLarvaPlacementComponent
{
  public const int Capacity = 100;

  private readonly int[] _larvaX = new int[Capacity];

  private readonly int[] _larvaY = new int[Capacity];

  public UndergroundDesertLarvaPlacementComponent(long generationId)
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
    if (Count >= Capacity)
    {
      return false;
    }

    _larvaX[Count] = position.X;
    _larvaY[Count] = position.Y;
    Count++;
    return true;
  }

  public void Clear()
  {
    Array.Clear(_larvaX, 0, _larvaX.Length);
    Array.Clear(_larvaY, 0, _larvaY.Length);
    Count = 0;
  }

  public UndergroundDesertLarvaPlacementSnapshot CreateSnapshot()
  {
    TilePosition[] copy = new TilePosition[Count];
    for (int index = 0; index < Count; index++)
    {
      copy[index] = new TilePosition(_larvaX[index], _larvaY[index]);
    }

    return new UndergroundDesertLarvaPlacementSnapshot(
      GenerationId,
      Count,
      Array.AsReadOnly(copy));
  }
}
