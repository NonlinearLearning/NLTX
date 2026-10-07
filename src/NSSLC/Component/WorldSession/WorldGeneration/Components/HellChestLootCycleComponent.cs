using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存地狱箱奖励的循环序列和当前选择。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：hellChest（第 268 行）； hellChestItem（第 270 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1658 行。</para>
/// </remarks>
public sealed class HellChestLootCycleComponent
{
  private readonly int[] _itemSequence;

  public HellChestLootCycleComponent(long generationId, IReadOnlyList<int> itemSequence)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    ArgumentNullException.ThrowIfNull(itemSequence);
    if (itemSequence.Count == 0)
    {
      throw new ArgumentException("The hell chest item sequence cannot be empty.", nameof(itemSequence));
    }

    _itemSequence = new int[itemSequence.Count];
    for (int index = 0; index < itemSequence.Count; index++)
    {
      _itemSequence[index] = itemSequence[index];
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int CurrentIndex { get; private set; }

  public int CurrentItem => _itemSequence[CurrentIndex];

  public HellChestLootCycleSnapshot CreateSnapshot()
  {
    int[] copy = new int[_itemSequence.Length];
    Array.Copy(_itemSequence, copy, _itemSequence.Length);
    return new HellChestLootCycleSnapshot(
      GenerationId,
      CurrentIndex,
      Array.AsReadOnly(copy));
  }

  internal bool TryAdvanceAfterSuccessfulPlacement()
  {
    return TryAdvanceAfterSuccessfulPlacement(placementSucceeded: true);
  }

  internal bool TryAdvanceAfterSuccessfulPlacement(bool placementSucceeded)
  {
    if (!placementSucceeded)
    {
      return false;
    }

    CurrentIndex++;
    if (CurrentIndex >= _itemSequence.Length)
    {
      CurrentIndex = 0;
    }

    return true;
  }
}
