namespace Terraria.Dome.Simulation.WorldObjects.Chest.Commands;

public readonly record struct ChestCreateCommand(
  long Sequence,
  int ChestId,
  int TileX,
  int TileY);
