namespace Terraria.Npc;

// status: implementation-started
// sourceMembers: SpawnedFromStatue, CanBeReplacedByOtherNPCs
// crossSubsystemOwner: spawn-commit-and-replacement-integration-review
public sealed class NpcSpawnAndCritterStateComponent
{
  public NpcSpawnAndCritterStateComponent(
    bool spawnedFromStatue = false,
    bool canBeReplacedByOtherNpcs = false)
  {
    SpawnedFromStatue = spawnedFromStatue;
    CanBeReplacedByOtherNpcs = canBeReplacedByOtherNpcs;
  }

  public bool SpawnedFromStatue { get; }

  public bool CanBeReplacedByOtherNpcs { get; }
}
