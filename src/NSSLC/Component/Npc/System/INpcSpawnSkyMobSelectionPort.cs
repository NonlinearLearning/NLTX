namespace Terraria.Npc;

public interface INpcSpawnSkyMobSelectionPort : INpcSpawnTypeResolutionRandomPort
{
  bool AnyDanger();
  bool HasActiveNpc(int npcTypeId);
  int RollLuck(int range);
}
