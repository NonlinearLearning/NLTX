using Terraria.Relationships;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Communication;

/// <summary>Owns the epoch-to-ECS-session mapping at the host/ECS seam.</summary>
public sealed class CommunicationSessionIngress
{
  private readonly ArchWorld _world;
  private readonly Dictionary<SessionEpoch, ArchEntity> _sessions = [];

  public CommunicationSessionIngress(ArchWorld world)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public int Count => _sessions.Count;

  public bool TryOpen(SessionEpoch epoch, out ArchEntity entity)
  {
    entity = ArchEntity.Null;
    if (!epoch.IsValid || _sessions.ContainsKey(epoch))
    {
      return false;
    }

    entity = _world.Create(
        new CommunicationSessionComponent(epoch),
        new CommunicationInboundMessageBufferComponent(),
        new CommunicationOutboundMessageBufferComponent());
    _sessions.Add(epoch, entity);
    return true;
  }

  public bool TryEnqueue(InboundCommunicationPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (!_sessions.TryGetValue(packet.Envelope.Epoch, out ArchEntity entity) ||
        !_world.IsAlive(entity) ||
        !_world.TryGet(entity, out CommunicationSessionComponent? session) ||
        !_world.TryGet(entity, out CommunicationInboundMessageBufferComponent? buffer))
    {
      return false;
    }

    if (session is null || buffer is null ||
        session.Stage is CommunicationSessionStage.Closing or CommunicationSessionStage.Closed ||
        !session.AcceptInbound(packet.Envelope.Sequence))
    {
      return false;
    }

    buffer.Enqueue(packet);
    return true;
  }

  public bool TryActivate(SessionEpoch epoch)
  {
    if (!_sessions.TryGetValue(epoch, out ArchEntity entity) ||
        !_world.IsAlive(entity) ||
        !_world.TryGet(entity, out CommunicationSessionComponent? session) ||
        session is null)
    {
      return false;
    }

    return session.TryAdvance(CommunicationSessionStage.Active);
  }

  public bool TryBindPlayer(SessionEpoch epoch, EntityRuntimeHandle player)
  {
    if (!_sessions.TryGetValue(epoch, out ArchEntity entity) ||
        !_world.IsAlive(entity) ||
        !_world.TryGet(entity, out CommunicationSessionComponent? session) ||
        session is null)
    {
      return false;
    }

    return session.BindPlayer(player);
  }

  public bool TryClose(ConnectionClosed closed)
  {
    ArgumentNullException.ThrowIfNull(closed);
    if (!_sessions.Remove(closed.Epoch, out ArchEntity entity) || !_world.IsAlive(entity))
    {
      return false;
    }

    if (_world.TryGet(entity, out CommunicationSessionComponent? session) && session is not null)
    {
      session.TryAdvance(CommunicationSessionStage.Closing);
      session.TryAdvance(CommunicationSessionStage.Closed);
    }

    _world.Destroy(entity);
    return true;
  }

  public bool TryGetEntity(SessionEpoch epoch, out ArchEntity entity)
  {
    return _sessions.TryGetValue(epoch, out entity) && _world.IsAlive(entity);
  }
}
