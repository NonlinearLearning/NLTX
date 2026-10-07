using Terraria.Npc;

namespace Terraria.Npc.Queries;

public static class NpcSpawnTileSpaceQuery
{
  public static bool CanSpawn(in NpcSpawnTileSpaceFacts facts)
  {
    return !(facts.IsActive && facts.IsSolid) && !facts.HasAnyLava;
  }
}
