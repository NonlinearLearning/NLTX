namespace Terraria.Player;

public readonly record struct PlayerLifecycleState(
  PlayerLifecyclePhase Phase,
  int DeadElapsedTicks,
  int RespawnRemainingTicks,
  LegacyPlayerSlot? SpectatingTargetSlot)
{
  public bool IsDead => Phase is PlayerLifecyclePhase.Dead or PlayerLifecyclePhase.Respawning;

  public bool CanRespawn => IsDead && RespawnRemainingTicks <= 0;
}
