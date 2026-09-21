namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct OreVeinComponent(
  string DefinitionId,
  int CursorX,
  int CursorY,
  int Density);
