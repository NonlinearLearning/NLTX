namespace Terraria.WorldGeneration.Terrain;

public readonly record struct WorldBoulderRainTransition(
  bool WasRaining,
  bool IsRaining,
  bool ShouldNotifyProgression);
