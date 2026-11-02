using System;
using System.Numerics;

namespace Terraria.Combat;

/// <summary>
/// Compatibility projection for callers that still use the former velocity namespace.
/// New spatial code uses <see cref="Terraria.Movement.VelocityComponent"/>.
/// </summary>
[Obsolete("Use Terraria.Movement.VelocityComponent for authoritative spatial state.")]
public readonly record struct VelocityComponent(Vector2 LinearVelocity)
{
  public Terraria.Movement.VelocityComponent ToMovementVelocity()
  {
    return new Terraria.Movement.VelocityComponent(LinearVelocity);
  }
}
