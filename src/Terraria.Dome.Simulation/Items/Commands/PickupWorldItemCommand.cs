namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct PickupWorldItemCommand(
  PlayerHandle Player,
  int WorldItemId,
  long Sequence = 0);
