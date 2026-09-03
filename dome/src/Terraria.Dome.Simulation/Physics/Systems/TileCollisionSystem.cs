using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Physics.Systems;

public sealed class TileCollisionSystem
{
  private const float CollisionBoundaryTolerance = 0.0001f;

  private static readonly TileDefinitionRegistry DefaultTileDefinitions =
    TileDefinitionRegistry.CreateVersion4Base();

  private readonly TileDefinitionRegistry _tileDefinitions;

  public TileCollisionSystem()
    : this(DefaultTileDefinitions)
  {
  }

  public TileCollisionSystem(TileDefinitionRegistry tileDefinitions)
  {
    _tileDefinitions = tileDefinitions ?? throw new ArgumentNullException(nameof(tileDefinitions));
  }

  public void MoveAndResolve(
    WorldGrid world,
    ref LocationComponent transform,
    ref VelocityComponent velocity,
    ref PhysicsStateComponent physics,
    ColliderComponent collider,
    bool fallThrough = false,
    bool deferTopSlopeCollision = false)
  {
    ArgumentNullException.ThrowIfNull(world);

    ValidateActiveTileDefinitions(world, transform, velocity, collider);
    physics.IsGrounded = false;
    MoveHorizontal(world, ref transform, ref velocity, collider, deferTopSlopeCollision);
    MoveVertical(
      world,
      ref transform,
      ref velocity,
      ref physics,
      collider,
      fallThrough,
      deferTopSlopeCollision);
  }

  private void MoveHorizontal(
    WorldGrid world,
    ref LocationComponent transform,
    ref VelocityComponent velocity,
    ColliderComponent collider,
    bool deferTopSlopeCollision)
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
        if (!ColumnOverlapsSolidTile(
              world,
              tileX,
              transform.Y,
              collider,
              deferTopSlopeCollision))
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
        if (!ColumnOverlapsSolidTile(
              world,
              tileX,
              transform.Y,
              collider,
              deferTopSlopeCollision))
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

  private void MoveVertical(
    WorldGrid world,
    ref LocationComponent transform,
    ref VelocityComponent velocity,
    ref PhysicsStateComponent physics,
    ColliderComponent collider,
    bool fallThrough,
    bool deferTopSlopeCollision)
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
        if (!TryGetSolidTileBottom(
              world,
              transform.X,
              tileY,
              collider,
              deferTopSlopeCollision,
              out float collisionBottom))
        {
          continue;
        }

        transform.Y = collisionBottom - collider.Height;
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
        if (!TryGetLandingTop(
              world,
              transform.X,
              tileY,
              collider,
              fallThrough,
              legacyVelocityY: -velocity.Y,
              deferTopSlopeCollision,
              out float landingTop))
        {
          continue;
        }

        transform.Y = landingTop;
        velocity.Y = 0.0f;
        physics.IsGrounded = true;
        return;
      }
    }

    transform.Y = targetY;
    if (velocity.Y < 0.0f && IsStandingOnSolidTile(
          world,
          transform.X,
          transform.Y,
           collider,
           fallThrough,
           legacyVelocityY: -velocity.Y,
           deferTopSlopeCollision))
    {
      velocity.Y = 0.0f;
      physics.IsGrounded = true;
    }
  }

  private bool IsStandingOnSolidTile(
    WorldGrid world,
    float x,
    float y,
    ColliderComponent collider,
    bool fallThrough,
    float legacyVelocityY,
    bool deferTopSlopeCollision)
  {
    int tileY = (int)MathF.Floor(y - CollisionBoundaryTolerance);
    return TryGetLandingTop(
      world,
      x,
      tileY,
      collider,
      fallThrough,
      legacyVelocityY,
      deferTopSlopeCollision,
      out float landingTop) && MathF.Abs(y - landingTop) <= CollisionBoundaryTolerance;
  }

  private bool ColumnOverlapsSolidTile(
    WorldGrid world,
    int tileX,
    float y,
    ColliderComponent collider,
    bool deferTopSlopeCollision)
  {
    int firstTileY = (int)MathF.Floor(y);
    int lastTileY = (int)MathF.Floor(y + collider.Height - float.Epsilon);
    for (int tileY = firstTileY; tileY <= lastTileY; tileY++)
    {
      if (TryGetSolidTileBounds(
            world,
            tileX,
            tileY,
            deferTopSlopeCollision,
            out float bottom,
            out float top) &&
          y + collider.Height > bottom && y < top)
      {
        return true;
      }
    }

    return false;
  }

  private bool TryGetSolidTileBottom(
    WorldGrid world,
    float x,
    int tileY,
    ColliderComponent collider,
    bool deferTopSlopeCollision,
    out float collisionBottom)
  {
    collisionBottom = 0.0f;
    int firstTileX = (int)MathF.Floor(x);
    int lastTileX = (int)MathF.Floor(x + collider.Width - float.Epsilon);
    for (int tileX = firstTileX; tileX <= lastTileX; tileX++)
    {
      if (!TryGetSolidTileBounds(
            world,
            tileX,
            tileY,
            deferTopSlopeCollision,
            out float bottom,
            out float _) ||
          x + collider.Width <= tileX || x >= tileX + 1.0f)
      {
        continue;
      }

      collisionBottom = bottom;
      return true;
    }

    return false;
  }

  private bool TryGetLandingTop(
    WorldGrid world,
    float x,
    int tileY,
    ColliderComponent collider,
    bool fallThrough,
    float legacyVelocityY,
    bool deferTopSlopeCollision,
    out float landingTop)
  {
    landingTop = 0.0f;
    bool found = false;
    int firstTileX = (int)MathF.Floor(x);
    int lastTileX = (int)MathF.Floor(x + collider.Width - float.Epsilon);
    for (int tileX = firstTileX; tileX <= lastTileX; tileX++)
    {
      if (!TryGetLandingTileTop(
            world,
            tileX,
            tileY,
            fallThrough,
            legacyVelocityY,
            deferTopSlopeCollision,
            out float tileTop))
      {
        continue;
      }

      landingTop = found ? MathF.Max(landingTop, tileTop) : tileTop;
      found = true;
    }

    return found;
  }

  private bool TryGetSolidTileBounds(
    WorldGrid world,
    int x,
    int y,
    bool deferTopSlopeCollision,
    out float bottom,
    out float top)
  {
    bottom = y;
    top = y + 1.0f;
    if (x < 0 || x >= world.Width || y < 0 || y >= world.Height)
    {
      return true;
    }

    WorldTile tile = world.GetTile(x, y);
    if (!tile.IsActive || tile.IsInactive)
    {
      return false;
    }

    if (!_tileDefinitions.TryGet(tile.Type, out TileDefinition definition))
    {
      throw new InvalidOperationException(
        $"Tile collision cannot resolve unknown active tile type {tile.Type}.");
    }

    if (!definition.BlocksLiquid || definition.IsPlatform)
    {
      return false;
    }

    if (deferTopSlopeCollision && tile.Slope is 1 or 2)
    {
      return false;
    }

    if (tile.IsHalfBrick && tile.Slope == 0)
    {
      top -= 0.5f;
    }

    return true;
  }

  private bool TryGetLandingTileTop(
    WorldGrid world,
    int x,
    int y,
    bool fallThrough,
    float legacyVelocityY,
    bool deferTopSlopeCollision,
    out float top)
  {
    top = y + 1.0f;
    if (x < 0 || x >= world.Width || y < 0 || y >= world.Height)
    {
      return true;
    }

    WorldTile tile = world.GetTile(x, y);
    if (!tile.IsActive || tile.IsInactive)
    {
      return false;
    }

    if (!_tileDefinitions.TryGet(tile.Type, out TileDefinition definition))
    {
      throw new InvalidOperationException(
        $"Tile collision cannot resolve unknown active tile type {tile.Type}.");
    }

    if (!definition.BlocksLiquid)
    {
      return false;
    }

    if (definition.IsPlatform)
    {
      return PlatformCollisionRuleSystem.ShouldCollideFromAbove(
        isPlatform: true,
        isProperTopFrame: tile.FrameY == 0,
        fallThrough: fallThrough,
        fall2: false,
        legacyVelocityY: legacyVelocityY);
    }

    if (deferTopSlopeCollision && tile.Slope is 1 or 2)
    {
      return false;
    }

    if (tile.IsHalfBrick && tile.Slope == 0)
    {
      top -= 0.5f;
    }

    return true;
  }

  private void ValidateActiveTileDefinitions(
    WorldGrid world,
    LocationComponent transform,
    VelocityComponent velocity,
    ColliderComponent collider)
  {
    float targetX = transform.X + velocity.X;
    float targetY = transform.Y + velocity.Y;
    int firstTileX = (int)MathF.Floor(MathF.Min(transform.X, targetX));
    int lastTileX = (int)MathF.Floor(
      MathF.Max(transform.X + collider.Width, targetX + collider.Width) - float.Epsilon);
    int firstTileY = (int)MathF.Floor(MathF.Min(transform.Y, targetY));
    int lastTileY = (int)MathF.Floor(
      MathF.Max(transform.Y + collider.Height, targetY + collider.Height) - float.Epsilon);

    for (int tileX = firstTileX; tileX <= lastTileX; tileX++)
    {
      for (int tileY = firstTileY; tileY <= lastTileY; tileY++)
      {
        if (!world.Contains(tileX, tileY))
        {
          continue;
        }

        WorldTile tile = world.GetTile(tileX, tileY);
        if (!tile.IsActive || tile.IsInactive)
        {
          continue;
        }

        if (!_tileDefinitions.TryGet(tile.Type, out TileDefinition _))
        {
          throw new InvalidOperationException(
            $"Tile collision cannot resolve unknown active tile type {tile.Type}.");
        }
      }
    }
  }
}
