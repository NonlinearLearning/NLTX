using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public static class HellChestGenerationSystem
{
  private static readonly int[] NormalItemPool = { 274, 220, 112, 218, 3019 };
  private static readonly int[] RemixItemPool = { 274, 220, 683, 218, 3019 };

  public static HellChestLootCycleComponent CreateForGeneration(
    long generationId,
    bool remixWorld,
    IReadOnlyList<int> shuffledItemSequence)
  {
    ArgumentNullException.ThrowIfNull(shuffledItemSequence);
    int[] expectedPool = remixWorld ? RemixItemPool : NormalItemPool;
    ValidateSequence(shuffledItemSequence, expectedPool);
    return new HellChestLootCycleComponent(generationId, shuffledItemSequence);
  }

  public static bool TryAdvanceAfterPlacement(
    HellChestLootCycleComponent component,
    bool placementSucceeded)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!placementSucceeded)
    {
      return false;
    }

    return component.TryAdvanceAfterSuccessfulPlacement();
  }

  private static void ValidateSequence(
    IReadOnlyList<int> sequence,
    IReadOnlyList<int> expectedPool)
  {
    if (sequence.Count != expectedPool.Count)
    {
      throw new ArgumentException(
        "The hell chest sequence must contain the complete item pool.",
        nameof(sequence));
    }

    bool[] matched = new bool[expectedPool.Count];
    for (int sequenceIndex = 0; sequenceIndex < sequence.Count; sequenceIndex++)
    {
      bool found = false;
      for (int expectedIndex = 0; expectedIndex < expectedPool.Count; expectedIndex++)
      {
        if (!matched[expectedIndex] && sequence[sequenceIndex] == expectedPool[expectedIndex])
        {
          matched[expectedIndex] = true;
          found = true;
          break;
        }
      }

      if (!found)
      {
        throw new ArgumentException(
          "The hell chest sequence is not a permutation of the selected item pool.",
          nameof(sequence));
      }
    }
  }
}
