using Terraria.Share.Ai;
using Terraria.Share.Entities;

namespace Terraria.Unshared.EntitySpawn;

/// <summary>One entry of the spawn table: which entity to create and how likely it is.</summary>
/// <remarks>
/// Version4 encodes this as a chain of <c>if</c> branches over biome flags inside
/// <c>Spawner.SpawnAnNPC</c> and then calls <c>NPC.NewNPC</c> with a numeric type. Here the entry
/// carries the entity definition, the protocol NPC type and the AI component to attach, so the
/// system never names a concrete creature.
/// </remarks>
public sealed record EntitySpawnCandidate(
    EntityId DefinitionId,
    short NetworkType,
    AiComponent Ai,
    int Weight)
{
  public bool IsValid => Weight > 0 && NetworkType > 0 && Ai.BehaviorType is not null;
}
