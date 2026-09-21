namespace Terraria.Player;

public sealed class PlayerIdentityComponent
{
  // Runtime EntityUuid remains owned by the shared entity identity component.
  public PersistentPlayerId? PersistentPlayerId { get; set; }

  public string CharacterName { get; set; } = string.Empty;

  // Compatibility alias for the Version4 public identity name.
  public string DisplayName
  {
    get => CharacterName;
    set => CharacterName = value;
  }

  public int TeamId { get; set; }

  public PlayerDifficulty Difficulty { get; set; }

  public bool IsActive { get; set; }

  public bool IsHost { get; set; }

  // Compatibility projection only; it is not a persistence key.
  public LegacyPlayerSlot? LegacyPlayerSlot { get; set; }
}
