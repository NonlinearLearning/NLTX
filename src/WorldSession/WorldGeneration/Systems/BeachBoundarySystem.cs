using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits one complete beach-boundary snapshot for its generation session.
/// </summary>
public static class BeachBoundarySystem
{
  public static void Commit(
    BeachBoundaryComponent component,
    in BeachBoundarySnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (snapshot.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Beach boundaries cannot be committed to another generation.",
        nameof(snapshot));
    }

    component.ReplaceBoundaries(
      snapshot.LeftBeachEnd,
      snapshot.RightBeachStart,
      snapshot.BeachBordersWidth,
      snapshot.BeachSandRandomCenter,
      snapshot.BeachSandRandomWidthRange,
      snapshot.BeachSandDungeonExtraWidth,
      snapshot.BeachSandJungleExtraWidth,
      snapshot.ShellStartXLeft,
      snapshot.ShellStartYLeft,
      snapshot.ShellStartXRight,
      snapshot.ShellStartYRight,
      snapshot.OceanWaterStartRandomMin);
  }
}
