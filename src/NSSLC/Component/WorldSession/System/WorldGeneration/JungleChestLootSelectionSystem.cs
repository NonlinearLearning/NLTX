using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits the cursor transition produced by the pure jungle-chest selection query.
/// </summary>
public static class JungleChestLootSelectionSystem
{
  public static JungleChestLootSelectionResult SelectNextItem(
    JungleChestAndLootGenerationStateComponent component,
    in JungleChestLootSelectionRandomInput randomInput)
  {
    ArgumentNullException.ThrowIfNull(component);

    JungleChestLootSelectionResult result =
      JungleChestLootSelectionQuery.SelectNext(
        component.JungleItemCount,
        randomInput);
    component.ReplaceLootState(
      result.NextJungleItemCount,
      component.GennedLivingMahoganyWands);
    return result;
  }
}
