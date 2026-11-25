using System;

namespace Terraria.Share.Attributes;

/// <summary>
/// 参考客户端药水表的 Buff 内容样例，用于填充空的 <see cref="BuffRegistry"/>。
/// </summary>
/// <remarks>
/// 每一项都直接取自参考客户端源码，不做推测：
/// <list type="bullet">
/// <item><c>Terraria/Player.cs:10038</c> 再生：<c>lifeRegen += 4</c>（Buff 2）</item>
/// <item><c>Terraria/Player.cs:10041</c> 迅捷：<c>moveSpeed += 0.25f</c>（Buff 3）</item>
/// <item><c>Terraria/Player.cs:10049</c> 铁皮：<c>statDefense += 8</c>（Buff 5）</item>
/// <item><c>Terraria/Item.cs:4826</c>、<c>:4840</c>、<c>:4868</c> 三者时长均为 28800 tick</item>
/// </list>
/// 客户端把这些效果直接写进 <c>Player</c> 的字段，这里改写为共有的 <see cref="AttributeContribution"/>，
/// 因此同一份内容可以同时被 Buff 与装备等来源复用。
/// </remarks>
public static class BuffContent {
  /// <summary>
  /// 三种药水共有的时长，单位为 tick：28800 / 60 = 480 秒 = 8 分钟。
  /// </summary>
  public const int PotionDurationTicks = 28800;

  /// <summary>铁皮药水：防御 +8。</summary>
  public static Buff Ironskin { get; } = new(
      new BuffEffectId("buff.ironskin"),
      new BuffId("buff.ironskin"),
      PotionDurationTicks,
      contributions:
      [
        new AttributeContribution(AttributeKind.Defense, AttributeModifierOperation.Additive, 8f)
      ]);

  /// <summary>迅捷药水：移动速度 +25%。</summary>
  public static Buff Swiftness { get; } = new(
      new BuffEffectId("buff.swiftness"),
      new BuffId("buff.swiftness"),
      PotionDurationTicks,
      contributions:
      [
        new AttributeContribution(
            AttributeKind.MovementSpeed, AttributeModifierOperation.Additive, 0.25f)
      ]);

  /// <summary>再生药水：生命再生 +4。</summary>
  public static Buff Regeneration { get; } = new(
      new BuffEffectId("buff.regeneration"),
      new BuffId("buff.regeneration"),
      PotionDurationTicks,
      contributions:
      [
        new AttributeContribution(
            AttributeKind.LifeRegeneration, AttributeModifierOperation.Additive, 4f)
      ],
      behaviors:
      [
          new BuffBehaviorContribution(BuffBehaviorKind.PeriodicHeal, periodTicks: 60, magnitude: 4)
      ],
      periodicEffects:
      [
          new BuffPeriodicEffect(BuffBehaviorKind.PeriodicHeal, periodTicks: 60, magnitude: 4)
      ]);

  /// <summary>Poison: periodic self-targeted combat damage request.</summary>
  public static Buff Poison { get; } = new(
      new BuffEffectId("buff.poison"),
      new BuffId("buff.poison"),
      durationTicks: 600,
      behaviors:
      [
          new BuffBehaviorContribution(BuffBehaviorKind.DamageOverTime, periodTicks: 60, magnitude: 4)
      ],
      periodicEffects:
      [
          new BuffPeriodicEffect(BuffBehaviorKind.DamageOverTime, periodTicks: 60, magnitude: 4)
      ]);

  /// <summary>Invisibility: concrete visibility behavior, not a scalar StatusFlag.</summary>
  public static Buff Invisibility { get; } = new(
      new BuffEffectId("buff.invisibility"),
      new BuffId("buff.invisibility"),
      PotionDurationTicks,
      behaviors:
      [
          new BuffBehaviorContribution(BuffBehaviorKind.Invisibility)
      ]);

  /// <summary>
  /// 把全部样例注册到给定注册表。
  /// </summary>
  public static void RegisterAll(BuffRegistry registry) {
    ArgumentNullException.ThrowIfNull(registry);
    registry.Register(Ironskin);
    registry.Register(Swiftness);
    registry.Register(Regeneration);
    registry.Register(Poison);
    registry.Register(Invisibility);
  }
}
