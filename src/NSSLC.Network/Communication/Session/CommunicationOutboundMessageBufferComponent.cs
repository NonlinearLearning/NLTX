using System.Collections.Concurrent;

namespace Terraria.Network;

/// <summary>Outbound packet intents waiting for the projection phase to submit them.</summary>
public sealed class CommunicationOutboundMessageBufferComponent
{
  private readonly ConcurrentQueue<OutboundPacketIntent> _realtimeIntents = new();
  private readonly ConcurrentQueue<OutboundPacketIntent> _intents = new();

  public int Count => _realtimeIntents.Count + _intents.Count;

  public void Enqueue(OutboundPacketIntent intent)
  {
    ArgumentNullException.ThrowIfNull(intent);
    if (intent.Priority == OutboundPacketPriority.Realtime)
    {
      _realtimeIntents.Enqueue(intent);
      return;
    }

    _intents.Enqueue(intent);
  }

  public bool TryDequeue(out OutboundPacketIntent? intent)
  {
    return _realtimeIntents.TryDequeue(out intent) || _intents.TryDequeue(out intent);
  }

  /// <summary>
  /// Peeks the next intent the delivery port should receive, realtime first. The projection uses
  /// peek-then-submit so a rejected intent keeps its position instead of being re-queued behind
  /// newer facts.
  /// </summary>
  public bool TryPeek(out OutboundPacketIntent? intent)
  {
    return _realtimeIntents.TryPeek(out intent) || _intents.TryPeek(out intent);
  }
}
