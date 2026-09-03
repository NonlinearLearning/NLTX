namespace Terraria.Dome.Simulation.WorldObjects.Chest.Commands;

public readonly record struct ChestCloseCommand(
  long Sequence,
  int ChestId,
  PlayerHandle Player);
