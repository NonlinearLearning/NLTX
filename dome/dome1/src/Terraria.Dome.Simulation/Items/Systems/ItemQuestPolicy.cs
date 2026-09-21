using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.Items.Systems;

public static class ItemQuestPolicy
{
  public static bool IsQuestItem(ItemDefinition definition)
  {
    return definition.Identity?.IsQuestItem == true ||
      LegacyAnglerQuestItemRegistry.ContainsItemType(definition.ItemType);
  }
}
