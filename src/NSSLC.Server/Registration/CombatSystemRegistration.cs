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
      CombatAttackDefinitionCatalog attackDefinitions)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(resolver);
    ArgumentNullException.ThrowIfNull(handleResolver);
    ArgumentNullException.ThrowIfNull(physicsContacts);
    ArgumentNullException.ThrowIfNull(projectileHits);
    ArgumentNullException.ThrowIfNull(projectileLifecycle);
    ArgumentNullException.ThrowIfNull(attackDefinitions);

    if (!projectileLifecycle.SharesRuntimeRegistryWith(resolver, handleResolver))
    {
      throw new ArgumentException(
          "Combat sources and projectile lifecycle must share one world-scoped entity registry.");
    }

    return Array.AsReadOnly(new EcsServerSystemRegistration[]
    {
      // 生成接缝放在 Preparation：新投射物当帧就能被 Simulation 相位的空间协调系统驱动。
      new EcsServerSystemRegistration(
          EcsServerPhase.Preparation,
          new ProjectileSpawnRequestSystem(world, projectileLifecycle)),
      // 必须排在生成之后：同帧新生成的投射物也要拿到当帧意图，否则会被
      // SpatialSimulationSystem 的 intent.Tick != frame.Tick 校验拒绝。
      new EcsServerSystemRegistration(
          EcsServerPhase.Preparation,
          new ProjectileMovementIntentSystem(world)),
      new EcsServerSystemRegistration(EcsServerPhase.Simulation, physicsContacts),
      new EcsServerSystemRegistration(
          EcsServerPhase.Simulation,
          new ProjectileLifetimeTickSystem(world, handleResolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Simulation,
          new ProjectileDamageRequestSystem(
              world,
              projectileHits,
              handleResolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new CombatAttackRequestSystem(world, attackDefinitions, resolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new CombatDamageRequestSystem(world, resolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new ProjectileDamageResultSystem(
              world, handleResolver, projectileHits)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new NpcCombatResultSystem(world, handleResolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new PlayerCombatResultSystem(world, handleResolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new PlayerDeathRespawnSystem(world, handleResolver)),
      // 必须紧跟死亡/复活系统：本帧到期的复活要在同一帧恢复满血。
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new LifeRestoreRequestSystem(world, resolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new ProjectileTerminationCommitSystem(
              world, projectileLifecycle, handleResolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new ProtectionWindowSystem(world))
    });
  }
}
