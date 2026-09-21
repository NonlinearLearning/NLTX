using Terraria.Dome.Simulation.Projectile.Definitions;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileDefinitionComponent(
  int ProjectileType,
  int BehaviorId,
  int DefaultDamage,
  int DefaultLifetimeTicks,
  ColliderComponent Collider,
  bool Friendly,
  bool Hostile,
  int MaximumBounces = 0,
  ProjectileLiquidPolicy LiquidPolicy = ProjectileLiquidPolicy.Pass,
  PlayerDamagePolicy PlayerDamagePolicy = PlayerDamagePolicy.None,
  float Knockback = 0.0f,
  int OriginalDamage = 0,
  bool CollidesWithTiles = true,
  ProjectileOnDespawnAreaDamage OnDespawnAreaDamage = default,
  ProjectileOnDespawnStatusEffect OnDespawnStatusEffect = default,
  ProjectileOnHitStatusEffect OnHitStatusEffect = default,
  bool ReflectsFromTiles = false,
  ProjectileChildSpawn ChildSpawn = default,
  float BounceVelocityMultiplier = 1.0f,
  float MinimumBounceSpeed = 0.0f,
  int ExtraUpdates = 0,
  bool IgnoreWater = false,
  bool CorrectSlopeCollision = true,
  bool UsesStaticNpcImmunity = false,
  int StaticNpcHitCooldownTicks = -1,
  bool AppliesImmunityTimeOnSingleHits = false,
  ProjectileDamageClass DamageClass = ProjectileDamageClass.Generic,
  int LegacyAiStyle = 0,
  bool IsColdDamage = false,
  bool IsArrow = false,
  int ArmorPenetration = 0,
  int BonusCritChance = 0,
  int BonusTagDamage = 0,
  int TagEffectType = 0,
  bool StopsDealingDamageAfterPenetrateHits = false,
  bool NoEnchantments = false,
  bool NoEnchantmentVisuals = false,
  bool OriginatedFromActivableTile = false,
  bool NoDropItem = false,
  bool UsesLocalNpcImmunity = false,
  int LocalNpcHitCooldownTicks = -2,
  bool IsNetworkImportant = false,
  float Scale = 1.0f,
  bool OwnerHitCheck = false,
  float OwnerHitCheckDistance = 1000.0f,
  bool IsSentry = false,
  bool IsMinion = false,
  float MinionSlots = 0.0f,
  int MinionPosition = 0,
  bool IsTrap = false,
  bool IsBobber = false,
  bool IsCounterweight = false,
  ProjectileHostileDamageScaling HostileDamageScaling = ProjectileHostileDamageScaling.Default,
  bool DecidesManualFallThrough = false,
  bool ManualDirectionChange = false,
  bool UsesOwnerMeleeHitCooldown = false,
  bool CopiesOwnerAttackCooldownToLocalImmunityOnSpawn = false)
{
  public int Type => ProjectileType;

  public int AiStyle => LegacyAiStyle;

  public int Damage => DefaultDamage;

  public ProjectileDamageClass CombatDamageClass => DamageClass;

  public bool IsMeleeDamage => DamageClass == ProjectileDamageClass.Melee;

  public bool IsRangedDamage => DamageClass == ProjectileDamageClass.Ranged;

  public bool IsMagicDamage => DamageClass == ProjectileDamageClass.Magic;

  public ProjectileHostileDamageScaling HostileDamageRule => HostileDamageScaling;

  public int TagEffect => TagEffectType;

  public bool OriginatedFromTileActivation => OriginatedFromActivableTile;

  public bool CopiesOwnerCooldownToLocalImmunity => CopiesOwnerAttackCooldownToLocalImmunityOnSpawn;

  public bool HasOnHitStatusEffect => OnHitStatusEffect.IsEnabled;

  public bool HasOnDespawnStatusEffect => OnDespawnStatusEffect.IsEnabled;

  public bool HasOnDespawnAreaDamage => OnDespawnAreaDamage.IsEnabled;

  public bool SpawnsChildProjectiles => ChildSpawn.IsEnabled;

  public int LifetimeTicks => DefaultLifetimeTicks;

  public bool TileCollide => CollidesWithTiles;

  public bool HostileProjectile => Hostile;

  public bool FriendlyProjectile => Friendly;

  public int MaximumBounceCount => MaximumBounces;

  public ProjectileLiquidPolicy LiquidCollisionPolicy => LiquidPolicy;

  public PlayerDamagePolicy PlayerDamageRule => PlayerDamagePolicy;

  public float KnockBack => Knockback;

  public int OriginalDamageValue => OriginalDamage;

  public bool ReflectsTiles => ReflectsFromTiles;

  public float BounceSpeedMultiplier => BounceVelocityMultiplier;

  public float MinimumBounceVelocity => MinimumBounceSpeed;

  public int UpdateCountPerTick => MaxUpdates;

  public int ExtraUpdateCount => ExtraUpdates;

  public bool WaterCollisionIgnored => IgnoreWater;

  public bool SlopeCollisionCorrected => CorrectSlopeCollision;

  public bool UsesStaticNpcHitImmunity => UsesStaticNpcImmunity;

  public int StaticNpcImmunityCooldown => StaticNpcHitCooldownTicks;

  public bool UsesIdStaticNpcHitImmunity => UsesStaticNpcImmunity;

  public int IdStaticNpcHitCooldown => StaticNpcHitCooldownTicks;

  public bool AppliesSingleHitImmunity => AppliesImmunityTimeOnSingleHits;

  public bool ColdDamage => IsColdDamage;

  public bool Arrow => IsArrow;

  public int ArmorPenetrationValue => ArmorPenetration;

  public int BonusCriticalChance => BonusCritChance;

  public int BonusTagDamageValue => BonusTagDamage;

  public bool StopsAfterPenetration => StopsDealingDamageAfterPenetrateHits;

  public bool EnchantmentsDisabled => NoEnchantments;

  public bool EnchantmentVisualsDisabled => NoEnchantmentVisuals;

  public bool NoDroppedItem => NoDropItem;

  public bool UsesLocalNpcHitImmunity => UsesLocalNpcImmunity;

  public int LocalNpcImmunityCooldown => LocalNpcHitCooldownTicks;

  public float ProjectileScale => Scale;

  public bool RequiresOwnerHitCheck => OwnerHitCheck;

  public float OwnerHitCheckRange => OwnerHitCheckDistance;

  public bool Sentry => IsSentry;

  public bool Minion => IsMinion;

  public float MinionSlotCount => MinionSlots;

  public bool Trap => IsTrap;

  public bool Bobber => IsBobber;

  public bool Counterweight => IsCounterweight;

  public bool ManualFallThrough => DecidesManualFallThrough;

  public bool ManualDirection => ManualDirectionChange;

  public bool UsesOwnerMeleeCooldown => UsesOwnerMeleeHitCooldown;

  public int MaxUpdates => checked(ExtraUpdates + 1);
}
