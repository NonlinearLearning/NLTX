namespace Terraria.Player;

public struct PlayerLifecycleComponent
{
  public PlayerLifecycleComponent(PlayerLifecycleStage stage, int remainingRespawnTicks)
  {
    Stage = stage;
    RemainingRespawnTicks = remainingRespawnTicks;
  }

  public PlayerLifecycleStage Stage;
  public int RemainingRespawnTicks;
}
