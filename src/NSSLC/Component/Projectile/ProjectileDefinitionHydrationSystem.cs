using System;
using System.Numerics;

using Terraria.Content;

namespace Terraria.Projectile;

public static class ProjectileDefinitionHydrationSystem
{
  public static bool TryHydrate(
    ProjectileDefinition definition,
    ProjectileSpawnCommand spawn,
    ProjectileDefinitionHydrationContext context,
    out ProjectileEntityState? state)
  {
    ArgumentNullException.ThrowIfNull(definition);

    if (!IsRepresentable(definition, spawn))
    {
      state = null;
      return false;
    }

    ProjectileIdentityDefinition identityDefinition = definition.Identity;
    ProjectileGeometryDefinition geometry = definition.Geometry;
    ProjectileBehaviorDefinition behavior = definition.Behavior;
    ProjectileCombatDefinition combat = definition.Combat;
    ProjectilePenetrationDefinition penetration = definition.Penetration;
    ProjectileCapabilitiesDefinition capabilities = definition.Capabilities;
    ProjectilePresentationDefinition presentation = definition.Presentation;

    int width = (int)((float)geometry.Width * geometry.Scale);
    int height = (int)((float)geometry.Height * geometry.Scale);
    ProjectileHostileDamageScaling hostileScaling = combat.HostileDamageScaling switch
    {
      ProjectileHostileDamageScalingDefinition.Default =>
        ProjectileHostileDamageScaling.Default,
      ProjectileHostileDamageScalingDefinition.Lightning =>
        ProjectileHostileDamageScaling.Lightning,
      _ => throw new InvalidOperationException(
        "Projectile content contains an unsupported hostile damage scaling value."),
    };

    var projectileState = new ProjectileEntityState(
      new ProjectileIdentityComponent(
        spawn.Owner.EntityReference,
        ownerSlot: spawn.Owner.LegacyOwnerSlot),
      new ProjectileLifetimeStateComponent(timeLeft: behavior.DefaultTimeLeft),
      new ProjectileNetworkStateComponent(
        playerCapacity: context.PlayerCapacity,
        networkImportant: definition.Network.NetworkImportant))
    {
      Definition = new ProjectileDefinitionComponent(
        identityDefinition.TypeId,
        behavior.AiStyle,
        combat.Friendly,
        combat.Hostile,
        behavior.ExtraUpdates,
        context.CatalogRevision,
        capabilities.NoEnchantments),
      Geometry = new ProjectileGeometryStateComponent(
        width,
        height,
        geometry.Scale),
      Behavior = new ProjectileBehaviorStateComponent(
        spawn.Ai0,
        spawn.Ai1,
        spawn.Ai2),
      UpdateCadence = new ProjectileUpdateCadenceComponent(behavior.ExtraUpdates),
      Disposition = new ProjectileDispositionStateComponent(
        combat.Friendly,
        combat.Hostile),
      Damage = new ProjectileDamagePayloadComponent(
        currentDamage: spawn.Damage,
        originalDamage: spawn.OriginalDamage,
        knockback: spawn.Knockback,
        armorPenetration: 0,
        bonusCritChance: 0,
        damageClass: ProjectileDamageClass.Generic,
        isColdDamage: combat.ColdDamage,
        isArrow: capabilities.IsArrow,
        hostileDamageScaling: hostileScaling,
        melee: combat.Melee,
        ranged: combat.Ranged,
        magic: combat.Magic),
      DamagePolicy = new ProjectileDamagePolicyComponent(
        capabilities.NoEnchantments,
        capabilities.NoEnchantmentVisuals),
      Penetration = new ProjectilePenetrationStateComponent(
        penetration.DefaultPenetrate,
        penetration.MaxPenetrate,
        stopsDealingDamageWhenDepleted:
          penetration.StopsDealingDamageAfterPenetrateHits),
      Collision = new ProjectileCollisionPolicyComponent(
        tileCollisionEnabled: geometry.TileCollide,
        ignoreWater: geometry.IgnoreWater,
        correctSlopeCollision: geometry.CorrectSlopeCollision,
        decidesManualFallThrough: behavior.DecidesManualFallThrough,
        shouldFallThrough: behavior.ShouldFallThrough,
        ownerHitCheckDistance: geometry.OwnerHitCheckDistance),
      HitImmunityPolicy = new ProjectileHitImmunityPolicyComponent(
        usesLocalNpcImmunity: penetration.UsesLocalNpcImmunity,
        usesStaticNpcImmunity: penetration.UsesIdStaticNpcImmunity,
        localNpcCooldownTicks: penetration.LocalNpcHitCooldown,
        staticNpcCooldownTicks: penetration.IdStaticNpcHitCooldown,
        appliesOnSingleHit: penetration.AppliesImmunityTimeOnSingleHits,
        usesOwnerMeleeCooldown: capabilities.UsesOwnerMeleeHitCooldown),
      HitImmunity = new ProjectileHitImmunityStateComponent(
        context.NpcCapacity,
        context.PlayerCapacity),
      Presentation = new ProjectilePresentationStateComponent(
        alpha: presentation.Alpha,
        glowMask: presentation.GlowMaskId ?? -1,
        light: presentation.Light,
        drawLayer: presentation.DrawLayer,
        usesOwnerLight: capabilities.UsesOwnerLight,
        hide: presentation.Hide),
      Animation = new ProjectileAnimationStateComponent(
        frameCount: presentation.FrameCount),
      Trail = new ProjectileTrailCacheComponent(presentation.TrailCacheLength),
      Source = new ProjectileSourceMetadataComponent(
        bannerIdToRespondTo: spawn.BannerIdToRespondTo,
        isNpcProjectile: combat.NpcProjectile),
      Minion = new ProjectileMinionCapabilityComponent(
        minionSlots: capabilities.MinionSlots,
        isMinion: capabilities.IsMinion),
      Sentry = new ProjectileSentryCapabilityComponent(capabilities.IsSentry),
      Bobber = new ProjectileBobberCapabilityComponent(
        bobberType: capabilities.IsBobber ? identityDefinition.TypeId : 0,
        isBobber: capabilities.IsBobber),
      Counterweight = new ProjectileCounterweightCapabilityComponent(
        capabilities.IsCounterweight),
      Trap = new ProjectileTrapCapabilityComponent(combat.Trap),
      Kinematics = new ProjectileKinematicsStateComponent(
        new Vector2(
          spawn.Center.X - width * 0.5f,
          spawn.Center.Y - height * 0.5f),
        spawn.Velocity),
    };

    state = projectileState;
    return true;
  }

  private static bool IsRepresentable(
    ProjectileDefinition definition,
    ProjectileSpawnCommand spawn)
  {
    if (definition.Identity is null || definition.Geometry is null ||
      definition.Behavior is null || definition.Combat is null ||
      definition.Penetration is null || definition.Capabilities is null ||
      definition.Presentation is null || definition.Network is null)
    {
      return false;
    }

    ProjectileIdentityDefinition identity = definition.Identity;
    ProjectileGeometryDefinition geometry = definition.Geometry;
    ProjectileBehaviorDefinition behavior = definition.Behavior;
    ProjectileCombatDefinition combat = definition.Combat;
    ProjectilePenetrationDefinition penetration = definition.Penetration;
    ProjectilePresentationDefinition presentation = definition.Presentation;

    if (identity.TypeId <= 0 || identity.TypeId != spawn.ProjectileType ||
      !identity.NeedsUuid.HasValue ||
      geometry.Width < 0 || geometry.Height < 0 ||
      !float.IsFinite(geometry.Scale) || geometry.Scale < 0.0f ||
      !float.IsFinite(geometry.OwnerHitCheckDistance) ||
      geometry.OwnerHitCheckDistance < 0.0f ||
      behavior.ExtraUpdates < 0 || behavior.DefaultTimeLeft < 0 ||
      !float.IsFinite(combat.KnockBack) ||
      !Enum.IsDefined(combat.HostileDamageScaling) ||
      penetration.DefaultPenetrate < -1 || penetration.MaxPenetrate < -1 ||
      penetration.LocalNpcHitCooldown < -2 || penetration.IdStaticNpcHitCooldown < -1 ||
      presentation.FrameCount < 0 || presentation.Alpha < 0 || presentation.Alpha > 255 ||
      presentation.TrailCacheLength < 0 || !float.IsFinite(presentation.Light) ||
      !float.IsFinite(definition.Capabilities.MinionSlots) ||
      definition.Capabilities.MinionSlots < 0.0f)
    {
      return false;
    }

    float scaledWidth = geometry.Width * geometry.Scale;
    float scaledHeight = geometry.Height * geometry.Scale;
    return float.IsFinite(scaledWidth) && float.IsFinite(scaledHeight) &&
      scaledWidth <= int.MaxValue && scaledHeight <= int.MaxValue;
  }
}
