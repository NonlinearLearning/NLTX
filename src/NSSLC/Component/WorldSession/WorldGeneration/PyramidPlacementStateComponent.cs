using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成的金字塔位置历史。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：numPyr（第 156 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 958 行。</para>
/// </remarks>
public sealed class PyramidPlacementStateComponent
{
  private readonly int[] _xPositions;
  private readonly int[] _yPositions;

  public PyramidPlacementStateComponent(long generationId, int capacity)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (capacity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    GenerationId = generationId;
    Capacity = capacity;
    _xPositions = new int[capacity];
    _yPositions = new int[capacity];
  }

  public long GenerationId { get; }

  public int Capacity { get; }

  public int Count { get; private set; }

  public bool TryAppend(int x, int y)
  {
    if (Count >= Capacity)
    {
      return false;
    }

    _xPositions[Count] = x;
    _yPositions[Count] = y;
    Count++;
    return true;
  }

  internal void ReplaceState(
    int count,
    IReadOnlyList<int> xPositions,
    IReadOnlyList<int> yPositions)
  {
    if (count < 0 || count > Capacity)
    {
      throw new ArgumentOutOfRangeException(nameof(count));
    }

    ArgumentNullException.ThrowIfNull(xPositions);
    ArgumentNullException.ThrowIfNull(yPositions);
    if (xPositions.Count != count || yPositions.Count != count)
    {
      throw new ArgumentException(
        "Pyramid coordinate lists must contain exactly the used coordinates.");
    }

    for (int index = 0; index < count; index++)
    {
      _xPositions[index] = xPositions[index];
      _yPositions[index] = yPositions[index];
    }
    Count = count;
  }

  public void Clear()
  {
    Count = 0;
  }

  public PyramidPlacementSnapshot CreateSnapshot()
  {
    int[] xCopy = new int[Count];
    int[] yCopy = new int[Count];
    Array.Copy(_xPositions, xCopy, Count);
    Array.Copy(_yPositions, yCopy, Count);
    return new PyramidPlacementSnapshot(
      GenerationId,
      Capacity,
      Count,
      Array.AsReadOnly(xCopy),
      Array.AsReadOnly(yCopy));
  }
}
