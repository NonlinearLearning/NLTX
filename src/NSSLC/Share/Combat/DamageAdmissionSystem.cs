using System;

namespace Terraria.Combat;

/// <summary>
/// Validates a shared damage request without changing target or source state.
/// </summary>
public sealed class DamageAdmissionSystem
{
  public DamageRejectionReason Check(
      in DamageRequest request,
      in LifeComponent currentTargetLife,
      ProtectionWindowComponent? protection = null)
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

    return protection is { IsActive: true }
        ? DamageRejectionReason.ProtectionWindowActive
        : DamageRejectionReason.None;
  }
}
