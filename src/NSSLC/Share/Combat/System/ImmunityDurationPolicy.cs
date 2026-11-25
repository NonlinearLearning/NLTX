namespace Terraria.Share.Combat;

/// <summary>Maps one landed hit to the number of ticks its immunity window lasts.</summary>
/// <remarks>
/// 对齐参考端 Player.cs:38668 的 num10 算式。参考端按减伤后的实际伤害反推，不按请求伤害：
/// 打 1 点只给短窗口，被重击给长窗口，PvP 只给 8。
/// <para>
/// longInvince 对应参考端的十字项链 / 星星面纱一类饰品。本仓库尚无饰品属性来源，
/// 调用点恒传 false；参数位保留，等来源落地再接，避免造出没有生产者的输入。
/// </para>
/// </remarks>
public static class ImmunityDurationPolicy
{
  /// <summary>参考端值为 8：PvP 命中。</summary>
  public const int Pvp = 8;

  /// <summary>参考端值为 20：实际伤害恰为 1 的擦伤。</summary>
  public const int ChipDamage = 20;

  /// <summary>参考端值为 40：实际伤害大于 1。</summary>
  public const int Normal = 40;

  /// <summary>参考端值为 40：长无敌下的擦伤。</summary>
  public const int LongChipDamage = 40;

  /// <summary>参考端值为 80：长无敌下的实伤。</summary>
  public const int LongNormal = 80;

  /// <summary>参考端值为 80：闪避。</summary>
  public const int Dodge = 80;

  /// <summary>参考端值为 120：长无敌下的闪避。</summary>
  public const int LongDodge = 120;

  /// <summary>参考端值为 60：进入世界。</summary>
  public const int SpawnIntoWorld = 60;

  /// <summary>参考端值为 180：复活。</summary>
  public const int ReviveFromDeath = 180;

  /// <summary>参考端值为 300：PvP 死亡后重生。</summary>
  public const int PvpDeath = 300;

  /// <summary>Computes the window length for one landed hit.</summary>
  /// <param name="appliedDamage">Post-mitigation damage that actually landed.</param>
  /// <param name="pvp">True when the hit came through the friendly-fire channel.</param>
  /// <param name="longInvince">True when a long-immunity source is active.</param>
  public static int ForHit(int appliedDamage, bool pvp, bool longInvince)
  {
    if (appliedDamage <= 0)
    {
      return 0;
    }

    if (pvp)
    {
      return Pvp;
    }

    return appliedDamage == 1
        ? (longInvince ? LongChipDamage : ChipDamage)
        : (longInvince ? LongNormal : Normal);
  }
}
