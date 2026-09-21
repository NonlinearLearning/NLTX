using System.Collections.Generic;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public static class GenerationRuleInitializationSystem
{
  public static StatuePlacementCatalogDefinition CreateStatuePlacementCatalog()
  {
    List<StatuePlacementOption> options = new(73);
    for (int style = 0; style < 44; style++)
    {
      options.Add(new StatuePlacementOption(105, style));
    }

    options[34] = new StatuePlacementOption(349, 0);
    options[43] = new StatuePlacementOption(105, 50);
    AddStyles(options, 63, 64, 65, 66, 68, 69, 70, 71, 72, 73, 75);
    AddStyles(options, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62);
    AddStyles(options, 77, 78, 67, 74, 37, 2);
    return new StatuePlacementCatalogDefinition(options);
  }

  public static StatueTrapSelectionDefinition CreateStatueTrapSelection()
  {
    return new StatueTrapSelectionDefinition(new[] { 4, 7, 10, 18 });
  }

  public static SpecialSeedGenerationRuleFlagsComponent
    CreateSpecialSeedGenerationRuleFlags(
      long generationId,
      bool notTheBeesWorld,
      bool noTrapsWorld,
      bool getGoodWorld,
      bool tenthAnniversaryWorld)
  {
    return new SpecialSeedGenerationRuleFlagsComponent(
      generationId,
      notTheBeesWorld && getGoodWorld && !tenthAnniversaryWorld,
      noTrapsWorld && getGoodWorld && !tenthAnniversaryWorld);
  }

  private static void AddStyles(List<StatuePlacementOption> options, params int[] styles)
  {
    for (int index = 0; index < styles.Length; index++)
    {
      options.Add(new StatuePlacementOption(105, styles[index]));
    }
  }
}
