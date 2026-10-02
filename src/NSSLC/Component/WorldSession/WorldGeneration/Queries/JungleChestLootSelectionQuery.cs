using System;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Supplies the deterministic jungle-chest item selection for one generation step.
/// </summary>
public static class JungleChestLootSelectionQuery
{
  public static JungleChestLootSelectionResult SelectNext(
    int jungleItemCount,
    in JungleChestLootSelectionRandomInput randomInput)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(jungleItemCount);
    randomInput.Validate();

    int itemType = (jungleItemCount % 4) switch
    {
      0 => 211,
      1 => 212,
      2 => 213,
      _ => 964,
    };

    if (randomInput.FiftyChanceRoll == 0)
    {
      itemType = 753;
    }
    else if (randomInput.FifteenChanceRoll == 0)
    {
      itemType = 2292;
    }
    else if (randomInput.TwentyChanceRoll == 0)
    {
      itemType = 3017;
    }

    return new JungleChestLootSelectionResult(
      itemType,
      jungleItemCount + 1);
  }
}
