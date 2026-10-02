using System.Numerics;

namespace Terraria.Physics;

public struct PhysicsStateComponent
{
  public Vector2 Acceleration;
  public GravityDirection GravityDirection;
  public float GravityScale;
  public bool IsGrounded;
  public bool IsMovementLocked;

  public bool IsGravityInverted => GravityDirection == GravityDirection.Up;
}
