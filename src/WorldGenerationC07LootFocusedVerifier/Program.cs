using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

static class Program
{
  private static int Main()
  {
    try
    {
      VerifyBaseCycleAndCursorAdvance();
      VerifyShortCircuitRareOverrides();
      VerifySystemCommitsOnlyTheCursor();
      VerifyRandomRollBounds();
      Console.WriteLine("C07 jungle-loot focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void VerifyBaseCycleAndCursorAdvance()
  {
    JungleChestLootSelectionRandomInput noOverrides = new(1, 1, 1);
    int[] expectedItems = [211, 212, 213, 964, 211];

    for (int index = 0; index < expectedItems.Length; index++)
    {
      JungleChestLootSelectionResult result =
        JungleChestLootSelectionQuery.SelectNext(index, noOverrides);
      Require(result.ItemType == expectedItems[index], "base jungle item cycle");
      Require(result.NextJungleItemCount == index + 1, "jungle item cursor advance");
    }
  }

  private static void VerifyShortCircuitRareOverrides()
  {
    JungleChestLootSelectionResult fifty =
      JungleChestLootSelectionQuery.SelectNext(
        0,
        new JungleChestLootSelectionRandomInput(0, 0, 0));
    Require(fifty.ItemType == 753, "the fifty-roll override has priority");

    JungleChestLootSelectionResult fifteen =
      JungleChestLootSelectionQuery.SelectNext(
        0,
        new JungleChestLootSelectionRandomInput(1, 0, 0));
    Require(fifteen.ItemType == 2292, "the fifteen-roll override follows the first check");

    JungleChestLootSelectionResult twenty =
      JungleChestLootSelectionQuery.SelectNext(
        0,
        new JungleChestLootSelectionRandomInput(1, 1, 0));
    Require(twenty.ItemType == 3017, "the twenty-roll override follows the first two checks");
  }

  private static void VerifySystemCommitsOnlyTheCursor()
  {
    JungleChestAndLootGenerationStateComponent component =
      new(generationId: 1);
    component.ReplaceLootState(jungleItemCount: 2, gennedLivingMahoganyWands: true);

    JungleChestLootSelectionResult result =
      JungleChestLootSelectionSystem.SelectNextItem(
        component,
        new JungleChestLootSelectionRandomInput(1, 1, 1));

    Require(result.ItemType == 213, "system returns the selected item");
    Require(component.JungleItemCount == 3, "system commits the next cursor");
    Require(component.GennedLivingMahoganyWands, "system preserves the wand result flag");
    Require(component.Count == 0, "loot selection does not mutate chest coordinates");
  }

  private static void VerifyRandomRollBounds()
  {
    RequireThrows(
      () => JungleChestLootSelectionQuery.SelectNext(
        0,
        new JungleChestLootSelectionRandomInput(50, 0, 0)),
      "fifty-roll upper bound");
    RequireThrows(
      () => JungleChestLootSelectionQuery.SelectNext(
        0,
        new JungleChestLootSelectionRandomInput(0, 15, 0)),
      "fifteen-roll upper bound");
    RequireThrows(
      () => JungleChestLootSelectionQuery.SelectNext(
        0,
        new JungleChestLootSelectionRandomInput(0, 0, 20)),
      "twenty-roll upper bound");
  }

  private static void RequireThrows(Action action, string message)
  {
    try
    {
      action();
    }
    catch (ArgumentOutOfRangeException)
    {
      return;
    }

    throw new InvalidOperationException(message);
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }
}
