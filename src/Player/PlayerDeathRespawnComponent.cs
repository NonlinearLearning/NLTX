namespace Terraria.Player;

// Owns the player's death, respawn and spectating lifecycle facts.
// Death effects, target validation and network projection remain external.
public sealed class PlayerDeathRespawnComponent
{
  public bool IsDead { get; set; }

  public int DeadElapsedTicks { get; set; }

  public LegacyPlayerSlot? SpectatingTarget { get; set; }

  public int RespawnRemainingTicks { get; set; }
}
