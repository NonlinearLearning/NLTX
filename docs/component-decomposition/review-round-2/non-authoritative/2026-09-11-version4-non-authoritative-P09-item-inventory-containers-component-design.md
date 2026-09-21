# P09 Proposed Component Design: Item, Inventory And Containers

partitionId: P09
sessionId: 06b62f1ec1a14935b94fe5661c830bb2
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\09-item-inventory-containers.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P09-item-inventory-containers-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P09-item-inventory-containers-component-execution.md
designStatus: proposed
executionStatus: completed (src2-only implementation checkpoint)
implementationStatus: in-progress
verificationStatus: partial
completedComponents:
- proposed.P09ChestStorageBoundary
- proposed.P09ItemIdentityAndStackBoundary
- proposed.P09ItemProgressionInteractionBoundary
- proposed.P09ItemBuffMountConsumableBoundary
- proposed.P09ItemDerivedQueryBoundary
- proposed.P09QuickStackPlanningBoundary
- proposed.P09QuickStackReferenceSnapshotBoundary
- proposed.P09TransferPolicyAndEffectBoundary
- proposed.P09WorldItemLifecycleBoundary
- proposed.P09ItemEquipmentAppearanceBoundary
- proposed.P09WorldItemEconomyProjectionBoundary
- proposed.P09WorldItemUsePresentationProjectionBoundary
- proposed.P09EmergencyStackingPolicyBoundary
- proposed.P09EmergencyStackingTransferPlanBoundary
- proposed.P09EmergencyStackMutationBoundary
- proposed.P09ToolPlacementCapabilityBoundary
- proposed.P09ContainerRegistryAdapterBoundary
- proposed.P09ContainerProtocolProjectionBoundary
- proposed.P09FocusedVerificationCheckpoint
currentComponent: proposed.P09IntegrationOwnershipReview
pendingComponents:
- proposed.P09IntegrationOwnershipReview
- proposed.P09AtomicContainerTransferPort
- proposed.P09WorldItemPickupAndCombineCommand
- proposed.P09BehaviorProtocolPersistenceVerification
lastCheckpointUtc: 2026-09-12T07:15:25.882Z
evidence-gap:
- P09 first-round public-decomposition report is missing at D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P09-item-inventory-containers-public-decomposition.md; it is recorded as a missing input and is not recreated here.
- The partition report declares upstream source-report SHA-256 b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196, while the observed SHA-256 of the current partition report is 22C9EA0B355D729C895799DAE6408ADFBFD5C798CD847A52349A81D97C78F0CA; provenance must be reconciled before implementation.
- Member-level readers, writers, initialization, cleanup, persistence, network semantics, and scheduler ownership are not closed for all 191 rows.
- Version4 has incomplete or stubbed behavior in QuickStacking, EmergencyStacking, Player pickup/slot helpers, and related transfer paths; no behavior-equivalence claim is possible.
- Item/container identity, slot reference, persistent ID, network ID, entity reference, inventory authority, world-item result authority, and global scheduler order remain cross-partition questions.
- The src2 implementation is independently compiled and focused-verified, but it is not wired into production src/ or the runtime scheduler.
- ItemTransferCommand currently records operation idempotency and post-commit effects; it does not perform an atomic multi-container slot mutation.
- WorldItem pickup/combine commands, player inventory ownership, and cross-partition protocol/persistence closure remain deferred.
blocking-decision:
- Final ownership of ItemInstanceId, ContainerId, SlotReference, EntityReference, persistent IDs, network IDs, snapshot value objects, and cross-partition interfaces is crossSubsystemOwner: integration-review.
- Final ownership of player inventory, equipment, Void Vault, bank containers, tile-entity containers, currency, NPC/world-item results, and UI/network/persistence schemas is crossSubsystemOwner: integration-review.
- Atomic transfer semantics, partial commit/compensation, reservation expiry, retry ownership, and scheduler barriers remain crossSubsystemOwner: integration-review.
src2CodeModified: yes
productionSrcModified: no

## Session Rebinding Checkpoint

- The prior failed session `9052197318bf40d282ff4936c4fa000d` was not reused as an owner.
- The non-authoritative runner issued a fresh `Claim -Retry` session
  `06b62f1ec1a14935b94fe5661c830bb2`; this document and its companion execution document are
  bound to that session.
- Existing src2 checkpoint files were retained as implementation evidence; no production `src/`
  file, ledger, or lock file was modified by the rebinding.

## Status Boundary

This is a second-round, non-authoritative, proposed design record with an implementation checkpoint.
The input report is a source inventory, not an ECS implementation and not a behavior-equivalence
proof. The design remains proposed; the separately listed src2 files are implementation evidence
only and are not production src/ migration, component registration, runtime integration, or final
cross-partition ownership.

The P09 inventory contains 14 leaf groups, 152 fields, 39 properties, and 191 total members.
Source sequence numbers are traceability keys only. They do not define runtime order and must not
be used as a scheduler contract.

## Scope And Exclusions

In scope:

- Chest slot contents, chest metadata, chest address/indexing, bank classification, naming,
  presentation state, and static icon lookup boundaries.
- Item content identity, quantity/stack state, variant and favorited state, footprint, derived
  content queries, progression/interaction capabilities, effects, equipment appearance, and
  tool/placement/ammunition capabilities.
- Quick Stack and Emergency Stack planning data, eligibility queries, transfer commands, and
  explicit side-effect boundaries.
- WorldItem relation to an item payload, reservation/ownership/pickup/shimmer/age/conveyor
  lifecycle, and world-item economy/use/presentation projections.
- Proposed adapters and projections for persistence, network synchronization, client/UI views,
  and legacy Terraria callers.

Out of scope:

- Final ownership of Player inventory, equipment containers, banks, Void Vault, tile-entity
  containers, currency, NPC/world-item spawning, combat/use resolution, fishing, crafting,
  loot, tile authority, UI, localization, network transport, persistence storage, and the global
  tick scheduler.
- Production changes under src/, Test/, Version4, complete-reference sources, the ledger, or the
  lock. The implementation checkpoint is confined to the listed src2 project and verifier files;
  the two declared second-round documents are the only documentation outputs.
- Persistent item/container/slot identity decisions. Proposed handles are integration seams only
  and cannot be promoted to an ID contract in this partition.

## Version4 Evidence

The direct source evidence read for this design includes:

- D:\TRbackup\Version4\Terraria\Chest.cs for Chest slot storage, capacity, coordinate/index
  registry, bank/name metadata, animation fields, and current-use tracking.
- D:\TRbackup\Version4\Terraria\Item.cs for item fields, stack and prefix state, capability
  payloads, equipment slots, and derived properties.
- D:\TRbackup\Version4\Terraria\WorldItem.cs for the embedded Item relation, ownership
  reservation, pickup constraints, shimmer, instancing, age, movement, and forwarding properties.
- D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs for destination/source snapshots,
  type-index scratch structures, blocked destination state, and quick-transfer boundaries.
- D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs for group policy, preservation
  order, candidate/transfer records, ownership qualification, and transfer-count calculation.
- D:\TRbackup\Version4\Terraria\GetItemSettings.cs for transfer presets, boolean policy flags,
  and the StepAfterHandlingSlotNormally callback.
- D:\TRbackup\Version4\Terraria\Player.cs, D:\TRbackup\Version4\Terraria.IO\WorldFile.cs,
  D:\TRbackup\Version4\Terraria\NetMessage.cs, D:\TRbackup\Version4\Terraria\MessageBuffer.cs,
  and D:\TRbackup\Version4\Terraria\Main.cs for item/chest callers, persistence, protocol,
  static registries, and current-player/world integration.

Additional structural cross-checks were read from the local tModLoader v2026.07 API mirror under
D:\TRbackup\tmodloader-api-docs-stable, including class_item.html, class_chest.html,
class_player.html, and struct_get_item_settings.html. These are public API references only and
do not establish Version4 private behavior.

Structural granularity references were read from:

- D:\TRbackup\无任何删减通过编译\Content.Shared\Containers\ContainerCompComponent.cs
- D:\TRbackup\无任何删减通过编译\Content.Shared\Containers\ContainerCompSystem.cs
- D:\TRbackup\无任何删减通过编译\Content.Shared\Containers\ItemSlot\ItemSlotsComponent.cs
- D:\TRbackup\无任何删减通过编译\Content.Shared\Containers\ItemSlot\ItemSlotsSystem.cs
- D:\TRbackup\无任何删减通过编译\Content.IntegrationTests\Tests\Stacks\StackTests.cs

These references supply structure and testing patterns only. They do not transfer ownership or
behavior to NLTX.

Confirmed boundary facts:

- Chest.item is chest slot storage. Player inventory/banks and WorldItem.inner are separate
  item-instance relationships; they cannot be collapsed into one container component.
- Chest coordinate lookup and current-use tracking are registries/ephemeral access state, not
  authoritative slot contents.
- Chest persistence writes coordinates, name, capacity, and item slots. Chest network messages
  synchronize slot type, stack, and prefix. These protocols do not establish persistent item or
  container identity.
- Item.type identifies content, stack/maxStack quantity, prefix/Variant variation, and
  active/IsAir/IsACoin/name/original statistics are derived or catalog lookups.
- WorldItem.inner is an embedded Item relation. Most WorldItem item properties forward to or
  derive from inner; they must not become duplicate independent authorities.
- Quick Stack and Emergency Stack records are planning/scratch data. Candidate and transfer
  records must expire with the operation and must not become durable item state.
- GetItemSettings.StepAfterHandlingSlotNormally is an effect callback. The proposed boundary is
  an explicit effect/command port so query and policy evaluation do not hide side effects.

## Current NLTX State

The P09 implementation checkpoint exists only under src2. It is an independent library and
focused verifier, not a production src/ migration or registered runtime ECS implementation. The
source changes are intentionally isolated from src/; no production source file was modified.
The implementation covers the proposed state/query/plan/projection seams listed below, while
atomic multi-container transfer, WorldItem pickup/combine commands, cross-partition ownership,
and behavior equivalence remain open.

## src2 Implementation Checkpoint

The following files are the complete P09 implementation and verifier file set observed at the
checkpoint. They are implementation evidence under src2, not a claim that production integration
is complete:

```text
src2/Items/InventoryContainers/Capabilities/ItemBuffMountConsumableCapabilityComponent.cs
src2/Items/InventoryContainers/Capabilities/ItemProgressionInteractionCapabilityComponent.cs
src2/Items/InventoryContainers/Capabilities/ItemToolPlacementCapabilityComponent.cs
src2/Items/InventoryContainers/Containers/ChestAccessTrackerAdapter.cs
src2/Items/InventoryContainers/Containers/ChestCapacityPolicy.cs
src2/Items/InventoryContainers/Containers/ChestIconCatalogAdapter.cs
src2/Items/InventoryContainers/Containers/ChestInitializationState.cs
src2/Items/InventoryContainers/Containers/ChestMetadataComponent.cs
src2/Items/InventoryContainers/Containers/ChestMetadataPolicy.cs
src2/Items/InventoryContainers/Containers/ChestPresentationStateComponent.cs
src2/Items/InventoryContainers/Containers/ChestRegistryAdapter.cs
src2/Items/InventoryContainers/Containers/ChestSlotStorageComponent.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingCandidateSnapshot.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingCommitCommand.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingCommitResult.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingComponentMutationPort.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingGroupDefinition.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingMutationResult.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingOwnershipQuery.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingPolicyDefinition.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingPredicateCatalog.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingSchedulerState.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingTransferAmountQuery.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingTransferDecision.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingTransferPlan.cs
src2/Items/InventoryContainers/EmergencyStacking/EmergencyStackingTransferQuery.cs
src2/Items/InventoryContainers/EmergencyStacking/IEmergencyStackingMutationPort.cs
src2/Items/InventoryContainers/Equipment/ItemEquipmentAppearanceComponent.cs
src2/Items/InventoryContainers/Equipment/ItemEquipmentAppearanceQuery.cs
src2/Items/InventoryContainers/Equipment/ItemEquipmentAppearanceSnapshot.cs
src2/Items/InventoryContainers/Identity/InventoryPosition.cs
src2/Items/InventoryContainers/Identity/ItemDefinition.cs
src2/Items/InventoryContainers/Identity/ItemDefinitionCatalog.cs
src2/Items/InventoryContainers/Identity/ItemFootprintComponent.cs
src2/Items/InventoryContainers/Identity/ItemIdentityAndStackComponent.cs
src2/Items/InventoryContainers/Identity/ItemStackSnapshot.cs
src2/Items/InventoryContainers/Projections/ItemContainerClientProjection.cs
src2/Items/InventoryContainers/Projections/ItemContainerClientSnapshot.cs
src2/Items/InventoryContainers/Projections/ItemContainerNetworkProjection.cs
src2/Items/InventoryContainers/Projections/ItemContainerNetworkSnapshot.cs
src2/Items/InventoryContainers/Projections/ItemContainerPersistenceProjection.cs
src2/Items/InventoryContainers/Projections/ItemContainerPersistenceSnapshot.cs
src2/Items/InventoryContainers/Projections/ItemPresentationColor.cs
src2/Items/InventoryContainers/Projections/WorldItemEconomyPayload.cs
src2/Items/InventoryContainers/Projections/WorldItemEconomyProjection.cs
src2/Items/InventoryContainers/Projections/WorldItemLifecycleClientProjection.cs
src2/Items/InventoryContainers/Projections/WorldItemLifecycleClientSnapshot.cs
src2/Items/InventoryContainers/Projections/WorldItemLifecycleNetworkProjection.cs
src2/Items/InventoryContainers/Projections/WorldItemLifecycleNetworkSnapshot.cs
src2/Items/InventoryContainers/Projections/WorldItemLifecyclePersistenceProjection.cs
src2/Items/InventoryContainers/Projections/WorldItemLifecyclePersistenceSnapshot.cs
src2/Items/InventoryContainers/Projections/WorldItemUsePresentationPayload.cs
src2/Items/InventoryContainers/Projections/WorldItemUsePresentationProjection.cs
src2/Items/InventoryContainers/Projections/WorldItemUsePresentationSource.cs
src2/Items/InventoryContainers/Queries/ItemDerivedQuery.cs
src2/Items/InventoryContainers/Queries/ItemDerivedValues.cs
src2/Items/InventoryContainers/QuickStacking/InventorySlotReference.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackCommitCommand.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackCommitResult.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackDestinationEligibility.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackDestinationReferenceSnapshot.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackDestinationSnapshot.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackPlan.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackPlannerScratchState.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackPlanQuery.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackSourceEligibility.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackSourcePositionSnapshot.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackSourceSlotReferenceSnapshot.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackSourceSlotSnapshot.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackSourceSnapshot.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackTransferIntent.cs
src2/Items/InventoryContainers/QuickStacking/QuickStackTypeIndexScratch.cs
src2/Items/InventoryContainers/Terraria.Items.InventoryContainers.csproj
src2/Items/InventoryContainers/Transfer/ItemTransferCommand.cs
src2/Items/InventoryContainers/Transfer/ItemTransferPolicyDefinition.cs
src2/Items/InventoryContainers/Transfer/ItemTransferPolicyPresetCatalog.cs
src2/Items/InventoryContainers/Transfer/ItemTransferPostAction.cs
src2/Items/InventoryContainers/Transfer/TransferCommitResult.cs
src2/Items/InventoryContainers/Transfer/TransferCompletionEffect.cs
src2/Items/InventoryContainers/Transfer/TransferCompletionEffectPort.cs
src2/Items/InventoryContainers/WorldItems/IWorldItemSceneMetricsPort.cs
src2/Items/InventoryContainers/WorldItems/WorldItemItemRelationAdapter.cs
src2/Items/InventoryContainers/WorldItems/WorldItemLifecycleComponent.cs
src2/Items/InventoryContainers/WorldItems/WorldItemLifecycleQuery.cs
src2/Items/InventoryContainers/WorldItems/WorldItemLifecycleSystem.cs
src2/Items/InventoryContainers/WorldItems/WorldItemLifecycleTick.cs
src2/Items/InventoryContainers/WorldItems/WorldItemSceneMetricsAdapter.cs
src2/Items/InventoryContainers/WorldItems/WorldItemSceneMetricsSnapshot.cs
src2/Items/InventoryContainersVerification/Program.cs
src2/Items/InventoryContainersVerification/Terraria.Items.InventoryContainersVerification.csproj
```

The implementation checkpoint also includes the focused assertions for item derivation, chest
capacity and adapters, Quick Stack purity/splitting/reference snapshots, transfer effect order,
WorldItem lifecycle and projections, equipment snapshots, Emergency Stack ownership/amount/
idempotency, and versioned container projections.

The missing first-round P09 report is a blocking evidence gap. This design therefore uses the
partition inventory, the Version4 files listed above, the common public-decomposition protocol,
the ECS evidence protocol, the local API mirror, and SS14 structural references; it does not
invent first-round reader/writer findings.

## Proposed Capability Topology

All rows in this table are proposed artifacts. The paths are design targets, not existing files.
The namespace prefix is a proposed namespace and requires integration review.

| Proposed boundary | Proposed type or file | Authority and responsibility |
|---|---|---|
| Chest slot authority | proposed src2/WorldStorage/Containers/Chest/ChestSlotStorageComponent.cs, proposed ChestSlotStorageComponent | Owns slot count and slot payload handles for one chest; only the proposed chest mutation system writes it. |
| Chest metadata and address | proposed src2/WorldStorage/Containers/Chest/ChestMetadataComponent.cs, proposed ChestMetadataComponent | Owns capacity/name/bank classification and the coordinate/index value needed by integration; persistent and entity identity remain unresolved. |
| Chest presentation | proposed src2/WorldStorage/Containers/Chest/ChestPresentationStateComponent.cs, proposed ChestPresentationStateComponent | Owns frame counter, frame, and eating animation state; no storage or registry writes. |
| Chest lookup and icon adapters | proposed src2/WorldStorage/Containers/Chest/ChestRegistryAdapter.cs, proposed ChestRegistryAdapter; proposed ChestIconCatalogAdapter.cs | Isolates coordinate lookup, current-use access tracking, static icon arrays, and external Point/Chest references. |
| Item identity and stack | proposed src2/Items/Identity/ItemIdentityAndStackComponent.cs, proposed ItemIdentityAndStackComponent | Owns content type, quantity, maximum, unique-stack rule, prefix, variant, favorited state, and name override subject to integration owner. |
| Item footprint | proposed src2/Items/Identity/ItemFootprintComponent.cs, proposed ItemFootprintComponent | Owns width/height physical or UI footprint, separate from content identity and quantity. |
| Item progression capability | proposed src2/Items/Capabilities/ItemProgressionInteractionCapabilityComponent.cs, proposed ItemProgressionInteractionCapabilityComponent | Owns quest, flame, mechanic, tile-wand, fishing, bait, NPC creation, and expert capability payloads. |
| Item effects capability | proposed src2/Items/Capabilities/ItemBuffMountConsumableCapabilityComponent.cs, proposed ItemBuffMountConsumableCapabilityComponent | Owns buff, mount/cart, extractinator, and DD2 summon payloads; effect application belongs to external systems. |
| Item derived queries | proposed src2/Items/Queries/ItemDerivedQuery.cs, proposed ItemDerivedQuery | Purely derives air/coin/name/original statistics from item state and catalogs; never writes item state. |
| Quick Stack plan | proposed src2/Items/QuickStacking/QuickStackPlan.cs, proposed QuickStackPlan; proposed source/destination snapshot and index types | Owns only operation-scoped snapshots, candidate ordering, blocked state, and type index scratch; the plan is not authoritative. |
| Transfer policy and effects | proposed src2/Items/Transfer/ItemTransferPolicyDefinition.cs, proposed ItemTransferPolicyDefinition; proposed TransferCompletionEffectPort.cs | Owns immutable transfer settings and routes post-slot effects through an explicit port. |
| World-item lifecycle | proposed src2/WorldObjects/Items/WorldItemLifecycleComponent.cs, proposed WorldItemLifecycleComponent; proposed relation and scene-metrics adapters | Owns reservation, pickup constraints, shimmer, age, conveyor/grab state, and keep time; item payload authority is separate. |
| Equipment appearance | proposed src2/Items/Equipment/ItemEquipmentAppearanceComponent.cs, proposed ItemEquipmentAppearanceComponent | Owns worn/social/vanity flags and appearance slot payloads; equipment container ownership is cross-partition. |
| World-item economy projection | proposed src2/WorldObjects/Items/WorldItemEconomyProjection.cs, proposed WorldItemEconomyProjection | Read projection of item identity, stack, favorited, value, max stack, rarity, name, coin, and air; no independent authority. |
| World-item use/presentation projection | proposed src2/WorldObjects/Items/WorldItemUsePresentationProjection.cs, proposed WorldItemUsePresentationProjection | Read projection of use, damage, placement, glow, color, alpha, buff, and expert payloads; no independent authority. |
| Emergency-stack policy | proposed src2/Items/EmergencyStacking/EmergencyStackingPolicyDefinition.cs, proposed EmergencyStackingPolicyDefinition | Owns group predicates, distance, priority, preservation policy, view bounds, and per-pass limits as definitions/configuration. |
| Emergency-stack transfer plan | proposed src2/Items/EmergencyStacking/EmergencyStackingTransferPlan.cs, proposed EmergencyStackingTransferPlan | Owns candidate and transfer records for one pass plus a command boundary; owns no durable item or world entity state. |
| Network/persistence/client output | proposed src2/Items/Projections/ItemContainerNetworkProjection.cs, proposed ItemContainerNetworkProjection; proposed persistence and client projection adapters | Converts authoritative snapshots to versioned external records; never writes domain components. |

The proposed Item and WorldItem boundaries intentionally use projections and adapters for
forwarding properties. A forwarding property is not evidence that the wrapper should own a
second copy of the field.

## Component Contracts And Lifecycle

### Proposed chest boundaries

ChestSlotStorageComponent has one writer: proposed ChestSlotMutationSystem. It reads a
container-local slot count and proposed item handles, validates capacity and stack transfer
commands, and emits a versioned mutation result. Initialization is proposed ChestCreateSystem
after a validated capacity; cleanup is proposed ChestDestroySystem with an explicit policy for
non-empty slots. WorldFile persistence and NetMessage synchronization are one-way projections
from the committed snapshot. No UI, network handler, or quick-stack query may write slots.

ChestMetadataComponent is written by proposed ChestCreateSystem, ChestRenameCommand, and the
integration-approved bank classification command. ChestRegistryAdapter owns the coordinate map
and current-use set. The adapter must reject stale index/coordinate entries and must not infer
slot authority from map membership. ChestPresentationStateComponent is written only by a
proposed ChestPresentationSystem, driven by committed access/use events; frame animation cannot
change slot contents.

The constants and static icon arrays are catalog/configuration inputs. They belong to the
proposed ChestIconCatalogAdapter or ChestCapacityPolicy, not a mutable per-chest component.
Capacity invariants are DefaultMaxItems <= maxItems <=
AbsoluteMaxItemsWeCanEverReachInAChestForNow, subject to the actual Version4 compatibility
decision. MaxNameLength is input validation policy, not an arbitrary UI truncation rule.

### Proposed item state and queries

ItemIdentityAndStackComponent is the only proposed writer-owned state for type, quantity,
maximum quantity, unique-stack behavior, prefix, variant, favorited, and name override. Item
stack commands validate non-negative quantity, maximum capacity, unique-stack rules, and type/
prefix compatibility. ItemFootprintComponent is updated by content-definition initialization
and only by a content/catalog synchronization boundary; transfer and stack queries must not
change it.

ItemDerivedQuery returns a value result for active/air, coin classification, localized name,
and original rarity/damage/defense. It reads ItemIdentityAndStackComponent and content catalogs.
It cannot mutate stack, prefix, or catalog state. Missing content entries return a typed
evidence-gap/invalid-content result rather than silently creating a default item.

ItemProgressionInteractionCapabilityComponent and ItemBuffMountConsumableCapabilityComponent
are capability payloads. They are initialized from content definitions and changed only by
content synchronization or an explicitly owned effect command. They do not spawn NPCs, apply
buffs, consume bait, place tiles, or alter player state directly; those effects cross into
integration-owned commands.

ItemEquipmentAppearanceComponent groups worn armor, armor/hand/accessory slot payloads,
social/vanity flags, new-and-shiny, and vanity-effect state. It is an appearance/equipment
projection boundary, not ownership of player equipment arrays. Equipment-slot identity and
player inventory references are crossSubsystemOwner: integration-review.

### Proposed transfer planning

Quick Stack snapshots copy the source inventory and destination candidates at the beginning of
one operation. Source slots, destination references, type-index entries, locked flags, and
transfer-blocked flags are immutable inputs to a pure plan query after capture. Scratch lists
and arrays are operation-local and must be cleared on completion, cancellation, or failure.

QuickStackPlanQuery returns ordered stack transfer intents and remaining unhandled sources. It
does not change any Item, Chest, Player, or WorldItem state. QuickStackCommitCommand submits
those intents to the integration-owned container/item mutation port. A stale revision, invalid
slot reference, ownership loss, or capacity mismatch rejects the whole plan or applies a
documented per-intent policy; partial commit behavior must be chosen by integration review.

ItemTransferPolicyDefinition stores the static presets and flags from GetItemSettings. The
StepAfterHandlingSlotNormally callback is not copied as a delegate inside a component. A
TransferCompletionEffectPort receives an explicit post-handling effect command with operation
identity and the committed item result. Effects are ordered after the slot mutation and are
idempotent or compensatable.

### Proposed WorldItem lifecycle and projections

WorldItemLifecycleComponent owns reservation/ownership, no-grab delay, shimmer timing,
instancing, ignore-owner, enemy pickup delay, spawn age, grabbing, conveyor, and keep-time.
WorldItemItemRelationAdapter represents the relation to the item payload without deciding a
persistent ItemInstanceId. WorldItemSceneMetricsAdapter isolates the static SceneMetrics cache
and any rendering or spatial dependencies.

WorldItemLifecycleSystem performs time-based transitions from an explicit clock/tick input.
WorldItemOwnershipQuery is pure and considers reservation, range, player eligibility, shimmer,
emergency-stack pending state, and the integration-provided inventory-space result. Pickup,
combination, release, and despawn are commands. TryCombiningIntoNearbyItems must become a
transactional WorldItemCombineCommand that validates both source and destination revisions,
mutates stacks through the item/container port, and emits network/presentation effects only
after commit.

WorldItemEconomyProjection exposes the identity/economy properties that currently forward to
inner or derive from it. WorldItemUsePresentationProjection exposes use and presentation
properties. Settable forwarding properties must become explicit commands to the underlying item
authority; read-only forwarding properties are projection fields. Neither projection may be
serialized as a second authoritative copy.

### Proposed Emergency Stack boundaries

EmergencyStackingPolicyDefinition contains group definitions, predicates, distances, priorities,
preservation order, player-view bounds, and per-pass limits. Predicates are registered policy
ports and are not serialized as arbitrary delegates. EmergencyStackingCandidateQuery reads
world-item lifecycle snapshots, item-derived queries, and player-view inputs and returns
transient candidate records.

EmergencyStackingTransferPlan contains source/destination references, ordering keys,
distance, ownership qualification, and transfer count calculation for one scheduler pass.
HasOwnership and NumToTransfer are pure queries over captured state; they do not mutate either
WorldItem. EmergencyStackingCommitCommand owns the mutation transaction and the network/update
effects. Pending transfer lists, per-type flags, and scratch rectangles are cleared after
commit, rejection, cancellation, or timeout.

## Complete Member To Proposed Role Mapping

The following table is the complete 191-row mapping. Source sequence and member spelling are
copied from the P09 input report. Each role is proposed; a role ending in Adapter, Query,
Projection, Plan, or Command is not an implementation claim.

### ChestContainerStorage, 23 members

| seq | member | proposed role |
|---:|---|---|
| 765 | chestStackRange | proposed ChestCapacityPolicy |
| 766 | maxChestTypes | proposed ChestIconCatalogAdapter |
| 767 | chestTypeToIcon | proposed ChestIconCatalogAdapter |
| 768 | maxChestTypes2 | proposed ChestIconCatalogAdapter |
| 769 | chestTypeToIcon2 | proposed ChestIconCatalogAdapter |
| 770 | maxDresserTypes | proposed ChestIconCatalogAdapter |
| 771 | dresserTypeToIcon | proposed ChestIconCatalogAdapter |
| 772 | DefaultMaxItems | proposed ChestCapacityPolicy |
| 773 | AbsoluteMaxItemsWeCanEverReachInAChestForNow | proposed ChestCapacityPolicy |
| 774 | maxItems | proposed ChestMetadataComponent |
| 775 | MaxNameLength | proposed ChestMetadataPolicy |
| 776 | item | proposed ChestSlotStorageComponent |
| 777 | x | proposed ChestMetadataComponent |
| 778 | y | proposed ChestMetadataComponent |
| 779 | index | proposed ChestMetadataComponent with proposed ChestRegistryAdapter boundary |
| 780 | bankChest | proposed ChestMetadataComponent |
| 781 | name | proposed ChestMetadataComponent |
| 782 | frameCounter | proposed ChestPresentationStateComponent |
| 783 | frame | proposed ChestPresentationStateComponent |
| 784 | eatingAnimationTime | proposed ChestPresentationStateComponent |
| 785 | _itemsGotSet | proposed ChestInitializationState |
| 786 | _chestsByCoords | proposed ChestRegistryAdapter |
| 787 | _chestInUse | proposed ChestAccessTrackerAdapter |

### SharedItemIdentityAndStackState, 14 members

| seq | member | proposed role |
|---:|---|---|
| 3138 | width | proposed ItemFootprintComponent |
| 3139 | height | proposed ItemFootprintComponent |
| 3144 | _nameOverride | proposed ItemIdentityAndStackComponent |
| 3189 | type | proposed ItemIdentityAndStackComponent |
| 3190 | favorited | proposed ItemIdentityAndStackComponent |
| 3197 | stack | proposed ItemIdentityAndStackComponent |
| 3198 | maxStack | proposed ItemIdentityAndStackComponent |
| 3262 | uniqueStack | proposed ItemIdentityAndStackComponent |
| 3271 | prefix | proposed ItemIdentityAndStackComponent |
| 3961 | active | proposed ItemDerivedQuery |
| 3962 | Name | proposed ItemDerivedQuery |
| 3967 | Variant | proposed ItemIdentityAndStackComponent |
| 3968 | IsACoin | proposed ItemDerivedQuery |
| 3969 | IsAir | proposed ItemDerivedQuery |

### SharedItemProgressionAndWorldInteractionState, 9 members

| seq | member | proposed role |
|---:|---|---|
| 3167 | questItem | proposed ItemProgressionInteractionCapabilityComponent |
| 3173 | flame | proposed ItemProgressionInteractionCapabilityComponent |
| 3174 | mech | proposed ItemProgressionInteractionCapabilityComponent |
| 3175 | tileWand | proposed ItemProgressionInteractionCapabilityComponent |
| 3180 | fishingPole | proposed ItemProgressionInteractionCapabilityComponent |
| 3181 | bait | proposed ItemProgressionInteractionCapabilityComponent |
| 3182 | makeNPC | proposed ItemProgressionInteractionCapabilityComponent |
| 3183 | expertOnly | proposed ItemProgressionInteractionCapabilityComponent |
| 3184 | expert | proposed ItemProgressionInteractionCapabilityComponent |

### SharedItemBuffMountAndConsumableEffects, 6 members

| seq | member | proposed role |
|---:|---|---|
| 3258 | buffType | proposed ItemBuffMountConsumableCapabilityComponent |
| 3259 | buffTime | proposed ItemBuffMountConsumableCapabilityComponent |
| 3260 | mountType | proposed ItemBuffMountConsumableCapabilityComponent |
| 3261 | cartTrack | proposed ItemBuffMountConsumableCapabilityComponent |
| 3266 | chlorophyteExtractinatorConsumable | proposed ItemBuffMountConsumableCapabilityComponent |
| 3267 | DD2Summon | proposed ItemBuffMountConsumableCapabilityComponent |

### SharedItemDerivedQueries, 3 members

| seq | member | proposed role |
|---:|---|---|
| 3964 | OriginalRarity | proposed ItemDerivedQuery |
| 3965 | OriginalDamage | proposed ItemDerivedQuery |
| 3966 | OriginalDefense | proposed ItemDerivedQuery |

### ItemQuickStackingState, 17 members

| seq | member | proposed role |
|---:|---|---|
| 2487 | _chest | proposed QuickStackDestinationReferenceSnapshot |
| 2488 | items | proposed QuickStackDestinationSnapshot |
| 2489 | itemCount | proposed QuickStackDestinationSnapshot |
| 2490 | locked | proposed QuickStackDestinationEligibility |
| 2491 | transferBlocked | proposed QuickStackDestinationEligibility |
| 2492 | value | proposed QuickStackTypeIndexScratch |
| 2493 | next | proposed QuickStackTypeIndexScratch |
| 2494 | entries | proposed QuickStackTypeIndexScratch |
| 2495 | firstEntryForType | proposed QuickStackTypeIndexScratch |
| 2496 | items | proposed QuickStackSourceSnapshot |
| 2497 | numItems | proposed QuickStackSourceSnapshot |
| 2498 | slots | proposed QuickStackSourceSlotReferenceSnapshot; crossSubsystemOwner: integration-review |
| 2499 | transferBlocked | proposed QuickStackSourceEligibility |
| 2500 | position | proposed QuickStackSourcePositionSnapshot |
| 2501 | destHelperListScratch | proposed QuickStackPlannerScratchState |
| 2502 | matchingItemTypeScratch | proposed QuickStackPlannerScratchState |
| 2503 | _blockedChests | proposed QuickStackPlannerScratchState |

### ItemTransferSettings, 16 members

| seq | member | proposed role |
|---:|---|---|
| 3090 | GiftRecieved | proposed ItemTransferPolicyPresetCatalog |
| 3091 | LootAllFromBank | proposed ItemTransferPolicyPresetCatalog |
| 3092 | LootAllFromChest | proposed ItemTransferPolicyPresetCatalog |
| 3093 | PickupItemFromWorld | proposed ItemTransferPolicyPresetCatalog |
| 3094 | QuickTransferFromSlot | proposed ItemTransferPolicyPresetCatalog |
| 3095 | ReturnItemFromSlot | proposed ItemTransferPolicyPresetCatalog |
| 3096 | ReturnItemShowAsNew | proposed ItemTransferPolicyPresetCatalog |
| 3097 | ItemCreatedFromItemUsage | proposed ItemTransferPolicyPresetCatalog |
| 3098 | RefundConsumedItem | proposed ItemTransferPolicyPresetCatalog |
| 3099 | ReturnItemShowAsNewNoCoinMerge | proposed ItemTransferPolicyPresetCatalog |
| 3100 | LongText | proposed ItemTransferPolicyDefinition |
| 3101 | NoText | proposed ItemTransferPolicyDefinition |
| 3102 | CanGoIntoVoidVault | proposed ItemTransferPolicyDefinition |
| 3103 | NoSound | proposed ItemTransferPolicyDefinition |
| 3104 | NoCoinMerge | proposed ItemTransferPolicyDefinition |
| 3105 | StepAfterHandlingSlotNormally | proposed TransferCompletionEffectPort |

### SharedWorldItemLifecycleState, 16 members

| seq | member | proposed role |
|---:|---|---|
| 3667 | inner | proposed WorldItemItemRelationAdapter; crossSubsystemOwner: integration-review |
| 3668 | ownTime | proposed WorldItemReservationLifecycleComponent |
| 3669 | playerIndexTheItemIsReservedFor | proposed WorldItemReservationLifecycleComponent; crossSubsystemOwner: integration-review |
| 3670 | noGrabDelay | proposed WorldItemPickupConstraintComponent |
| 3671 | shimmered | proposed WorldItemShimmerComponent |
| 3672 | shimmerTime | proposed WorldItemShimmerComponent |
| 3673 | instanced | proposed WorldItemPickupConstraintComponent |
| 3674 | ownIgnore | proposed WorldItemReservationLifecycleComponent; crossSubsystemOwner: integration-review |
| 3675 | timeSinceTheItemHasBeenReservedForSomeone | proposed WorldItemReservationLifecycleComponent |
| 3676 | timeLeftInWhichTheItemCannotBeTakenByEnemies | proposed WorldItemPickupConstraintComponent |
| 3677 | timeSinceItemSpawned | proposed WorldItemAgingComponent |
| 3678 | beingGrabbed | proposed WorldItemPickupConstraintComponent |
| 3679 | onConveyor | proposed WorldItemMovementStateComponent |
| 3680 | keepTime | proposed WorldItemAgingComponent |
| 3681 | _sceneMetrics | proposed WorldItemSceneMetricsAdapter |
| 4025 | active | proposed WorldItemLifecycleQuery |

### SharedItemEquipmentSlotState, 21 members

| seq | member | proposed role |
|---:|---|---|
| 3176 | wornArmor | proposed ItemEquipmentAppearanceComponent |
| 3221 | headSlot | proposed ItemEquipmentAppearanceComponent |
| 3222 | bodySlot | proposed ItemEquipmentAppearanceComponent |
| 3223 | legSlot | proposed ItemEquipmentAppearanceComponent |
| 3224 | handOnSlot | proposed ItemEquipmentAppearanceComponent |
| 3225 | handOffSlot | proposed ItemEquipmentAppearanceComponent |
| 3226 | backSlot | proposed ItemEquipmentAppearanceComponent |
| 3227 | frontSlot | proposed ItemEquipmentAppearanceComponent |
| 3228 | shoeSlot | proposed ItemEquipmentAppearanceComponent |
| 3229 | waistSlot | proposed ItemEquipmentAppearanceComponent |
| 3230 | wingSlot | proposed ItemEquipmentAppearanceComponent |
| 3231 | shieldSlot | proposed ItemEquipmentAppearanceComponent |
| 3232 | neckSlot | proposed ItemEquipmentAppearanceComponent |
| 3233 | faceSlot | proposed ItemEquipmentAppearanceComponent |
| 3234 | balloonSlot | proposed ItemEquipmentAppearanceComponent |
| 3235 | beardSlot | proposed ItemEquipmentAppearanceComponent |
| 3236 | voiceSlot | proposed ItemEquipmentAppearanceComponent |
| 3254 | social | proposed ItemEquipmentAppearanceComponent |
| 3255 | vanity | proposed ItemEquipmentAppearanceComponent |
| 3278 | newAndShiny | proposed ItemEquipmentAppearanceComponent |
| 3279 | hasVanityEffects | proposed ItemEquipmentAppearanceComponent |

### SharedWorldItemIdentityAndEconomyPayloadState, 9 members

| seq | member | proposed role |
|---:|---|---|
| 4026 | type | proposed WorldItemEconomyProjection and explicit ItemMutationCommand bridge |
| 4027 | stack | proposed WorldItemEconomyProjection and explicit ItemMutationCommand bridge |
| 4030 | favorited | proposed WorldItemEconomyProjection and explicit ItemMutationCommand bridge |
| 4032 | value | proposed WorldItemEconomyProjection |
| 4036 | maxStack | proposed WorldItemEconomyProjection |
| 4044 | rare | proposed WorldItemEconomyProjection |
| 4049 | Name | proposed WorldItemEconomyProjection |
| 4052 | IsACoin | proposed WorldItemEconomyProjection |
| 4053 | IsAir | proposed WorldItemEconomyProjection |

### SharedWorldItemUseAndPresentationPayloadState, 19 members

| seq | member | proposed role |
|---:|---|---|
| 4028 | newAndShiny | proposed WorldItemUsePresentationProjection and explicit ItemMutationCommand bridge |
| 4029 | color | proposed WorldItemUsePresentationProjection and explicit ItemMutationCommand bridge |
| 4031 | makeNPC | proposed WorldItemUsePresentationProjection and explicit ItemMutationCommand bridge |
| 4033 | useTime | proposed WorldItemUsePresentationProjection |
| 4034 | useAnimation | proposed WorldItemUsePresentationProjection |
| 4035 | useAmmo | proposed WorldItemUsePresentationProjection |
| 4037 | damage | proposed WorldItemUsePresentationProjection |
| 4038 | knockBack | proposed WorldItemUsePresentationProjection |
| 4039 | shootSpeed | proposed WorldItemUsePresentationProjection |
| 4040 | scale | proposed WorldItemUsePresentationProjection |
| 4041 | ammo | proposed WorldItemUsePresentationProjection |
| 4042 | notAmmo | proposed WorldItemUsePresentationProjection |
| 4043 | shoot | proposed WorldItemUsePresentationProjection |
| 4045 | placeStyle | proposed WorldItemUsePresentationProjection |
| 4046 | createTile | proposed WorldItemUsePresentationProjection |
| 4047 | glowMask | proposed WorldItemUsePresentationProjection |
| 4048 | expert | proposed WorldItemUsePresentationProjection |
| 4050 | alpha | proposed WorldItemUsePresentationProjection |
| 4051 | buffType | proposed WorldItemUsePresentationProjection |

### SharedItemEmergencyStackingPolicyState, 6 members

| seq | member | proposed role |
|---:|---|---|
| 2403 | PreservationOrder | proposed EmergencyStackingPolicyDefinition |
| 2404 | PlayerViewRectSize | proposed EmergencyStackingPolicyDefinition |
| 2405 | ItemsToStackEachTime | proposed EmergencyStackingPolicyDefinition |
| 2406 | PendingTransfers | proposed EmergencyStackingSchedulerState |
| 2407 | HasPendingTransfer | proposed EmergencyStackingSchedulerState |
| 2408 | playerViewRectsScratch | proposed EmergencyStackingSchedulerState |

### SharedItemEmergencyStackingTransferState, 21 members

| seq | member | proposed role |
|---:|---|---|
| 2384 | DefaultStackDistanceStepSize | proposed EmergencyStackingPolicyDefinition |
| 2385 | DistanceStepSize | proposed EmergencyStackingPolicyDefinition |
| 2386 | Conditions | proposed EmergencyStackingPredicateCatalog |
| 2387 | StackingPriority | proposed EmergencyStackingPolicyDefinition |
| 2388 | FallenStars | proposed EmergencyStackingGroupDefinition |
| 2389 | CopperCoins | proposed EmergencyStackingGroupDefinition |
| 2390 | SilverCoins | proposed EmergencyStackingGroupDefinition |
| 2391 | Equipment | proposed EmergencyStackingGroupDefinition |
| 2392 | RareCurrency | proposed EmergencyStackingGroupDefinition |
| 2393 | Default | proposed EmergencyStackingGroupDefinition |
| 2394 | type | proposed EmergencyStackingCandidateSnapshot |
| 2395 | age | proposed EmergencyStackingCandidateSnapshot |
| 2396 | isOnScreen | proposed EmergencyStackingCandidateSnapshot |
| 2397 | item | proposed EmergencyStackingCandidateSnapshot; crossSubsystemOwner: integration-review |
| 2398 | src | proposed EmergencyStackingTransferPlan; crossSubsystemOwner: integration-review |
| 2399 | dst | proposed EmergencyStackingTransferPlan; crossSubsystemOwner: integration-review |
| 2400 | distanceOrder | proposed EmergencyStackingTransferPlan |
| 2401 | preservationOrder | proposed EmergencyStackingTransferPlan |
| 2402 | distance | proposed EmergencyStackingTransferPlan |
| 3851 | HasOwnership | proposed EmergencyStackingOwnershipQuery |
| 3852 | NumToTransfer | proposed EmergencyStackingTransferAmountQuery |

### SharedItemToolPlacementCapabilityState, 11 members

| seq | member | proposed role |
|---:|---|---|
| 3199 | pick | proposed ItemToolPlacementCapabilityComponent |
| 3200 | axe | proposed ItemToolPlacementCapabilityComponent |
| 3201 | hammer | proposed ItemToolPlacementCapabilityComponent |
| 3202 | tileBoost | proposed ItemToolPlacementCapabilityComponent |
| 3203 | createTile | proposed ItemToolPlacementCapabilityComponent |
| 3204 | createWall | proposed ItemToolPlacementCapabilityComponent |
| 3205 | placeStyle | proposed ItemToolPlacementCapabilityComponent |
| 3243 | ammo | proposed ItemToolPlacementCapabilityComponent |
| 3244 | notAmmo | proposed ItemToolPlacementCapabilityComponent |
| 3245 | useAmmo | proposed ItemToolPlacementCapabilityComponent |
| 3256 | material | proposed ItemToolPlacementCapabilityComponent |

Coverage invariant: 14 groups, 191 unique source sequences, 152 fields, and 39 properties are
represented above. The source report remains the authority for declaration text and type details.

## Dependency Direction And System Order

The proposed dependency direction is:

content/catalog definitions -> item/container authority -> pure queries and snapshots ->
operation plans -> commands and mutation ports -> committed events -> network/persistence/client
projections.

The proposed order is an integration contract candidate, not a runtime claim:

1. Proposed ContentDefinitionLoadSystem validates item capability and icon/capacity catalogs.
2. Proposed ChestCreateSystem and ItemInstanceInitializationSystem create validated authority
   state without assigning unresolved persistent IDs.
3. Proposed WorldItemLifecycleSystem advances time and reservation state from an explicit clock.
4. Proposed ItemDerivedQuery and WorldItemOwnershipQuery evaluate pure eligibility from captured
   snapshots.
5. Proposed QuickStackPlanQuery and EmergencyStackingCandidateQuery create operation-local plans.
6. Proposed QuickStackCommitCommand and EmergencyStackingCommitCommand validate revisions,
   reserve authority, and commit stack mutations.
7. Proposed WorldItemPickupOrCombineCommand applies item/world-result ownership through integration
   ports, with explicit rollback/compensation outcomes.
8. Proposed ChestPresentationSystem and effect ports consume committed events after authority
   writes; they cannot feed back through hidden setters.
9. Proposed NetworkProjectionSystem, PersistenceProjectionSystem, and ClientViewProjectionSystem
   serialize committed snapshots in their own retry domains.

No sequence number in the source inventory overrides this order. A global scheduler owner and
cross-domain phase barriers are crossSubsystemOwner: integration-review.

## IDs, References, And External Objects

The proposal distinguishes these unresolved concepts:

- Item content/type key: identifies a catalog entry and is not an item instance identity.
- Runtime entity reference: identifies an in-process WorldItem, Player, Chest, or other entity.
- Item instance reference: relates a WorldItem or container slot to an item payload; the persistent
  form is not chosen here.
- Slot reference: identifies a player/chest/bank slot and must carry a container boundary, not just
  an integer.
- Container reference: identifies a chest, bank, player inventory, or tile-entity store; the
  final owner is crossSubsystemOwner: integration-review.
- Persistent ID, network ID, and external/platform ID: separate namespaces and adapters.

No proposed component stores a network ID as a content type, uses an array index as a persistent
identity, or serializes a live Terraria object/reference. Proposed snapshot value objects must
carry an explicit schema/version and owner decision before implementation.

## Network, Persistence, And Client Projection Boundaries

Network projections may include versioned chest slot deltas, item identity/stack/prefix changes,
WorldItem lifecycle/reservation changes, and explicit transfer accept/reject results. They must
exclude live Item/WorldItem/Chest references, delegates, scratch arrays, static caches, and
unresolved identity assumptions. MessageBuffer and NetMessage adapters own protocol encoding;
domain commands own validation and commits.

Persistence projections may include chest coordinates, name, capacity, bank classification,
committed slot payloads, item identity/stack/prefix/variant state, and durable WorldItem state
only when integration assigns its lifetime. They must not persist Quick Stack scratch, emergency
candidate/transfer lists, scene metrics, animation-only state, or temporary ownership views
unless a separately approved save contract says so.

Client/UI projections may include chest presentation frames, item names, rarity/footprint,
WorldItem color/glow/alpha and transfer feedback. They are read-only projections. UI input is
converted into commands and cannot write components or invoke legacy setters directly.

Protocol schema ownership, snapshot versioning, persistence migration keys, reconnect semantics,
client/server authority, and projection retry policy are all crossSubsystemOwner:
integration-review.

## Evidence Gaps And Blocking Decisions

The first-round P09 report is absent, so no first-round conclusions about hidden readers/writers
or existing NLTX architecture are imported. The input report's declared upstream hash and the
observed current report hash differ; source lineage must be reconciled.

The following Version4 behavior is not closed by the evidence used here:

- QuickStacking.QuickStackToNearbyChests, ReadNetInventory, and WriteBlockedChestList.
- EmergencyStacking.MemoStackableItems, FindBestTransfers, DoTransfer, and
  RequestOwnershipReleaseForPendingTransfers.
- Player.PickupItem and Player.GetItem helper paths for Void Vault and slot handling.
- Full transfer ordering, rollback, ownership loss, network acknowledgement, and failure policy.

Integration review must decide:

- the authoritative owner and revision model for every inventory/container/slot mutation;
- whether WorldItem.inner becomes a relation to a canonical item payload or a compatibility
  snapshot, without inventing a persistent ID in P09;
- ownership of equipment, bank, Void Vault, tile-entity, currency, and world-result operations;
- command idempotency keys, reservation expiry, partial-transfer and compensation behavior;
- network/persistence/client schemas and their version/migration/retry owners;
- the global system order and cross-partition phase barriers.

## Focused Verifier Plan And Evidence

The following focused verifier was run against the src2 implementation checkpoint. It is not a
behavior-equivalence or production-integration verifier:

```text
Command: pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build','.\src2\Items\InventoryContainersVerification\Terraria.Items.InventoryContainersVerification.csproj','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')
Project: src2/Items/InventoryContainersVerification/Terraria.Items.InventoryContainersVerification.csproj
Exit code: 0
Warnings: 0
Errors: 0
Artifacts: Build/bin/Terraria.Items.InventoryContainers/Debug/net10.0/Terraria.Items.InventoryContainers.dll; Build/bin/Terraria.Items.InventoryContainersVerification/Debug/net10.0/Terraria.Items.InventoryContainersVerification.dll

Command: pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run','--project','.\src2\Items\InventoryContainersVerification\Terraria.Items.InventoryContainersVerification.csproj','--no-build','--no-restore','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')
Project: src2/Items/InventoryContainersVerification/Terraria.Items.InventoryContainersVerification.csproj
Exit code: 0
Output: P09 verifier passed.
```

The red-green cycle also recorded a deliberate failing run for the Quick Stack partial-capacity
case (exit code 1, expected assertion failure) before the production fix was applied. The focused
plan for later integration work remains:

1. Inventory coverage verifier parses the P09 report and both second-round documents, asserts 14
   unique groups, 191 unique source sequences, 152 fields, 39 properties, and no out-of-scope
   member.
2. Static boundary verifier rejects proposed queries with mutation ports, direct projection
   writes, persistent IDs in item payload components, live Terraria references in snapshots, and
   duplicate authorities for WorldItem forwarding properties.
3. Component invariant tests cover stack bounds, unique-stack behavior, prefix/variant handling,
   chest capacity/name validation, empty-slot semantics, and item-derived query purity.
4. Plan tests cover deterministic Quick Stack and Emergency Stack candidate ordering, stale
   revision rejection, blocked/locked destination behavior, ownership loss, and no mutation
   before commit.
5. Command/port tests cover atomic chest/world-item transfer, idempotent retries, compensation
   after downstream failure, callback/effect ordering, and no duplicate network/persistence
   projection.
6. Protocol/persistence tests cover versioned slot/item/lifecycle projections and exclude
   delegates, live references, static caches, and transient scratch.
7. Integration tests cover explicit scheduler order and single-writer enforcement.
8. Build and run evidence above covers only the src2 project and focused verifier. It does not
   cover production src integration, protocol closure, persistence recovery, scheduler ordering,
   or behavior equivalence.

## Integration Handoff

This P09 proposal hands off the following candidate seams to integration-review:

- container/slot and item-payload authority, revision and transaction model;
- entity/item/container/slot/persistent/network/external reference types;
- player inventory, equipment, bank, Void Vault, tile-entity storage, currency, and world-result
  owners;
- transfer reservation, ownership release, partial commit, rollback, retry, and idempotency;
- WorldItem lifecycle time source, spatial query, scene metrics, and pickup/combination commit;
- network/persistence/client projection schemas and migration ownership;
- global scheduler order and barriers around inventory, world items, networking, persistence, and
  UI.

The proposed file layout groups by capability: WorldStorage/Containers/Chest, Items/Identity,
Items/Capabilities, Items/QuickStacking, Items/Transfer, Items/Equipment,
Items/EmergencyStacking, and WorldObjects/Items. It does not create a generic shared component
directory. One core public type per same-named proposed file and the repository ECS naming/file
constraints remain required at implementation time.

## Explicit Non-Claims

The src2 checkpoint is not a production src/ migration, component registration, network or
persistence closure, runtime scheduler change, API compatibility result, or behavior-equivalence
proof. The implementation does not yet provide an atomic multi-container transfer port or a
WorldItem pickup/combine command. The missing first-round report, hash discrepancy, incomplete
Version4 methods, and unresolved cross-partition owners remain open.
