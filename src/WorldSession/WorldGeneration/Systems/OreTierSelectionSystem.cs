using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public static class OreTierSelectionSystem
{
  public static void Commit(
    WorldGenerationOreSelectionComponent component,
    WorldGenerationOreSelectionSnapshot selection)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (selection.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "An ore selection cannot be committed to another generation.",
        nameof(selection));
    }

    component.ReplaceSelection(
      selection.Copper,
      selection.Iron,
      selection.Silver,
      selection.Gold,
      selection.CopperBar,
      selection.IronBar,
      selection.SilverBar,
      selection.GoldBar);
  }
}
