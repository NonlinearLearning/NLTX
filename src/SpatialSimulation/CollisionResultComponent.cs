using System;
using System.Collections.Generic;
using System.Numerics;

using Terraria.Physics;
using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.COLLISION_RESULT
// crossSubsystemOwner: integration-review
public sealed class CollisionResultComponent
{
  private readonly List<EntityReference> _touchedEntities = new();
  private readonly List<TileCoordinate> _touchedTiles = new();

  public long? ResolvedAtTick { get; private set; }
  public Vector2 BlockingNormal { get; private set; }
  public CollisionAxisMask BlockedAxes { get; private set; }
  public bool DidStepUp { get; private set; }

  public IReadOnlyList<EntityReference> TouchedEntities =>
    _touchedEntities.AsReadOnly();

  public IReadOnlyList<TileCoordinate> TouchedTiles =>
    _touchedTiles.AsReadOnly();

  public bool CollidedOnX =>
    (BlockedAxes & CollisionAxisMask.Horizontal) !=
    CollisionAxisMask.None;

  public bool CollidedOnY =>
    (BlockedAxes & CollisionAxisMask.Vertical) !=
    CollisionAxisMask.None;

  public bool HasEntityContact => _touchedEntities.Count != 0;
  public bool HasTileContact => _touchedTiles.Count != 0;

  public void Replace(
    long tick,
    CollisionAxisMask blockedAxes,
    bool didStepUp,
    IReadOnlyList<TileCoordinate> touchedTiles,
    Vector2 blockingNormal = default,
    IReadOnlyList<EntityReference>? touchedEntities = null)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(tick),
        tick,
        "Tick must be non-negative.");
    }

    ArgumentNullException.ThrowIfNull(touchedTiles);
    EnsureFinite(blockingNormal, nameof(blockingNormal));
    EnsureKnownAxes(blockedAxes);

    _touchedTiles.Clear();
    for (int index = 0; index < touchedTiles.Count; index++)
    {
      _touchedTiles.Add(touchedTiles[index]);
    }

    _touchedEntities.Clear();
    if (touchedEntities is not null)
    {
      for (int index = 0; index < touchedEntities.Count; index++)
      {
        _touchedEntities.Add(touchedEntities[index]);
      }
    }

    ResolvedAtTick = tick;
    BlockingNormal = blockingNormal;
    BlockedAxes = blockedAxes;
    DidStepUp = didStepUp;
  }

  public void Clear()
  {
    _touchedEntities.Clear();
    _touchedTiles.Clear();
    ResolvedAtTick = null;
    BlockingNormal = Vector2.Zero;
    BlockedAxes = CollisionAxisMask.None;
    DidStepUp = false;
  }

  private static void EnsureFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Vector components must be finite.");
    }
  }

  private static void EnsureKnownAxes(CollisionAxisMask value)
  {
    const CollisionAxisMask knownAxes =
      CollisionAxisMask.Horizontal | CollisionAxisMask.Vertical;
    if ((value & ~knownAxes) != CollisionAxisMask.None)
    {
      throw new ArgumentOutOfRangeException(
        nameof(value),
        value,
        "Collision axis mask contains unknown flags.");
    }
  }
}
