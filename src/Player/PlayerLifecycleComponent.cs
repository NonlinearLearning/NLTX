namespace Terraria.Player;

public struct PlayerLifecycleComponent
{
  public PlayerLifecycleComponent(PlayerLifecycleStage stage, int remainingRespawnTicks)
  {
    Stage = stage;
    RemainingRespawnTicks = remainingRespawnTicks;
    Phase = stage switch
    {
      PlayerLifecycleStage.Alive => PlayerLifecyclePhase.Alive,
      PlayerLifecycleStage.Dead => PlayerLifecyclePhase.Dead,
      PlayerLifecycleStage.WaitingToRespawn => PlayerLifecyclePhase.Respawning,
      _ => PlayerLifecyclePhase.Alive,
    };
    IsActive = Phase == PlayerLifecyclePhase.Alive;
    IsDead = Phase == PlayerLifecyclePhase.Dead;
    DeadElapsedTicks = 0;
    RespawnRemainingTicks = remainingRespawnTicks;
    SpectatingTarget = null;
    WasPvpDeath = false;
    PveDeathCount = 0;
    PvpDeathCount = 0;
    LastDeathPosition = default;
    LastDeathTime = default;
    ShowLastDeath = false;
  }

  public PlayerLifecycleStage Stage;
  public int RemainingRespawnTicks;
  public PlayerLifecyclePhase Phase;
  public bool IsActive;
  public bool IsDead;
  public int DeadElapsedTicks;
  public int RespawnRemainingTicks;
  public LegacyPlayerSlot? SpectatingTarget;
  public bool WasPvpDeath;
  public int PveDeathCount;
  public int PvpDeathCount;
  public WorldPosition LastDeathPosition;
  public DateTime LastDeathTime;
  public bool ShowLastDeath;
}
