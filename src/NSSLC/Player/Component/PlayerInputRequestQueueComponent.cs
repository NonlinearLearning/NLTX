using System.Collections.Generic;

namespace Terraria.Player;

/// <summary>Tick-scoped queue of player state changes requested by other domains.</summary>
/// <remarks>
/// The queue lives on a dedicated world-scoped entity so the packet adapter and the Player domain
/// owner share one instance without the host handing player components to either of them.
/// </remarks>
public sealed class PlayerInputRequestQueueComponent
{
  private readonly List<PlayerInputRequest> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(in PlayerInputRequest request) => _pending.Add(request);

  public IReadOnlyList<PlayerInputRequest> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    PlayerInputRequest[] pending = _pending.ToArray();
    _pending.Clear();
    return pending;
  }

  public void Clear() => _pending.Clear();
}
