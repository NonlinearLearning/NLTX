using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads committed jungle region facts without exposing component mutation.
/// </summary>
public static class JungleRegionStructureQuery
{
  public static JungleRegionStructureSnapshot Snapshot(
    JungleRegionStructureComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }

  public static bool IsWithinJungleConversionRange(
    in JungleRegionStructureSnapshot snapshot,
    int columnX)
  {
    return columnX >= snapshot.JungleMinX &&
      columnX <= snapshot.JungleMaxX;
  }
}
