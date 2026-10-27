using System.Collections.Concurrent;

namespace Terraria.Communication;

/// <summary>Outbound packet intents waiting for the projection phase to submit them.</summary>
public sealed class CommunicationOutboundMessageBufferComponent
{
  private readonly ConcurrentQueue<OutboundPacketIntent> _intents = new();

  public int Count => _intents.Count;

  public void Enqueue(OutboundPacketIntent intent)
  {
    ArgumentNullException.ThrowIfNull(intent);
    _intents.Enqueue(intent);
  }

  public bool TryDequeue(out OutboundPacketIntent? intent)
  {
    return _intents.TryDequeue(out intent);
  }
}
