using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class GemTileRandomPolicy
{
  private const ushort StoneTileType = 1;

  public static (GenerationRandomState State, int GemIndex) NextGemIndex(
    GenerationRandomState state,
    IReadOnlyList<bool> enabledGems)
  {
    ArgumentNullException.ThrowIfNull(enabledGems);
    if (enabledGems.Count != 6)
    {
      throw new ArgumentException(
        "Exactly six legacy gem flags are required.",
        nameof(enabledGems));
    }

    if (!HasEnabledGem(enabledGems))
    {
      throw new ArgumentException(
        "At least one legacy gem flag must be enabled.",
        nameof(enabledGems));
    }

    GenerationRandomState next = state;
    int index;
    do
    {
      (next, index) = next.NextExclusive(6);
    }
    while (!enabledGems[index]);

    return (next, index);
  }

  public static GemTileRandomResult NextTile(
    GenerationRandomState state,
    IReadOnlyList<bool> enabledGems)
  {
    (GenerationRandomState next, int chance) = state.NextExclusive(20);
    if (chance != 0)
    {
      return new GemTileRandomResult(next, -1, StoneTileType);
    }

    (GenerationRandomState finalState, int gemIndex) = NextGemIndex(next, enabledGems);
    return new GemTileRandomResult(finalState, gemIndex, GetTileType(gemIndex));
  }

  private static ushort GetTileType(int gemIndex)
  {
    return gemIndex switch
    {
      0 => 67,
      1 => 66,
      2 => 63,
      3 => 65,
      4 => 64,
      _ => 68
    };
  }

  private static bool HasEnabledGem(IReadOnlyList<bool> enabledGems)
  {
    for (int index = 0; index < enabledGems.Count; index++)
    {
      if (enabledGems[index])
      {
        return true;
      }
    }

    return false;
  }
}
