using System.Collections.Generic;

namespace Terraria.Share.Player;

/// <summary>Tick-scoped queue of item use intentions.</summary>
public sealed class PlayerItemUseRequestQueueComponent
{
  private readonly List<PlayerItemUseRequest> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(in PlayerItemUseRequest request) => _pending.Add(request);

  public IReadOnlyList<PlayerItemUseRequest> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    PlayerItemUseRequest[] requests = _pending.ToArray();
    _pending.Clear();
    return requests;
  }

  public void Clear() => _pending.Clear();
}
