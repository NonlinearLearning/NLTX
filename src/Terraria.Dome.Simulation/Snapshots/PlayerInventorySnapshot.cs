using System.Collections.Generic;
using Terraria.Dome.Simulation.Inventory.Components;
using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation;

public readonly record struct PlayerInventorySnapshot(
  PlayerHandle Player,
  int SelectedSlot,
  int SelectionRevision,
  EquipmentLoadoutComponent Loadout,
  IReadOnlyList<ItemStack> Items);
