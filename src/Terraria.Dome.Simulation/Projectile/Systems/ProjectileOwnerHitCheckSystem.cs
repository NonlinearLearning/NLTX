using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileOwnerHitCheckSystem
{
  private readonly ProjectileCollisionSystem _collisionSystem = new();

  public bool CanHit(
    WorldGrid world,
    TransformComponent owner,
    ColliderComponent ownerCollider,
    int ownerFacing,
    float ownerGravityDirection,
    TransformComponent target,
    ColliderComponent targetCollider,
    float maximumDistance)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (!float.IsFinite(maximumDistance) || maximumDistance <= 0.0f)
    {
      return false;
    }

    float ownerX = owner.X + ownerCollider.Width / 2.0f;
    float ownerY = owner.Y + ownerCollider.Height / 2.0f;
    float targetX = target.X + targetCollider.Width / 2.0f;
    float targetY = target.Y + targetCollider.Height / 2.0f;
    float deltaX = targetX - ownerX;
    float deltaY = targetY - ownerY;
    if (deltaX * deltaX + deltaY * deltaY > maximumDistance * maximumDistance)
    {
      return false;
    }

    if (!_collisionSystem.PathHitsSolidTile(
          world,
          owner,
          ownerCollider,
          target,
          targetCollider))
    {
      return true;
    }

    int facing = ownerFacing < 0 ? -1 : 1;
    float gravityDirection = ownerGravityDirection < 0.0f ? -1.0f : 1.0f;
    return HasClearLine(world, ownerX, ownerY, targetX, targetY) ||
      HasClearLine(
        world,
        ownerX + facing * ownerCollider.Width / 2.0f,
        ownerY - gravityDirection * ownerCollider.Height / 3.0f,
        targetX,
        targetY - targetCollider.Height / 3.0f) ||
      HasClearLine(
        world,
        ownerX + facing * ownerCollider.Width / 2.0f,
        ownerY - gravityDirection * ownerCollider.Height / 3.0f,
        targetX,
        targetY) ||
      HasClearLine(
        world,
        ownerX + facing * ownerCollider.Width / 2.0f,
        ownerY,
        targetX,
        targetY + targetCollider.Height / 3.0f);
  }

  private static bool HasClearLine(
    WorldGrid world,
    float startX,
    float startY,
    float endX,
    float endY)
  {
    float deltaX = endX - startX;
    float deltaY = endY - startY;
    int steps = Math.Max(1, (int)MathF.Ceiling(MathF.Max(MathF.Abs(deltaX), MathF.Abs(deltaY))));
    for (int index = 1; index < steps; index++)
    {
      float progress = (float)index / steps;
      int tileX = (int)MathF.Floor(startX + deltaX * progress);
      int tileY = (int)MathF.Floor(startY + deltaY * progress);
      if (tileX < 0 || tileX >= world.Width || tileY < 0 || tileY >= world.Height ||
          world.GetTile(tileX, tileY).IsActive)
      {
        return false;
      }
    }

    return true;
  }
}
