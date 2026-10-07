namespace Terraria.Player;

public readonly record struct PlayerIdentityState(
  string CharacterName,
  int TeamId,
  PlayerDifficulty Difficulty,
  bool IsHost,
  PlayerConnectionState ConnectionState,
  LegacyPlayerSlot? LegacySlot)
{
  public bool IsActive => ConnectionState == PlayerConnectionState.Active;
}
