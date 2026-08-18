namespace Terraria.Dome.Simulation.WorldObjects.Chest.Commands;

public readonly record struct ChestOpenCommand(
  long Sequence,
  int ChestId,
  PlayerHandle Player,
  SimulationVector PlayerPosition);
