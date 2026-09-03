using Terraria.Dome.Simulation.Npc.Definitions;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcSpawnSlotPolicy
{
  public const int ProtectionTimeTicks = NpcLegacyFieldPolicy.SpawnSlotProtectionTime;

  public static bool IsProtected(int ageTicks)
  {
    return ageTicks >= 0 && ageTicks < ProtectionTimeTicks;
  }
}
