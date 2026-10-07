namespace Terraria.Npc;

public interface INpcSpawnCritterSelectionPort : INpcSpawnTypeResolutionRandomPort
{
  int RollLuck(int range);
}
