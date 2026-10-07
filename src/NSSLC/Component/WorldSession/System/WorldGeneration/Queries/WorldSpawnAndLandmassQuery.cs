using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads a copied spawn and landmass snapshot.
/// </summary>
public static class WorldSpawnAndLandmassQuery
{
  public static WorldSpawnAndLandmassSnapshot Snapshot(
    WorldSpawnAndLandmassComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
