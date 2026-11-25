using System.Numerics;
using Terraria.Share.Liquid;

namespace Terraria.Share.Items;

/// <summary>
/// Per-tick world item motion arithmetic ported from Version4 <c>WorldItem.MoveInWorld</c>.
/// </summary>
/// <remarks>
/// Version4 item velocities are pixels per simulation tick and the position advances by exactly
/// that displacement. This solver keeps those units, so callers must not scale the result by the
/// ECS delta time; only the world motion query's tile sweep runs in the same units.
/// </remarks>
public static class ItemMotionSolver
{
  /// <summary>Version4 horizontal friction applied to a falling item: <c>velocity.X *= 0.95</c>.</summary>
  public const float HorizontalFriction = 0.95f;

  /// <summary>Version4 horizontal dead zone: magnitudes below this become zero.</summary>
  public const float HorizontalDeadZone = 0.1f;

  /// <summary>Version4 applies half the velocity while the item is submerged.</summary>
  public const float LiquidDisplacementScale = 0.5f;

  /// <summary>Chooses the integration profile from the liquid flags touching the item.</summary>
  public static ItemMotionProfile ProfileFor(LiquidContactFlags liquid)
  {
    if ((liquid & LiquidContactFlags.Shimmer) != LiquidContactFlags.None)
    {
      return ItemMotionProfile.Shimmer;
    }

    if ((liquid & LiquidContactFlags.Honey) != LiquidContactFlags.None)
    {
      return ItemMotionProfile.Honey;
    }

    if ((liquid & LiquidContactFlags.Water) != LiquidContactFlags.None)
    {
      return ItemMotionProfile.Water;
    }

    if ((liquid & LiquidContactFlags.Lava) != LiquidContactFlags.None)
    {
      return ItemMotionProfile.Lava;
    }

    return ItemMotionProfile.Air;
  }

  /// <summary>Integrates one tick of gravity, fall-speed clamping and horizontal friction.</summary>
  public static Vector2 Integrate(Vector2 velocity, ItemMotionProfile profile)
  {
    float vertical = velocity.Y + profile.Gravity;
    if (vertical > profile.MaximumFallSpeed)
    {
      vertical = profile.MaximumFallSpeed;
    }

    float horizontal = velocity.X * HorizontalFriction;
    if (horizontal is < HorizontalDeadZone and > -HorizontalDeadZone)
    {
      horizontal = 0f;
    }

    return new Vector2(horizontal, vertical);
  }

  /// <summary>Displacement applied for one tick; submerged items move at half velocity.</summary>
  public static Vector2 ToDisplacement(Vector2 velocity, ItemMotionProfile profile)
  {
    return profile.Wet ? velocity * LiquidDisplacementScale : velocity;
  }
}
