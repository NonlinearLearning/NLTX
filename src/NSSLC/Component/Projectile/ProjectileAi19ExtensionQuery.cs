using System;
using System.Numerics;

namespace Terraria.Projectile;

/// <summary>
/// Computes the owner-dependent spear extension rectangle from explicit input.
/// </summary>
public static class ProjectileAi19ExtensionQuery
{
  public static ProjectileAi19ExtensionSnapshot Evaluate(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileAi19OwnerSnapshot owner)
  {

    if (!owner.HasResults)
    {
      return default;
    }

    int projectileType = projectile.Definition.ProjectileType;
    int extensionStartAnimation = owner.ItemAnimationMax / 3;
    if (owner.ItemAnimation < extensionStartAnimation)
    {
      return new ProjectileAi19ExtensionSnapshot(true, false, default);
    }

    float maximumReach;
    float maximumWidth;
    switch (projectileType)
    {
      case 105:
        maximumReach = 50.0f;
        maximumWidth = 20.0f;
        break;
      case 46:
        maximumReach = 50.0f;
        maximumWidth = 15.0f;
        break;
      case 153:
        maximumReach = 40.0f;
        maximumWidth = 10.0f;
        break;
      default:
        return new ProjectileAi19ExtensionSnapshot(true, false, default);
    }

    float animationProgress = RemapItemAnimation(
      owner.ItemAnimation,
      owner.ItemAnimationMax,
      extensionStartAnimation);
    maximumReach *= 1.0f / owner.MeleeSpeed;
    float reach = 10.0f + maximumReach * animationProgress;
    float width = 10.0f + maximumWidth * animationProgress;

    Vector2 center = projectile.Kinematics.Position + new Vector2(
      projectile.Width / 2.0f,
      projectile.Height / 2.0f);
    Vector2 velocity = projectile.Kinematics.Velocity;
    float velocityRotation = (float)Math.Atan2(velocity.Y, velocity.X);
    Vector2 direction = new(
      (float)Math.Cos(velocityRotation),
      (float)Math.Sin(velocityRotation));
    Vector2 extensionCenter = center + direction * reach;
    int extensionSize = (int)width;
    ProjectileCollisionTargetRectangle extensionHitbox = new(
      (int)(extensionCenter.X - width / 2.0f),
      (int)(extensionCenter.Y - width / 2.0f),
      extensionSize,
      extensionSize);

    return new ProjectileAi19ExtensionSnapshot(
      true,
      true,
      extensionHitbox);
  }

  private static float RemapItemAnimation(
    int itemAnimation,
    int itemAnimationMax,
    int extensionStartAnimation)
  {
    float from = itemAnimationMax;
    float to = extensionStartAnimation;
    float value = itemAnimation;
    if (from < to)
    {
      if (value < from)
      {
        return 0.0f;
      }

      if (value > to)
      {
        return 1.0f;
      }
    }
    else
    {
      if (value < to)
      {
        return 1.0f;
      }

      if (value > from)
      {
        return 0.0f;
      }
    }

    return (value - from) / (to - from);
  }
}
