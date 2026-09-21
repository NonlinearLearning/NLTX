using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldLayerMetricsSnapshot(
  long GenerationId,
  int LowestCloud,
  double WorldSurfaceLow,
  double WorldSurface,
  double WorldSurfaceHigh,
  double RockLayerLow,
  double RockLayer,
  double RockLayerHigh,
  int SnowTop,
  int SnowBottom,
  int SnowOriginLeft,
  int SnowOriginRight,
  IReadOnlyList<int> SnowMinX,
  IReadOnlyList<int> SnowMaxX);
