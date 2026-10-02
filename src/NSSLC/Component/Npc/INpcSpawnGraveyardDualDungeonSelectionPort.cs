namespace Terraria.Npc;

public interface INpcSpawnGraveyardDualDungeonSelectionPort
{
  int RollBadLuckExtreme(int denominator);

  bool HasActiveNpc(int npcTypeId);

  bool IsGoodPlaceForStatueMimic(int tileX, int tileY);

  int RollBadLuck(int denominator);
}
