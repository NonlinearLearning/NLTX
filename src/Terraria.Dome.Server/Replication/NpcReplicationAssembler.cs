using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Npc;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Npc.Snapshots;

namespace Terraria.Dome.Server.Replication;

public sealed class NpcReplicationAssembler
{
  private readonly NpcStateProjector _projector = new();

  public IReadOnlyList<NpcSyncPacket> CollectPackets(
    SessionReplicationState session,
    IReadOnlyList<NpcStateSnapshot> states)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(states);
    List<NpcSyncPacket> packets = new();
    for (int index = 0; index < states.Count; index++)
    {
      NpcStateSnapshot state = states[index];
      NpcReplicationSnapshot replication = state.ToReplicationSnapshot();
      bool isVisible = session.VisibleSections.Contains(replication.Section);
      if ((replication.IsActive && !isVisible) ||
          (!replication.IsActive && !session.WasCombatNpcSent(replication.ReplicationId)))
      {
        continue;
      }

      NpcProjectionResult projection = _projector.Project(state);
      if (!projection.IsSupported)
      {
        continue;
      }

      if (!session.ShouldSendCombatNpc(replication))
      {
        continue;
      }

      packets.Add(projection.Packet);
    }

    return packets;
  }

  public IReadOnlyList<byte[]> CollectFrames(
    SessionReplicationState session,
    IReadOnlyList<NpcStateSnapshot> states)
  {
    IReadOnlyList<NpcSyncPacket> packets = CollectPackets(session, states);
    List<byte[]> frames = new(packets.Count);
    for (int index = 0; index < packets.Count; index++)
    {
      frames.Add(NpcSyncPacketCodec.Encode(packets[index]));
    }

    return frames;
  }
}
