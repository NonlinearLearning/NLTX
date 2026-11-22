using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Relationships;

namespace Terraria.Communication;

/// <summary>Consumes transport ingress and records session structure changes for Commit.</summary>
public sealed class CommunicationIngressPreparationSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private sealed class PendingSession
  {
    public required ArchEntity Entity { get; init; }
    public required CommunicationSessionComponent Session { get; init; }
    public required CommunicationInboundMessageBufferComponent Inbound { get; init; }
    public required CommunicationOutboundMessageBufferComponent Outbound { get; init; }
    public EntityRuntimeHandle? Player { get; set; }
    public bool Closing { get; set; }
  }

  private readonly CommunicationSessionIngress _ingress;
  private readonly ICommunicationSessionPlayerLifecycle? _playerLifecycle;
  private readonly Dictionary<SessionEpoch, ArchEntity> _sessions = [];
  private readonly Dictionary<SessionEpoch, PendingSession> _pendingSessions = [];
  private readonly HashSet<SessionEpoch> _closedEpochs = [];
  private readonly List<SessionEpoch> _pendingClose = [];

  public CommunicationIngressPreparationSystem(
      ArchWorld world,
      CommunicationSessionIngress ingress,
      ICommunicationSessionPlayerLifecycle? playerLifecycle = null)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _ingress = ingress ?? throw new ArgumentNullException(nameof(ingress));
    _playerLifecycle = playerLifecycle;
  }

  public int ActiveSessionCount => _sessions.Count;
  public int PendingSessionCount => _pendingSessions.Count;
  public int PendingCloseCount => _pendingClose.Count;
  public long DiscardedEventCount { get; private set; }

  public override void Update(in EcsServerTickContext context)
  {
    EcsServerCommandBuffer commands = context.CommandBuffer;
    while (_ingress.TryDequeue(out CommunicationIngressEvent? ingressEvent))
    {
      if (ingressEvent is null)
      {
        continue;
      }

      switch (ingressEvent.Kind)
      {
        case CommunicationIngressEventKind.Open:
          HandleOpen(ingressEvent.Epoch, commands);
          break;
        case CommunicationIngressEventKind.Packet:
          HandlePacket(ingressEvent);
          break;
        case CommunicationIngressEventKind.Close:
          HandleClose(ingressEvent.Epoch);
          break;
        default:
          DiscardedEventCount++;
          break;
      }
    }
  }

  public void CommitPendingClosures(EcsServerCommandBuffer commands)
  {
    ArgumentNullException.ThrowIfNull(commands);
    foreach (SessionEpoch epoch in _pendingClose.ToArray())
    {
      if (_pendingSessions.Remove(epoch, out PendingSession? pending))
      {
        DestroyPendingSession(epoch, pending, commands);
        _closedEpochs.Add(epoch);
        continue;
      }

      if (!_sessions.Remove(epoch, out ArchEntity entity))
      {
        continue;
      }

      if (World.IsAlive(entity) &&
          World.TryGet(entity, out CommunicationSessionComponent? session) &&
          session?.Player is EntityRuntimeHandle player)
      {
        try
        {
          _playerLifecycle?.DestroyPlayer(epoch, player, commands);
        }
        finally
        {
          session.UnbindPlayer(player);
        }
      }

      if (World.IsAlive(entity))
      {
        commands.Destroy(entity);
      }
      _closedEpochs.Add(epoch);
    }

    _pendingClose.Clear();
  }

  public void CloseAllSessions(EcsServerCommandBuffer commands)
  {
    ArgumentNullException.ThrowIfNull(commands);
    foreach (SessionEpoch epoch in _pendingSessions.Keys.ToArray())
    {
      HandleClose(epoch);
    }

    foreach (SessionEpoch epoch in _sessions.Keys.ToArray())
    {
      HandleClose(epoch);
    }

    CommitPendingClosures(commands);
  }

  private void HandleOpen(SessionEpoch epoch, EcsServerCommandBuffer commands)
  {
    if (!epoch.IsValid || _sessions.ContainsKey(epoch) ||
        _pendingSessions.ContainsKey(epoch) || _closedEpochs.Contains(epoch))
    {
      DiscardedEventCount++;
      return;
    }

    CommunicationSessionComponent session = new(epoch);
    CommunicationInboundMessageBufferComponent inbound = new();
    CommunicationOutboundMessageBufferComponent outbound = new();
    ArchEntity entity = commands.Create(
        (ComponentType)typeof(CommunicationSessionComponent),
        (ComponentType)typeof(CommunicationInboundMessageBufferComponent),
        (ComponentType)typeof(CommunicationOutboundMessageBufferComponent));
    commands.Set(entity, session);
    commands.Set(entity, inbound);
    commands.Set(entity, outbound);

    PendingSession pending = new()
    {
      Entity = entity,
      Session = session,
      Inbound = inbound,
      Outbound = outbound
    };
    _pendingSessions.Add(epoch, pending);

    try
    {
      if (_playerLifecycle is not null)
      {
        pending.Player = _playerLifecycle.CreatePlayer(epoch, commands);
        if (!session.BindPlayer(pending.Player.Value))
        {
          throw new InvalidOperationException("The deferred player could not be bound to its session.");
        }
        commands.Set(entity, session);
      }

      commands.RegisterAfterPlayback(_ =>
      {
        if (pending.Closing || !_pendingSessions.Remove(epoch))
        {
          return;
        }

        _sessions[epoch] = commands.Resolve(pending.Entity);
      });
      commands.RegisterOnDiscard(() =>
      {
        _pendingSessions.Remove(epoch);
        _closedEpochs.Add(epoch);
      });
    }
    catch
    {
      _pendingSessions.Remove(epoch);
      if (!commands.WasDestroyed(entity))
      {
        commands.Destroy(entity);
      }
      throw;
    }
  }

  private void HandlePacket(CommunicationIngressEvent ingressEvent)
  {
    CommunicationSessionComponent? session = null;
    CommunicationInboundMessageBufferComponent? buffer = null;
    if (_pendingSessions.TryGetValue(ingressEvent.Epoch, out PendingSession? pending))
    {
      session = pending.Session;
      buffer = pending.Inbound;
    }
    else if (_sessions.TryGetValue(ingressEvent.Epoch, out ArchEntity entity) &&
             World.IsAlive(entity) &&
             World.TryGet(entity, out session) &&
             World.TryGet(entity, out buffer))
    {
    }

    if (session is null || buffer is null || !session.AcceptInbound(ingressEvent.PacketSequence))
    {
      DiscardedEventCount++;
      return;
    }

    ReadOnlyMemory<byte> frameBytes = ingressEvent.FrameBytes;
    if (ingressEvent.PayloadOffset > frameBytes.Length)
    {
      DiscardedEventCount++;
      return;
    }

    var envelope = new InboundPacketEnvelope(
        ingressEvent.Epoch,
        ingressEvent.PacketSequence,
        ingressEvent.ReceivedAt,
        ingressEvent.MessageId,
        ingressEvent.ModuleId,
        frameBytes,
        session.Stage.ToString(),
        null,
        null,
        null);
    buffer.Enqueue(new InboundCommunicationPacket(
        envelope,
        new InboundPacketFrame(
            ingressEvent.MessageId,
            ingressEvent.ModuleId,
            frameBytes,
            frameBytes[ingressEvent.PayloadOffset..])));
  }

  private void HandleClose(SessionEpoch epoch)
  {
    if (_pendingSessions.TryGetValue(epoch, out PendingSession? pending))
    {
      if (!pending.Closing)
      {
        pending.Closing = true;
        pending.Session.TryAdvance(CommunicationSessionStage.Closing);
        pending.Session.TryAdvance(CommunicationSessionStage.Closed);
        _pendingClose.Add(epoch);
      }
      return;
    }

    if (!_sessions.TryGetValue(epoch, out ArchEntity entity) ||
        !World.IsAlive(entity) ||
        !World.TryGet(entity, out CommunicationSessionComponent? session) ||
        session is null)
    {
      _closedEpochs.Add(epoch);
      DiscardedEventCount++;
      return;
    }

    if (_pendingClose.Contains(epoch))
    {
      return;
    }

    session.TryAdvance(CommunicationSessionStage.Closing);
    session.TryAdvance(CommunicationSessionStage.Closed);
    _pendingClose.Add(epoch);
  }

  private void DestroyPendingSession(
      SessionEpoch epoch,
      PendingSession pending,
      EcsServerCommandBuffer commands)
  {
    if (pending.Player is EntityRuntimeHandle player)
    {
      _playerLifecycle?.DestroyPlayer(epoch, player, commands);
      pending.Session.UnbindPlayer(player);
    }

    if (!commands.WasDestroyed(pending.Entity))
    {
      commands.Destroy(pending.Entity);
    }
  }
}
