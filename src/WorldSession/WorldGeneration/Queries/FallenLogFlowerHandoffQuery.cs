using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads the copied fallen-log handoff without changing its pending state.
/// </summary>
public static class FallenLogFlowerHandoffQuery
{
  public static FallenLogFlowerHandoffSnapshot Snapshot(
    FallenLogFlowerHandoffComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
