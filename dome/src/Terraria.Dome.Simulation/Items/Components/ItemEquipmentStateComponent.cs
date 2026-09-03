using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items.Components;

public readonly record struct ItemEquipmentStateComponent(
  ItemEquipmentSlot Slot,
  int SourceSlot,
  bool IsVanity);
