using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items.Events;

public readonly record struct ItemEquippedEvent(
  PlayerHandle Player,
  ushort ItemType,
  ItemEquipmentSlot Slot,
  int SourceSlot,
  bool IsVanity);
