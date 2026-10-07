using System;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Explicit random rolls consumed by one jungle-chest item selection.
/// </summary>
public readonly record struct JungleChestLootSelectionRandomInput(
  int FiftyChanceRoll,
  int FifteenChanceRoll,
  int TwentyChanceRoll)
{
  public void Validate()
  {
    if (FiftyChanceRoll < 0 || FiftyChanceRoll >= 50)
    {
      throw new ArgumentOutOfRangeException(nameof(FiftyChanceRoll));
    }

    if (FifteenChanceRoll < 0 || FifteenChanceRoll >= 15)
    {
      throw new ArgumentOutOfRangeException(nameof(FifteenChanceRoll));
    }

    if (TwentyChanceRoll < 0 || TwentyChanceRoll >= 20)
    {
      throw new ArgumentOutOfRangeException(nameof(TwentyChanceRoll));
    }
  }
}
