using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Network;

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
    _outbound.BeginTickBatch(context.Tick);
    try
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
    finally
    {
      // 批闭合必须无条件执行：否则连接会一直压住普通流量。
      _outbound.SealTickBatch(context.Tick);
    }
  }
}
