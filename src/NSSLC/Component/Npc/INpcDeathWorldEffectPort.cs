namespace Terraria.Npc;

public interface INpcDeathWorldEffectPort
{
  int MaxTilesY { get; }

  int MaxNpcSlots { get; }

  int Next(int minimumInclusive, int maximumExclusive);

  bool IsSolidTile(int tileX, int tileY);

  int SpawnNpcFromNpcAi(
    NpcInstanceId sourceNpcInstanceId,
    NpcTypeId npcType,
    int positionX,
    int positionY);
}
