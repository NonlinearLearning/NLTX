using System;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Projectile.Definitions;
using ArchWorld = Arch.Core.World;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class NpcProjectileSpawnSystem
{
  public Entity Spawn(
    ArchWorld world,
    NpcProjectileSpawnRequest request,
    int identity)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (!request.SourceNpc.IsValid || request.Sequence < 0 || identity <= 0 ||
        identity == int.MaxValue || request.Damage <= 0 ||
        !IsFinite(request.Position) || !IsFinite(request.Velocity) ||
        request.Definition.Hostile == request.Definition.Friendly ||
        request.Definition.MaximumPenetration == 0 ||
        request.Definition.PlayerDamagePolicy != PlayerDamagePolicy.HostileNonPvp)
    {
      throw new ArgumentException("NPC projectile request is invalid.", nameof(request));
    }

    Entity entity = world.Create(
      new ProjectileTagComponent(),
      new ProjectileDefinitionComponent(
        request.Definition.ProjectileType,
        request.Definition.BehaviorId,
        request.Definition.Damage,
        request.Definition.LifetimeTicks,
        request.Definition.Collider,
        request.Definition.Friendly,
        request.Definition.Hostile,
        request.Definition.MaximumBounces,
        request.Definition.LiquidPolicy,
        request.Definition.PlayerDamagePolicy,
        request.Definition.Knockback,
        request.Definition.OriginalDamage == 0
          ? request.Definition.Damage
          : request.Definition.OriginalDamage,
        request.Definition.CollidesWithTiles,
        request.Definition.OnDespawnAreaDamage,
        request.Definition.OnDespawnStatusEffect,
        request.Definition.OnHitStatusEffect,
        request.Definition.ReflectsFromTiles,
        request.Definition.ChildSpawn,
        request.Definition.BounceVelocityMultiplier,
        request.Definition.MinimumBounceSpeed,
        request.Definition.ExtraUpdates,
        request.Definition.IgnoreWater,
        request.Definition.CorrectSlopeCollision,
        request.Definition.UsesStaticNpcImmunity,
        request.Definition.StaticNpcHitCooldownTicks,
        request.Definition.AppliesImmunityTimeOnSingleHits,
        request.Definition.DamageClass,
        request.Definition.LegacyAiStyle,
        request.Definition.IsColdDamage,
        request.Definition.IsArrow,
        request.Definition.ArmorPenetration,
        request.Definition.BonusCritChance,
        request.Definition.BonusTagDamage,
        request.Definition.TagEffectType,
        request.Definition.StopsDealingDamageAfterPenetrateHits,
        request.Definition.NoEnchantments,
        request.Definition.NoEnchantmentVisuals,
        request.Definition.OriginatedFromActivableTile,
        request.Definition.NoDropItem,
        request.Definition.UsesLocalNpcImmunity,
        request.Definition.LocalNpcHitCooldownTicks,
        request.Definition.IsNetworkImportant,
        request.Definition.Scale,
        request.Definition.OwnerHitCheck,
        request.Definition.OwnerHitCheckDistance,
        request.Definition.IsSentry,
        request.Definition.IsMinion,
        request.Definition.MinionSlots,
        request.Definition.MinionPosition,
        request.Definition.IsTrap,
        request.Definition.IsBobber,
        request.Definition.IsCounterweight,
        request.Definition.HostileDamageScaling,
        request.Definition.DecidesManualFallThrough,
        request.Definition.ManualDirectionChange,
        request.Definition.UsesOwnerMeleeHitCooldown,
        request.Definition.CopiesOwnerAttackCooldownToLocalImmunityOnSpawn),
      new ProjectileFriendlyStateComponent(request.Definition.Friendly),
      new ProjectileBehaviorComponent(
        request.Definition.BehaviorId,
        ProjectileBehaviorInitialStateFactory.Create(request.Definition.BehaviorId, identity)),
      new NpcProjectileNetworkIdentityComponent(request.SourceNpc, identity, Guid.NewGuid()),
      new ProjectilePenetrationComponent(request.Definition.MaximumPenetration),
      new LocationComponent(request.Position.X, request.Position.Y),
      new VelocityComponent(request.Velocity.X, request.Velocity.Y),
      new ProjectileDirectionComponent(request.Velocity.X < 0.0f ? -1 : 1),
      new ProjectileBannerResponseComponent(request.BannerIdToRespondTo),
      ScaleCollider(request.Definition.Collider, request.Definition.Scale),
      new NpcProjectileOwnerComponent(request.SourceNpc),
      new ProjectileDamageComponent(request.Damage),
      new ProjectileBounceComponent(request.Definition.MaximumBounces),
      new ProjectileReflectionComponent(),
      new ProjectileRestrikeDelayComponent(),
      new ProjectileNetworkUpdateComponent(),
      new ProjectileSoundDelayComponent(),
      new ProjectileTileCollisionComponent(request.Definition.CollidesWithTiles),
      new ProjectileStepSpeedComponent(),
      new ProjectileLifetimeComponent(request.Definition.LifetimeTicks),
      new ProjectileUpdateCountComponent());
    if (request.Definition.IsSentry)
    {
      ProjectileSentryComponent sentry = new();
      world.Add(entity, in sentry);
    }

    if (request.Definition.IsMinion)
    {
      ProjectileMinionComponent minion = new(
        request.Definition.MinionSlots,
        request.Definition.MinionPosition);
      world.Add(entity, in minion);
    }

    if (request.Definition.IsTrap)
    {
      ProjectileTrapComponent trap = new();
      world.Add(entity, in trap);
    }

    if (request.Definition.IsBobber)
    {
      ProjectileBobberComponent bobber = new();
      world.Add(entity, in bobber);
    }

    if (request.Definition.IsCounterweight)
    {
      ProjectileCounterweightComponent counterweight = new();
      world.Add(entity, in counterweight);
    }

    if (request.Definition.DecidesManualFallThrough)
    {
      ProjectileFallThroughComponent fallThrough = new();
      world.Add(entity, in fallThrough);
    }

    return entity;
  }

  private static bool IsFinite(SimulationVector value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }

  private static ColliderComponent ScaleCollider(ColliderComponent collider, float scale)
  {
    return new ColliderComponent(collider.Width * scale, collider.Height * scale);
  }
}
