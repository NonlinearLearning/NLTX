using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads the committed layer measurements without exposing component mutation.
/// </summary>
public static class WorldLayerMetricsQuery
{
  public static WorldLayerMetricsSnapshot Snapshot(
    WorldLayerMetricsComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
