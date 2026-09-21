namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct GemTileRandomResult(
  GenerationRandomState State,
  int GemIndex,
  ushort TileType);
