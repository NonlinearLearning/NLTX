using System;

namespace Terraria.Share.Physics;

/// <summary>
/// Immutable world input shared by all entities in one spatial batch.
/// </summary>
public readonly record struct SpatialFrame
{
  public SpatialFrame(long tick, SpatialWorldRevision revision, ISpatialWorldQuery world)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (!revision.IsValid || world.Revision != revision)
    {
      throw new ArgumentException(
          "The spatial world query does not match the supplied revision.", nameof(revision));
    }

    Tick = tick;
    Revision = revision;
    World = world;
  }

  public long Tick { get; }

  public SpatialWorldRevision Revision { get; }

  public ISpatialWorldQuery World { get; }
}
