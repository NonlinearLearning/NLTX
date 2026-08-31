namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcCheckDeadSpawnCyclePolicy
{
  public static NpcCheckDeadSpawnCycleDecision Evaluate(
    NpcCheckDeadSpawnCycleInput input)
  {
    if (!input.IsQualifiedDeath)
    {
      return new(false, false);
    }

    return new(true, input.SkipNextSpawnCycleAlreadyMarked);
  }
}
