using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct PlaceItemCommand(
  PlayerHandle Player,
  int SourceSlot,
  int X,
  int Y,
  long Sequence = 0);
