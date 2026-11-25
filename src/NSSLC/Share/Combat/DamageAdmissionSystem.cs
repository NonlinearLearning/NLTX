using System;

namespace Terraria.Share.Combat;

/// <summary>
/// Validates a shared damage request without changing target or source state.
/// </summary>
/// <remarks>
/// 这里只判「请求本身合法吗」，不判「目标现在能不能挨打」。
/// 后者是 <see cref="DamageImmunityQuery" /> 的职责：它是唯一实现，且需要世界状态访问。
/// 两个职责分开，是因为前者是纯函数、后者是查询，混在一起会让准入类被迫持有世界引用。
/// </remarks>
public sealed class DamageAdmissionSystem
{
  public DamageRejectionReason Check(
      in DamageRequest request,
      in LifeComponent currentTargetLife)
  {
    if (!request.Source.IsValid || !request.Target.IsValid || !request.Payload.IsValid ||
        request.Tick < 0 || request.RequestId < 0 || !Enum.IsDefined(request.Cause) ||
        request.ExpectedTargetLife.Maximum <= 0)
    {
      return DamageRejectionReason.InvalidRequest;
    }

    if (!currentTargetLife.Equals(request.ExpectedTargetLife))
    {
      return DamageRejectionReason.TargetChanged;
    }

    if (!currentTargetLife.IsAlive)
    {
      return DamageRejectionReason.TargetAlreadyDepleted;
    }

    if (request.Payload.BaseDamage <= 0)
    {
      return DamageRejectionReason.NonPositiveDamage;
    }

    return DamageRejectionReason.None;
  }
}
