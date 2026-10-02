namespace Terraria.WorldGeneration.Adapters;

public readonly record struct WorldGenerationConfigurationRequest(
  int WorldWidth,
  int WorldHeight,
  string SeedText,
  int RulesVersion = 1);
