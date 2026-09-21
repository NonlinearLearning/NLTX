using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.StatusEffects.Snapshots;

namespace Terraria.Dome.Server.Replication;

public sealed class CombatReplicationAssembler
{
  public IReadOnlyList<byte[]> CollectFrames(
    SessionReplicationState session,
    IReadOnlyList<NpcReplicationSnapshot> npcs,
    IReadOnlyList<ProjectileReplicationSnapshot> projectiles,
    long? currentTick = null)
  {
    CombatReplicationBatch batch = CollectBatch(session, npcs, projectiles, currentTick);
    session.ConfirmCombatBatch(batch);
    return batch.Frames;
  }

  public CombatReplicationBatch CollectBatch(
    SessionReplicationState session,
    IReadOnlyList<NpcReplicationSnapshot> npcs,
    IReadOnlyList<ProjectileReplicationSnapshot> projectiles,
    long? currentTick = null)
  {
    return CollectBatch(
      session,
      npcs,
      projectiles,
      Array.Empty<NpcProjectileReplicationSnapshot>(),
      Array.Empty<NpcStatusEffectStateSnapshot>(),
      currentTick);
  }

  public CombatReplicationBatch CollectBatch(
    SessionReplicationState session,
    IReadOnlyList<NpcReplicationSnapshot> npcs,
    IReadOnlyList<ProjectileReplicationSnapshot> projectiles,
    IReadOnlyList<NpcProjectileReplicationSnapshot> npcProjectiles,
    long? currentTick = null)
  {
    return CollectBatch(
      session,
      npcs,
      projectiles,
      npcProjectiles,
      Array.Empty<NpcStatusEffectStateSnapshot>(),
      currentTick);
  }

  public CombatReplicationBatch CollectBatch(
    SessionReplicationState session,
    IReadOnlyList<NpcReplicationSnapshot> npcs,
    IReadOnlyList<ProjectileReplicationSnapshot> projectiles,
    IReadOnlyList<NpcProjectileReplicationSnapshot> npcProjectiles,
    IReadOnlyList<NpcStatusEffectStateSnapshot> npcStatusEffects,
    long? currentTick = null)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(projectiles);
    ArgumentNullException.ThrowIfNull(npcProjectiles);
    ArgumentNullException.ThrowIfNull(npcStatusEffects);
    List<byte[]> frames = new();
    List<NpcReplicationSnapshot> sentNpcs = new();
    List<ProjectileReplicationSnapshot> sentProjectiles = new();
    if (session.TryTakeDefaultNpcReconciliation() && npcs.Count > 0)
    {
      NpcReplicationSnapshot firstNpc = npcs[0];
      if (firstNpc.IsActive && session.VisibleSections.Contains(firstNpc.Section))
      {
        frames.Add(TerrariaPacketCodec.EncodeNpcReplication(firstNpc));
        sentNpcs.Add(firstNpc);
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
      sentNpcs.Add(npc);
    }

    for (int index = 0; index < projectiles.Count; index++)
    {
      ProjectileReplicationSnapshot projectile = projectiles[index];
      bool isVisible = session.VisibleSections.Contains(projectile.Section);
      if (!projectile.IsActive &&
          currentTick.HasValue &&
          projectile.TombstoneRetainedUntilTick > 0 &&
          currentTick.Value >= projectile.TombstoneRetainedUntilTick)
      {
        continue;
      }

      if (!projectile.IsActive && !session.WasCombatProjectileSent(projectile.ReplicationId))
      {
        continue;
      }

      if (projectile.IsActive && !isVisible &&
          !ProjectileNetworkImportancePolicy.IsImportant(projectile))
      {
        session.MarkCombatProjectileSkipped(projectile);
        continue;
      }

      if (projectile.IsActive && !projectile.NetworkUpdateReady &&
          session.WasCombatProjectileSent(projectile.ReplicationId))
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
      sentProjectiles.Add(projectile);
    }

    List<NpcProjectileReplicationSnapshot> visibleNpcProjectiles =
      CollectNpcProjectiles(session, npcProjectiles, currentTick);
    List<NpcProjectileReplicationSnapshot> sentNpcProjectiles = new();
    if (session.SupportsNpcProjectile)
    {
      for (int index = 0; index < visibleNpcProjectiles.Count; index++)
    {
        NpcProjectileReplicationSnapshot projectile = visibleNpcProjectiles[index];
        if ((!projectile.IsActive && !session.WasNpcProjectileSent(projectile.ReplicationId)) ||
            !session.ShouldSendNpcProjectile(projectile))
        {
          continue;
        }

        if (projectile.IsActive && !projectile.NetworkUpdateReady &&
            session.WasNpcProjectileSent(projectile.ReplicationId))
        {
          continue;
        }

        frames.Add(session.SupportsNpcProjectileV3
          ? ContractExtensionCodec.EncodeNpcProjectileReplicationV3(projectile)
          : session.SupportsNpcProjectileV2
            ? ContractExtensionCodec.EncodeNpcProjectileReplicationV2(projectile)
            : ContractExtensionCodec.EncodeNpcProjectileReplication(projectile));
        sentNpcProjectiles.Add(projectile);
      }
    }

    List<NpcStatusEffectStateSnapshot> sentNpcStatusEffects = new();
    if (session.SupportsNpcStatusEffect)
    {
      Dictionary<int, NpcReplicationSnapshot> visibleNpcs = new();
      for (int index = 0; index < npcs.Count; index++)
      {
        NpcReplicationSnapshot npc = npcs[index];
        if (npc.IsActive && session.VisibleSections.Contains(npc.Section))
        {
          visibleNpcs[npc.ReplicationId] = npc;
        }
      }

      for (int index = 0; index < npcStatusEffects.Count; index++)
      {
        NpcStatusEffectStateSnapshot status = npcStatusEffects[index];
        if (!visibleNpcs.ContainsKey(status.ReplicationId) ||
            !session.ShouldSendNpcStatusEffect(status))
        {
          continue;
        }

        frames.Add(ContractExtensionCodec.EncodeNpcStatusEffect(
          new NpcStatusEffectEnvelope(status.ReplicationId, status.Revision, status.Effects)));
        sentNpcStatusEffects.Add(status);
      }
    }

    return new CombatReplicationBatch(frames, sentNpcs, sentProjectiles)
    {
      NpcProjectiles = session.SupportsNpcProjectile
        ? sentNpcProjectiles
        : visibleNpcProjectiles,
      NpcStatusEffects = sentNpcStatusEffects
    };
  }

  private static List<NpcProjectileReplicationSnapshot> CollectNpcProjectiles(
    SessionReplicationState session,
    IReadOnlyList<NpcProjectileReplicationSnapshot> projectiles,
    long? currentTick)
  {
    List<NpcProjectileReplicationSnapshot> visible = new();
    for (int index = 0; index < projectiles.Count; index++)
    {
      NpcProjectileReplicationSnapshot projectile = projectiles[index];
      if (!projectile.IsActive && currentTick.HasValue &&
          projectile.TombstoneRetainedUntilTick > 0 &&
          currentTick.Value >= projectile.TombstoneRetainedUntilTick)
      {
        continue;
      }

      if (projectile.IsActive && !session.VisibleSections.Contains(projectile.Section))
      {
        session.MarkNpcProjectileSkipped(projectile);
        continue;
      }

      visible.Add(projectile);
    }

    return visible;
  }
}
