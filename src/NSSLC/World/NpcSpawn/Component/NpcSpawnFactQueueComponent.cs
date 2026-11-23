using System.Collections.Generic;
using ArchEntity = Arch.Core.Entity;

namespace Terraria.Npc;

/// <summary>Tick-scoped queue of creatures that were just created and need a network identity.</summary>
public sealed class NpcSpawnFactQueueComponent
{
  private readonly List<NpcSpawnedFact> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(in NpcSpawnedFact fact) => _pending.Add(fact);

  public IReadOnlyList<NpcSpawnedFact> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    NpcSpawnedFact[] facts = _pending.ToArray();
    _pending.Clear();
    return facts;
  }

  public void Requeue(IReadOnlyList<NpcSpawnedFact> facts)
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
  public void Replace(in NpcSpawnedFact fact, ArchEntity entity)
  {
    for (int index = 0; index < _pending.Count; index++)
    {
      if (_pending[index] == fact)
      {
        _pending[index] = new NpcSpawnedFact(entity, fact.NetworkType);
      }
    }
  }

  public void Clear() => _pending.Clear();
}
