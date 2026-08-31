using System;
using System.IO;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileReplicationSystem
{
  public ProjectileReplicationSnapshot Project(
    Entity entity,
    Arch.Core.World world,
    int replicationId,
    long revision,
    WorldSectionCoordinates section)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (replicationId <= 0 || revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(replicationId));
    }

    if (!world.IsAlive(entity))
    {
      throw new ArgumentException("Projectile entity is not alive in the supplied world.", nameof(entity));
    }

    TransformComponent transform = world.Get<TransformComponent>(entity);
    VelocityComponent velocity = world.Get<VelocityComponent>(entity);
    ProjectileOwnerComponent owner = world.Get<ProjectileOwnerComponent>(entity);
    ProjectileDamageComponent damage = world.Get<ProjectileDamageComponent>(entity);
    ProjectileLifetimeComponent lifetime = world.Get<ProjectileLifetimeComponent>(entity);
    ProjectilePenetrationComponent penetration = world.Get<ProjectilePenetrationComponent>(entity);
    ProjectileDefinitionComponent definition = world.Get<ProjectileDefinitionComponent>(entity);
    bool friendly = !world.Has<ProjectileFriendlyStateComponent>(entity) ||
      world.Get<ProjectileFriendlyStateComponent>(entity).IsFriendly;
    ProjectileNetworkIdentityComponent identity = world.Get<ProjectileNetworkIdentityComponent>(entity);
    ProjectileBehaviorComponent behavior = world.Get<ProjectileBehaviorComponent>(entity);
    ProjectileUpdateCountComponent updates = world.Get<ProjectileUpdateCountComponent>(entity);
    if (behavior.BehaviorId != definition.BehaviorId)
    {
      throw new InvalidDataException("Projectile behavior and definition do not match.");
    }

    if (!ProjectileOwnershipPolicy.OwnedBySomeone(owner))
    {
      throw new InvalidDataException("Projectile replication requires a valid owner.");
    }

    ProjectileBehaviorReplicationState behaviorState =
      ProjectileBehaviorStateProjection.Project(behavior);
    return new ProjectileReplicationSnapshot(
      replicationId,
      definition.ProjectileType,
      owner.Owner,
      new SimulationVector(transform.X, transform.Y),
      new SimulationVector(velocity.X, velocity.Y),
      damage.Amount,
      lifetime.RemainingTicks,
      true,
      revision,
      section,
      identity.Identity,
      identity.ProjectileUuid,
      Ai0: behaviorState.Ai0,
      Ai1: behaviorState.Ai1,
      Ai2: behaviorState.Ai2,
      Banner: world.Get<ProjectileBannerResponseComponent>(entity).BannerId,
      DefinitionKnockback: definition.Knockback,
      DefinitionOriginalDamage: definition.OriginalDamage,
      Reflected: world.Has<ProjectileReflectionComponent>(entity) &&
        world.Get<ProjectileReflectionComponent>(entity).HasReflected,
      LegacyAiStyle: definition.LegacyAiStyle,
      DamageClass: definition.DamageClass,
      IsColdDamage: definition.IsColdDamage,
      IsArrow: definition.IsArrow,
      HitCount: damage.HitCount,
      ArmorPenetration: definition.ArmorPenetration,
      BonusCritChance: definition.BonusCritChance,
      BonusTagDamage: definition.BonusTagDamage,
      TagEffectType: definition.TagEffectType,
      StopsDealingDamageAfterPenetrateHits: definition.StopsDealingDamageAfterPenetrateHits,
      NoEnchantments: definition.NoEnchantments,
      NoEnchantmentVisuals: definition.NoEnchantmentVisuals,
      OriginatedFromActivableTile: definition.OriginatedFromActivableTile,
      NoDropItem: definition.NoDropItem,
      UsesLocalNpcImmunity: definition.UsesLocalNpcImmunity,
      LocalNpcHitCooldownTicks: definition.LocalNpcHitCooldownTicks,
      IsNetworkImportant: definition.IsNetworkImportant,
      Scale: definition.Scale,
      OwnerHitCheck: definition.OwnerHitCheck,
      OwnerHitCheckDistance: definition.OwnerHitCheckDistance,
      IsSentry: world.Has<ProjectileSentryComponent>(entity),
      IsDd2Summon: world.Has<ProjectileDd2SummonComponent>(entity),
      IsMinion: world.Has<ProjectileMinionComponent>(entity),
      MinionSlots: world.Has<ProjectileMinionComponent>(entity)
        ? world.Get<ProjectileMinionComponent>(entity).Slots
        : 0.0f,
      MinionPosition: world.Has<ProjectileMinionComponent>(entity)
        ? world.Get<ProjectileMinionComponent>(entity).Position
        : 0,
      IsTrap: world.Has<ProjectileTrapComponent>(entity),
      IsBobber: world.Has<ProjectileBobberComponent>(entity),
      IsCounterweight: world.Has<ProjectileCounterweightComponent>(entity),
      MaximumPenetration: penetration.MaximumPenetration,
      DecidesManualFallThrough: definition.DecidesManualFallThrough,
      ShouldFallThrough: world.Has<ProjectileFallThroughComponent>(entity) &&
        world.Get<ProjectileFallThroughComponent>(entity).ShouldFallThrough,
      Direction: world.Get<ProjectileDirectionComponent>(entity).Horizontal,
      ManualDirectionChange: definition.ManualDirectionChange,
      UsesOwnerMeleeHitCooldown: definition.UsesOwnerMeleeHitCooldown,
      CopiesOwnerAttackCooldownToLocalImmunityOnSpawn:
        definition.CopiesOwnerAttackCooldownToLocalImmunityOnSpawn,
      HostileDamageScaling: definition.HostileDamageScaling,
      CollidesWithTiles: definition.CollidesWithTiles,
      IgnoreWater: definition.IgnoreWater,
      ReflectsFromTiles: definition.ReflectsFromTiles,
      CorrectSlopeCollision: definition.CorrectSlopeCollision,
      MaximumBounces: definition.MaximumBounces,
      BounceVelocityMultiplier: definition.BounceVelocityMultiplier,
      MinimumBounceSpeed: definition.MinimumBounceSpeed,
      LiquidPolicy: definition.LiquidPolicy,
      ChildSpawn: definition.ChildSpawn,
      OnHitStatusEffect: definition.OnHitStatusEffect,
      OnDespawnStatusEffect: definition.OnDespawnStatusEffect,
      OnDespawnAreaDamage: definition.OnDespawnAreaDamage,
      BehaviorId: definition.BehaviorId,
      Friendly: friendly,
      Hostile: definition.Hostile,
      PlayerDamagePolicy: definition.PlayerDamagePolicy,
      MinionSpawnItemType: world.Has<ProjectileMinionSpawnSourceComponent>(entity)
        ? world.Get<ProjectileMinionSpawnSourceComponent>(entity).ItemType
        : (ushort)0,
      MinionSpawnItemPrefix: world.Has<ProjectileMinionSpawnSourceComponent>(entity)
        ? world.Get<ProjectileMinionSpawnSourceComponent>(entity).ItemPrefix
        : 0,
      LocalAi0: behavior.State.LocalAi0,
      LocalAi1: behavior.State.LocalAi1,
      LocalAi2: behavior.State.LocalAi2,
      SecondaryUpdatePending: world.Get<ProjectileNetworkUpdateComponent>(entity)
        .SecondaryUpdatePending,
      NetSpam: world.Get<ProjectileNetworkUpdateComponent>(entity).NetSpam,
      SoundDelay: world.Get<ProjectileSoundDelayComponent>(entity).RemainingTicks,
      TileCollisionEnabled: world.Get<ProjectileTileCollisionComponent>(entity).Enabled,
      PrimaryUpdatePending: world.Get<ProjectileNetworkUpdateComponent>(entity)
        .PrimaryUpdatePending,
      NetworkUpdateReady: revision == 1 ||
        world.Get<ProjectileNetworkUpdateComponent>(entity).SendRequested,
      UpdateCount: updates.Count);
  }
}
