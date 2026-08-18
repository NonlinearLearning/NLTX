using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct EquipItemCommand(
  PlayerHandle Player,
  int SourceSlot,
  bool IsVanity,
  long Sequence = 0);
