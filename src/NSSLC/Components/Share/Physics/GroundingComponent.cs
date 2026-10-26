using System;
using System.Numerics;

namespace Terraria.Physics;

/// <summary>Stores the last committed support contact for jump and movement rules.</summary>
public readonly record struct GroundingComponent
{
  public GroundingComponent(bool isGrounded, bool onPlatform, Vector2 surfaceNormal)
  {
    if (!float.IsFinite(surfaceNormal.X) || !float.IsFinite(surfaceNormal.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(surfaceNormal));
    }

    IsGrounded = isGrounded;
    OnPlatform = onPlatform;
    SurfaceNormal = surfaceNormal;
  }

  public bool IsGrounded { get; }

  public bool OnPlatform { get; }

  public Vector2 SurfaceNormal { get; }

  public void Deconstruct(
      out bool isGrounded,
      out bool onPlatform,
      out Vector2 surfaceNormal)
  {
    isGrounded = IsGrounded;
    onPlatform = OnPlatform;
    surfaceNormal = SurfaceNormal;
  }

  public static GroundingComponent Airborne =>
      new(false, false, Vector2.Zero);
}
