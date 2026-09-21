using Terraria.Dome.Simulation.Npc.Snapshots;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcSpawnPlayerReadinessQuery
{
  public static bool CanSpawnEnemiesNear(NpcSpawnPlayerReadiness player)
  {
    if (!player.IsActive || player.IsDead)
    {
      return false;
    }

    if (player.IsJourneyMode && player.IsSpawnRateDisabled)
    {
      return false;
    }

    return !player.IsNearMoonLord;
  }
}
