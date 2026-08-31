using System;
using System.IO;
using Arch.Core;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation;
using ArchWorld = Arch.Core.World;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class NpcProjectileReplicationSystem
{
  public NpcProjectileReplicationSnapshot Project(
    Entity entity,
    ArchWorld world,
    int replicationId,
    long revision,
    WorldSectionCoordinates section)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (replicationId <= 0 || revision < 0 || !world.IsAlive(entity))
    {
      throw new ArgumentOutOfRangeException(nameof(replicationId));
    }

    TransformComponent transform = world.Get<TransformComponent>(entity);
    VelocityComponent velocity = world.Get<VelocityComponent>(entity);
    NpcProjectileOwnerComponent owner = world.Get<NpcProjectileOwnerComponent>(entity);
    ProjectileDamageComponent damage = world.Get<ProjectileDamageComponent>(entity);
    ProjectileLifetimeComponent lifetime = world.Get<ProjectileLifetimeComponent>(entity);
    ProjectilePenetrationComponent penetration = world.Get<ProjectilePenetrationComponent>(entity);
    ProjectileDefinitionComponent definition = world.Get<ProjectileDefinitionComponent>(entity);
    bool friendly = !world.Has<ProjectileFriendlyStateComponent>(entity) ||
      world.Get<ProjectileFriendlyStateComponent>(entity).IsFriendly;
    NpcProjectileNetworkIdentityComponent identity =
      world.Get<NpcProjectileNetworkIdentityComponent>(entity);
    ProjectileBehaviorReplicationState behaviorState = ProjectileBehaviorStateProjection.Project(
      world.Get<ProjectileBehaviorComponent>(entity));
    ProjectileUpdateCountComponent updates = world.Get<ProjectileUpdateCountComponent>(entity);
    if (!owner.Owner.IsValid || identity.Identity <= 0 || identity.Identity == int.MaxValue ||
        identity.ProjectileUuid == null || identity.ProjectileUuid == Guid.Empty ||
        !IsFinite(new SimulationVector(transform.X, transform.Y)) ||
        !IsFinite(new SimulationVector(velocity.X, velocity.Y)) || damage.Amount <= 0 ||
        lifetime.RemainingTicks <= 0)
    {
      throw new InvalidDataException("NPC projectile replication state is invalid.");
    }

    return new NpcProjectileReplicationSnapshot(
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
      behaviorState.Ai0,
      behaviorState.Ai1,
      behaviorState.Ai2,
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
      BannerIdToRespondTo: world.Get<ProjectileBannerResponseComponent>(entity).BannerId,
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
      LocalAi0: world.Get<ProjectileBehaviorComponent>(entity).State.LocalAi0,
      LocalAi1: world.Get<ProjectileBehaviorComponent>(entity).State.LocalAi1,
      LocalAi2: world.Get<ProjectileBehaviorComponent>(entity).State.LocalAi2,
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

  public NpcProjectileReplicationSnapshot ProjectTombstone(
    Entity entity,
    ArchWorld world,
    int replicationId,
    long revision,
    long currentTick,
    WorldSectionCoordinates section,
    ProjectileTombstoneReason reason)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (replicationId <= 0 || revision < 0 || !world.IsAlive(entity))
    {
      throw new ArgumentOutOfRangeException(nameof(replicationId));
    }

    TransformComponent transform = world.Get<TransformComponent>(entity);
    VelocityComponent velocity = world.Get<VelocityComponent>(entity);
    NpcProjectileOwnerComponent owner = world.Get<NpcProjectileOwnerComponent>(entity);
    ProjectileDamageComponent damage = world.Get<ProjectileDamageComponent>(entity);
    ProjectilePenetrationComponent penetration = world.Get<ProjectilePenetrationComponent>(entity);
    ProjectileDefinitionComponent definition = world.Get<ProjectileDefinitionComponent>(entity);
    bool friendly = !world.Has<ProjectileFriendlyStateComponent>(entity) ||
      world.Get<ProjectileFriendlyStateComponent>(entity).IsFriendly;
    NpcProjectileNetworkIdentityComponent identity =
      world.Get<NpcProjectileNetworkIdentityComponent>(entity);
    ProjectileBehaviorReplicationState behaviorState = ProjectileBehaviorStateProjection.Project(
      world.Get<ProjectileBehaviorComponent>(entity));
    ProjectileUpdateCountComponent updates = world.Get<ProjectileUpdateCountComponent>(entity);
    SimulationVector position = new(transform.X, transform.Y);
    SimulationVector direction = new(velocity.X, velocity.Y);
    if (!owner.Owner.IsValid || identity.Identity <= 0 || identity.Identity == int.MaxValue ||
        identity.ProjectileUuid == null || identity.ProjectileUuid == Guid.Empty ||
        !IsFinite(position) || !IsFinite(direction) || damage.Amount <= 0)
    {
      throw new InvalidDataException("NPC projectile tombstone state is invalid.");
    }

    ProjectileTombstoneReason normalizedReason = reason == ProjectileTombstoneReason.None
      ? ProjectileTombstoneReason.Administrative
      : reason;
    return new NpcProjectileReplicationSnapshot(
      replicationId,
      definition.ProjectileType,
      owner.Owner,
      position,
      direction,
      damage.Amount,
      0,
      false,
      revision,
      section,
      identity.Identity,
      identity.ProjectileUuid,
      behaviorState.Ai0,
      behaviorState.Ai1,
      behaviorState.Ai2,
      normalizedReason,
      ProjectileTombstonePolicy.CalculateRetentionUntil(currentTick),
      definition.Knockback,
      definition.OriginalDamage,
      world.Has<ProjectileReflectionComponent>(entity) &&
        world.Get<ProjectileReflectionComponent>(entity).HasReflected,
      definition.LegacyAiStyle,
      definition.DamageClass,
      definition.IsColdDamage,
      definition.IsArrow,
      damage.HitCount,
      updates.Count,
      definition.ArmorPenetration,
      definition.BonusCritChance,
      definition.BonusTagDamage,
      definition.TagEffectType,
      definition.StopsDealingDamageAfterPenetrateHits,
      definition.NoEnchantments,
      definition.NoEnchantmentVisuals,
      definition.OriginatedFromActivableTile,
      definition.NoDropItem,
      definition.UsesLocalNpcImmunity,
      definition.LocalNpcHitCooldownTicks,
      definition.IsNetworkImportant,
      definition.Scale,
      definition.OwnerHitCheck,
      definition.OwnerHitCheckDistance,
      world.Has<ProjectileSentryComponent>(entity),
      world.Has<ProjectileMinionComponent>(entity),
      world.Has<ProjectileMinionComponent>(entity)
        ? world.Get<ProjectileMinionComponent>(entity).Slots
        : 0.0f,
      world.Has<ProjectileMinionComponent>(entity)
        ? world.Get<ProjectileMinionComponent>(entity).Position
        : 0,
      world.Has<ProjectileTrapComponent>(entity),
      world.Has<ProjectileBobberComponent>(entity),
      world.Has<ProjectileCounterweightComponent>(entity),
      MaximumPenetration: penetration.MaximumPenetration,
      DecidesManualFallThrough: definition.DecidesManualFallThrough,
      ShouldFallThrough: world.Has<ProjectileFallThroughComponent>(entity) &&
        world.Get<ProjectileFallThroughComponent>(entity).ShouldFallThrough,
      Direction: world.Get<ProjectileDirectionComponent>(entity).Horizontal,
      ManualDirectionChange: definition.ManualDirectionChange,
      BannerIdToRespondTo: world.Get<ProjectileBannerResponseComponent>(entity).BannerId,
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
      SecondaryUpdatePending: world.Get<ProjectileNetworkUpdateComponent>(entity)
        .SecondaryUpdatePending,
      NetSpam: world.Get<ProjectileNetworkUpdateComponent>(entity).NetSpam,
      SoundDelay: world.Get<ProjectileSoundDelayComponent>(entity).RemainingTicks,
      TileCollisionEnabled: world.Get<ProjectileTileCollisionComponent>(entity).Enabled,
      PrimaryUpdatePending: world.Get<ProjectileNetworkUpdateComponent>(entity)
        .PrimaryUpdatePending,
      NetworkUpdateReady: true);
  }

  private static bool IsFinite(SimulationVector value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }
}
