using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldLayerMetricsComponent
{
  private int[] _snowMinX;
  private int[] _snowMaxX;
  private IReadOnlyList<int> _snowMinXView;
  private IReadOnlyList<int> _snowMaxXView;

  public WorldLayerMetricsComponent(
    long generationId,
    int lowestCloud = -1,
    double worldSurfaceLow = 0.0d,
    double worldSurface = 0.0d,
    double worldSurfaceHigh = 0.0d,
    double rockLayerLow = 0.0d,
    double rockLayer = 0.0d,
    double rockLayerHigh = 0.0d,
    int snowTop = 0,
    int snowBottom = 0,
    int snowOriginLeft = 0,
    int snowOriginRight = 0,
    IReadOnlyList<int>? snowMinX = null,
    IReadOnlyList<int>? snowMaxX = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    ValidateFinite(worldSurfaceLow, nameof(worldSurfaceLow));
    ValidateFinite(worldSurface, nameof(worldSurface));
    ValidateFinite(worldSurfaceHigh, nameof(worldSurfaceHigh));
    ValidateFinite(rockLayerLow, nameof(rockLayerLow));
    ValidateFinite(rockLayer, nameof(rockLayer));
    ValidateFinite(rockLayerHigh, nameof(rockLayerHigh));

    GenerationId = generationId;
    LowestCloud = lowestCloud;
    WorldSurfaceLow = worldSurfaceLow;
    WorldSurface = worldSurface;
    WorldSurfaceHigh = worldSurfaceHigh;
    RockLayerLow = rockLayerLow;
    RockLayer = rockLayer;
    RockLayerHigh = rockLayerHigh;
    SnowTop = snowTop;
    SnowBottom = snowBottom;
    SnowOriginLeft = snowOriginLeft;
    SnowOriginRight = snowOriginRight;
    (_snowMinX, _snowMaxX) = CopySnowColumns(snowMinX, snowMaxX);
    _snowMinXView = Array.AsReadOnly(_snowMinX);
    _snowMaxXView = Array.AsReadOnly(_snowMaxX);
  }

  public long GenerationId { get; }

  public int LowestCloud { get; private set; }

  public double WorldSurfaceLow { get; private set; }

  public double WorldSurface { get; private set; }

  public double WorldSurfaceHigh { get; private set; }

  public double RockLayerLow { get; private set; }

  public double RockLayer { get; private set; }

  public double RockLayerHigh { get; private set; }

  public int SnowTop { get; private set; }

  public int SnowBottom { get; private set; }

  public int SnowOriginLeft { get; private set; }

  public int SnowOriginRight { get; private set; }

  public IReadOnlyList<int> SnowMinX => _snowMinXView;

  public IReadOnlyList<int> SnowMaxX => _snowMaxXView;

  internal void ReplaceMetrics(
    int lowestCloud,
    double worldSurfaceLow,
    double worldSurface,
    double worldSurfaceHigh,
    double rockLayerLow,
    double rockLayer,
    double rockLayerHigh,
    int snowTop,
    int snowBottom,
    int snowOriginLeft,
    int snowOriginRight,
    IReadOnlyList<int>? snowMinX,
    IReadOnlyList<int>? snowMaxX)
  {
    ValidateFinite(worldSurfaceLow, nameof(worldSurfaceLow));
    ValidateFinite(worldSurface, nameof(worldSurface));
    ValidateFinite(worldSurfaceHigh, nameof(worldSurfaceHigh));
    ValidateFinite(rockLayerLow, nameof(rockLayerLow));
    ValidateFinite(rockLayer, nameof(rockLayer));
    ValidateFinite(rockLayerHigh, nameof(rockLayerHigh));
    (int[] copiedSnowMinX, int[] copiedSnowMaxX) =
      CopySnowColumns(snowMinX, snowMaxX);

    LowestCloud = lowestCloud;
    WorldSurfaceLow = worldSurfaceLow;
    WorldSurface = worldSurface;
    WorldSurfaceHigh = worldSurfaceHigh;
    RockLayerLow = rockLayerLow;
    RockLayer = rockLayer;
    RockLayerHigh = rockLayerHigh;
    SnowTop = snowTop;
    SnowBottom = snowBottom;
    SnowOriginLeft = snowOriginLeft;
    SnowOriginRight = snowOriginRight;
    _snowMinX = copiedSnowMinX;
    _snowMaxX = copiedSnowMaxX;
    _snowMinXView = Array.AsReadOnly(_snowMinX);
    _snowMaxXView = Array.AsReadOnly(_snowMaxX);
  }

  public WorldLayerMetricsSnapshot CreateSnapshot()
  {
    return new WorldLayerMetricsSnapshot(
      GenerationId,
      LowestCloud,
      WorldSurfaceLow,
      WorldSurface,
      WorldSurfaceHigh,
      RockLayerLow,
      RockLayer,
      RockLayerHigh,
      SnowTop,
      SnowBottom,
      SnowOriginLeft,
      SnowOriginRight,
      CopyReadOnly(_snowMinX),
      CopyReadOnly(_snowMaxX));
  }

  private static (int[] SnowMinX, int[] SnowMaxX) CopySnowColumns(
    IReadOnlyList<int>? snowMinX,
    IReadOnlyList<int>? snowMaxX)
  {
    int minLength = snowMinX?.Count ?? 0;
    int maxLength = snowMaxX?.Count ?? 0;
    if (minLength != maxLength)
    {
      throw new ArgumentException(
        "Snow column bounds must contain matching numbers of minimum and maximum entries.",
        nameof(snowMaxX));
    }

    int[] copiedSnowMinX = new int[minLength];
    int[] copiedSnowMaxX = new int[maxLength];
    for (int index = 0; index < minLength; index++)
    {
      copiedSnowMinX[index] = snowMinX![index];
      copiedSnowMaxX[index] = snowMaxX![index];
    }

    return (copiedSnowMinX, copiedSnowMaxX);
  }

  private static IReadOnlyList<int> CopyReadOnly(IReadOnlyList<int> source)
  {
    int[] copy = new int[source.Count];
    for (int index = 0; index < source.Count; index++)
    {
      copy[index] = source[index];
    }

    return Array.AsReadOnly(copy);
  }

  private static void ValidateFinite(double value, string parameterName)
  {
    if (!double.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
