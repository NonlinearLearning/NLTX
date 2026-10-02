using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

public static class ShimmerAnchorQuery
{
  public static ShimmerBiomeAnchorSnapshot Snapshot(
    ShimmerBiomeAnchorComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }

  public static bool HasAnchorWithinDistance(
    ShimmerBiomeAnchorSnapshot snapshot,
    ShimmerBiomeAnchorPoint candidate,
    double distance)
  {
    if (!snapshot.HasAnchor || distance <= 0)
    {
      return false;
    }

    double deltaX = candidate.X - snapshot.Position.X;
    double deltaY = candidate.Y - snapshot.Position.Y;
    double squaredDistance = deltaX * deltaX + deltaY * deltaY;
    return squaredDistance < distance * distance;
  }
}
