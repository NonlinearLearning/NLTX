namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct UseItemCommand(
  PlayerHandle Player,
  int SelectedSlot,
  long Sequence = 0);
