namespace Terraria.Attributes;

/// <summary>
/// 可被来源修饰的通用属性键。
/// </summary>
public enum AttributeKind : byte
{
  Defense,
  Endurance,
  DamageReduction,
  Damage,
  MeleeDamage,
  RangedDamage,
  MagicDamage,
  SummonDamage,
  AttackSpeed,
  MovementSpeed,
  CriticalChance,
  CriticalMultiplier,
  LifeRegeneration,
  ArmorPenetration,
  ElementalPenetration,
  FireResistance,
  IceResistance,
  PoisonResistance,
  LightningResistance,
  HolyResistance,
  ShadowResistance,
  Shield,
  MaxShield,
  ReflectRatio,
  LifeStealRatio,
  Knockback,
  ProjectilePenetrationLimit,
  ProjectileHitImmunityTicks,

  // 以下通道服务于独立管线形态（不经 Buff 列表的持久来源）。
  /// <summary>幸运值。多个来源以 Additive 汇入，语义为单一标量。</summary>
  Luck,
  /// <summary>潜行度，连续量，影响伤害与仇恨。</summary>
  Stealth,
  /// <summary>仇恨系数。</summary>
  Aggro,
  /// <summary>怪物旗帜提供的伤害减免比例。</summary>
  BannerDamageReduction,
  /// <summary>召唤物槽位容量。</summary>
  MinionCapacity,
  /// <summary>鞭子作用范围加成。</summary>
  WhipRange,
  /// <summary>药水冷却剩余 Tick，由冷却子系统以 Override 写入当前值。</summary>
  PotionCooldownTicks,
  /// <summary>恢复药水冷却剩余 Tick。</summary>
  RestorationCooldownTicks,
  /// <summary>蘑菇类冷却剩余 Tick。</summary>
  MushroomCooldownTicks,
  /// <summary>呼吸容量 Tick。</summary>
  BreathCapacityTicks,
  /// <summary>坐骑种类标识，以 Override 写入；-1 表示未骑乘。</summary>
  MountKind,
  /// <summary>布尔状态标记，以 Override 写入 1 或 0。</summary>
  StatusFlag
}
