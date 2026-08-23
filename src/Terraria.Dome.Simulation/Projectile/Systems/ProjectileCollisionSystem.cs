using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileCollisionSystem
{
  public bool HitsSolidTile(
    WorldGrid world,
    TransformComponent transform,
    ColliderComponent collider)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (!IsValidGeometry(transform, collider))
    {
      return false;
    }

    int firstTileX = (int)MathF.Floor(transform.X);
    int lastTileX = (int)MathF.Floor(transform.X + collider.Width - float.Epsilon);
    int firstTileY = (int)MathF.Floor(transform.Y);
    int lastTileY = (int)MathF.Floor(transform.Y + collider.Height - float.Epsilon);
    for (int y = firstTileY; y <= lastTileY; y++)
    {
      for (int x = firstTileX; x <= lastTileX; x++)
      {
        if (x < 0 || x >= world.Width || y < 0 || y >= world.Height ||
            world.GetTile(x, y).IsActive)
        {
          return true;
        }
      }
    }

    return false;
  }

  public bool PathHitsSolidTile(
    WorldGrid world,
    TransformComponent previousTransform,
    TransformComponent currentTransform,
    ColliderComponent collider)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (!IsValidGeometry(previousTransform, collider) ||
        !float.IsFinite(currentTransform.X) || !float.IsFinite(currentTransform.Y))
    {
      return false;
    }

    int steps = GetStepCount(previousTransform, currentTransform);
    for (int index = 0; index <= steps; index++)
    {
      float progress = (float)index / steps;
      TransformComponent sample = new(
        Lerp(previousTransform.X, currentTransform.X, progress),
        Lerp(previousTransform.Y, currentTransform.Y, progress));
      if (HitsSolidTile(world, sample, collider))
      {
        return true;
      }
    }

    return false;
  }

  private static int GetStepCount(
    TransformComponent previousTransform,
    TransformComponent currentTransform)
  {
    float deltaX = currentTransform.X - previousTransform.X;
    float deltaY = currentTransform.Y - previousTransform.Y;
    return Math.Max(1, (int)MathF.Ceiling(MathF.Max(MathF.Abs(deltaX), MathF.Abs(deltaY))));
  }

  private static bool IsValidGeometry(
    TransformComponent transform,
    ColliderComponent collider)
  {
    return float.IsFinite(transform.X) && float.IsFinite(transform.Y) &&
      float.IsFinite(collider.Width) && collider.Width > 0.0f &&
      float.IsFinite(collider.Height) && collider.Height > 0.0f;
  }

  private static float Lerp(float first, float second, float progress)
  {
    return first + (second - first) * progress;
  }
}
