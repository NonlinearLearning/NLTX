using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;

namespace Terraria.Share.Combat;

/// <summary>Opens immunity windows for one landed damage request.</summary>
/// <remarks>
/// 开窗点必须紧跟「已确认落地」之后，且必须在同一 tick 的请求循环内完成：
/// 参考端同一 tick 内第二发命中是会被 immune 挡掉的，若把开窗推迟到 tick 末的另一次遍历，
/// 同 tick 的多发命中会全部落地，等于放开了一个语义口子。
/// <para>
/// 因此这里是一个策略类而不是一个独立系统；协调器只负责在正确的位置调用它，不持有任何时长规则。
/// 计划原文的 ImmunityGrantSystem 方案在实施时被否，理由见计划 §5 的偏差记录。
/// </para>
/// </remarks>
public static class ImmunityGrantPolicy
{
  /// <summary>Grants every immunity carrier on the target. Returns the granted tick count.</summary>
  public static int Grant(
      ArchWorld world,
      ArchEntity target,
      in DamageRequest request,
      int appliedDamage,
      bool longInvince = false)
  {
    // 持续伤害不开窗：参考端的灼烧 / 中毒走 Buff 周期结算，从不写 immune 与 hurtCooldowns，
    // 若这里也给 DoT 开窗，一个 5 tick 一跳的灼烧会在每跳之后把目标锁成不可受击，
    // 等于让「被点燃」变成「免伤」。
    if (request.Payload.Type == DamageType.DamageOverTime)
    {
      return 0;
    }

    int duration = ImmunityDurationPolicy.ForHit(
        appliedDamage,
        request.Payload.AllowFriendlyFire,
        longInvince);
    if (duration <= 0)
    {
      return 0;
    }

    if (world.Has<ImmunityWindowComponent>(target))
    {
      world.Get<ImmunityWindowComponent>(target).Grant(request.Channel, request.Tick, duration);
    }

    if (world.Has<AttackerImmunityComponent>(target))
    {
      world.Get<AttackerImmunityComponent>(target).Grant(request.Source, request.Tick, duration);
    }

    return duration;
  }
}
