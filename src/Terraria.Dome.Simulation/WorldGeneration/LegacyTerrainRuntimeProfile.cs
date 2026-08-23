using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTerrainRuntimeProfile(
  double WorldSurface,
  double RockLayer,
  double WorldSurfaceLow,
  double WorldSurfaceHigh,
  double RockLayerLow,
  double RockLayerHigh,
  int LeftBeachEnd,
  int RightBeachStart,
  int WaterLine,
  int LavaLine)
{
  public double? InitialWorldSurface { get; init; }
  public double? InitialRockLayer { get; init; }

  public void Validate(WorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    ValidateFiniteRange(WorldSurface, 0, metadata.Height, nameof(WorldSurface));
    ValidateFiniteRange(RockLayer, 0, metadata.Height, nameof(RockLayer));
    ValidateFiniteRange(WorldSurfaceLow, 0, metadata.Height, nameof(WorldSurfaceLow));
    ValidateFiniteRange(WorldSurfaceHigh, 0, metadata.Height, nameof(WorldSurfaceHigh));
    ValidateFiniteRange(RockLayerLow, 0, metadata.Height, nameof(RockLayerLow));
    ValidateFiniteRange(RockLayerHigh, 0, metadata.Height, nameof(RockLayerHigh));
    if (InitialWorldSurface.HasValue)
    {
      ValidateFiniteRange(
        InitialWorldSurface.Value,
        0,
        metadata.Height,
        nameof(InitialWorldSurface));
    }

    if (InitialRockLayer.HasValue)
    {
      ValidateFiniteRange(
        InitialRockLayer.Value,
        0,
        metadata.Height,
        nameof(InitialRockLayer));
    }

    if (RockLayer <= WorldSurface ||
        (InitialWorldSurface.HasValue && InitialRockLayer.HasValue &&
          InitialRockLayer.Value <= InitialWorldSurface.Value) ||
        WorldSurfaceLow > WorldSurfaceHigh ||
        RockLayerLow > RockLayerHigh)
    {
      throw new ArgumentException(
        "Legacy terrain surface and rock-layer ranges are inconsistent.",
        nameof(RockLayer));
    }

    if (LeftBeachEnd < 0 ||
        LeftBeachEnd >= RightBeachStart ||
        RightBeachStart >= metadata.Width)
    {
      throw new ArgumentOutOfRangeException(nameof(LeftBeachEnd));
    }

    ValidateIntegerRange(WaterLine, metadata.Height, nameof(WaterLine));
    ValidateIntegerRange(LavaLine, metadata.Height, nameof(LavaLine));
  }

  private static void ValidateFiniteRange(
    double value,
    double minimum,
    double maximum,
    string parameterName)
  {
    if (double.IsNaN(value) || double.IsInfinity(value) || value < minimum || value >= maximum)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }

  private static void ValidateIntegerRange(int value, int height, string parameterName)
  {
    if (value < 0 || value >= height)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
