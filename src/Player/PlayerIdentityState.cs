namespace Terraria.Player;

public sealed class PlayerIdentityState
{
  public string CharacterName { get; set; } = string.Empty;

  public int TeamId { get; set; }

  public PlayerDifficulty Difficulty { get; set; }

  public bool IsHost { get; set; }

  public PlayerConnectionState ConnectionState { get; set; }

  public LegacyPlayerSlot? LegacySlot { get; set; }

  public bool IsActive => ConnectionState == PlayerConnectionState.Active;
}
