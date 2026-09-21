using Terraria.WorldGeneration.Host;

namespace Terraria.WorldGeneration.Projections;

public readonly record struct WorldGenerationGraphicsSnapshot(
  WorldGenerationColor FavoriteColor,
  bool MapEnabled,
  bool IsEnginePreloaded,
  bool SkipAssemblyLoad,
  int RenderCount,
  float ShimmerAlpha,
  float ShimmerDarken,
  bool AfterPartyOfDoom);
