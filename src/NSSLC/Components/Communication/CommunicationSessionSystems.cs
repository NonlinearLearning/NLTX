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
/// <remarks>
/// The drain is bounded per session per tick. A world (tile) flood must not turn the projection
/// into an unbounded loop: that work would lengthen the tick, and a longer tick is what delays the
/// next authoritative NPC position snapshot. Intents the delivery port cannot accept stay at the
/// head of their priority queue and are retried on the next tick, so nothing is dropped or
/// reordered.
/// </remarks>
public sealed class CommunicationOutboundProjectionSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  /// <summary>
  /// Upper bound on intents submitted per session per tick. Ordinary gameplay submits a handful;
  /// the bound only engages when a backlog has built up.
  /// </summary>
  public const int DefaultMaximumIntentsPerTick = 256;

  private readonly ICommunicationOutboundPort _outbound;
  private readonly int _maximumIntentsPerTick;
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      CommunicationSessionComponent,
      CommunicationOutboundMessageBufferComponent>();

  public CommunicationOutboundProjectionSystem(
      ArchWorld world,
      ICommunicationOutboundPort outbound,
      int maximumIntentsPerTick = DefaultMaximumIntentsPerTick)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _outbound = outbound ?? throw new ArgumentNullException(nameof(outbound));
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumIntentsPerTick);
    _maximumIntentsPerTick = maximumIntentsPerTick;
  }

  public override void Update(in EcsServerTickContext context)
  {
    World.Query(
        in _query,
        (ref CommunicationSessionComponent session,
            ref CommunicationOutboundMessageBufferComponent outbound) =>
        {
          _ = session;
          for (int submitted = 0; submitted < _maximumIntentsPerTick; submitted++)
          {
            if (!outbound.TryPeek(out OutboundPacketIntent? intent) || intent is null)
            {
              return;
            }

            if (!_outbound.TrySubmit(intent))
            {
              return;
            }

            outbound.TryDequeue(out OutboundPacketIntent? _);
          }
        });
  }
}
