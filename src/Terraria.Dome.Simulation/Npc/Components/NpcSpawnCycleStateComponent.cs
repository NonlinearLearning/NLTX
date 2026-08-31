namespace Terraria.Dome.Simulation.Npc.Components;

public struct NpcSpawnCycleStateComponent
{
  private bool _skipNextSpawnCycle;

  public bool SkipNextSpawnCycle => _skipNextSpawnCycle;

  public void MarkSkipNextSpawnCycle()
  {
    _skipNextSpawnCycle = true;
  }

  public bool TryConsumeSkipNextSpawnCycle()
  {
    if (!_skipNextSpawnCycle)
    {
      return false;
    }

    _skipNextSpawnCycle = false;
    return true;
  }
}
