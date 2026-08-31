using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileCollisionSystem
{
  private static readonly TileDefinitionRegistry DefaultTileDefinitions =
    TileDefinitionRegistry.CreateVersion4Base();

  private readonly TileDefinitionRegistry _tileDefinitions;

  public ProjectileCollisionSystem()
    : this(DefaultTileDefinitions)
  {
  }

  public ProjectileCollisionSystem(TileDefinitionRegistry tileDefinitions)
  {
    _tileDefinitions = tileDefinitions ?? throw new ArgumentNullException(nameof(tileDefinitions));
  }

  public bool HitsSolidTile(
    WorldGrid world,
    TransformComponent transform,
    ColliderComponent collider,
    bool fallThrough = false)
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
            IsBlockingTile(world.GetTile(x, y), fallThrough))
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
    ColliderComponent collider,
    bool fallThrough = false)
  {
    return PathHitsSolidTile(
      world,
      previousTransform,
      collider,
      currentTransform,
      collider,
      fallThrough);
  }

  public bool PathHitsSolidTile(
    WorldGrid world,
    TransformComponent previousTransform,
    ColliderComponent previousCollider,
    TransformComponent currentTransform,
    ColliderComponent currentCollider,
    bool fallThrough = false)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (!IsValidGeometry(previousTransform, previousCollider) ||
        !IsValidGeometry(currentTransform, currentCollider))
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
      ColliderComponent sampleCollider = new(
        Lerp(previousCollider.Width, currentCollider.Width, progress),
        Lerp(previousCollider.Height, currentCollider.Height, progress));
      if (HitsSolidTile(world, sample, sampleCollider, fallThrough))
      {
        return true;
      }
    }

    return false;
  }

  public bool TryGetSolidTileImpactTile(
    WorldGrid world,
    TransformComponent previousTransform,
    TransformComponent currentTransform,
    ColliderComponent collider,
    out int tileX,
    out int tileY,
    bool fallThrough = false)
  {
    ArgumentNullException.ThrowIfNull(world);
    tileX = 0;
    tileY = 0;
    if (!IsValidGeometry(previousTransform, collider) ||
        !IsValidGeometry(currentTransform, collider))
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
      if (TryGetBlockingTile(world, sample, collider, fallThrough, out tileX, out tileY))
      {
        return true;
      }
    }

    return false;
  }

  public bool TryGetSolidTileImpact(
    WorldGrid world,
    TransformComponent previousTransform,
    TransformComponent currentTransform,
    ColliderComponent collider,
    out bool reflectHorizontal,
    out bool reflectVertical,
    bool correctSlopeCollision = true,
    bool fallThrough = false)
  {
    reflectHorizontal = false;
    reflectVertical = false;
    if (!PathHitsSolidTile(world, previousTransform, currentTransform, collider, fallThrough))
    {
      return false;
    }

    reflectHorizontal = currentTransform.X != previousTransform.X;
    reflectVertical = currentTransform.Y != previousTransform.Y;
    if (correctSlopeCollision && TryGetSlopeImpactAxis(world, currentTransform, collider, out bool slopeHorizontal,
        out bool slopeVertical))
    {
      reflectHorizontal = slopeHorizontal;
      reflectVertical = slopeVertical;
    }

    if (!reflectHorizontal && !reflectVertical)
    {
      reflectVertical = true;
    }

    return true;
  }

  private static bool TryGetSlopeImpactAxis(
    WorldGrid world,
    TransformComponent transform,
    ColliderComponent collider,
    out bool reflectHorizontal,
    out bool reflectVertical)
  {
    reflectHorizontal = false;
    reflectVertical = false;
    int firstTileX = (int)MathF.Floor(transform.X);
    int lastTileX = (int)MathF.Floor(transform.X + collider.Width - float.Epsilon);
    int firstTileY = (int)MathF.Floor(transform.Y);
    int lastTileY = (int)MathF.Floor(transform.Y + collider.Height - float.Epsilon);
    for (int y = firstTileY; y <= lastTileY; y++)
    {
      for (int x = firstTileX; x <= lastTileX; x++)
      {
        if (x < 0 || x >= world.Width || y < 0 || y >= world.Height)
        {
          continue;
        }

        WorldTile tile = world.GetTile(x, y);
        if (!tile.IsActive || tile.IsInactive)
        {
          continue;
        }

        if (tile.Slope is 1 or 2 || tile.IsHalfBrick && tile.Slope == 0)
        {
          reflectVertical = true;
          return true;
        }

        if (tile.Slope is 3 or 4)
        {
          reflectHorizontal = true;
          return true;
        }
      }
    }

    return false;
  }

  public bool HitsLiquid(
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
        if (x >= 0 && x < world.Width && y >= 0 && y < world.Height &&
            world.GetTile(x, y).LiquidAmount > 0)
        {
          return true;
        }
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

  private bool TryGetBlockingTile(
    WorldGrid world,
    TransformComponent transform,
    ColliderComponent collider,
    bool fallThrough,
    out int tileX,
    out int tileY)
  {
    tileX = 0;
    tileY = 0;
    int firstTileX = (int)MathF.Floor(transform.X);
    int lastTileX = (int)MathF.Floor(transform.X + collider.Width - float.Epsilon);
    int firstTileY = (int)MathF.Floor(transform.Y);
    int lastTileY = (int)MathF.Floor(transform.Y + collider.Height - float.Epsilon);
    for (int y = firstTileY; y <= lastTileY; y++)
    {
      for (int x = firstTileX; x <= lastTileX; x++)
      {
        if (x < 0 || x >= world.Width || y < 0 || y >= world.Height ||
            IsBlockingTile(world.GetTile(x, y), fallThrough))
        {
          tileX = x;
          tileY = y;
          return true;
        }
      }
    }

    return false;
  }

  private bool IsBlockingTile(WorldTile tile, bool fallThrough)
  {
    if (!tile.IsActive || tile.IsInactive)
    {
      return false;
    }

    return !fallThrough || !_tileDefinitions.TryGet(tile.Type, out TileDefinition definition) ||
      !definition.IsPlatform;
  }

  private static float Lerp(float first, float second, float progress)
  {
    return first + (second - first) * progress;
  }
}
