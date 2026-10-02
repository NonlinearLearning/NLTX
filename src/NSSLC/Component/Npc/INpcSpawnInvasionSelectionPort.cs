namespace Terraria.Npc;

public interface INpcSpawnInvasionSelectionPort : INpcSpawnTypeResolutionRandomPort
{
  bool HasActiveNpc(int npcTypeId);
  bool HasAnySolidTiles(
    int startXInclusive,
    int endXInclusive,
    int startYInclusive,
    int endYInclusive);
}
