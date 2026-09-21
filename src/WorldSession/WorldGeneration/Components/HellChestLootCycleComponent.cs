using System;

namespace Terraria.WorldGeneration.Components;

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
