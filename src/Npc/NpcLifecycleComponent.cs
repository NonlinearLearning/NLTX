namespace Terraria.Npc;

public struct NpcLifecycleComponent
{
  public NpcLifecycleComponent(NpcLifecycleStage stage, int remainingDespawnTicks)
  {
    Stage = stage;
    RemainingDespawnTicks = remainingDespawnTicks;
  }

  public NpcLifecycleStage Stage;
  public int RemainingDespawnTicks;
}
