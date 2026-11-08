using System.Collections.Generic;

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

  public void Clear() => _pending.Clear();
}
