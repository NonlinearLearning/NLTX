namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct HardmodeOreTierSelection(
  HardmodeOreTierState State,
  int SelectedTileType,
  bool WasInitialized,
  bool WasToggled);
