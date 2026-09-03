using System;
using System.Collections;
using System.Collections.Generic;
using Arch.Core;

namespace Terraria.Dome.Simulation.Npc;

public sealed class NpcStore : IReadOnlyDictionary<NpcHandle, Entity>
{
  private readonly Dictionary<NpcHandle, Entity> _entities = new();

  public int Count => _entities.Count;

  public IEnumerable<NpcHandle> Keys => _entities.Keys;

  public IEnumerable<Entity> Values => _entities.Values;

  public Entity this[NpcHandle npc] => _entities[npc];

  public void Add(NpcHandle npc, Entity entity)
  {
    if (!npc.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(npc));
    }

    if (!_entities.TryAdd(npc, entity))
    {
      throw new ArgumentException("The NPC handle is already owned.", nameof(npc));
    }
  }

  public bool ContainsKey(NpcHandle npc)
  {
    return _entities.ContainsKey(npc);
  }

  public bool Remove(NpcHandle npc, out Entity entity)
  {
    return _entities.Remove(npc, out entity);
  }

  public bool TryGetValue(NpcHandle npc, out Entity entity)
  {
    return _entities.TryGetValue(npc, out entity);
  }

  public IEnumerator<KeyValuePair<NpcHandle, Entity>> GetEnumerator()
  {
    return _entities.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}
