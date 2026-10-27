using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Communication;

/// <summary>Domain handler for one already-framed inbound communication message.</summary>
public interface ICommunicationPacketHandler
{
  IReadOnlyList<OutboundPacketIntent> Handle(
      InboundCommunicationPacket packet,
      CommunicationSessionComponent session,
      long tick);
}

/// <summary>Consumes inbound session queues at the Preparation phase.</summary>
public sealed class CommunicationSessionTickSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly ICommunicationPacketHandler _handler;
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      CommunicationSessionComponent,
      CommunicationInboundMessageBufferComponent,
      CommunicationOutboundMessageBufferComponent>();

  public CommunicationSessionTickSystem(
      ArchWorld world,
      ICommunicationPacketHandler handler)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _handler = handler ?? throw new ArgumentNullException(nameof(handler));
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    World.Query(
        in _query,
        (ref CommunicationSessionComponent session,
            ref CommunicationInboundMessageBufferComponent inbound,
            ref CommunicationOutboundMessageBufferComponent outbound) =>
        {
          while (inbound.TryDequeue(out InboundCommunicationPacket? packet))
          {
            if (packet is null || packet.Envelope.Epoch != session.Epoch)
            {
              continue;
            }

            IReadOnlyList<OutboundPacketIntent> intents = _handler.Handle(
                packet,
                session,
                tick);
            ArgumentNullException.ThrowIfNull(intents);
            foreach (OutboundPacketIntent intent in intents)
            {
              outbound.Enqueue(intent);
            }
          }
        });
  }
}

/// <summary>Moves ECS outbound intents into the host-owned delivery queue.</summary>
public sealed class CommunicationOutboundProjectionSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly ICommunicationOutboundPort _outbound;
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      CommunicationSessionComponent,
      CommunicationOutboundMessageBufferComponent>();

  public CommunicationOutboundProjectionSystem(
      ArchWorld world,
      ICommunicationOutboundPort outbound)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _outbound = outbound ?? throw new ArgumentNullException(nameof(outbound));
  }

  public override void Update(in EcsServerTickContext context)
  {
    World.Query(
        in _query,
        (ref CommunicationSessionComponent _,
            ref CommunicationOutboundMessageBufferComponent outbound) =>
        {
          int pending = outbound.Count;
          List<OutboundPacketIntent> rejected = [];
          for (int index = 0; index < pending; index++)
          {
            if (!outbound.TryDequeue(out OutboundPacketIntent? intent) || intent is null)
            {
              break;
            }

            if (!_outbound.TrySubmit(intent))
            {
              rejected.Add(intent);
            }
          }

          foreach (OutboundPacketIntent intent in rejected)
          {
            outbound.Enqueue(intent);
          }
        });
  }
}
