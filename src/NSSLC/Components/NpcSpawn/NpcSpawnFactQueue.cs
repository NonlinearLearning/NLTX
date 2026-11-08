using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Npc;

/// <summary>Locates the single entity that carries the NPC spawn fact queue.</summary>
public static class NpcSpawnFactQueue
{
  public static NpcSpawnFactQueueComponent Require(ArchWorld world)
    => SingletonComponent<NpcSpawnFactQueueComponent>.Require(
        world,
        "An NpcSpawnFactQueueComponent entity must exist before the spawner can create NPCs.");
}
