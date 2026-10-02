# System Decomposition Report: authoritative P02

designStatus: proposed
verificationStatus: not-run
sourceModified: false
migrationStatus: not-claimed
partitionId: P02
taskId: AUTH-SYS-P02
sessionId: 7415697df88244a19f3f4eec4c907e35
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P02-Leashed-Entity.md
outputReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P02-leashed-entity.md
sourceProject: D:\TRbackup\Version4
targetArea: D:\TRbackup\NLTX\src\NSSLC

## Scope and Evidence

This report covers only the eight P02 leaf groups in the claimed input: `LeashedRegistryAndSections` (15 members), `LeashedCritterCoreState` (15), `LeashedWalkerBehavior` (7), `LeashedJumperBehavior` (11), `LeashedFlyerBehavior` (11), `LeashedKiteBehavior` (20), `LeashedButterflyVariants` (6), and `LeashedSpeciesPrototypes` (13), totaling 93 fields and 5 properties (98 members). No member inventory or owner decision from another partition is included. The input ledger is the member-scope authority; it records these exact group totals and declarations.

Complete claimed member scope (names are preserved from the input ledger; repeated `Prototype` names are qualified by declaring type):

| Leaf group | Member names |
|---|---|
| `LeashedRegistryAndSections` | `Registry.Prototypes`; `SectionEntityList.coordinates`, `active`, `list`, `count`, `emptySlots`; `LeashedEntity.BySection`, `ActiveSectionList`, `ByWhoAmI`, `sectionSlot`, `active`, `whoAmI`; properties `Type`, `AnchorPosition`, `SectionCoordinates` |
| `LeashedCritterCoreState` | `_dummy`, `anchorStyle`, `npcType`, `spriteDirection`, `frame`, `frameCounter`, `rand`, `WaitTime`, `State`, `TargetPosition`, `netOffset`, `scale`, `strayingRangeInBlocks`, `isAquatic`, `RecallDuration` |
| `LeashedWalkerBehavior` | `WalkerLeashedCritter.Prototype`, `State_Standing`, `State_PickDirection`, `State_Walking`, `State_Falling`, `State_Recalling`, `walkingPace` |
| `LeashedJumperBehavior` | `JumperLeashedCritter.Prototype`, `State_Normal`, `State_Recalling`, `minWaitTime`, `maxWaitTime`, `maxJumpWidth`, `minJumpWidth`, `maxJumpHeight`, `maxJumpDuration`, `jumpCooldown`, `canStandOnWater` |
| `LeashedFlyerBehavior` | `FlyerLeashedCritter.Prototype`, `minWaitTime`, `maxWaitTime`, `maxFlySpeed`, `acceleration`, `brakeDuration`, `rotationScalar`, `hoverAmplitude`, `hoverPeriod`, `hasGroundBias`, `HoverYVelocity` |
| `LeashedKiteBehavior` | `LeashedKite.Prototype`, `_dummy`, `projType`, `frame`, `frameCounter`, `rotation`, `spriteDirection`, `kiteDistance`, `windTarget`, `windCurrent`, `timeCounter`, `cloudAlpha`, `timeWithoutWind`, `projectileLocalAI0`, `projectileLocalAI1`, `oldPos`, `oldRot`, `oldSpriteDirection`, `netOffset`; property `AnchorWorldPosition` |
| `LeashedButterflyVariants` | `EmpressButterflyLeashedCritter.Prototype`, `fadeAmount`, `FadeAwayCap`; `NormalButterflyLeashedCritter.Prototype`, `variant`; property `Opacity` |
| `LeashedSpeciesPrototypes` | `BirdLeashedCritter.Prototype`, `CrawlerLeashedCritter.Prototype`, `CrawlingFlyLeashedCritter.Prototype`, `DragonflyLeashedCritter.Prototype`, `FairyLeashedCritter.Prototype`, `FireflyLeashedCritter.Prototype`, `FishLeashedCritter.Prototype`, `HellButterflyLeashedCritter.Prototype`, `RunnerLeashedCritter.Prototype`, `ShimmerFlyLeashedCritter.Prototype`, `SnailLeashedCritter.Prototype`, `WaterfowlLeashedCritter.Prototype`, `WaterStriderLeashedCritter.Prototype` |

Evidence labels in this report mean: `confirmed` is a source declaration or indexed static fact whose endpoints and selected scope were inspected; `partial` is a bounded static relationship, fallback-checkout behavior, or structure with unresolved effects; `unknown` means the current source/API does not establish behavior or closure; `proposed` is a target design only. Empty or stubbed method bodies are `unknown`, not evidence of no effects.

| Evidence source | Scope and observation | Status and limit |
|---|---|---|
| P02 authoritative member input | `docs/migration/ledgers/authoritative-20-partitions/P02-Leashed-Entity.md`; all eight group tables and 98 rows | confirmed inventory; not runtime ownership |
| Version4 declarations | `Terraria.GameContent/LeashedEntity.cs`; `Terraria.GameContent.LeashedEntities/{LeashedCritter,WalkerLeashedCritter,JumperLeashedCritter,FlyerLeashedCritter,LeashedKite,NormalButterflyLeashedCritter,EmpressButterflyLeashedCritter}.cs`; `Terraria.GameContent.Tile_Entities/{TELeashedEntityAnchor,TELeashedEntityAnchorWithItem,TECritterAnchor,TEKiteAnchor}.cs` | declaration shape confirmed against current checkout; many virtual and lifecycle bodies are empty/stubbed, so effects remain unknown |
| Version4 inbound source | `Terraria/Main.cs:3347,11572`; `Terraria/WorldGen.cs:6665,52726-52728`; `Terraria/MessageBuffer.cs:3311-3317`; `Terraria/RemoteClient.cs:65,181-185`; `Terraria/WorldItem.cs:168` | selected call/registration facts confirmed; not a complete inbound or scheduler graph |
| CPG read-only Query API | `D:\\TRbackup\\Version4-cpg-export\\out-dop8-interproc.sqlite`, schema 1, complete import, 967 shards, 8,166,789 nodes, 71,038,907 edges, 1,317 diagnostics; manifest SHA-256 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`; project fingerprint `521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`; `SourceSnapshotId=null` | indexed source snapshot is not bound to the current source revision. `Find-CpgSymbols`, `Get-CpgTypeSurface`, `Find-CpgCallSites`, `Get-CpgMemberUses`, and `Get-CpgCallableFacts` were used. Query `complete` means indexed query completed in selected scope only. |
| CPG selected facts | `LeashedEntity` type surface: complete, 28 direct members. `RegisterAll` -> `Main.cs`, `UpdateEntities` -> `Main.cs`, `Clear(bool)` -> `WorldGen.cs`: complete selected call-site results. Callable facts for these methods: partial, `CalleeEffectsNotExpanded`. Selected field/property uses have `AccessMode: Unknown` in several cases; `sectionSlot` has one resolved write fact. | static facts confirmed/partial as indicated; do not establish runtime closure, sole writer, dynamic dispatch, event subscriber closure, or behavior parity |
| Full-reference fallback | `D:\\TRbackup\\无任何删减通过编译\\Terraria.GameContent\\LeashedEntity.cs`, `Terraria.GameContent.LeashedEntities\\*.cs`, `Terraria.GameContent.Tile_Entities\\*.cs` | partial behavior recovery only; this is not the authoritative Version4 checkout and cannot upgrade current-source gaps |
| Current NLTX target | inspected P02 component files under `src/NSSLC/Component/LeashedEntity/`; no confirmed P02 System implementation or closed update/registration/network loop was found in the scoped target review | existing component schema evidence only; no migration claim |
| SS14 reference project | `C:\\Users\\shan\\Downloads\\ECS\\space-station-14-master` snippets for component, System, query/event and side-effect boundaries | organization analogy only; no Version4 semantics or names are imported |

The CPG caller set is deliberately bounded to requested source paths. A zero result, a `complete` selected query, or an empty method body does not prove that no other caller, subscriber, reflection entry, serializer, or runtime schedule exists. The source tree and the index may represent different revisions because the manifest has no source snapshot ID.

## Prior Component Decomposition Reconciliation

The prior P02 component design/execution material correctly treats the inventory as a candidate boundary proposal, separates component schemas from runtime Systems, and leaves catalog IDs, entity hydration, section activation, anchor lifecycle, network application, random-state semantics, Projectile compatibility, and cross-domain ownership unresolved. Its component-only historical build evidence is not a current-session verification result and does not establish System ownership.

Current files under `src/NSSLC/Component/LeashedEntity/` include state, lifecycle, anchor relation, section membership, legacy slot, critter behavior, kite behavior, butterfly variant, and motion schemas. These are storage shapes, not evidence of an owner System. `LeashedEntityMotionComponent.cs` is marked proposed. The prior use of a shared `LeashedEntitySimulation` component owner remains a grouping label, not a demonstrated unique runtime writer. Avoid consolidating those schemas into one large System or treating the stored values as behavior parity.

P02 can propose a registration/hydration coordinator, a section index/activation owner, shared critter authority with behavior-specific evaluators, a distinct kite owner, and output adapters. The System boundaries below remain proposed until source behavior and target integration are closed.

## Conceptual Behaviors

| Stable behavior ID | Observable behavior and state boundary | Decision |
|---|---|---|
| `Leashed.DefinitionCatalog` | Resolve an immutable definition/prototype identity and capability/configuration for registration, hydration, critter, kite, and presentation paths. Current `Registry.RegisterAll` visibly appends prototypes and assigns sequential `Type`; exact stable-ID compatibility and all consumers remain partial. | Keep one catalog/query boundary; do not create one runtime component or System per species. |
| `Leashed.EntityLifecycle` | Bind definition and anchor relation, assign/release the compatibility slot, activate/spawn, deactivate/despawn, remove, and rehydrate after world reset. Current declaration contains `active`, `whoAmI`, `AnchorPosition`; some lifecycle functions are empty. | Proposed `LeashedEntityRegistrationSystem` coordinates the single local commit path; anchor and world ownership stay under integration review. |
| `Leashed.SectionMembership` | Map committed entities into section buckets; compact slots; reconcile external section visibility; spawn on activation and despawn on deactivation. Current code shows bucket mutation and section `Activate`/`Deactivate` methods, but the external activation subscription is not visible in current `LeashedEntity.cs`. | Separate rebuildable index and section activation responsibility from durable entity lifecycle. |
| `Leashed.CritterAuthority` | Maintain generic critter `WaitTime`, `State`, target, LCG/random state, NPC content binding, geometry/motion, network offset and shared frame facts. Several update/network virtual bodies are empty in current checkout. | One shared authority commit path; behavior evaluators cannot independently write duplicated generic state. |
| `Leashed.WalkerTransitions` | Evaluate standing, direction selection, walking, falling, recalling and pace-dependent movement against world/tile inputs. | Separate capability evaluator/System only if current behavior and distinct world-query contract are restored and confirmed. |
| `Leashed.JumperTransitions` | Evaluate wait selection, jump target/width/height/duration, cooldown, water support and recall transitions. | Separate capability evaluator/System on shared critter authority; random and tile effects are partial. |
| `Leashed.FlyerTransitions` | Evaluate acceleration/braking, speed, hover, ground bias and movement orientation. | Separate capability evaluator/System on shared critter authority; preserve one generic state commit and explicit environment inputs. |
| `Leashed.KiteTransitions` | Update wind response, timers, motion, frames and trail/interpolation while bridging a Projectile-shaped compatibility object. | Separate `LeashedKiteSystem` because state/effect and compatibility surface differs from critters; Projectile runtime authority remains external. |
| `Leashed.ButterflyPresentation` | Preserve normal variant identity and Empress fade/opacity/render decisions. `Opacity` is derived from fade state in the declaration; actual update/draw effects are not fully available. | Keep variant as narrow per-entity data; propose a presentation projection for fade/render state, not an authority System unless source shows authoritative writes. |
| `Leashed.NetworkProjection` | Translate committed facts to full/partial/remove frames; hydrate received records after validation. Current NetModule.Sync writes framing and calls virtual `NetSend`; `Deserialize` is stubbed. | Adapter/projection only; must not allocate identity or bypass owner commit. |
| `Leashed.AnchorBoundary` | Insert item, create/hydrate/despawn linked entity, drop item on tile break, and restore after world load. Current declarations expose the boundary but key methods are empty. | Adapter/commands at TileEntity boundary; `crossSubsystemOwner: integration-review`. |

## State Ownership and Write Closure

Ownership below is a proposal constrained by observed invariants, not a source-confirmed unique writer. Each authoritative field set must have exactly one commit path; where current source has multiple possible writers or an unknown method body, the owner remains unresolved.

| State/effect set | Kind and observed writers/readers | Proposed owner / closure |
|---|---|---|
| `Registry.Prototypes`, definition `Type`, prototype defaults and species mapping | Static mutable catalog. Current `RegisterAll` calls generic/specific `Register`; `Register` assigns `Type` then appends. Main initialization invokes `RegisterAll`. Other lookups and stable-ID consumers are not closed. | Proposed immutable definition catalog loaded by one bootstrap writer; registration order/IDs retained behind adapter until compatibility evidence. Reader closure partial; external species map `crossSubsystemOwner: integration-review`. |
| `BySection`, `ActiveSectionList`, section `list/count/emptySlots`, entity `sectionSlot` | Derived mutable indexes. Current source shows append/compaction/activate/deactivate behavior, `Clear`, and `_UpdateEntities` traversal. CPG selected uses are mostly access-unknown; complete global writer closure unavailable. | Proposed `LeashedSectionIndexSystem` as sole index mutator, with index rebuild from committed entity and section facts. Never persistence authority. External section activity `crossSubsystemOwner: integration-review`. |
| Entity lifecycle `active`, `whoAmI`, `AnchorPosition`, `Type`; `ByWhoAmI` | Entity lifecycle and legacy compatibility mapping. Current fields/properties are visible; selected CPG accesses do not close all writers. `Type` is visibly assigned in registration code. | Proposed `LeashedEntityRegistrationSystem` for bind/hydrate/slot transitions and one lifecycle commit port. Stable entity ID, anchor host, legacy slot and network IDs remain distinct; closure partial/unknown. |
| Critter generic state: `WaitTime`, `State`, `TargetPosition`, `rand`, NPC/configuration, geometry, frame/interpolation | Mixed authoritative, definition, random and presentation state. Many current virtual bodies are empty. Fallback source recovers behavior only partially. | Proposed `LeashedCritterSystem` owns generic commit; definition queries read immutable content; Walker/Jumper/Flyer evaluators produce transition facts only. Exact random algorithm and generic writer closure unknown. |
| Walker/Jumper/Flyer tuning constants and capabilities | Definition configuration, not per-tick authority. Prototype fields include state discriminants, movement speeds/limits, cooldowns and capability flags. Current behavior method bodies may be empty. | Immutable definition catalog/query; no per-species runtime component fan-out. Constructor/default and override closure partial. |
| Kite wind, timer, movement, frame, trail and Projectile-local AI | Kite authority mixed with presentation and Projectile compatibility state. Current declaration exposes fields; current update body may not establish writes. | Proposed `LeashedKiteSystem` commits only kite-owned facts. Projectile runtime/local-AI adaptation and output are behind explicit adapter; `crossSubsystemOwner: integration-review`. |
| Butterfly `variant`, Empress `fadeAmount`/`Opacity` | Variant is a per-entity content/presentation selector; fade/opacity is presentation candidate. | Registration/network boundary hydrates variant through validation; butterfly projection owns derived opacity/render output. Current write lifecycle partial. |
| Network, anchor tile, item storage/drop, persistence, NPC/Projectile runtime, world section activity | External I/O and other domain authorities; current packet and event paths are incomplete. | Adapters/projections or commands. Every final owner is `crossSubsystemOwner: integration-review`. |

Queries may expose catalog definitions, section coordinate derivation, and read-only committed snapshots. Their call contract is `unknown` for purity/repeatability wherever lazy cache writes, shared temporary state, virtual callbacks, random/time reads, or alias exposure are not ruled out. Commands remain local methods/port calls unless concrete queue/retry/replay requirements justify a command type; this report does not introduce CQRS or an event bus.

## Boundary Role and Decision

| Candidate boundary | Decision | Rationale and rejected alternative |
|---|---|---|
| Registration, entity hydration, lifecycle and legacy slot | `partial` / proposed `LeashedEntityRegistrationSystem` | One coordinated commit path is needed to avoid duplicate entity/slot state. It is not yet safe to claim it owns the anchor host, stable identity, or global registry. Reject one monolithic `LeashedEntitySystem` because it would mix catalog, lifecycle, behavior and external I/O. |
| Section bucket/index maintenance | `separate` proposed `LeashedSectionIndexSystem`; activation reconciliation may be a coordinated phase in the same System until a real distinct schedule/input proves a second node is useful | Index is rebuildable derived state, while entity lifecycle is durable. Reject persisting bucket arrays or making section activation observation the entity authority. |
| Catalog and species definitions | `separate` immutable Definition/Query boundary; bootstrap write remains explicit | Definition reads are reused by hydration and behavior. Species prototypes are not runtime entity instances. Reject a System per prototype and mutable shared prototype-as-live-state. |
| Shared critter authority and Walker/Jumper/Flyer transitions | `partial`: one generic authority System plus proposed capability evaluators; split evaluators into scheduler Systems only after behavior/effect evidence | Generic timer/state/target/random values need one writer. Distinct movement inputs suggest capability-specific logic. Reject one System per field and competing behavior writers. |
| Kite | `separate` proposed `LeashedKiteSystem` | Kite combines wind/environment, Projectile-shaped compatibility state, motion, trail and interpolation. The distinct effect boundary warrants its own owner if verified. Reject storing Projectile state inside kite authority. |
| Butterfly variant and fade | `partial`: narrow variant data plus a presentation Projection | Variant choice may be content/network input; fade/opacity can be visual. Current writer semantics are incomplete. Reject a second presentation writer of gameplay state. |
| Network/anchor/persistence/render | `separate` Adapter/Projection/port boundaries, not authoritative Systems | These translate external representation or emit outputs. Reject adapter-side entity allocation, domain rules, or writeback into owner state. |

## System API and Legacy Behavior Mapping

The API sketches are `proposed`; names and signatures are conceptual, not existing code. Query API shapes are intentionally emphasized for reads. Each API is synchronous unless a later source-backed contract proves queued execution, and no caller may interpret `void` as proof of commit.

| Legacy entry / evidence | Proposed API composition | Result, effect and unresolved compatibility |
|---|---|---|
| `Registry.RegisterAll()`; exact selected call from `Main` initialization | `DefinitionCatalogBootstrap.RegisterDefinitions(definitions)` followed by `LeashedEntityDefinitionQuery.TryResolve(contentKey)` | Preserve deterministic definition binding and reserved/sentinel semantics only after verifier defines accepted IDs. Current call is confirmed; all consuming lookups and override behavior partial. |
| `Register<T>()`, `Register(prototype)` | Keep as a bootstrap adapter around catalog registration; do not expose runtime mutable prototype instances | Current code assigns sequential `Type` then appends. Error/duplicate behavior and reset/reentry semantics unknown. |
| `AddNewEntity` / `TryGet` | Proposed `LeashedEntityRegistrationSystem.TryHydrate(anchorSnapshot, definitionId)` and `LeashedEntityQuery.TryGetByLegacySlot(slot)` | These method names were not found in indexed current Version4 source; fallback supports analogous behavior partially. Do not claim a current legacy edge. |
| `GetSection`, `SectionCoordinates` | `LeashedSectionQuery.GetCoordinate(anchor)` plus `LeashedSectionIndexSystem.EnsureMembership(entity)` | Derived coordinate query must be checked for bounds, world size and current section arithmetic. Creation of a missing bucket is a write and therefore stays in the System/index API, not in a pure Query. |
| `UpdateEntities()` | `LeashedSectionActivationSystem.Reconcile(inputSnapshot)` -> `LeashedWalker/Jumper/FlyerSystem.Evaluate(...)` or `LeashedKiteSystem.Evaluate(...)` -> one owner `CommitTransition(...)` -> `LeashedNetworkProjection.EmitCommittedChanges(...)` | Main update call and local `RecheckActiveSections` then `_UpdateEntities` order are confirmed in current source. Full phase order, mutation visibility and caller tick semantics remain partial. |
| `Spawn(bool)`, `Despawn()`, `Update()`, virtual overrides | Proposed capability evaluator + explicit `LeashedEntityLifecycleSystem` transition/commit; external effects return as effect intents to adapters | Current base and derived bodies with no implementation are unknown. Dynamic dispatch closure incomplete; preserve observable spawn/despawn and per-type behavior only after source evidence is restored. |
| `Remove(entity)`, inactive update cleanup | `LeashedEntityRegistrationSystem.RemoveAndReleaseSlot(entity)` with index cleanup and one committed remove result | Current `Remove` body is empty. Fallback has slot and section cleanup, partial only. Repeated removal/idempotency and network timing unknown. |
| `NetModule.Sync(entity, full, toClient)` and virtual `NetSend` | `LeashedEntityNetworkAdapter.BuildFull/PartialSnapshot(committedState)` -> transport send | Current framing writes full/partial discriminator and slot/type, full mode adds anchor coordinates, then virtual `NetSend`; send/broadcast uses target/active section. Payload fields, delivery guarantees and callee effects partial. |
| NetModule `Deserialize`, `NetReceive`, remove framing | `LeashedEntityNetworkAdapter.ParseAndValidate(packet)` -> `ApplyNetworkEntityCommand` -> registration/behavior owner commit; remove output uses Projection | `Deserialize` is stubbed; current call/use closure is not proved. Validation, malformed packet behavior, authority split and remove semantics unknown. Do not let Adapter allocate or become authority. |
| `TELeashedEntityAnchorWithItem.InsertItem`, `RespawnLeashedEntity`, `OnWorldLoaded`, `OnRemoved` | `LeashedAnchorAdapter.ReadAnchor()` -> `LeashedEntityRegistrationSystem.TryHydrate(...)`; item insertion/drop as an explicit host command | Current `InsertItem` assigns `itemType` and calls `RespawnLeashedEntity`; latter, load and removal methods are empty. Full reference fallback has behavior but remains partial. Anchor lifecycle/persistence owner integration-review. |
| `DropItemForTileBreak`; `WorldGen` tile-break path | `LeashedAnchorCommand.DropStoredItemOnBreak()` -> item-world adapter | Current body emits `Item.NewItem` and clears `itemType`; caller guarded by tile type and `!fail`. Error ordering and host removal interaction need verification. |
| `MessageBuffer` item insertion packet | Protocol Adapter validates packet and anchor, then emits `InsertAnchorItem` command | Selected current caller/endpoint confirmed; authorization, rejection, duplicate handling and response are unknown. |
| `Draw`, butterfly opacity, kite trail/frame | `LeashedPresentationProjection.BuildRenderSnapshot(committedState)` -> renderer adapter | Current draw methods are virtual and empty; no rendering behavior equivalence asserted. Projection must not modify gameplay authority. |

Read APIs must return immutable value snapshots or narrow scalars where aliasing is possible. `TryResolve`, `TryGetByLegacySlot`, coordinate conversion, and committed-state reads are proposed Query responsibilities only when repeated-call stability and no observable writes are demonstrated. `EnsureMembership`, registration, section reconciliation, receive/apply, and removal are Commands/effectful System operations despite any boolean return; they are not standalone pure Queries.

## Call and Dependency DAG

### Confirmed selected static edges

```text
Main initialization -> LeashedEntity.Registry.RegisterAll
Main update         -> LeashedEntity.UpdateEntities
WorldGen clear      -> LeashedEntity.Clear
MessageBuffer       -> TileEntity lookup -> TELeashedEntityAnchorWithItem.InsertItem
WorldGen tile break -> TileEntity lookup -> DropItemForTileBreak -> Item.NewItem
Main.UpdateEntities -> RecheckActiveSections -> CompactIfNecesary / Deactivate
Main.UpdateEntities -> _UpdateEntities -> virtual Update / StreamNetUpdates / Remove (if inactive)
RemoteClient        -> NetSectionActivated event publication
WorldItem           -> NetSectionActivated subscription for its own section sync
```

The first three `CallTargets` edges were confirmed with `Find-CpgCallSites` in the selected CPG paths and source-checked at the listed endpoints. CPG callable queries for registration/update/clear returned `partial` with `CalleeEffectsNotExpanded`. The local edges in `LeashedEntity.cs` are source-level control-flow observations; only the statements and branches read in current source are confirmed.

### Proposed target dependency order

```text
definition bootstrap/catalog
  -> anchor snapshot and entity hydration/slot bind
  -> rebuild section index and reconcile activation input
  -> evaluate one capability behavior from immutable input snapshots
  -> one generic critter or kite authority commit
  -> removal and compatibility-slot/index cleanup
  -> network, persistence, and presentation projections
```

This is a proposed dependency order, not a confirmed scheduler DAG. Current source confirms that `UpdateEntities` invokes section recheck before `_UpdateEntities`, and within the visible loop it calls `Update`, then `StreamNetUpdates`, then tests inactive and calls `Remove`. It does not confirm when section activation events are registered, whether activation invokes synchronization, or exact ordering relative to world/tile updates, NPC/Projectile updates, persistence and network flush. `RemoteClient.NetSectionActivated` is declared and raised; current `LeashedEntity.cs` has no visible matching subscription, so leashed-section activation synchronization is a version gap, not proof of no subscriber elsewhere.

### Unresolved edges

- Dynamic dispatch from base `Spawn`, `Despawn`, `Update`, `Draw`, `NetSend`, and `NetReceive` to all derived implementations: `partial`; some current implementations are empty/stubbed and all runtime targets are not closed.
- Registry lookup to concrete prototype, TileEntity item/content mapping, and definition catalog consumers: `partial` via fallback; current source mapping incomplete.
- Event subscription, reflection, serialization, content loading and network handler registration: `unknown` outside inspected call sites.
- Cross-domain reads/writes into world tiles, NPC, Projectile, item, anchor TileEntity and section activity: `crossSubsystemOwner: integration-review`.
- CPG member-use AccessMode for multiple fields is `Unknown`; `sectionSlot` resolved write coverage does not prove sole writer. No edge is inferred from zero-hit queries.

## Lifecycle and Side Effects

| Lifecycle stage | Current evidence | Proposed seam and open question |
|---|---|---|
| Definition bootstrap | `Main` calls `Registry.RegisterAll`; source visibly appends prototypes and assigns type order. | Bootstrap writer and read-only catalog Query. Reinitialization, duplicate registration, mod/override behavior, and catalog reset are unknown. |
| Entity create/hydrate | Anchor subclasses expose `CreateLeashedEntity`; item anchor insertion calls `RespawnLeashedEntity`, whose current body is empty. | Anchor adapter supplies immutable snapshot; registration System commits entity, relation, lifecycle and slot together. Exact creation path unknown. |
| Activate/spawn | Section list `Activate` sets its active flag, calls `Spawn(false)` on entries, and appends to `ActiveSectionList`. | Activation System handles an external snapshot and one lifecycle commit; spawn side effects/dynamic overrides unknown. |
| Update | Main invokes `UpdateEntities`; visible code reconciles active sections, updates active entities, streams updates, then removes inactive ones. | Scheduler phase/barrier and deterministic input snapshot required. Time, random, world reads, and mutation visibility unknown/partial. |
| Deactivate/end | `Deactivate` clears section activity and invokes `Despawn()` on entries. | Do not destroy durable entity state when section visibility ends. Idempotency and whether partial network/removal occurs are unknown. |
| Remove | `_UpdateEntities` reaches `Remove` for inactive entity; current `Remove` body is empty. | One removal commit releases slot and index entries; fallback behavior partial. Duplicate callbacks and final network frame ordering unknown. |
| Clear/reset | `WorldGen` calls `Clear()` after resetting active sections; `Clear` clears indexes and optionally reconstructs active sections from `ActiveSections`. | Reset ordering and keep-active semantics to preserve; world isolation, dangling relation handling and callbacks are unknown. |
| Anchor break/item flow | Current item drop body creates an item then clears `itemType`; WorldGen calls it only for relevant anchor tile types when not failing. `MessageBuffer` reaches `InsertItem`. | Item/TileEntity adapter owns host storage, transactional ordering and failure behavior. `crossSubsystemOwner: integration-review`. |
| World-load/persistence | Anchor declares `OnWorldLoaded`, `WriteExtraData`, `ReadExtraData`, but bodies are empty. | Persistence/restore format and relation reconnection unknown. Do not persist derived indexes or infer a stable ID. |
| Network | `NetModule.Sync` serializes framing and sends/broadcasts to client(s) selected by section activity; `Deserialize` stubbed; virtual payload methods exist. | Adapter owns validation and wire representation; no direct writes until a validated command reaches the authoritative owner. Retry, malformed input, disconnect and version behavior unknown. |
| Presentation | virtual `Draw`, frame, opacity, offsets and trail members exist. | Projection/render adapter consumes snapshots. Whether any such fields are authoritative or replicated is partial/unknown. |
| Exceptions/unload/multi-world/session boundary | No complete evidence in selected source/API. | `unknown`; determine reset/unload, concurrent worlds, server/client ownership, cancellation and cleanup before implementation. |

Side-effect ports proposed for future integration are `ILeashedAnchorPort`, `ISectionActivityQuery`, `ILeashedWorldQuery`, `ILeashedRandomSource`, `ILeashedNpcCompatibilityPort`, `ILeashedProjectileCompatibilityPort`, `ILeashedNetworkTransport`, `ILeashedPersistencePort`, and `ILeashedPresentationSink`. These are contracts to evaluate, not existing target APIs.

## Integration Handoff

This P02 report does not assign final ownership beyond the P02 conceptual behaviors. Integrators must resolve the following before implementation or dual-read rollout:

| Handoff item | Required decision/evidence | Current disposition |
|---|---|---|
| Definition and identity model | Stable definition key vs sequential `Type`; reserved sentinel; content key conversion; mod ordering; compatibility with entity/runtime/network IDs | `crossSubsystemOwner: integration-review`; blocking |
| Anchor TileEntity lifecycle | Placement, persistence, world-load, removal, item insertion/drop, relation hydration, atomicity with entity lifecycle | `crossSubsystemOwner: integration-review`; current implementation bodies incomplete |
| World section ownership | Source of section visibility, activation subscription and callback timing, server/client section semantics, clear/rebuild ordering | `crossSubsystemOwner: integration-review`; activation caller closure partial |
| NPC and Projectile | Content IDs vs runtime IDs, dummy object side effects, ownership of `projectileLocalAI0/1`, NPC item mapping and resource cleanup | `crossSubsystemOwner: integration-review`; current body/effect evidence partial |
| Network and persistence contracts | Packet IDs/field ordering, full/partial/remove rules, validation, snapshot consistency, save format, reconnect and schema version | `crossSubsystemOwner: integration-review`; current NetReceive/Deserialize/persistence incomplete |
| Shared critter authority | Which owner commits generic `State`, `WaitTime`, target and random cursor when behavior capability changes or network input arrives | Proposed one coordinator; blocking until complete write closure |
| Scheduler | Tick phase, barrier, visibility, removal and projection order, event subscription/unsubscription | Proposed DAG only; source/scheduler evidence incomplete |
| NLTX target | Reconcile existing schemas with actual `D:\\TRbackup\\NLTX\\src\\NSSLC` boundaries, ensure there is no second writer in other target module, and select concrete System file locations under applicable organization constraints | No System implementation found in scope; integration review before code changes |

## Migration Behavior Contract

This section is a proposed contract for a later authorized migration. It is not an implementation result.

| Behavior slice | Input -> output | Required invariant/order/error semantics | Focused observation vector |
|---|---|---|---|
| Catalog registration/resolution | ordered content definitions -> immutable IDs/definitions | IDs deterministic for the accepted content set; reserved IDs and duplicate/error behavior preserved; initialization is explicitly once or idempotent according to source evidence | key-to-ID map, duplicate outcome, repeated initialization state, defaults and override selection |
| Hydration and anchor relation | validated anchor/item snapshot + definition -> entity state/lifecycle/slot/index intent | one entity per accepted anchor according to source; invalid definition/item must not partially bind; runtime ID, content ID, legacy slot and persistent ID remain distinct | entity count, relation, slot map, contents, failure atomicity and errors |
| Section membership/activation | committed entity anchor + section activity snapshot -> membership/index/spawn/despawn transitions | index is rebuildable; compaction preserves every live mapping; activation/deactivation is idempotent; deactivation does not destroy durable entity state | bucket contents, slots, spawn/despawn count, active-list order, repeat behavior, reset/rebuild |
| Critter behavior | prior authority state + definition + world/tick/random snapshot -> transition facts | one commit; preserve timer, state, target, LCG consumption, movement, recall and update ordering; random/time queries are not reordered | state delta, position/velocity/direction, target, random cursor, emitted effect sequence, errors |
| Kite behavior | kite authority + wind/world/projectile snapshot -> kite transition and compatibility effects | preserve distance, wind/time, fast-forward if present in accepted source, trail/frame and Projectile interaction order; Projectile adapter cannot become second authority | kite fields, projectile mirror, trail/offset, emitted calls/order and failure cleanup |
| Butterfly presentation | variant/fade/render inputs -> immutable render output | presentation cannot write gameplay authority; accepted variant and full/partial suffix behavior preserved if confirmed | variant, opacity, draw inputs, state before/after projection, malformed variant outcome |
| Network apply/publish | bytes/committed snapshot -> validated command or framed bytes | parse before commit; full/partial/remove semantics and section recipient rule preserved; invalid packets cause no partial writes; adapter cannot allocate an unvalidated entity | bytes, decoded fields, entity/slot state, recipient set, event order, rejection/error result |
| Anchor/item/persistence | placement/load/insert/break/save input -> host storage and lifecycle intent | source-confirmed ordering must be captured before switching; item cannot be duplicated/lost across failed transitions; indexes are not persisted as authority | tile/item/entity state, emitted item, save bytes, reload relation, failure atomicity |

Compatibility is assessed at the conceptual behavior level: observable state delta, result, invariant, side effects, order, error, lifecycle, scope and idempotency. No behavior-equivalence, API compatibility, migration success or implementation is claimed here.

## Evidence Gaps and Blocking Decisions

| Gap / blocker | Status | Decision needed before implementation |
|---|---|---|
| Current Version4 empty/stubbed lifecycle, behavior, serialization, receive, draw and remove bodies | unknown | Obtain accepted complete source or behavior traces for the same Version4 revision; do not substitute fallback as authoritative. |
| CPG snapshot is not source-bound; callables do not expand callee effects; several access modes unknown | partial | Reconcile indexed facts with current source hashes and add focused call/write/event/scheduler evidence. |
| Inbound closure for virtual overrides, event subscribers, reflection, network handlers, serializers and dynamic factories | unknown/partial | Enumerate registration and subscription sites and all target implementations; no negative conclusion from zero hits. |
| Unique writer for lifecycle, slot allocation/reuse, section indexes, random state, motion, kite state and network apply | partial/unknown | Build complete read/write set and choose one commit owner for each invariant. |
| Stable definition IDs, prototype ordering, species/item/NPC/Projectile mapping and defaults | partial | Agree immutable catalog identity/versioning and retain compatibility mapping. |
| Section activation and sync subscription for leashed entities | unknown | Find current-version subscription and establish event order, client scope, reconnect and active-section rebuild behavior. |
| Anchor create/load/remove/persist and relation transaction | unknown in current source; partial from fallback | Coordinate with TileEntity/world-storage owner and specify atomic failure/retry semantics. |
| Network full/partial/remove validation, wire fields, Deserialize and malformed input | partial/unknown | Close protocol reader/writer pairs and authority rules; run focused compatibility verification in a later authorized implementation task. |
| Walker/Jumper/Flyer random, world queries, recall and effect ordering; Kite Projectile dummy and local AI | partial from fallback, unknown in current bodies | Recover authoritative behavior and separate deterministic state transitions from adapter effects. |
| Cross-module target duplication or existing NLTX runtime schedule | unknown | Integrate with target owners in and around `src/NSSLC`; no P02 report can finalize cross-partition ownership. |
| Exception, unload, multi-world and client/server lifecycle | unknown | Define cancellation, cleanup, isolation, teardown and ownership context. |

Blocking decisions for this proposal are: (1) identity/catalog contract; (2) anchor and section integration owners; (3) unique shared critter and kite write paths; (4) network/persistence transaction contract; and (5) scheduler phase and event registration. Until these close, the report is a proposed decomposition only.

## Verification Plan

`verificationStatus: not-run`. This session did not modify production code, tests, project files, input reports, prompt files, or other reports, and did not run a build, test, runtime behavior verifier, or migration implementation. The read-only CPG Query API and static source inspection are evidence collection, not behavior verification. Prior historical component-project build claims are `existing-evidence` only and were not rerun or upgraded.

Future verification, after the blockers above are resolved and an implementation is separately authorized:

1. Definition/catalog checks: deterministic IDs, reserved entry, duplicate and reinitialization behavior, species/content mapping and immutable read semantics.
2. Lifecycle/index checks: hydrate, activate, deactivate, compact, remove, slot reuse, repeated commands, `Clear(keepActiveSections)` and rebuild; assert exactly one writer and no stale relation/index entries.
3. Behavior checks: source-derived Walker/Jumper/Flyer/Kite scenarios with controlled tile/world inputs and deterministic random state; compare authority fields, movement, target, timer, effects and order after each tick.
4. Network and persistence checks: full/partial/remove serialization, deserialize validation, malformed/truncated data, section recipient selection, anchor save/load, disconnect/reconnect and transactional failure observations.
5. Presentation checks: butterfly variant/fade, kite trail/frame/interpolation and NPC/Projectile projections cannot mutate authoritative gameplay state.
6. Integration checks: scheduler phase/barriers, section event subscribe/unsubscribe, world reset/unload, multiple worlds and client/server authority; exercise the new owner path rather than only a retained facade.

No verifier was executed in this session. Proposed API composition and call order remain unverified; migration success must be decided only by a later real migration project with the agreed behavior tests passing.
