namespace Terraria.Dome.Simulation.Npc.Components;

public enum NpcDespawnReason
{
  None = 0,
  TimedOut = 1,
  OutOfRange = 2,
  Killed = 3,
  SegmentRootRemoved = 4
}

public struct NpcLifecycleComponent
{
  public NpcLifecycleComponent(bool isActive, int timeLeft)
  {
    IsActive = isActive;
    TimeLeft = timeLeft;
    DespawnReason = NpcDespawnReason.None;
  }

  public bool IsActive;
  public int TimeLeft;
  public NpcDespawnReason DespawnReason;
}
