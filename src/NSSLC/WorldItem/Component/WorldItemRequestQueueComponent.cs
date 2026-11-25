using System.Collections.Generic;

namespace Terraria.Unshared.World;

/// <summary>Tick-scoped queue of world-item changes requested by other domains.</summary>
/// <remarks>
/// The queue lives on a dedicated world-scoped entity so the requesting adapters and the World
/// domain owner share one instance without the host handing a mutable store to either of them.
/// </remarks>
public sealed class WorldItemRequestQueueComponent
{
  private readonly List<WorldItemRequest> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(in WorldItemRequest request) => _pending.Add(request);

  public IReadOnlyList<WorldItemRequest> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    WorldItemRequest[] requests = _pending.ToArray();
    _pending.Clear();
    return requests;
  }

  public void Clear() => _pending.Clear();
}
