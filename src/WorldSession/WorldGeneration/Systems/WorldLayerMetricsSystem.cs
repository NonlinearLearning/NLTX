using System;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Passes;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits one complete layer-metrics snapshot for its generation session.
/// </summary>
public static class WorldLayerMetricsSystem
{
  public static WorldLayerMetricsSnapshot CalculateAndCommit(
    WorldLayerMetricsComponent component,
    in WorldLayerMetricsCalculationInput input,
    IGenerationRandomSource random)
  {
    return CalculateAndCommit(
      component,
      input,
      random,
      new WorldLayerMetricsCalculator());
  }

  public static WorldLayerMetricsSnapshot CalculateAndCommit(
    WorldLayerMetricsComponent component,
    in WorldLayerMetricsCalculationInput input,
    IGenerationRandomSource random,
    IWorldLayerMetricsCalculator calculator)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(calculator);
    if (input.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Layer metrics cannot be calculated for another generation.",
        nameof(input));
    }

    WorldLayerMetricsSnapshot metrics = calculator.Calculate(input, random);
    Commit(component, metrics);
    return component.CreateSnapshot();
  }

  public static void Commit(
    WorldLayerMetricsComponent component,
    in WorldLayerMetricsSnapshot metrics)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (metrics.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Layer metrics cannot be committed to another generation.",
        nameof(metrics));
    }

    component.ReplaceMetrics(
      metrics.LowestCloud,
      metrics.WorldSurfaceLow,
      metrics.WorldSurface,
      metrics.WorldSurfaceHigh,
      metrics.RockLayerLow,
      metrics.RockLayer,
      metrics.RockLayerHigh,
      metrics.SnowTop,
      metrics.SnowBottom,
      metrics.SnowOriginLeft,
      metrics.SnowOriginRight,
      metrics.SnowMinX,
      metrics.SnowMaxX);
  }
}
