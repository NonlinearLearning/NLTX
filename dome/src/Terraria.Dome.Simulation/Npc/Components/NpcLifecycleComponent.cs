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
  public NpcLifecycleComponent(
    bool isActive,
    int timeLeft,
    bool canBeReplaced = false,
    bool doesNotCountMe = false,
    bool homelessDespawn = false)
  {
    IsActive = isActive;
    TimeLeft = timeLeft;
    CanBeReplaced = canBeReplaced;
    DoesNotCountMe = doesNotCountMe;
    HomelessDespawn = homelessDespawn;
    DespawnEncouraged = false;
    DespawnReason = NpcDespawnReason.None;
  }

  public bool IsActive;
  public int TimeLeft;
  public bool CanBeReplaced;
  public bool DoesNotCountMe;
  public bool HomelessDespawn;
  public bool DespawnEncouraged;
  public NpcDespawnReason DespawnReason;

  public void MarkHomelessDespawn()
  {
    HomelessDespawn = true;
  }

  public void ClearHomelessDespawn()
  {
    HomelessDespawn = false;
  }

  public void EncourageDespawn(int despawnTime)
  {
    if (TimeLeft > despawnTime)
    {
      TimeLeft = despawnTime;
    }

    DespawnEncouraged = true;
  }

  public void DiscourageDespawn(int despawnTime)
  {
    if (TimeLeft < despawnTime)
    {
      TimeLeft = despawnTime;
    }

    DespawnEncouraged = false;
  }
}
