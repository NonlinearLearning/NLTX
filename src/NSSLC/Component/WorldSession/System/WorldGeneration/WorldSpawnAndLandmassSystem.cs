using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits one complete spawn and landmass snapshot for its generation session.
/// </summary>
public static class WorldSpawnAndLandmassSystem
{
  public static void Commit(
    WorldSpawnAndLandmassComponent component,
    in WorldSpawnAndLandmassSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (snapshot.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Spawn and landmass facts cannot be committed to another generation.",
        nameof(snapshot));
    }

    component.ReplaceState(
      snapshot.WorldSpawnHasBeenRandomized,
      snapshot.LandmassData,
      snapshot.RemixSurfaceLayerLow,
      snapshot.RemixSurfaceLayerHigh,
      snapshot.RemixMushroomLayerLow,
      snapshot.RemixMushroomLayerHigh,
      snapshot.BoulderPetsPlaced);
  }
}
