using System;
using System.Collections.Generic;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Npc;
using Terraria.Physics;
using Terraria.Projectile;
using Terraria.Player;
using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>Creates the explicit local Tick registration for shared hostile damage Combat.</summary>
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
      CombatDamageRequestBuffer? requestBuffer = null,
      PhysicsContactBuffer? contactBuffer = null,
      CombatAttackIntentBuffer? attackIntents = null)
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

    CombatDamageRequestBuffer buffer = requestBuffer ?? new CombatDamageRequestBuffer();
    CombatAttackIntentBuffer intents = attackIntents ?? new CombatAttackIntentBuffer();
    PhysicsContactBuffer contacts = contactBuffer ?? physicsContacts.Buffer;
    if (!ReferenceEquals(contacts, physicsContacts.Buffer))
    {
      throw new ArgumentException(
          "Physics contact capture and Combat adapters must share one contact buffer.",
          nameof(contactBuffer));
    }
    projectileHits.AttachTerminationBuffer(projectileLifecycle.Terminations);
    return Array.AsReadOnly(new EcsServerSystemRegistration[]
    {
      new EcsServerSystemRegistration(EcsServerPhase.Simulation, physicsContacts),
      new EcsServerSystemRegistration(
          EcsServerPhase.Simulation,
          new ProjectileLifetimeTickSystem(
              world, projectileLifecycle.Terminations, handleResolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Simulation,
          new ProjectileDamageRequestSystem(
              world,
              projectileHits,
              intents,
              handleResolver,
              contacts)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new CombatAttackRequestSystem(world, intents, buffer, attackDefinitions, resolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new CombatDamageRequestSystem(world, buffer, resolver)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new ProjectileDamageResultSystem(
              world, handleResolver, buffer, projectileHits)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new NpcCombatResultSystem(world, handleResolver, buffer)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new PlayerCombatResultSystem(world, handleResolver, buffer)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new PlayerDeathRespawnSystem(world)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new ProjectileTerminationCommitSystem(
              world, projectileLifecycle, handleResolver, projectileLifecycle.Terminations)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          new ProtectionWindowSystem(world))
    });
  }
}
