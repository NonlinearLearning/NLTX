using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits one complete underground-desert layout snapshot for its generation session.
/// </summary>
public static class UndergroundDesertStructureSystem
{
  public static void Commit(
    UndergroundDesertStructureComponent component,
    in UndergroundDesertStructureSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (snapshot.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Underground-desert layout cannot be committed to another generation.",
        nameof(snapshot));
    }

    component.ReplaceLayout(
      snapshot.UndergroundDesertLocation,
      snapshot.UndergroundDesertHiveLocation,
      snapshot.DesertHiveHigh,
      snapshot.DesertHiveLow,
      snapshot.DesertHiveLeft,
      snapshot.DesertHiveRight);
  }
}
