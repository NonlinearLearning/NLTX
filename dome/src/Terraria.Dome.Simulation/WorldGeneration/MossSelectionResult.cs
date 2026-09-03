namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct MossSelectionResult(
  GenerationRandomState State,
  ushort NeonMossTileType,
  int FirstMossType,
  int SecondMossType,
  int ThirdMossType);
