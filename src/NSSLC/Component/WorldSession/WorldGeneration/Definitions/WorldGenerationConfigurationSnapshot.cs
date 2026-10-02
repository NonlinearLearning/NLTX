namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldGenerationConfigurationSnapshot(
  int WorldWidth,
  int WorldHeight,
  string SeedText,
  int RulesVersion);
