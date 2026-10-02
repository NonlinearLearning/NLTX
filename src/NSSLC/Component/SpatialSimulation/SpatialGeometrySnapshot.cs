using System;
using System.Numerics;

namespace Terraria.SpatialSimulation;

// status: proposed
// snapshotId: SPATIAL.SNAPSHOT.GEOMETRY
// crossSubsystemOwner: integration-review
public readonly record struct SpatialGeometrySnapshot
{
  public SpatialGeometrySnapshot(
    long revision,
    Vector2 position,
    Vector2 size)
  {
    EnsureNonNegative(revision, nameof(revision));
    EnsureFinite(position, nameof(position));
    EnsureFinite(size, nameof(size));
    if (size.X < 0.0f || size.Y < 0.0f)
    {
      throw new ArgumentOutOfRangeException(
        nameof(size),
        size,
        "Geometry size must not be negative.");
    }

    Revision = revision;
    Position = position;
    Size = size;
  }

  public long Revision { get; }

  public Vector2 Position { get; }

  public Vector2 Size { get; }

  public float Left => Position.X;

  public float Right => Position.X + Size.X;

  public float Top => Position.Y;

  public float Bottom => Position.Y + Size.Y;

  public bool HasArea => Size.X > 0.0f && Size.Y > 0.0f;

  private static void EnsureFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Geometry components must be finite.");
    }
  }

  private static void EnsureNonNegative(long value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Revision must be non-negative.");
    }
  }
}
