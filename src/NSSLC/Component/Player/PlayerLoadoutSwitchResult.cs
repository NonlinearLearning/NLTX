namespace Terraria.Player;

public readonly record struct PlayerLoadoutSwitchResult(
  bool Applied,
  int PreviousLoadoutIndex,
  int CurrentLoadoutIndex,
  PlayerLoadoutSwitchRejectionReason RejectionReason)
{
  public static PlayerLoadoutSwitchResult Rejected(
    int currentLoadoutIndex,
    PlayerLoadoutSwitchRejectionReason rejectionReason)
  {
    return new PlayerLoadoutSwitchResult(
      Applied: false,
      PreviousLoadoutIndex: currentLoadoutIndex,
      CurrentLoadoutIndex: currentLoadoutIndex,
      RejectionReason: rejectionReason);
  }
}
