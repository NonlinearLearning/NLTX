namespace Terraria.Dome.Simulation.WorldObjects.Chest.Commands;

public readonly record struct ChestDestroyCommand(
  long Sequence,
  int ChestId);
