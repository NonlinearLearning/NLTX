namespace Terraria.Npc;

public interface INpcSpawnEntityPreparationPort
{
  NpcTypeId FromNetId(NpcTypeId requestedType);

  int RollLuck(int range);
}
