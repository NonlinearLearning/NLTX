using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Attributes;
using Terraria.Share.Relationships;

namespace Terraria.Share.Combat;

/// <summary>Single read-only rule for "can this source hurt this target right now".</summary>
/// <remarks>
/// 这是无敌语义的唯一实现。伤害裁决（Commit 阶段）与投射物预检（Simulation 阶段）都调它，
/// 所以规则只有一份，两个门禁不会漂移。
/// <para>
/// 它同时吸收了三件此前散落的事：永久免疫标记、状态来源免疫、以及受击窗口。
/// 调用方不需要知道它们的区别，只需要知道「被挡了，原因是 X」。
/// </para>
/// <para>
/// 本方法不修改任何状态。预检用它只为了少产生请求；裁决用它才是权威。
/// </para>
/// </remarks>
public static class DamageImmunityQuery
{
  /// <summary>Returns <see cref="DamageRejectionReason.None"/> when the hit is allowed.</summary>
  /// <param name="damageType">
  /// Calculation family of the hit. <see cref="DamageType.DamageOverTime"/> 不消耗受击窗口，
  /// 但永久免疫与状态免疫仍然挡它。
  /// </param>
  /// <remarks>
  /// 持续伤害豁免受击窗口，是因为参考端灼烧 / 中毒走 Buff 周期结算，前置条件是
  /// <c>hurtCooldowns[Lava] &lt;= 0</c> 而不是 <c>!immune</c>（Player.cs:27887、29206），
  /// 身上有受击无敌完全不影响掉血。
  /// <para>
  /// 这里若也拿 General 窗口挡 DoT，后果不是「少掉几滴血」而是语义反转：被点燃的目标在窗口期内
  /// 免掉一次灼烧，而开窗本身只发生在一次性打击落地后，等于让「刚被打过」短暂等于「火烧不疼」。
  /// </para>
  /// </remarks>
  public static DamageRejectionReason Check(
      ArchWorld world,
      ArchEntity target,
      EntityRuntimeHandle attacker,
      ImmunityChannel channel,
      long tick,
      DamageType damageType = DamageType.Generic)
  {
    if (world.Has<DamageImmunityComponent>(target))
    {
      return DamageRejectionReason.TargetImmune;
    }

    if (world.Has<DamageImmunityStatusComponent>(target) &&
        world.Get<DamageImmunityStatusComponent>(target).IsActive)
    {
      return DamageRejectionReason.TargetImmune;
    }

    if (damageType == DamageType.DamageOverTime)
    {
      return DamageRejectionReason.None;
    }

    if (world.Has<ImmunityWindowComponent>(target) &&
        world.Get<ImmunityWindowComponent>(target).IsActive(channel, tick))
    {
      return DamageRejectionReason.ProtectionWindowActive;
    }

    if (world.Has<AttackerImmunityComponent>(target) &&
        world.Get<AttackerImmunityComponent>(target).IsActive(attacker, tick))
    {
      return DamageRejectionReason.ProtectionWindowActive;
    }

    return DamageRejectionReason.None;
  }
}
