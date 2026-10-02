# P13 NPC, Town, Event, Boss, and World Progression System Execution Plan

```yaml
partitionId: P13
taskId: AUTH-SYS-P13
designDocument: D:\\TRbackup\\NLTX\\docs\\plans\\system-decomposition\\2026-09-30-p13-npc-town-progression-system-design.md
sourceReport: D:\\TRbackup\\NLTX\\docs\\system-decomposition\\reports\\2026-09-18-system-decomposition-authoritative-P13-npc-town-progression.md
originalSessionId: 90f3766e5cf742bea166c28a864ded62
executionStatus: partial-core-slice
verificationStatus: focused-passed-10-percent
sourceModified: false
targetModified: true
testsRun: true
migrationStatus: not-claimed
```

## Execution Boundary

This is the implementation sequence derived from the P13 System design. It now includes a
focused execution record for the first-clear progression commit seam. The current checkout
contains partial data components and a verifier under `src/NSSLC/Component`; there is still no
evidence that the proposed Systems are registered in a runtime or that their legacy callers have
been routed.

The execution must remain one owner per invariant. A successful compilation, an isolated verifier,
or a callable compatibility facade is not a migration-success signal. A slice may advance only
after its required Observation tuple is captured against the real migration entry point.

## Inputs and Evidence Freeze

Before implementation, record and compare these immutable inputs:

1. P13 authoritative report with 106/106 members and its original session ID.
2. Version4 source hashes from the design document.
3. Complete-reference source hashes from `D:\\TRbackup\\无任何删减通过编译`.
4. CPG manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`.
5. Current NLTX component inventory under `D:\\TRbackup\\NLTX\\src\\NSSLC`.
6. The exact runtime branch and world/session identity model selected by integration review.

The read-only query evidence to preserve in the implementation issue is:

| Evidence check | Expected static result | Meaning |
|---|---|---|
| `SetEventFlagCleared` call sites | 26 | one shared first-clear seam; route all callers before deleting a writer |
| `UpdateHomeTileState` call sites | 8 | housing entry points are concentrated in NPC, but room ownership remains external |
| `CheckDrowning` call sites | 1 | breath is reached from NPC update; effects need explicit ports |
| `downedMoonlord` member uses | 6 | WorldGen and WorldFile confirmed writes plus partial/unknown reads |
| `nextDialogue` member uses | 0 plus `NoMatchingFactInScannedScope` | keep as unknown compatibility behavior |
| `StartImpendingDoom` call sites | 2 | countdown transition has two indexed static callers; spawn/chat/network effects remain outside the owner |
| `UpdateLunarApocalypse` call sites | 5 across two indexed records | tower scan and impending-doom transition are coupled; exact runtime dispatch remains partial |
| `SyncAnInvasion` call sites | 1 | message 78 projection has a confirmed selected caller, not a complete transport proof |
| `UpdateTime` records/call sites | 8 records; 2 selected `Main.cs` calls | countdown and travel/NPC time consumers share the legacy method name; phase ordering remains unknown |

## Ordered Execution Phases

### Phase 0: Integration and identity gate

**Objective:** establish the scope in which every later component is valid.

**Work:**

- Define a world/session context and prove static Version4 fields are isolated per world.
- Choose distinct types for NPC slot, runtime entity ID, persistent entity ID, generation, and
  network ID.
- Define the boss registry key and the lifecycle event that invalidates it.
- Decide whether shared boss/event/progression state belongs to `WorldProgression` or
  `NpcProgression`.

**Gate:** no P13 writer is enabled until the identity and progression-owner decisions are recorded.
Unresolved items remain `blocked: integration-review`, not guessed.

### Phase 1: Component and projection seams

**Objective:** make data and external boundaries explicit without routing behavior yet.

**Work:**

- Reconcile existing `MoonLord`, invasion, tower, boss/event, town, housing, travel, breath,
  active-scan, and replication components with the design ownership table.
- Define immutable save snapshots and network views.
- Define effect ports for spawn/despawn, life damage/strike, chat, item/projectile effects,
  WorldGen actions, TownManager assignment, and network acknowledgement/retry.
- Ensure projections cannot mutate authority and adapters do not dual-write legacy fields.

**Gate:** every authoritative member has one proposed writer, a reset owner, a persistence rule,
and a lifecycle scope. Missing values stay `unknown`.

### Phase 2: First-clear progression commit

**Objective:** route the shared event/boss first-clear behavior through one transaction seam.

**Work:**

- Implement a `MarkBossDefeated`/`MarkEventDefeated` command boundary returning a first-clear
  result and rejection reason.
- Map every confirmed `SetEventFlagCleared` caller before enabling the new writer.
- Preserve LanternNight, credits, hardmode, meteor, dual-dungeon, loot, and chat ordering through
  effect ports identified from the complete reference.
- Keep `downedMechBossAny` derived only after its persistence/network contract is resolved; no
  compatibility dual-write.

**Gate:** first-clear, duplicate, reset, re-entrant death, and effect-order scenarios have real
  entry-point evidence. Until then the System remains shadow-only.

### Phase 3: Encounter, invasion, and tower transitions

**Objective:** migrate world runtime tuples while preserving their commit boundaries.

**Work:**

- Route Moon Lord countdown start, tick, zero-crossing spawn, reset, and message 103 projection.
- Route Pumpkin Moon and Snow Moon start/stop, points/kills/wave tuple, and message 78 projection.
- Route tower shield damage, active scans, apocalypse recomputation, and messages 101/103.
- Keep WorldGen and Main as adapters until all callers are observed at the new commit barrier.

**Gate:** countdown zero crossing spawns once; invasion stop is idempotent; tower disappearance
  and impending doom match the source trace; save/network snapshots are ordered after commit.

### Phase 4: Boss registry and NPC replication intent

**Objective:** separate entity identity from legacy index slots and packet intent.

**Work:**

- Replace raw boss slot assumptions with generation-aware registry keys.
- Route `CheckBossIndexes` behavior through `TryResolve` and `ClearIfInactiveOrMismatched`.
- Route `netUpdate`, low-priority update, acknowledgement, retry, and entity-reuse reset through
  replication intent APIs.
- Integrate server authority and packet scheduling only after the transport contract is traced.

**Gate:** inactive/type-mismatch clear, slot reuse, generation mismatch, duplicate ack, retry,
  and entity reuse scenarios are covered by an actual network/lifecycle harness.

### Phase 5: Town rescue, adoption, spawn unlocks, and books

**Objective:** migrate monotonic world facts and the effectful pet command.

**Work:**

- Route `AI_007_TownEntities_UpdateSavedStates` facts through rescue commands.
- Route pet license input to `AdoptPet`, preserving first-use chat/network effects and repeat
  reroll success/failure.
- Route sixteen spawn unlocks and three book/satchel flags through idempotent commands and
  eligibility Queries.
- Add WorldFile and NetMessage projections only after the owner state is committed.

**Gate:** first-use, duplicate, reset, old-save load, network replay, and spawn-eligibility cases
  produce the expected state delta and effect order.

### Phase 6: Active presence, housing, and travel

**Objective:** migrate revision-scoped scans and relation state without confusing room data with
  world progression.

**Work:**

- Route active NPC scans through increasing revisions and invalidate on world unload.
- Commit housing current/old relation atomically; query TownManager through an adapter.
- Preserve changed-only message 60 publication.
- Resolve `travelNPC` owner between calendar/time and NPC spawn/despawn before routing its writes.

**Gate:** scan revision replacement, room assignment failure, homeless snapshots, timeout/door
  intent, TownManager persistence, travel despawn, and multi-world separation are evidenced.

### Phase 7: Breath and effect ports

**Objective:** extract deterministic breath state while keeping environment and life effects outside
the component.

**Work:**

- Route `CheckDrowning` input through collision and wetness adapters.
- Preserve cadence 7, recovery +3 capped at 200, zero-breath damage 2, life floor/strike behavior,
  low-priority sync, and visual dust order from the complete reference.
- Make life damage and replication effects explicit and retry-aware.

**Gate:** submerged/dry cadence, recovery cap, lethal damage guard, liquid exceptions, sync, and
  visual effect scenarios have complete Observation records.

### Phase 8: Dialogue and legacy writer removal

**Objective:** close the unresolved compatibility seam and only then remove legacy writers.

**Work:**

- Find the reader, writer, reset, serialization, and network behavior for `nextDialogue`.
- If the contract cannot be found, keep a compatibility adapter and record the unresolved edge.
- Compare legacy and new observations at a commit barrier.
- Remove old writers only after every P13 member has one observed new owner and rollback state is
  available.

**Gate:** no `migration-success` label is allowed until all required behaviors and cross-partition
  seams pass the real behavior suite.

## API and Error Contract

Every public command used in the implementation must define:

- input identity and world/session scope;
- precondition and validation error;
- whether duplicate input is a no-op, retry, or rejection;
- exact authoritative delta and commit point;
- emitted effect requests and their order;
- projection visibility and acknowledgement behavior;
- reset, unload, and entity-reuse behavior.

Queries must be repeatable for the same committed snapshot. They may not lazily initialize caches,
write compatibility fields, consume queues, or invoke effect ports. If a read has hidden mutation,
classify it as a Command and return to owner analysis.

## Focused Core Slice Record

The first implementation slice is intentionally limited to deterministic state commits that the
design assigns to `NpcProgressionCommitSystem`, `MoonLordEncounterSystem`,
`InvasionWaveProgressSystem`, and `LunarTowerEncounterSystem`:

- `NpcProgressionCommitSystem.MarkBossDefeated` validates the enum, commits one boss flag, and
  returns `Committed`, `AlreadyCleared`, or `RejectedUnknownKind`.
- `NpcProgressionCommitSystem.MarkEventDefeated` applies the same contract to event and tower
  flags.
- `Reset` clears both progression components. No network, chat, WorldGen, save, achievement,
  spawn, or item effect is emitted by this seam.
- `MoonLordEncounterSystem` starts natural/item countdowns, ticks once, returns a single
  server-authoritative zero-crossing spawn request, exposes an immutable sync view, and resets.
- `InvasionWaveProgressSystem` starts/stops the wave tuple, commits points/kills/wave atomically,
  rejects backward waves through the component invariant, and exposes an immutable sync view.
- `LunarTowerEncounterSystem` starts the four-tower state, clamps shield damage, deactivates
  missing towers from an explicit presence snapshot, and returns an impending-doom request without
  invoking WorldGen, network, chat, or spawn effects.
- The focused verifier is
  `src/NSSLC/Component/NpcProgressionVerification/Program.cs` and covers first clear, duplicate,
  aggregate `DownedMechBossAny`, invalid input, reset, countdown zero crossing, invasion tuple
  idempotency/reset, and tower shield/apocalypse transitions.

The complete reference tree `D:\TRbackup\无任何删减通过编译` was used to confirm that the legacy
first-clear path also performs external effects. Those effects, the 26 static
`SetEventFlagCleared` call sites, and their real entry-point routing remain outside this slice.

### Build and verifier evidence

The repository serial wrapper and the SDK selected by `global.json` were used. The local SDK
directory was prepended only for these commands; `global.json` was not changed.

```powershell
$env:PATH = 'C:\Users\shan\AppData\Local\Codex\dotnet-sdk-10.0.400;' + $env:PATH
$project = Join-Path (Join-Path (Join-Path 'src' 'NSSLC') 'Component') 'NpcProgressionVerification\Terraria.NpcProgressionVerification.csproj'
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @(
  'build', $project, '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
```

- Build exit code: `0`.
- Build diagnostics: `0` warnings, `0` errors.
- Verified artifact: `Build/bin/Terraria.NpcProgressionVerification/Debug/net10.0/Terraria.NpcProgressionVerification.dll`.

The verifier was then run without rebuilding or restoring:

```powershell
$env:PATH = 'C:\Users\shan\AppData\Local\Codex\dotnet-sdk-10.0.400;' + $env:PATH
$project = Join-Path (Join-Path (Join-Path 'src' 'NSSLC') 'Component') 'NpcProgressionVerification\Terraria.NpcProgressionVerification.csproj'
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @(
  'run', '--project', $project, '--no-build', '--no-restore')
```

- Verifier exit code: `0`.
- Verifier output: `PASS: P13 core progression/encounter smoke (10% scope)`.
- The focused run is not a full P13 test result and does not establish runtime integration,
  behavior equivalence, or migration success.

## Verification Matrix

The full matrix remains not-run. Only the focused core slice above has a recorded execution.

| Slice | Required scenarios | Evidence required |
|---|---|---|
| First-clear progression | first clear, duplicate, reset, re-entrant death, effect ordering | real death/event entry plus Observation tuple |
| Moon Lord countdown | natural/item start, decrement, zero crossing, restart, save/load, message 103 | spawn-once and persistence/network traces |
| Invasion | start/stop, wave transitions, reset, sync 78, duplicate commands | atomic tuple and packet order |
| Lunar towers | shield damage, active disappearance, apocalypse start, messages 101/103 | world effect and tower flag trace |
| Boss registry | type validation, generation/slot reuse, inactive clear, spawn decode | identity and lifecycle trace |
| Replication | mark, throttle, ack, retry, duplicate ack, reuse reset | server/client transport trace |
| Town progression | rescue/adoption/unlock/book idempotency, reset, save/load, network replay | command result and effect order |
| Active scan | revision replacement, invalidation, consumer visibility | query-only cache trace |
| Housing/travel | room assignment, homeless snapshots, timeout, door intent, travel expiry | TownManager and calendar trace |
| Breath | cadence, recovery, life floor/strike, liquid exceptions, low-priority sync | environment and life-effect trace |
| Dialogue | reader/writer/reset/projection discovery | source-backed contract or explicit deferred adapter |
| Cross-slice | commit order, retry, multi-world isolation, compatibility reads | full integration Observation diff |

The repository build contract governs any further compile or test action. Full P13 build, runtime,
network, save, lifecycle, retry, and cross-world verification remain not-run.

## Rollback and Failure Handling

- Keep legacy writers available behind a compatibility adapter until the new System owns the full
  invariant and the observation diff is accepted.
- Shadow mode may compare new projections with legacy output, but it must not publish two state
  authorities or allow unordered effects.
- On a failed slice, disable the new route at the last commit barrier, discard only uncommitted
  new state, and restore the legacy adapter path.
- Preserve save schema versions and packet compatibility while the new owner is shadow-only.
- A missing caller, lifecycle path, scheduler phase, or effect port blocks the slice; it does not
  justify a guessed owner.

## Completion Criteria

An isolated slice may be recorded as `partial-core-slice` before the integration gates only when
its scope, command contract, build evidence, and verification output are explicit. The execution
document may move to an implementation-complete status only when:

1. all 106 members have one observed owner or an explicitly approved deferred adapter;
2. every required legacy entry point reaches the proposed composition;
3. save/load, network, lifecycle, error, retry, and multi-world behavior are covered;
4. the full Observation tuple matches the accepted legacy trace;
5. old writers are removed only after the deletion gate is approved.

Until those conditions hold, the status remains `executionStatus: not-started` or an explicitly
recorded partial stage, `verificationStatus: not-run` or the measured result, and
`migrationStatus: not-claimed`.

## Complete-reference cross-check for implementation planning

The supplementary tree `D:\TRbackup\无任何删减通过编译` was read after the report was produced to
close the behavior questions that the cropped Version4 bodies cannot answer. The following facts
are inputs to the later implementation phases, not claims about current NLTX runtime behavior:

| Source span | Planning fact | Required target boundary |
|---|---|---|
| `Terraria/Main.cs:65862-65868` | Tick decrements the countdown and requests spawn only at zero on the server. | Return a `SpawnRequest` from the encounter System; do not spawn from the component or query. |
| `Terraria/WorldGen.cs:88195-88267` | Missing towers are deactivated; the last-tower transition clears apocalypse and starts a 3600-tick countdown, then publishes 103/chat/cultist effects. | Keep `RecomputeApocalypse` deterministic and route effects through ports; leave scheduler, network, and chat `partial`. |
| `Terraria/Main.cs:11121-11185` | Stop resets the invasion tuple; Pumpkin/Snow start sets tuple to `(0, 0, 1)` on the server. | Commit points, kills, and wave atomically and expose explicit reset/start commands. |
| `Terraria/NPC.cs:79749-79775` | Threshold crossing advances the wave and publishes progress/message 78 after the tuple changes. | Keep threshold lookup, achievements, chat, and message 78 outside the tuple owner until their callers are traced. |
| `Terraria/NPC.cs:80549-80571` | Tower defeat writes the defeat flag, clears active state, recomputes apocalypse, and publishes the lunar message in order. | Do not enable separate tower writers without an ordered progression commit port. |

The focused implementation verification below covers only deterministic target-side state and
transition behavior. No runtime registration, network, save/load, lifecycle, retry, cross-world,
or behavior-equivalence verification was performed; the result remains a 10% target-side smoke
result and does not change `migrationStatus: not-claimed`.

## Current Execution Record

This pass preserved the authoritative `Version4` tree and the complete reference tree unchanged.
It added deterministic target-side core seams for progression, countdown, invasion, and lunar
tower state plus their focused verifier under `src/NSSLC`, then built only
`Terraria.NpcProgressionVerification.csproj` and ran that verifier with `--no-build --no-restore`.
The result is the focused `10%` smoke pass recorded above. The 106-member P13 scope, real legacy
entry points, effect ordering, save/network projections, lifecycle and retry behavior,
multi-world isolation, and unresolved `nextDialogue` contract remain open or not-run. Therefore
`migrationStatus` stays `not-claimed`; this record does not call the migration successful.
