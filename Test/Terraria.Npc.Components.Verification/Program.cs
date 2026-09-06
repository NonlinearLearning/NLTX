using Terraria.Npc;
using Terraria.Relationships;
using Terraria.Server.Npc;
using Terraria.Town;

const ulong instanceIdValue = 19;
NpcEntityIdentityComponent identity = new(new NpcInstanceId(instanceIdValue), new NpcSlot(12));
if (identity.InstanceId.Value != instanceIdValue || !identity.HasAssignedSlot)
{
  throw new InvalidOperationException("NPC identity did not preserve the stable instance ID and legacy slot.");
}

NpcDefinitionReferenceComponent definition = new(new NpcTypeId(17), new NpcNetId(-17));
if (!definition.IsInitialized || !definition.UsesNetIdVariant)
{
  throw new InvalidOperationException("NPC definition reference did not preserve type and variant identity.");
}

NpcBehaviorStateComponent behavior = new(behaviorKind: 7, legacyAiStyle: 7, action: 3);
if (!behavior.HasAuthoritativeSlots || behavior.AuthoritativeAiSlots.Length != 4)
{
  throw new InvalidOperationException("NPC behavior state did not allocate the four authoritative AI slots.");
}

NpcLifetimeComponent lifetime = new(remainingTicks: 60, populationSlotCost: 1.5f, countsAgainstPopulation: true);
if (lifetime.IsExpired || lifetime.EffectivePopulationCost != 1.5f)
{
  throw new InvalidOperationException("NPC lifetime did not expose the active population contribution.");
}

NpcTargetComponent target = new(
  NpcTargetKind.Player,
  new EntityReference(
    Guid.Parse("c1f2b4d0-7aac-4e27-9aa6-52f095fdc9d0"),
    EntityReferenceScope.Player),
  selectedAtTick: 18);
if (!target.HasTarget || target.TargetEntity?.Scope != EntityReferenceScope.Player)
{
  throw new InvalidOperationException("NPC target state did not retain the selected entity relationship.");
}

NpcParentRelationComponent parent = new(
  new NpcInstanceId(44),
  new NpcSlot(3),
  attachedAtTick: 19);
if (!parent.HasLegacyParentSlot || parent.ParentInstanceId.Value != 44)
{
  throw new InvalidOperationException("NPC parent relation did not distinguish stable identity from slot.");
}

NpcReplicationDirtyState replication = new()
{
  Flags = NpcReplicationFlags.StateDirty | NpcReplicationFlags.SpawnNeedsSync,
};
if (!replication.IsDirty || !replication.RequiresSpawnSync)
{
  throw new InvalidOperationException("NPC replication state did not expose projection-only dirty flags.");
}

TownResidentComponent resident = new(
  isFriendly: true,
  housingCategory: 2,
  capabilities: TownResidentCapabilities.CanUseHousing | TownResidentCapabilities.CanOpenDialogue);
if (!resident.CanUseHousing || !resident.CanOpenDialogue)
{
  throw new InvalidOperationException("Town resident capabilities were not exposed as derived properties.");
}

NpcHousingAssignmentComponent housing = new(
  TownHousingStatus.Homeless,
  homeTile: null,
  searchCooldownTicks: 0,
  despawnWhenHomeless: false,
  assignmentRevision: 4);
if (!housing.IsHomeless || !housing.IsEligibleForSearch)
{
  throw new InvalidOperationException("Homeless town resident state did not expose housing-search eligibility.");
}

Console.WriteLine("PASS: NPC and town component field composition");
