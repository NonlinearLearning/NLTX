using System;
using System.Numerics;

namespace Terraria.Share.Movement;

/// <summary>
/// Stores world-space velocity in units per second. A spatial step applies
/// <c>LinearVelocity * deltaTime</c> as displacement.
/// </summary>
public struct VelocityComponent
{
 public VelocityComponent(Vector2 linearVelocity)
 {
  Validate(linearVelocity);
  LinearVelocity = linearVelocity;
 }

 private Vector2 _linearVelocity;

 public Vector2 LinearVelocity
 {
  get => _linearVelocity;
  set
  {
   Validate(value);
   _linearVelocity = value;
  }
 }

 public static VelocityComponent Stationary => new(Vector2.Zero);

 private static void Validate(Vector2 linearVelocity)
 {
  if (!float.IsFinite(linearVelocity.X) || !float.IsFinite(linearVelocity.Y))
  {
   throw new ArgumentOutOfRangeException(nameof(linearVelocity));
  }
 }
}
