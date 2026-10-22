using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Identity;

/// <summary>Allocates process-unique identities and rejects retired or stale handles.</summary>
public sealed class EntityIdentityRegistry
{
  private readonly HashSet<EntityUuid> _observed = [];
  private readonly Dictionary<EntityUuid, EntityHandle> _active = [];
  private long _worldScope = 1;
  private long _nextGeneration = 1;

  public long CurrentWorldScope => _worldScope;

  public EntityHandle Create()
  {
    if (_nextGeneration <= 0)
    {
      throw new InvalidOperationException("The entity generation range is exhausted.");
    }

    EntityUuid id;
    do
    {
      id = EntityUuid.Create();
    }
    while (!_observed.Add(id));

    var handle = new EntityHandle(
      id,
      new WorldScopeId(_worldScope),
      new EntityGeneration(_nextGeneration));
    _nextGeneration = _nextGeneration == long.MaxValue ? 0 : _nextGeneration + 1;
    _active.Add(id, handle);
    return handle;
  }

  public bool IsActive(EntityHandle handle)
  {
    return handle.Id.IsValid &&
      handle.WorldScope.Value == _worldScope &&
      _active.TryGetValue(handle.Id, out EntityHandle current) &&
      current == handle;
  }

  public bool Retire(EntityHandle handle)
  {
    return IsActive(handle) && _active.Remove(handle.Id);
  }

  public void BeginWorldScope()
  {
    if (_worldScope == long.MaxValue)
    {
      throw new InvalidOperationException("The world scope identity range is exhausted.");
    }

    _active.Clear();
    _worldScope++;
    _nextGeneration = 1;
  }
}
