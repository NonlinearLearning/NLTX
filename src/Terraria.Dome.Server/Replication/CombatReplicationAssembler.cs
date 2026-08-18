using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Server.Replication;

public sealed class CombatReplicationAssembler
{
  public IReadOnlyList<byte[]> CollectFrames(
    SessionReplicationState session,
    IReadOnlyList<NpcReplicationSnapshot> npcs,
    IReadOnlyList<ProjectileReplicationSnapshot> projectiles)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(projectiles);
    List<byte[]> frames = new();
    if (session.TryTakeDefaultNpcReconciliation() && npcs.Count > 0)
    {
      NpcReplicationSnapshot firstNpc = npcs[0];
      if (firstNpc.IsActive && session.VisibleSections.Contains(firstNpc.Section))
      {
        frames.Add(TerrariaPacketCodec.EncodeNpcReplication(firstNpc));
        if (npcs.Count == 1)
        {
          session.MarkDefaultCombatNpcSent(firstNpc);
        }
      }
    }

    for (int index = 0; index < npcs.Count; index++)
    {
      NpcReplicationSnapshot npc = npcs[index];
      bool isVisible = session.VisibleSections.Contains(npc.Section);
      if ((npc.IsActive && !isVisible) ||
          (!npc.IsActive && !session.WasCombatNpcSent(npc.ReplicationId)) ||
          !session.ShouldSendCombatNpc(npc))
      {
        continue;
      }

      frames.Add(TerrariaPacketCodec.EncodeNpcReplication(npc));
    }

    for (int index = 0; index < projectiles.Count; index++)
    {
      ProjectileReplicationSnapshot projectile = projectiles[index];
      bool isVisible = session.VisibleSections.Contains(projectile.Section);
      if (!projectile.IsActive && !session.WasCombatProjectileSent(projectile.ReplicationId))
      {
        continue;
      }

      if (projectile.IsActive && !isVisible)
      {
        continue;
      }

      if (!session.ShouldSendCombatProjectile(projectile))
      {
        continue;
      }

      frames.Add(projectile.IsActive
        ? TerrariaPacketCodec.EncodeProjectileSync(ProjectileStateProjection.Project(projectile))
        : TerrariaPacketCodec.EncodeProjectileDespawn(projectile));
    }

    return frames;
  }
}
