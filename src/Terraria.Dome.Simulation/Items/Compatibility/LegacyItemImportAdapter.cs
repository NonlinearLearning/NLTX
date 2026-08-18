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
    if (item.SlotId < 0 || item.Stack <= 0 || item.ItemType <= 0 ||
        item.ItemType > ushort.MaxValue || definition.ItemType != item.ItemType ||
        definition.StackLimit <= 0)
    {
      return false;
    }

    int quantity = Math.Min(item.Stack, definition.StackLimit);
    if (quantity <= 0)
    {
      return false;
    }

    snapshot = new ItemInstanceSnapshot(
      new ItemStack((ushort)item.ItemType, quantity),
      new ItemInstanceStateComponent(
        PrefixId: item.Prefix,
        IsFavorited: item.IsFavorited));
    return true;
  }
}
