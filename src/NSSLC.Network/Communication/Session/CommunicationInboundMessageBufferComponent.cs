using System.Collections.Concurrent;

namespace Terraria.Network;

/// <summary>Transient inbound packet queue owned by one ECS communication session.</summary>
public sealed class CommunicationInboundMessageBufferComponent
{
  private readonly ConcurrentQueue<InboundCommunicationPacket> _messages = new();

  public int Count => _messages.Count;

  public void Enqueue(InboundCommunicationPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    _messages.Enqueue(packet);
  }

  public bool TryDequeue(out InboundCommunicationPacket? packet)
  {
    return _messages.TryDequeue(out packet);
  }
}
