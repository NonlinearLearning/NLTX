using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads the committed beach-boundary facts without exposing component mutation.
/// </summary>
public static class BeachBoundaryQuery
{
  public static BeachBoundarySnapshot Snapshot(BeachBoundaryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
