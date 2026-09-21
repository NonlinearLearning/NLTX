using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads the committed underground-desert layout without exposing component mutation.
/// </summary>
public static class UndergroundDesertStructureQuery
{
  public static UndergroundDesertStructureSnapshot Snapshot(
    UndergroundDesertStructureComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }

  public static bool ContainsDesertTile(
    UndergroundDesertStructureComponent component,
    TilePosition position)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.UndergroundDesertLocation.Contains(position);
  }

  public static bool ContainsHiveTile(
    UndergroundDesertStructureComponent component,
    TilePosition position)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.UndergroundDesertHiveLocation.Contains(position);
  }

  public static bool IsHiveWithinDesert(
    UndergroundDesertStructureComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.UndergroundDesertLocation.Contains(
      component.UndergroundDesertHiveLocation);
  }

  public static bool IntersectsDesertArea(
    UndergroundDesertStructureComponent component,
    UndergroundDesertRectangle area)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.UndergroundDesertLocation.Intersects(area);
  }

  public static bool IntersectsHiveArea(
    UndergroundDesertStructureComponent component,
    UndergroundDesertRectangle area)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.UndergroundDesertHiveLocation.Intersects(area);
  }
}
