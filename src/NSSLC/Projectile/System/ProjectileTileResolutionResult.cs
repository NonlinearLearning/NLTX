namespace Terraria.Unshared.Projectiles;

/// <summary>一次瓦片接触的处理去向。</summary>
public enum ProjectileTileResolutionOutcome : byte
{
  /// <summary>已按策略反射，弹体继续飞行。</summary>
  Bounced = 0,

  /// <summary>反弹完成同时预算耗尽，已入队终止。</summary>
  BouncedAndExhausted = 1,

  /// <summary>不可反弹或预算不足，已入队终止。</summary>
  Terminated = 2
}

/// <summary>一次瓦片接触的处理结果，供诊断与验证读取。</summary>
/// <remarks>
/// 结果里带反射后的速度，是为了让验证能直接断言"反弹方向对不对"，
/// 而不必再去世界里翻 <see cref="Terraria.Share.Movement.VelocityComponent" />。
/// </remarks>
public readonly record struct ProjectileTileResolutionResult(
    Terraria.Share.Relationships.EntityRuntimeHandle Projectile,
    ProjectileTileResolutionOutcome Outcome,
    System.Numerics.Vector2 ReflectedVelocity);
