using System;
using Terraria.Share.Relationships;

namespace Terraria.Share.Physics;

/// <summary>
/// Tick-scoped geometric fact produced by Physics. Endpoint direction is intentionally neutral;
/// domain adapters decide which endpoint is an attacker, target or interaction participant.
/// </summary>
public readonly record struct PhysicsContactFact
{
  public PhysicsContactFact(
      EntityRuntimeHandle first,
      EntityRuntimeHandle second,
      EntityCollisionContact contact,
      long tick,
      SpatialWorldRevision revision,
      int substep = 0)
  {
    if (!first.IsValid || !second.IsValid || first.Matches(second))
    {
      throw new ArgumentException("Two distinct runtime handles are required.");
    }

    if (tick < 0 || !revision.IsValid || substep < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    First = first;
    Second = second;
    Contact = contact;
    Tick = tick;
    Revision = revision;
    Substep = substep;
  }

  public EntityRuntimeHandle First { get; }

  public EntityRuntimeHandle Second { get; }

  public EntityCollisionContact Contact { get; }

  public long Tick { get; }

  public SpatialWorldRevision Revision { get; }

  public int Substep { get; }
}
