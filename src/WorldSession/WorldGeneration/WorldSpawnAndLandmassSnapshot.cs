using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldSpawnAndLandmassSnapshot(
  long GenerationId,
  bool WorldSpawnHasBeenRandomized,
  IReadOnlyList<WorldGenerationLandmassValue> LandmassData,
  int RemixSurfaceLayerLow,
  int RemixSurfaceLayerHigh,
  int RemixMushroomLayerLow,
  int RemixMushroomLayerHigh,
  int BoulderPetsPlaced);
