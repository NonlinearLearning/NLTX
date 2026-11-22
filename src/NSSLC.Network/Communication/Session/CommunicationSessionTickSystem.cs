using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Network;

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
