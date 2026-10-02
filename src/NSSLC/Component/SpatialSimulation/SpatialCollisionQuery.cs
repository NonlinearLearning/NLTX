using System;
using System.Numerics;

namespace Terraria.SpatialSimulation;

// status: proposed
// queryId: SPATIAL.QUERY.COLLISION_AABB
// crossSubsystemOwner: integration-review
public static class SpatialCollisionQuery
{
  public static bool CheckAabb(
    in SpatialGeometrySnapshot first,
    in SpatialGeometrySnapshot second)
  {
    if (!first.HasArea || !second.HasArea)
    {
      return false;
    }

    return first.Left < second.Right &&
      first.Top < second.Bottom &&
      first.Right > second.Left &&
      first.Bottom > second.Top;
  }

  public static bool CheckAabb(
    Vector2 firstPosition,
    Vector2 firstSize,
    Vector2 secondPosition,
    Vector2 secondSize)
  {
    SpatialGeometrySnapshot first = new(
      0,
      firstPosition,
      firstSize);
    SpatialGeometrySnapshot second = new(
      0,
      secondPosition,
      secondSize);
    return CheckAabb(in first, in second);
  }

  internal static SpatialGeometrySnapshot CreateWetProbe(
    in SpatialGeometrySnapshot subject)
  {
    float probeWidth = MathF.Min(10.0f, subject.Size.X);
    float probeHeight = MathF.Min(subject.Size.Y / 2.0f, subject.Size.Y);
    Vector2 probeSize = new(probeWidth, probeHeight);
    Vector2 probePosition = subject.Position +
      (subject.Size - probeSize) / 2.0f;
    return new SpatialGeometrySnapshot(
      subject.Revision,
      probePosition,
      probeSize);
  }
}
