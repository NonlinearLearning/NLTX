namespace Terraria.Player;

public readonly record struct PlayerLoadoutNetworkResult(
  int PacketPlayerIndex,
  int AuthorityPlayerIndex,
  PlayerLoadoutSwitchResult Loadout,
  PlayerAccessoryVisibilityResult Visibility,
  bool VisibilityAttemptedAfterLoadout);
