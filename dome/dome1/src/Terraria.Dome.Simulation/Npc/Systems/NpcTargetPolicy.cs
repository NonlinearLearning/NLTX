using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Npc.Definitions;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcTargetPolicy
{
  public static int GetBusinessTargetingIndex(NpcHandle handle)
  {
    if (!handle.IsValid)
    {
      return -1;
    }

    return checked(handle.Value + NpcLegacyFieldPolicy.NpcTargetsStart);
  }
}
