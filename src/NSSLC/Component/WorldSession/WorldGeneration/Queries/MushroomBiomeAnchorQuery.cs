using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Performs pure mushroom-anchor spacing checks over copied state.
/// </summary>
public static class MushroomBiomeAnchorQuery
{
  public static MushroomBiomeAnchorStateSnapshot Snapshot(
    MushroomBiomeAnchorStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }

  public static bool HasAnchorWithinDistance(
    MushroomBiomeAnchorStateSnapshot snapshot,
    TilePosition candidate,
    double distance)
  {
    if (distance <= 0 || snapshot.Count <= 0)
    {
      return false;
    }

    int inspectedCount = Math.Min(snapshot.Count, snapshot.Positions.Count);
    double distanceSquared = distance * distance;
    for (int index = 0; index < inspectedCount; index++)
    {
      TilePosition existing = snapshot.Positions[index];
      double deltaX = candidate.X - (double)existing.X;
      double deltaY = candidate.Y - (double)existing.Y;
      if (deltaX * deltaX + deltaY * deltaY < distanceSquared)
      {
        return true;
      }
    }

    return false;
  }
}
