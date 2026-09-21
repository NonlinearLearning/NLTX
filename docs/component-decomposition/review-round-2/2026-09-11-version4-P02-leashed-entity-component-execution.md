# Version4 P02 Leashed Entity 注册、行为与物种定义执行计划

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
evidence-gap: C08 execution is planned around one immutable 13-row catalog, explicit content binding, and a single registration root; current Version4 behavior bodies, stable IDs, item/NPC mapping, network application, persistence, section/anchor ownership, and runtime equivalence remain independently unverified.
blocking-decision: integration-review; definition catalog and registration writer remain deferred

## 8. Implementation checkpoint: LeashedEntityStateComponent

- status: implemented
- source file: `src/LeashedEntity/LeashedEntityStateComponent.cs`
- actual change: retained the semantic `DefinitionId` field and added the deterministic `IsBound`
  derived view; no runtime registration key was invented.
- dependency impact: registration/hydration remains the future single writer; catalog validation is
  not part of this component.
- verificationStatus: not-verified; the component project build passed, but definition binding and
  runtime equivalence remain unverified.
- evidence-gap: accepted definition ID range, catalog lookup and registration lifecycle remain
  partial; no behavior or network equivalence is claimed.

## 9. Implementation checkpoint: LeashedEntityLifecycleComponent

- status: implemented
- source file: `src/LeashedEntity/LeashedEntityLifecycleComponent.cs`
- actual change: marked the existing lifecycle state component as implemented, preserving the
  explicit default state and derived active/removed views; no transition behavior was added.
- dependency impact: registration and section activation remain external writers and are not part of
  this component-only change.
- verificationStatus: not-verified; the component project build passed, while lifecycle equivalence
  remains unverified.
- evidence-gap: the source report confirms only the legacy `active` member; `Spawned` and
  `TransitionSequence` remain compatibility candidates until focused lifecycle evidence exists.

## 10. Implementation checkpoint: LeashedEntityAnchorRelationComponent

- status: implemented
- source file: `src/LeashedEntity/LeashedEntityAnchorRelationComponent.cs`
- actual change: promoted the existing entity-side relation schema to implemented component code;
  the component remains a passive value container with a derived runtime-reference view.
- dependency impact: tile-anchor storage, persistence and relation commit ownership remain external;
  no adapter, command, system or persistence code was added.
- verificationStatus: not-verified; no anchor hydration or persistence verifier was run after the
  component project build.
- evidence-gap: only `AnchorPosition` is authoritative in the P02 source report; persistent identity
  and relation revision are retained as partial compatibility fields pending integration review.

## 11. Implementation checkpoint: LeashedEntitySectionMembershipComponent

- status: implemented
- source file: `src/LeashedEntity/LeashedEntitySectionMembershipComponent.cs`
- actual change: promoted the existing passive membership state to implemented component code and
  retained `IsIndexed` as a derived slot view.
- dependency impact: section indexes and activation systems remain absent by scope; no global list,
  compaction, external section call, or lifecycle transition was added.
- verificationStatus: not-verified; the component project build passed, but no section verifier has
  run.
- evidence-gap: source-level `sectionSlot` shape is confirmed, while section-coordinate conversion,
  activation ownership, rebuild and clear behavior remain partial.

## 12. Implementation checkpoint: LeashedEntityLegacySlotComponent

- status: implemented
- source file: `src/LeashedEntity/LeashedEntityLegacySlotComponent.cs`
- actual change: promoted the reusable legacy-slot value component, preserving the `-1` default and
  derived `IsAssigned`; no durable identity or slot allocator was introduced.
- dependency impact: `ByWhoAmI`, slot allocation/reuse and replication remain future adapter/system
  work and are intentionally not implemented here.
- verificationStatus: not-verified; no slot lifecycle or network verifier has run after the component
  project build.
- evidence-gap: `SlotGeneration` is an NLTX compatibility candidate without a matching Version4
  member, so generation-based stale-reference rejection is not claimed.

## 13. Implementation checkpoint: LeashedCritterBehaviorComponent

- status: implemented
- source file: `src/LeashedEntity/LeashedCritterBehaviorComponent.cs`
- actual change: promoted the passive critter authority schema, clarified content/configuration
  semantics, and kept the derived target-presence view free of side effects.
- dependency impact: no behavior system, deterministic random port, protocol adapter, interpolation
  projection, dummy adapter, or presentation writer was added.
- verificationStatus: not-verified; exact random-state semantics and authority transition equivalence
  remain pending focused verification after the component project build.
- evidence-gap: anchor style and aquatic capability remain partial definition snapshots until C08
  catalog ownership and content mapping are implemented.

## 14. Implementation checkpoint: LeashedKiteBehaviorComponent

- status: implemented
- source file: `src/LeashedEntity/LeashedKiteBehaviorComponent.cs`
- actual change: kept only kite authority fields and removed the two Projectile-local AI fields that
  the design assigns to an adapter; constructor parameters now match the component boundary.
- dependency impact: no Projectile, world-environment, network, trail, interpolation, or anchor query
  code was added.
- verificationStatus: not-verified; source compatibility and kite behavior equivalence remain
  unverified after the component project build.
- evidence-gap: the removed fields still require a future scoped compatibility adapter; this component
  must not be used as a replacement for that adapter.

## 15. Implementation checkpoint: LeashedButterflyVariantComponent

- status: implemented
- source file: `src/LeashedEntity/LeashedButterflyVariantComponent.cs`
- actual change: added the missing passive component for the normal butterfly's `variant` byte.
- dependency impact: no item adapter, network command, fade/opacity query, visual projection, or
  prototype catalog was added.
- verificationStatus: not-verified; accepted variant range and full/partial network semantics remain
  pending focused verification after the component project build.
- evidence-gap: the source field is confirmed, but its content/style mapping and protocol ownership
  remain partial.

## 1. Plan boundary

The following section is the original implementation plan for P02. Its non-component paths remain
proposed, while the component-only implementation records at the top of this document are the
current execution state. The required current state is:

```text
executionStatus: in-progress
implementationStatus: implemented
verificationStatus: not-verified
```

The plan covers only the 98 P02 members. The current implementation changed only component source
files in `src/LeashedEntity` and these two P02 documents. It did not change the authoritative report,
another partition's documents, the ledger, or production code outside the component scope.

## 2. File organization and dependency impact

The leashed domain is small enough to stay flat under `src/LeashedEntity`. Do not create
`Shared/Components`, `Common`, `Misc`, or empty `Systems`/`Queries` directories. Add a subdirectory
only after the implementation has a stable independent boundary and records that decision.

| Boundary | Proposed target | Source/member impact | Rollback |
|---|---|---|---|
| Definition binding | `src/LeashedEntity/LeashedEntityStateComponent.cs` and `LeashedDefinitionCatalogAdapter.cs` | `Type`/`Prototypes` become a definition query; no runtime state in catalog | retain legacy registry adapter until ID and default verifier passes |
| Lifecycle | `src/LeashedEntity/LeashedEntityLifecycleComponent.cs`; `LeashedEntityRegistrationSystem.cs` | entity `active`, spawn/despawn/remove transitions | stop new writer and restore legacy lifecycle read adapter |
| Anchor relation | `src/LeashedEntity/LeashedEntityAnchorRelationComponent.cs` | `AnchorPosition` relation only; tile host remains external | keep legacy anchor callback and discard new relation commit |
| Section index | `src/LeashedEntity/LeashedEntitySectionMembershipComponent.cs`; `LeashedSectionIndexAdapter.cs`; `LeashedSectionActivationSystem.cs` | section bucket arrays, slots, active observations | rebuild index from legacy entity facts; no persisted bucket data |
| Legacy network slot | `src/LeashedEntity/LeashedEntityLegacySlotComponent.cs`; `LeashedEntityReplicationProjection.cs` | `whoAmI`/`ByWhoAmI` compatibility only | keep old slot adapter; never migrate it as stable identity |
| Pure section query | `src/LeashedEntity/LeashedSectionCoordinatesQuery.cs` | `SectionCoordinates` derived property | use existing conversion at adapter boundary |
| Critter behavior | `src/LeashedEntity/LeashedCritterBehaviorComponent.cs` and C02-C05 systems | `rand`, `WaitTime`, `State`, `TargetPosition`, content IDs | do not enable behavior writer until C02-C05 focused verifiers pass |
| Kite behavior | `src/LeashedEntity/LeashedKiteBehaviorComponent.cs`, `LeashedKiteSystem.cs` | kite state, motion and wind seam | retain protocol projection and legacy update adapter |
| Butterfly/species definitions | `src/LeashedEntity/LeashedButterflyVariantComponent.cs`, `LeashedSpeciesDefinitionCatalog.cs` | variant and 13 prototype rows | keep definition ID lookup and old prototype adapter |

The eight `src/LeashedEntity` component files are implemented schemas and compile as part of the
affected project; they are not proof that the full target architecture is implemented. The remaining
`dome/src/Terraria.Dome.Simulation/Leash` and protocol paths must be reconciled before one owner is
selected; they cannot be dual writers.

## 3. Global execution invariants

- Establish a focused verifier before introducing each new authority writer.
- Migrate one source boundary at a time: add read-only compatibility view, prove the new writer,
  migrate readers, then remove the old writer. Never use double-write as equivalence evidence.
- Keep content ID, runtime ID, legacy slot, network slot, anchor TileEntity ID, and persistent ID
  distinct. Add explicit conversion ports where a protocol uses a compact integer.
- Components hold durable or tick-to-tick domain state. Commands hold one-shot intent. Queries are
  deterministic and do not write. Adapters own I/O, serialization, clocks, random providers,
  logging, and resource release. Projections emit immutable facts and do not call back into owners.
- Add explicit scheduler edges for section reconciliation, registration, behavior, removal and
  projection. File or project order is not an execution contract.
- Any persistence format decision stays blocked while Version4 save/restore evidence is empty or
  belongs to a different source checkout.

## 4. Planned verification commands

The component-only implementation was compiled through the repository wrapper. The following
non-component commands remain plans only and must be serialized through the wrapper if their
implementation is authorized:

```text
pwsh -NoProfile -Command '$dotnetArguments = [string[]]@("build", ".\\src\\LeashedEntity\\Terraria.LeashedEntity.csproj", "-m:1", "-nr:false", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false", "-p:BuildInParallel=false"); & ".\\Build\\Tools\\Invoke-SerialDotnet.ps1" @dotnetArguments; exit $LASTEXITCODE'
project: src/LeashedEntity/Terraria.LeashedEntity.csproj
exitCode: 0
warnings: 0
errors: 0
artifact: Build/bin/Terraria.LeashedEntity/Debug/net10.0/Terraria.LeashedEntity.dll
```

The build validates source compilation only; focused behavior, network, persistence, and
equivalence verifiers were not run.

```powershell
$dotnetArguments = [string[]]@("build", ".\src\LeashedEntity\Terraria.LeashedEntity.csproj", "-m:1", "-nr:false", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false", "-p:BuildInParallel=false")
& ".\Build\Tools\Invoke-SerialDotnet.ps1" @dotnetArguments
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$dotnetArguments = [string[]]@("test", ".\Test\<affected-leashed-verifier>.csproj", "--no-build", "--no-restore", "-m:1", "-nr:false", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false", "-p:BuildInParallel=false")
& ".\Build\Tools\Invoke-SerialDotnet.ps1" @dotnetArguments
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
```

Before any future compile-capable command, inspect active `dotnet.exe`/`csc.exe`, build only the
affected project, record exit code and warning/error counts, and confirm artifacts are under
`Build/bin`. The component-project command, artifact, and result for this session are recorded above;
future focused verifier commands remain unexecuted.

## 5. C01 execution checkpoint: LeashedRegistryAndSections

### 5.1 Implementation sequence

1. Freeze the identity map: `DefinitionId`, runtime entity ID, legacy slot, anchor identity,
   network slot and persistent ID. Add a read-only mapping verifier before moving code.
2. Introduce the proposed `LeashedDefinitionCatalogAdapter` around the existing definition IDs.
   Preserve the reserved slot and deterministic registration order until a source-approved
   replacement is verified.
3. Make `LeashedEntityRegistrationSystem` the single proposed writer for lifecycle, anchor
   relation hydration and legacy-slot allocation. Keep `ByWhoAmI` behind an adapter.
4. Introduce `LeashedSectionIndexAdapter` as a rebuildable index. Test add, remove, compaction,
   activation and `Clear(keepActiveSections)` without serializing its arrays.
5. Introduce `LeashedSectionActivationSystem` with an explicit input port for external section
   activity. Ensure a deactivated section does not destroy the relation or durable state.
6. Add `LeashedSectionCoordinatesQuery` as a pure conversion. Migrate callers only after the
   coordinate and boundary tests pass.
7. Add `LeashedEntityReplicationProjection` over committed facts. The protocol adapter retains
   full/partial/remove framing and rejects type/anchor mismatches; it cannot allocate entities.

### 5.2 C01 dependency and rollback matrix

| Step | Depends on | New writer | Readers moved after | Rollback condition |
|---|---|---|---|---|
| identity map | Version4/fallback evidence | none | verifier only | ID meanings conflict |
| definition catalog | identity map | catalog adapter | definition queries | order or unknown ID mismatch |
| registration | catalog, relation model | registration system | anchor/lifecycle consumers | stale slot or duplicate entity |
| section index | registration | section index adapter | section queries and update gate | compaction or active-section mismatch |
| activation | section index, external port | activation system | behavior scheduler | spawn/despawn is not idempotent |
| replication projection | committed facts | projection only | network adapter | full/partial/remove mismatch |

If any step fails, disable the new reader path, preserve the legacy adapter, remove only the new
writer introduced by that step, and rerun the focused verifier. Do not delete the legacy slot or
section data until the replacement has an independently reproducible rebuild path.

### 5.3 C01 acceptance evidence

- registry IDs and protocol type range are deterministic;
- slot reuse cannot resolve to a previous entity generation;
- section membership and compaction preserve all live entities;
- activation, deactivation, clear and reload are idempotent;
- no section bucket or projection becomes a second authority;
- full/partial/remove packet projection observes committed state only;
- anchor removal and world-load recreation have one auditable commit path.

## 5.4 C02 execution checkpoint: LeashedCritterCoreState

### 5.4.1 Implementation sequence

1. Freeze the C02 field map before adding a writer: NPC content ID, definition configuration,
   behavior authority, target coordinates, random cursor, interpolation cache, presentation cache,
   and dummy adapter must remain separate categories.
2. Add a read-only `LeashedCritterDefinitionQuery` over the selected catalog. Hydrate `npcType`,
   `anchorStyle`, `strayingRangeInBlocks`, `isAquatic`, and `scale` from validated content data;
   do not let a behavior tick mutate catalog entries.
3. Define the authority transition input/output around `WaitTime`, `State`, `TargetPosition`, and
   the random cursor. Preserve the exact LCG state representation until a source-approved verifier
   establishes the replacement; do not equate a `uint` wire field with the full random algorithm.
4. Add the full/partial network adapter as a command producer. Validate anchor-relative target
   offsets and content IDs before commit, and keep protocol framing outside the owner system.
5. Add interpolation and presentation projections for `netOffset`, `frame`, `frameCounter`, and
   `spriteDirection`. Build a render snapshot and copy it into a scoped dummy adapter only for
   drawing; do not expose dummy state to behavior or persistence.
6. Migrate C02 readers only after the focused verifier proves full/partial hydration, deterministic
   random transitions, recall boundaries, and authority/presentation separation. Keep the legacy
   read adapter until that evidence exists.

### 5.4.2 C02 dependency and rollback matrix

| Step | Depends on | New writer | Readers moved after | Rollback condition |
|---|---|---|---|---|
| definition query | C01 definition binding and accepted content IDs | definition query adapter | critter hydration | content ID/size/default mismatch |
| authority transition | definition query, explicit random port | critter owner system | selected behavior systems | nondeterministic or duplicate state writes |
| network command | authority schema and protocol evidence | network adapter/command producer | full/partial consumers | field order, offset, or malformed-input mismatch |
| interpolation | network command | interpolation projection | client render path | interpolation leaks into authority or persistence |
| dummy projection | presentation snapshot and NPC content port | dummy adapter | draw adapter | static dummy state or wrong frame/direction |
| reader migration | all above focused verifiers | none | legacy call sites | any unverified behavior or API regression |

On failure, disable only the new C02 reader path, retain the legacy declaration adapter, and remove
the newly introduced writer or projection. Do not roll back by deleting the existing behavior
component or by resetting network/persistence data. The fallback behavior source remains evidence,
not a runtime replacement.

### 5.4.3 C02 acceptance evidence

- every one of source rows 93-107 has a single proposed owner and a stated category;
- definition configuration cannot be mutated by behavior or presentation systems;
- authority transitions have one writer and explicit random input/output state;
- full and partial network paths produce commands rather than hidden component writes;
- interpolation and dummy state are excluded from authority/persistence snapshots;
- focused verifier plans cover malformed payloads, target-coordinate bounds, recall and rendering;
- no System, Query, Command, Adapter, Projection, test, build, or behavior-equivalence result is
  claimed in this checkpoint; component-only implementation is recorded above.

## 5.5 C03 execution checkpoint: LeashedWalkerBehavior

### 5.5.1 Implementation sequence

1. Register a read-only `LeashedWalkerDefinition` containing the five state discriminants, the
   constructor defaults `walkingPace = 0.8f` and `strayingRangeInBlocks = 3`, and no entity runtime
   fields. Keep `Prototype` behind the definition catalog adapter.
2. Define `IWalkerTileQuery` for in-world, solid, liquid and half-brick observations. Capture a
   stable world-input snapshot before evaluating the walker; the owner system must not call
   `WorldGen`, `Main`, rendering, dust, or logging directly.
3. Implement the transition calculation around the generic C02 authority: timer expiration,
   direction selection, target advancement, walking movement, falling/gravity, and recalling. The
   transition returns state, timer, target, velocity and direction facts; one commit port writes
   them.
4. Represent the fallback `Recall` behavior as an explicit command/effect output. The client dust
   effect and any position-side-effect adapter execute after the authority transition and are not
   part of the pure walker calculation.
5. Build walker presentation output after commit: half-brick offset, dummy opacity and animation
   inputs remain downstream projections. Do not let `CopyToDummy` or frame finding write walker
   authority.
6. Move legacy readers only after transition, range, query ordering, recall command, and single
   writer verifiers pass. Retain the old adapter during the migration and remove it only after a
   reproducible rollback path exists.

### 5.5.2 C03 dependency and rollback matrix

| Step | Depends on | New writer | Readers moved after | Rollback condition |
|---|---|---|---|---|
| walker definition | C01 catalog and C02 state schema | definition catalog entry | definition query | discriminant/default mismatch |
| tile input port | accepted world-query boundary | snapshot adapter | walker evaluator | hidden world write or unstable query order |
| walker evaluator | definition, C02 authority, tile snapshot | pure transition calculator | commit root | transition differs at boundary cases |
| authority commit | evaluator result, lifecycle eligibility | walker owner/commit port | section behavior scheduler | duplicate writer or invalid state transition |
| recall effect | committed recall transition | explicit effect adapter | client/world effects | effect emitted from calculation or twice |
| presentation | committed facts, C02 presentation seam | projection/dummy adapter | draw path | visual cache mutates authority |

On failure, disable the new walker reader and retain the generic/legacy behavior adapter. Remove only
the newly added definition, evaluator, commit or effect writer that failed its verifier. Do not
delete `LeashedCritterBehaviorComponent`, rewrite the source fallback, or reset section/network/
persistence data as a rollback mechanism.

### 5.5.3 C03 acceptance evidence

- all source rows 132-138 have one proposed owner and are represented as definition data or an owner
  system input;
- state transitions use the generic C02 `State` byte and do not add a parallel walker state field;
- tile queries are explicit, deterministic, read-only inputs with stable predicate ordering;
- walking, falling, target-range clamp, recall timer and velocity results are committed once;
- recall and presentation effects are downstream commands/projections, not hidden evaluator writes;
- no System, Query, Command, Adapter, Projection, test, build, or behavior-equivalence result is
  claimed in this checkpoint; component-only implementation is recorded above.

## 5.6 C04 execution checkpoint: LeashedJumperBehavior

### 5.6.1 Implementation sequence

1. Register `LeashedJumperDefinition` with the two state discriminants and the declared constructor
   defaults. Keep the 11 source members as immutable definition fields; do not allocate a
   `JumperComponent` that duplicates generic C02 state.
2. Define a read-only `IJumperWorldQuery` and capture a stable snapshot for solid, liquid and
   in-world observations. The query adapter owns access to `WorldGen`/tile state and exposes no
   mutation method.
3. Implement deterministic target selection and reachable-tile evaluation using the C02 random
   port. Validate width, height, anchor range and bounds before creating a target transition.
4. Implement jump velocity, cooldown, movement and collision as a pure transition calculation.
   Commit `State`, `WaitTime`, `TargetPosition`, velocity and direction through the single jumper
   owner/commit port.
5. Represent recall as an explicit transition plus effect command. Run opacity, half-brick offset,
   dummy animation and other visual operations only after authority commit.
6. Migrate callers only after focused verifiers cover normal/recalling transitions, collision,
   liquid standing and malformed coordinates. Retain the legacy adapter and remove it only when a
   reproducible rollback path is recorded.

### 5.6.2 C04 dependency and rollback matrix

| Step | Depends on | New writer | Readers moved after | Rollback condition |
|---|---|---|---|---|
| jumper definition | C01 catalog, C02 state schema | definition catalog entry | jumper definition query | default/discriminant mismatch |
| world snapshot | tile/world ownership decision | read-only world adapter | target/collision evaluator | hidden write or inconsistent tile snapshot |
| target evaluator | definition, random port, snapshot | pure target calculator | jumper owner | target escapes range or random nondeterminism |
| jump/collision transition | target evaluator, C02 authority | jumper owner/commit port | behavior scheduler | partial velocity/state write or collision mismatch |
| recall effect | committed transition | explicit effect adapter | client/world effect path | duplicate or calculation-time side effect |
| reader migration | all focused verifiers | none | legacy callers | unverified network/lifecycle behavior |

On failure, disable the new jumper reader and retain the legacy/definition adapter. Remove only the
new failed writer or adapter. Do not mutate the authoritative report, another partition's document,
the shared ledger, or external tile state to make a failed verifier pass.

### 5.6.3 C04 acceptance evidence

- source rows 82-92 have one proposed owner and are classified as definition/discriminant data;
- all constructor defaults are explicit and immutable;
- world queries are read-only, deterministic inputs;
- target selection, jump physics, collision, cooldown and recall commit once through the jumper
  owner;
- liquid/water behavior is a query decision, not a hidden tile effect;
- no System, Query, Command, Adapter, Projection, test, build, or behavior-equivalence result is
  claimed in this checkpoint; component-only implementation is recorded above.

## 5.7 C05 execution checkpoint: LeashedFlyerBehavior

### 5.7.1 Implementation sequence

1. Register `LeashedFlyerDefinition` with flyer movement defaults and `HoverYVelocity`; keep
   `rotationScalar`, `hoverAmplitude`, and `hoverPeriod` in the presentation definition view rather
   than the authority component.
2. Add `IFlyerWorldQuery` and `IFlyerVisualClock` ports. Capture collision/ground observations as a
   stable read-only world snapshot and inject visual time only into the render projection.
3. Implement target selection, wait timing and ground bias through the C02 random/authority port.
   Keep the inherited target, wait, random cursor, direction and position facts in one generic
   authority; do not add a flyer duplicate.
4. Implement acceleration, braking, speed cap, zero-distance normalization, collision response and
   ground/air transition as one pure flyer transition. Commit position/velocity/target/wait/direction
   once through `LeashedFlyerCommitPort`.
5. Build presentation output after commit: dummy rotation, half-brick offset and hover bobbing are
   projection operations. They must not read or write network/persistence authority through hidden
   callbacks.
6. Migrate callers only after focused verifiers cover target selection, collision, `HoverYVelocity`,
   visual-clock determinism and single-writer behavior. Retain the legacy adapter until rollback is
   reproducible.

### 5.7.2 C05 dependency and rollback matrix

| Step | Depends on | New writer | Readers moved after | Rollback condition |
|---|---|---|---|---|
| flyer definitions | C01 catalog and C02 authority | definition catalog entry | flyer definition query | inherited/default mismatch |
| world snapshot | world ownership decision | read-only collision adapter | flyer evaluator | hidden world write or inconsistent observation |
| visual clock | presentation ownership decision | visual-clock adapter | bobbing projection | visual time reaches authority/network state |
| motion evaluator | definition, random port, world snapshot | pure flyer transition | flyer commit root | normalization/collision mismatch |
| presentation | committed facts, visual definition | projection/dummy adapter | draw path | rotation/bobbing mutates authority |
| reader migration | all focused verifiers | none | legacy callers | unverified network/lifecycle behavior |

On failure, disable the new flyer reader and retain the generic/legacy adapter. Remove only the new
failed definition, evaluator, or projection writer. Do not replace `Main.timeForVisualEffects` with a
new authority clock or treat the fallback implementation as a drop-in runtime patch.

### 5.7.3 C05 acceptance evidence

- source rows 70-80 have one proposed owner and are classified as definition or presentation data;
- movement transitions have one writer and use explicit world/random inputs;
- hover and rotation consume only presentation inputs after authority commit;
- `HoverYVelocity`, collision, ground bias and wait-range rules are covered by focused verifier
  plans;
- no System, Query, Command, Adapter, Projection, test, build, or behavior-equivalence result is
  claimed in this checkpoint; component-only implementation is recorded above.

## 5.8 C06 execution checkpoint: LeashedKiteBehavior

### 5.8.1 Implementation sequence

1. Register a kite definition with the Projectile content ID and any immutable trail-length or size
   metadata. Keep `projType` as content data, not a Projectile runtime identity.
2. Define `IKiteEnvironmentPort`, a section-availability query, a time input, and a Projectile
   definition/logic port. Capture wind, cloud, local wind area and content facts without letting the
   kite evaluator write world, player or Projectile global state.
3. Implement normal and fast-forward kite transitions around wind target/current, no-wind timer,
   derived distance, time counter, rotation and inherited position/velocity. Commit only accepted
   kite authority through one port.
4. Isolate `Projectile.KiteLogic`, `HandleMovement`, collision and first-appearance behavior behind
   `LeashedProjectileLogicPort`. Copy compatibility state in/out through a scoped adapter; never
   allocate or reuse a global Projectile entity as P02 authority.
5. Add the full/partial replication projection and interpolation projection. Validate content IDs,
   quantized rotation, position/velocity and environment fields before issuing a command; full
   receive resets interpolation and partial receive accumulates it.
6. Add trail and draw projections for frame, frame counter, sprite direction, old position/rotation
   history, cloud presentation and dummy draw input. Keep all arrays out of durable authority.
7. Migrate readers only after focused verifiers cover normal/fast-forward, wind/no-wind, collision,
   section gating, packet order, first appearance and single-writer ownership. Retain the legacy
   adapter until rollback is reproducible.

### 5.8.2 C06 dependency and rollback matrix

| Step | Depends on | New writer | Readers moved after | Rollback condition |
|---|---|---|---|---|
| kite definition | C01 catalog, Projectile content boundary | definition catalog entry | kite definition query | content/size/trail mismatch |
| environment input | world/network owner decision | read-only environment adapter | kite evaluator | hidden global write or unstable wind input |
| kite transition | environment, section snapshot, C02 anchor/lifecycle | kite owner/commit port | movement scheduler | normal/fast-forward or no-wind mismatch |
| Projectile bridge | Projectile API contract | scoped compatibility adapter | kite transition | dummy state escapes or runtime Projectile is allocated |
| replication/interpolation | authority schema and packet evidence | network projection/command adapter | full/partial consumers | field order or offset mismatch |
| trail/presentation | committed facts and content metadata | visual projection/dummy adapter | draw path | trail cache becomes authority |
| reader migration | all focused verifiers | none | legacy callers | unverified persistence/lifecycle behavior |

On failure, disable the new kite reader and retain the legacy projection/adapter. Remove only the
new writer or projection introduced by the failed step. Do not delete trail history as if it were
durable state, reset external wind/player state, or patch the fallback source in place.

### 5.8.3 C06 acceptance evidence

- source rows 108-126 and 154 have one proposed owner with explicit authority, adapter, projection,
  or query classification;
- environment and Projectile effects enter through explicit ports and have visible ordering;
- full/partial network commands validate content and interpolation semantics before commit;
- trail, frame, dummy and interpolation caches cannot mutate kite authority or persistence identity;
- anchor world position remains a pure query and tile/entity host ownership stays external;
- no System, Query, Command, Adapter, Projection, test, build, or behavior-equivalence result is
  claimed in this checkpoint; component-only implementation is recorded above.

## 5.9 C07 execution checkpoint: LeashedButterflyVariants

### 5.9.1 Implementation sequence

1. Register Empress and normal butterfly definitions in the species catalog. Keep `Prototype` as an
   immutable definition entry and share inherited flyer behavior through the C05 boundary.
2. Add `LeashedButterflyVariantComponent` only for the normal butterfly's per-entity `variant`.
   Hydrate it from an explicit item/style adapter and validate the accepted range before committing
   the entity state.
3. Extend the full/partial protocol adapter with the full-only variant suffix command. A partial
   packet must preserve the existing variant and must not consume a nonexistent byte.
4. Add a pure opacity query and a client visual system for `fadeAmount`. Read viewer distance,
   visual time, and random particle decisions through explicit visual inputs; clamp with the
   immutable `FadeAwayCap` rule.
5. Emit lighting/dust and dummy `ai[2]` effects through downstream ports/projections. They cannot
   write flyer motion, generic state, inventory, anchor, lifecycle, or definition catalog data.
6. Migrate butterfly readers only after focused verifiers cover full/partial suffixes, range errors,
   fade/opacity boundaries, presentation purity and single flyer-writer ownership. Retain the
   legacy adapter until rollback is reproducible.

### 5.9.2 C07 dependency and rollback matrix

| Step | Depends on | New writer | Readers moved after | Rollback condition |
|---|---|---|---|---|
| butterfly definitions | C01 catalog and C05 flyer boundary | definition catalog entries | definition query | prototype/inherited-default mismatch |
| item variant | item/style ownership decision | variant hydration adapter | spawn/default reader | source/range mismatch |
| full suffix | protocol framing evidence | network command adapter | full butterfly consumer | partial packet consumes suffix or range mismatch |
| opacity query | fade/cap rule | pure query | visual projection | query mutates state or wrong clamp |
| fade/effects | explicit visual input and effect ports | visual system/projection | draw/effect path | global time/random/dust writes authority |
| reader migration | all focused verifiers | none | legacy callers | unverified network/lifecycle/persistence behavior |

On failure, disable the butterfly reader and retain the legacy variant/projection adapter. Remove only
the new writer or projection introduced by the failed step. Do not reset the generic flyer authority,
rewrite item data, or treat the unabridged fallback as a runtime patch.

### 5.9.3 C07 acceptance evidence

- source rows 64-66, 127-128 and 153 have one proposed owner and explicit authority/presentation
  classification;
- full-only variant suffix behavior and malformed-input rejection are planned for verification;
- fade/opacity is pure/presentation-only and visual effects use explicit downstream ports;
- no System, Query, Command, Adapter, Projection, test, build, or behavior-equivalence result is
  claimed in this checkpoint; component-only implementation is recorded above.

## 5.10 C08 execution checkpoint: LeashedSpeciesPrototypes

### 5.10.1 Implementation sequence

1. Freeze the 13-row source-to-definition map and the identity distinction from C01. Do not assign
   new numeric IDs until the accepted registry owner confirms the existing Version4 mapping.
2. Introduce `LeashedSpeciesDefinitionCatalog` as an immutable bootstrap result. Flatten constructor
   defaults into data bundles for shared Walker/Jumper/Flyer definitions and record custom override
   capabilities without constructing mutable prototype instances per entity.
3. Add explicit content binding validation for item, NPC and Projectile content IDs, anchor style,
   aquatic/water capability, and custom network/visual seams. Unknown mappings fail before entity
   registration.
4. Make `LeashedEntityRegistrationSystem` consume a validated definition ID and catalog row. Entity
   hydration stores the definition binding and capability components; it never stores a prototype
   object reference or creates a species-specific runtime component.
5. Migrate behavior systems to query shared definitions and species overrides. Keep scheduler order
   explicit in the registration/section/behavior contract; class inheritance and file order cannot
   decide execution order.
6. Add catalog and content-binding verifiers before moving readers. Retain the legacy prototype
   adapter until deterministic registration, no-aliasing, rollback, and unknown-mapping behavior are
   proven. The original plan did not authorize catalog implementation; only the component-only
   implementation recorded above is active, and catalog work remains deferred.

### 5.10.2 C08 dependency and rollback matrix

| Step | Depends on | New writer | Readers moved after | Rollback condition |
|---|---|---|---|---|
| source map | C01 identity and all C02-C07 boundaries | none | verifier input only | source row/default conflict |
| catalog bootstrap | accepted definition IDs and behavior schemas | immutable catalog bootstrap | definition queries | duplicate ID or mutable alias |
| content binding | item/NPC/Projectile and anchor owners | binding adapter | registration hydration | unknown/conflicting mapping |
| entity registration | catalog, binding, lifecycle and relation owners | registration commit root | runtime entity readers | partial entity or prototype reference |
| shared behavior reads | catalog plus C03-C07 verifiers | none | Walker/Jumper/Flyer/Kite/Butterfly systems | inheritance/file order used as behavior order |
| legacy removal | all focused verifiers and rollback path | none | all definition consumers | catalog/network/persistence regression |

On failure, retain the legacy prototype/definition adapter and disable the new catalog reader. Remove
only the new catalog or binding writer introduced by the failed step. Do not renumber registry IDs,
mutate source prototype objects, rewrite item/NPC/Projectile data, or modify another partition's
document/report/ledger.

### 5.10.3 C08 acceptance evidence

- all 13 source prototype members have one immutable catalog row;
- shared behavior definitions and species overrides are explicit data, not runtime inheritance state;
- content and anchor integration inputs are validated before registration;
- runtime entities store definition IDs and capability components, never prototype object references;
- catalog lookup is deterministic and immutable after bootstrap;
- rollback preserves the legacy adapter and leaves no partially initialized entity;
- no System, Query, Command, Adapter, Projection, test, build, or behavior-equivalence result is
  claimed in this checkpoint; component-only implementation is recorded above.

## 6. Later checkpoint plan

| Checkpoint | Primary implementation unit | Required evidence before implementation | Planned focused verifier |
|---|---|---|---|
| C02 | `LeashedCritterBehaviorComponent` + `LeashedCritterSystem` | random cursor, target, wait/state, NPC content, definition/configuration and presentation/dummy split | spawn/defaults, full/partial state, random determinism, recall, interpolation and visual cache isolation |
| C03 | `LeashedWalkerDefinition` + `LeashedWalkerSystem` | fallback walker transitions and tile query semantics | standing/direction/walking/falling/recall transitions and anchor-range clamp |
| C04 | `LeashedJumperDefinition` + `LeashedJumperSystem` | jump cooldown, reachable tile and water semantics | jump bounds, collision, cooldown, recall, water branch and deterministic target selection |
| C05 | `LeashedFlyerDefinition` + `LeashedFlyerSystem` | flyer movement, ground bias, hover and random timing | acceleration/braking, collision, target range, ground bias, visual bob separation |
| C06 | `LeashedKiteBehaviorComponent` + `LeashedKiteSystem` | wind/cloud ports, projectile definition, scoped dummy bridge, trail/interpolation and pure anchor query | fast-forward, wind/no-wind, trail cache, network state, section gating and anchor movement |
| C07 | `LeashedButterflyVariantComponent` + butterfly visual projection | item/style variant, full-only suffix, fade cap and opacity ownership | variant full/partial sync, range rejection, fade cap, opacity purity and presentation-only writes |
| C08 | `LeashedSpeciesDefinitionCatalog` | all 13 prototype constructors, shared behavior bundles, custom override capabilities and item/NPC/Projectile mapping | deterministic catalog, species selection, definition defaults, unknown mapping rejection and no inheritance runtime owner |

All non-component rows remain plan-only. Their code paths, tests and verifier results must not be
reported as existing until they are actually created and run.

## 7. Checkpoint record

- Completed component units: `LeashedEntityStateComponent`, `LeashedEntityLifecycleComponent`,
  `LeashedEntityAnchorRelationComponent`, `LeashedEntitySectionMembershipComponent`,
  `LeashedEntityLegacySlotComponent`, `LeashedCritterBehaviorComponent`,
  `LeashedKiteBehaviorComponent`, `LeashedButterflyVariantComponent`.
- Current: `none`.
- Pending component units: none.
- `implementationStatus`: `implemented` for the component-only scope; all planned non-component
  paths remain deferred.
- `evidence-gap`: All eight checkpoints have an explicit execution plan, but current-checkout
  behavior bodies, stable definition IDs, item/NPC/Projectile mapping, external section and anchor
  ownership, network application, persistence, and runtime equivalence remain partial.
- `blocking-decision`: `integration-review`.
- `verificationStatus`: `not-verified`.
- No test, focused behavior verifier, or behavior-equivalence check was executed. The eight
  component source files were modified or added under `src/LeashedEntity`; the affected project
  build passed with 0 warnings and 0 errors, and the artifact is recorded above.
