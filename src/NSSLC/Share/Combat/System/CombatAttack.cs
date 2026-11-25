using System;

namespace Terraria.Share.Combat;

/// <summary>
/// Immutable content rule used to create a one-tick damage payload. It is not runtime entity state.
/// </summary>
public sealed class CombatAttack
{
  public CombatAttack(
      CombatAttackId id,
      DamagePayload payload,
      ImmunityChannel channel = ImmunityChannel.General)
  {
    if (!id.IsValid)
    {
      throw new ArgumentException("An attack definition id is required.", nameof(id));
    }

    if (!payload.IsValid)
    {
      throw new ArgumentException("A valid attack payload is required.", nameof(payload));
    }

    if ((int)channel < 0 || (int)channel >= (int)ImmunityChannel.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(channel));
    }

    Id = id;
    Payload = payload;
    Channel = channel;
  }

  public CombatAttackId Id { get; }

  public DamagePayload Payload { get; }

  /// <summary>Target-side immunity channel this attack consumes. Content fact, not derived.</summary>
  /// <remarks>
  /// 通道不能从 DamageCause 推导：同为弹体，毒牙走 General，岩浆弹走 Lava。
  /// 由内容显式声明，未声明就是 General。
  /// </remarks>
  public ImmunityChannel Channel { get; }
}
