using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Physics.Systems;

public sealed class TopSlopeContactSystem
{
  private const float CollisionBoundaryTolerance = 0.0001f;

  private static readonly TileDefinitionRegistry DefaultTileDefinitions =
    TileDefinitionRegistry.CreateVersion4Base();

  private readonly TileDefinitionRegistry _tileDefinitions;

  public TopSlopeContactSystem()
    : this(DefaultTileDefinitions)
  {
  }

  public TopSlopeContactSystem(TileDefinitionRegistry tileDefinitions)
  {
    _tileDefinitions = tileDefinitions ?? throw new ArgumentNullException(nameof(tileDefinitions));
  }

  public void Resolve(
    WorldGrid world,
    ref LocationComponent transform,
    ref VelocityComponent velocity,
    ref PhysicsStateComponent physics,
    ColliderComponent collider)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (velocity.Y >= 0.0f)
    {
      return;
    }

    float previousFootY = transform.Y - velocity.Y;
    float highestSurface = float.NegativeInfinity;
    int firstTileY = (int)MathF.Floor(transform.Y - CollisionBoundaryTolerance);
    int lastTileY = (int)MathF.Floor(previousFootY + CollisionBoundaryTolerance);
    for (int tileY = firstTileY; tileY <= lastTileY; tileY++)
    {
      for (int tileX = (int)MathF.Floor(transform.X);
           tileX <= (int)MathF.Floor(transform.X + collider.Width - CollisionBoundaryTolerance);
           tileX++)
      {
        if (!TryGetTopSurface(world, tileX, tileY, transform, collider, out float surfaceY))
        {
          continue;
        }

        if (transform.Y > surfaceY + CollisionBoundaryTolerance ||
            previousFootY < surfaceY - CollisionBoundaryTolerance)
        {
          continue;
        }

        highestSurface = MathF.Max(highestSurface, surfaceY);
      }
    }

    if (float.IsNegativeInfinity(highestSurface))
    {
      return;
    }

    transform.Y = highestSurface;
    velocity.Y = 0.0f;
    physics.IsGrounded = true;
  }

  private bool TryGetTopSurface(
    WorldGrid world,
    int tileX,
    int tileY,
    LocationComponent transform,
    ColliderComponent collider,
    out float surfaceY)
  {
    surfaceY = 0.0f;
    if (!world.Contains(tileX, tileY))
    {
      return false;
    }

    WorldTile tile = world.GetTile(tileX, tileY);
    if (!tile.IsActive || tile.IsInactive)
    {
      return false;
    }

    if (!_tileDefinitions.TryGet(tile.Type, out TileDefinition definition))
    {
      throw new InvalidOperationException(
        $"Top slope contact cannot resolve unknown active tile type {tile.Type}.");
    }

    if (tile.Slope is not (1 or 2) || !definition.BlocksLiquid || definition.IsPlatform)
    {
      return false;
    }

    float footX = tile.Slope == 1 ? transform.X : transform.X + collider.Width;
    float localX = footX - tileX;
    if (localX < 0.0f || localX > 1.0f)
    {
      return false;
    }

    surfaceY = tile.Slope == 1 ? tileY + 1.0f - localX : tileY + localX;
    return true;
  }
}
