using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct UnequipItemCommand(
  PlayerHandle Player,
  ItemEquipmentSlot Slot,
  long Sequence = 0);
