using System;
using System.Numerics;

namespace Terraria.Projectile;

/// <summary>
/// Immutable world-edge snapshot for one projectile update pass.
/// </summary>
public readonly record struct ProjectileWorldBounds
{
  public ProjectileWorldBounds(
    float leftWorld,
    float topWorld,
    float rightWorld,
    float bottomWorld)
  {
    ValidateFinite(leftWorld, nameof(leftWorld));
    ValidateFinite(topWorld, nameof(topWorld));
    ValidateFinite(rightWorld, nameof(rightWorld));
    ValidateFinite(bottomWorld, nameof(bottomWorld));
    if (leftWorld >= rightWorld)
    {
      throw new ArgumentOutOfRangeException(nameof(rightWorld));
    }

    if (topWorld >= bottomWorld)
    {
      throw new ArgumentOutOfRangeException(nameof(bottomWorld));
    }

    LeftWorld = leftWorld;
    TopWorld = topWorld;
    RightWorld = rightWorld;
    BottomWorld = bottomWorld;
  }

  public float LeftWorld { get; }

  public float TopWorld { get; }

  public float RightWorld { get; }

  public float BottomWorld { get; }

  public bool IsValid =>
    float.IsFinite(LeftWorld) &&
    float.IsFinite(TopWorld) &&
    float.IsFinite(RightWorld) &&
    float.IsFinite(BottomWorld) &&
    LeftWorld < RightWorld &&
    TopWorld < BottomWorld;

  public bool ContainsProjectile(Vector2 position, float width, float height)
  {
    if (!IsValid)
    {
      throw new InvalidOperationException(
        "World bounds must be initialized by the validated constructor.");
    }

    return position.X > LeftWorld &&
      position.X + width < RightWorld &&
      position.Y > TopWorld &&
      position.Y + height < BottomWorld;
  }

  private static void ValidateFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
