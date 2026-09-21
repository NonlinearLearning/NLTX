using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct MergeItemStackCommand(
  PlayerHandle Player,
  int SourceSlot,
  int DestinationSlot,
  int Quantity,
  long Sequence = 0);
