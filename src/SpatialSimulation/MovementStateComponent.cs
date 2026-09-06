using System;
using System.Numerics;

using Terraria.Physics;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.MOVEMENT_STATE
// crossSubsystemOwner: integration-review
public struct MovementStateComponent
{
  public MovementStateComponent(
    Vector2 position = default,
    Vector2 velocity = default,
    Vector2 acceleration = default,
    GravityDirection gravityDirection = GravityDirection.Down,
    float gravityScale = 0.0f,
    bool isGrounded = false,
    bool isMovementLocked = false)
  {
    EnsureFinite(position, nameof(position));
    EnsureFinite(velocity, nameof(velocity));
    EnsureFinite(acceleration, nameof(acceleration));

    if (!float.IsFinite(gravityScale))
    {
      throw new ArgumentOutOfRangeException(
        nameof(gravityScale),
        gravityScale,
        "GravityScale must be finite.");
    }

    Position = position;
    Velocity = velocity;
    Acceleration = acceleration;
    GravityDirection = gravityDirection;
    GravityScale = gravityScale;
    IsGrounded = isGrounded;
    IsMovementLocked = isMovementLocked;
  }

  public Vector2 Position;
  public Vector2 Velocity;
  public Vector2 Acceleration;
  public GravityDirection GravityDirection;
  public float GravityScale;
  public bool IsGrounded;
  public bool IsMovementLocked;

  public bool IsGravityInverted =>
    GravityDirection == GravityDirection.Up;

  private static void EnsureFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Vector components must be finite.");
    }
  }
}
