using System.Text.Json.Serialization;

namespace NSSLC.Server.Content.Projectile;

/// <summary>
/// Raw JSON shape of one projectile content entry.
/// </summary>
/// <remarks>
/// This is a parse target, not a domain value: every member is mutable and nullable so that
/// missing fields surface as diagnostics instead of deserialization exceptions. Validation and
/// conversion happen in <see cref="ProjectileCodec"/>. Nested shapes are part of the same
/// document and carry no behaviour.
/// </remarks>
internal sealed class ProjectilePrototype
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("type")]
  public string Type { get; set; } = string.Empty;

  [JsonPropertyName("behaviorId")]
  public string BehaviorId { get; set; } = string.Empty;

  [JsonPropertyName("profileId")]
  public string ProfileId { get; set; } = string.Empty;

  [JsonPropertyName("extraUpdates")]
  public int ExtraUpdates { get; set; }

  [JsonPropertyName("lifetimeTicks")]
  public int LifetimeTicks { get; set; }

  [JsonPropertyName("collider")]
  public ProjectileColliderPrototype? Collider { get; set; }

  /// <summary>
  /// 出厂速度：生成时写入的初始运动，之后由 AI 行为接管。
  /// </summary>
  /// <remarks>
  /// 速度属于内容而不是发射意图——发射方只说"在哪、射什么"。
  /// 省略时为零速，适用于生成后完全由 AI 决定运动的弹体。
  /// </remarks>
  [JsonPropertyName("initialVelocity")]
  public ProjectileInitialVelocityPrototype? InitialVelocity { get; set; }

  [JsonPropertyName("tileCollide")]
  public bool TileCollide { get; set; } = true;

  [JsonPropertyName("ignoreOwner")]
  public bool IgnoreOwner { get; set; } = true;

  [JsonPropertyName("gravity")]
  public ProjectileGravityPrototype? Gravity { get; set; }

  [JsonPropertyName("liquid")]
  public ProjectileLiquidPrototype? Liquid { get; set; }

  [JsonPropertyName("penetration")]
  public ProjectilePenetrationPrototype? Penetration { get; set; }

  [JsonPropertyName("bounce")]
  public ProjectileBouncePrototype? Bounce { get; set; }

  [JsonPropertyName("homing")]
  public ProjectileHomingPrototype? Homing { get; set; }

  [JsonPropertyName("attackId")]
  public string AttackId { get; set; } = string.Empty;

  [JsonPropertyName("damageType")]
  public string DamageType { get; set; } = string.Empty;

  [JsonPropertyName("baseDamage")]
  public int BaseDamage { get; set; }
}

/// <summary>Collision bounds of one projectile entry, in world units.</summary>
internal sealed class ProjectileColliderPrototype
{
  [JsonPropertyName("width")]
  public float Width { get; set; }

  [JsonPropertyName("height")]
  public float Height { get; set; }
}

/// <summary>出厂速度 of one projectile entry, in world units per tick.</summary>
internal sealed class ProjectileInitialVelocityPrototype
{
  [JsonPropertyName("x")]
  public float X { get; set; }

  [JsonPropertyName("y")]
  public float Y { get; set; }
}

/// <summary>Gravity parameters of one projectile entry.</summary>
internal sealed class ProjectileGravityPrototype
{
  [JsonPropertyName("enabled")]
  public bool Enabled { get; set; }

  [JsonPropertyName("strength")]
  public float Strength { get; set; }

  [JsonPropertyName("maximumFallSpeed")]
  public float MaximumFallSpeed { get; set; }
}

/// <summary>Liquid interaction parameters of one projectile entry.</summary>
internal sealed class ProjectileLiquidPrototype
{
  [JsonPropertyName("enabled")]
  public bool Enabled { get; set; } = true;

  [JsonPropertyName("water")]
  public bool Water { get; set; } = true;

  [JsonPropertyName("honey")]
  public bool Honey { get; set; } = true;

  [JsonPropertyName("lava")]
  public bool Lava { get; set; } = true;

  [JsonPropertyName("shimmer")]
  public bool Shimmer { get; set; } = true;
}

/// <summary>Penetration budget of one projectile entry.</summary>
internal sealed class ProjectilePenetrationPrototype
{
  [JsonPropertyName("maximumHits")]
  public int MaximumHits { get; set; } = 1;

  /// <summary>
  /// 打空后是否停止造成伤害；默认真，对齐参考端"穿完即哑火"。
  /// </summary>
  [JsonPropertyName("suppressesDamage")]
  public bool SuppressesDamage { get; set; } = true;
}

/// <summary>Bounce parameters of one projectile entry.</summary>
internal sealed class ProjectileBouncePrototype
{
  [JsonPropertyName("maximumBounces")]
  public int MaximumBounces { get; set; }

  [JsonPropertyName("penetrationCost")]
  public int PenetrationCost { get; set; }

  [JsonPropertyName("speedScale")]
  public float SpeedScale { get; set; } = 1f;

  [JsonPropertyName("consumeOrder")]
  public string ConsumeOrder { get; set; } = string.Empty;
}

/// <summary>Homing parameters of one projectile entry.</summary>
internal sealed class ProjectileHomingPrototype
{
  [JsonPropertyName("turnRate")]
  public float TurnRate { get; set; }

  [JsonPropertyName("range")]
  public float Range { get; set; }
}
