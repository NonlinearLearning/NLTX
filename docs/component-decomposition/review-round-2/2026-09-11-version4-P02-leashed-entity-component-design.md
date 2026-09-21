# Version4 P02 Leashed Entity 注册、行为与物种定义组件拆分设计

partitionId: P02
sessionId: 43ae70a27ce444d699a88b2004e21696
inputReport: D:\\TRbackup\\NLTX\\docs\\组件文档\\迁移参考表\\Version4权威模拟系统字段属性逐成员源码声明-20分区\\P02-Leashed-Entity.md
componentDesignPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-P02-leashed-entity-component-design.md
componentExecutionPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-P02-leashed-entity-component-execution.md
designStatus: proposed
executionStatus: in-progress
implementationStatus: implemented
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: not-verified
completedComponents: [LeashedEntityStateComponent, LeashedEntityLifecycleComponent, LeashedEntityAnchorRelationComponent, LeashedEntitySectionMembershipComponent, LeashedEntityLegacySlotComponent, LeashedCritterBehaviorComponent, LeashedKiteBehaviorComponent, LeashedButterflyVariantComponent]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T07:09:16.2949464Z
verificationEvidence: serial wrapper build passed for src/LeashedEntity/Terraria.LeashedEntity.csproj; exitCode=0; warnings=0; errors=0; artifact=Build/bin/Terraria.LeashedEntity/Debug/net10.0/Terraria.LeashedEntity.dll
evidence-gap: C08 catalog ownership is mapped from current prototype declarations and constructor defaults, but exact stable definition IDs, item/NPC mapping, custom override semantics, current-checkout behavior, network application, persistence, and cross-domain lifecycle ownership remain partial across P02.
blocking-decision: integration-review; definition catalog and registration writer remain deferred

## 8. Implementation checkpoint: LeashedEntityStateComponent

- status: implemented
- source: `src/LeashedEntity/LeashedEntityStateComponent.cs`
- fields: `DefinitionId`; zero remains the unbound default, and `IsBound` is a derived view.
- registration: no registration attribute or runtime key was added because this project exposes no
  component registration mechanism for this domain.
- dependency impact: entity hydration/registration remains the future writer; no catalog, system,
  adapter, query, projection, or test was added.
- verificationStatus: not-verified; the affected project compiled successfully in this checkpoint,
  but definition-catalog and runtime equivalence evidence is still absent.
- evidence-gap: accepted definition ID range and catalog ownership remain unresolved by the P02
  evidence, so this component does not validate or allocate definition IDs.

## 9. Implementation checkpoint: LeashedEntityLifecycleComponent

- status: implemented
- source: `src/LeashedEntity/LeashedEntityLifecycleComponent.cs`
- fields: `State` is the lifecycle authority; `Spawned`, `IsActive`, and `IsRemoved` remain local
  lifecycle views. `TransitionSequence` is retained as an explicitly unverified compatibility
  candidate because Version4 has no corresponding member.
- registration: no registration attribute or runtime key was added; no component registration
  mechanism exists in the affected project.
- dependency impact: the future registration/section owner remains the sole writer for transitions;
  this component does not perform spawn, despawn, removal, logging, or external I/O.
- verificationStatus: not-verified; project compilation passed in this checkpoint, while lifecycle
  equivalence remains unverified.
- evidence-gap: section activation, anchor removal, clear/reload, and transition ordering remain
  integration-review responsibilities.

## 10. Implementation checkpoint: LeashedEntityAnchorRelationComponent

- status: implemented
- source: `src/LeashedEntity/LeashedEntityAnchorRelationComponent.cs`
- fields: runtime anchor reference, tile coordinate, optional persistent anchor identity, and
  relation revision; `HasRuntimeAnchor` is a derived view and is not persistence authority.
- registration: no runtime component key was invented.
- dependency impact: `TELeashedEntityAnchor`/tile storage owns anchor lifecycle and persistence;
  P02 stores only the entity-side relation and does not mutate the host.
- verificationStatus: not-verified; relation hydration, stale-revision handling, persistence and
  anchor-removal behavior remain unverified after the component project build.
- evidence-gap: the Version4 report exposes only `AnchorPosition`; the additional runtime/persistent
  relation fields remain partial compatibility state until the cross-subsystem owner is accepted.

## 11. Implementation checkpoint: LeashedEntitySectionMembershipComponent

- status: implemented
- source: `src/LeashedEntity/LeashedEntitySectionMembershipComponent.cs`
- fields: section coordinate, recyclable section slot, local section-activity observation, and
  optional observation tick; `IsIndexed` is derived from the slot.
- registration: no section registry key was added.
- dependency impact: `BySection`, `ActiveSectionList`, bucket compaction, and external activation
  remain Adapter/System responsibilities; this component does not own global section state.
- verificationStatus: not-verified; section rebuild, compaction, activation and clear/reload semantics
  remain unverified after the component project build.
- evidence-gap: the source report confirms only the legacy `sectionSlot` member; the coordinate and
  observation fields are explicit NLTX boundary state pending integration ownership.

## 12. Implementation checkpoint: LeashedEntityLegacySlotComponent

- status: implemented
- source: `src/LeashedEntity/LeashedEntityLegacySlotComponent.cs`
- fields: reusable `Slot` with `-1` as unassigned, `SlotGeneration` as an explicit compatibility
  guard candidate, and derived `IsAssigned`.
- registration: no network or component registration key was added.
- dependency impact: allocation, reuse, `ByWhoAmI` lookup, network projection and removal remain
  outside the component; the slot is not a durable entity identity.
- verificationStatus: not-verified; slot allocation/reuse and network lookup are unverified after
  the component project build.
- evidence-gap: Version4 confirms the reusable `whoAmI` field but not a generation counter, so the
  generation field cannot be treated as behavior-equivalent until the compatibility verifier exists.

## 13. Implementation checkpoint: LeashedCritterBehaviorComponent

- status: implemented
- source: `src/LeashedEntity/LeashedCritterBehaviorComponent.cs`
- fields: NPC content binding, anchor/capability snapshots, target coordinate, random cursor,
  wait timer and behavior state; `HasTargetPosition` is derived only.
- registration: no NPC, network, or component registration key was added.
- dependency impact: behavior systems remain the only intended writers for target/timer/state/random
  fields; catalog, random provider, NPC dummy, interpolation, presentation and protocol adapters are
  not implemented in this component-only step.
- verificationStatus: not-verified; exact LCG semantics, full/partial network hydration and behavior
  transitions remain unverified after the component project build.
- evidence-gap: `anchorStyle` and `isAquatic` still require immutable catalog ownership; they are
  retained as partial snapshots and must not be treated as catalog authority.

## 14. Implementation checkpoint: LeashedKiteBehaviorComponent

- status: implemented
- source: `src/LeashedEntity/LeashedKiteBehaviorComponent.cs`
- fields: projectile content binding, rotation, kite distance, wind target/current, elapsed timer,
  and no-wind timer; `DefaultKiteDistance` preserves the source-confirmed 250-unit default.
- boundary correction: removed `projectileLocalAI0` and `projectileLocalAI1` from the component;
  the P02 member ledger classifies them as Projectile compatibility-adapter state.
- dependency impact: Projectile dummy/logic, environment reads, trail/interpolation, network and
  anchor-world queries remain outside this component.
- verificationStatus: not-verified; wind transitions, fast-forward, section gating and content mapping
  remain unverified after the component project build.
- evidence-gap: Projectile adapter ownership and the exact kite update loop remain integration-review
  dependencies.

## 15. Implementation checkpoint: LeashedButterflyVariantComponent

- status: implemented
- source: `src/LeashedEntity/LeashedButterflyVariantComponent.cs`
- fields: per-entity `Variant` byte for the normal butterfly; no prototype, fade, opacity, or visual
  cache is stored here.
- registration: no component or network key was invented.
- dependency impact: item/style hydration and the full-only network suffix remain future adapter/
  command responsibilities; Empress fade/opacity remains presentation-only and is not included.
- verificationStatus: not-verified; variant range validation, full/partial suffix behavior and
  presentation purity remain unverified after the component project build.
- evidence-gap: the Version4 source confirms the byte field but not the accepted range in the current
  checkout; the passive component intentionally performs no validation.

## 1. Scope and status

This document covers only the eight leaf groups in P02: `LeashedRegistryAndSections`,
`LeashedCritterCoreState`, `LeashedWalkerBehavior`, `LeashedJumperBehavior`,
`LeashedFlyerBehavior`, `LeashedKiteBehavior`, `LeashedButterflyVariants`, and
`LeashedSpeciesPrototypes`. The authoritative report contains 93 fields, 5 properties, and 98
members. No member from another P01-P20 partition is assigned here.

This is an evidence-driven ECS boundary proposal. It is not a runtime migration, API-compatibility
proof, persistence proof, network-closure proof, or behavior-equivalence proof. The eight component
types have implementation records above; every non-component type and target path below remains
`status: proposed`.

The eight checkpoints are intentionally not eight giant runtime components. A leaf group may map
to a narrow Component plus a System, Query, Adapter, Projection, or Definition catalog when the
source member is an index, a constant, a cache, or an external protocol value.

| Checkpoint | Leaf group | Members | Proposed boundary | Status |
|---|---|---:|---|---|
| C01 | `LeashedRegistryAndSections` | 15 | definition catalog, registration/section systems, slot and section adapters | design-checkpoint-complete |
| C02 | `LeashedCritterCoreState` | 15 | critter authority, behavior-state and presentation seams | design-checkpoint-complete |
| C03 | `LeashedWalkerBehavior` | 7 | walker definition plus walker owner system | design-checkpoint-complete |
| C04 | `LeashedJumperBehavior` | 11 | jumper definition plus jumper owner system | design-checkpoint-complete |
| C05 | `LeashedFlyerBehavior` | 11 | flyer definition plus flyer owner system | design-checkpoint-complete |
| C06 | `LeashedKiteBehavior` | 20 | kite authority, motion/environment port, and visual/network projection | design-checkpoint-complete |
| C07 | `LeashedButterflyVariants` | 6 | variant definition and butterfly visual state projection | design-checkpoint-complete |
| C08 | `LeashedSpeciesPrototypes` | 13 | immutable species definition catalog; no per-species component fan-out | design-checkpoint-complete |

## 2. Evidence register

| Source | Read evidence | Supports | Status |
|---|---|---|---|
| P02 authoritative report | `D:\\TRbackup\\NLTX\\docs\\组件文档\\迁移参考表\\Version4权威模拟系统字段属性逐成员源码声明-20分区\\P02-Leashed-Entity.md:1-304`; runner observed 98/98 members | complete P02 member inventory, source rows, leaf grouping, original declarations | confirmed |
| Version4 current declaration | `D:\\TRbackup\\Version4\\Terraria.GameContent\\LeashedEntity.cs:11-306`; `D:\\TRbackup\\Version4\\Terraria.GameContent.LeashedEntities\\LeashedCritter.cs:11-50`; `WalkerLeashedCritter.cs:7-30`; `JumperLeashedCritter.cs:7-47`; `FlyerLeashedCritter.cs:7-45`; `LeashedKite.cs:9-54` | current declaration shape, visibility, fields, properties, virtual seams; many method bodies are empty | confirmed with version-drift |
| Full-reference fallback | `D:\\TRbackup\\无任何删减通过编译\\Terraria.GameContent\\LeashedEntity.cs:22-596`; same-root `Terraria.GameContent.LeashedEntities\\LeashedCritter.cs:47-245`, `WalkerLeashedCritter.cs:23-210`, `JumperLeashedCritter.cs:31-299`, `FlyerLeashedCritter.cs:31-172`, `LeashedKite.cs:49-294` | behavior, registration, slot reuse, section activation, full/partial network payload and side effects | partial; fallback is not the current Version4 checkout |
| Version4 anchor declaration | `D:\\TRbackup\\Version4\\Terraria.GameContent.Tile_Entities\\TELeashedEntityAnchor.cs:5-12`; `TELeashedEntityAnchorWithItem.cs:6-34`; `TEKiteAnchor.cs:7-46`; `TECritterAnchor.cs:7-73` | anchor and item/prototype relationship shape; current bodies are incomplete | confirmed shape, lifecycle partial |
| Full-reference anchor fallback | `D:\\TRbackup\\无任何删减通过编译\\Terraria.GameContent.Tile_Entities\\TELeashedEntityAnchor.cs:5-63`; `TELeashedEntityAnchorWithItem.cs:6-80`; `TEKiteAnchor.cs:7-83`; `TECritterAnchor.cs:7-115` | world-load, tile removal, item validation, prototype selection and respawn side effects | partial; integration input only |
| World and network callers | `D:\\TRbackup\\Version4\\Terraria\\Main.cs:3347,11572`; `WorldGen.cs:6665,52726`; `RemoteClient.cs:65,184`; `MessageBuffer.cs:3314` | registry initialization, tick entry, clear path, section activation callback and anchor item placement read | confirmed call sites; ownership still partial |
| Current NLTX entity components | `D:\\TRbackup\\NLTX\\src\\LeashedEntity\\LeashedEntityStateComponent.cs`; `LeashedEntityLifecycleComponent.cs`; `LeashedEntityAnchorRelationComponent.cs`; `LeashedEntitySectionMembershipComponent.cs`; `LeashedEntityLegacySlotComponent.cs`; `LeashedCritterBehaviorComponent.cs`; `LeashedKiteBehaviorComponent.cs`; `LeashedButterflyVariantComponent.cs` | implemented component schemas and remaining duplicate/unfinished ownership risks | implementation evidence; compile verified, runtime behavior partial |
| Current NLTX simulation/protocol | `D:\\TRbackup\\NLTX\\dome\\src\\Terraria.Dome.Simulation\\Leash\\LeashAnchorStateComponent.cs`; `LeashSectionIndexComponent.cs`; `dome\\src\\Terraria.Dome.Protocol.V1456\\Packets\\TerrariaPacketCodec.cs:569-640,4172-4290` | protocol packet shape and local simulation fragments | existing-evidence; no production loop closure |
| Protocol verifier | `D:\\TRbackup\\NLTX\\dome\\Test\\Terraria.Dome.Protocol.Compatibility.Verification\\Program.cs:1232-1375,2998-3035` | existing tests exercise remove/full/partial framing, type range and butterfly/Shimmer suffixes | existing-evidence only; not run in this session |
| tModLoader stable mirror | `D:\\TRbackup\\tmodloader-api-docs-stable\\index.html` reports `tModLoader v2026.07`; `annotated.html` resolves `Main` to `class_main.html` and `TileEntity` to `class_tile_entity.html`; no exact `LeashedEntity`, `LeashedCritter`, `LeashedKite`, or `TELeashedEntityAnchor` hit in `annotated.html`, `classes.html`, `namespaces.html`, or `functions.html` | public external `Main`/`TileEntity` boundary cross-check only | public-boundary; private Terraria types remain source-evidence only |
| SS14 read-only ECS reference | `C:\\Users\\shan\\Downloads\\ECS\\space-station-14-master\\Content.Shared\\Zombies\\ZombieComponent.cs:23-65`; `Content.Shared\\Changeling\\Systems\\SharedChangelingIdentitySystem.cs:20-72`; `Content.Shared\\Teleportation\\Systems\\LinkedEntitySystem.cs:8-76`; `Content.Server\\Atmos\\EntitySystems\\AtmosphereSystem.cs:25-118` | narrow capability components, explicit systems, subscriptions, queries and dependency/side-effect seams | organization-only; no copied names or semantics |
| Repository constraints | `D:\\TRbackup\\NLTX\\AGENTS.md`; `D:\\TRbackup\\NLTX\\Context\\架构设计\\ECS文件组织设计约束.md`; `D:\\TRbackup\\NLTX\\Context\\约束\\非函数式编码副作用隔离规范.md` | domain-first files, one public type per file, explicit effect boundary and visible ordering | confirmed |

### 2.1 Evidence interpretation

The current Version4 checkout is authoritative for declarations and call-site shape. Its empty
methods are not evidence that behavior is absent from the intended reference; the unabridged
checkout is a read-only fallback used only to recover behavior and side-effect ordering. Every
fallback conclusion remains `partial` until the target Version4 behavior source and verifier are
available in the same accepted input set.

The tModLoader mirror does not document the private `Leashed*` types. It is therefore not used to
invent field meanings. The SS14 checkout is used only to validate the organization pattern: a
component stores a capability/state slice, a system owns transitions and subscriptions, and a
query or projection does not become a second authority.

## 3. Boundary rules

1. `DefinitionId`, `NpcType`, and `ProjectileType` are content/definition identifiers. They are
   not runtime entity IDs, network slots, persistent anchor IDs, or array positions.
2. `whoAmI` is a reusable Version4 compatibility slot. It must be represented by an adapter or
   compatibility component and must not become durable entity identity.
3. `AnchorPosition` is an entity-to-anchor relation fact. The anchor TileEntity owns its own tile,
   item and persistence lifecycle; P02 owns only the entity-side relation and uses
   `crossSubsystemOwner: integration-review` for the final host owner.
4. `BySection`, `ActiveSectionList`, `SectionEntityList.list`, `count`, and `emptySlots` are
   indexes/caches. They are not persistent entity authority and must be rebuilt from committed
   entity state.
5. `active` is a lifecycle fact on the entity; section `active` is an observation of the external
   section-streaming owner. Same spelling does not imply shared ownership.
6. Prototype inheritance is a source definition mechanism. Runtime entities use a definition ID
   plus capability components; they do not inherit a deep runtime behavior tree.
7. Binary readers/writers, network packets, tile storage, clocks, global random sources, logging,
   UI and rendering are adapters or projections. Core systems receive explicit ports and commit
   deterministic state transitions.
8. System order is a scheduler contract, never file order: register definitions -> hydrate anchor
   relation and lifecycle -> maintain section membership -> run behavior owner -> commit lifecycle
   removals -> publish network/persistence/render projections.

## 4. Component and boundary inventory

The table below is the design-time boundary inventory. The eight component entries implemented in
this session have implementation records in sections 8-15; systems, queries, adapters, projections,
and definition catalogs remain proposals.

| Proposed boundary | Kind | Target path | Authority and seam |
|---|---|---|---|
| `LeashedEntityStateComponent` | Component | `src/LeashedEntity/LeashedEntityStateComponent.cs` | definition binding; one writer in registration/hydration system |
| `LeashedEntityLifecycleComponent` | Component | `src/LeashedEntity/LeashedEntityLifecycleComponent.cs` | active/spawn/despawn state; one lifecycle commit writer |
| `LeashedEntityAnchorRelationComponent` | Component | `src/LeashedEntity/LeashedEntityAnchorRelationComponent.cs` | entity-side anchor relation; host owner is integration-review |
| `LeashedEntitySectionMembershipComponent` | Component | `src/LeashedEntity/LeashedEntitySectionMembershipComponent.cs` | current section and recyclable section slot; index system writes |
| `LeashedEntityLegacySlotComponent` | Compatibility Component | `src/LeashedEntity/LeashedEntityLegacySlotComponent.cs` | reusable `whoAmI` mapping only; no durable identity |
| `LeashedCritterBehaviorComponent` | Component | `src/LeashedEntity/LeashedCritterBehaviorComponent.cs` | critter runtime state and random cursor; behavior systems write |
| `LeashedKiteBehaviorComponent` | Component | `src/LeashedEntity/LeashedKiteBehaviorComponent.cs` | kite runtime state; environment and projectile seams are integration-review |
| `LeashedButterflyVariantComponent` | Component | `src/LeashedEntity/LeashedButterflyVariantComponent.cs` | normal butterfly variant only; implemented in this session, range and protocol ownership remain partial |
| `LeashedDefinitionCatalogAdapter` | Adapter/Definition catalog | `src/LeashedEntity/LeashedDefinitionCatalogAdapter.cs` | maps stable definition IDs to immutable definitions; never stores entity runtime state |
| `LeashedSectionIndexAdapter` | Adapter | `src/LeashedEntity/LeashedSectionIndexAdapter.cs` | rebuilt section buckets, compaction and slot lookup; not persistence authority |
| `LeashedEntityRegistrationSystem` | System | `src/LeashedEntity/LeashedEntityRegistrationSystem.cs` | add/remove/hydrate entity and commit slot/lifecycle/relation changes |
| `LeashedSectionActivationSystem` | System | `src/LeashedEntity/LeashedSectionActivationSystem.cs` | consumes section activity input and gates Spawn/Despawn; external owner integration-review |
| `LeashedSectionCoordinatesQuery` | Query | `src/LeashedEntity/LeashedSectionCoordinatesQuery.cs` | pure anchor-coordinate to section-coordinate calculation |
| `LeashedEntityReplicationProjection` | Projection | `src/LeashedEntity/LeashedEntityReplicationProjection.cs` | full/partial/remove snapshot; packet encoding remains protocol adapter |
| `LeashedEntityPersistenceAdapter` | Adapter | `src/LeashedEntity/LeashedEntityPersistenceAdapter.cs` | future snapshot/recovery boundary; format is blocked until evidence exists |
| `LeashedWalkerDefinition` / `LeashedWalkerSystem` | Definition/System | `src/LeashedEntity/LeashedWalkerDefinition.cs`, `LeashedWalkerSystem.cs` | walker constants/config and transitions; runtime state stays in components |
| `LeashedJumperDefinition` / `LeashedJumperSystem` | Definition/System | `src/LeashedEntity/LeashedJumperDefinition.cs`, `LeashedJumperSystem.cs` | jumper constants/config and transitions; tile/liquid queries via ports |
| `LeashedFlyerDefinition` / `LeashedFlyerSystem` | Definition/System | `src/LeashedEntity/LeashedFlyerDefinition.cs`, `LeashedFlyerSystem.cs` | flyer constants/config and transitions; world collision query via port |
| `LeashedKiteSystem` | System | `src/LeashedEntity/LeashedKiteSystem.cs` | kite movement and wind state; player/projectile/world effects via ports |
| `LeashedKiteVisualProjection` | Projection | `src/LeashedEntity/LeashedKiteVisualProjection.cs` | trail arrays, frame data and interpolation output; no authority |
| `LeashedButterflyVisualProjection` | Projection | `src/LeashedEntity/LeashedButterflyVisualProjection.cs` | fade/opacity and visual effects; no gameplay authority |
| `LeashedSpeciesDefinitionCatalog` | Definition/Adapter | `src/LeashedEntity/LeashedSpeciesDefinitionCatalog.cs` | all 13 species prototype rows in one immutable catalog; no 13-component fan-out |

## 5. C01 checkpoint: LeashedRegistryAndSections

### 5.1 Source facts and ownership

The current declaration has `Registry.Prototypes` at `LeashedEntity.cs:44`, section bucket fields at
`:96-104`, global indexes at `:177-181`, and entity-side `sectionSlot`, `active`, `whoAmI`, `Type`,
`AnchorPosition`, and `SectionCoordinates` at `:183-194`. The full-reference fallback adds
`Register.Get` (`LeashedEntity.cs:151-177`), bucket add/remove/compaction (`:196-238`), section
activate/deactivate/sync (`:239-283`), entity registration/removal (`:377-453`), and the update
loop (`:455-520`). `Main` calls registry initialization and `UpdateEntities` at lines 3347 and
11572; `RemoteClient.NetSectionActivated` is declared/called at lines 65 and 184.

This source shape crosses six meanings. C01 therefore does not create a single
`LeashedRegistryAndSectionsComponent`.

| Source member | Classification | Proposed owner | Reader/writer and lifecycle | Evidence |
|---|---|---|---|---|
| `Registry.Prototypes` | definition catalog/cache | `LeashedDefinitionCatalogAdapter` | registration bootstrap writes once; definition queries read; rebuild on process/world bootstrap | current `LeashedEntity.cs:44,46-80`; fallback `:121-177` |
| `SectionEntityList.coordinates` | index key | `LeashedSectionIndexAdapter` | section index creates bucket; survives only while index is alive; rebuilt from entity anchor relation | current `:96`; fallback `:179-194` |
| `SectionEntityList.active` | external section observation/cache | `LeashedSectionActivationSystem` plus section adapter | section activation input writes; deactivation clears; source of truth is external section service | current `:98,133-161`; fallback `:239-268`; `crossSubsystemOwner: integration-review` |
| `SectionEntityList.list` | ephemeral index storage | `LeashedSectionIndexAdapter` | registration system inserts/removes; compaction rewrites slots; never persisted/networked as a collection | current `:100`; fallback `:185-238` |
| `SectionEntityList.count` | index metadata/cache | `LeashedSectionIndexAdapter` | add/remove/compaction writer; invariant `0 <= count <= list.Length` | current `:102`; fallback `:187,196-238` |
| `SectionEntityList.emptySlots` | compaction cache | `LeashedSectionIndexAdapter` | remove increments, compaction resets; never domain state | current `:104`; fallback `:189,208-236` |
| `BySection` | global section index | `LeashedSectionIndexAdapter` | created/rebuilt on registration and clear; reads by section query | current `:177`; fallback `:285,413-423` |
| `ActiveSectionList` | active-bucket index | `LeashedSectionIndexAdapter` | activation adds, recheck compacts/removes; no persistence owner | current `:179`; fallback `:287,463-482` |
| `ByWhoAmI` | compatibility lookup index | `LeashedEntityRegistrationSystem` + `LeashedEntityLegacySlotComponent` | slot allocation/reuse/removal; valid only for current session/connection | current `:181`; fallback `:289,377-453` |
| `sectionSlot` | entity-side recyclable index relation | `LeashedEntitySectionMembershipComponent.SectionSlot` | section index writes during add/compaction; invalidated on removal | current `:183`; fallback `:204-205,224-232` |
| entity `active` | authoritative lifecycle state | `LeashedEntityLifecycleComponent.State` | registration/section system commits active, despawning, removed transitions; behavior systems read | current `:185`; fallback `:293,397-406,425-440` |
| `whoAmI` | compatibility slot projection | `LeashedEntityLegacySlotComponent.Slot` | registration adapter writes reusable slot; network projection reads; not durable identity | current `:187`; fallback `:295,383-400` |
| `Type` | definition binding | `LeashedEntityStateComponent.DefinitionId` | definition registration assigns; entity never changes it after hydration except replacement command | current `:190`; fallback `:309`; protocol range 1..19 in `TerrariaPacketCodec.cs:55,601-607` |
| `AnchorPosition` | entity-to-anchor relation | `LeashedEntityAnchorRelationComponent.AnchorCoordinate` | anchor spawn/restore command writes; lifecycle/removal clears or tombstones; tile host is external | current `:192`; fallback `:311,397-403`; anchor fallback `:54-61` |
| `SectionCoordinates` | derived query | `LeashedSectionCoordinatesQuery` | pure calculation from anchor coordinate and Netplay section conversion; no stored writer | current `:194`; fallback `:313`; tModLoader `Main` public page is only cross-reference |

### 5.2 C01 interface contract

The proposed registration boundary has these minimal seams. Signatures are design sketches, not
code:

```text
LeashedEntityRegistrationSystem.Register(DefinitionId, AnchorCoordinate, SpawnReason)
LeashedEntityRegistrationSystem.Remove(RuntimeEntityId, RemovalReason)
LeashedSectionActivationSystem.Apply(SectionCoordinate, SectionActivity)
LeashedSectionCoordinatesQuery.Get(AnchorCoordinate) -> SectionCoordinate
LeashedSectionIndexAdapter.TryGetByLegacySlot(LegacySlot) -> RuntimeEntityId?
LeashedEntityReplicationProjection.CreateFull/Partial/RemoveSnapshot(entityFacts)
```

`Register` is the only proposed commit root for definition binding, relation hydration,
lifecycle state and compatibility slot allocation. `LeashedSectionIndexAdapter` receives committed
entity facts and maintains indexes; it cannot mutate definition or anchor authority. The section
activation system may call Spawn/Despawn only after a validated external section activity fact.

### 5.3 C01 invariants and ordering

- Registry slot zero remains reserved if the Version4 protocol contract retains the fallback
  `Prototypes.Add(null)` behavior; valid network entity types are 1..19 until a Version4 source
  confirms another range.
- A `whoAmI` slot may be reused after removal. Any future stable runtime/persistent/network ID
  must use a distinct type and cannot be reconstructed from slot position.
- A section bucket contains only entities whose committed `SectionCoordinates` equals the bucket
  key. Compaction updates every surviving `SectionSlot` before exposing the index.
- `active` lifecycle and section `active` observation are different facts. Section deactivation
  gates updates and calls a lifecycle transition; it does not erase the entity relation.
- The rebuild path must reproduce `Clear(keepActiveSections)`: clear indexes, then rehydrate only
  externally active sections. It must not call persistence or network from a pure query.
- Proposed tick order is `apply section activity -> compact/reconcile index -> register/remove
  commands -> spawn/despawn transitions -> run behavior systems -> remove inactive entities ->
  publish full/partial/remove projections`. Scheduler evidence is still missing.

### 5.4 C01 current NLTX mapping and risk

`src/LeashedEntity/LeashedEntityStateComponent.cs`, `LeashedEntityLifecycleComponent.cs`,
`LeashedEntityAnchorRelationComponent.cs`, `LeashedEntitySectionMembershipComponent.cs`, and
`LeashedEntityLegacySlotComponent.cs` already express fragments of the proposed boundary, but no
P02 registration or section system exists in the inspected tree. `dome/src/Terraria.Dome.Simulation/Leash`
contains `LeashAnchorStateComponent` and `LeashSectionIndexComponent`, which are additional
prototype fragments, not proof of a unique owner. The protocol parser can decode full/partial/remove
frames, but does not own entity registration, section activation, or lifecycle commits.

The C01 migration must therefore consolidate before adding readers. It must not dual-write the
`src` and `dome` state models, and it must not move anchor TileEntity item state into an entity-side
component.

### 5.5 C01 focused verifier plan

No verifier was run in this session. The implementation-stage focused verifier must prove:

1. registry bootstrap assigns deterministic definition IDs, rejects unknown IDs, and keeps slot zero
   semantics explicit;
2. add/remove/re-add reuses legacy slots without aliasing a prior runtime entity;
3. section add, remove and compaction preserve membership and update all section slots;
4. activation and deactivation are idempotent, and update runs only for active committed sections;
5. clear/rebuild preserves only externally active sections and does not duplicate entities;
6. full/partial/remove projection ordering and type/anchor mismatch rejection match the protocol;
7. anchor removal and world-load recreation do not leave a stale entity-side relation;
8. no index or projection writes `LeashedEntityStateComponent`, lifecycle authority, or anchor host
   state through a hidden callback.

## 5.6 C02 checkpoint: LeashedCritterCoreState

### 5.6.1 Source facts and classification

The current Version4 declaration at `Terraria.GameContent.LeashedEntities/LeashedCritter.cs:13-41`
contains the 15 P02 members, while its `SetDefaults`, `NetSend`, `NetReceive`, `Spawn`, `Update`,
visual and dummy methods at `:42-52` are empty in this checkout. The complete-reference fallback
shows the intended boundary shape: content/default hydration and the full/partial wire contract at
`:47-109`, spawn/recall and interpolation decay at `:111-148,120-126`, dummy projection at
`:181-208`, and presentation reads at `:210-248`. Those fallback bodies are behavior evidence only;
they are not current-checkout implementation evidence.

The 15 members do not form one undifferentiated runtime component. The proposed split is:

| Source member | Classification | Proposed owner | Readers/writers and lifecycle | Evidence |
|---|---|---|---|---|
| `_dummy` | ephemeral dummy adapter state | `LeashedCritterDummyAdapter` | presentation projection copies a committed snapshot into one adapter-local NPC; cleared after draw; never serialized or persisted | current `:13`; fallback `:181-208,210-248`; partial |
| `anchorStyle` | immutable anchor/content configuration | `LeashedCritterDefinitionCatalog` query | definition bootstrap supplies; behavior/anchor integration reads; no per-tick writer | current `:15`; flyer default at `FlyerLeashedCritter.cs:31`; partial |
| `npcType` | NPC content identifier plus full-sync definition input | `LeashedCritterBehaviorComponent.NpcType` hydrated by registration/definition system; network adapter validates | definition hydration writes once; full snapshot reads/writes through commands; NPC runtime is external | current `:17`; fallback `:54-61,67-71,85-89`; partial |
| `spriteDirection` | presentation cache derived from movement direction | `LeashedCritterPresentationProjection` | behavior result supplies direction; dummy/animation adapter writes and reads; rebuilt after spawn or update | current `:19`; fallback `:191-193,205-208`; partial |
| `frame` | presentation animation cache | `LeashedCritterPresentationProjection` | dummy animation adapter writes; draw reads; not authority or wire state | current `:21`; fallback `:187,205-208,215-221`; partial |
| `frameCounter` | presentation animation cache | `LeashedCritterPresentationProjection` | dummy animation adapter writes; draw reads; not persistence authority | current `:23`; fallback `:188,205-208`; partial |
| `rand` | deterministic behavior random stream state | `LeashedCritterBehaviorComponent.RandomState` plus explicit random port | behavior system consumes and commits one state; full/partial network adapter transfers state; no global random writer in core | current `:25`; fallback `:74,93`; NLTX random state candidate; type semantics partial |
| `WaitTime` | authoritative behavior timer | `LeashedCritterBehaviorComponent.WaitTime` | selected behavior owner decrements/sets; full/partial replication reads and applies via command; spawn initializes | current `:27`; fallback `:75,94`; partial |
| `State` | authoritative behavior state discriminant | `LeashedCritterBehaviorComponent.State` | selected behavior owner is the only writer; wire adapter validates selected definition state range; reset on spawn/recall | current `:29`; fallback `:76,95`; partial |
| `TargetPosition` | authoritative tile target relation | `LeashedCritterBehaviorComponent.TargetPosition` | behavior owner selects and commits; network adapter encodes relative to anchor; anchor/lifecycle changes invalidate or rebase explicitly | current `:31`; fallback `:77-78,96`; partial |
| `netOffset` | client interpolation/presentation cache | `LeashedCritterInterpolationProjection` | full receive clears, partial receive accumulates, update decays; never read by authority or persistence | current `:33`; fallback `:90-104,124`; partial |
| `scale` | presentation/content-derived size scale | definition query plus `LeashedCritterDummyAdapter` | definition/default hydration supplies; dummy projection reads; not changed by behavior system unless a source-approved effect exists | current `:35`; fallback `:193`; partial |
| `strayingRangeInBlocks` | immutable behavior configuration | `LeashedCritterDefinitionCatalog` query | species/behavior definition supplies; walker/jumper/flyer systems read; no runtime writer | current `:37`; constructors set distinct values in C03-C05; partial |
| `isAquatic` | immutable capability/configuration | `LeashedCritterDefinitionCatalog` query | definition bootstrap supplies; presentation and world-query ports read; no runtime writer | current `:39`; fallback `:224-227`; semantic use beyond draw remains partial |
| `RecallDuration` | compile-time transition rule | selected behavior definition/system constant | recall transition reads; never stored as entity state or sent on the wire | current `:41`; fallback `:139-147`; partial |

`npcType`, `anchorStyle`, `strayingRangeInBlocks`, `isAquatic`, and `scale` must not be copied into
the definition catalog as mutable per-entity state. The catalog is immutable; the entity component
holds only the content binding and authoritative runtime fields. Conversely, `frame`,
`frameCounter`, `spriteDirection`, and `netOffset` must not become a second behavior authority just
because the legacy class stores them beside `State`.

### 5.6.2 C02 interface contract

The proposed signatures are design sketches, not code:

```text
LeashedCritterDefinitionQuery.Get(DefinitionId) -> CritterDefinition
LeashedCritterSystem.Evaluate(CritterAuthority, CritterInput, IDeterministicRandom)
  -> CritterTransition
LeashedCritterCommitPort.Commit(CritterTransition) -> CommittedCritterFacts
LeashedCritterReplicationAdapter.DecodeFull/Partial(payload, anchor)
  -> ApplyCritterNetworkStateCommand
LeashedCritterInterpolationProjection.ApplyFull/Partial(position)
LeashedCritterPresentationProjection.Build(committedFacts, definition)
  -> CritterRenderSnapshot
LeashedCritterDummyAdapter.Draw(CritterRenderSnapshot, INpcPresentationPort)
```

`Evaluate` may consume a random value only through an explicit stateful port whose input and output
state are visible in the transition. `Commit` is the only proposed writer for `WaitTime`, `State`,
`TargetPosition`, and the committed random cursor. The network adapter may validate and issue a
command, but it must not mutate the component directly. The presentation projection may update
its own cache and the dummy adapter, but it cannot write authority, anchor relation, lifecycle, or
NPC runtime state.

### 5.6.3 C02 invariants and ordering

- `NpcType` is an NPC content/definition ID, not an index into `Main.npc`, a network slot, or a
  live NPC entity ID. Dummy hydration must validate it through an external content catalog.
- The random cursor is serialized only where the accepted Version4 full/partial protocol requires
  it. Replacing `LCG32Random` with an unverified `uint` algorithm is not behavior equivalence;
  `RandomState` remains a compatibility candidate until the exact transition verifier exists.
- `TargetPosition` is interpreted in the same tile coordinate domain as `AnchorPosition`. Relative
  wire offsets require explicit range validation before commit; overflow or malformed payloads are
  rejected without changing the prior authority state.
- `netOffset` is presentation interpolation state. Full receive sets it to zero, partial receive
  accumulates the position delta, and the presentation update decays it; it is excluded from
  persistence and authoritative behavior comparisons.
- Dummy state is reconstructed from an immutable render snapshot for one presentation operation.
  A static `NPC` adapter cannot be queried as entity authority and must not leak across concurrent
  draw operations.
- `RecallDuration` is a rule constant. A recall transition sets the selected state and timer, but
  no component stores a mutable copy that can diverge from the selected behavior definition.
- Proposed scheduler edges are `hydrate definition -> apply network/commands -> evaluate selected
  behavior -> commit authority -> update interpolation -> build dummy/render projection -> emit
  replication`. The exact game-loop edge remains an integration-review decision.

### 5.6.4 C02 current NLTX mapping and risk

`src/LeashedEntity/LeashedCritterBehaviorComponent.cs` already contains candidates for `NpcType`,
`AnchorStyle`, `IsAquatic`, `TargetPosition`, `RandomState`, `WaitTime`, and `State`. This is a
useful partial state map, not proof of a writer, exact `LCG32Random` semantics, full/partial
network closure, or behavior equivalence. The definition/configuration candidates need to be
separated from mutable behavior state before implementation. No inspected NLTX component owns
`_dummy`, `frame`, `frameCounter`, `spriteDirection`, `netOffset`, or `scale` as an implemented
presentation pipeline. The protocol codec contains a related critter payload parser, but that
parser is an adapter candidate and has not been run in this session.

### 5.6.5 C02 focused verifier plan

No verifier was run for this checkpoint. The implementation-stage verifier must prove:

1. full hydration resolves content IDs and size/configuration through a deterministic definition
   catalog without creating an NPC runtime entity;
2. full and partial payloads preserve the accepted field order, relative target offsets, random
   state, wait/state values, and malformed-input rejection behavior;
3. random transitions are reproducible from an explicitly supplied seed/state and do not read a
   global clock or random source;
4. authority snapshots exclude `frame`, `frameCounter`, `spriteDirection`, `netOffset`, and dummy
   internals, while render snapshots include only the intended presentation data;
5. full receive clears interpolation offset and partial receive accumulates it without changing
   behavior authority;
6. recall timer/state transitions obey the selected behavior definition and do not mutate the
   immutable catalog;
7. dummy projection copies values in and out only through the adapter, with no static dummy state
   observable as component authority.

## 5.7 C03 checkpoint: LeashedWalkerBehavior

### 5.7.1 Source facts and classification

The current Version4 declaration at
`Terraria.GameContent.LeashedEntities/WalkerLeashedCritter.cs:9-21` contains the seven P02
members. Its constructor defaults are present at `:23-27`, but `Update`, `CopyToDummy`, and
`GetDrawOffset` are empty at `:28-32` in the current checkout. The complete-reference fallback
provides transition evidence for target advancement and tile inspection at `:29-69`, the update
ordering at `:71-109`, falling at `:111-148`, walking at `:150-172`, and presentation behavior at
`:174-195`. It is a fallback behavior reference, not proof that the sparse checkout executes these
operations.

The seven members are split between definition data, a state-machine namespace, and the owner
system. Constants are not mutable ECS state and `Prototype` is a catalog entry, not an entity
component:

| Source member | Classification | Proposed owner | Readers/writers and lifecycle | Evidence |
|---|---|---|---|---|
| `Prototype` | immutable walker definition/catalog entry | `LeashedWalkerDefinitionCatalog` / `LeashedSpeciesDefinitionCatalog` | catalog bootstrap registers once; registration resolves definition ID; no runtime entity writer | current `:9`; fallback `:9`; partial |
| `State_Standing` | state discriminant definition | `LeashedWalkerDefinition` | walker owner interprets/validates `LeashedCritterBehaviorComponent.State`; compile-time definition; never serialized as separate field | current `:11`; fallback `:79-87,98-101`; partial |
| `State_PickDirection` | state discriminant definition | `LeashedWalkerDefinition` | walker owner interprets state and emits direction transition; no independent component field | current `:13`; fallback `:161-165`; partial |
| `State_Walking` | state discriminant definition | `LeashedWalkerDefinition` | walker owner interprets state and invokes target query; no independent component field | current `:15`; fallback `:164-170`; partial |
| `State_Falling` | state discriminant definition | `LeashedWalkerDefinition` | walker owner interprets falling state and recall boundary; no independent component field | current `:17`; fallback `:118-146`; partial |
| `State_Recalling` | state discriminant definition | `LeashedWalkerDefinition` | walker owner interprets recall state and emits `Recall` command; effect adapter owns visual/position side effects | current `:19`; fallback `:81-85,136-142,154-156`; partial |
| `walkingPace` | immutable movement configuration | `LeashedWalkerDefinition.WalkingPace` | constructor/catalog supplies; walker system reads to compute velocity; no tick writer | current `:21,23-27`; fallback `:158`; partial |

`State_*` values remain definition-local discriminants. The runtime `State` byte from C02 is the
only state storage; introducing a second `WalkerState` field would permit divergence. A walker
system may share the generic C02 authority transition/commit port, but it owns the interpretation
of these five values and the write intent for `State`, `WaitTime`, `TargetPosition`, `velocity`,
and `direction` through one explicit transition.

### 5.7.2 C03 interface contract

The proposed signatures are design sketches, not code:

```text
LeashedWalkerDefinitionQuery.Get(DefinitionId) -> WalkerDefinition
LeashedWalkerSystem.Evaluate(WalkerAuthority, WalkerDefinition, WalkerWorldInput)
  -> WalkerTransition
IWalkerTileQuery.IsInWorld(TileCoordinate) -> bool
IWalkerTileQuery.IsSolid(TileCoordinate) -> bool
IWalkerTileQuery.HasLiquid(TileCoordinate) -> bool
LeashedWalkerCommitPort.Commit(WalkerTransition) -> CommittedCritterFacts
LeashedRecallEffectPort.Emit(RecallRequest) -> void
LeashedWalkerPresentationProjection.Build(CommittedCritterFacts, WalkerPresentationInput)
  -> CritterRenderSnapshot
```

`IWalkerTileQuery` is a read-only deterministic world port. The system must not call `WorldGen`,
`Main`, dust, rendering, or logging directly. `Recall` is a command/effect boundary: the state
transition and position/velocity result are deterministic facts, while dust and client-only
effects are handled by an adapter. The section/lifecycle owner decides whether a walker is eligible
to tick; the walker system does not activate or remove entities.

### 5.7.3 C03 invariants and ordering

- The walker target remains within the accepted anchor-relative range, with the exact comparison
  semantics preserved from the fallback until a current-checkout verifier is available.
- `AdvanceTargetPosition` evaluates in-world bounds before tile probes and evaluates solid/liquid
  predicates in a stable order. Query results are inputs, not hidden writes.
- A falling walker applies gravity and clamps its fall speed; crossing the straying boundary enters
  recalling with the generic `RecallDuration` rule. The gravity/fall constants are behavior
  definition data and are not added to this seven-member component.
- Wait timer expiration performs recall handling, timer reselection, and state reselection in the
  same explicit transition order. Random selection uses the C02 random port.
- A standing walker zeroes velocity only after reaching its target. Direction and sprite direction
  are separate authority/presentation facts as established by C02; the walker system emits the
  direction result and the presentation projection derives the sprite cache.
- `CopyToDummy`, frame finding, half-brick draw offset and visual effects are presentation steps
  after authority commit. They cannot write `State` or `TargetPosition`.
- Proposed scheduler edges are `section/lifecycle eligibility -> tile snapshot query -> walker
  evaluate -> commit authority -> issue recall/effect command -> presentation projection`. Exact
  scheduler ownership remains `integration-review`.

### 5.7.4 C03 current NLTX mapping and risk

No walker-specific component, definition, tile query port, or owner system was found in the current
NLTX tree. `LeashedCritterBehaviorComponent` is the partial generic state candidate from C02. The
full-reference fallback's use of `WorldGen`, `Main`, `Dust`, and the static NPC dummy must be
translated into explicit ports before implementation. The current Version4 sparse method bodies
cannot establish runtime behavior, and the fallback source cannot establish current-checkout
network or persistence semantics.

### 5.7.5 C03 focused verifier plan

No verifier was run for this checkpoint. The implementation-stage verifier must prove:

1. definition registration preserves all five state discriminants and `walkingPace = 0.8f`,
   `strayingRangeInBlocks = 3` defaults without per-entity prototype mutation;
2. standing, direction-pick, walking, falling, and recalling transitions match the accepted
   fallback ordering for representative tile-query results;
3. in-world, solid, liquid, half-brick, and anchor-range boundaries are deterministic and do not
   write through the query port;
4. gravity/fall-speed and target movement results are committed exactly once by the walker owner;
5. recall emits one explicit effect command and does not call presentation/I/O from the transition
   calculation;
6. dummy/frame/sprite projection is downstream-only and cannot mutate generic behavior authority;
7. section deactivation, removal, and network application do not create a second walker writer.

## 5.8 C04 checkpoint: LeashedJumperBehavior

### 5.8.1 Source facts and classification

The current Version4 declaration at
`Terraria.GameContent.LeashedEntities/JumperLeashedCritter.cs:9-37` contains the 11 P02
members. Its constructor defaults are present at `:39-51`; `Spawn`, `Update`, `CopyToDummy`, and
`GetDrawOffset` are empty at `:53-57` in this checkout. The complete-reference fallback supplies
spawn/target initialization and update ordering at `:44-98`, jump calculation at `:100-132`,
movement/collision and falling at `:134-208`, target selection/reachability at `:210-275`, and
presentation at `:277-299`. These bodies are fallback evidence, not proof of current-checkout
runtime behavior.

The 11 members are definition data and state-discriminant data consumed by one jumper owner. The
generic C02 `State`, `WaitTime`, `TargetPosition`, velocity and direction remain the only runtime
authority fields:

| Source member | Classification | Proposed owner | Readers/writers and lifecycle | Evidence |
|---|---|---|---|---|
| `Prototype` | immutable jumper definition/catalog entry | `LeashedJumperDefinitionCatalog` / `LeashedSpeciesDefinitionCatalog` | catalog bootstrap registers once; registration resolves definition ID; no runtime entity writer | current `:9`; fallback `:9`; partial |
| `State_Normal` | state discriminant definition | `LeashedJumperDefinition` | jumper owner validates/interprets generic `State`; compile-time definition; never serialized separately | current `:11`; fallback `:62-70`; partial |
| `State_Recalling` | state discriminant definition | `LeashedJumperDefinition` | jumper owner interprets recall and emits recall command; no parallel runtime state field | current `:13`; fallback `:71-75,194-208`; partial |
| `minWaitTime` | immutable jump timing configuration | `LeashedJumperDefinition.MinWaitTime` | constructor/catalog supplies; jumper evaluator reads for cooldown selection; no tick writer | current `:15`; fallback `:104-105,72`; partial |
| `maxWaitTime` | immutable jump timing configuration | `LeashedJumperDefinition.MaxWaitTime` | constructor/catalog supplies; random port selects inclusive upper bound; no tick writer | current `:17`; fallback `:104-105`; partial |
| `maxJumpWidth` | immutable jump geometry configuration | `LeashedJumperDefinition.MaxJumpWidth` | target selection and jump evaluator read; content definition owns value | current `:19`; fallback `:113-118,214-218`; partial |
| `minJumpWidth` | immutable jump geometry configuration | `LeashedJumperDefinition.MinJumpWidth` | target selection rejects too-short jumps; evaluator reads; no runtime writer | current `:21`; fallback `:118-120,214-218`; partial |
| `maxJumpHeight` | immutable jump geometry configuration | `LeashedJumperDefinition.MaxJumpHeight` | jump eligibility and reachable-tile query read; no runtime writer | current `:23`; fallback `:111-114,258-269`; partial |
| `maxJumpDuration` | immutable jump timing configuration | `LeashedJumperDefinition.MaxJumpDuration` | jump velocity calculation reads; no mutable entity copy | current `:25`; fallback `:121-128`; partial |
| `jumpCooldown` | immutable post-jump timing configuration | `LeashedJumperDefinition.JumpCooldown` | transition adds cooldown to jump duration; no runtime writer | current `:27`; fallback `:104-105,128`; partial |
| `canStandOnWater` | immutable traversal capability | `LeashedJumperDefinition.CanStandOnWater` | target reachability and movement query read; liquid semantics delegated to world port | current `:29`; fallback `:155-160,270-275`; partial |

`State_Normal` and `State_Recalling` are labels for the generic state byte, not component members.
The definition contains the numeric discriminants and validates them; the owner system is the only
writer of the generic authority. `canStandOnWater` is a rule/configuration input and cannot be
inferred solely from an observed liquid tile, because the query port must preserve the source's
solid/liquid ordering and tile domain.

### 5.8.2 C04 interface contract

The proposed signatures are design sketches, not code:

```text
LeashedJumperDefinitionQuery.Get(DefinitionId) -> JumperDefinition
IJumperWorldQuery.IsSolid(TileCoordinate) -> bool
IJumperWorldQuery.HasLiquid(TileCoordinate, LiquidQueryMode) -> bool
IJumperWorldQuery.IsInWorld(TileCoordinate) -> bool
LeashedJumperSystem.Evaluate(JumperAuthority, JumperDefinition, JumperWorldSnapshot)
  -> JumperTransition
LeashedJumperCommitPort.Commit(JumperTransition) -> CommittedCritterFacts
LeashedRecallEffectPort.Emit(RecallRequest) -> void
```

The world snapshot must expose collision and liquid observations without allowing the evaluator to
write tiles, entities, or global state. `TryGetReachableTile`, jump velocity calculation, movement,
collision response, and anchor-range recall are pure calculations over the snapshot and explicit
authority input. A command/effect port handles dust or other client-only recall effects after commit.
The section/lifecycle system controls tick eligibility; the jumper system does not register,
activate, despawn, or allocate a legacy slot.

### 5.8.3 C04 invariants and ordering

- Constructor defaults are preserved as definition data: `strayingRangeInBlocks = 12`, wait range
  `180..300`, jump width `48..112`, height `64`, duration `30`, cooldown `60`, and water-standing
  disabled, subject to current-checkout verifier confirmation.
- Wait expiration evaluates the current state before selecting the next target/cooldown. A normal
  state attempts a jump; a failed attempt selects a new target and sets cooldown. A recalling state
  emits recall, selects a target, sets cooldown, and returns to normal in the same transition order.
- Jump velocity is derived from the target delta and definition geometry. A non-upward or too-short
  candidate is rejected without partially writing velocity or state.
- Collision response updates horizontal/vertical velocity only through the committed transition.
  `canStandOnWater` is consulted only through an explicit liquid query and does not turn liquid into
  a hidden entity or tile write.
- Reachability scans toward the anchor with explicit bounds. Malformed/out-of-world coordinates
  produce no target commit; target selection cannot escape the straying range.
- Recall and presentation are downstream of authority. Opacity, half-brick offsets, dummy animation,
  and visual effects cannot mutate the generic state or target.
- Proposed scheduler edges are `section/lifecycle eligibility -> world snapshot -> target selection /
  jump evaluation -> collision transition -> authority commit -> recall effect -> presentation`.
  Exact scheduling and world-owner decisions remain `integration-review`.

### 5.8.4 C04 current NLTX mapping and risk

No jumper-specific definition, system, world-query port, or focused verifier was found in the current
NLTX tree. `LeashedCritterBehaviorComponent` supplies only the generic state candidates. The
fallback implementation directly calls `WorldGen`, `Main`, and inherited dummy/presentation logic;
those calls are not yet an NLTX implementation and must be replaced by explicit ports. The current
Version4 methods are empty, so jump equivalence and network/persistence behavior remain unverified.

### 5.8.5 C04 focused verifier plan

No verifier was run for this checkpoint. The implementation-stage verifier must prove:

1. definition registration preserves both state discriminants and all constructor defaults without
   mutating shared prototypes;
2. target selection respects minimum/maximum width, maximum height, anchor range, in-world bounds,
   and deterministic random choices;
3. jump velocity and cooldown calculations match accepted source ordering and reject invalid jumps
   atomically;
4. solid collision, falling, water-standing, and anchor-range recall branches use only explicit
   read-only world inputs;
5. normal/recalling transitions update the generic C02 authority once and emit at most one recall
   effect per transition;
6. presentation/dummy projections cannot write target, state, velocity, or definition data;
7. section deactivation, removal, and network commands cannot introduce a second jumper writer.

## 5.9 C05 checkpoint: LeashedFlyerBehavior

### 5.9.1 Source facts and classification

The current Version4 declaration at
`Terraria.GameContent.LeashedEntities/FlyerLeashedCritter.cs:9-29` contains the 11 P02
members. Its constructor defaults are present at `:31-40`, while `Spawn`, `CopyToDummy`, `Update`,
and `GetDrawOffset` are empty at `:42-47` in this checkout. The complete-reference fallback
provides spawn/target selection at `:42-64`, dummy rotation projection at `:66-75`, movement and
collision ordering at `:77-143`, and ground/hover presentation at `:145-172`. The fallback is
behavior evidence only; it does not prove current-checkout execution.

The 11 members split into catalog configuration, one movement-rule constant, and presentation
configuration. Inherited `anchorStyle`, `strayingRangeInBlocks`, generic state, target and random
cursor remain owned by C02/C08 and are not duplicated here:

| Source member | Classification | Proposed owner | Readers/writers and lifecycle | Evidence |
|---|---|---|---|---|
| `Prototype` | immutable flyer definition/catalog entry | `LeashedFlyerDefinitionCatalog` / `LeashedSpeciesDefinitionCatalog` | catalog bootstrap registers once; registration resolves definition ID; no runtime entity writer | current `:9`; fallback `:9`; partial |
| `minWaitTime` | immutable target-selection timing configuration | `LeashedFlyerDefinition.MinWaitTime` | flyer evaluator reads when the generic wait timer expires; no tick writer | current `:11`; fallback `:82-87`; partial |
| `maxWaitTime` | immutable target-selection timing configuration | `LeashedFlyerDefinition.MaxWaitTime` | random port selects inclusive upper bound; no tick writer | current `:13`; fallback `:82-87`; partial |
| `maxFlySpeed` | immutable movement configuration | `LeashedFlyerDefinition.MaxFlySpeed` | acceleration/braking evaluator reads; no runtime writer | current `:15`; fallback `:97-104`; partial |
| `acceleration` | immutable movement configuration | `LeashedFlyerDefinition.Acceleration` | target steering evaluator reads; no runtime writer | current `:17`; fallback `:96-99`; partial |
| `brakeDuration` | immutable braking configuration | `LeashedFlyerDefinition.BrakeDuration` | distance-based speed cap evaluator reads; no runtime writer | current `:19`; fallback `:98-104`; partial |
| `rotationScalar` | presentation configuration | `LeashedFlyerPresentationDefinition.RotationScalar` | render projection derives dummy rotation from committed horizontal velocity; never authority writer | current `:21`; fallback `:66-75`; partial |
| `hoverAmplitude` | presentation configuration | `LeashedFlyerPresentationDefinition.HoverAmplitude` | bobbing projection reads when airborne; no behavior-state writer | current `:23`; fallback `:158-162,165-172`; partial |
| `hoverPeriod` | presentation configuration | `LeashedFlyerPresentationDefinition.HoverPeriod` | bobbing projection reads with an injected visual clock; no behavior-state writer | current `:25`; fallback `:158-162,169-171`; partial |
| `hasGroundBias` | immutable target-selection capability | `LeashedFlyerDefinition.HasGroundBias` | target selection reads; ground-biased Y choice is a deterministic transition input | current `:27`; fallback `:58-63`; partial |
| `HoverYVelocity` | compile-time airborne-state rule | `LeashedFlyerDefinition.HoverYVelocity` | spawn/ground transition uses the exact small vertical velocity; never stored as entity state | current `:29`; fallback `:47-50,117-128`; partial |

`rotationScalar`, `hoverAmplitude`, and `hoverPeriod` are deliberately presentation-side values even
though they are declared beside movement parameters. The fallback uses `rotationScalar` only while
copying to the dummy and uses `Main.timeForVisualEffects` for bobbing. A future implementation must
inject a visual clock and must not make a legacy `whoAmI` slot or client visual time part of the
authoritative motion state.

### 5.9.2 C05 interface contract

The proposed signatures are design sketches, not code:

```text
LeashedFlyerDefinitionQuery.Get(DefinitionId) -> FlyerDefinition
IFlyerWorldQuery.IsSolid(TileCoordinate) -> bool
IFlyerWorldQuery.IsHalfBrick(TileCoordinate) -> bool
LeashedFlyerSystem.Evaluate(FlyerAuthority, FlyerDefinition, FlyerWorldSnapshot,
  IDeterministicRandom) -> FlyerTransition
LeashedFlyerCommitPort.Commit(FlyerTransition) -> CommittedCritterFacts
IFlyerVisualClock.GetTime() -> double
LeashedFlyerPresentationProjection.Build(CommittedCritterFacts, FlyerPresentationDefinition,
  VisualTime) -> CritterRenderSnapshot
```

The world query is read-only and provides a stable collision snapshot. The flyer system owns target
selection, acceleration, braking, collision response, ground transition, and direction result; it
does not call `WorldGen`, `Main`, rendering, or effects directly. The presentation projection owns
hover bobbing, half-brick offset and dummy rotation. Section/lifecycle eligibility and network
application remain upstream/downstream boundaries, not flyer writers.

### 5.9.3 C05 invariants and ordering

- Flyer defaults are immutable definition data: `minWaitTime = 60`, `maxWaitTime = 300`,
  `maxFlySpeed = 1f`, `acceleration = 0.2f`, `brakeDuration = 10`, `anchorStyle = 4`, and
  `strayingRangeInBlocks = 7`; inherited values are recorded here for validation but are not
  duplicate C05 members.
- Target selection uses the C02 random port and keeps X within the declared straying range and Y
  at or above the anchor. `hasGroundBias` may force the anchor Y only under its explicit source
  condition; it must not be inferred from a mutable visual flag.
- Movement guards zero-length target deltas before normalization. Acceleration, braking and the
  minimum speed floor are applied in a stable order; the transition clamps speed without writing
  twice.
- Collision input is queried at the predicted position. A collision stops horizontal movement and
  chooses `0` or `HoverYVelocity` based on the ground observation; unobstructed movement updates
  position and rechecks ground support before committing the next vertical velocity.
- Direction changes derive from the sign of committed horizontal velocity. `spriteDirection` and
  dummy rotation are presentation outputs and cannot become a second movement authority.
- Hover bobbing consumes an injected visual clock and definition parameters only. `whoAmI`, global
  `Main.timeForVisualEffects`, and client-side lighting/effects remain adapter inputs, not durable
  or network-authoritative state.
- Proposed scheduler edges are `section/lifecycle eligibility -> world snapshot -> target/random
  evaluation -> motion/collision transition -> authority commit -> visual-clock projection ->
  dummy/draw adapter`; exact scheduling remains `integration-review`.

### 5.9.4 C05 current NLTX mapping and risk

No flyer-specific definition, system, world-query port, visual-clock port, or focused verifier was
found in the current NLTX tree. The generic `LeashedCritterBehaviorComponent` is only the C02
authority candidate. The fallback directly uses `WorldGen`, `Main.timeForVisualEffects`, the legacy
slot and the static NPC dummy; these are not current NLTX implementation evidence and must be
isolated behind ports. The current Version4 flyer methods are empty, so motion, hover and network
equivalence remain unverified.

### 5.9.5 C05 focused verifier plan

No verifier was run for this checkpoint. The implementation-stage verifier must prove:

1. flyer definition registration preserves all declared defaults and does not mutate shared
   prototypes or inherited C02 configuration;
2. target selection, ground bias, wait range and random cursor progression are deterministic;
3. zero-distance normalization, acceleration, braking, speed floor and max-speed clamping match
   the accepted source ordering;
4. solid collision and ground/air transitions use only read-only world snapshots and preserve the
   exact `HoverYVelocity` threshold;
5. direction changes are committed once and presentation rotation/hover output cannot write motion
   authority;
6. injected visual time produces bobbing only in render projections and never changes network or
   persistence authority;
7. section deactivation, removal, and network commands do not create a second flyer writer.

## 5.10 C06 checkpoint: LeashedKiteBehavior

### 5.10.1 Source facts and classification

The current Version4 declaration at `Terraria.GameContent.LeashedEntities/LeashedKite.cs:11-54`
contains all 20 P02 members, but `NetSend`, `NetReceive`, `Draw`, `Update`, and `Spawn` are empty
at `:51-54` in this checkout. The complete-reference fallback gives full/partial wire ordering at
`:60-104`, first-appearance adjustment at `:106-116`, draw and update entry points at `:117-177`,
movement/dummy bridging at `:179-247`, spawn initialization at `:248-257`, and dummy copy at
`:259-294`. These fallback bodies establish candidate side-effect order only; they do not prove
current-checkout behavior or integration closure.

Kite fields cross four boundaries: immutable Projectile definition, authoritative wind/motion
state, compatibility state exchanged with the Projectile logic adapter, and presentation/network
interpolation caches. The 20 members are therefore not one flat component:

| Source member | Classification | Proposed owner | Readers/writers and lifecycle | Evidence |
|---|---|---|---|---|
| `Prototype` | immutable kite definition/catalog entry | `LeashedKiteDefinitionCatalog` / `LeashedSpeciesDefinitionCatalog` | catalog bootstrap registers once; registration resolves definition ID; no runtime writer | current `:11`; fallback `:11`; partial |
| `_dummy` | ephemeral Projectile logic/presentation adapter state | `LeashedProjectileDummyAdapter` | scoped adapter copies committed facts in/out for kite logic and draw; never serialized or persisted | current `:13`; fallback `:259-294,117-124`; partial |
| `projType` | Projectile content identifier and full-sync definition input | `LeashedKiteBehaviorComponent.ProjectileType` plus definition query | definition hydration writes once; full packet adapter validates; projectile runtime is external | current `:15`; fallback `:51-58,64-67,80-83`; partial |
| `frame` | Projectile presentation animation state | `LeashedKiteVisualProjection.Frame` | dummy/animation adapter writes; draw reads; not a durable authority field | current `:17`; fallback `:265-266,285-286`; partial |
| `frameCounter` | Projectile presentation animation counter | `LeashedKiteVisualProjection.FrameCounter` | dummy/animation adapter writes; draw reads; not a network/persistence authority field | current `:19`; fallback `:265-266,285-286`; partial |
| `rotation` | motion/render orientation fact | `LeashedKiteBehaviorComponent.Rotation` with kite owner commit | movement/Projectile adapter produces; full/partial packet projection transfers; owner commits accepted result | current `:21`; fallback `:68-73,85-90,234-236,288-289`; partial |
| `spriteDirection` | presentation orientation and trail cache value | `LeashedKiteVisualProjection.SpriteDirection` | dummy logic and trail projection write/read; not present in the kite packet payload shown by fallback | current `:23`; fallback `:189-191,270,290`; partial |
| `kiteDistance` | derived movement-control state | `LeashedKiteBehaviorComponent.KiteDistance` | kite system derives from no-wind duration and passes to Projectile logic; no external writer | current `:25`; fallback `:172-175,275`; partial |
| `windTarget` | environment input/replicated kite state | `LeashedKiteBehaviorComponent.WindTarget` plus `IKiteEnvironmentPort` | environment adapter supplies on authority side; full/partial packet reads/writes; owner commits validated value | current `:27`; fallback `:70-73,88-90,158-171`; partial |
| `windCurrent` | derived wind state | `LeashedKiteBehaviorComponent.WindCurrent` | kite system interpolates or fast-forwards from target and environment; no packet field directly shown | current `:29`; fallback `:163-170`; partial |
| `timeCounter` | simulation/render input time used by kite logic | `LeashedKiteBehaviorComponent.TimeCounter` with explicit time port | kite owner increments; full/partial packet transfers; must not read a hidden global clock | current `:31`; fallback `:70-73,89-90,193-200`; partial |
| `cloudAlpha` | external visual environment input | `LeashedKiteVisualEnvironmentProjection` / `IKiteEnvironmentPort` | environment adapter supplies and packet projection transfers; not persistent entity identity or content | current `:33`; fallback `:71-73,88-90,158-162,197-200`; partial |
| `timeWithoutWind` | authoritative/derived no-wind timer | `LeashedKiteBehaviorComponent.TimeWithoutWind` | kite owner increments/resets from wind state; derives distance; no hidden environment writer | current `:35`; fallback `:172-175`; partial |
| `projectileLocalAI0` | Projectile adapter compatibility state | `LeashedProjectileCompatibilityState` | dummy copy/first-appearance adapter reads/writes; not ordinary Projectile runtime authority | current `:37`; fallback `:106-114,276-277,291-292`; partial |
| `projectileLocalAI1` | Projectile adapter compatibility state | `LeashedProjectileCompatibilityState` | dummy copy/first-appearance adapter reads/writes; not ordinary Projectile runtime authority | current `:39`; fallback `:106-114,276-277,291-292`; partial |
| `oldPos` | trail/presentation history cache | `LeashedKiteVisualProjection.TrailPositions` | initialized from content trail length, shifted or fast-forwarded; draw reads; excluded from authority/persistence | current `:41`; fallback `:138-156,222-236,271`; partial |
| `oldRot` | trail orientation cache | `LeashedKiteVisualProjection.TrailRotations` | shifted/fast-forwarded with trail positions; draw reads; excluded from authority/persistence | current `:43`; fallback `:138-156,222-236,272`; partial |
| `oldSpriteDirection` | trail orientation cache | `LeashedKiteVisualProjection.TrailDirections` | shifted/fast-forwarded with trail positions; draw reads; excluded from authority/persistence | current `:45`; fallback `:138-156,222-236,273`; partial |
| `netOffset` | client interpolation cache | `LeashedKiteInterpolationProjection` | full receive clears, partial receive accumulates position delta, update decays; never authority/persistence | current `:47`; fallback `:84-98,176`; partial |
| `AnchorWorldPosition` | pure anchor-to-world derived query | `LeashedAnchorWorldPositionQuery` | reads entity anchor relation and coordinate conversion; no stored writer or persistence authority | current `:49`; fallback `:186`; partial |

`projType` is a content identifier, not a Projectile runtime index owned by P02. `_dummy` is a
compatibility adapter around an external Projectile API; it is not a second Projectile entity.
`oldPos`, `oldRot`, `oldSpriteDirection`, `frame`, `frameCounter`, and `netOffset` are caches and
must not be saved as the durable entity identity or section index. `AnchorWorldPosition` remains a
pure query and does not move ownership of the anchor TileEntity into this partition.

### 5.10.2 C06 interface contract

The proposed signatures are design sketches, not code:

```text
LeashedKiteDefinitionQuery.Get(DefinitionId) -> KiteDefinition
IKiteEnvironmentPort.Read(KiteEnvironmentRequest) -> KiteEnvironmentSnapshot
IProjectileDefinitionPort.Resolve(ProjectileContentId) -> ProjectilePresentationDefinition
LeashedKiteSystem.Evaluate(KiteAuthority, KiteDefinition, KiteEnvironmentSnapshot, KiteTime)
  -> KiteTransition
LeashedProjectileLogicPort.Evaluate(ProjectileCompatibilityInput)
  -> ProjectileCompatibilityResult
LeashedKiteCommitPort.Commit(KiteTransition, ProjectileCompatibilityResult)
  -> CommittedKiteFacts
LeashedKiteReplicationProjection.CreateFull/Partial(CommittedKiteFacts)
LeashedKiteInterpolationProjection.ApplyFull/Partial(Position)
LeashedKiteVisualProjection.Build(CommittedKiteFacts, TrailHistory, VisualEnvironment)
  -> KiteRenderSnapshot
```

The kite owner receives wind, cloud, section availability, time and projectile-definition facts
through explicit ports. It commits only the accepted kite authority. The Projectile adapter may
return compatibility state, but cannot allocate or mutate a global Projectile runtime entity. The
network projection owns packet framing through the protocol adapter; it cannot apply packets
directly to component storage. Visual/trail/interpolation projections are downstream-only.

### 5.10.3 C06 invariants and ordering

- Full kite payload resolves `projType` before applying size/Projectile compatibility state. Unknown
  content IDs are rejected without partially mutating the entity.
- Full receive resets `netOffset`; partial receive accumulates the position delta; the interpolation
  cache decays independently of wind and authority.
- `windCurrent` is derived from an explicit environment snapshot and `windTarget`; `timeWithoutWind`
  resets on accepted wind and increments otherwise. `kiteDistance` is derived from that timer and
  is not an externally writable identity field.
- Normal and fast-forward update are separate transition modes with explicit input. Fast-forward
  may initialize trail history and first appearance, but cannot bypass lifecycle or section checks.
- Projectile dummy state is copied through a scoped adapter. `projectileLocalAI0/1` are compatibility
  values and cannot be treated as ordinary Projectile AI authority or shared global state.
- Trail arrays are fixed by an external content definition and rebuilt when the definition changes;
  they are render cache, not durable state. Trail shifting must not alter position/velocity authority
  except through the returned Projectile compatibility result.
- `AnchorWorldPosition` is a pure query over `AnchorPosition`; anchor TileEntity persistence and tile
  removal remain external integration owners.
- Proposed scheduler edges are `lifecycle/section eligibility -> environment snapshot -> kite
  evaluate -> Projectile compatibility port -> authority commit -> network projection and
  interpolation -> trail/render projection`. Exact network, world, and Projectile ownership remains
  `integration-review`.

### 5.10.4 C06 current NLTX mapping and risk

`src/LeashedEntity/LeashedKiteBehaviorComponent.cs` contains candidates for `ProjectileType`,
`Rotation`, `KiteDistance`, `WindTarget`, `WindCurrent`, `TimeCounter`, `TimeWithoutWind`, and
`ProjectileLocalAI0/1`. It does not establish a kite owner system, environment port, Projectile
compatibility adapter, trail projection, network application, or interpolation writer. Existing dome
protocol code is an adapter candidate only; it must not dual-write the `src` component model. The
current Version4 kite methods are empty, while the fallback directly uses `Main`, `WorldGen`,
`Projectile`, collision, wind and player state. Those cross-domain effects remain `partial` and
`integration-review`.

### 5.10.5 C06 focused verifier plan

No verifier was run for this checkpoint. The implementation-stage verifier must prove:

1. full/partial packet order, `projType` validation, position/velocity/rotation quantization, wind,
   cloud and time fields, and interpolation reset/accumulation;
2. wind/no-wind transitions, `timeWithoutWind`, `kiteDistance`, normal update and fast-forward are
   deterministic from explicit environment/time inputs;
3. Projectile compatibility state is scoped, does not allocate a runtime Projectile, and cannot
   become a second authority writer;
4. trail arrays initialize to the accepted content length, shift/fast-forward correctly, and remain
   excluded from persistence and authority snapshots;
5. collision, player/anchor, section-availability and first-appearance effects use explicit ports;
6. `AnchorWorldPosition` is pure and anchor TileEntity ownership remains outside P02;
7. render/interpolation projections cannot mutate kite authority or persistent identity.

## 5.11 C07 checkpoint: LeashedButterflyVariants

### 5.11.1 Source facts and classification

The current Version4 declarations contain the six P02 members in
`EmpressButterflyLeashedCritter.cs:7-13` and `NormalButterflyLeashedCritter.cs:7-9`, but their
override bodies are empty in the current checkout. The complete-reference fallback shows the
normal variant source and full-only wire suffix at `NormalButterflyLeashedCritter.cs:11-48`, and
Empress fade/opacity and client-only visual effects at
`EmpressButterflyLeashedCritter.cs:15-63`. This is partial behavior evidence, not a current
checkout implementation claim.

The two butterfly types share the generic C02/C05 flyer authority but have different ownership
semantics:

| Source member | Classification | Proposed owner | Readers/writers and lifecycle | Evidence |
|---|---|---|---|---|
| `EmpressButterflyLeashedCritter.Prototype` | immutable species/variant definition entry | `LeashedSpeciesDefinitionCatalog` | catalog bootstrap registers once; registration resolves definition ID; no runtime entity writer | current `EmpressButterflyLeashedCritter.cs:7`; fallback `:7`; partial |
| `fadeAmount` | presentation-only proximity/fade cache | `LeashedButterflyPresentationComponent` / `LeashedButterflyVisualProjection` | client visual system updates from explicit viewer-distance input; draw projection reads; never full/partial authority state | current `:9`; fallback `:28-38`; partial |
| `FadeAwayCap` | immutable visual clamp rule | `LeashedButterflyDefinition.FadeAwayCap` | opacity/fade query reads; compile-time/config rule; never serialized as mutable entity state | current `:11`; fallback `:33`; partial |
| `NormalButterflyLeashedCritter.Prototype` | immutable species/variant definition entry | `LeashedSpeciesDefinitionCatalog` | catalog bootstrap registers once; registration resolves definition ID; no runtime entity writer | current `NormalButterflyLeashedCritter.cs:7`; fallback `:7`; partial |
| `variant` | per-entity content/style variant | `LeashedButterflyVariantComponent.Variant` plus full-sync network adapter | item/default hydration writes; full packet projection writes through validated command; dummy presentation reads; no per-tick behavior writer | current `:9`; fallback `:11-17,23-24,31-45`; partial |
| `Opacity` | pure presentation query | `LeashedButterflyOpacityQuery` | derives from `fadeAmount` and immutable cap; render projection reads; no stored writer or network field | current Empress `:13`; fallback `:38`; partial |

`variant` is not an NPC runtime ID and must not be inferred from the inherited flyer `npcType`.
The fallback derives it from `Item.placeStyle` and sends it only in full butterfly payloads. The
adapter must therefore retain whether the command is full or partial and reject a partial payload
that attempts to overwrite an absent suffix. `fadeAmount` is driven by client viewer distance and
visual effects in the fallback; it is not part of the deterministic authority snapshot.

### 5.11.2 C07 interface contract

The proposed signatures are design sketches, not code:

```text
LeashedButterflyDefinitionQuery.Get(DefinitionId) -> ButterflyDefinition
IItemVariantPort.Read(ItemContentId) -> ButterflyVariant
LeashedButterflyVariantCommand.ApplyFull(Variant) -> Command
LeashedButterflyOpacityQuery.Get(FadeAmount, FadeAwayCap) -> float
IButterflyVisualInputPort.Read(ViewerDistance, VisualTime, RandomSource)
LeashedButterflyPresentationSystem.Evaluate(ButterflyVisualState, VisualInput)
  -> ButterflyVisualTransition
LeashedButterflyVisualProjection.Build(CommittedButterflyFacts, ButterflyVisualTransition)
  -> ButterflyRenderSnapshot
```

Item content and style are read through an explicit adapter. The full network adapter validates
the variant range and emits a command; it does not mutate the component. The presentation system
owns fade and light/dust requests through effect ports, while `Opacity` remains a pure query. No
butterfly projection may write flyer motion, generic state, anchor relation, lifecycle, or item
inventory state.

### 5.11.3 C07 invariants and ordering

- Both `Prototype` fields are catalog definitions, not two runtime components or mutable entity
  instances. They must be registered deterministically with the species catalog.
- `variant` is hydrated from the accepted item/style source and is included only in the full normal
  butterfly suffix. Partial snapshots preserve the prior variant unless an explicit command says
  otherwise.
- Variant range validation occurs before commit; malformed or unknown values leave prior authority
  unchanged and cannot alter the inherited flyer content ID.
- `fadeAmount` is clamped to `0..FadeAwayCap` by the presentation system using explicit viewer
  distance input. It is excluded from persistence, authority comparison, and network identity.
- `Opacity` is pure and deterministic for a given fade amount and cap. It cannot emit lighting,
  dust, or mutation.
- Lighting, dust, random particle selection, local player distance and global visual time are
  effect/visual adapter inputs. They execute after authority/network application and never from
  the definition catalog or item adapter.
- Proposed scheduler edges are `definition/item hydration -> full variant command -> flyer authority
  update -> visual input snapshot -> fade transition -> opacity query -> render/effect projection`.
  Exact client/network ownership remains `integration-review`.

### 5.11.4 C07 current NLTX mapping and risk

No `LeashedButterflyVariantComponent` or butterfly visual owner was found in the current NLTX tree.
The existing protocol verifier contains butterfly/Shimmer suffix coverage, but it was not run in
this session and does not establish runtime component ownership. The current Version4 override bodies
are empty; the complete-reference fallback's item, player, lighting, dust, and global-time calls
must be isolated before implementation. `fadeAmount` and `Opacity` must not be added to the generic
authority component merely because the legacy class stores them beside the prototype.

### 5.11.5 C07 focused verifier plan

No verifier was run for this checkpoint. The implementation-stage verifier must prove:

1. deterministic registration of both butterfly definitions without per-entity prototype mutation;
2. item/style hydration and full-only variant suffix encoding/decoding with range and malformed-input
   rejection;
3. partial snapshots preserve variant authority and do not consume a missing suffix;
4. fade amount clamp, cap, opacity query purity and viewer-distance boundary behavior;
5. lighting/dust/random visual effects are downstream ports and cannot write authority, inventory,
   anchor, or lifecycle state;
6. variant and fade/opacity data are excluded or explicitly classified in persistence and network
   snapshots;
7. butterfly visual projection cannot create a second flyer behavior writer.

## 5.12 C08 checkpoint: LeashedSpeciesPrototypes

### 5.12.1 Source facts and classification

The authoritative report contains 13 static `Prototype` fields, one for each species definition
entry. The current Version4 declarations preserve those fields and the related constructors. The
constructor evidence was read from the current Version4 files: Bird sets flyer/anchor and hover
defaults, Crawler and Runner set walker pace, CrawlingFly and Waterfowl set ground bias, Dragonfly
and Fairy set flyer timing/speed, Fish sets aquatic flyer values, and WaterStrider sets jumper
water/range parameters. The source fields are static prototype objects, not per-entity runtime
components. The exact registry type IDs and item/NPC content mapping remain outside this member
set and are `integration-review` inputs.

Each source member maps to one immutable catalog row. The row may refer to a shared behavior
definition; it must not create a separate component type or a mutable prototype copy per entity:

| Source member | Definition row and inherited behavior | Constructor/configuration evidence | Proposed owner and unresolved input |
|---|---|---|---|
| `BirdLeashedCritter.Prototype` | Flyer species definition | `anchorStyle=2`, wait `120..420`, speed `1.2`, acceleration `0.1`, rotation scalar `0.25`, brake `10`, hover `3/0.005` | `LeashedSpeciesDefinitionCatalog.Bird`; exact content/type binding deferred to registry/item integration |
| `CrawlerLeashedCritter.Prototype` | Walker species definition | `anchorStyle=1`, walking pace `0.4` | `LeashedSpeciesDefinitionCatalog.Crawler`; exact content/type binding deferred |
| `CrawlingFlyLeashedCritter.Prototype` | Flyer species definition with ground bias | `hasGroundBias=true`; custom `SetDefaults` override exists in source shape | `LeashedSpeciesDefinitionCatalog.CrawlingFly`; item-to-NPC/content semantics deferred |
| `DragonflyLeashedCritter.Prototype` | Flyer species definition | min wait `10`, max speed `2.5`, acceleration `0.4`, brake `10` | `LeashedSpeciesDefinitionCatalog.Dragonfly`; exact type binding deferred |
| `FairyLeashedCritter.Prototype` | Flyer species definition | wait `30..90`, speed `1.1`, acceleration `0.05`, rotation scalar `0.25`, brake `30` | `LeashedSpeciesDefinitionCatalog.Fairy`; exact type binding deferred |
| `FireflyLeashedCritter.Prototype` | Flyer species definition with visual/dummy overrides | no constructor override; custom `CopyToDummy` and `VisualEffects` source seams | `LeashedSpeciesDefinitionCatalog.Firefly`; presentation override policy deferred |
| `FishLeashedCritter.Prototype` | Aquatic flyer species definition | `anchorStyle=3`, min wait `120`, speed `0.5`, acceleration `0.015`, hover `10/0.003`, `isAquatic=true` | `LeashedSpeciesDefinitionCatalog.Fish`; aquatic content and anchor host mapping deferred |
| `HellButterflyLeashedCritter.Prototype` | Flyer species definition with visual override | no constructor override; custom `VisualEffects` source seam | `LeashedSpeciesDefinitionCatalog.HellButterfly`; presentation/content mapping deferred |
| `RunnerLeashedCritter.Prototype` | Walker species definition | `anchorStyle=1`, walking pace `1.5` | `LeashedSpeciesDefinitionCatalog.Runner`; exact content/type binding deferred |
| `ShimmerFlyLeashedCritter.Prototype` | Flyer species definition with custom defaults/network/visual seams | custom `SetDefaults`, `NetSend`, `NetReceive`, `VisualEffects`, and `Draw` source seams | `LeashedSpeciesDefinitionCatalog.ShimmerFly`; suffix/content/protocol ownership deferred to integration-review |
| `SnailLeashedCritter.Prototype` | Crawler/Walker species definition | inherits crawler anchor/pace; custom `SetDefaults` and `VisualEffects` seams | `LeashedSpeciesDefinitionCatalog.Snail`; content and presentation mapping deferred |
| `WaterfowlLeashedCritter.Prototype` | Bird/Flyer species definition with ground bias | inherits Bird values; `hasGroundBias=true` | `LeashedSpeciesDefinitionCatalog.Waterfowl`; exact content/type binding deferred |
| `WaterStriderLeashedCritter.Prototype` | Jumper species definition with water support | wait `60..120`, range `5`, width `8..32`, height `0`, duration `14`, cooldown `15`, `canStandOnWater=true` | `LeashedSpeciesDefinitionCatalog.WaterStrider`; item/liquid/anchor mapping deferred |

The catalog owns the immutable row and a deterministic lookup from the accepted definition ID. It
does not own item inventory, NPC/Projectile runtime instances, anchor TileEntities, packet slots, or
render resources. Custom override seams such as `SetDefaults`, `NetSend`, `VisualEffects`, and
`Draw` are represented as explicit definition capabilities or adapters and must not reintroduce
inheritance-based runtime state ownership.

### 5.12.2 C08 interface contract

The proposed signatures are design sketches, not code:

```text
LeashedSpeciesDefinitionCatalog.RegisterAll(DefinitionRegistrationInput)
LeashedSpeciesDefinitionCatalog.TryGet(DefinitionId) -> LeashedSpeciesDefinition?
LeashedSpeciesDefinitionQuery.GetBehaviorDefinition(DefinitionId)
  -> WalkerDefinition | JumperDefinition | FlyerDefinition
LeashedSpeciesDefinitionQuery.GetPresentationCapabilities(DefinitionId)
  -> PresentationCapabilities
LeashedContentBindingAdapter.Resolve(ItemContentId, DefinitionId)
  -> ContentBinding
LeashedEntityRegistrationSystem.Register(DefinitionId, AnchorCoordinate, SpawnReason)
  -> RegistrationCommand
```

`RegisterAll` is the only catalog bootstrap writer. It validates that every accepted prototype row
has one definition ID, behavior kind, default bundle and explicit override capability. Runtime
systems query immutable rows; they do not call constructors or mutate prototypes. The content
binding adapter supplies item/NPC/Projectile IDs through explicit integration ports, and the
registration system remains the only proposed entity commit root.

### 5.12.3 C08 invariants and ordering

- All 13 rows are registered exactly once in deterministic order through the accepted registry
  boundary. No row is represented by a per-species runtime component.
- A prototype object is never stored as entity authority. Entity state stores a definition ID and
  capability components selected from the immutable row; changing a catalog row requires a new
  definition registration/version decision, not an in-place mutation.
- Shared behavior definitions are referenced by kind (`Walker`, `Jumper`, `Flyer`) while species
  overrides remain data/capability values. Inheritance depth and source type names do not define
  runtime scheduling order.
- Item content, NPC content, Projectile content, anchor style, liquid capability, packet suffixes
  and custom visual/dummy behavior are validated at adapters. Unknown or incompatible mappings
  fail registration without creating a partially initialized entity.
- Catalog bootstrap precedes entity hydration; entity definition binding precedes behavior-system
  evaluation; behavior systems query catalog rows but cannot write them. Presentation and network
  projections consume committed facts after behavior commit.
- `Prototype` fields are not persistence IDs, network slots, `whoAmI` slots, TileEntity handles,
  or runtime entity references. Those identity domains remain distinct per C01.
- Proposed scheduler edges are `catalog bootstrap -> content binding validation -> entity
  registration/hydration -> section/lifecycle reconciliation -> behavior owner -> network/render/
  persistence projections`. Exact registry, item and anchor owners remain `integration-review`.

### 5.12.4 C08 current NLTX mapping and risk

`src/LeashedEntity/LeashedEntityStateComponent.cs` provides a definition-ID candidate, but no
immutable 13-row species catalog or deterministic bootstrap was found. Existing behavior components
are generic fragments and must not be expanded into one component per species. The P02 report and
current Version4 declarations confirm the prototype members and constructor shape; the full
reference supplies additional override behavior, but neither source closes item/NPC mapping,
stable IDs, persistence or network application. The catalog remains proposed only.

### 5.12.5 C08 focused verifier plan

No verifier was run for this checkpoint. The implementation-stage verifier must prove:

1. all 13 prototype rows register once, in deterministic accepted order, with no duplicate definition
   ID or mutable prototype alias;
2. every row resolves exactly one behavior kind and the constructor/default bundle recorded above;
3. item/NPC/Projectile content binding and custom override capabilities reject unknown or conflicting
   mappings before entity creation;
4. registry lookup is immutable after bootstrap and runtime entities store definition IDs rather
   than prototype object references;
5. shared behavior systems read catalog data without writing it or using inheritance order as the
   scheduler contract;
6. network, persistence, anchor and presentation projections consume committed definition facts and
   cannot become catalog writers;
7. registration rollback leaves no partially initialized entity or leaked external resource.

## 6. Member coverage ledger

C01-C08 are complete at their design checkpoints. All 98 members are mapped below and in the
per-member checkpoint tables; no member is intentionally omitted. The authoritative report remains
the canonical declaration ledger and this table records only proposed ownership.

| Source rows | Leaf group | Current checkpoint mapping |
|---|---|---|
| 141-157 | `LeashedRegistryAndSections` | mapped individually in section 5.1 |
| 93-107 | `LeashedCritterCoreState` | mapped individually in section 5.6.1; definition, authority, presentation, network and dummy seams are explicit |
| 132-138 | `LeashedWalkerBehavior` | mapped individually in section 5.7.1; state discriminants remain definition data and `walkingPace` is read-only configuration |
| 82-92 | `LeashedJumperBehavior` | mapped individually in section 5.8.1; state discriminants remain definition data and all jump parameters are immutable configuration |
| 70-80 | `LeashedFlyerBehavior` | mapped individually in section 5.9.1; movement parameters are immutable definition data and hover/rotation values are presentation configuration |
| 108-126,154 | `LeashedKiteBehavior` | mapped individually in section 5.10.1; authority, environment, Projectile compatibility, trail, interpolation and pure anchor query seams are explicit |
| 64-66,127-128,153 | `LeashedButterflyVariants` | mapped individually in section 5.11.1; variant is full-sync content state and fade/opacity are presentation-only |
| 60-63,67-69,81,129-131,139-140 | `LeashedSpeciesPrototypes` | mapped individually in section 5.12.1; each static prototype is one immutable catalog row and runtime fan-out is prohibited |

Coverage total: `15 + 15 + 7 + 11 + 11 + 20 + 6 + 13 = 98` members. Each checkpoint table
assigns every source member to a proposed `Component`, `System`, `Definition`, `Query`, `Command`,
`Adapter`, `Projection`, or an explicit `integration-review` deferred boundary. The component-only
implementation records above claim only the listed component schemas; no non-component behavior or
verification is claimed.

### 6.1 Explicit 98-member source-row ledger

The following ledger is intentionally one row per authoritative source sequence. `partial` means
the declaration/member inventory is confirmed while runtime readers, writers, lifecycle,
serialization, persistence, or current-checkout behavior still require focused verification.

| Source row | Leaf group | Member | Mapping kind | Proposed owner | Evidence |
|---:|---|---|---|---|---|
| 141 | `LeashedRegistryAndSections` | `Prototypes` | Definition/Adapter | `LeashedDefinitionCatalogAdapter` | partial |
| 142 | `LeashedRegistryAndSections` | `coordinates` | Adapter/Index | `LeashedSectionIndexAdapter` | partial |
| 143 | `LeashedRegistryAndSections` | `active` (section) | Adapter/System | `LeashedSectionActivationSystem`; integration-review | partial |
| 144 | `LeashedRegistryAndSections` | `list` | Adapter/Index | `LeashedSectionIndexAdapter` | partial |
| 145 | `LeashedRegistryAndSections` | `count` | Adapter/Index | `LeashedSectionIndexAdapter` | partial |
| 146 | `LeashedRegistryAndSections` | `emptySlots` | Adapter/Index | `LeashedSectionIndexAdapter` | partial |
| 147 | `LeashedRegistryAndSections` | `BySection` | Adapter/Index | `LeashedSectionIndexAdapter` | partial |
| 148 | `LeashedRegistryAndSections` | `ActiveSectionList` | Adapter/Index | `LeashedSectionIndexAdapter` | partial |
| 149 | `LeashedRegistryAndSections` | `ByWhoAmI` | Adapter/Compatibility | `LeashedEntityRegistrationSystem` + `LeashedEntityLegacySlotComponent` | partial |
| 150 | `LeashedRegistryAndSections` | `sectionSlot` | Component/Adapter | `LeashedEntitySectionMembershipComponent` + section index | partial |
| 151 | `LeashedRegistryAndSections` | `active` (entity) | Component/System | `LeashedEntityLifecycleComponent` + registration system | partial |
| 152 | `LeashedRegistryAndSections` | `whoAmI` | Compatibility Component/Projection | `LeashedEntityLegacySlotComponent` + replication projection | partial |
| 155 | `LeashedRegistryAndSections` | `Type` | Component/Definition | `LeashedEntityStateComponent` + definition catalog | partial |
| 156 | `LeashedRegistryAndSections` | `AnchorPosition` | Component/Command | `LeashedEntityAnchorRelationComponent`; integration-review | partial |
| 157 | `LeashedRegistryAndSections` | `SectionCoordinates` | Query | `LeashedSectionCoordinatesQuery` | partial |
| 93 | `LeashedCritterCoreState` | `_dummy` | Adapter | `LeashedCritterDummyAdapter` | partial |
| 94 | `LeashedCritterCoreState` | `anchorStyle` | Definition/Query | `LeashedCritterDefinitionCatalog` | partial |
| 95 | `LeashedCritterCoreState` | `npcType` | Component/Adapter | `LeashedCritterBehaviorComponent.NpcType` + content/network adapter | partial |
| 96 | `LeashedCritterCoreState` | `spriteDirection` | Projection | `LeashedCritterPresentationProjection` | partial |
| 97 | `LeashedCritterCoreState` | `frame` | Projection | `LeashedCritterPresentationProjection` | partial |
| 98 | `LeashedCritterCoreState` | `frameCounter` | Projection | `LeashedCritterPresentationProjection` | partial |
| 99 | `LeashedCritterCoreState` | `rand` | Component/Adapter | `LeashedCritterBehaviorComponent.RandomState` + deterministic random port | partial |
| 100 | `LeashedCritterCoreState` | `WaitTime` | Component/System | `LeashedCritterBehaviorComponent` + selected behavior owner | partial |
| 101 | `LeashedCritterCoreState` | `State` | Component/System | `LeashedCritterBehaviorComponent` + selected behavior owner | partial |
| 102 | `LeashedCritterCoreState` | `TargetPosition` | Component/System | `LeashedCritterBehaviorComponent` + selected behavior owner | partial |
| 103 | `LeashedCritterCoreState` | `netOffset` | Projection | `LeashedCritterInterpolationProjection` | partial |
| 104 | `LeashedCritterCoreState` | `scale` | Definition/Adapter | definition query + `LeashedCritterDummyAdapter` | partial |
| 105 | `LeashedCritterCoreState` | `strayingRangeInBlocks` | Definition/Query | `LeashedCritterDefinitionCatalog` | partial |
| 106 | `LeashedCritterCoreState` | `isAquatic` | Definition/Query | `LeashedCritterDefinitionCatalog` + world/presentation query | partial |
| 107 | `LeashedCritterCoreState` | `RecallDuration` | Definition/System | selected behavior definition/system | partial |
| 132 | `LeashedWalkerBehavior` | `Prototype` | Definition/Adapter | `LeashedWalkerDefinitionCatalog` | partial |
| 133 | `LeashedWalkerBehavior` | `State_Standing` | Definition/System | `LeashedWalkerDefinition` + `LeashedWalkerSystem` | partial |
| 134 | `LeashedWalkerBehavior` | `State_PickDirection` | Definition/System | `LeashedWalkerDefinition` + `LeashedWalkerSystem` | partial |
| 135 | `LeashedWalkerBehavior` | `State_Walking` | Definition/System | `LeashedWalkerDefinition` + `LeashedWalkerSystem` | partial |
| 136 | `LeashedWalkerBehavior` | `State_Falling` | Definition/System | `LeashedWalkerDefinition` + `LeashedWalkerSystem` | partial |
| 137 | `LeashedWalkerBehavior` | `State_Recalling` | Definition/Command | `LeashedWalkerSystem` + recall command port | partial |
| 138 | `LeashedWalkerBehavior` | `walkingPace` | Definition/Query | `LeashedWalkerDefinition.WalkingPace` | partial |
| 82 | `LeashedJumperBehavior` | `Prototype` | Definition/Adapter | `LeashedJumperDefinitionCatalog` | partial |
| 83 | `LeashedJumperBehavior` | `State_Normal` | Definition/System | `LeashedJumperDefinition` + `LeashedJumperSystem` | partial |
| 84 | `LeashedJumperBehavior` | `State_Recalling` | Definition/Command | `LeashedJumperSystem` + recall command port | partial |
| 85 | `LeashedJumperBehavior` | `minWaitTime` | Definition/Query | `LeashedJumperDefinition.MinWaitTime` | partial |
| 86 | `LeashedJumperBehavior` | `maxWaitTime` | Definition/Query | `LeashedJumperDefinition.MaxWaitTime` | partial |
| 87 | `LeashedJumperBehavior` | `maxJumpWidth` | Definition/Query | `LeashedJumperDefinition.MaxJumpWidth` | partial |
| 88 | `LeashedJumperBehavior` | `minJumpWidth` | Definition/Query | `LeashedJumperDefinition.MinJumpWidth` | partial |
| 89 | `LeashedJumperBehavior` | `maxJumpHeight` | Definition/Query | `LeashedJumperDefinition.MaxJumpHeight` | partial |
| 90 | `LeashedJumperBehavior` | `maxJumpDuration` | Definition/Query | `LeashedJumperDefinition.MaxJumpDuration` | partial |
| 91 | `LeashedJumperBehavior` | `jumpCooldown` | Definition/Query | `LeashedJumperDefinition.JumpCooldown` | partial |
| 92 | `LeashedJumperBehavior` | `canStandOnWater` | Definition/Query | `LeashedJumperDefinition.CanStandOnWater` + world query | partial |
| 70 | `LeashedFlyerBehavior` | `Prototype` | Definition/Adapter | `LeashedFlyerDefinitionCatalog` | partial |
| 71 | `LeashedFlyerBehavior` | `minWaitTime` | Definition/Query | `LeashedFlyerDefinition.MinWaitTime` | partial |
| 72 | `LeashedFlyerBehavior` | `maxWaitTime` | Definition/Query | `LeashedFlyerDefinition.MaxWaitTime` | partial |
| 73 | `LeashedFlyerBehavior` | `maxFlySpeed` | Definition/Query | `LeashedFlyerDefinition.MaxFlySpeed` | partial |
| 74 | `LeashedFlyerBehavior` | `acceleration` | Definition/Query | `LeashedFlyerDefinition.Acceleration` | partial |
| 75 | `LeashedFlyerBehavior` | `brakeDuration` | Definition/Query | `LeashedFlyerDefinition.BrakeDuration` | partial |
| 76 | `LeashedFlyerBehavior` | `rotationScalar` | Definition/Projection | `LeashedFlyerPresentationDefinition` | partial |
| 77 | `LeashedFlyerBehavior` | `hoverAmplitude` | Definition/Projection | `LeashedFlyerPresentationDefinition` | partial |
| 78 | `LeashedFlyerBehavior` | `hoverPeriod` | Definition/Projection | `LeashedFlyerPresentationDefinition` | partial |
| 79 | `LeashedFlyerBehavior` | `hasGroundBias` | Definition/Query | `LeashedFlyerDefinition.HasGroundBias` | partial |
| 80 | `LeashedFlyerBehavior` | `HoverYVelocity` | Definition/System | `LeashedFlyerDefinition` + `LeashedFlyerSystem` | partial |
| 108 | `LeashedKiteBehavior` | `Prototype` | Definition/Adapter | `LeashedKiteDefinitionCatalog` | partial |
| 109 | `LeashedKiteBehavior` | `_dummy` | Adapter | `LeashedProjectileDummyAdapter` | partial |
| 110 | `LeashedKiteBehavior` | `projType` | Component/Adapter | `LeashedKiteBehaviorComponent.ProjectileType` + Projectile content adapter | partial |
| 111 | `LeashedKiteBehavior` | `frame` | Projection | `LeashedKiteVisualProjection.Frame` | partial |
| 112 | `LeashedKiteBehavior` | `frameCounter` | Projection | `LeashedKiteVisualProjection.FrameCounter` | partial |
| 113 | `LeashedKiteBehavior` | `rotation` | Component/System | `LeashedKiteBehaviorComponent` + `LeashedKiteSystem` | partial |
| 114 | `LeashedKiteBehavior` | `spriteDirection` | Projection | `LeashedKiteVisualProjection.SpriteDirection` | partial |
| 115 | `LeashedKiteBehavior` | `kiteDistance` | Component/System | `LeashedKiteBehaviorComponent` + kite owner | partial |
| 116 | `LeashedKiteBehavior` | `windTarget` | Component/Adapter | `LeashedKiteBehaviorComponent` + `IKiteEnvironmentPort` | partial |
| 117 | `LeashedKiteBehavior` | `windCurrent` | Component/System | `LeashedKiteBehaviorComponent` + kite owner | partial |
| 118 | `LeashedKiteBehavior` | `timeCounter` | Component/Adapter | `LeashedKiteBehaviorComponent` + explicit kite time port | partial |
| 119 | `LeashedKiteBehavior` | `cloudAlpha` | Adapter/Projection | kite visual environment adapter | partial |
| 120 | `LeashedKiteBehavior` | `timeWithoutWind` | Component/System | `LeashedKiteBehaviorComponent` + kite owner | partial |
| 121 | `LeashedKiteBehavior` | `projectileLocalAI0` | Adapter | `LeashedProjectileCompatibilityState` | partial |
| 122 | `LeashedKiteBehavior` | `projectileLocalAI1` | Adapter | `LeashedProjectileCompatibilityState` | partial |
| 123 | `LeashedKiteBehavior` | `oldPos` | Projection | `LeashedKiteVisualProjection.TrailPositions` | partial |
| 124 | `LeashedKiteBehavior` | `oldRot` | Projection | `LeashedKiteVisualProjection.TrailRotations` | partial |
| 125 | `LeashedKiteBehavior` | `oldSpriteDirection` | Projection | `LeashedKiteVisualProjection.TrailDirections` | partial |
| 126 | `LeashedKiteBehavior` | `netOffset` | Projection | `LeashedKiteInterpolationProjection` | partial |
| 154 | `LeashedKiteBehavior` | `AnchorWorldPosition` | Query | `LeashedAnchorWorldPositionQuery` | partial |
| 64 | `LeashedButterflyVariants` | `EmpressButterflyLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog` | partial |
| 65 | `LeashedButterflyVariants` | `fadeAmount` | Component/Projection | `LeashedButterflyPresentationComponent` + visual projection | partial |
| 66 | `LeashedButterflyVariants` | `FadeAwayCap` | Definition/Query | `LeashedButterflyDefinition` + opacity query | partial |
| 127 | `LeashedButterflyVariants` | `NormalButterflyLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog` | partial |
| 128 | `LeashedButterflyVariants` | `variant` | Component/Command | `LeashedButterflyVariantComponent` + full-sync network command | partial |
| 153 | `LeashedButterflyVariants` | `Opacity` | Query/Projection | `LeashedButterflyOpacityQuery` + visual projection | partial |
| 60 | `LeashedSpeciesPrototypes` | `BirdLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.Bird` | partial |
| 61 | `LeashedSpeciesPrototypes` | `CrawlerLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.Crawler` | partial |
| 62 | `LeashedSpeciesPrototypes` | `CrawlingFlyLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.CrawlingFly` | partial |
| 63 | `LeashedSpeciesPrototypes` | `DragonflyLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.Dragonfly` | partial |
| 67 | `LeashedSpeciesPrototypes` | `FairyLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.Fairy` | partial |
| 68 | `LeashedSpeciesPrototypes` | `FireflyLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.Firefly` | partial |
| 69 | `LeashedSpeciesPrototypes` | `FishLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.Fish` | partial |
| 81 | `LeashedSpeciesPrototypes` | `HellButterflyLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.HellButterfly` | partial |
| 129 | `LeashedSpeciesPrototypes` | `RunnerLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.Runner` | partial |
| 130 | `LeashedSpeciesPrototypes` | `ShimmerFlyLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.ShimmerFly` | partial |
| 131 | `LeashedSpeciesPrototypes` | `SnailLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.Snail` | partial |
| 139 | `LeashedSpeciesPrototypes` | `WaterfowlLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.Waterfowl` | partial |
| 140 | `LeashedSpeciesPrototypes` | `WaterStriderLeashedCritter.Prototype` | Definition/Adapter | `LeashedSpeciesDefinitionCatalog.WaterStrider` | partial |

## 7. Cross-subsystem findings and blocking decisions

The following are findings, not owner claims for other partitions:

- `TELeashedEntityAnchor`, `TEKiteAnchor`, and `TECritterAnchor` belong to the tile/entity-host
  boundary. P02 consumes anchor facts and publishes an explicit relation command;
  `crossSubsystemOwner: integration-review`.
- `ActiveSections`, `RemoteClient.NetSectionActivated`, and the server/client network mode decide
  streaming ownership. P02 can gate behavior but cannot declare NetworkSession or WorldStorage as
  its owner.
- NPC and Projectile content IDs are definition inputs. P02 does not own NPC or Projectile runtime
  entities, inventories, item payloads, or their network IDs.
- Persistence and replication formats are not closed by the current Version4 source; a durable
  snapshot adapter and packet projection must be reviewed with the respective subsystem owners.

Current blocking decisions:

1. Confirm whether P02's `LeashedEntityRegistrationSystem` is the sole commit root or whether a
   shared entity/lifecycle owner will host registration.
2. Confirm the stable identity tuple: runtime entity ID, legacy `whoAmI` slot, anchor TileEntity
   identity, persistent ID, and network slot must remain distinct.
3. Confirm section activation and anchor persistence owners before implementing clear/reload.
4. Confirm whether the accepted Version4 source is the current sparse checkout or the unabridged
   fallback for behavior-equivalence verification.

## 8. Checkpoint record

- Completed component units: `LeashedEntityStateComponent`, `LeashedEntityLifecycleComponent`,
  `LeashedEntityAnchorRelationComponent`, `LeashedEntitySectionMembershipComponent`,
  `LeashedEntityLegacySlotComponent`, `LeashedCritterBehaviorComponent`,
  `LeashedKiteBehaviorComponent`, `LeashedButterflyVariantComponent`.
- Current: `none`.
- Pending: none.
- `implementationStatus`: `implemented` for the component-only scope; systems, queries, commands,
  adapters, projections, catalogs, and tests remain unimplemented.
- `evidence-gap`: All 98 members have proposed ownership mappings, but current-checkout behavior
  bodies, exact random and movement semantics, content/definition IDs, external section and anchor
  ownership, network application, persistence, presentation effects, and runtime equivalence remain
  partial.
- `blocking-decision`: `integration-review`.
- `verificationStatus`: `not-verified`; the affected project build passed with 0 warnings and 0
  errors, while focused behavior and integration verifiers were not run.
- The eight component source files were modified or added under `src/LeashedEntity`; no test,
  project, authoritative report, other session document, ledger, or lock file was modified.
