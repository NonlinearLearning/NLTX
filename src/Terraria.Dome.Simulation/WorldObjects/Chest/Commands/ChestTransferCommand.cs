namespace Terraria.Dome.Simulation.WorldObjects.Chest.Commands;

public readonly record struct ChestTransferCommand(
  long Sequence,
  int ChestId,
  PlayerHandle Player,
  int InventorySlot,
  int ChestSlot,
  bool Withdraw,
  SimulationVector PlayerPosition);
