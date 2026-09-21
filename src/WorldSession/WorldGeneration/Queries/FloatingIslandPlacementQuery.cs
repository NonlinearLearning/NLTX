using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

public static class FloatingIslandPlacementQuery
{
  public static FloatingIslandPlacementSnapshot Snapshot(
    FloatingIslandPlacementStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
