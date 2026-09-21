using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

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
