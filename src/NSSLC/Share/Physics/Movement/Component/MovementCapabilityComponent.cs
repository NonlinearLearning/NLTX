using System;

namespace Terraria.Movement;

/// <summary>
/// Declares movement limits and jump capability. It contains no movement algorithm.
/// </summary>
public readonly record struct MovementCapabilityComponent
{
 public MovementCapabilityComponent(
   bool enabled,
   float maximumSpeed,
   float acceleration,
   bool canJump = false,
   float jumpSpeed = 0f,
   MovementMode mode = MovementMode.Grounded)
 {
  if (!float.IsFinite(maximumSpeed) || maximumSpeed < 0f)
  {
   throw new ArgumentOutOfRangeException(nameof(maximumSpeed));
  }

  if (!float.IsFinite(acceleration) || acceleration < 0f)
  {
   throw new ArgumentOutOfRangeException(nameof(acceleration));
  }

  if (!float.IsFinite(jumpSpeed) || jumpSpeed < 0f)
  {
   throw new ArgumentOutOfRangeException(nameof(jumpSpeed));
  }

  Enabled = enabled;
  MaximumSpeed = maximumSpeed;
  Acceleration = acceleration;
  CanJump = canJump;
  JumpSpeed = jumpSpeed;
  Mode = mode;
 }

 public bool Enabled { get; }

 public float MaximumSpeed { get; }

 public float Acceleration { get; }

 public bool CanJump { get; }

 public float JumpSpeed { get; }

 public MovementMode Mode { get; }
}
