# Authoritative P03 Mount and Vehicle System Execution Plan

documentKind: system-execution-plan
partitionId: P03
taskId: AUTH-SYS-P03
derivedFromDesign: D:\TRbackup\NLTX\docs\system-decomposition\design\2026-09-30-authoritative-P03-mount-vehicle-system-design.md
derivedFromSystemReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P03-mount-vehicle.md
claimInputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P03-Mount-Vehicle.md
outputReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P03-mount-vehicle.md
settledSessionId: 453b6a2e98c54a31a10b0a17d4c1e816
targetSource: D:\TRbackup\Version4
fullReferenceSource: D:\TRbackup\无任何删减通过编译
ecsReferenceSource: C:\Users\shan\Downloads\ECS\space-station-14-master
migrationRoot: D:\TRbackup\NLTX\src\NSSLC
sourceModified: false
testsRun: false
buildRun: false

| Field | Value |
| --- | --- |
| `partitionId` | `P03` |
| `taskId` | `AUTH-SYS-P03` |
| `sourceReportSessionId` | `453b6a2e98c54a31a10b0a17d4c1e816` |
| `executionStatus` | `partial-core-slice` |
| `implementationStatus` | `partial` |
| `verificationStatus` | `not-run` |
| `historicalCoreSliceEvidence` | `existing-evidence` |
| `fullVerificationStatus` | `not-run` |
| `migrationStatus` | `deferred` |
| `focused verifier` | `Test/Terraria.Player.MountSystem.CoreVerification` |
| design document | `docs/system-decomposition/design/2026-09-30-authoritative-P03-mount-vehicle-system-design.md` |
| static report | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P03-mount-vehicle.md` |
| primary source | `D:\TRbackup\Version4` |
| complete reference source | `D:\TRbackup\无任何删减通过编译` |
| organization reference | `C:\Users\shan\Downloads\ECS\space-station-14-master` |
| target tree | `D:\TRbackup\NLTX\src\NSSLC` |

This is the P03 execution record derived from the proposed System design. It records the isolated
core implementation and its focused verifier while preserving the remaining integration gates.
The organization reference is used only for ECS boundary vocabulary and layout comparison; it
does not supply target behavior. No external Player/Collision/Tile/Projectile/network/persistence
integration was wired, and no other partition, input ledger, prompt, report, or CPG artifact was
modified.

## 1. Execution Rules

1. Establish the source revision and integration decisions before adding a writer.
2. Work only in the P03 mount/vehicle boundary; Player, Collision, Tile, Projectile, network,
   persistence, and presentation owners remain integration handoffs until explicitly approved.
3. Keep one authoritative writer for each invariant. Existing `PlayerMountState`,
   `PlayerMountComponent`, empty `MountDefinition`, and `PlayerMountVehicleIntegrationComponent`
   must be adapted or retired only after a one-writer audit.
4. Use the Query API and source inspection for every unresolved caller or writer. A zero CPG result
   with `NoMatchingFactInScannedScope` is a gap, not a negative relation.
5. Keep Definitions immutable, Queries side-effect free, Commands explicit, Adapters at external
   effect boundaries, and Projections one-way.
6. Every phase has a focused verifier and rollback condition. Only the isolated core phase is
   recorded as executed below; all other phase gates retain their explicit `not-run` state until a
   later implementation session closes them.
7. Do not use directory order, file order, or registration order as a runtime schedule.

## 2. Evidence and Source-Revision Gate

Before implementation, record the exact source revision used for the behavior contract. The
primary Version4 CPG artifact is read-only and has manifest SHA-256
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`; it has no source snapshot ID.
The complete reference files have different hashes from the primary tree, so they are a
supplement, not an implicit replacement.

| Evidence to confirm | Current evidence | Gate |
| --- | --- | --- |
| `SetMount`, `Dismount`, `UpdateFrame`, `UseAbility`, `AimAbility`, `CanFly`, `CanHover` callers | CPG `complete` positive call-site results | record caller identity and preserve in focused coverage |
| effects, drill, hover, flight and recovery callers | CPG `partial` zero results plus complete-reference/source `Player.cs` calls | close with source or runtime trace before routing writers |
| `Mount.Initialize` bootstrap | complete reference has `Main.cs:6743-6744` and `11037-11038`; Version4 CPG has a positive Main call | define idempotent or scoped bootstrap behavior |
| drill smart cursor | complete reference `Mount.cs:3193-3268` has real tile-line logic; primary Version4 report marked it `unknown` | select source revision and compare behavior before Drill implementation |
| external effects | complete reference `Mount.cs:4882-5290` writes Player stats and emits light/dust/projectiles | define ports, ordering, failure and retry semantics |

The static report's read-only Query API check used the existing `CpgEvidence.ps1` entry point. The
same command shape is retained here for reproducibility; this implementation session did not
upgrade any `partial` relationship to `complete`:

```powershell
. '.\.agents\skills\ecs-system\tools\CpgEvidence.ps1'
Initialize-CpgEvidence -DatabasePath 'D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite'
Start-CpgEvidenceServer
Find-CpgSymbols -Name 'SetMount' -Kind SymbolMethod -SourcePath @('Terraria/Mount.cs', 'Terraria/Player.cs')
Find-CpgCallSites -MethodSymbolId <resolved-symbol-id> -SourcePath @('Terraria/Mount.cs', 'Terraria/Player.cs', 'Terraria/Projectile.cs')
Close-CpgEvidence
```

The recorded result remains the report baseline: `SetMount`, `Dismount`, and `UpdateFrame` have
`complete` positive call-site evidence; effects, drill, hover, flight and recovery relations
remain `partial`/`unknown` where stated above. A zero result with
`NoMatchingFactInScannedScope` is not treated as no caller.

The document-pass query refresh returned the following compact record. It was read-only and did
not build or modify the reader, database, source tree, or reports:

| Method | `Find-CpgSymbols` | `Find-CpgCallSites` | Sites | Scope observed |
| --- | --- | --- | ---: | --- |
| `SetMount` | `complete` | `complete` | 2 | `Player.cs` |
| `Dismount` | `complete` | `complete` | 3 | `Mount.cs`, `Player.cs` |
| `UpdateFrame` | `complete` | `complete` | 5 | `Mount.cs`, `Player.cs` |
| `UseAbility` | `complete` | `complete` | 4 | `Player.cs`, `Projectile.cs` |
| `AimAbility` | `complete` | `complete` | 3 | `Mount.cs`, `Projectile.cs` |
| `CanFly` | `complete` | `complete` | 3 | `Player.cs`, `Projectile.cs` |
| `CanHover` | `complete` | `complete` | 2 | `Mount.cs`, `Player.cs` |
| `UpdateEffects`, `UpdateDrill`, `UseDrill`, `Hover`, `Flight`, `AbilityRecovery`, `FatigueRecovery`, `ResetFlightTime` | `complete` | `partial` | 0 | gap `NoMatchingFactInScannedScope`; source follow-up required |

The complete reference is recorded separately from the CPG snapshot. Current file hashes are:

| File | SHA-256 |
| --- | --- |
| `D:\TRbackup\Version4\Terraria\Mount.cs` | `2DED2B174731DDDD003BCD1B03F83853D0AD39183CD3683B4C5289459AEDCEFC` |
| `D:\TRbackup\无任何删减通过编译\Terraria\Mount.cs` | `3F94D523F50A44498BB3E8FC7F98FA18AD11DE5EA1E65AFAD6E0D81F05040964` |
| `D:\TRbackup\Version4\Terraria\Player.cs` | `E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86` |
| `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs` | `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C` |

The full reference source is used to close concrete relationship gaps left by the Version4 index:
`Main.cs:6743-6744` and `11037-11038` initialize Mount and Minecart; `Player.cs:26051`,
`27020-27041`, `27136`, `28658`, and `36670-36686` interleave effects, recovery, drill, flight,
and frame updates; `Mount.cs:3193-3268` contains non-empty block/wall smart cursors; and
`Mount.cs:4882-5290` contains Player writes and lighting/dust/random/projectile effects. These
facts determine the next execution gates but do not upgrade the CPG gaps or prove runtime
equivalence.

## 3. Target Layout and Contracts

The proposed target remains adjacent to the existing Player mount components:

```text
src/NSSLC/Component/Player/Mount/
  Definitions/
  Components/
  Queries/
  Systems/
  Adapters/
  Projections/
```

The following targets are proposed and must not be created as empty placeholders:

| Area | Proposed types | Contract |
| --- | --- | --- |
| Definitions | `MountDefinitionCatalog`, frame/geometry/movement/vehicle/drill catalog definitions | bootstrap-only immutable data; no player counters or executable delegates |
| Components | existing frame/flight, fatigue/ability, drill, and variant components | data only; internal setters controlled by owner Systems |
| Systems | `MountRuntimeSystem`, `DrillMountSystem` | sole writers for Mount runtime and drill invariants |
| Queries | identity/frame, mobility/ability, catalog, and qualification queries | immutable snapshots; no collision writes, callbacks, random, logging, network, or persistence |
| Adapters | `IPlayerMountCommitPort`, `IMountCollisionPort`, `IMountTileActionPort`, `IMountProjectilePort`, `IMountPresentationEffectPort` | translate external capabilities; do not copy Mount rules |
| Projections | network and persistence projections | one-way committed-state output; no reverse writes |

## 4. Phase Plan

### Phase 0: Source and owner lock

**Inputs:** P03 report, design document, Version4 source, complete reference source, current NLTX
tree, CPG query results.

**Actions:**

- choose the source revision for catalog, drill cursor, delegate, and effect behavior;
- inventory all current mount writers in `src/NSSLC` and mark duplicate state;
- decide whether `MountRuntimeSystem` owns ability/fatigue directly or through a partial
  collaborator without a second writer;
- record Player/Collision/Tile/Projectile/network/persistence integration owners.

**Exit gate:** no unresolved decision may be hidden by creating a second component or facade.

**Rollback:** stop before any source edit and record the unresolved owner as
`crossSubsystemOwner: integration-review`.

### Phase 1: Immutable catalogs and read surfaces

| Unit | Scope | Proposed target | Dependencies | Focused gate |
| --- | --- | --- | --- | --- |
| C01 | frame/draw constants and rat frames | `Definitions/MountFrameAndDrawCatalogDefinition.cs`, `Queries/MountFrameAndDrawQuery.cs` | source revision | defaults, ranges, immutable-array and deterministic-read checks |
| C02 | mount registry and Scutlix/Santank data | `Definitions/MountSpecialVehicleCatalogDefinition.cs`, query | bootstrap decision | IDs, duplicate slots, transformed eye positions and no mutable registry exposure |
| C03 | drill constants | `Definitions/MountDrillConstantsDefinition.cs`, `Queries/MountDrillRulesQuery.cs` | C02 bootstrap | source values, finite ranges, no runtime target/cooldown ownership |
| C04 | Super Cart values | `Definitions/MountSuperCartDefinition.cs`, `Queries/MountSuperCartQuery.cs` | mobility owner decision | override qualification, ordinary fallback and no-write query checks |

**Commit rule:** catalog data may be added behind a read-only compatibility path. No legacy writer
is redirected until all values are source-bound and the catalog snapshot lifecycle is approved.

**Rollback:** remove only the new read path and preserve the legacy catalog if any source value,
slot, array length, default, or bootstrap scope differs.

### Phase 2: Runtime owner and lifecycle transitions

| Unit | Scope | Owner | Required behavior | Rollback trigger |
| --- | --- | --- | --- | --- |
| C05 | active/type/frame/flight/grace/reset | `MountRuntimeSystem` + frame/flight component | `SetMount`, `Dismount`, `Reset`, `UpdateFrame`, `Flight`, `ResetFlightTime` preserve state delta and commit order | second writer, stale projection, or altered reset/frame sequence |
| C06 | fatigue and ability timers | same runtime owner or approved partial collaborator | fractional fatigue, charge/cooldown/duration, aim/active/charging transitions | polarity, zero-max, charge bound, or recovery mismatch |
| C17 | tagged variants | runtime owner with variant component | Boolean/selective-flying/extra-frame setup, reset and variant exclusivity | untyped state leak or unapproved schema change |

The phase must first route one writer for activation/reset, then expand to per-tick transitions. It
must not route `UpdateEffects` or drill tile/projectile effects before the owner invariant is proven.

### Phase 3: Pure projections and qualification

| Unit | Proposed target | Inputs | Gate |
| --- | --- | --- | --- |
| C07 | `Queries/MountRuntimeIdentityAndFrameProjectionQuery.cs` | committed runtime + immutable catalog | inactive/default, offset bounds, deterministic repeated reads, no writeback |
| C08 | `Queries/MountRuntimeMobilityAndAbilityProjectionQuery.cs` | runtime, catalog, Super Cart query, explicit Player snapshot | speed/cart/rail/wing/ability cases and non-executable delegate descriptors |
| C09 | `Definitions/MountGeometryAndOffsetCatalogDefinition.cs` + query | catalog bootstrap | frame arrays, bounds, inactive defaults, no collision mutation |
| C10-C12 | ground/aerial/water/dash frame Definitions + Queries | catalog + runtime frame state | sequence, delay, idle/random input and special variant cases |
| C13 | movement/ability catalog + query | C03/C04 and runtime bounds | source values, zero semantics, resource seeding and item-use dismount policy |
| C14 | vehicle/presentation catalog + adapter-facing query | C02 and effect port | descriptor-only data; no callback execution or Player writes |

Qualification queries must return a snapshot or failure reason. `CanMount` and dismount checks may
use external Collision ports, but the owner must recheck the invariant immediately before commit.

### Phase 4: Effect and delegate boundary

**Unit C15:** introduce `IMountEffectPort` and `MountDelegateAdapter` only after Player, lighting,
dust, sound, projectile and random boundaries are assigned. Translate delegate descriptors; do not
store executable callbacks in ECS components.

Required effect order is explicit: authoritative state commit, Player geometry/stat commit,
projectile/tile commit where applicable, then presentation publication. The exact legacy order is
`partial` until trace evidence exists.

**Rollback:** keep the compatibility path if an adapter emits duplicate or reordered effects, or
if exception/retry semantics cannot be observed.

### Phase 5: Drill owner

**Unit C16:** introduce `DrillMountSystem` over `DrillMountRuntimeComponent`.

1. Resolve the source revision for `DrillSmartCursor_Blocks/Walls`.
2. Convert cursor and input to an immutable request snapshot.
3. Validate beam deduplication, tile target and wall/block purpose.
4. Publish explicit tile/projectile/dust/sound intents.
5. Commit beam cooldown, target and rotation state under the drill owner.

The complete reference's `PlotTileLine`, `WorldGen.CanKillTile`, `Main.tile`, and
`Player.CanPlayerSmashWall` calls must be represented by explicit ports or reviewed integration
owners. No cursor implementation is copied into NLTX by this document.

### Phase 6: Compatibility and external integration

- route old Player and Projectile callers through a compatibility adapter to the new composition;
- preserve old return/error and effect ordering until observation-vector comparisons pass;
- add network and persistence projections only after variant, drill, fatigue, frame and ability
  schemas are approved;
- verify item-use, buff-loss, hit, death, equipment and failed-dismount paths;
- keep the old facade and any shadow state read-only or isolated; it may not become a second effect
  publisher.

`crossSubsystemOwner: integration-review` remains mandatory for Player, Collision, Tile, Projectile,
network, persistence, Minecart track logic, lighting, sound, dust and random services.

### Phase 7: Verification and deletion gate

The implementation session must run focused checks in the real migration project, then the project
acceptance process. Required scenario groups are:

- catalog bootstrap twice, invalid IDs, missing definitions, reload and unload;
- mount activation, replacement, blocked space, wetness, grappling and dismount failure;
- reset, death, item-use, buff loss and equipment reconciliation;
- frame, idle randomness, dash, hover, flight, fatigue recovery and ability charge/release;
- drill blocks/walls, duplicate beams, cooldown, tile failure and projectile failure;
- minecart delegates, light/dust/sound ordering and adapter retry;
- network prediction, save/load, restore, multi-world/session isolation and rollback.

The old implementation cannot be deleted until static references, dynamic/reflection/configuration
entries, runtime traces, observation-vector differences and rollback evidence are all closed.

## 5. Per-Phase Evidence Record

Each implementation change must record:

```text
source revision and file hash
target path and owning System
read set / write set / emitted effects
inbound and outbound callers
schedule barrier and visibility point
focused verifier and result
rollback condition
unknown / partial / evidence-gap items
```

The CPG API result status, source path, symbol ID, call-site span, and manifest hash must be kept
with the evidence record. Complete-reference facts must include the separate source path and hash;
they must not be silently merged into the Version4 CPG snapshot.

## 6. Current Execution Status

The following core slice is implemented under `src/NSSLC/Component/Player/Mount`:

- immutable frame, geometry, movement, ability, presentation, drill, special-vehicle and Super Cart
  Definitions;
- duplicate-rejecting `MountDefinitionCatalog` with defensive geometry offsets;
- `MountRuntimeSystem` lifecycle, frame/grace, flight, fatigue and ability resource transitions;
- read-only runtime snapshot, identity/frame, mobility/ability and qualification Queries;
- explicit Player/Collision/Tile/Projectile/presentation Adapter contracts;
- `DrillMountSystem` rotation, bounded beam reservation, duplicate suppression and cooldown target
  clearing.

An earlier workspace checkpoint contains the following focused-core verifier record. It is retained
as historical `existing-evidence`; this documentation pass did not run the command again:

```text
project: Test/Terraria.Player.MountSystem.CoreVerification/Terraria.Player.MountSystem.CoreVerification.csproj
build: exit 0; 0 warnings; 0 errors
artifact: Build/bin/Terraria.Player.MountSystem.CoreVerification/Debug/net10.0/
run: --no-build --no-restore; exit 0
result: PASS: mount catalog, lifecycle, frame, resources, qualification, and drill core
```

This is focused evidence for an isolated core only. No runtime trace, behavior differential,
network/save check, scheduler registration, external adapter, deletion gate, or full P03 scenario
group was run for this document pass. Player geometry/buffs, Collision, Tile, Projectile, effects,
random/time, multi-world scope, compatibility callers and source-revision-dependent behavior
remain `unknown` or `partial`; the status remains `executionStatus: partial-core-slice`,
`implementationStatus: partial`, `verificationStatus: not-run`,
`historicalCoreSliceEvidence: existing-evidence`, `fullVerificationStatus: not-run`, and
`migrationStatus: deferred`. The report is not re-claimed or re-settled; the original session ID
above remains the provenance for this partition.
