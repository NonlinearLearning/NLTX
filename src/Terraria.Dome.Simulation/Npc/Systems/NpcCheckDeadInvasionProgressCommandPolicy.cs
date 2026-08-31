using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcCheckDeadInvasionProgressCommandPolicy
{
  public static bool TryCreate(
    NpcCheckDeadInvasionProgressDecision decision,
    long sequence,
    out WorldInvasionProgressCommand command)
  {
    if (!decision.Applies || decision.Points <= 0 || sequence < 0 || sequence == long.MaxValue)
    {
      command = default;
      return false;
    }

    command = new WorldInvasionProgressCommand(decision.Points, sequence);
    return true;
  }
}
