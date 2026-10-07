using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界地表、岩层、雪地和云层的生成指标。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>
/// 主要源成员：lowestCloud（第 46 行）； worldSurfaceLow（第 66 行）； worldSurface（第 68 行）； worldSurfaceHigh（第
/// 70 行）； rockLayerLow（第 72 行）； rockLayer（第 74 行）； rockLayerHigh（第 76 行）； snowTop（第 78 行）；
/// snowBottom（第 80 行）； snowOriginLeft（第 82 行）； snowOriginRight（第 84 行）； snowMinX（第 86 行）；
/// snowMaxX（第 88 行）。
/// </para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 388 行。</para>
/// </remarks>
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
