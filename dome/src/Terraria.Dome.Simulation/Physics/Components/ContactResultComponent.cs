using System;
using System.Collections.Generic;
using System.Numerics;
using Arch.Core;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Physics.Components;

public sealed class ContactResultComponent
{
  private readonly List<WorldTileCoordinate> _touchedTileCoordinates = new();
  private readonly List<Entity> _touchedEntities = new();

  public long? ResolvedAtTick { get; private set; }
  public Vector2 BlockingNormal { get; private set; }
  public CollisionAxisMask BlockedAxes { get; private set; }
  public bool DidStepUp { get; private set; }
  public IReadOnlyList<WorldTileCoordinate> TouchedTileCoordinates =>
    _touchedTileCoordinates.AsReadOnly();
  public IReadOnlyList<Entity> TouchedEntities => _touchedEntities.AsReadOnly();
  public bool HasTileContact => _touchedTileCoordinates.Count != 0;
  public bool HasEntityContact => _touchedEntities.Count != 0;

  public void Replace(
    long tick,
    CollisionAxisMask blockedAxes,
    bool didStepUp,
    IReadOnlyList<WorldTileCoordinate> touchedTiles,
    Vector2 blockingNormal = default,
    IReadOnlyList<Entity>? touchedEntities = null)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    ArgumentNullException.ThrowIfNull(touchedTiles);
    const CollisionAxisMask knownAxes =
      CollisionAxisMask.Horizontal | CollisionAxisMask.Vertical;
    if ((blockedAxes & ~knownAxes) != CollisionAxisMask.None)
    {
      throw new ArgumentOutOfRangeException(nameof(blockedAxes));
    }

    _touchedTileCoordinates.Clear();
    for (int index = 0; index < touchedTiles.Count; index++)
    {
      _touchedTileCoordinates.Add(touchedTiles[index]);
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
    _touchedTileCoordinates.Clear();
    _touchedEntities.Clear();
    ResolvedAtTick = null;
    BlockingNormal = Vector2.Zero;
    BlockedAxes = CollisionAxisMask.None;
    DidStepUp = false;
  }
}
