namespace Terraria.Dome.Simulation.WorldGeneration.Definitions;

public readonly record struct TorchDefinition(
  short TorchId,
  int DustType,
  bool IsBiomeTorch);
