using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成的山洞起点历史。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：numMCaves（第 224 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1241 行。</para>
/// </remarks>
public sealed class MountainCaveHistoryComponent
{
  public const int Capacity = 30;

  private readonly int[] _xOrigins = new int[Capacity];
  private readonly int[] _yOrigins = new int[Capacity];

  public MountainCaveHistoryComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int Count { get; private set; }

  internal bool TryAppend(int x, int y)
  {
    if (Count >= Capacity)
    {
      return false;
    }

    _xOrigins[Count] = x;
    _yOrigins[Count] = y;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public MountainCaveHistorySnapshot CreateSnapshot()
  {
    int[] xCopy = new int[Count];
    int[] yCopy = new int[Count];
    Array.Copy(_xOrigins, xCopy, Count);
    Array.Copy(_yOrigins, yCopy, Count);
    return new MountainCaveHistorySnapshot(
      GenerationId,
      Capacity,
      Count,
      Array.AsReadOnly(xCopy),
      Array.AsReadOnly(yCopy));
  }
}
