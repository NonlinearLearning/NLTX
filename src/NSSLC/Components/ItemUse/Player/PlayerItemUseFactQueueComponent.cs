using System.Collections.Generic;

namespace Terraria.Player;

/// <summary>Tick-scoped queue of applied item uses.</summary>
public sealed class PlayerItemUseFactQueueComponent
{
  private readonly List<PlayerItemUseFact> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(in PlayerItemUseFact fact) => _pending.Add(fact);

  public IReadOnlyList<PlayerItemUseFact> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    PlayerItemUseFact[] facts = _pending.ToArray();
    _pending.Clear();
    return facts;
  }

  public void Clear() => _pending.Clear();
}
