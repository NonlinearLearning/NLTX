using System;
using EntityEcs.Components;

namespace Terraria.Dome.Simulation.Movement.Components;

public struct MotionHistoryComponent
{
  public long? RecordedAtTick;
  public LocationComponent PreviousPosition;
  public VelocityComponent PreviousVelocity;
  public MotionHistoryKind Kind;

  public void Record(
    long tick,
    LocationComponent position,
    VelocityComponent velocity,
    MotionHistoryKind kind = MotionHistoryKind.Tick)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) ||
        !float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y))
    {
      throw new ArgumentException("Motion history requires finite position and velocity values.");
    }

    if (!Enum.IsDefined(kind))
    {
      throw new ArgumentOutOfRangeException(nameof(kind));
    }

    RecordedAtTick = tick;
    PreviousPosition = position;
    PreviousVelocity = velocity;
    Kind = kind;
  }

  public void Clear()
  {
    RecordedAtTick = null;
    PreviousPosition = default;
    PreviousVelocity = default;
    Kind = MotionHistoryKind.Tick;
  }
}
