using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Unshared.EntitySpawn;

/// <summary>Locates the single entity that carries the NPC spawn fact queue.</summary>
public static class EntitySpawnFactQueue
{
  public static EntitySpawnFactQueueComponent Require(ArchWorld world)
    => SingletonComponent<EntitySpawnFactQueueComponent>.Require(
        world,
        "An EntitySpawnFactQueueComponent entity must exist before the spawner can create NPCs.");
}
