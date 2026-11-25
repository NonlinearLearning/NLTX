using System;
using System.Collections.Generic;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Unshared.Npc;
using Terraria.Share.Physics;
using Terraria.Unshared.Player;
using Terraria.Unshared.Projectiles;
using Terraria.Share.Relationships;

namespace Terraria.Server;

/// <summary>Creates the explicit local Tick registration for shared hostile damage Combat.</summary>
/// <remarks>
/// Assembly wiring belongs to the server host, not to the component library: the library publishes
/// systems, the host decides which of them run and in what order.
/// </remarks>
public static class CombatSystemRegistration
{
  public static IReadOnlyList<EcsServerSystemRegistration> Create(
      ArchWorld world,
      IEntityRuntimeResolver resolver,
      IEntityRuntimeHandleResolver handleResolver,
      PhysicsContactCaptureSystem physicsContacts,
      ProjectileHitResolutionSystem projectileHits,
      ProjectileLifecycleSystem projectileLifecycle,
      ProjectileTileResolutionSystem projectileTiles,
      CombatAttackCatalog attackDefinitions,
      IEnumerable<string>? attributeDependencyKeys = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(resolver);
    ArgumentNullException.ThrowIfNull(handleResolver);
    ArgumentNullException.ThrowIfNull(physicsContacts);
    ArgumentNullException.ThrowIfNull(projectileHits);
    ArgumentNullException.ThrowIfNull(projectileLifecycle);
    ArgumentNullException.ThrowIfNull(projectileTiles);
    ArgumentNullException.ThrowIfNull(attackDefinitions);

    if (!projectileLifecycle.SharesRuntimeRegistryWith(resolver, handleResolver))
    {
      throw new ArgumentException(
          "Combat sources and projectile lifecycle must share one world-scoped entity registry.");
    }

    return Array.AsReadOnly(new EcsServerSystemRegistration[]
    {
      // 生成接缝放在 Preparation：结构创建提交到当前 tick 的 Commit barrier，
      // 新投射物从下一个 Simulation tick 起由空间协调系统驱动。
      new EcsServerSystemRegistration(
          EcsServerPhase.Preparation,
          EcsServerRegistrationKeys.CombatProjectileSpawn,
          new ProjectileSpawnRequestSystem(world, projectileLifecycle)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Simulation,
          EcsServerRegistrationKeys.CombatPhysicsContactCapture,
          physicsContacts),
      new EcsServerSystemRegistration(
          EcsServerPhase.Simulation,
          EcsServerRegistrationKeys.CombatProjectileTileResolution,
          projectileTiles,
          after: [EcsServerRegistrationKeys.CombatPhysicsContactCapture],
          reason: "Tile contacts are drained after the spatial step that produces them."),
      new EcsServerSystemRegistration(
          EcsServerPhase.Simulation,
          EcsServerRegistrationKeys.CombatProjectileLifetime,
          new ProjectileLifetimeTickSystem(world, handleResolver),
          after: attributeDependencyKeys,
          reason: attributeDependencyKeys is null
              ? null
              : "Projectile simulation reads current inventory-derived combat attributes."),
      new EcsServerSystemRegistration(
          EcsServerPhase.Simulation,
          EcsServerRegistrationKeys.CombatProjectileDamageRequest,
          new ProjectileDamageRequestSystem(
              world,
              projectileHits,
              handleResolver,
              attackDefinitions),
          after: attributeDependencyKeys,
          reason: attributeDependencyKeys is null
              ? null
              : "Projectile damage requests must observe same-tick inventory attributes."),
      // 无敌窗口的到期归零必须排在本阶段最前：归零只清理过期戳，不参与「是否被挡」的判断，
      // 所以早跑晚跑都不会让窗口多生效一个 tick；排最前是为了让同帧的读取看到规范状态。
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatImmunityWindow,
          new ImmunityWindowExpirySystem(world)),
      // 攻击者分槽的稀疏表没有上限保护，Prune 必须每 tick 无条件跑，不能等消费者触发。
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatAttackerImmunity,
          new AttackerImmunityExpirySystem(world)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatAttackRequest,
          new CombatAttackRequestSystem(world, attackDefinitions, resolver),
          after: [EcsServerRegistrationKeys.CombatImmunityWindow],
          reason: "Attack requests run after the immunity-window state has been normalised."),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatDamageRequest,
          new CombatDamageRequestSystem(world, resolver),
          after: [EcsServerRegistrationKeys.CombatAttackRequest],
          reason: "Damage requests consume attack requests produced earlier in Commit."),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatProjectileDamageResult,
          new ProjectileDamageResultSystem(
              world, handleResolver, projectileHits),
          after: [EcsServerRegistrationKeys.CombatDamageRequest],
          reason: "Damage results consume committed damage outcomes."),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatNpcResult,
          new NpcCombatResultSystem(world, handleResolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatPlayerResult,
          new PlayerCombatResultSystem(world, handleResolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatPlayerDeathRespawn,
          new PlayerDeathRespawnSystem(world, handleResolver),
          after: [EcsServerRegistrationKeys.CombatPlayerResult],
          reason: "Death and respawn observe the player combat result for this tick."),
      // 必须紧跟死亡/复活系统：本帧到期的复活要在同一帧恢复满血。
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatLifeRestore,
          new LifeRestoreRequestSystem(world, resolver),
          after: [EcsServerRegistrationKeys.CombatPlayerDeathRespawn],
          reason: "Expired respawn requests restore health after death and respawn resolution."),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatProjectileTermination,
          new ProjectileTerminationCommitSystem(
              world, projectileLifecycle, handleResolver),
          after: [EcsServerRegistrationKeys.CombatProjectileDamageResult],
          reason: "Projectile termination consumes damage and lifetime terminal facts.")
    });
  }
}
