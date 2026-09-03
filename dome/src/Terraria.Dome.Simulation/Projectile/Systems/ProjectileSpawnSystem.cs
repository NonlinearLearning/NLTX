using System;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Combat.Systems;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Projectile.Definitions;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileSpawnSystem
{
  private readonly HitImmunitySystem _hitImmunitySystem = new();

  public Entity Spawn(
    Arch.Core.World world,
    SpawnProjectileCommand command,
    ProjectileDefinition definition,
    int identity,
    HitImmunityComponent? projectileHitImmunity = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (command.ProjectileType != definition.ProjectileType ||
        !command.Owner.IsValid || identity <= 0 || identity == int.MaxValue ||
        command.MiscText is null || command.MiscText.Length > 100 ||
        !float.IsFinite(command.X) || !float.IsFinite(command.Y) ||
        !float.IsFinite(command.InitialVelocityY) ||
        !float.IsFinite(command.ProjectileSpeed) || !float.IsFinite(command.Ai0) ||
        !float.IsFinite(command.Ai1) || !float.IsFinite(command.Ai2) ||
        command.ShootsEveryUse && command.ProjectileType == 0)
    {
      throw new ArgumentException(
        "Spawn command and definition types must match.",
        nameof(command));
    }

    bool isSentry = command.IsSentry || definition.IsSentry;
    if (command.IsDd2Summon && !isSentry)
    {
      throw new ArgumentException(
        "DD2 summon projectiles must also be sentries.",
        nameof(command));
    }

    definition = definition with
    {
      IsSentry = isSentry,
      DamageClass = command.AuthoritativeDamageClass == ProjectileDamageClass.Generic
        ? definition.DamageClass
        : command.AuthoritativeDamageClass,
      LifetimeTicks = LegacyProjectileLifetimePolicy.Resolve(definition.LifetimeTicks, isSentry)
    };

    float velocityX = command.UseZeroVelocity
      ? 0.0f
      : command.Facing * (command.ProjectileSpeed > 0.0f ? command.ProjectileSpeed : 4.0f);
    float velocityY = command.UseZeroVelocity ? 0.0f : command.InitialVelocityY;

    Entity entity = world.Create(
      new ProjectileTagComponent(),
      new ProjectileDefinitionComponent(
        definition.ProjectileType,
        definition.BehaviorId,
        definition.Damage,
        definition.LifetimeTicks,
        definition.Collider,
        definition.Friendly,
        definition.Hostile,
        definition.MaximumBounces,
        definition.LiquidPolicy,
        definition.PlayerDamagePolicy,
        definition.Knockback,
        definition.OriginalDamage == 0 ? definition.Damage : definition.OriginalDamage,
        definition.CollidesWithTiles,
        definition.OnDespawnAreaDamage,
        definition.OnDespawnStatusEffect,
        definition.OnHitStatusEffect,
        definition.ReflectsFromTiles,
        definition.ChildSpawn,
        definition.BounceVelocityMultiplier,
        definition.MinimumBounceSpeed,
        definition.ExtraUpdates,
        definition.IgnoreWater,
        definition.CorrectSlopeCollision,
        definition.UsesStaticNpcImmunity,
        definition.StaticNpcHitCooldownTicks,
        definition.AppliesImmunityTimeOnSingleHits,
        definition.DamageClass,
        definition.LegacyAiStyle,
        definition.IsColdDamage,
        definition.IsArrow,
        definition.ArmorPenetration,
        definition.BonusCritChance,
        command.AuthoritativeBonusTagDamage > 0
          ? command.AuthoritativeBonusTagDamage
          : definition.BonusTagDamage,
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
        definition.IsSentry,
        definition.IsMinion,
        definition.MinionSlots,
        definition.MinionPosition,
        definition.IsTrap,
        definition.IsBobber,
        definition.IsCounterweight,
        definition.HostileDamageScaling,
        definition.DecidesManualFallThrough,
        definition.ManualDirectionChange,
        definition.UsesOwnerMeleeHitCooldown,
        definition.CopiesOwnerAttackCooldownToLocalImmunityOnSpawn),
      new ProjectileFriendlyStateComponent(definition.Friendly),
      new ProjectileBehaviorComponent(
        definition.BehaviorId,
        ProjectileBehaviorInitialStateFactory.Create(
          definition.BehaviorId,
          identity,
          command.Ai0,
          command.Ai1,
          command.Ai2)),
      new ProjectileNetworkIdentityComponent(command.Owner, identity, Guid.NewGuid()),
      new ProjectilePenetrationComponent(definition.MaximumPenetration),
      new LocationComponent(command.X, command.Y),
      new VelocityComponent(velocityX, velocityY),
      new ProjectileDirectionComponent(command.Facing),
      new ProjectileBannerResponseComponent(command.BannerIdToRespondTo),
      new ProjectileMiscTextComponent(command.MiscText),
      ScaleCollider(definition.Collider, definition.Scale),
      new ProjectileOwnerComponent(command.Owner),
      new ProjectileDamageComponent(definition.Damage),
      new ProjectileBounceComponent(definition.MaximumBounces),
      new ProjectileReflectionComponent(),
      new ProjectileRestrikeDelayComponent(),
      new ProjectileNetworkUpdateComponent(),
      new ProjectileSoundDelayComponent(),
      new ProjectileTileCollisionComponent(definition.CollidesWithTiles),
      new ProjectileStepSpeedComponent(),
      new ProjectileLifetimeComponent(definition.LifetimeTicks),
      new ProjectileUpdateCountComponent());
    if (definition.IsSentry)
    {
      ProjectileSentryComponent sentry = new();
      world.Add(entity, in sentry);
    }

    if (command.IsDd2Summon)
    {
      ProjectileDd2SummonComponent dd2Summon = new();
      world.Add(entity, in dd2Summon);
    }

    if (definition.IsMinion)
    {
      ProjectileMinionComponent minion = new(definition.MinionSlots, definition.MinionPosition);
      world.Add(entity, in minion);

      if (command.MinionSpawnItemType != 0)
      {
        ProjectileMinionSpawnSourceComponent source = new(
          command.MinionSpawnItemType,
          command.MinionSpawnItemPrefix);
        world.Add(entity, in source);
      }
    }

    if (definition.IsTrap)
    {
      ProjectileTrapComponent trap = new();
      world.Add(entity, in trap);
    }

    if (definition.IsBobber)
    {
      ProjectileBobberComponent bobber = new();
      world.Add(entity, in bobber);
    }

    if (definition.IsCounterweight)
    {
      ProjectileCounterweightComponent counterweight = new();
      world.Add(entity, in counterweight);
    }

    if (definition.DecidesManualFallThrough)
    {
      ProjectileFallThroughComponent fallThrough = new();
      world.Add(entity, in fallThrough);
    }

    if (definition.CopiesOwnerAttackCooldownToLocalImmunityOnSpawn &&
        projectileHitImmunity != null)
    {
      _hitImmunitySystem.CopyOwnerMeleeNpcTicksToProjectile(
        projectileHitImmunity,
        command.Owner.Value,
        identity);
    }

    return entity;
  }

  private static ColliderComponent ScaleCollider(ColliderComponent collider, float scale)
  {
    return new ColliderComponent(collider.Width * scale, collider.Height * scale);
  }
}
