using System.Collections.Generic;
using System.Numerics;
using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.Physics;

public struct CollisionResultComponent
{
  public List<EntityReference>? TouchedEntities;
  public List<TileCoordinate>? TouchedTiles;

  public CollisionResultComponent()
    : this(collidedOnX: false, collidedOnY: false)
  {
  }

  public CollisionResultComponent(bool collidedOnX, bool collidedOnY)
  {
    CollidedOnX = collidedOnX;
    CollidedOnY = collidedOnY;
    BlockedAxes = (collidedOnX ? CollisionAxisMask.Horizontal : CollisionAxisMask.None) |
      (collidedOnY ? CollisionAxisMask.Vertical : CollisionAxisMask.None);
  }

  public Vector2 BlockingNormal;
  public CollisionAxisMask BlockedAxes;
  public bool CollidedOnX;
  public bool CollidedOnY;
  public bool DidStepUp;
  public long? ResolvedAtTick;
  public bool HasEntityContact => TouchedEntities is { Count: > 0 };
  public bool HasTileContact => TouchedTiles is { Count: > 0 };
}
