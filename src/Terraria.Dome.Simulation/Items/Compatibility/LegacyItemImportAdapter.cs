using System;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Snapshots;
using Terraria.Dome.Simulation.Players;

namespace Terraria.Dome.Simulation.Items.Compatibility;

public static class LegacyItemImportAdapter
{
  public static bool TryImport(
    PlayerPersistentItem item,
    ItemDefinition definition,
    out ItemInstanceSnapshot snapshot)
  {
    snapshot = new ItemInstanceSnapshot(ItemStack.Empty, default);
    if (item.SlotId < 0 || item.SlotId >= PlayerPersistentState.ItemSlotCount ||
        item.Stack <= 0 || item.ItemType <= 0 ||
        item.ItemType > ushort.MaxValue || definition.ItemType != item.ItemType ||
        definition.StackLimit <= 0 || item.Stack > definition.StackLimit)
    {
      return false;
    }

    ItemInstanceStateComponent state = new(
      PrefixId: item.Prefix,
      VariantId: item.VariantId,
      Dye: item.Dye,
      Paint: item.Paint,
      IsFavorited: item.IsFavorited,
      IsNewAndShiny: item.IsNewAndShiny,
      NameOverride: item.NameOverride);
    try
    {
      state.Validate();
    }
    catch (ArgumentOutOfRangeException)
    {
      return false;
    }

    snapshot = new ItemInstanceSnapshot(
      new ItemStack((ushort)item.ItemType, item.Stack),
      state);
    return true;
  }
}
