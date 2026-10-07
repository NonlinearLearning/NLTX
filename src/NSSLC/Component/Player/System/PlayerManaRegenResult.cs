namespace Terraria.Player;

public readonly record struct PlayerManaRegenResult(
  bool Applied,
  int ManaBefore,
  int ManaAfter,
  int ManaRegen,
  int ManaRegenCountAfter,
  float ManaRegenDelayAfter,
  PlayerManaRegenFailureReason FailureReason)
{
  public static PlayerManaRegenResult Rejected(
    PlayerManaRegenFailureReason reason,
    int currentMana)
  {
    return new PlayerManaRegenResult(
      Applied: false,
      ManaBefore: currentMana,
      ManaAfter: currentMana,
      ManaRegen: 0,
      ManaRegenCountAfter: 0,
      ManaRegenDelayAfter: 0f,
      FailureReason: reason);
  }
}
