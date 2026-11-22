using System;
using System.Collections.Generic;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Npc;
using Terraria.Physics;
using Terraria.Player;
using Terraria.Projectile;
using Terraria.Relationships;

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
      CombatAttackDefinitionCatalog attackDefinitions,
      int protectionWindowTicks,
      IEnumerable<string>? attributeDependencyKeys = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(resolver);
    ArgumentNullException.ThrowIfNull(handleResolver);
    ArgumentNullException.ThrowIfNull(physicsContacts);
    ArgumentNullException.ThrowIfNull(projectileHits);
    ArgumentNullException.ThrowIfNull(projectileLifecycle);
    ArgumentNullException.ThrowIfNull(attackDefinitions);
    ArgumentOutOfRangeException.ThrowIfNegative(protectionWindowTicks);

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
      // 排在生成之后，使仍在兼容同步路径上的投射物能够继续获得当前 tick 的意图；
      // deferred 创建的实体从下一 tick 起进入这条管线。
      new EcsServerSystemRegistration(
          EcsServerPhase.Preparation,
          EcsServerRegistrationKeys.CombatProjectileMovementIntent,
          new ProjectileMovementIntentSystem(world)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Simulation,
          EcsServerRegistrationKeys.CombatPhysicsContactCapture,
          physicsContacts),
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
              handleResolver),
          after: attributeDependencyKeys,
          reason: attributeDependencyKeys is null
              ? null
              : "Projectile damage requests must observe same-tick inventory attributes."),
      // 无敌窗口的推进必须排在本阶段最前：本帧受击新开的窗口若在同帧被推进，
      // 实际保护时长会少一个 tick。
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatProtectionWindow,
          new ProtectionWindowSystem(world)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatAttackRequest,
          new CombatAttackRequestSystem(world, attackDefinitions, resolver),
          after: [EcsServerRegistrationKeys.CombatProtectionWindow],
          reason: "Attack requests run after the protection-window state has advanced."),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CombatDamageRequest,
          new CombatDamageRequestSystem(world, resolver, protectionWindowTicks),
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
