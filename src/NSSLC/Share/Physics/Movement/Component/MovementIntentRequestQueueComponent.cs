using System.Collections.Generic;

namespace Terraria.Share.Movement;

/// <summary>Tick-scoped queue of movement intents requested by other domains.</summary>
/// <remarks>
/// The queue lives on a dedicated world-scoped entity so the input adapter and the NPC AI adapter
/// share one channel without either of them holding <see cref="MovementIntent"/> for writing.
/// </remarks>
public sealed class MovementIntentRequestQueueComponent
{
  private readonly List<MovementIntentRequest> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(in MovementIntentRequest request) => _pending.Add(request);

  public IReadOnlyList<MovementIntentRequest> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    MovementIntentRequest[] pending = _pending.ToArray();
    _pending.Clear();
    return pending;
  }

  public void Clear() => _pending.Clear();
}
