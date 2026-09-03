namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct AlchemicalTileValidationResult(
  bool IsValid,
  bool ShouldKill,
  int Style,
  bool HasSupportedBase,
  bool HasLavaContact);
