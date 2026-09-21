using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads committed pyramid coordinates without exposing mutable arrays.
/// </summary>
public static class PyramidPlacementQuery
{
  public static PyramidPlacementSnapshot Snapshot(
    PyramidPlacementStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
