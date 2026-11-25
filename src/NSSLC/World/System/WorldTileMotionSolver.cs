using System;
using System.Numerics;
using Terraria.Share.Movement;
using Terraria.Share.Physics;

namespace Terraria.Unshared.World;

/// <summary>
/// Pure tile collision arithmetic for one movement query.
/// </summary>
/// <remarks>
/// The solver reads only through <see cref="IWorldTileGrid" /> and never touches storage, locks,
/// revisions or mutations. <see cref="WorldRuntime" /> keeps the row lock and projects the result
/// onto the spatial query port.
/// </remarks>
public static class WorldTileMotionSolver
{
  private const int CollisionSearchIterations = 12;
  private static readonly WorldFacts TileFacts = WorldFacts.CreateVersion4();

  /// <summary>Resolves one movement query against the tile grid.</summary>
  public static SpatialTileMotionResult Move(
      IWorldTileGrid grid,
      Vector2 position,
      Vector2 velocity,
      Vector2 colliderSize,
      TileCollisionCapabilityComponent capability,
      GravityDirection gravityDirection)
  {
    ArgumentNullException.ThrowIfNull(grid);
    if (!Terraria.Share.Physics.SimulationVector.IsFinite(position) || !Terraria.Share.Physics.SimulationVector.IsFinite(velocity) || !Terraria.Share.Physics.SimulationVector.IsFinite(colliderSize) ||
        colliderSize.X <= 0f || colliderSize.Y <= 0f)
    {
      return new SpatialTileMotionResult(position, Vector2.Zero, false, false);
    }

    if (!capability.Enabled)
    {
      return new SpatialTileMotionResult(position + velocity, velocity, false, false);
    }

    Vector2 current = ClampToWorld(grid, position, colliderSize);
    bool collidedX = false;
    bool collidedY = false;

    if (velocity.X != 0f)
    {
      float resolvedX = MoveAxis(
          grid,
          current,
          velocity.X,
          colliderSize,
          horizontal: true,
          capability,
          out collidedX);
      current.X = resolvedX;
    }

    if (velocity.Y != 0f)
    {
      float resolvedY = MoveAxis(
          grid,
          current,
          velocity.Y,
          colliderSize,
          horizontal: false,
          capability,
          out collidedY);
      current.Y = resolvedY;
    }

    TileContactGeometry contact = collidedX || collidedY
        ? DescribeContact(grid, current, colliderSize, velocity, collidedX, collidedY)
        : default;

    // At large world coordinates a one-tick gravity displacement can be smaller than one
    // float ULP. Probe the support plane explicitly so an entity resting exactly on a tile
    // still becomes grounded and its vertical velocity is consumed.
    if (!collidedY && TrySnapToGround(
        grid,
        ref current,
        colliderSize,
        capability,
        gravityDirection,
        velocity.Y))
    {
      collidedY = true;
    }

    Vector2 resultingVelocity = new(
        collidedX ? 0f : velocity.X,
        collidedY ? 0f : velocity.Y);
    return new SpatialTileMotionResult(current, resultingVelocity, collidedX, collidedY, contact);
  }

  private static bool TrySnapToGround(
      IWorldTileGrid grid,
      ref Vector2 position,
      Vector2 colliderSize,
      TileCollisionCapabilityComponent capability,
      GravityDirection gravityDirection,
      float verticalVelocity)
  {
    bool movingTowardSupport = gravityDirection == GravityDirection.Normal
        ? verticalVelocity >= 0f
        : verticalVelocity <= 0f;
    if (!movingTowardSupport)
    {
      return false;
    }

    int minX = (int)Math.Clamp(
        MathF.Floor(position.X / grid.TileSize), 0, grid.Width - 1);
    int maxX = (int)Math.Clamp(
        MathF.Floor((position.X + colliderSize.X - 0.0001f) / grid.TileSize),
        0,
        grid.Width - 1);
    float supportY = gravityDirection == GravityDirection.Normal
        ? position.Y + colliderSize.Y
        : position.Y;
    const float maximumSnapDistance = 2f;
    int minY = gravityDirection == GravityDirection.Normal
        ? (int)Math.Clamp(MathF.Floor(supportY / grid.TileSize), 0, grid.Height - 1)
        : (int)Math.Clamp(MathF.Ceiling(supportY / grid.TileSize) - 1, 0, grid.Height - 1);
    for (int x = minX; x <= maxX; x++)
    {
      for (int y = Math.Max(0, minY - 1); y <= Math.Min(grid.Height - 1, minY + 1); y++)
      {
        WorldTileState tile = grid.ReadTile(new WorldTileCoordinate(x, y));
        if (!BlocksMovement(tile, capability))
        {
          continue;
        }

        float boundary = gravityDirection == GravityDirection.Normal
            ? y * grid.TileSize
            : (y + 1) * grid.TileSize;
        float distance = gravityDirection == GravityDirection.Normal
            ? boundary - supportY
            : supportY - boundary;
        if (distance < -0.0001f || distance > maximumSnapDistance)
        {
          continue;
        }

        position.Y = gravityDirection == GravityDirection.Normal
            ? boundary - colliderSize.Y
            : boundary;
        return true;
      }
    }

    return false;
  }

  private static bool BlocksMovement(
      WorldTileState tile,
      TileCollisionCapabilityComponent capability)
  {
    if (!tile.Active || tile.Inactive)
    {
      return false;
    }

    if (tile.Type >= TileFacts.Solid.Count)
    {
      return true;
    }

    bool solidTop = TileFacts.SolidTop[tile.Type];
    if (capability.FallThroughPlatforms && solidTop)
    {
      return false;
    }

    return TileFacts.Solid[tile.Type] || solidTop;
  }

  private static float MoveAxis(
      IWorldTileGrid grid,
      Vector2 current,
      float delta,
      Vector2 colliderSize,
      bool horizontal,
      TileCollisionCapabilityComponent capability,
      out bool collided)
  {
    float value = horizontal ? current.X : current.Y;
    float remaining = delta;
    float maximumStep = Math.Max(1f, grid.TileSize);
    collided = false;
    while (MathF.Abs(remaining) > 0.0001f)
    {
      float step = MathF.CopySign(MathF.Min(MathF.Abs(remaining), maximumStep), remaining);
      float target = value + step;
      Vector2 position = current;
      if (horizontal)
      {
        position.X = value;
      }
      else
      {
        position.Y = value;
      }

      float resolved = ResolveAxis(grid, position, target, colliderSize, horizontal, capability);
      if (MathF.Abs(resolved - target) > 0.0001f)
      {
        value = resolved;
        collided = true;
        break;
      }

      value = resolved;
      remaining -= step;
      if (horizontal)
      {
        current.X = value;
      }
      else
      {
        current.Y = value;
      }
    }

    return value;
  }

  private static float ResolveAxis(
      IWorldTileGrid grid,
      Vector2 current,
      float target,
      Vector2 colliderSize,
      bool horizontal,
      TileCollisionCapabilityComponent capability)
  {
    Vector2 currentPosition = current;
    if (horizontal)
    {
      currentPosition.X = current.X;
    }
    else
    {
      currentPosition.Y = current.Y;
    }

    if (Collides(grid, currentPosition, colliderSize, capability))
    {
      return horizontal ? current.X : current.Y;
    }

    Vector2 targetPosition = currentPosition;
    if (horizontal)
    {
      targetPosition.X = target;
    }
    else
    {
      targetPosition.Y = target;
    }

    if (!Collides(grid, targetPosition, colliderSize, capability))
    {
      return target;
    }

    float safe = horizontal ? current.X : current.Y;
    float blocked = target;
    for (int iteration = 0; iteration < CollisionSearchIterations; iteration++)
    {
      float middle = safe + (blocked - safe) / 2f;
      Vector2 probe = currentPosition;
      if (horizontal)
      {
        probe.X = middle;
      }
      else
      {
        probe.Y = middle;
      }

      if (Collides(grid, probe, colliderSize, capability))
      {
        blocked = middle;
      }
      else
      {
        safe = middle;
      }
    }

    return safe;
  }

  private static bool Collides(
      IWorldTileGrid grid,
      Vector2 position,
      Vector2 colliderSize,
      TileCollisionCapabilityComponent capability)
  {
    float worldWidth = (float)((double)grid.Width * grid.TileSize);
    float worldHeight = (float)((double)grid.Height * grid.TileSize);
    if (position.X < 0f || position.Y < 0f ||
        position.X + colliderSize.X > worldWidth ||
        position.Y + colliderSize.Y > worldHeight)
    {
      return true;
    }

    TileRange range = GetTileRange(grid, position, colliderSize);
    for (int y = range.MinY; y <= range.MaxY; y++)
    {
      for (int x = range.MinX; x <= range.MaxX; x++)
      {
        WorldTileState tile = grid.ReadTile(new WorldTileCoordinate(x, y));
        if (BlocksMovement(tile, capability))
        {
          return true;
        }
      }
    }

    return false;
  }

  /// <summary>把"撞了"翻译成几何事实：接触点取被挡住那一侧的表面，法线指向实体。</summary>
  /// <remarks>
  /// 位置是碰撞后的安全位置，所以接触点直接用碰撞盒对应的边：水平撞墙取迎面的 X 边，
  /// 垂直取迎面的 Y 边。瓦片索引由接触点往里挪半像素后换算，避免正好压在边界上取到空瓦片。
  /// </remarks>
  private static TileContactGeometry DescribeContact(
      IWorldTileGrid grid,
      Vector2 position,
      Vector2 colliderSize,
      Vector2 velocity,
      bool collidedX,
      bool collidedY)
  {
    int tileSize = Math.Max(1, grid.TileSize);
    float pointX;
    float pointY;
    Vector2 normal;
    if (collidedX)
    {
      pointX = velocity.X > 0f ? position.X + colliderSize.X : position.X;
      pointY = position.Y + colliderSize.Y * 0.5f;
      normal = velocity.X > 0f ? -Vector2.UnitX : Vector2.UnitX;
    }
    else
    {
      pointX = position.X + colliderSize.X * 0.5f;
      pointY = velocity.Y > 0f ? position.Y + colliderSize.Y : position.Y;
      normal = velocity.Y > 0f ? -Vector2.UnitY : Vector2.UnitY;
    }

    float probeX = pointX - normal.X * 0.5f;
    float probeY = pointY - normal.Y * 0.5f;
    int tileX = Math.Clamp((int)MathF.Floor(probeX / tileSize), 0, Math.Max(0, grid.Width - 1));
    int tileY = Math.Clamp((int)MathF.Floor(probeY / tileSize), 0, Math.Max(0, grid.Height - 1));
    return new TileContactGeometry(tileX, tileY, new Vector2(pointX, pointY), normal);
  }

  private static Vector2 ClampToWorld(
      IWorldTileGrid grid,
      Vector2 position,
      Vector2 colliderSize)
  {
    float worldWidth = (float)((double)grid.Width * grid.TileSize);
    float worldHeight = (float)((double)grid.Height * grid.TileSize);
    return new Vector2(
        Math.Clamp(position.X, 0f, Math.Max(0f, worldWidth - colliderSize.X)),
        Math.Clamp(position.Y, 0f, Math.Max(0f, worldHeight - colliderSize.Y)));
  }

  private static TileRange GetTileRange(
      IWorldTileGrid grid,
      Vector2 position,
      Vector2 colliderSize)
  {
    int minX = (int)Math.Clamp(
        Math.Floor((double)position.X / grid.TileSize), 0, grid.Width - 1);
    int minY = (int)Math.Clamp(
        Math.Floor((double)position.Y / grid.TileSize), 0, grid.Height - 1);
    int maxX = (int)Math.Clamp(
        Math.Floor(((double)position.X + colliderSize.X - 0.0001d) / grid.TileSize),
        0,
        grid.Width - 1);
    int maxY = (int)Math.Clamp(
        Math.Floor(((double)position.Y + colliderSize.Y - 0.0001d) / grid.TileSize),
        0,
        grid.Height - 1);
    return new TileRange(minX, maxX, minY, maxY);
  }

  private readonly record struct TileRange(int MinX, int MaxX, int MinY, int MaxY);
}
