using System.Numerics;

namespace Terraria.Npc;

public interface INpcMotherSlimeDeathSplitEffectPort
{
  int MaxNpcSlots { get; }

  int Next(int maxExclusive);

  int Next(int minInclusive, int maxExclusive);

  int SpawnBlueSlimeChild(
    NpcInstanceId sourceNpcInstanceId,
    int positionX,
    int positionY);

  void ConfigureBlueSlimeChild(
    int npcSlot,
    Vector2 velocity,
    float ai0);

  void SendNpcSyncPacket(int npcSlot);
}
