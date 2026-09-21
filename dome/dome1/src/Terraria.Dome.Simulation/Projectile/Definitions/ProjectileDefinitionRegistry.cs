using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Projectile.Behaviors;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Projectile.Definitions;

public sealed class ProjectileDefinitionRegistry
{
  private readonly IReadOnlyDictionary<int, ProjectileDefinition> _definitions;
  private readonly IReadOnlyList<ProjectileDefinition> _orderedDefinitions;

  public ProjectileDefinitionRegistry(IEnumerable<ProjectileDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    List<ProjectileDefinition> materialized = new(definitions);
    for (int index = 0; index < materialized.Count; index++)
    {
      ProjectileDefinition definition = materialized[index];
      if (LegacyProjectileAttackCooldownCopyRegistry.IsEnabled(definition.ProjectileType))
      {
        materialized[index] = definition with
        {
          CopiesOwnerAttackCooldownToLocalImmunityOnSpawn = true
        };
      }

      if (LegacyProjectileHostileDamageScalingRegistry.UsesLightningScaling(
        definition.ProjectileType))
      {
        materialized[index] = materialized[index] with
        {
          HostileDamageScaling = ProjectileHostileDamageScaling.Lightning
        };
      }
    }

    Dictionary<int, ProjectileDefinition> indexed = new();
    foreach (ProjectileDefinition definition in materialized)
    {
      if (definition.ProjectileType <= 0 || definition.BehaviorId <= 0 ||
          definition.Damage < 0 || definition.LifetimeTicks <= 0 ||
          !float.IsFinite(definition.Collider.Width) || definition.Collider.Width <= 0.0f ||
          !float.IsFinite(definition.Collider.Height) || definition.Collider.Height <= 0.0f ||
          definition.MaximumPenetration == 0 || definition.MaximumPenetration < -1 ||
          definition.MaximumBounces < 0 ||
          definition.BonusCritChance < 0 || definition.BonusCritChance > 100 ||
          definition.BonusTagDamage < 0 ||
          definition.TagEffectType < 0 ||
          definition.LocalNpcHitCooldownTicks < -2 ||
          !float.IsFinite(definition.MinionSlots) || definition.MinionSlots < 0.0f ||
          definition.MinionPosition < 0 ||
          !float.IsFinite(definition.Scale) || definition.Scale <= 0.0f ||
          !float.IsFinite(definition.OwnerHitCheckDistance) ||
          definition.OwnerHitCheckDistance <= 0.0f ||
          definition.ExtraUpdates < 0 || definition.ExtraUpdates > 180 ||
          !Enum.IsDefined(definition.LiquidPolicy) ||
          !Enum.IsDefined(definition.DamageClass) ||
          !Enum.IsDefined(definition.PlayerDamagePolicy) ||
          !Enum.IsDefined(definition.HostileDamageScaling) ||
          !float.IsFinite(definition.OnDespawnAreaDamage.Width) ||
          !float.IsFinite(definition.OnDespawnAreaDamage.Height) ||
          definition.OnDespawnAreaDamage.Width < 0.0f ||
          definition.OnDespawnAreaDamage.Height < 0.0f ||
          (definition.OnDespawnAreaDamage.Width == 0.0f) !=
            (definition.OnDespawnAreaDamage.Height == 0.0f) ||
          !float.IsFinite(definition.BounceVelocityMultiplier) ||
          definition.BounceVelocityMultiplier <= 0.0f ||
          !float.IsFinite(definition.MinimumBounceSpeed) ||
          definition.MinimumBounceSpeed < 0.0f ||
          definition.StaticNpcHitCooldownTicks < -1 ||
          definition.UsesStaticNpcImmunity && definition.StaticNpcHitCooldownTicks <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(definitions));
      }

      definition.ChildSpawn.Validate();
      definition.OnDespawnStatusEffect.Validate();
      if (definition.OnHitStatusEffect.IsEnabled)
      {
        definition.OnHitStatusEffect.Validate();
      }

      if (!indexed.TryAdd(definition.ProjectileType, definition))
      {
        throw new ArgumentException("Projectile definitions must have unique types.", nameof(definitions));
      }
    }

    foreach (ProjectileDefinition definition in materialized)
    {
      if (definition.ChildSpawn.IsEnabled &&
          !indexed.ContainsKey(definition.ChildSpawn.ProjectileType))
      {
        throw new ArgumentException(
          "Projectile child-spawn definitions must reference a registered " +
          "projectile type.",
          nameof(definitions));
      }
    }

    _definitions = new ReadOnlyDictionary<int, ProjectileDefinition>(indexed);
    _orderedDefinitions = materialized.AsReadOnly();
  }

  public static ProjectileDefinitionRegistry CreateDefault()
  {
    return new ProjectileDefinitionRegistry([
      new ProjectileDefinition(1, 1, 10, 1200, new ColliderComponent(0.5f, 0.5f), true, false, 1,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 1,
        IsArrow: true),
      new ProjectileDefinition(2, 2, 10, 1200, new ColliderComponent(0.5f, 0.5f), true, false, 1,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 1,
        IsArrow: true),
      new ProjectileDefinition(
        3,
        3,
        0,
        3600,
        new ColliderComponent(1.375f, 1.375f),
        true,
        false,
        4,
        LegacyAiStyle: 2,
        DamageClass: ProjectileDamageClass.Ranged,
        IsColdDamage: false),
      new ProjectileDefinition(4, 1, 10, 1200, new ColliderComponent(0.5f, 0.5f), true, false, 5, 1,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 1,
        IsArrow: true),
      new ProjectileDefinition(
        5,
        1,
        10,
        120,
        new ColliderComponent(0.5f, 0.5f),
        true,
        false,
        1,
        LiquidPolicy: ProjectileLiquidPolicy.Destroy,
        ExtraUpdates: 1,
        IgnoreWater: true,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 1,
        IsArrow: true),
      new ProjectileDefinition(
        6,
        1,
        10,
        30,
        new ColliderComponent(0.5f, 0.5f),
        true,
        false,
        -1,
        LegacyAiStyle: 3,
        DamageClass: ProjectileDamageClass.Melee),
      new ProjectileDefinition(
        69,
        4,
        0,
        3600,
        new ColliderComponent(0.875f, 0.875f),
        true,
        false,
        1,
        IgnoreWater: true,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        70,
        4,
        0,
        3600,
        new ColliderComponent(0.875f, 0.875f),
        true,
        false,
        1,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        621,
        4,
        0,
        3600,
        new ColliderComponent(0.875f, 0.875f),
        true,
        false,
        1,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        249,
        5,
        0,
        3600,
        new ColliderComponent(0.75f, 0.75f),
        true,
        false,
        1,
        LegacyAiStyle: 2,
        DamageClass: ProjectileDamageClass.Ranged),
      new ProjectileDefinition(
        347,
        6,
        0,
        3600,
        new ColliderComponent(0.375f, 0.375f),
        false,
        true,
        -1,
        PlayerDamagePolicy: PlayerDamagePolicy.HostileNonPvp,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        300,
        7,
        0,
        3600,
        new ColliderComponent(2.375f, 2.375f),
        false,
        true,
        -1,
        PlayerDamagePolicy: PlayerDamagePolicy.HostileNonPvp,
        CollidesWithTiles: false,
        IgnoreWater: true,
        DamageClass: ProjectileDamageClass.Magic,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        48,
        3,
        0,
        3600,
        new ColliderComponent(0.75f, 0.75f),
        true,
        false,
        2,
        LegacyAiStyle: 2,
        DamageClass: ProjectileDamageClass.Ranged),
      new ProjectileDefinition(
        599,
        3,
        0,
        3600,
        new ColliderComponent(1.375f, 1.375f),
        true,
        false,
        6,
        LegacyAiStyle: 2,
        DamageClass: ProjectileDamageClass.Ranged),
      new ProjectileDefinition(
        909,
        8,
        0,
        3600,
        new ColliderComponent(0.75f, 0.75f),
        false,
        true,
        1,
        PlayerDamagePolicy: PlayerDamagePolicy.HostileNonPvp,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        520,
        3,
        0,
        3600,
        new ColliderComponent(1.375f, 1.375f),
        true,
        false,
        3,
        LegacyAiStyle: 2,
        DamageClass: ProjectileDamageClass.Ranged,
        IsColdDamage: true),
      new ProjectileDefinition(
        471,
        LegacyAiStyle2ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.0f, 1.0f),
        false,
        true,
        1,
        PlayerDamagePolicy: PlayerDamagePolicy.HostileNonPvp,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        501,
        9,
        0,
        3600,
        new ColliderComponent(0.875f, 0.875f),
        false,
        true,
        1,
        PlayerDamagePolicy: PlayerDamagePolicy.HostileNonPvp,
        OnDespawnAreaDamage: new ProjectileOnDespawnAreaDamage(8.375f, 8.375f),
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        504,
        18,
        0,
        3600,
        new ColliderComponent(0.625f, 0.625f),
        true,
        false,
        2,
        OnHitStatusEffect: new ProjectileOnHitStatusEffect(323, 60, 239),
        DamageClass: ProjectileDamageClass.Melee,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        240,
        10,
        0,
        3600,
        new ColliderComponent(1.0f, 1.0f),
        false,
        true,
        -1,
        PlayerDamagePolicy: PlayerDamagePolicy.HostileNonPvp,
        OnDespawnAreaDamage: new ProjectileOnDespawnAreaDamage(6.0f, 6.0f),
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        521,
        LegacyAiStyle29ParentProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(0.875f, 0.875f),
        true,
        false,
        1,
        ChildSpawn: new ProjectileChildSpawn(522, 3, 5, 7.0f, 10.0f, 0.8f, 0.8f),
        ExtraUpdates: 1,
        DamageClass: ProjectileDamageClass.Magic,
        LegacyAiStyle: 29),
      new ProjectileDefinition(
        522,
        LegacyAiStyle29ChildProjectileBehavior.Id,
        0,
        41,
        new ColliderComponent(0.5f, 0.5f),
        true,
        false,
        1,
        ReflectsFromTiles: true,
        DamageClass: ProjectileDamageClass.Magic,
        LegacyAiStyle: 29),
      new ProjectileDefinition(
        162,
        LegacyAiStyle2Type162ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.0f, 1.0f),
        true,
        false,
        4,
        OnDespawnAreaDamage: new ProjectileOnDespawnAreaDamage(4.0f, 4.0f),
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        281,
        LegacyAiStyle49Type281ProjectileBehavior.Id,
        0,
        600,
        new ColliderComponent(1.25f, 1.25f),
        true,
        false,
        -1,
        ReflectsFromTiles: true,
        BounceVelocityMultiplier: 0.5f,
        MinimumBounceSpeed: 2.0f,
        UsesStaticNpcImmunity: true,
        StaticNpcHitCooldownTicks: 10,
        LegacyAiStyle: 49),
      new ProjectileDefinition(
        21,
        LegacyAiStyle2ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.0f, 1.0f),
        true,
        false,
        1,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        330,
        LegacyAiStyle2ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.375f, 1.375f),
        true,
        false,
        6,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        589,
        LegacyAiStyle2ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(0.625f, 0.625f),
        true,
        false,
        1,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        1012,
        LegacyAiStyle2ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.125f, 1.125f),
        true,
        false,
        1,
        DamageClass: ProjectileDamageClass.Melee,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        304,
        LegacyAiStyle2Type304ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.875f, 1.875f),
        true,
        false,
        1,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        357,
        1,
        0,
        600,
        new ColliderComponent(0.25f, 0.25f),
        true,
        false,
        6,
        ExtraUpdates: 2,
        Scale: 1.2f,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 1),
      new ProjectileDefinition(
        451,
        1,
        0,
        3600,
        new ColliderComponent(1.0f, 1.0f),
        true,
        false,
        3,
        DamageClass: ProjectileDamageClass.Melee,
        LegacyAiStyle: 81),
      new ProjectileDefinition(
        645,
        1,
        0,
        3600,
        new ColliderComponent(0.625f, 0.625f),
        true,
        false,
        -1,
        ExtraUpdates: 5,
        DamageClass: ProjectileDamageClass.Magic,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: -1,
        CollidesWithTiles: false,
        LegacyAiStyle: 1),
      new ProjectileDefinition(
        656,
        1,
        0,
        1200,
        new ColliderComponent(0.625f, 0.625f),
        true,
        false,
        -1,
        CollidesWithTiles: false,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: 8,
        DamageClass: ProjectileDamageClass.Magic,
        LegacyAiStyle: 127),
      new ProjectileDefinition(
        657,
        1,
        0,
        1200,
        new ColliderComponent(0.625f, 0.625f),
        false,
        true,
        -1,
        CollidesWithTiles: false,
        LegacyAiStyle: 127),
      new ProjectileDefinition(
        658,
        1,
        0,
        900,
        new ColliderComponent(0.875f, 0.875f),
        false,
        true,
        1,
        CollidesWithTiles: false,
        IgnoreWater: true,
        LegacyAiStyle: 128),
      new ProjectileDefinition(
        876,
        1,
        0,
        3600,
        new ColliderComponent(0.25f, 0.25f),
        true,
        false,
        8,
        ExtraUpdates: 3,
        Scale: 1.4f,
        DamageClass: ProjectileDamageClass.Magic,
        LegacyAiStyle: 1),
      new ProjectileDefinition(
        864,
        1,
        0,
        60,
        new ColliderComponent(0.625f, 0.625f),
        true,
        false,
        -1,
        IgnoreWater: true,
        ArmorPenetration: 25,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: 10,
        IsNetworkImportant: true,
        IsMinion: true,
        MinionSlots: 1.0f,
        CollidesWithTiles: false,
        LegacyAiStyle: 169),
      new ProjectileDefinition(
        866,
        1,
        0,
        3600,
        new ColliderComponent(1.875f, 1.875f),
        true,
        false,
        5,
        ExtraUpdates: 1,
        DamageClass: ProjectileDamageClass.Melee,
        LegacyAiStyle: 3,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: -1),
      new ProjectileDefinition(
        166,
        LegacyAiStyle2Type166ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(0.875f, 0.875f),
        true,
        false,
        1,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        370,
        LegacyAiStyle2StatusEffectProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(0.875f, 0.875f),
        true,
        false,
        1,
        OnDespawnStatusEffect: new ProjectileOnDespawnStatusEffect(119, 1800, 5.0f),
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        371,
        LegacyAiStyle2StatusEffectProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(0.875f, 0.875f),
        true,
        false,
        1,
        OnDespawnStatusEffect: new ProjectileOnDespawnStatusEffect(120, 1800, 5.0f),
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        936,
        LegacyAiStyle2StatusEffectProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(0.875f, 0.875f),
        true,
        false,
        1,
        OnDespawnStatusEffect: new ProjectileOnDespawnStatusEffect(320, 1800, 5.0f),
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        43,
        LegacyAiStyle17ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.5f, 1.5f),
        false,
        false,
        -1,
        Knockback: 12.0f,
        LegacyAiStyle: 17),
      new ProjectileDefinition(
        607,
        LegacyType607ProjectileBehavior.Id,
        0,
        600,
        new ColliderComponent(0.625f, 0.625f),
        true,
        false,
        -1,
        CollidesWithTiles: false,
        IgnoreWater: true,
        LegacyAiStyle: 116),
      new ProjectileDefinition(
        972,
        LegacyAiStyle190ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.0f, 1.0f),
        true,
        false,
        2,
        CollidesWithTiles: false,
        IgnoreWater: true,
        DamageClass: ProjectileDamageClass.Melee,
        LegacyAiStyle: 190,
        StopsDealingDamageAfterPenetrateHits: true,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: -1,
        OwnerHitCheck: true,
        OwnerHitCheckDistance: 300.0f,
        UsesOwnerMeleeHitCooldown: true),
      new ProjectileDefinition(
        982,
        LegacyAiStyle190ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.0f, 1.0f),
        true,
        false,
        3,
        CollidesWithTiles: false,
        IgnoreWater: true,
        DamageClass: ProjectileDamageClass.Melee,
        LegacyAiStyle: 190,
        StopsDealingDamageAfterPenetrateHits: true,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: -1,
        OwnerHitCheck: true,
        OwnerHitCheckDistance: 300.0f,
        UsesOwnerMeleeHitCooldown: true),
      new ProjectileDefinition(
        983,
        LegacyAiStyle190ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.0f, 1.0f),
        true,
        false,
        6,
        CollidesWithTiles: false,
        IgnoreWater: true,
        DamageClass: ProjectileDamageClass.Melee,
        LegacyAiStyle: 190,
        StopsDealingDamageAfterPenetrateHits: true,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: -1,
        OwnerHitCheck: true,
        OwnerHitCheckDistance: 300.0f,
        UsesOwnerMeleeHitCooldown: true),
      new ProjectileDefinition(
        984,
        LegacyAiStyle190ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.0f, 1.0f),
        true,
        false,
        3,
        CollidesWithTiles: false,
        IgnoreWater: true,
        DamageClass: ProjectileDamageClass.Melee,
        LegacyAiStyle: 190,
        StopsDealingDamageAfterPenetrateHits: true,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: -1,
        OwnerHitCheck: true,
        OwnerHitCheckDistance: 300.0f,
        UsesOwnerMeleeHitCooldown: true),
      new ProjectileDefinition(
        997,
        LegacyAiStyle190ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.0f, 1.0f),
        true,
        false,
        3,
        CollidesWithTiles: false,
        IgnoreWater: true,
        DamageClass: ProjectileDamageClass.Melee,
        LegacyAiStyle: 190,
        StopsDealingDamageAfterPenetrateHits: true,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: -1,
        OwnerHitCheck: true,
        OwnerHitCheckDistance: 300.0f,
        UsesOwnerMeleeHitCooldown: true),
      new ProjectileDefinition(
        1043,
        LegacyAiStyle190ProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(1.0f, 1.0f),
        true,
        false,
        2,
        CollidesWithTiles: false,
        IgnoreWater: true,
        DamageClass: ProjectileDamageClass.Melee,
        LegacyAiStyle: 190,
        StopsDealingDamageAfterPenetrateHits: true,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: -1,
        OwnerHitCheck: true,
        OwnerHitCheckDistance: 300.0f,
        UsesOwnerMeleeHitCooldown: true),
      new ProjectileDefinition(
        954,
        LegacyAiStyle2HitStatusProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(0.625f, 0.625f),
        true,
        false,
        2,
        OnHitStatusEffect: new ProjectileOnHitStatusEffect(24, 60, 239),
        DamageClass: ProjectileDamageClass.Magic,
        LegacyAiStyle: 2),
      new ProjectileDefinition(
        979,
        LegacyAiStyle2HitStatusProjectileBehavior.Id,
        0,
        3600,
        new ColliderComponent(0.625f, 0.625f),
        true,
        false,
        2,
        OnHitStatusEffect: new ProjectileOnHitStatusEffect(44, 60, 239),
        DamageClass: ProjectileDamageClass.Magic,
        LegacyAiStyle: 2,
        IsColdDamage: true),
      new ProjectileDefinition(
        638,
        1,
        0,
        600,
        new ColliderComponent(0.25f, 0.25f),
        true,
        false,
        -1,
        ExtraUpdates: 5,
        IgnoreWater: true,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 1,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: -1),
      new ProjectileDefinition(
        639,
        1,
        0,
        90,
        new ColliderComponent(0.625f, 0.625f),
        true,
        false,
        4,
        ExtraUpdates: 1,
        IgnoreWater: true,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 1,
        IsArrow: true,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: -1),
      new ProjectileDefinition(
        640,
        1,
        0,
        90,
        new ColliderComponent(0.625f, 0.625f),
        true,
        false,
        4,
        ExtraUpdates: 2,
        IgnoreWater: true,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 1,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: -1),
      new ProjectileDefinition(
        706,
        1,
        0,
        300,
        new ColliderComponent(4.125f, 4.125f),
        true,
        false,
        -1,
        DamageClass: ProjectileDamageClass.Ranged,
        LegacyAiStyle: 1,
        UsesLocalNpcImmunity: true,
        LocalNpcHitCooldownTicks: 10),
      new ProjectileDefinition(
        1001,
        1,
        0,
        3600,
        new ColliderComponent(0.5f, 0.5f),
        true,
        false,
        -1,
        CollidesWithTiles: false,
        IgnoreWater: true,
        LegacyAiStyle: 1,
        IsBobber: true),
      new ProjectileDefinition(
        663,
        1,
        0,
        3600,
        new ColliderComponent(1.0f, 1.0f),
        true,
        false,
        1,
        LegacyAiStyle: 1,
        IsSentry: true)]);
  }

  public IReadOnlyDictionary<int, ProjectileDefinition> Definitions => _definitions;

  public IReadOnlyList<ProjectileDefinition> OrderedDefinitions => _orderedDefinitions;

  public bool TryGet(int projectileType, out ProjectileDefinition definition)
  {
    return _definitions.TryGetValue(projectileType, out definition);
  }
}
