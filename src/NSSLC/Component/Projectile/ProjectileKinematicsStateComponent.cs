using System;
using System.Numerics;

namespace Terraria.Projectile;

public struct ProjectileKinematicsStateComponent
{
  public ProjectileKinematicsStateComponent(Vector2 position, Vector2 velocity)
  {
    ValidateFinite(position, nameof(position));
    ValidateFinite(velocity, nameof(velocity));
    Position = position;
    Velocity = velocity;
  }

  public Vector2 Position;
  public Vector2 Velocity;

  private static void ValidateFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
