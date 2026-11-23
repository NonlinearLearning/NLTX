using System.Collections.Generic;
using ArchEntity = Arch.Core.Entity;

namespace Terraria.EntitySpawn;

/// <summary>Tick-scoped queue of creatures that were just created and need a network identity.</summary>
public sealed class EntitySpawnFactQueueComponent
{
  private readonly List<EntitySpawnedFact> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(in EntitySpawnedFact fact) => _pending.Add(fact);

  public IReadOnlyList<EntitySpawnedFact> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    EntitySpawnedFact[] facts = _pending.ToArray();
    _pending.Clear();
    return facts;
  }

  public void Requeue(IReadOnlyList<EntitySpawnedFact> facts)
  {
    ArgumentNullException.ThrowIfNull(facts);
    for (int index = 0; index < facts.Count; index++)
    {
      _pending.Add(facts[index]);
    }
  }

  /// <summary>Removes facts for an entity whose deferred creation was discarded.</summary>
  public void Remove(ArchEntity entity)
  {
    for (int index = _pending.Count - 1; index >= 0; index--)
    {
      if (_pending[index].Entity == entity)
      {
        _pending.RemoveAt(index);
      }
    }
  }

  /// <summary>Rebinds a deferred fact to its resolved Arch entity after playback.</summary>
  public void Replace(in EntitySpawnedFact fact, ArchEntity entity)
  {
    for (int index = 0; index < _pending.Count; index++)
    {
      if (_pending[index] == fact)
      {
        _pending[index] = new EntitySpawnedFact(entity, fact.NetworkType);
      }
    }
  }

  public void Clear() => _pending.Clear();
}
