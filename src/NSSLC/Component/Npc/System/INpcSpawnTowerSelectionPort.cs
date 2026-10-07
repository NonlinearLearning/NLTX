namespace Terraria.Npc;

public interface INpcSpawnTowerSelectionPort : INpcSpawnTypeResolutionRandomPort
{
  int CountActiveNpcs(int npcTypeId);
}
