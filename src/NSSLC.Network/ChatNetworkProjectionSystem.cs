using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Chat;
using Terraria.Entity;
using Terraria.Relationships;
using Terraria.Session;

namespace Terraria.Network;

/// <summary>
/// Projects chat facts into the reference's text net-module packet, targeting the sessions the fact
/// addresses.
/// </summary>
/// <remarks>
/// The reference broadcasts with <c>ChatHelper.BroadcastChatMessageAs</c>, which sends only to
/// players whose session reached the logged-in state (<c>Netplay.Clients[i].State == 10</c>); the
/// projection mirrors that by targeting sessions in <see cref="CommunicationSessionStage.Active"/>.
/// Private replies resolve the speaker's handle back to its session through
/// <see cref="SessionPlayerRoster"/>, so the Chat domain never learns about sessions.
/// </remarks>
public sealed class ChatNetworkProjectionSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeResolver _resolver;
  private readonly QueryDescription _sessionQuery = new QueryDescription().WithAll<
      CommunicationSessionComponent,
      CommunicationOutboundMessageBufferComponent>();
  private ChatBroadcastQueueComponent? _broadcasts;

  public ChatNetworkProjectionSystem(ArchWorld ecsWorld, IEntityRuntimeResolver resolver)
      : base(ecsWorld ?? throw new System.ArgumentNullException(nameof(ecsWorld)))
  {
    _resolver = resolver ?? throw new System.ArgumentNullException(nameof(resolver));
  }

  // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
  private ChatBroadcastQueueComponent Broadcasts
    => _broadcasts ??= ChatBroadcastQueue.Require(World);

  public override void Update(in EcsServerTickContext context)
  {
    IReadOnlyList<ChatBroadcast> broadcasts = Broadcasts.Drain();
    if (broadcasts.Count == 0)
    {
      return;
    }

    List<SessionEpoch> activeSessions = [];
    World.Query(
        in _sessionQuery,
        (ref CommunicationSessionComponent session,
            ref CommunicationOutboundMessageBufferComponent _) =>
        {
          if (session.Stage == CommunicationSessionStage.Active)
          {
            activeSessions.Add(session.Epoch);
          }
        });

    if (activeSessions.Count == 0)
    {
      return;
    }

    SessionPlayer[] players = SessionPlayerRoster.Snapshot(World, _resolver);
    for (int index = 0; index < broadcasts.Count; index++)
    {
      ChatBroadcast broadcast = broadcasts[index];
      SessionEpoch[] targets = ResolveTargets(broadcast, players, activeSessions);
      if (targets.Length == 0)
      {
        continue;
      }

      byte[] payload = ChatNetTextFrame.EncodeServerMessage(
          broadcast.AuthorId,
          broadcast.DisplayText,
          broadcast.Color);
      World.Query(
          in _sessionQuery,
          (ref CommunicationSessionComponent session,
              ref CommunicationOutboundMessageBufferComponent outbound) =>
          {
            if (!Contains(targets, session.Epoch))
            {
              return;
            }

            outbound.Enqueue(new OutboundPacketIntent(
                [session.Epoch],
                null,
                ChatNetTextFrame.NetModulesMessageId,
                payload,
                CommitVersion.Initial));
          });
    }
  }

  private static SessionEpoch[] ResolveTargets(
      in ChatBroadcast broadcast,
      SessionPlayer[] players,
      List<SessionEpoch> activeSessions)
  {
    if (broadcast.OnlyFor is not EntityRuntimeHandle onlyFor)
    {
      return [.. activeSessions];
    }

    for (int index = 0; index < players.Length; index++)
    {
      if (players[index].Handle == onlyFor)
      {
        return [players[index].Epoch];
      }
    }

    return [];
  }

  private static bool Contains(SessionEpoch[] epochs, SessionEpoch candidate)
  {
    for (int index = 0; index < epochs.Length; index++)
    {
      if (epochs[index] == candidate)
      {
        return true;
      }
    }

    return false;
  }
}
