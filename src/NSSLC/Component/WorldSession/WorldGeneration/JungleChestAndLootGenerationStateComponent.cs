using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存丛林箱位置、奖励计数和红木魔杖生成状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：JungleItemCount（第 176 行）； gennedLivingMahoganyWands（第 178 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 968 行。</para>
/// </remarks>
public sealed class JungleChestAndLootGenerationStateComponent
{
  public const int Capacity = 100;

  private readonly int[] _chestXPositions = new int[Capacity];
  private readonly int[] _chestYPositions = new int[Capacity];

  public JungleChestAndLootGenerationStateComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int JungleItemCount { get; private set; }

  public bool GennedLivingMahoganyWands { get; private set; }

  public int Count { get; private set; }

  public void ReplaceLootState(
    int jungleItemCount,
    bool gennedLivingMahoganyWands)
  {
    JungleItemCount = jungleItemCount;
    GennedLivingMahoganyWands = gennedLivingMahoganyWands;
  }

  internal void ReplaceState(
    int jungleItemCount,
    bool gennedLivingMahoganyWands,
    int count,
    IReadOnlyList<int> chestXPositions,
    IReadOnlyList<int> chestYPositions)
  {
    if (count < 0 || count > Capacity)
    {
      throw new ArgumentOutOfRangeException(nameof(count));
    }

    ArgumentNullException.ThrowIfNull(chestXPositions);
    ArgumentNullException.ThrowIfNull(chestYPositions);
    if (chestXPositions.Count != count || chestYPositions.Count != count)
    {
      throw new ArgumentException(
        "Jungle chest coordinate lists must cover the complete used range.");
    }

    int[] xCopy = new int[count];
    int[] yCopy = new int[count];
    for (int index = 0; index < count; index++)
    {
      xCopy[index] = chestXPositions[index];
      yCopy[index] = chestYPositions[index];
    }

    Array.Copy(xCopy, _chestXPositions, count);
    Array.Copy(yCopy, _chestYPositions, count);
    JungleItemCount = jungleItemCount;
    GennedLivingMahoganyWands = gennedLivingMahoganyWands;
    Count = count;
  }

  public bool TryAppendChest(int x, int y)
  {
    if (Count >= Capacity)
    {
      return false;
    }

    _chestXPositions[Count] = x;
    _chestYPositions[Count] = y;
    Count++;
    return true;
  }

  public void ClearChests()
  {
    Count = 0;
  }

  public JungleChestAndLootGenerationSnapshot CreateSnapshot()
  {
    int[] xCopy = new int[Count];
    int[] yCopy = new int[Count];
    Array.Copy(_chestXPositions, xCopy, Count);
    Array.Copy(_chestYPositions, yCopy, Count);
    return new JungleChestAndLootGenerationSnapshot(
      GenerationId,
      JungleItemCount,
      GennedLivingMahoganyWands,
      Capacity,
      Count,
      Array.AsReadOnly(xCopy),
      Array.AsReadOnly(yCopy));
  }
}
