using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Physics.Systems;

public sealed class TileCollisionSystem
{
  public void MoveAndResolve(
    WorldGrid world,
    ref TransformComponent transform,
    ref VelocityComponent velocity,
    ref PhysicsStateComponent physics,
    ColliderComponent collider)
  {
    ArgumentNullException.ThrowIfNull(world);

    physics.IsGrounded = false;
    MoveHorizontal(world, ref transform, ref velocity, collider);
    MoveVertical(world, ref transform, ref velocity, ref physics, collider);
  }

  private static void MoveHorizontal(
    WorldGrid world,
    ref TransformComponent transform,
    ref VelocityComponent velocity,
    ColliderComponent collider)
  {
    if (velocity.X == 0.0f)
    {
      return;
    }

    float targetX = transform.X + velocity.X;
    if (velocity.X > 0.0f)
    {
      int firstTileX = (int)MathF.Floor(transform.X + collider.Width);
      int lastTileX = (int)MathF.Floor(targetX + collider.Width - float.Epsilon);
      for (int tileX = firstTileX; tileX <= lastTileX; tileX++)
      {
        if (!ColumnOverlapsSolidTile(world, tileX, transform.Y, collider))
        {
          continue;
        }

        transform.X = tileX - collider.Width;
        velocity.X = 0.0f;
        return;
      }
    }
    else
    {
      int firstTileX = (int)MathF.Floor(transform.X - float.Epsilon);
      int lastTileX = (int)MathF.Floor(targetX);
      for (int tileX = firstTileX; tileX >= lastTileX; tileX--)
      {
        if (!ColumnOverlapsSolidTile(world, tileX, transform.Y, collider))
        {
          continue;
        }

        transform.X = tileX + 1.0f;
        velocity.X = 0.0f;
        return;
      }
    }

    transform.X = targetX;
  }

  private static void MoveVertical(
    WorldGrid world,
    ref TransformComponent transform,
    ref VelocityComponent velocity,
    ref PhysicsStateComponent physics,
    ColliderComponent collider)
  {
    if (velocity.Y == 0.0f)
    {
      return;
    }

    float targetY = transform.Y + velocity.Y;
    if (velocity.Y > 0.0f)
    {
      int firstTileY = (int)MathF.Floor(transform.Y + collider.Height);
      int lastTileY = (int)MathF.Floor(targetY + collider.Height - float.Epsilon);
      for (int tileY = firstTileY; tileY <= lastTileY; tileY++)
      {
        if (!RowOverlapsSolidTile(world, transform.X, tileY, collider))
        {
          continue;
        }

        transform.Y = tileY - collider.Height;
        velocity.Y = 0.0f;
        return;
      }
    }
    else
    {
      int firstTileY = (int)MathF.Floor(transform.Y - float.Epsilon);
      int lastTileY = (int)MathF.Floor(targetY);
      for (int tileY = firstTileY; tileY >= lastTileY; tileY--)
      {
        if (!RowOverlapsSolidTile(world, transform.X, tileY, collider))
        {
          continue;
        }

        transform.Y = tileY + 1.0f;
        velocity.Y = 0.0f;
        physics.IsGrounded = true;
        return;
      }
    }

    transform.Y = targetY;
    if (velocity.Y < 0.0f && IsStandingOnSolidTile(world, transform.X, transform.Y, collider))
    {
      velocity.Y = 0.0f;
      physics.IsGrounded = true;
    }
  }

  private static bool IsStandingOnSolidTile(
    WorldGrid world,
    float x,
    float y,
    ColliderComponent collider)
  {
    float top = MathF.Round(y);
    if (MathF.Abs(y - top) > float.Epsilon)
    {
      return false;
    }

    return RowOverlapsSolidTile(world, x, (int)top - 1, collider);
  }

  private static bool ColumnOverlapsSolidTile(
    WorldGrid world,
    int tileX,
    float y,
    ColliderComponent collider)
  {
    int firstTileY = (int)MathF.Floor(y);
    int lastTileY = (int)MathF.Floor(y + collider.Height - float.Epsilon);
    for (int tileY = firstTileY; tileY <= lastTileY; tileY++)
    {
      if (IsSolid(world, tileX, tileY))
      {
        return true;
      }
    }

    return false;
  }

  private static bool RowOverlapsSolidTile(
    WorldGrid world,
    float x,
    int tileY,
    ColliderComponent collider)
  {
    int firstTileX = (int)MathF.Floor(x);
    int lastTileX = (int)MathF.Floor(x + collider.Width - float.Epsilon);
    for (int tileX = firstTileX; tileX <= lastTileX; tileX++)
    {
      if (IsSolid(world, tileX, tileY))
      {
        return true;
      }
    }

    return false;
  }

  private static bool IsSolid(WorldGrid world, int x, int y)
  {
    if (x < 0 || x >= world.Width || y < 0 || y >= world.Height)
    {
      return true;
    }

    return world.GetTile(x, y).IsActive;
  }
}
