namespace Terraria.Player;

public struct PlayerLifecycleComponent
{
  public PlayerLifecycleComponent(PlayerLifecyclePhase phase, int respawnRemainingTicks)
  {
    Phase = phase;
    DeadElapsedTicks = 0;
    RespawnRemainingTicks = respawnRemainingTicks;
    SpectatingTargetSlot = null;
  }

  public PlayerLifecyclePhase Phase { get; internal set; }

  public int DeadElapsedTicks { get; internal set; }

  public int RespawnRemainingTicks { get; internal set; }

  public LegacyPlayerSlot? SpectatingTargetSlot { get; internal set; }

  public bool IsDead => Phase is PlayerLifecyclePhase.Dead or PlayerLifecyclePhase.Respawning;

  public bool CanRespawn => IsDead && RespawnRemainingTicks <= 0;
}
