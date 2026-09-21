# P09 Proposed Component Execution Plan: Item, Inventory And Containers

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
  `06b62f1ec1a14935b94fe5661c830bb2`; this document and its companion design document are bound
  to that session.
- Existing src2 checkpoint files were retained as implementation evidence; no production `src/`
  file, ledger, or lock file was modified by the rebinding.

## Execution Boundary

This is a non-authoritative execution record with a completed src2 implementation checkpoint.
It translates the companion proposed design into staged source evidence while retaining the
implementation plan for deferred integration work. The checkpoint does not move production C#,
register runtime components, alter the scheduler, or assert behavior equivalence. Target paths
marked proposed below are design targets; the actual src2 files are listed separately.

The execution plan is deliberately staged around authority and side effects:

catalog and policy inputs -> authoritative item/container state -> pure snapshots and queries ->
transfer plans -> explicit commands and ports -> committed events -> network/persistence/client
projections.

The source inventory contains 14 groups and 191 members. Its sequence numbers are mapping keys,
not implementation or scheduler order.

## Proposed Target Layout

The following are proposed target paths under the capability-first layout. They must not be
treated as existing files:

| Proposed directory | Proposed target types | Responsibility |
|---|---|---|
| proposed src2/WorldStorage/Containers/Chest | ChestSlotStorageComponent, ChestMetadataComponent, ChestPresentationStateComponent, ChestCapacityPolicy, ChestMetadataPolicy | Chest contents, metadata, capacity, and presentation state. |
| proposed src2/WorldStorage/Containers/Chest/Adapters | ChestRegistryAdapter, ChestAccessTrackerAdapter, ChestIconCatalogAdapter | Coordinate/index lookup, access tracking, and static icon catalog isolation. |
| proposed src2/Items/Identity | ItemIdentityAndStackComponent, ItemFootprintComponent | Content identity, quantity/stack, variation, favorited state, name override, and footprint. |
| proposed src2/Items/Capabilities | ItemProgressionInteractionCapabilityComponent, ItemBuffMountConsumableCapabilityComponent, ItemToolPlacementCapabilityComponent | Content capability payloads without directly applying effects. |
| proposed src2/Items/Queries | ItemDerivedQuery, WorldItemLifecycleQuery, WorldItemOwnershipQuery | Pure item/world-item-derived decisions. |
| proposed src2/Items/QuickStacking | QuickStackPlan, QuickStackDestinationSnapshot, QuickStackSourceSnapshot, QuickStackTypeIndexScratch, QuickStackCommitCommand | Operation-local planning and explicit commit boundary. |
| proposed src2/Items/Transfer | ItemTransferPolicyDefinition, ItemTransferPolicyPresetCatalog, TransferCompletionEffectPort, ItemTransferCommand | Immutable policies and explicit effect/transfer ports. |
| proposed src2/Items/Equipment | ItemEquipmentAppearanceComponent | Equipment and appearance payloads, not player equipment ownership. |
| proposed src2/Items/EmergencyStacking | EmergencyStackingPolicyDefinition, EmergencyStackingPredicateCatalog, EmergencyStackingCandidateSnapshot, EmergencyStackingTransferPlan, EmergencyStackingCommitCommand, EmergencyStackingSchedulerState | Emergency-stack policy, transient candidate/transfer state, and commit. |
| proposed src2/WorldObjects/Items | WorldItemLifecycleComponent, WorldItemItemRelationAdapter, WorldItemSceneMetricsAdapter, WorldItemEconomyProjection, WorldItemUsePresentationProjection | World-item lifecycle and read projections. |
| proposed src2/Items/Projections | ItemContainerNetworkProjection, ItemContainerPersistenceProjection, ItemContainerClientProjection | One-way versioned external projections. |

The exact namespace and project placement require repository architecture review before any C#
work. One core public type per same-named file, component naming constraints, and 2-space C#
formatting from the repository guide apply when implementation is authorized.

## Source Member To Proposed Target Mapping

This is the complete source-to-target mapping for the execution plan. Each row is proposed and
must be checked against the input report again immediately before implementation. The target
role/file does not mean that a file currently exists.

### ChestContainerStorage, 23 members

| seq | member | proposed target role/file |
|---:|---|---|
| 765 | chestStackRange | proposed ChestCapacityPolicy.cs |
| 766 | maxChestTypes | proposed ChestIconCatalogAdapter.cs |
| 767 | chestTypeToIcon | proposed ChestIconCatalogAdapter.cs |
| 768 | maxChestTypes2 | proposed ChestIconCatalogAdapter.cs |
| 769 | chestTypeToIcon2 | proposed ChestIconCatalogAdapter.cs |
| 770 | maxDresserTypes | proposed ChestIconCatalogAdapter.cs |
| 771 | dresserTypeToIcon | proposed ChestIconCatalogAdapter.cs |
| 772 | DefaultMaxItems | proposed ChestCapacityPolicy.cs |
| 773 | AbsoluteMaxItemsWeCanEverReachInAChestForNow | proposed ChestCapacityPolicy.cs |
| 774 | maxItems | proposed ChestMetadataComponent.cs |
| 775 | MaxNameLength | proposed ChestMetadataPolicy.cs |
| 776 | item | proposed ChestSlotStorageComponent.cs |
| 777 | x | proposed ChestMetadataComponent.cs |
| 778 | y | proposed ChestMetadataComponent.cs |
| 779 | index | proposed ChestMetadataComponent.cs and proposed ChestRegistryAdapter.cs seam |
| 780 | bankChest | proposed ChestMetadataComponent.cs |
| 781 | name | proposed ChestMetadataComponent.cs |
| 782 | frameCounter | proposed ChestPresentationStateComponent.cs |
| 783 | frame | proposed ChestPresentationStateComponent.cs |
| 784 | eatingAnimationTime | proposed ChestPresentationStateComponent.cs |
| 785 | _itemsGotSet | proposed ChestInitializationState.cs |
| 786 | _chestsByCoords | proposed ChestRegistryAdapter.cs |
| 787 | _chestInUse | proposed ChestAccessTrackerAdapter.cs |

### SharedItemIdentityAndStackState, 14 members

| seq | member | proposed target role/file |
|---:|---|---|
| 3138 | width | proposed ItemFootprintComponent.cs |
| 3139 | height | proposed ItemFootprintComponent.cs |
| 3144 | _nameOverride | proposed ItemIdentityAndStackComponent.cs |
| 3189 | type | proposed ItemIdentityAndStackComponent.cs |
| 3190 | favorited | proposed ItemIdentityAndStackComponent.cs |
| 3197 | stack | proposed ItemIdentityAndStackComponent.cs |
| 3198 | maxStack | proposed ItemIdentityAndStackComponent.cs |
| 3262 | uniqueStack | proposed ItemIdentityAndStackComponent.cs |
| 3271 | prefix | proposed ItemIdentityAndStackComponent.cs |
| 3961 | active | proposed ItemDerivedQuery.cs |
| 3962 | Name | proposed ItemDerivedQuery.cs |
| 3967 | Variant | proposed ItemIdentityAndStackComponent.cs |
| 3968 | IsACoin | proposed ItemDerivedQuery.cs |
| 3969 | IsAir | proposed ItemDerivedQuery.cs |

### SharedItemProgressionAndWorldInteractionState, 9 members

| seq | member | proposed target role/file |
|---:|---|---|
| 3167 | questItem | proposed ItemProgressionInteractionCapabilityComponent.cs |
| 3173 | flame | proposed ItemProgressionInteractionCapabilityComponent.cs |
| 3174 | mech | proposed ItemProgressionInteractionCapabilityComponent.cs |
| 3175 | tileWand | proposed ItemProgressionInteractionCapabilityComponent.cs |
| 3180 | fishingPole | proposed ItemProgressionInteractionCapabilityComponent.cs |
| 3181 | bait | proposed ItemProgressionInteractionCapabilityComponent.cs |
| 3182 | makeNPC | proposed ItemProgressionInteractionCapabilityComponent.cs |
| 3183 | expertOnly | proposed ItemProgressionInteractionCapabilityComponent.cs |
| 3184 | expert | proposed ItemProgressionInteractionCapabilityComponent.cs |

### SharedItemBuffMountAndConsumableEffects, 6 members

| seq | member | proposed target role/file |
|---:|---|---|
| 3258 | buffType | proposed ItemBuffMountConsumableCapabilityComponent.cs |
| 3259 | buffTime | proposed ItemBuffMountConsumableCapabilityComponent.cs |
| 3260 | mountType | proposed ItemBuffMountConsumableCapabilityComponent.cs |
| 3261 | cartTrack | proposed ItemBuffMountConsumableCapabilityComponent.cs |
| 3266 | chlorophyteExtractinatorConsumable | proposed ItemBuffMountConsumableCapabilityComponent.cs |
| 3267 | DD2Summon | proposed ItemBuffMountConsumableCapabilityComponent.cs |

### SharedItemDerivedQueries, 3 members

| seq | member | proposed target role/file |
|---:|---|---|
| 3964 | OriginalRarity | proposed ItemDerivedQuery.cs |
| 3965 | OriginalDamage | proposed ItemDerivedQuery.cs |
| 3966 | OriginalDefense | proposed ItemDerivedQuery.cs |

### ItemQuickStackingState, 17 members

| seq | member | proposed target role/file |
|---:|---|---|
| 2487 | _chest | proposed QuickStackDestinationReferenceSnapshot.cs |
| 2488 | items | proposed QuickStackDestinationSnapshot.cs |
| 2489 | itemCount | proposed QuickStackDestinationSnapshot.cs |
| 2490 | locked | proposed QuickStackDestinationEligibility.cs |
| 2491 | transferBlocked | proposed QuickStackDestinationEligibility.cs |
| 2492 | value | proposed QuickStackTypeIndexScratch.cs |
| 2493 | next | proposed QuickStackTypeIndexScratch.cs |
| 2494 | entries | proposed QuickStackTypeIndexScratch.cs |
| 2495 | firstEntryForType | proposed QuickStackTypeIndexScratch.cs |
| 2496 | items | proposed QuickStackSourceSnapshot.cs |
| 2497 | numItems | proposed QuickStackSourceSnapshot.cs |
| 2498 | slots | proposed QuickStackSourceSlotReferenceSnapshot.cs; crossSubsystemOwner: integration-review |
| 2499 | transferBlocked | proposed QuickStackSourceEligibility.cs |
| 2500 | position | proposed QuickStackSourcePositionSnapshot.cs |
| 2501 | destHelperListScratch | proposed QuickStackPlannerScratchState.cs |
| 2502 | matchingItemTypeScratch | proposed QuickStackPlannerScratchState.cs |
| 2503 | _blockedChests | proposed QuickStackPlannerScratchState.cs |

### ItemTransferSettings, 16 members

| seq | member | proposed target role/file |
|---:|---|---|
| 3090 | GiftRecieved | proposed ItemTransferPolicyPresetCatalog.cs |
| 3091 | LootAllFromBank | proposed ItemTransferPolicyPresetCatalog.cs |
| 3092 | LootAllFromChest | proposed ItemTransferPolicyPresetCatalog.cs |
| 3093 | PickupItemFromWorld | proposed ItemTransferPolicyPresetCatalog.cs |
| 3094 | QuickTransferFromSlot | proposed ItemTransferPolicyPresetCatalog.cs |
| 3095 | ReturnItemFromSlot | proposed ItemTransferPolicyPresetCatalog.cs |
| 3096 | ReturnItemShowAsNew | proposed ItemTransferPolicyPresetCatalog.cs |
| 3097 | ItemCreatedFromItemUsage | proposed ItemTransferPolicyPresetCatalog.cs |
| 3098 | RefundConsumedItem | proposed ItemTransferPolicyPresetCatalog.cs |
| 3099 | ReturnItemShowAsNewNoCoinMerge | proposed ItemTransferPolicyPresetCatalog.cs |
| 3100 | LongText | proposed ItemTransferPolicyDefinition.cs |
| 3101 | NoText | proposed ItemTransferPolicyDefinition.cs |
| 3102 | CanGoIntoVoidVault | proposed ItemTransferPolicyDefinition.cs |
| 3103 | NoSound | proposed ItemTransferPolicyDefinition.cs |
| 3104 | NoCoinMerge | proposed ItemTransferPolicyDefinition.cs |
| 3105 | StepAfterHandlingSlotNormally | proposed TransferCompletionEffectPort.cs |

### SharedWorldItemLifecycleState, 16 members

| seq | member | proposed target role/file |
|---:|---|---|
| 3667 | inner | proposed WorldItemItemRelationAdapter.cs; crossSubsystemOwner: integration-review |
| 3668 | ownTime | proposed WorldItemLifecycleComponent.cs |
| 3669 | playerIndexTheItemIsReservedFor | proposed WorldItemLifecycleComponent.cs; crossSubsystemOwner: integration-review |
| 3670 | noGrabDelay | proposed WorldItemPickupConstraintComponent.cs |
| 3671 | shimmered | proposed WorldItemShimmerComponent.cs |
| 3672 | shimmerTime | proposed WorldItemShimmerComponent.cs |
| 3673 | instanced | proposed WorldItemPickupConstraintComponent.cs |
| 3674 | ownIgnore | proposed WorldItemLifecycleComponent.cs; crossSubsystemOwner: integration-review |
| 3675 | timeSinceTheItemHasBeenReservedForSomeone | proposed WorldItemLifecycleComponent.cs |
| 3676 | timeLeftInWhichTheItemCannotBeTakenByEnemies | proposed WorldItemPickupConstraintComponent.cs |
| 3677 | timeSinceItemSpawned | proposed WorldItemAgingComponent.cs |
| 3678 | beingGrabbed | proposed WorldItemPickupConstraintComponent.cs |
| 3679 | onConveyor | proposed WorldItemMovementStateComponent.cs |
| 3680 | keepTime | proposed WorldItemAgingComponent.cs |
| 3681 | _sceneMetrics | proposed WorldItemSceneMetricsAdapter.cs |
| 4025 | active | proposed WorldItemLifecycleQuery.cs |

### SharedItemEquipmentSlotState, 21 members

| seq | member | proposed target role/file |
|---:|---|---|
| 3176 | wornArmor | proposed ItemEquipmentAppearanceComponent.cs |
| 3221 | headSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3222 | bodySlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3223 | legSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3224 | handOnSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3225 | handOffSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3226 | backSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3227 | frontSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3228 | shoeSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3229 | waistSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3230 | wingSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3231 | shieldSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3232 | neckSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3233 | faceSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3234 | balloonSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3235 | beardSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3236 | voiceSlot | proposed ItemEquipmentAppearanceComponent.cs |
| 3254 | social | proposed ItemEquipmentAppearanceComponent.cs |
| 3255 | vanity | proposed ItemEquipmentAppearanceComponent.cs |
| 3278 | newAndShiny | proposed ItemEquipmentAppearanceComponent.cs |
| 3279 | hasVanityEffects | proposed ItemEquipmentAppearanceComponent.cs |

### SharedWorldItemIdentityAndEconomyPayloadState, 9 members

| seq | member | proposed target role/file |
|---:|---|---|
| 4026 | type | proposed WorldItemEconomyProjection.cs and explicit ItemMutationCommand bridge |
| 4027 | stack | proposed WorldItemEconomyProjection.cs and explicit ItemMutationCommand bridge |
| 4030 | favorited | proposed WorldItemEconomyProjection.cs and explicit ItemMutationCommand bridge |
| 4032 | value | proposed WorldItemEconomyProjection.cs |
| 4036 | maxStack | proposed WorldItemEconomyProjection.cs |
| 4044 | rare | proposed WorldItemEconomyProjection.cs |
| 4049 | Name | proposed WorldItemEconomyProjection.cs |
| 4052 | IsACoin | proposed WorldItemEconomyProjection.cs |
| 4053 | IsAir | proposed WorldItemEconomyProjection.cs |

### SharedWorldItemUseAndPresentationPayloadState, 19 members

| seq | member | proposed target role/file |
|---:|---|---|
| 4028 | newAndShiny | proposed WorldItemUsePresentationProjection.cs and explicit ItemMutationCommand bridge |
| 4029 | color | proposed WorldItemUsePresentationProjection.cs and explicit ItemMutationCommand bridge |
| 4031 | makeNPC | proposed WorldItemUsePresentationProjection.cs and explicit ItemMutationCommand bridge |
| 4033 | useTime | proposed WorldItemUsePresentationProjection.cs |
| 4034 | useAnimation | proposed WorldItemUsePresentationProjection.cs |
| 4035 | useAmmo | proposed WorldItemUsePresentationProjection.cs |
| 4037 | damage | proposed WorldItemUsePresentationProjection.cs |
| 4038 | knockBack | proposed WorldItemUsePresentationProjection.cs |
| 4039 | shootSpeed | proposed WorldItemUsePresentationProjection.cs |
| 4040 | scale | proposed WorldItemUsePresentationProjection.cs |
| 4041 | ammo | proposed WorldItemUsePresentationProjection.cs |
| 4042 | notAmmo | proposed WorldItemUsePresentationProjection.cs |
| 4043 | shoot | proposed WorldItemUsePresentationProjection.cs |
| 4045 | placeStyle | proposed WorldItemUsePresentationProjection.cs |
| 4046 | createTile | proposed WorldItemUsePresentationProjection.cs |
| 4047 | glowMask | proposed WorldItemUsePresentationProjection.cs |
| 4048 | expert | proposed WorldItemUsePresentationProjection.cs |
| 4050 | alpha | proposed WorldItemUsePresentationProjection.cs |
| 4051 | buffType | proposed WorldItemUsePresentationProjection.cs |

### SharedItemEmergencyStackingPolicyState, 6 members

| seq | member | proposed target role/file |
|---:|---|---|
| 2403 | PreservationOrder | proposed EmergencyStackingPolicyDefinition.cs |
| 2404 | PlayerViewRectSize | proposed EmergencyStackingPolicyDefinition.cs |
| 2405 | ItemsToStackEachTime | proposed EmergencyStackingPolicyDefinition.cs |
| 2406 | PendingTransfers | proposed EmergencyStackingSchedulerState.cs |
| 2407 | HasPendingTransfer | proposed EmergencyStackingSchedulerState.cs |
| 2408 | playerViewRectsScratch | proposed EmergencyStackingSchedulerState.cs |

### SharedItemEmergencyStackingTransferState, 21 members

| seq | member | proposed target role/file |
|---:|---|---|
| 2384 | DefaultStackDistanceStepSize | proposed EmergencyStackingPolicyDefinition.cs |
| 2385 | DistanceStepSize | proposed EmergencyStackingPolicyDefinition.cs |
| 2386 | Conditions | proposed EmergencyStackingPredicateCatalog.cs |
| 2387 | StackingPriority | proposed EmergencyStackingPolicyDefinition.cs |
| 2388 | FallenStars | proposed EmergencyStackingGroupDefinition.cs |
| 2389 | CopperCoins | proposed EmergencyStackingGroupDefinition.cs |
| 2390 | SilverCoins | proposed EmergencyStackingGroupDefinition.cs |
| 2391 | Equipment | proposed EmergencyStackingGroupDefinition.cs |
| 2392 | RareCurrency | proposed EmergencyStackingGroupDefinition.cs |
| 2393 | Default | proposed EmergencyStackingGroupDefinition.cs |
| 2394 | type | proposed EmergencyStackingCandidateSnapshot.cs |
| 2395 | age | proposed EmergencyStackingCandidateSnapshot.cs |
| 2396 | isOnScreen | proposed EmergencyStackingCandidateSnapshot.cs |
| 2397 | item | proposed EmergencyStackingCandidateSnapshot.cs; crossSubsystemOwner: integration-review |
| 2398 | src | proposed EmergencyStackingTransferPlan.cs; crossSubsystemOwner: integration-review |
| 2399 | dst | proposed EmergencyStackingTransferPlan.cs; crossSubsystemOwner: integration-review |
| 2400 | distanceOrder | proposed EmergencyStackingTransferPlan.cs |
| 2401 | preservationOrder | proposed EmergencyStackingTransferPlan.cs |
| 2402 | distance | proposed EmergencyStackingTransferPlan.cs |
| 3851 | HasOwnership | proposed EmergencyStackingOwnershipQuery.cs |
| 3852 | NumToTransfer | proposed EmergencyStackingTransferAmountQuery.cs |

### SharedItemToolPlacementCapabilityState, 11 members

| seq | member | proposed target role/file |
|---:|---|---|
| 3199 | pick | proposed ItemToolPlacementCapabilityComponent.cs |
| 3200 | axe | proposed ItemToolPlacementCapabilityComponent.cs |
| 3201 | hammer | proposed ItemToolPlacementCapabilityComponent.cs |
| 3202 | tileBoost | proposed ItemToolPlacementCapabilityComponent.cs |
| 3203 | createTile | proposed ItemToolPlacementCapabilityComponent.cs |
| 3204 | createWall | proposed ItemToolPlacementCapabilityComponent.cs |
| 3205 | placeStyle | proposed ItemToolPlacementCapabilityComponent.cs |
| 3243 | ammo | proposed ItemToolPlacementCapabilityComponent.cs |
| 3244 | notAmmo | proposed ItemToolPlacementCapabilityComponent.cs |
| 3245 | useAmmo | proposed ItemToolPlacementCapabilityComponent.cs |
| 3256 | material | proposed ItemToolPlacementCapabilityComponent.cs |

Coverage invariant: the two mapping sections above represent the 14 input groups, 191 unique
source sequences, 152 fields, and 39 properties. No source member outside P09 is assigned.

## Implementation Sequence

The sequence below is conditional on the blocking ownership decisions. Each step must preserve
the proposed status of the remaining integration work. The current status fields record the
bounded src2 implementation checkpoint above; the remaining steps are still planned and do not
authorize production migration or runtime integration.

1. Reconcile evidence and ownership. Locate or formally disposition the missing first-round
   report, reconcile the declared upstream hash with the observed report hash, and obtain written
   decisions for item/container/slot/entity IDs, inventory authority, result ownership, schema
   owners, and scheduler order.
2. Freeze the inventory mapping. Run a read-only mapping verifier against the input report and
   the two proposed documents. Reject omitted, duplicate, renamed, or cross-partition members.
3. Register proposed catalogs and policies. Add capacity/icon policy, item capability
   definitions, transfer presets, and emergency-stack group definitions with deterministic
   validation. Predicates must use named ports/definitions, not arbitrary persisted delegates.
4. Add proposed ItemIdentityAndStackComponent and ItemFootprintComponent. Import content/type
   data through an adapter, validate stack bounds and prefix/variant rules, and assign no
   unresolved persistent identity.
5. Add proposed ChestSlotStorageComponent and ChestMetadataComponent. Migrate create/assign/
   remove/index boundaries through a single writer and decide non-empty destruction behavior.
   Keep coordinate lookup and current-use tracking in adapters.
6. Add pure ItemDerivedQuery and capability readers. Verify that queries do not mutate item,
   catalog, chest, player, or WorldItem state.
7. Add proposed WorldItem lifecycle and relation boundary. Introduce explicit time, spatial,
   inventory-space, reservation, and scene-metrics ports. Keep WorldItem.inner from becoming a
   second independently writable payload.
8. Add Quick Stack snapshots and planner. Capture revisions and slot references, calculate
   deterministic intents, and return stale/locked/blocked outcomes without mutation.
9. Add transfer commands and completion effects. Commit item/container changes through the
   integration port, then deliver StepAfterHandlingSlotNormally semantics through an explicit
   idempotent effect port.
10. Add Emergency Stack policy, candidate query, and transfer plan. Preserve group priority,
    distance order, ownership qualification, and quantity calculation; keep all candidate and
    pending lists transient.
11. Add equipment/appearance and WorldItem economy/use/presentation projections. Route settable
    forwarding members to explicit item mutation commands and keep projection fields read-only.
12. Add versioned network, persistence, and client projections after schema ownership is
    approved. Do not serialize delegates, live objects, scratch plans, scene caches, or
    operation-local candidates.
13. Shadow and dual-read compare only behavior with closed Version4 evidence. Incomplete
    QuickStacking, EmergencyStacking, and Player helper methods remain not-closed and block a
    behavior-equivalence claim.
14. Remove compatibility writes only after single-writer, parity, protocol, persistence, and
    rollback gates pass. Retain read adapters until integration review approves removal.

## Single-Writer Contract

| Concern | proposed single writer | forbidden direct writers |
|---|---|---|
| Item content/type/stack/prefix/variant | proposed ItemMutationSystem | queries, projections, network handlers, UI |
| Item capability payloads | proposed ContentDefinitionLoadSystem or approved capability command | transfer planner, projection, UI |
| Chest slots | proposed ChestSlotMutationSystem | quick-stack query, UI, network decoder, persistence reader |
| Chest capacity/name/bank metadata | proposed ChestMetadataCommandSystem | icon adapter, presentation system, query |
| Chest coordinate/index registry | proposed ChestRegistryAdapter | slot component, UI, arbitrary legacy caller |
| Chest current-use access set | proposed ChestAccessTrackerAdapter | storage component, projection |
| WorldItem lifecycle | proposed WorldItemLifecycleSystem | economy/use projection, query, network projection |
| WorldItem item relation | integration-owned ItemRelationPort | lifecycle component, projection |
| Quick Stack plan scratch | proposed QuickStackPlanner | authority components, projections |
| Quick Stack commit | integration-owned ContainerItemTransferPort | plan query, UI, network decoder |
| Transfer post-slot effects | proposed TransferCompletionEffectPort | policy query, slot component |
| Emergency Stack candidates/plans | proposed EmergencyStackPlanner | item authority, projection |
| Emergency Stack commit | integration-owned WorldItemCombinePort | candidate query, UI |
| Equipment ownership | integration-owned PlayerEquipmentPort | item appearance projection |
| Network output | proposed versioned network projection owner | components and queries |
| Persistence output | proposed versioned persistence projection owner | components and queries |
| Client/UI output | proposed client projection owner | domain state and network decoder |

Any temporary double-write must be feature-gated, operation-identified, telemetry-backed,
idempotent, and time-bounded. The proposed writer owns the canonical decision; compatibility
adapters receive a one-way projection and cannot feed back into domain state.

## Compatibility And Double-Write Strategy

1. Read legacy Item, Chest, WorldItem, QuickStacking, EmergencyStacking, and GetItemSettings
   through proposed adapters. Normalize fields without modifying the legacy objects.
2. Capture equivalent operation inputs and, where possible, an explicit deterministic clock/RNG
   and revision. Run proposed pure queries in shadow mode without committing.
3. Compare item-derived values, chest capacity/slot views, transfer policy decisions, quick-stack
   intents, emergency-stack candidates, ownership qualification, and transfer quantities.
4. Route one canonical command through the approved owner only after comparison and integration
   approval. Legacy callers receive a compatibility projection, never an independent mutation.
5. On mismatch, keep the gate in shadow/not-closed state, preserve evidence, and do not assert
   behavior equivalence for incomplete Version4 methods.
6. Roll back by feature-routing uncommitted operations to the legacy path, draining or
   compensating pending reservations by operation identity, and retaining committed records.
   Never replay a committed stack transfer without idempotency proof.

## Network, Snapshot, And Persistence Migration

Network migration is proposed in three stages:

1. Add versioned read-only projections for chest slot snapshots/deltas, item stack/prefix
   changes, WorldItem lifecycle/reservation state, and transfer accept/reject results.
2. Shadow-decode and compare against MessageBuffer/NetMessage behavior without changing
   authoritative state. Reject live object references, delegates, static dictionaries, scratch
   lists, and unresolved IDs from wire records.
3. Cut over one message family at a time with protocol version, acknowledgement, replay,
   disconnect, and retry tests owned by integration review.

Persistence migration is proposed in three stages:

1. Define versioned records for chest address/name/capacity/bank state, committed slots, item
   identity/quantity/prefix/variant, and any lifecycle state explicitly approved as durable.
2. Read legacy WorldFile data through an adapter into proposed records, validate capacity,
   item type, stack bounds, and missing-content outcomes before commit.
3. Write proposed projections only after load/save/recovery tests pass; retain a rollback reader
   until the migration gate is closed.

Do not persist Quick Stack or Emergency Stack scratch, candidate/transfer records, callbacks,
scene metrics, animation-only state, temporary ownership views, or client-only presentation
unless a separate owner and durability requirement is approved.

## Transaction, Failure, Retry, And Rollback Plan

- Catalog registration fails closed on duplicate/invalid definitions; no partial catalog becomes
  visible.
- Snapshot capture records source/destination revisions. A stale revision rejects the plan
  before any mutation.
- A query failure or blocked/locked destination produces a typed non-commit result.
- A transfer command reserves all required authority before changing stack quantities. The
  integration port owns atomicity across inventory/container/world-item domains.
- If a multi-destination operation cannot be atomic, integration review must select a
  per-intent commit policy and compensation record; P09 must not assume rollback is automatic.
- The donor/destination stack update is committed before network, persistence, UI, sound, or
  StepAfterHandlingSlotNormally effects. Projection failure does not undo a committed domain
  result; it enters a retry queue.
- Retry uses an operation identity and expected revisions. Replaying an already committed
  operation is a no-op or a typed duplicate result.
- Ownership loss, reservation expiry, missing item definition, capacity mismatch, or malformed
  slot reference rejects the command without partial hidden writes.
- Rollback is a routing/feature-flag decision plus pending-operation drain. It does not delete
  durable records or reset shared workspace state.

## Focused Verification Plan And Evidence

The src2 implementation checkpoint was verified with the following serial commands:

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

The red-green cycle recorded an earlier expected failure for the Quick Stack partial-capacity
case (exit code 1) before the source fix. The remaining verifier sequence is:

1. Read-only inventory verifier: assert exact 14 groups, 191 source rows, 152 fields, 39
   properties, unique sequence numbers, and synchronized metadata/checkpoint fields.
2. Target-layout verifier: assert that every proposed mapping target is capability-first, every
   proposed public type has a same-named file, no generic shared component directory is used,
   and no path is treated as created before implementation.
3. Authority verifier: detect duplicate writers for item stack, chest slots, WorldItem
   lifecycle, transfer commits, projections, and effect callbacks.
4. Pure-query tests: prove ItemDerivedQuery, WorldItemOwnershipQuery, QuickStackPlanQuery,
   EmergencyStackingOwnershipQuery, and EmergencyStackingTransferAmountQuery have no writes.
5. State/command tests: cover stack bounds, chest capacity, slot locking, revision conflicts,
   reservation release, ownership loss, partial transfer, retry idempotency, and compensation.
6. Snapshot/protocol tests: verify versioned network/persistence/client records exclude live
   references, delegates, static caches, and operation-local scratch.
7. Shadow parity tests: compare only closed evidence; mark stubbed/incomplete Version4 behavior
   not-closed rather than turning a mismatch into a pass.
8. Process/build gate is satisfied for src2 only. Production integration remains unverified;
   any future compile must continue to inspect active dotnet.exe/csc.exe processes and use the
   repository wrapper with the required serial properties.

## Risks, Evidence Gaps, And Blocking Decisions

| risk or gap | impact | required decision/control |
|---|---|---|
| Missing first-round P09 report | Hidden readers/writers and prior boundary decisions are unavailable | Recover or explicitly accept the missing evidence before implementation. |
| Declared upstream hash differs from observed report hash | Provenance and source lineage cannot be assumed | Recompute, identify the hashed artifact, and reconcile with integration review. |
| QuickStacking and EmergencyStacking stubs | Transfer ordering and failure semantics cannot be claimed equivalent | Keep plan/command boundary not-closed; add source-level evidence or compatibility policy. |
| Player pickup/Void Vault helpers incomplete | Inventory authority and transfer side effects are unresolved | Assign an integration-owned inventory port and test real caller paths. |
| WorldItem.inner wrapper | Duplicate authority or hidden writes are likely | Choose canonical item relation and route settable forwarding through commands. |
| Static registries and callbacks | Hidden global coupling and untestable side effects | Isolate adapters/effect ports and define lifecycle/ownership. |
| Cross-partition IDs and snapshots | Network/persistence/rollback cannot be closed | Assign typed owners and schema/version/retry decisions. |
| Unresolved scheduler order | Interleaved mutation/projection races remain possible | Integration review must approve explicit phase barriers. |

Blocking decisions remain:

- crossSubsystemOwner: integration-review for ItemInstanceId, ContainerId, SlotReference,
  EntityReference, persistent IDs, network IDs, external IDs, snapshot value objects, and all
  cross-partition interfaces;
- crossSubsystemOwner: integration-review for player inventory/equipment/banks/Void Vault,
  tile-entity storage, currency, world/NPC result commits, network/persistence schemas, and
  scheduler order;
- src2 implementation is present and focused-verified, but production migration, protocol
  closure, persistence closure, and behavior parity remain unclaimed.

## Integration Handoff And Exit Gates

Before implementation, integration review must assign:

- canonical item payload and WorldItem relation owner;
- chest/container/slot authority, revision, and destruction behavior;
- player inventory/equipment/bank/Void Vault/tile-entity storage ports;
- transfer reservation, ownership, partial commit, compensation, retry, and idempotency;
- Item/Container/Slot/Entity/Persistent/Network/External ID types;
- network/persistence/client schema and projection retry owners;
- explicit system scheduler order and cross-partition barriers;
- treatment of missing first-round research, hash discrepancy, and incomplete Version4 behavior.

Implementation exit for the remaining work requires exact source mapping, focused state/query/
command tests, protocol and persistence tests, serial affected-project build with Build/bin
artifact verification, a rollback drill, and removal or explicit retention of every compatibility
writer. The src2-focused gate is executed; the integration gates are not.

## src2 Implementation File Inventory

The following 90 implementation and verifier files were observed for this checkpoint. They are
actual checkpoint files under `src2`, not the proposed target paths in the layout section above;
they are confined to `src2` and `Build/bin`, and no implementation file was written under `src/`:

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

## Explicit Non-Claims

No production C# file was added, moved, split, renamed, or edited. No runtime component was
registered, no production scheduler changed, no protocol/persistence schema was closed, and no
behavior-equivalence result was established. The src2 implementation is a bounded checkpoint;
the atomic container transfer port, WorldItem pickup/combine command, integration-owned IDs,
cross-partition scheduler, and final network/persistence verification remain pending.
