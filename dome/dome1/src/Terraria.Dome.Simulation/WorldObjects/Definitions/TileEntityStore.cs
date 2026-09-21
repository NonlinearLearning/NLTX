using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public sealed class TileEntityStore
{
  private readonly Dictionary<int, TileEntityIdentityComponent> _byId = new();
  private readonly Dictionary<(int TileX, int TileY), int> _byPosition = new();
  private int _nextId = 1;

  public int Count => _byId.Count;

  public bool TryAllocate(byte type, int tileX, int tileY, out TileEntityIdentityComponent identity)
  {
    identity = default;
    if (!TileEntityDefinitionRegistry.TryGet(type, out _) ||
        _byPosition.ContainsKey((tileX, tileY)) ||
        _nextId <= 0)
    {
      return false;
    }

    identity = new TileEntityIdentityComponent(_nextId++, type).Validate();
    _byId.Add(identity.EntityId, identity);
    _byPosition.Add((tileX, tileY), identity.EntityId);
    return true;
  }

  public bool TryRestore(
    TileEntityIdentityComponent identity,
    TileEntityAnchorComponent anchor,
    bool isOpaque = false)
  {
    identity.Validate();
    if (_byId.ContainsKey(identity.EntityId) ||
        _byPosition.ContainsKey((anchor.TileX, anchor.TileY)) ||
        !isOpaque && !TileEntityDefinitionRegistry.TryGet(identity.Type, out _))
    {
      return false;
    }

    _byId.Add(identity.EntityId, identity);
    _byPosition.Add((anchor.TileX, anchor.TileY), identity.EntityId);
    if (identity.EntityId >= _nextId)
    {
      _nextId = identity.EntityId == int.MaxValue ? -1 : identity.EntityId + 1;
    }

    return true;
  }

  public bool TryGet(int entityId, out TileEntityIdentityComponent identity)
  {
    return _byId.TryGetValue(entityId, out identity);
  }

  public bool TryFindAt(int tileX, int tileY, out TileEntityIdentityComponent identity)
  {
    if (_byPosition.TryGetValue((tileX, tileY), out int entityId))
    {
      return _byId.TryGetValue(entityId, out identity);
    }

    identity = default;
    return false;
  }

  public bool Remove(int entityId, TileEntityAnchorComponent anchor)
  {
    if (!_byId.TryGetValue(entityId, out _))
    {
      return false;
    }

    if (!_byPosition.TryGetValue(
          (anchor.TileX, anchor.TileY),
          out int indexedId) ||
        indexedId != entityId)
    {
      return false;
    }

    _byId.Remove(entityId);
    _byPosition.Remove((anchor.TileX, anchor.TileY));
    return true;
  }
}
