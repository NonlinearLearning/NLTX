# Version4 P13 NPC, Town, Event, Boss, and World Progression Execution Plan

```yaml
partitionId: P13
sessionId: 06c2332691e84e2682405500e43d7be8
inputReport: D:\\TRbackup\\NLTX\\docs\\组件文档\\迁移参考表\\Version4权威模拟系统字段属性逐成员源码声明-20分区\\P13-NPC-Town-Progression.md
designPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-p13-npc-town-progression-component-design.md
executionPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-p13-npc-town-progression-component-execution.md
componentDesignPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-p13-npc-town-progression-component-design.md
componentExecutionPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-p13-npc-town-progression-component-execution.md
designStatus: proposed
evidenceStatus: partial
currentNltxStatus: partial
executionStatus: in-progress
implementationStatus: in-progress
verificationStatus: partial
completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14, C15, C16, C17]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T07:02:53.7727506Z
evidence-gap: C01, C02, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14, C15, C16, and C17 have isolated owner builds and smoke evidence. C03 isolated registry build and boundary smoke passed, but its runtime allocator bridge, spawn/despawn writer, save/network projection, and behavior equivalence remain open. C04 isolated NPC build, focused verifier, and boundary smoke passed, but the Version4 scheduler caller, transport acknowledgement/retry path, entity lifecycle reset caller, client/server authority, duplicate packet suppression, and behavior equivalence remain open. Existing `NpcIdentityComponent` still marks Version4 stable instance identity as missing, while the C03 key carries the required instance, compatibility slot, generation, and NPC type explicitly. C09 now also has an explicit scan owner that consumes caller-provided active entries, but the real `Main.npc` caller, scheduler placement, lifecycle invalidation hooks, and behavior equivalence remain unverified. C10 WorldGen/Projectile writers and dynamic `downedMoonlord` shield policy, C11 boss defeat event routing and persistence/network adapters, C12 event-end routing and persistence/network adapters, C13 townNPC/dialogue/housing adapter integration, C14 travel selector/entity relation/persistence integration, C15 Town versus WorldGeneration housing owner, resident key mode, revision and projection integration, and C16 tile validity/door system/adapter integration also remain open. C17 Version4 environment caller, life-damage handling after breath reaches zero, liquid exception rules, save/network compatibility, scheduler placement, and behavior equivalence remain unverified. The integrated P13 focused verifier source has passed serial restore, build, and run for the 105 confirmed source-owner members; Version4 initializer parity and full runtime writer/reset/persistence/network closure remain unverified; `nextDialogue` readers and writers remain unproven.
blocking-decision: C03 isolated build and boundary smoke passed using an explicit temporary exclusion for the unrelated WorldGeneration source error; runtime lifecycle routing and legacy reader migration remain deferred until their callers are evidenced. C04 isolated build, existing focused verifier, and boundary smoke passed with one dirty-state owner; transport scheduler, send acknowledgement, retry integration, entity lifecycle caller, and client/server authority remain unverified. C01 remains isolated to immutable definition data and a bounded countdown state; WorldGen reset, item command, save projection, and network projection remain deferred until their writers and adapters are evidenced. C09 scan ownership is explicit and deterministic, but its real active-NPC enumeration caller, invalidation hooks, and runtime scheduling remain deferred because no evidence-backed integration boundary was found. C17 is limited to the confirmed immutable breath rules and deterministic state transition; health damage, environment reads, lifecycle scheduling, and projections remain outside the component until their callers are evidenced. The integrated P13 focused verifier passed its serial restore, build, and run, but the result is bounded to the confirmed source-owner members and does not close runtime integration or `nextDialogue` projection evidence. Integration review must still approve world-level ownership for boss/event/progression flags, NPC network dirty-state integration, housing authority between src/Town and src/WorldSession/WorldGeneration, and the nextDialogue projection boundary.
```

## 1. Plan contract

This document is an in-progress, reversible implementation sequence. C01-C17 source files have
been saved. The affected projects and component-level smoke checks have run for the completed
checkpoints, including the C03 registry and C04 network-intent boundaries. No integrated focused
verifier, network replay, save/load replay, runtime scheduler/transport integration, or
behavior-equivalence check has established full P13 closure. The current status is:

```text
executionStatus: in-progress
implementationStatus: in-progress
verificationStatus: partial
```

Only P13 members may be migrated. The Version4 public names, namespaces, serialization shape, packet
shape, and behavior must remain behind adapters until a replacement owner passes focused verification.
There must be one writer for each authoritative field; a compatibility bridge may read the owner but
may not write both old and new representations.

## 2. C01 initial checkpoint

`C01 MoonLordEncounterDefinitionAndState` covers report members 1587..1593. The first implementation
unit must keep the following separation:

- attack arrays and countdown rule values move behind an immutable definition/config adapter;
- `MoonLordCountdown` moves behind one world encounter state owner;
- countdown requests become explicit commands and committed changes become events;
- WorldGen reset, item-triggered countdown, NPC lifecycle changes, save/load, and network output
  each use a defined port;
- no NPC entity receives a copy of the global Moon Lord state.

The checkpoint is design-complete only. No source or test file has been changed.

## 3. Ordered implementation units

Each unit is small and independently revertible. Before moving a type, record the source path, target
path, namespace, dependency impact, one writer, and focused verifier. File order is never the runtime
schedule.

| Unit | Checkpoint | Source-to-target boundary | Single writer | Dependency impact |
|---:|---|---|---|---|
| 0 | verifier harness | `Test/Terraria.Npc.Components.Verification` -> focused P13 scenarios | verifier assertions only | references affected NPC/Town/WorldSession projects |
| 1 | C01 | `Terraria/NPC.cs:5915-5927` -> `WorldSession/NpcProgression/MoonLord` | Moon Lord encounter system | WorldGen reset, item command, save/network projections |
| 2 | C02 | `Terraria/NPC.cs:5937-5941` -> `WorldSession/NpcProgression/Invasion` | invasion wave system | invasion event and world calendar queries |
| 3 | C03 | `Terraria/NPC.cs:6007-6013` -> `WorldSession/NpcProgression/Boss` | boss registry system | NPC identity/lifecycle and active-slot cleanup |
| 4 | C04 | `Terraria/NPC.cs:6015` -> `Npc/Network` | NPC replication scheduler | `NpcReplicationDirtyState`, NetMessage projection |
| 5 | C05 | `Terraria/NPC.cs:6151-6165` -> `Town/Progression/Rescue` | rescue commit system | spawn eligibility, save and network flags |
| 6 | C06 | `Terraria/NPC.cs:6167-6171` -> `Town/Progression/Pets` | pet adoption commit system | purchase input, town NPC population |
| 7 | C07 | `Terraria/NPC.cs:6173-6203` -> `Town/Progression/Spawn` | spawn unlock commit system | spawn query and world progression |
| 8 | C08 | `Terraria/NPC.cs:6205-6209` -> `WorldSession/NpcProgression/Books` | book usage commit system | item/use command and progression projection |
| 9 | C09 | `Terraria/NPC.cs:6293` -> `Npc/Queries` | active scan owner | NPC population scan only; never save as authority |
| 10 | C10 | `Terraria/NPC.cs:6265-6283` -> `WorldSession/NpcProgression/LunarTower` | tower event system | tile/event state, shield transitions, network projection |
| 11 | C11 | `Terraria/NPC.cs:6211-6219,6229-6231,6235,6247-6249,6259-6263,6285-6291` -> `WorldSession/NpcProgression/Boss` | defeat commit system | save/load, achievement/event queries, mech derived policy |
| 12 | C12 | `Terraria/NPC.cs:6221-6227,6233,6237-6245,6251-6257` -> `WorldSession/NpcProgression/Events` | event defeat commit system | event eligibility and persistent world flags |
| 13 | C13 | `Terraria/NPC.cs:6397-6399` -> `Town/Residents` plus dialogue adapter | resident system; dialogue adapter deferred | TownResidentComponent and ConditionalDialogue integration |
| 14 | C14 | `Terraria/NPC.cs:6401` -> `WorldSession/Town` | travel-NPC lifecycle system | travel spawn/despawn and world event state |
| 15 | C15 | `Terraria/NPC.cs:6403-6421` -> `Town/Housing` | housing assignment system | duplicate Town/WorldGeneration housing types and room manager |
| 16 | C16 | `Terraria/NPC.cs:6425-6429` -> `Town/Doors` | door intent consumer | tile/door adapter and NPC movement schedule |
| 17 | C17 | `Terraria/NPC.cs:6433-6437` -> `Npc/Environment` | breath system | liquid/environment query and NPC reset |
| 18 | compatibility removal | legacy `NPC` reads/writes -> adapters, then removal | replacement owner per field | full P13 regression and integration review |

The source-to-target table is a plan. It does not mean the target files exist.

## 4. Target namespace and ownership rules

World-scoped types belong under `Terraria.WorldSession.NpcProgression` and its capability
subdirectories. NPC-instance types belong under `Terraria.Npc` capability directories. Town-specific
types belong under `Terraria.Town`. Serialization and transport remain in adapters under
`Terraria.WorldStorage` and server/network boundaries. Do not create `Shared/Components` or a generic
NPC catch-all directory.

For each unit:

1. Add or identify the single owner and its command/commit port.
2. Add a focused verifier fixture before moving behavior.
3. Migrate readers to a read-only query or projection.
4. Migrate the one writer and preserve the legacy API through a read adapter if required.
5. Verify reset, duplicate input, lifecycle cleanup, save/load, and network projection boundaries.
6. Record the checkpoint in both P13 documents before starting the next unit.

## 5. Save/load, network, and adapter boundary

`WorldFile` reads and writes a versioned `NpcProgressionSaveProjection`. The projection contains
world flags, encounter state, registry references only in a stable serialized key form, and housing
relations according to the approved owner. It must not serialize `NpcActivePresenceCache`, door
intent, `breathCounter` unless the legacy contract proves it is persistent, or `nextDialogue` before
its contract is closed.

`NetMessage` and `MessageBuffer` use `NpcProgressionNetworkProjection` and
`NpcNetworkSyncIntentComponent`. Packet DTOs, client stream cursors, and transport acknowledgements
remain outside authority components. `netUpdate` is consumed once by the network scheduler and then
cleared or acknowledged by that scheduler; a projection must not reassert it.

Legacy bridges are read adapters during migration. They may translate an old static field access into
the new world owner, but no compatibility period may write both representations. Derived
`downedMechBossAny` must have either one explicit stored owner or one explicit derivation point after
integration review; the migration cannot mix both.

## 6. Proposed system schedule

The implementation must install or document an explicit schedule equivalent to:

```text
1. Validate world revision, NPC entity identity, and incoming command source.
2. Apply spawn/rescue/adoption/unlock/book commands in the progression commit phase.
3. Advance Moon Lord, invasion, tower, and travel-NPC world systems.
4. Resolve boss index registration and cleanup against NPC lifecycle events.
5. Apply defeat/event commits once and publish committed facts.
6. Update town resident/housing/door state and environmental breath state.
7. Rebuild or invalidate the active presence cache for the current scan.
8. Produce save/network/presentation projections from committed state.
```

Exact ordering must be reconciled with Version4 `Main`, `WorldGen`, `NPC`, `WorldFile`,
`NetMessage`, and `MessageBuffer` call paths before implementation. A directory or source-file order
cannot establish execution order.

## 7. Focused verifier plan

The existing focused project is `Test/Terraria.Npc.Components.Verification/` with project
`Terraria.Npc.Components.Verification.csproj`. Planned scenarios include:

- C01/C02: default, reset, countdown transition, item countdown, invasion points, wave kill and
  wave-number monotonicity;
- C03/C04: active boss slot registration/cleanup, stale slot rejection, `whoAmI`/identity mapping,
  `netUpdate` consumption and duplicate sync suppression;
- C05-C08: rescue, pet adoption, spawn unlock, and progression-book commands are idempotent and
  have one writer;
- C09: active presence cache is rebuilt/invalidate-only, never serialized or used as durable truth;
- C10-C12: tower runtime shield/active state is distinct from persistent tower defeat flags, boss
  and event defeat commits round-trip, and `downedMechBossAny` has one owner;
- C13-C17: resident/travel boundaries, dialogue deferral, housing transitions and old snapshots,
  door intent consumption, breath reset and environment-driven updates;
- cross-cutting: save/load projection, network projection, world reset, duplicate command, stale
  entity, interrupted tick, and adapter-only side effects.

Compile-capable commands below are plans only. When implementation is authorized, inspect active
`dotnet.exe` and `csc.exe` processes first and run one command at a time from the repository root:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  build .\src\Npc\Terraria.Npc.csproj `
  --no-restore -m:1 -nr:false `
  -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false

pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\Test\Terraria.Npc.Components.Verification\Terraria.Npc.Components.Verification.csproj `
  --no-build --no-restore -m:1 -nr:false `
  -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

If Town, WorldSession, or WorldProgression projects are changed, build the affected project one at
a time through the same wrapper. Each actual build must record the exact command, project, exit code,
warning/error counts, and artifact under `Build/bin`. No compile-capable command ran for this plan.

## 8. Rollback and completion criteria

Rollback a unit if a focused verifier finds duplicate writers, changed event order, changed packet
or save shape, stale boss references, cache leakage, or a query writing state. Restore the prior
single-writer adapter and preserve the failing fixture. Never restore safety by enabling old and new
writes simultaneously.

The P13 implementation is complete only when all 106 members have one verified owner or an explicit
definition/cache/projection classification, cross-domain ports are explicit, save/network adapters
are round-trip verified, affected projects build serially, focused tests pass, and `Build/bin`
contains the expected artifacts. This document makes none of those claims.

## 9.1 Completed checkpoint: C02 InvasionWaveProgressState

Implementation order for C02:

1. Add the proposed world progression owner and a value/definition seam for point and wave rules.
2. Route invasion kill, start, end, and reset inputs through explicit commands.
3. Move reads to `InvasionWaveQuery`; keep `ProgressionAggregate` and NPC entities out of the write path.
4. Replace the legacy writes with one commit call that updates points, kills, and wave atomically.
5. Keep save/network projections read-only and record the legacy compatibility mapping.

Focused verifier cases must cover reset to defaults, bounded or invalid point inputs, duplicate kill
commands, wave transitions, world restart, and projection round trips. Roll back if points and kills
can diverge, if a second aggregate writes the counters, or if event ordering changes. This checkpoint
is planned only; no implementation or verification ran.

## 9.2 Completed checkpoint: C03 BossEntityIndexRegistry

Implementation order for C03:

1. Identify the existing NPC slot, generation, entity identity, type, and lifecycle signals.
2. Define `BossKind` and a registry value that validates the identity against the current NPC slot.
3. Route boss spawn/replace/despawn/world-reset paths through one registry system.
4. Keep the four legacy static names as a read adapter until all readers use a validated query.
5. Add stale-slot, slot-reuse, wrong-type, duplicate-registration, and terminal-cleanup fixtures.

The adapter must not infer a stable entity from an integer after slot reuse. Roll back if a stale
registry entry can resolve to a new NPC, if two systems write the same boss slot, or if cleanup is
not atomic with NPC terminal state. This checkpoint is planned only; no C# or verifier run occurred.

## 9.3 Completed checkpoint: C04 NpcNetworkSyncIntent

Implementation order for C04:

1. Trace all Version4 writers of `netUpdate` and the current NLTX dirty-flag composition.
2. Define the entity-scoped intent and a single mark/acknowledge API.
3. Route authoritative NPC mutation results to that API, preserving coalescing and retry behavior.
4. Make the network scheduler consume the intent through a projection adapter; keep packet types out
   of the component and do not make save/load a writer.
5. Reset intent state on NPC slot reuse and verify a failed send remains observable.

Focused fixtures must cover repeated mutation before one send, successful acknowledgement, failed
send retry, entity reuse, client/server boundary, and no duplicate packet emission. Roll back if
`NpcReplicationDirtyState` and the new component can independently clear or set the same field, or if
the transport adapter mutates authority. This checkpoint is planned only; no C# or verifier run occurred.

## 9.4 Completed checkpoint: C05 TownRescueProgressionState

Implementation order for C05:

1. Define the typed rescue-key mapping and the world persistence version for the eight legacy flags.
2. Route rescue success through one command/commit owner and make repeat requests idempotent.
3. Migrate spawn eligibility and town-resident creation to a read-only rescue query.
4. Implement the save/network compatibility projection from the committed owner and remove legacy
   writes only after the focused verifier passes.
5. Verify world reset, old-save defaults, duplicate rescue input, and NPC slot reuse.

Rollback if a rescued NPC can write the world flag, if a reset leaves a stale flag, or if the old
static bridge and component both accept writes. This checkpoint is planned only; no source or
verification change occurred.

## 9.5 Completed checkpoint: C06 TownPetAdoptionProgressionState

Implementation order for C06:

1. Define `PetKind` and map each legacy flag to one typed state key.
2. Put item/player validation in an input adapter and send a typed adoption command.
3. Commit the adoption flag exactly once and publish the adoption event to pet spawning/dialogue.
4. Keep item consumption and entity spawn as explicit downstream commands with no flag write-back.
5. Add duplicate-command, failed-transaction, world-reset, save/load, and duplicate-spawn fixtures.

Rollback if an item adapter, pet entity, or legacy bridge can independently set a purchase flag, or
if retries make adoption non-idempotent. This checkpoint is planned only; no C# or verifier run occurred.

## 9.6 Completed checkpoint: C07 TownSpawnUnlockState

Implementation order for C07:

1. Define a stable typed unlock-key catalog and preserve the 16 legacy names in the adapter map.
2. Add one unlock commit system with idempotent commands and explicit world-reset behavior.
3. Migrate spawn eligibility to a read-only query that combines unlocks with current world/NPC rules.
4. Add versioned save/load and network projection fields without exposing mutable DTOs to systems.
5. Verify every key, old-save default, duplicate unlock, failed spawn, and catalog revision case.

Rollback if spawn code writes unlock state, an unlock is lost on a failed spawn, or the legacy adapter
and component both accept writes. This checkpoint is planned only; no implementation or verification ran.

## 9.7 Completed checkpoint: C08 NpcProgressionBookUsageState

Implementation order for C08:

1. Define typed book keys and a versioned save mapping for the three legacy booleans.
2. Route item-use validation through one command handler and commit a flag only after acceptance.
3. Publish the committed event to item effects and queries; keep those consumers read-only.
4. Preserve old static reads through an adapter and remove old writes after focused verification.
5. Test rejected/replayed/accepted item use, world reset, save/load, and no interaction with
   `ProgressionAggregate`.

Rollback if item effects or generic progression can set a book flag, or if command retry repeats a
   one-way effect. This checkpoint is planned only; no C# or verifier run occurred.

## 9.8 Completed checkpoint: C09 NpcActivePresenceCache

Implementation order for C09:

1. Identify the active-NPC scan owner and its type-key catalog.
2. Replace the raw static array with an owner-scoped workspace plus scan revision/invalidation.
3. Migrate active-presence readers to a read-only query and keep spawn/progression systems from
   writing the cache.
4. Explicitly exclude the cache from save/load and network projection DTOs.
5. Verify reset per scan, active spawn/despawn invalidation, stale-revision rejection, deterministic
   result ordering, and no cross-world leakage.

Rollback if a stale cache can authorize a spawn, if a query mutates the workspace, or if serialization
   treats it as durable authority. This checkpoint is planned only; no source or verifier run occurred.

## 9.9 Completed checkpoint: C10 LunarTowerEncounterState

Implementation order for C10:

1. Define typed tower keys and separate shield, active, and apocalypse phase values.
2. Route tower damage/activation/reset inputs to one event system and one commit owner.
3. Migrate tile/player/NPC readers to read-only tower queries and committed events.
4. Keep persistent `downedTower*` flags in the later defeat owner; do not use runtime state as a
   save substitute.
5. Verify shield transitions, active toggles, apocalypse entry/exit, world reset, duplicate damage,
   and runtime-versus-persistent projection separation.

Rollback if runtime shield or active state changes a defeat flag implicitly, or if two systems write
   the same tower state. This checkpoint is planned only; no source or verifier run occurred.

## 9.10 Completed checkpoint: C11 BossDefeatProgressionState

Implementation order for C11:

1. Define typed boss defeat keys and preserve the legacy flag-to-key mapping.
2. Route authoritative boss defeat events into one idempotent defeat commit system.
3. Keep boss entities, registry slots, achievements, spawn rules, and network projections read-only
   consumers of committed state.
4. Store `downedMechBossAny` under the same owner during migration and update it atomically with
   `downedMechBoss1..3`; do not add a derived second writer.
5. Verify every flag, world reset, old-save default, duplicate defeat, save/load, network projection,
   and stale active-boss event case.

Rollback if a defeated boss entity writes the flag directly, `downedMechBossAny` diverges from the
   approved policy, or a replayed defeat changes state twice. This checkpoint is planned only; no
   source or verifier run occurred.

## 9.11 Completed checkpoint: C12 EventDefeatProgressionState

Implementation order for C12:

1. Define typed event defeat keys and preserve the legacy flag-to-key mapping.
2. Route event-end transitions through one idempotent commit system.
3. Keep runtime invasion/seasonal/tower systems as command producers and query consumers only.
4. Migrate save/load and network projections to the event owner; do not merge tower runtime state
   with tower defeat persistence.
5. Verify every event flag, duplicate event-end, world reset, old-save default, tower separation,
   and projection round trip.

Rollback if an event runtime system writes persistent state directly, a tower shield change marks a
   tower defeated without an event commit, or replay changes a completed flag. This checkpoint is
   planned only; no source or verifier run occurred.

## 9.12 Completed checkpoint: C13 TownResidentState

Implementation order for C13:

1. Trace all `townNPC` readers/writers and compare them with the existing `TownResidentComponent`
   capability contract.
2. Create one entity-scoped resident owner/adapter and keep housing assignment separate.
3. Migrate resident queries and commands to the owner; retain legacy reads only through an adapter.
4. Do not implement an authoritative `nextDialogue` component until its read/write, reset,
   persistence, and network evidence is complete; keep the compatibility projection deferred.
5. Verify resident reset, entity reuse, rescue/spawn interaction, capability queries, and dialogue
   deferral behavior.

Rollback if resident classification can be written by both legacy and new paths, if housing changes
   resident identity implicitly, or if a guessed dialogue owner is introduced without evidence. This
   checkpoint is planned only; no source or verifier run occurred.

## 9.13 Completed checkpoint: C14 TravelNpcWorldState

Implementation order for C14:

1. Trace the world travel-NPC selector, entity spawn/despawn, and day/event reset paths.
2. Define one world role owner and an explicit relation to the spawned NPC identity.
3. Migrate role queries and lifecycle commands; do not mirror the role as a second entity authority.
4. Confirm save/network behavior and notification projection from the world owner.
5. Verify role uniqueness, despawn/reset cleanup, entity slot reuse, and reconnect behavior.

Rollback if world and NPC entity copies diverge, if a stale travel role survives entity cleanup, or if
network projection writes the marker. This checkpoint is planned only; no source or verifier run occurred.

## 9.14 Completed checkpoint: C15 TownHousingRelationState

Implementation order for C15:

1. Inventory all Town and WorldGeneration housing readers/writers and choose the integration owner.
2. Resolve `TownHousingResidentKey` as type, instance, or dual key with generation semantics.
3. Define one relation component plus a housing commit system; split rule constants, search timer,
   stable assignment, and old-value compatibility snapshot in the boundary contract.
4. Route room scanning through a read/query adapter and migrate legacy housing types to read-only
   compatibility adapters before removing duplicate writers.
5. Verify assignment revision, homeless transition, search timeout, old snapshot compatibility,
   room invalidation, NPC slot reuse, save/load, and network notification.

Rollback if Town and WorldGeneration can commit the same relation independently, if a reused NPC
slot inherits a prior home, or if a compatibility snapshot becomes a second source of truth. This
checkpoint is planned only; no source or verifier run occurred.

## 9.15 Completed checkpoint: C16 TownDoorInteractionIntent

Implementation order for C16:

1. Define a typed door-intent value with validated tile coordinates and an expiry/sequence.
2. Route NPC door requests through one intent owner and one door interaction system.
3. Keep tile mutation, audio, and network output behind adapters; consume or reject the intent exactly
   once and clear it on terminal result.
4. Reset the intent on NPC slot reuse and keep housing relation writes in the housing owner.
5. Verify repeated ticks, invalid/moved doors, changed tiles, rejection, expiry, slot reuse, and
   adapter-only side effects.

Rollback if a door adapter can reassert an intent, if repeated ticks cause duplicate effects, or if
door handling mutates housing/progression directly. This checkpoint is planned only; no source or
verifier run occurred.

## 9.16 Completed checkpoint: C17 NpcBreathState

Implementation order for C17:

1. Define `NpcBreathStateComponent` for `breath` and `breathCounter`, and keep `breathMax` in the
   immutable environment rule/definition boundary.
2. Route liquid/environment reads through a query port and let one breath system commit transitions.
3. Add explicit reset behavior for NPC initialization, environment changes, death/despawn, and slot
   reuse; keep queries and projections read-only.
4. Verify max bounds, counter cadence, environment transitions, reset, save/network compatibility,
   and no counter advancement from a projection.

Rollback if `breathMax` becomes mutable per-NPC authority without evidence, if two systems advance
the counter, or if slot reuse carries state across entities. The component source was saved before
verification and remains limited to deterministic state transitions; no health, environment, save,
network, or lifecycle side effect was added.

Verification command 1:

```powershell
$dotnetArgs = @('build', '.\\src\\Npc\\Terraria.Npc.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs
```

Result: exit code `0`; `0` warnings and `0` errors. The verified artifact is
`Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`.

Verification command 2:

```powershell
$dotnetArgs = @('run', '--project', '.\\Test\\Terraria.Npc.Components.Verification\\Terraria.Npc.Components.Verification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs
```

Result: exit code `0`; output `PASS: NPC and town component field composition`.

The strict reflection smoke loaded the verified DLL and exited `0` with
`PASS: NpcBreathStateComponent C17 boundary smoke`. It covered `BreathMax=200`, drowning cadence
`7`, recovery `3`, default `Breath=200` and `BreathCounter=0`, submerged cadence decrement,
non-submerged recovery and counter reset, lower/upper clamps, the zero-breath signal, invalid
breath/counter rejection, and reset. C17 is now complete for its isolated component boundary;
C03 and C04 remained pending at this earlier checkpoint. Save/network compatibility, environment caller integration, life damage
after breath reaches zero, liquid exception rules, scheduler placement, and behavior equivalence are
not verified.

## 9. Checkpoint log

| Checkpoint | Status | Implementation note |
|---|---|---|
| C01 `MoonLordEncounterDefinitionAndState` | complete for design ledger | definition/state split recorded; no C# change |
| C02 `InvasionWaveProgressState` | complete for design ledger | world-scoped atomic points/kills/wave owner recorded; no C# change |
| C03 `BossEntityIndexRegistry` | complete for design ledger | generation-aware registry and integration key decision recorded; no C# change |
| C04 `NpcNetworkSyncIntent` | complete for design ledger | entity intent separated from transport; dirty-state integration remains review item |
| C05 `TownRescueProgressionState` | complete for design ledger | eight world rescue flags and idempotent owner recorded; no C# change |
| C06 `TownPetAdoptionProgressionState` | complete for design ledger | typed adoption command and world owner recorded; no C# change |
| C07 `TownSpawnUnlockState` | complete for design ledger | typed world unlock set and read-only spawn query recorded; no C# change |
| C08 `NpcProgressionBookUsageState` | complete for design ledger | typed one-way book-use owner recorded; no C# change |
| C09 `NpcActivePresenceCache` | complete for design ledger | scan-owned non-durable cache and invalidation contract recorded; no C# change |
| C10 `LunarTowerEncounterState` | complete for design ledger | tower runtime state separated from defeat persistence; no C# change |
| C11 `BossDefeatProgressionState` | complete for design ledger | persistent boss flags and single-owner mech compatibility policy recorded; no C# change |
| C12 `EventDefeatProgressionState` | complete for design ledger | event and tower defeat persistence separated from runtime state; no C# change |
| C13 `TownResidentState` | complete for design ledger | `townNPC` entity boundary recorded; `nextDialogue` explicitly deferred |
| C14 `TravelNpcWorldState` | complete for design ledger | world role separated from NPC entity classification; no C# change |
| C15 `TownHousingRelationState` | complete for design ledger | relation/timer/rule/snapshot split recorded; duplicate housing owner requires integration review |
| C16 `TownDoorInteractionIntent` | complete for design ledger | short-lived consume-once door intent recorded; no C# change |
| C17 `NpcBreathState` | implemented and verified in isolation | `NpcBreathRuleDefinition` and `NpcBreathStateComponent` saved; affected-project build, focused verifier, and boundary smoke passed; integration remains open |

## 10. Implementation Checkpoint: C01 MoonLordEncounterDefinitionAndState

- Saved source files before advancing:
  - `src/WorldSession/NpcProgression/MoonLord/MoonLordEncounterDefinition.cs`
  - `src/WorldSession/NpcProgression/MoonLord/MoonLordEncounterStateComponent.cs`
- Core behavior: the definition copies the two attack arrays on construction and on read; the
  mutable encounter component stores only `MoonLordCountdown`, validates the configured upper bound,
  and resets it to zero. The source does not add an NPC copy of world encounter state.
- Dependency impact: only the `Terraria.WorldSession` project source set is changed. There is no
  WorldGen, item-use, save, network, registration, or legacy static writer integration in this
  checkpoint.
- Unverified items: Version4 attack-table initialization, all countdown callers, reset scheduling,
  item-triggered countdown behavior, save/network projection, and behavior equivalence.
- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. An isolated
  reflection smoke also exited `0` with defensive-array, countdown-bound, and reset assertions
  passing. No integrated focused verifier or runtime parity evidence exists.

## 11. Implementation Checkpoint: C02 InvasionWaveProgressState

- Saved source file before advancing:
  - `src/WorldSession/NpcProgression/Invasion/InvasionWaveProgressStateComponent.cs`
- Core behavior: the component commits points, kills, and wave number together. Points and kills are
  finite and non-negative; wave number is non-negative and monotonic until an explicit reset. The
  component validates all inputs before mutating any field and resets all values to zero.
- Dependency impact: only the `Terraria.WorldSession` project source set is changed. No invasion
  command, `ProgressionAggregate`, NPC entity, persistence adapter, network projection, or legacy
  writer was changed.
- Unverified items: Version4 point calculation, kill/start/end callers, duplicate command handling,
  save/network projection, scheduler order, and behavior equivalence.
- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. The isolated
  smoke exited `0` and verified atomic points/kills/wave commit, backwards-wave rejection without
  mutation, and reset. No integrated caller or projection evidence exists.

## 12. Implementation Checkpoint: C05 TownRescueProgressionState

- Saved source files before advancing:
  - `src/Town/Progression/Rescue/TownRescueKind.cs`
  - `src/Town/Progression/Rescue/TownRescueProgressStateComponent.cs`
- Core behavior: the typed rescue enum maps all eight Version4 flags to one state owner. The owner
  commits a rescue once, returns `false` for a repeated rescue, exposes read-only status, and resets
  all flags together.
- Dependency impact: only the `Terraria.Town` project source set is changed. No Version4 source,
  NPC spawn path, rescue event writer, WorldFile adapter, network projection, registration key, or
  other partition document was changed.
- Unverified items: legacy rescue writer replacement, spawn eligibility migration, persistence and
  network projection, scheduler placement, and behavior equivalence.
- Verification command: `$dotnetArgs = @('build', '.\\src\\Town\\Terraria.Town.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll` exists. The isolated smoke exited `0`
  and verified all eight typed rescue keys, first-write acceptance, duplicate-write rejection,
  readback, and reset. No integrated legacy writer or projection evidence exists.

## 13. Verification Checkpoint: C06 TownPetAdoptionProgressionState

- Verification command: `$dotnetArgs = @('build', '.\\src\\Town\\Terraria.Town.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll` exists. The isolated smoke exited `0`
  and verified Cat, Dog, and Bunny first adoption, duplicate rejection, readback, and reset. No
  integrated purchase, pet spawn, save/network, or legacy writer evidence exists.

## 13. Implementation Checkpoint: C06 TownPetAdoptionProgressionState

- Saved source files before advancing:
  - `src/Town/Progression/Pets/TownPetKind.cs`
  - `src/Town/Progression/Pets/TownPetAdoptionProgressStateComponent.cs`
- Core behavior: the typed pet enum maps the three Version4 purchase flags to one world state owner.
  The first adoption is accepted, a repeated adoption is rejected without a second transition, and
  reset clears all three flags.
- Dependency impact: only the `Terraria.Town` project source set is changed. No purchase adapter,
  pet entity/population path, WorldFile serialization, MessageBuffer input, network projection,
  registration key, or legacy writer was changed.
- Unverified items: item/player validation, duplicate command handling at the transaction boundary,
  pet spawn effects, save/network round trip, scheduler placement, and behavior equivalence.
- Verification: `not-run`; the affected Town project build and focused smoke are the next actions.

## 14. Implementation Checkpoint: C07 TownSpawnUnlockState

- Saved source files before verification:
  - `src/Town/Progression/Spawn/TownSpawnUnlockKind.cs`
  - `src/Town/Progression/Spawn/TownSpawnUnlockStateComponent.cs`
- Core behavior: `TownSpawnUnlockKind` provides typed keys for all sixteen Version4 spawn-unlock
  fields (`SlimeBlue`, `SlimeGreen`, `SlimeOld`, `SlimePurple`, `SlimeRainbow`, `SlimeRed`,
  `SlimeYellow`, `SlimeCopper`, `Merchant`, `Demolitionist`, `PartyGirl`, `DyeTrader`, `Truffle`,
  `ArmsDealer`, `Nurse`, and `Princess`). `TownSpawnUnlockStateComponent` owns the corresponding
  boolean facts, accepts each unlock once, returns `false` for a repeated unlock, exposes read-only
  status through `IsUnlocked`, and clears all sixteen values through `Reset`.
- Dependency impact: only the `Terraria.Town` project source set is extended. No existing NPC spawn
  writer, `NpcSpawnEligibilityQuery`, WorldFile projection, network projection, registration key, or
  scheduler integration was changed, and no second owner was introduced.
- Evidence boundary: the source does not claim that failed spawn attempts, NPC definitions, legacy
  boolean compatibility views, save/load, network output, or world reset callers are wired. The
  component remains a world-scoped state owner; query and adapter integration require separate
  evidence.
- Verification status: affected-project build and isolated smoke are pending; the component is still
  `currentComponent: C07` and remains in `pendingComponents` until both are recorded.

## 15. Verification Checkpoint: C07 TownSpawnUnlockState

- Verification command: `$dotnetArgs = @('build', '.\\src\\Town\\Terraria.Town.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll` exists. The isolated reflection smoke
  exited `0` and verified all sixteen typed unlock keys, initial locked state, first-write acceptance,
  duplicate rejection, readback, invalid-key rejection, and reset. Spawn-query behavior, legacy
  writer replacement, save/network projection, scheduler placement, and behavior equivalence remain
  unverified.

## 16. Implementation Checkpoint: C08 NpcProgressionBookUsageState

- Saved source files before verification:
  - `src/WorldSession/NpcProgression/Books/NpcProgressionBookKind.cs`
  - `src/WorldSession/NpcProgression/Books/NpcProgressionBookUsageStateComponent.cs`
- Core behavior: `NpcProgressionBookKind` provides typed keys for `CombatBook`,
  `CombatBookVolumeTwo`, and `PeddlersSatchel`. `NpcProgressionBookUsageStateComponent` owns the
  corresponding three one-way usage facts, accepts each first `MarkUsed` transition, returns `false`
  for a repeated transition, exposes read-only status through `IsUsed`, and clears all three values
  through `Reset`.
- Dependency impact: only the existing `Terraria.WorldSession` project source set is extended. The
  old `NpcWorldUnlockFlags` aggregate, item-use path, item effects, save projection, network
  projection, registration key, and `ProgressionAggregate` remain unchanged; no second writer was
  added.
- Evidence boundary: item validation and command handling, committed usage events, old static read
  compatibility, versioned save/load, network output, world reset callers, and scheduler integration
  are not wired by this state component.
- Verification status: affected-project build and isolated smoke are pending; the component is still
  `currentComponent: C08` and remains in `pendingComponents` until both are recorded.

## 17. Verification Checkpoint: C08 NpcProgressionBookUsageState

- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. The isolated
  reflection smoke exited `0` and verified all three typed book keys, initial unused state,
  first-use acceptance, duplicate rejection, readback, invalid-key rejection, and reset. Item
  command validation, legacy writer replacement, save/network projection, scheduler placement, and
  behavior equivalence remain unverified.

## 18. Implementation Checkpoint: C09 NpcActivePresenceCache

- Saved source file before verification:
  - `src/Npc/Queries/NpcActivePresenceCache.cs`
- Core behavior: `NpcActivePresenceCache` owns a scan-local `bool[]` keyed by the Version4 integer
  NPC type range. `BeginScan` requires a strictly increasing scan revision, clears the previous
  workspace, and marks the new revision valid. `TryMarkActive` accepts only current-revision and
  in-range type indices; `TryGetActive` provides a read-only point query; and
  `TryGetActiveNpcTypes` returns a new ascending snapshot. `Invalidate` clears the workspace and
  makes current results unavailable.
- Dependency impact: only the existing `Terraria.Npc` project source set is extended. No NPC
  lifecycle writer, spawn/progression writer, save projection, network projection, registration key,
  or scheduler integration was changed; the cache is intentionally not a durable world component.
- Evidence boundary: the active-population scan caller, NPC spawn/despawn invalidation hooks, exact
  cross-world ownership, save/network exclusion wiring, and runtime scheduler placement remain
  unverified. The cache API rejects stale revisions but does not itself discover active NPC entities.
- Verification status: affected-project build and isolated smoke are pending; the component is still
  `currentComponent: C09` and remains in `pendingComponents` until both are recorded.

## 19. Verification Checkpoint: C09 NpcActivePresenceCache

- Verification command: `$dotnetArgs = @('build', '.\\src\\Npc\\Terraria.Npc.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll` exists. The isolated reflection smoke exited
  `0` and verified per-scan clearing, strictly increasing revisions, current-revision marking and
  reading, ascending type snapshot, invalidation, stale-revision rejection, and type-range bounds.
  The active scan caller, lifecycle invalidation hooks, save/network exclusion, scheduler placement,
  and behavior equivalence remain unverified.

## 20. Implementation Checkpoint: C10 LunarTowerEncounterState

- Saved source files before verification:
  - `src/WorldSession/NpcProgression/LunarTower/LunarTowerKind.cs`
  - `src/WorldSession/NpcProgression/LunarTower/LunarTowerEncounterStateComponent.cs`
- Core behavior: `LunarTowerKind` provides typed keys for Solar, Vortex, Nebula, and Stardust.
  `LunarTowerEncounterStateComponent` owns the four runtime shield values, the immutable normal
  shield rule, four runtime active flags, and `LunarApocalypseIsUp`. Shield updates are bounded to
  `0..LunarShieldPowerNormal`; `ApplyShieldDamage` clamps at zero; active and apocalypse transitions
  are explicit; and `Reset` clears runtime encounter state. Persistent `downedTower*` flags are not
  represented or inferred here.
- Dependency impact: only the existing `Terraria.WorldSession` project source set is extended. No
  `WorldGen`, `Projectile`, NPC defeat writer, `LunarProgressState`, save projection, network
  projection, registration key, or scheduler integration was changed; no second runtime/defeat owner
  was added.
- Evidence boundary: Version4 `ShieldStrengthTowerMax` halves the normal value after
  `downedMoonlord`, but the C11 owner is not implemented, so this isolated component intentionally
  exposes only the confirmed normal shield bound. WorldGen activation/update, projectile damage,
  NPC defeat transitions, network/save projections, and event scheduling remain unwired.
- Verification evidence: the serial `Terraria.WorldSession` build exited `0` with `0` warnings and
  `0` errors; `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists.
  The isolated smoke exited `0` and verified all four typed tower keys, shield bounds and damage,
  active flags, apocalypse state, invalid input rejection, and reset. WorldGen activation/update,
  projectile damage, NPC defeat transitions, network/save projections, event scheduling, and
  behavior equivalence remain unverified. The checkpoint is complete for the isolated component.

## 21. Verification Checkpoint: C10 LunarTowerEncounterState

- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. The isolated
  smoke exited `0` and verified all four tower keys, shield bounds and damage, active flags,
  apocalypse state, invalid input rejection, and reset. WorldGen/Projectile integration, the
  dynamic `downedMoonlord` policy, save/network projection, scheduler placement, and behavior
  equivalence remain unverified.

## 22. Implementation Checkpoint: C11 BossDefeatProgressionState

- Saved source files before verification:
  - `src/WorldSession/NpcProgression/Boss/BossDefeatProgressionKind.cs`
  - `src/WorldSession/NpcProgression/Boss/BossDefeatProgressionStateComponent.cs`
- Core behavior: `BossDefeatProgressionKind` provides typed keys for the 17 authoritative boss
  defeat fields. `BossDefeatProgressionStateComponent` owns one explicit boolean for each field,
  accepts each first `MarkDefeated` transition, returns `false` for a repeated transition, exposes
  read-only status through `IsDefeated`, and clears all flags through `Reset`. The compatibility
  field `DownedMechBossAny` is stored by the same owner and is set in the same transition as each
  `DownedMechBoss1..3` flag; no independent derived writer was added.
- Dependency impact: only the existing `Terraria.WorldSession` project source set is extended. No
  NPC defeat event writer, active-boss registry, achievement/spawn reader, WorldFile projection,
  network projection, registration key, or scheduler integration was changed.
- Evidence boundary: authoritative defeat event routing, stale active-boss event rejection,
  versioned old-save default/load, save/network round trip, legacy static compatibility, and
  behavior equivalence remain unverified. This checkpoint intentionally implements the isolated
  state owner and its typed access boundary only.
- Verification status: build and isolated smoke are pending; C11 remains `currentComponent: C11`
  and in `pendingComponents` until both are recorded.

## 23. Verification Checkpoint: C11 BossDefeatProgressionState

- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. The isolated
  reflection smoke exited `0` and verified all 17 typed boss keys, initial false state, first defeat
  acceptance, duplicate rejection, readback, atomic `DownedMechBossAny` updates with each concrete
  mechanical boss flag, invalid-key rejection, and reset. The existing `Terraria.Npc.Components.Verification`
  project does not reference `Terraria.WorldSession`, so it was not used for this component. Defeat
  event routing, stale active-boss rejection, old-save/load compatibility, save/network projection,
  legacy static compatibility, scheduler placement, and behavior equivalence remain unverified.

## 24. Implementation Checkpoint: C12 EventDefeatProgressionState

- Saved source files before verification:
  - `src/WorldSession/NpcProgression/Events/EventDefeatProgressionKind.cs`
  - `src/WorldSession/NpcProgression/Events/EventDefeatProgressionStateComponent.cs`
- Core behavior: `EventDefeatProgressionKind` provides typed keys for all 14 event defeat fields.
  `EventDefeatProgressionStateComponent` owns one explicit boolean for each event fact, accepts each
  first `MarkDefeated` transition, returns `false` for a repeated transition, exposes read-only
  status through `IsDefeated`, and clears all flags through `Reset`. `DownedTowerSolar`,
  `DownedTowerVortex`, `DownedTowerNebula`, and `DownedTowerStardust` are persistent event facts
  here and are intentionally distinct from C10 runtime shield and active values.
- Dependency impact: only the existing `Terraria.WorldSession` project source set is extended. No
  invasion, seasonal, Martian, or tower runtime writer, old `InvasionHistoryStateComponent`,
  WorldFile projection, network projection, registration key, or scheduler integration was changed.
- Evidence boundary: authoritative event-end routing, old-save default/load, save/network round trip,
  legacy static compatibility, runtime-to-persistence adapter behavior, and behavior equivalence remain
  unverified. This checkpoint implements only the isolated event progression owner and typed access
  boundary.
- Verification status: build and isolated smoke are pending; C12 remains `currentComponent: C12`
  and in `pendingComponents` until both are recorded.

## 25. Verification Checkpoint: C12 EventDefeatProgressionState

- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. The corrected
  strict-error isolated smoke exited `0` and verified all 14 event keys, initial false state, first
  event-end acceptance, duplicate rejection, readback, tower defeat persistence remaining separate
  from C10 shield/active runtime state, invalid-key rejection, and reset. Event-end routing,
  old-save/load compatibility, save/network round trip, legacy static compatibility, scheduler
  placement, and behavior equivalence remain unverified.

## 26. Implementation Checkpoint: C13 TownResidentState

- Saved source file before verification:
  - `src/Town/Residents/TownResidentStateComponent.cs`
- Core behavior: `TownResidentStateComponent` is an entity-scoped resident classification owner.
  `IsTownResident` starts false, `SetResident` applies the explicit classification transition, and
  `Reset` clears the value for entity reuse. Housing assignment, rescue progression, friendly and
  capability values remain separate state boundaries.
- Dependency impact: only the existing `Terraria.Town` project source set is extended. The partial
  `TownResidentComponent`, `NpcHousingAssignmentComponent`, rescue/spawn progression owners,
  dialogue types, WorldFile projection, network projection, registration key, and scheduler were
  not changed; no second resident writer was introduced.
- Evidence boundary: the current `nextDialogue` declaration is `Terraria.GameContent.ConditionalDialogue`,
  but its readers, writers, reset behavior, persistence, and network contract are not evidenced.
  It remains a deferred `TownDialogueCompatibilityProjection`; no authoritative dialogue component
  was guessed. Legacy `townNPC` adapters, resident query/command integration, entity-reuse hooks,
  save/network round trip, and behavior equivalence remain unverified.
- Verification status: build and isolated smoke are pending; C13 remains `currentComponent: C13`
  and in `pendingComponents` until both are recorded.

## 27. Verification Checkpoint: C13 TownResidentState

- Verification command: `$dotnetArgs = @('build', '.\\src\\Town\\Terraria.Town.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`, followed by the existing verifier through the same wrapper with `run --project .\\Test\\Terraria.Npc.Components.Verification\\Terraria.Npc.Components.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Verification result: `Terraria.Town` build exited `0` with `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll` exists. The existing NPC/Town verifier
  executed and exited `0` with `PASS: NPC and town component field composition`. The C13 isolated
  smoke exited `0` and verified resident default false, explicit true/false transitions, and reset
  for entity reuse. `nextDialogue`, legacy `townNPC` adapter integration, housing/spawn interaction,
  save/network projection, scheduler placement, and behavior equivalence remain unverified.

## 28. Implementation Checkpoint: C14 TravelNpcWorldState

- Saved source file before verification:
  - `src/WorldSession/Town/TravelNpcWorldStateComponent.cs`
- Core behavior: `TravelNpcWorldStateComponent` is a world-level runtime marker owner for the
  traveling NPC role. `IsTravelNpcActive` starts false, `SetTravelNpcActive` applies an explicit
  lifecycle transition, and `Reset` clears the role. The marker is not copied into every NPC entity.
- Dependency impact: only the existing `Terraria.WorldSession` project source set is extended. No
  travel-NPC selector, spawned-entity identity relation, despawn/day-event writer, notification
  projection, WorldFile projection, network projection, registration key, or scheduler integration
  was changed.
- Evidence boundary: Version4 call-path evidence for selector ownership, entity spawn/despawn,
  stale-role cleanup, save/network persistence, reconnect behavior, and behavior equivalence remains
  open. This checkpoint implements only the isolated world role owner and explicit reset boundary.
- Verification status: build and isolated smoke are pending; C14 remains `currentComponent: C14`
  and in `pendingComponents` until both are recorded.

## 29. Verification Checkpoint: C14 TravelNpcWorldState

- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Verification result: exit code `0`; `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` exists. The strict
  isolated smoke exited `0` and verified travel role default inactive, explicit activation and
  deactivation, and world reset cleanup. Selector ownership, spawned-entity relation, stale-role
  cleanup, save/network persistence, reconnect behavior, scheduler placement, and behavior
  equivalence remain unverified.

## 30. Implementation Checkpoint: C15 TownHousingRelationState

- Saved source files before verification:
  - `src/Town/Housing/TownHousingRuleDefinition.cs`
  - `src/Town/Housing/TownHousingRelationStateComponent.cs`
- Core behavior: `TownHousingRuleDefinition` preserves the confirmed immutable
  `KickOutLookForHomeTimeout` value of `3600`. `TownHousingRelationStateComponent` owns the typed
  NPC housing relation: homeless state, homeless-despawn policy, non-negative home-search timeout,
  optional `TownRoomTilePoint` assignment, housing category, and transition compatibility snapshot.
  `CommitRelation` validates the homeless/home invariant, captures the previous relation atomically
  within the same owner, and applies the new relation. `Reset` clears relation and snapshot state.
- Dependency impact: only the existing `Terraria.Town` project source set is extended. Existing
  `TownHousingRelationComponent`, `NpcHousingAssignmentComponent`, WorldGeneration housing registry
  and key types, room scan code, save/network projection, registration key, and scheduler were not
  changed; no cross-domain writer or second relation owner was added.
- Evidence boundary: `TownHousingResidentKey` type/instance/dual mode, assignment revision semantics,
  Town versus WorldGeneration commit ownership, room query adapter, legacy coordinate sentinel shape,
  housingCategory integration with resident capabilities, save/load, network notifications, slot
  reuse, and behavior equivalence remain unresolved. This checkpoint intentionally implements the
  relation boundary and confirmed rule value without selecting the cross-domain owner.
- Verification status: build and isolated smoke are pending; C15 remains `currentComponent: C15`
  and in `pendingComponents` until both are recorded.

## 31. Verification Checkpoint: C15 TownHousingRelationState

- Verification command: `$dotnetArgs = @('build', '.\\src\\Town\\Terraria.Town.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`, followed by the existing verifier through the same wrapper with `run --project .\\Test\\Terraria.Npc.Components.Verification\\Terraria.Npc.Components.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Verification result: `Terraria.Town` build exited `0` with `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll` exists. The existing NPC/Town verifier
  executed and exited `0` with `PASS: NPC and town component field composition`. The C15 isolated
  smoke exited `0` and verified rule value `3600`, assigned and homeless relation transitions,
  compatibility snapshot capture, negative-timeout and homeless-with-home rejection, and reset.
  Cross-domain key mode, assignment revision, Town/WorldGeneration commit ownership, room adapter,
  save/load, network notification, slot reuse, scheduler placement, and behavior equivalence remain
  unverified.

## 32. Implementation Checkpoint: C16 TownDoorInteractionIntent

- Saved source files before verification:
  - `src/Town/Doors/TownDoorInteractionIntent.cs`
  - `src/Town/Doors/TownDoorInteractionIntentComponent.cs`
- Core behavior: `TownDoorInteractionIntent` is a typed value containing the requested close/open
  action, non-negative tile coordinates, a monotonic sequence, and a non-negative expiry tick.
  `TownDoorInteractionIntentComponent` stores at most one pending intent, exposes read-only legacy
  field views, rejects invalid coordinates/ticks, consumes a pending intent once through
  `TryConsume`, and clears it through expiry, rejection, or `Reset`. Tile mutation, audio, network,
  and housing effects are not performed by the component.
- Dependency impact: only the existing `Terraria.Town` project source set is extended. No tile/door
  adapter, movement schedule, housing relation, NPC lifecycle writer, network projection, registration
  key, or scheduler integration was changed; no external side-effect writer was added.
- Evidence boundary: Version4 tile validity, moved-door detection, terminal result events, exact expiry
  scheduling, slot-reuse caller, adapter side effects, save/network behavior, and behavior equivalence
  remain unverified. This checkpoint implements only the explicit short-lived intent owner.
- Verification status: build and isolated smoke are pending; C16 remains `currentComponent: C16`
  and in `pendingComponents` until both are recorded.

## 33. Verification Checkpoint: C16 TownDoorInteractionIntent

- Verification command: `$dotnetArgs = @('build', '.\\src\\Town\\Terraria.Town.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`, followed by the existing verifier through the same wrapper with `run --project .\\Test\\Terraria.Npc.Components.Verification\\Terraria.Npc.Components.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Verification result: `Terraria.Town` build exited `0` with `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll` exists. The existing NPC/Town verifier
  executed and exited `0` with `PASS: NPC and town component field composition`. The corrected strict
  isolated smoke exited `0` and verified coordinates, monotonic sequence, field readback,
  consume-once behavior, expiry, rejection, invalid input, and reset. Tile validity, moved-door
  handling, terminal result events, slot-reuse caller, adapter-only effects, save/network behavior,
  scheduler placement, and behavior equivalence remain unverified.

## 34. Implementation Checkpoint: C17 NpcBreathState

- Saved source files before verification:
  - `src/Npc/Environment/NpcBreathRuleDefinition.cs`
  - `src/Npc/Environment/NpcBreathStateComponent.cs`
- Core behavior: `NpcBreathRuleDefinition` preserves the confirmed immutable `BreathMax` value of
  `200`, drowning cadence of `7`, and recovery step of `3`. `NpcBreathStateComponent` defaults to
  `Breath=200` and `BreathCounter=0`, validates the bounded state, advances the counter only while
  submerged, decrements breath at the confirmed cadence, recovers and resets the counter when not
  submerged, clamps breath to `0..200`, reports when breath reaches zero, and resets for entity
  initialization/death/despawn/slot reuse. Health damage, environment queries, logging, and network
  effects remain outside the component.
- Dependency impact: only the existing `Terraria.Npc` project source set is extended. No NPC health,
  lifecycle, liquid/environment query, network projection, save projection, registration key, or
  scheduler integration was changed; no side-effect writer was added.
- Evidence boundary: exact Version4 environment caller, life-damage/defeat handling after breath reaches
  zero, liquid exception rules, save/network compatibility, scheduler placement, and behavior
  equivalence remain unverified. This checkpoint implements only the confirmed breath state/rule
  boundary.
- Verification status: the affected-project build, existing focused verifier, and isolated C17 smoke
  passed. The build used the serial wrapper and exited `0` with `0` warnings and `0` errors; the
  artifact is `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`. The focused verifier exited
  `0` with `PASS: NPC and town component field composition`. The strict reflection smoke exited
  `0` with `PASS: NpcBreathStateComponent C17 boundary smoke`, covering definition constants,
  default state, cadence, recovery, bounds, zero signal, invalid inputs, and reset. C17 is now in
  `completedComponents`; at this earlier checkpoint, C03 and C04 remained pending. Environment caller integration, life-damage
  handling after breath reaches zero, liquid exception rules, save/network compatibility, scheduler
  placement, and behavior equivalence remain unverified.

## 35. Current implementation boundary

C03 source and its isolated affected-project verification are complete. The isolated build used a
temporary exclusion for only the unrelated `WorldGeneration/Systems/HellChestLootCycleSystem.cs:16`
call to a component method with the wrong arity; the temporary targets file was deleted afterward.
The C03 registry carries `NpcInstanceId`, legacy `NpcSlot`, positive generation, and `NpcTypeId`;
its boundary smoke covers invalid identities, wrong Version4 boss types, duplicate registration,
stale replacement/clear requests, inactive resolution, and slot-reuse generation mismatches. C04
source, its affected-project build, focused verifier, and boundary smoke are also complete. Runtime
lifecycle callers, scheduler, transport acknowledgement/retry, client/server authority,
persistence/network projection, and behavior equivalence remain evidence-gaps; no C04 integration
caller is being inferred from the component boundary alone.

## 36. Implementation Checkpoint: C03 BossEntityIndexRegistry

- Saved source files before verification:
  - `src/WorldSession/NpcProgression/Boss/BossKind.cs`
  - `src/WorldSession/NpcProgression/Boss/BossEntityIndexKey.cs`
  - `src/WorldSession/NpcProgression/Boss/BossEntityIndexRegistryComponent.cs`
  - `src/WorldSession/Terraria.WorldSession.csproj`
- Core behavior: `BossEntityIndexKey` requires a valid `NpcInstanceId`, assigned legacy
  `NpcSlot`, positive slot generation, and valid `NpcTypeId`. The registry accepts one entry per
  `BossKind`, rejects duplicate registration, permits replacement only when the expected current
  generation-aware key matches, rejects wrong Version4 NPC types (Golem 245, Plant 262, Crimson
  266, Deerclops 668), rejects inactive or stale resolution, and clears only an exact current key.
  `GolemBoss`, `PlantBoss`, `CrimsonBoss`, and `DeerclopsBoss` are read-only legacy slot views and
  return `-1` when unregistered.
- Dependency impact: `Terraria.WorldSession` now references the existing `Terraria.Npc` project
  for the established identity, slot, and type value types. No Version4 runtime lifecycle caller,
  NPC spawn/despawn writer, save projection, network projection, registration key, or other
  partition document was changed.
- Verification command: `$dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false', '-p:CustomAfterMicrosoftCommonTargets=D:\\TRbackup\\NLTX\\Build\\Tools\\P13-WorldSession-Verification-Exclusions.targets'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`. The temporary exclusion removed only the unrelated `WorldGeneration/Systems/HellChestLootCycleSystem.cs` source and was deleted after verification.
- Verification status: the isolated affected-project build exited `0` with `0` warnings and `0`
  errors after excluding only the unrelated `HellChestLootCycleSystem.cs` source; the artifact is
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`. The existing NPC/Town
  focused verifier exited `0` with `PASS: NPC and town component field composition`. The C03
  boundary smoke exited `0` with `PASS: BossEntityIndexRegistry C03 boundary smoke` and covered
  registration, duplicate rejection, active resolution, wrong type, inactive resolution, slot
  reuse, stale clear, current clear, and legacy slot cleanup. The complete WorldSession build still
  cannot be claimed because the excluded external source remains unresolved. C03 is now in
  `completedComponents`; at this earlier checkpoint, C04 was the current pending component.

## 37. Implementation Checkpoint: C04 NpcNetworkSyncIntent

- Saved source files before verification:
  - `src/Npc/Network/NpcNetworkSyncIntentComponent.cs`
  - `src/Npc/NpcReplicationDirtyState.cs`
- Core behavior: `NpcNetworkSyncIntentComponent` owns the pending synchronization intent and a
  monotonic revision. Repeated `Mark` calls coalesce while a send is pending; `Acknowledge` clears
  only the exact current revision; `Retry` leaves a failed send observable; and
  `ResetForEntityReuse` clears pending and acknowledgement state without reusing the revision
  sequence. `NpcReplicationDirtyState` now derives `StateDirty` from that component and exposes
  `MarkStateChanged`, `AcknowledgeStateChanged`, `RetryStateChanged`, and
  `ResetForEntityReuse`. Its existing `Flags` setter remains a compatibility adapter over the same
  owner, so no second dirty bit is introduced.
- Dependency impact: only the existing `Terraria.Npc` project source set is extended. No packet
  DTO, transport handle, client cursor, save writer, network scheduler, registration key, or
  authority boundary was added; existing `SpawnNeedsSync`, `ForceFullSync`, `RemovalNeedsSync`,
  client state, and stream cursor fields remain outside the new intent owner.
- Evidence boundary: the local component, dirty-state composition, affected-project build, focused
  verifier, and boundary smoke passed. The Version4 scheduler caller, successful-send
  acknowledgement path, failed-send retry path, entity lifecycle reset caller, client/server
  authority, duplicate packet suppression, and behavior equivalence remain unverified.

## 38. Verification Checkpoint: C04 NpcNetworkSyncIntent

- Verification command: `$dotnetArgs = @('build', '.\\src\\Npc\\Terraria.Npc.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`, followed by the existing verifier through the same wrapper with `run --project .\\Test\\Terraria.Npc.Components.Verification\\Terraria.Npc.Components.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Verification result: `Terraria.Npc` build exited `0` with `0` warnings and `0` errors; artifact
  `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll` exists. The existing focused verifier
  exited `0` with `PASS: NPC and town component field composition`. The strict C04 boundary smoke
  exited `0` with `PASS: NpcNetworkSyncIntent C04 boundary smoke`, covering coalesced marks,
  revision matching, stale and duplicate acknowledgement rejection, retry visibility, entity reuse
  reset, compatibility `Flags` projection, and single-owner dirty-state cleanup. Runtime scheduler,
  transport, persistence/network projection, and behavior equivalence remain unverified. C04 is now
  in `completedComponents`; no pending component remains in this isolated P13 implementation slice.

## 42. Final P13 implementation revalidation

- Revalidation command: `$dotnetArgs = @('build', '.\\src\\Npc\\Terraria.Npc.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`, followed by `$dotnetArgs = @('run', '--project', '.\\Test\\Terraria.Npc.Components.Verification\\Terraria.Npc.Components.Verification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`.
- Revalidation result: the `Terraria.Npc` build exited `0` with `0` warnings and `0` errors; the
  artifact is `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`. The focused verifier ran
  with `--no-build --no-restore`, exited `0`, and printed `PASS: NPC and town component field
  composition`. This confirms the current NPC component composition only; it does not close the
  P13 runtime scheduler, lifecycle, transport, persistence/network, or behavior-equivalence gaps.
- Full WorldSession revalidation result: the serial `Terraria.WorldSession` build was also attempted
  through the wrapper with the same repository build properties. It exited `1` with `0` warnings
  and `1` error at `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs:16`:
  `TryAdvanceAfterSuccessfulPlacement` has no overload accepting one argument. This source is
  outside P13 and was not changed; no fresh full-WorldSession artifact is claimed from the failed
  build. The previously recorded isolated C03 artifact remains valid only for the temporary
  exclusion boundary described in Section 39.

## 43. Implementation Checkpoint: C09 Active-Presence Scan Owner

- Saved source files before verification:
  - `src/Npc/Queries/NpcActivePresenceScanEntry.cs`
  - `src/Npc/Queries/NpcActivePresenceScanSystem.cs`
- Core behavior: `NpcActivePresenceScanEntry` makes scan input explicit. `NpcActivePresenceScanSystem.Rebuild`
  begins a strictly increasing scan revision, clears the previous cache through the existing cache
  owner, and marks only entries with `IsActive=true`. Invalid type values are rejected by
  `NpcActivePresenceCache`; stale revisions remain unreadable. The system is pure with respect to
  external effects: it does not access `Main`, time, randomness, network, persistence, entity
  creation, or unrelated state.
- Dependency impact: only the existing `Terraria.Npc` project source set is extended. No real
  `Main.npc` caller, scheduler, lifecycle invalidation hook, registration key, save projection, or
  network projection was changed.
- Evidence boundary: Version4 `NPC.cs:7119-7142` supports the clear-and-rebuild active/type-range
  semantics. The runtime caller, scheduler placement, cross-world ownership, and behavior
  equivalence remain unverified.
- Component state: C09 is recorded in `completedComponents`; `currentComponent: none` and
  `pendingComponents: []` remain unchanged. The focused verifier has not run yet, so
  `verificationStatus: partial` is retained.

## 44. Verification Checkpoint: C09 Active-Presence Scan Owner

- Verification commands, each executed serially through
  `Build/Tools/Invoke-SerialDotnet.ps1`:
  - `restore .\src\NpcC09FocusedVerifier\Terraria.NpcC09FocusedVerifier.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0`.
  - `build .\src\NpcC09FocusedVerifier\Terraria.NpcC09FocusedVerifier.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0` with `0` warnings and `0` errors.
  - `run --project .\src\NpcC09FocusedVerifier\Terraria.NpcC09FocusedVerifier.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0` and printed `PASS: NpcActivePresenceScanSystem C09 focused verifier`.
- Artifact: `Build/bin/Terraria.NpcC09FocusedVerifier/Debug/net10.0/Terraria.NpcC09FocusedVerifier.dll`.
- Coverage: active entries at revisions 10 and 11, inactive filtering, duplicate type
  deduplication, ascending snapshots, invalid negative/capacity types, replacement clearing,
  stale revision rejection, invalidation, and non-increasing revision rejection.
- The initial no-restore build attempt exited `1` with `NETSDK1004` because the new verifier had no
  assets file; the serialized restore above resolved that bootstrap condition. This is recorded as
  a tooling bootstrap event and not as a C09 source failure.
- Verification boundary: the isolated scan owner is verified. The real `Main.npc` caller, runtime
  scheduler placement, lifecycle invalidation hooks, cross-world ownership, and behavior
  equivalence remain unverified, so overall `verificationStatus: partial` is retained.

## 45. Affected Project Verification: C09

- Command, executed serially through `Build/Tools/Invoke-SerialDotnet.ps1`:
  `build .\src\Npc\Terraria.Npc.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Result: exit code `0`, `0` warnings, `0` errors. Artifact:
  `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`.
- This confirms the C09 cache and scan-owner source files compile as part of the affected NPC
  project. The runtime caller, scheduler placement, lifecycle invalidation hooks, cross-world
  ownership, and behavior equivalence remain unverified.

## 46. Existing NPC/Town Regression Verification

- Command, executed serially through `Build/Tools/Invoke-SerialDotnet.ps1`:
  `run --project .\Test\Terraria.Npc.Components.Verification\Terraria.Npc.Components.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- Result: exit code `0`; output `PASS: NPC and town component field composition`.
- Scope: existing identity, definition, behavior, lifetime, target, parent relation, replication,
  resident capability, and housing assignment composition. This is regression evidence only; it
  does not close the P13 runtime caller, persistence, network, or behavior-equivalence gaps.

## 47. Integrated P13 focused verifier source checkpoint

- Saved source files:
  - `src/NpcTownProgressionP13FocusedVerifier/Program.cs`
  - `src/NpcTownProgressionP13FocusedVerifier/Terraria.NpcTownProgressionP13FocusedVerifier.csproj`
- Scope: the verifier exercises the saved C01-C17 component boundaries across the existing NPC,
  Town, and WorldSession source owners, including field readback, reset behavior, monotonic or
  duplicate-operation guards, housing transitions, door-intent consumption, active-presence scan,
  shield damage, and breath cadence. It covers the 105 members with confirmed source owners;
  `nextDialogue` remains a deferred projection because its reader, writer, reset, persistence, and
  network evidence is still missing.
- Dependency impact: this is a source-only focused verifier under `src/`; it introduces no runtime
  registration, scheduler, persistence, network, or production behavior change. Its project file
  explicitly compiles the WorldSession owner files so verification can remain bounded while the
  unrelated `HellChestLootCycleSystem.cs` error is unresolved.
- Execution status: source is saved, but restore/build/run have not yet been executed for this
  verifier. `verificationStatus: partial` is therefore retained. No integrated verifier result or
  full P13 behavior-equivalence claim is made by this checkpoint.

## 48. Integrated P13 focused verifier verification

- Commands, each executed serially through `Build/Tools/Invoke-SerialDotnet.ps1`:
  - `restore .\src\NpcTownProgressionP13FocusedVerifier\Terraria.NpcTownProgressionP13FocusedVerifier.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0`.
  - `build .\src\NpcTownProgressionP13FocusedVerifier\Terraria.NpcTownProgressionP13FocusedVerifier.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0` with `0` warnings and `0` errors.
  - `run --project .\src\NpcTownProgressionP13FocusedVerifier\Terraria.NpcTownProgressionP13FocusedVerifier.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` exited `0` and printed `PASS: P13 NPC/Town progression focused verifier (105 implemented members; nextDialogue deferred)`.
- The first no-restore build before verifier API correction exited `1` with `0` warnings and two
  verifier-only errors (`CS8117` for matching a `void` return and `CS0246` for the missing
  `Terraria.Town` using). The verifier was corrected from the actual component declarations and the
  serial build above then passed. No production component source was changed for this correction.
- Artifact: `Build/bin/Terraria.NpcTownProgressionP13FocusedVerifier/Debug/net10.0/Terraria.NpcTownProgressionP13FocusedVerifier.dll`.
- Coverage: C01-C17 boundary assertions for the 105 members with confirmed owners, including field
  readback, reset, duplicate-operation rejection, monotonic revisions, housing transitions,
  door-intent consume/expiry, active-presence scan replacement, tower shield state, and breath
  cadence. `nextDialogue` is explicitly reported as deferred.
- Verification status: the integrated focused verifier is verified at its bounded source-owner
  boundary. Overall `verificationStatus: partial` remains correct because Version4 runtime callers,
  scheduler placement, lifecycle wiring, persistence/network adapters, and behavior equivalence are
  still outside the available evidence.

## 49. Final P13 session verification record

- Session metadata is now bound to the active manual claim: `partitionId=P13` and
  `sessionId=06c2332691e84e2682405500e43d7be8`. No active `dotnet.exe` or `csc.exe` process was
  present before the compile-capable commands.
- The affected-project commands were executed serially through
  `Build/Tools/Invoke-SerialDotnet.ps1` with `-m:1 -nr:false -p:UseSharedCompilation=false
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false`:
  - `build .\src\Npc\Terraria.Npc.csproj --no-restore` exited `0` with `0` warnings and `0`
    errors. Artifact: `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`.
  - `build .\src\Town\Terraria.Town.csproj --no-restore` exited `0` with `0` warnings and `0`
    errors. Artifact: `Build/bin/Terraria.Town/Debug/net10.0/Terraria.Town.dll`.
  - `build .\src\WorldSession\Terraria.WorldSession.csproj --no-restore` exited `1` with `0`
    warnings and `1` error at `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs:16`:
    `TryAdvanceAfterSuccessfulPlacement` has no one-argument overload. This external source was not
    modified, and no complete WorldSession artifact is claimed from the failed build.
- The integrated verifier command
  `build .\src\NpcTownProgressionP13FocusedVerifier\Terraria.NpcTownProgressionP13FocusedVerifier.csproj
  --no-restore` exited `0` with `0` warnings and `0` errors. Artifact:
  `Build/bin/Terraria.NpcTownProgressionP13FocusedVerifier/Debug/net10.0/Terraria.NpcTownProgressionP13FocusedVerifier.dll`.
- The verifier command
  `run --project .\src\NpcTownProgressionP13FocusedVerifier\Terraria.NpcTownProgressionP13FocusedVerifier.csproj
  --no-build --no-restore` exited `0` and printed
  `PASS: P13 NPC/Town progression focused verifier (105 implemented members; nextDialogue deferred)`.
- The 105 confirmed source-owner members are therefore covered by the bounded verifier. The
  `nextDialogue` member remains deferred because its complete reader, writer, reset, persistence,
  and network evidence is unavailable; runtime scheduling, lifecycle, persistence, network, and
  behavior-equivalence evidence remain open. `implementationStatus: in-progress` and
  `verificationStatus: partial` remain intentional.
