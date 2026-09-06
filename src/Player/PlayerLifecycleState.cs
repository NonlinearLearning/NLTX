namespace Terraria.Player;

public sealed class PlayerLifecycleState
{
  public PlayerLifecyclePhase Phase { get; set; }

  public int DeadElapsedTicks { get; set; }

  public int RespawnRemainingTicks { get; set; }

  public LegacyPlayerSlot? SpectatingTargetSlot { get; set; }

  public bool WasPvpDeath { get; set; }

  public int PveDeathCount { get; set; }

  public int PvpDeathCount { get; set; }

  public WorldPosition LastDeathWorldPosition { get; set; }

  public SimulationTick? LastDeathTick { get; set; }

  public bool IsDead => Phase is PlayerLifecyclePhase.Dead or PlayerLifecyclePhase.Respawning;

  public bool CanRespawn => IsDead && RespawnRemainingTicks <= 0;
}
