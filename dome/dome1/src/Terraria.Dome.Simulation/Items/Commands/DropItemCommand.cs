using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct DropItemCommand(
  PlayerHandle Player,
  int SourceSlot,
  int Quantity,
  long Sequence = 0);
