using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits one complete ocean-biome constraint snapshot for its generation session.
/// </summary>
public static class OceanBiomeConstraintSystem
{
  public static void Commit(
    OceanBiomeConstraintComponent component,
    in OceanBiomeConstraintSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (snapshot.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Ocean-biome constraints cannot be committed to another generation.",
        nameof(snapshot));
    }

    component.ReplaceConstraints(
      snapshot.OceanWaterStartRandomMax,
      snapshot.OceanWaterForcedJungleLength,
      snapshot.EvilBiomeBeachAvoidance,
      snapshot.EvilBiomeAvoidanceMidFixer,
      snapshot.LakesBeachAvoidance,
      snapshot.SmallHolesBeachAvoidance,
      snapshot.SurfaceCavesBeachAvoidance,
      snapshot.SurfaceCavesBeachAvoidance2);
  }
}
