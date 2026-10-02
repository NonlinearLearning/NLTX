namespace Terraria.Player;

public readonly record struct PlayerLifeRegenResult(
  bool Computed,
  int LifeRegen,
  int LifeRegenCountAfter,
  float LifeRegenTimeAfter,
  int HealingPoints,
  int DamagePoints,
  int DamageCommitCount,
  bool UsesSpecialDamageRule,
  bool RequiresExternalLifeCommit,
  PlayerLifeRegenFailureReason FailureReason)
{
  public int HealingTicks => HealingPoints;

  public int LifeDamageTotal => DamagePoints;

  public static PlayerLifeRegenResult Rejected(
    PlayerLifeRegenFailureReason reason,
    in PlayerLifeRegenInput input)
  {
    return new PlayerLifeRegenResult(
      Computed: false,
      LifeRegen: input.LifeRegen,
      LifeRegenCountAfter: input.LifeRegenCount,
      LifeRegenTimeAfter: input.LifeRegenTime,
      HealingPoints: 0,
      DamagePoints: 0,
      DamageCommitCount: 0,
      UsesSpecialDamageRule: false,
      RequiresExternalLifeCommit: false,
      FailureReason: reason);
  }
}
