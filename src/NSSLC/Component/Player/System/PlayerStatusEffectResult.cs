namespace Terraria.Player;

public readonly record struct PlayerStatusEffectResult(
  bool Applied,
  bool Refreshed,
  bool Evicted,
  int SlotIndex,
  int RemainingTicks,
  PlayerStatusEffectRejectionReason RejectionReason)
{
  public static PlayerStatusEffectResult Rejected(
    PlayerStatusEffectRejectionReason reason)
  {
    return new PlayerStatusEffectResult(
      Applied: false,
      Refreshed: false,
      Evicted: false,
      SlotIndex: -1,
      RemainingTicks: 0,
      RejectionReason: reason);
  }
}
