using System;
using System.Numerics;

namespace Terraria.Movement;

/// <summary>Identifies the domain that produced a one-step movement request.</summary>
public enum MovementIntentSource : byte
{
  Input = 0,
  Ai = 1,
  ExternalForce = 2,
  Mount = 3,
  Teleport = 4
}

/// <summary>
/// A one-step movement request produced by input, AI or another authoritative system.
/// </summary>
public readonly record struct MovementIntent
{
 public MovementIntent(
   Vector2 desiredVelocity,
   long tick,
   int priority = 0,
   bool jumpRequested = false,
      bool hasDesiredVelocity = true,
      MovementIntentSource source = MovementIntentSource.Input,
      long sourceRevision = 0,
      float jumpSpeed = 0f)
 {
  if (!float.IsFinite(desiredVelocity.X) || !float.IsFinite(desiredVelocity.Y))
  {
   throw new ArgumentOutOfRangeException(nameof(desiredVelocity));
  }

  if (tick < 0)
  {
   throw new ArgumentOutOfRangeException(nameof(tick));
  }

    if (sourceRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sourceRevision));
    }

    if (!float.IsFinite(jumpSpeed) || jumpSpeed < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(jumpSpeed));
    }

  DesiredVelocity = desiredVelocity;
  Tick = tick;
  Priority = priority;
  JumpRequested = jumpRequested;
  HasDesiredVelocity = hasDesiredVelocity;
    Source = source;
    SourceRevision = sourceRevision;
    JumpSpeed = jumpSpeed;
 }

 public Vector2 DesiredVelocity { get; }

 public long Tick { get; }

 public int Priority { get; }

 public bool JumpRequested { get; }

 public bool HasDesiredVelocity { get; }

  public MovementIntentSource Source { get; }

  public long SourceRevision { get; }

  /// <summary>Optional per-request jump speed in world units per second; zero uses the capability.</summary>
  public float JumpSpeed { get; }

 public static MovementIntent Stop(long tick, int priority = 0)
 {
  return new MovementIntent(Vector2.Zero, tick, priority);
 }
}
