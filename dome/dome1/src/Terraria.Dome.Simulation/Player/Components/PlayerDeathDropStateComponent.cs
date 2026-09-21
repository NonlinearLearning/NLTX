namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerDeathDropStateComponent
{
  public bool DropCommitted;
  public int DeathCause;

  public void Begin(int deathCause)
  {
    DeathCause = deathCause;
    DropCommitted = false;
  }

  public bool TryCommitDrop()
  {
    if (DropCommitted)
    {
      return false;
    }

    DropCommitted = true;
    return true;
  }

  public void Clear()
  {
    DropCommitted = false;
    DeathCause = 0;
  }
}
