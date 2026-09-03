using Terraria.Dome.Simulation.Items.Components;

namespace Terraria.Dome.Simulation.Items.Events;

public readonly record struct InventoryChangedEvent(
  PlayerHandle Player,
  int Slot,
  ItemStack Stack,
  ItemInstanceStateComponent InstanceState,
  long Revision);
