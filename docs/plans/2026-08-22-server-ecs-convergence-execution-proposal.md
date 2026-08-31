# Server ECS Convergence Execution Proposal

> implement this plan task-by-task.

**Goal:** Converge the server-authoritative ECS migration by restoring
reproducible loopback behavior, closing source-backed Main/WLD state boundaries,
and making legacy WorldGen and physical-file deletion removal evidence-driven.

**Architecture:** `Terraria.Dome.Simulation` owns gameplay state transitions by
immutable snapshots, typed commands, named tick phases, and deterministic
commits. `Terraria.Dome.Server` owns session lifecycle, persistence
orchestration, and ordered outbound projection. WLD parsing/projection stays in
`WorldFile.V319` and `WorldCompatibility`; V1456 packet code stays an adapter.
Mixed legacy responsibilities are split by owner. Deleted or ambiguous source
behavior remains explicitly deferred.

**Tech Stack:** .NET 10, C#, Arch 2.1.0, immutable records, typed command
buffers, V1456 protocol projection, WLD V319 reader, Version4 source oracle,
and standalone verifier projects.

---

## Baseline and Scope

### Measured baseline

- `Terraria.Dome.Simulation` and `Terraria.Dome.Server` Release builds pass
  serially with zero warnings and zero errors.
- The completion verifier reports `100` manifest rows and a weighted core score
  of `92/100`. The score is evidence coverage, not a semantic-completion claim.
- WorldClock, WorldFile V319, WorldImport, WorldRules, PlayerAuthority,
  WorldObjects, WorldRules loopback, and MainBoundary verifiers pass from fresh
  processes. The main initializer gate reports `rows=59` and
  `invalidStatusRows=0`.
- WorldGen remains intentionally blocked: the complete differential compares
  `5,040,000` tiles and currently mismatches all of them; the deletion gate is
  `canRemoveLegacyWorldGen=false`. The oracle inventory is 684 methods, 233
  fields, 125 partial methods, and 559 unmapped methods.
- The Version3-to-Version4 deletion ledger contains 535 rows. Classification
  is complete, but 55 `ServerRelevant` rows remain deferred, so physical
  deletion safety is not accepted.

### Work already closed before this proposal

The following source-backed boundaries are already implemented and must not be
reopened as speculative rewrites: fractional WLD time, raw rain metadata,
fail-closed endless-rain repair, pre-advance time-rate resolution, invasion
travel `max(dayRate, 1)` semantics, bounded bootstrap outbound queuing, chest
stale-revision protection, and typed ping replacement evidence. Main-boundary
and initializer-ledger normalization are also complete. Future work starts
from these contracts and adds only new evidence.

### Current blockers

1. WorldGen lacks recovered RNG/options/pass-order/runtime contracts and full
   differential parity.
2. TileEntity TrainingDummy has a bounded typed ownership, activation,
   deactivation, NPC linkage, persistence, V1456 projection, and server-owned
   tile-interaction loopback chain. Complete qualification still lacks a
   source-backed authorization contract for raw inbound `87` and a client
   mutation loopback for that separate path.
3. Fifty-five server-relevant physical deletions remain deferred.

These blockers are release gates, not permission to infer behavior from names,
bounded tests, or the reduced Version4 tree.

### Non-goals

- Do not put UI, graphics, audio, RGB, local input, asset loading, or social
  presentation into Simulation.
- Do not introduce `Simulation.InitializeAlmostEverything`, `Queue<Action>`, a
  generic `IEnumerator` scheduler, a duplicate `WorldTile`, or fallback values
  for unknown legacy behavior.
- Do not remove the legacy WorldGen source/oracle or claim parity solely because
  a bounded verifier passes.
- Do not create commits from the shared dirty worktree. Each task uses scoped
  diffs and evidence artifacts; a future clean integration boundary owns commits.

### Rules for every task

1. Execute from `D:\TRbackup\NLTX`.
2. Use `-p:UseSharedCompilation=false -p:MSBuildNodeReuse=false` for every
   serial `dotnet` command.
3. Before editing C#, read `AGENTS.md`, the local Google C# constraint, the
   exact Version4 member, and the current target type.
4. Use `apply_patch`; preserve unrelated dirty files.
5. Create a fresh artifact under
   `Build/diagnostics/server-ecs-convergence/<task>/<run-id>/` containing source
   hash/lines, scenario, commands, exit codes, and accepted/deferred conclusion.
6. A status becomes `accepted` only after focused RED/GREEN evidence, affected
   project build, required persistence/protocol verification, and
   `git diff --check` all succeed.
7. An absent or ambiguous source rule is `deferred`; it is never defaulted.

## Task 1: Freeze a Build and Failure Baseline

**Files:**

- Read: `src/Terraria.Dome.Simulation/WorldGeneration/CoatingColorSelectionQuery.cs`
- Read: `src/Terraria.Dome.Simulation/World/WorldTile.cs`
- Read: `src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj`
- Read: `Build/full-regression-20260819-rerun.log`
- Create: `Build/diagnostics/server-ecs-convergence/P0-build/<run-id>/baseline.md`
- Create: `Build/diagnostics/server-ecs-convergence/P0-build/<run-id>/git-status.txt`

**Step 1: Capture ownership.** Run `git status --short` and a scoped diff for
the coating query, WorldTile, and Simulation project. Record whether the files
are modified, deleted, or untracked before attributing a failure to this work.

**Step 2: Build the authoritative boundary.**

```powershell
dotnet build src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj `
  -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false --nologo
```

Expected: exit `0`; record warning/error count. A failure outside the active
write set blocks the next task; do not duplicate or alias a missing type.

**Step 3: Build direct consumers.**

```powershell
dotnet build Test\Terraria.Dome.WorldClock.Verification\Terraria.Dome.WorldClock.Verification.csproj `
  -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false --nologo
dotnet build Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj `
  -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false --nologo
```

Expected: both exit `0`.

**Step 4: Add the narrow regression only if required.** The WorldGeneration
verifier should use the actual `WorldModel.WorldTile`; assert a null input
returns the default coating result and each real coating flag projects to the
correct block/wall selection. No local surrogate is permitted.

**Step 5: Verify.** Run the changed verifier and `git diff --check`; place all
output in `baseline.md`.

## Task 2: Make V1456 Join Completion an Atomic Server Transaction

**Source anchors:** Version4 `MessageBuffer.cs` join/bootstrap handling and
`NetMessage.cs` initial state projection. Current
`TerrariaProtocolSessionHost.HandleAsync` writes WorldData, initial sections,
initial NPCs, HostStatus, greeting modules, and completion as separate steps.

**Files:**

- Modify: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs`
- Modify: `src/Terraria.Dome.Server/DomeServer.cs`
- Modify: `src/Terraria.Dome.Server/Replication/SessionReplicationState.cs`
- Modify: `src/Terraria.Dome.Server/Replication/PlayerBootstrapProjection.cs`
- Modify: `src/Terraria.Dome.Server/Replication/PlayerStateProjection.cs`
- Test: `Test/Terraria.Dome.PlayerAuthority.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldRules.Loopback.Verification/Program.cs`
- Create: `Build/diagnostics/server-ecs-convergence/P1-join/<run-id>/trace.md`

**Step 1: Write a failing frame-order test.** Capture the first join stream and
require this order:

```text
hello response + initial modules
-> WorldData
-> initial sections and NPCs
-> complete player projection
-> HostStatus
-> join greeting modules
-> FinishedConnecting
-> incremental replication
```

Each required frame must occur once, before `FinishedConnecting`, and be owned
by the joining session.

**Step 2: Reproduce the failures.**

```powershell
dotnet run --project Test\Terraria.Dome.PlayerAuthority.Verification\Terraria.Dome.PlayerAuthority.Verification.csproj -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
dotnet run --project Test\Terraria.Dome.WorldRules.Loopback.Verification\Terraria.Dome.WorldRules.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

Expected before the fix: missing player-authority frames or missing HostStatus
is reproduced and saved in the trace.

**Step 3: Implement one immutable bootstrap payload.** It contains profile,
active state, controls, life/mana, buffs, inventory/loadout, and equipment. It
must be emitted before HostStatus and not reuse incremental cursors.

**Step 4: Activate only after the complete write succeeds.**
`SessionReplicationState.MarkActive()` happens after completion frames are
written. A failed write tears down the session and leaves no active cursor.

**Step 5: Validate.** Re-run both verifiers plus
`Terraria.Dome.Load.Verification`. All must exit `0`; otherwise classify any
transport reset rather than retrying it as a generic timing issue.

## Task 3: Repair Player Physics and Lifecycle Projection as One Tick Contract

**Source anchors:** Version4 `Player.cs` movement, `KillMe`, dead, and respawn
paths; Version4 `MessageBuffer.cs` player-control and life-state routes.

**Files:**

- Modify: `src/Terraria.Dome.Simulation/Player/Systems/PlayerGravitySystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Physics/Systems/TileCollisionSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Players/PlayerLifecycleSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Modify: `src/Terraria.Dome.Simulation/Tick/SimulationTickSchedule.cs`
- Modify: `src/Terraria.Dome.Server/Replication/PlayerStateProjection.cs`
- Test: `Test/Terraria.Dome.Verification/Program.cs`
- Test: `Test/Terraria.Dome.PlayerLifecycle.Loopback.Verification/Program.cs`

**Step 1: Add two RED cases.** First: an upward-moving player receives gravity,
contacts a flat floor, has grounded velocity/state, and stays grounded on the
next tick. Second: a two-session scenario requires server-owned damage, death,
respawn, and ordered state visibility to owner and observer.

**Step 2: Reproduce the currently recorded errors.** Run the two listed
verifiers; retain the gravity and missing death/respawn diagnostics.

**Step 3: Enforce this single schedule:**

```text
input application
-> gravity and velocity integration
-> tile collision / grounded resolution
-> damage resolution
-> death or respawn commit
-> player snapshot revision
-> server projection
```

Simulation never writes packets. A lifecycle transition creates one observable
revision; the observer must not see a death without respawn or vice versa.

**Step 4: Validate.** Run both verifiers and PlayerAuthority. Also assert that
a foreign session cannot mutate the owner lifecycle through a packet.

## Task 4: Repair Combat PVS Cursor and Tombstone Delivery

**Source anchors:** Version4 `NPC.cs`/`Projectile.cs` lifecycle and active-slot
semantics; Version4 `NetMessage.cs` `SyncNPC`, projectile, and kill routes.

**Files:**

- Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcBehaviorSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileLifetimeSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Combat/Systems/DamageResolutionSystem.cs`
- Modify: `src/Terraria.Dome.Server/Replication/CombatReplicationCursor.cs`
- Modify: `src/Terraria.Dome.Server/Replication/CombatReplicationAssembler.cs`
- Modify: `src/Terraria.Dome.Server/Replication/SessionReplicationState.cs`
- Test: `Test/Terraria.Dome.Combat.Protocol.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Combat.Loopback.Verification/Program.cs`

**Step 1: Write the complete observer-lifecycle RED test.** For an observer in
PVS, require ordered NPC spawn/update/health/death and projectile
spawn/update/kill. A tombstone cannot be sent before the first visible state;
cursors may advance only after the session accepts the frame.

**Step 2: Reproduce.** Run Combat Loopback and retain its incomplete-lifecycle
diagnostic.

**Step 3: Make visibility explicit.** The cursor must distinguish `Unseen`,
`VisibleAtRevision`, and `TombstonedAtRevision`. Unseen -> active snapshot;
visible -> changed snapshot; visible -> tombstone. Preserve enough state across
PVS exit to avoid stale send or skipped re-entry bootstrap.

**Step 4: Validate.** Run Combat Protocol then Combat Loopback. Both must pass
from fresh processes and record a trace with entity ID, revision, PVS decision,
and frame order.

## Task 5: Complete Container, Sign, and Tile-Entity Authority

**Source anchors:** Version4 `Chest.cs`, `Sign.cs`, `MessageBuffer.cs:31`, and
`NetMessage.cs:32`. This is the only partial eight-point capability family.

**Files:**

- Modify: `src/Terraria.Dome.Simulation/WorldObjects/Chest/Systems/ChestOpenSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldObjects/Chest/Systems/ChestMutationCommitSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldObjects/SignComponent.cs`
- Modify: `src/Terraria.Dome.Server/Validation/ContainerInteractionValidator.cs`
- Modify: `src/Terraria.Dome.Server/Replication/ChestReplicationCursor.cs`
- Modify: `src/Terraria.Dome.Server/Replication/SignReplicationCursor.cs`
- Test: `Test/Terraria.Dome.WorldObjects.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldObjects.Loopback.Verification/Program.cs`

**Step 1: Add conflict RED cases.** Cover simultaneous open, stale revision,
out-of-range transfer, sign ownership, disconnect cleanup, and reconnect
bootstrap. Invalid commands leave snapshot revision unchanged.

**Step 2: Implement one ownership transaction.** Validate player handle, range,
object identity, current opener, expected revision, and transfer result before
commit. A successful commit increments one authority revision then projects full
bootstrap or delta by session.

**Step 3: Validate and score.** Run both WorldObjects verifiers. Only then may
the G family become `evidenced` in the completion manifest; do not change other
family scores.

## Task 6: Establish WLD Time and Rain as Lossless Input Boundaries

**Source anchors:**

- `Terraria.IO/WorldFile.cs:1312-1313` writes `_tempTime` as `Double`.
- `Terraria.IO/WorldFile.cs:2125-2126` reads `_tempTime` as `Double`.
- `Terraria.IO/WorldFile.cs:3675-3690` reads `raining`, `rainTime`, and
  `maxRaining` independently.
- `Terraria.IO/WorldFile.cs:3355-3383` repairs endless rain only with version
  and secret-seed conditions.
- `Main.cs:12958-13046` moves invasion by `max(dayRate, 1)`.
- `Main.cs:13325-13337` derives `maxRaining` from a separate random rule.

**Files:**

- Modify: `src/Terraria.Dome.Simulation/World/WorldClock.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/DomeSimulationSnapshot.cs`
- Modify: `src/Terraria.Dome.Simulation/World/WorldRuleState.cs`
- Modify: `src/Terraria.Dome.Simulation/World/Systems/WorldWeatherSystem.cs`
- Modify: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`
- Modify: `src/Terraria.Dome.Server/Persistence/WorldPersistenceFormat.cs`
- Modify: `src/Terraria.WorldFile.V319/Model/LegacyWorldMetadata.cs`
- Modify: `src/Terraria.WorldCompatibility/Model/CompatibilityWorldMetadata.cs`
- Modify: `src/Terraria.WorldCompatibility/Projection/CompatibilityToDomeProjection.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Test: `Test/Terraria.Dome.WorldClock.Verification/Program.cs`
- Test: `Test/Terraria.WorldFile.V319.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldImport.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`

**Step 1: Write loss fixtures.** A WLD fixture with `time = 1.5d` proves the
integer clock cannot round-trip it. Add raw weather matrices for valid
independent triples, absent fields, non-finite strength, negative duration, and
the endless-rain repair branch.

**Step 2: Decide time representation from source.** Search all server-visible
uses of time. If an integral invariant cannot be proved, add a versioned
fractional authoritative value with explicit scale/overflow validation. Packet
quantization, if V1456 requires it, remains projection-only and separately
tested.

**Step 3: Preserve raw weather before runtime policy.** Add a raw weather value
object containing `IsRaining`, `RainTimeTicks`, and `MaximumRainStrength`.
Carry it losslessly through WLD and compatibility models. Do not call
`WorldRuleState.WithRain` during import because it replaces maximum strength
with current strength.

**Step 4: Fail closed on secret-seed repair.** Clear all three raw weather fields
only when version, duration, and active secret-seed codes are available. A
missing secret-seed context produces explicit unsupported recovery, not a
guessed clear or retain result.

**Step 5: Verify.** Run all four listed verifiers. Valid values round-trip;
unsupported legacy repair is test-visible and rejected.

## Task 7: Model Time Rate and Invasion Travel Explicitly

**Files:**

- Create: `src/Terraria.Dome.Simulation/World/WorldTimeRateSnapshot.cs`
- Create: `src/Terraria.Dome.Simulation/World/Systems/WorldTimeRateSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/World/WorldClock.cs`
- Modify: `src/Terraria.Dome.Simulation/World/WorldProgressionState.cs`
- Modify: `src/Terraria.Dome.Simulation/World/Systems/WorldProgressionSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/DomeSimulationSnapshot.cs`
- Modify: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`
- Test: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- Test: `Test/Terraria.Dome.TickOrder.Verification/Program.cs`

**Step 1: Add RED cases.** Cover normal rate, frozen time, fast-forward,
sleep acceleration, menu fallback, travel from each side, spawn arrival, and
warning interval. Assert `TicksPerUpdate` is not substituted for legacy
`dayRate` without proof.

**Step 2: Add a validated immutable policy.** `WorldTimeRateSystem` receives
only source-backed inputs and returns `WorldTimeRateSnapshot`; no client-owned
mutable static state is allowed.

**Step 3: Establish tick order.** Compute the rate before clock advance, then
pass the result into the single invasion-travel consumer. Persist the rate as an
append-only format field. Older saves carry explicit legacy/unavailable rate
state, not fabricated semantics.

**Step 4: Verify.** WorldRules and TickOrder must prove stable rate -> clock ->
invasion -> warning -> snapshot -> restart order.

## Task 8: Decompose Main Initialization and Keep Generic Queues Deferred

**Source anchors:** Version4 `Main.cs:3732-3859` mixes definitions, world
initialization, protocol startup, content catalogs, and client presentation.
`Main.cs:11590-11755` advances mutable `DelayedProcesses`; recovered callers in
`WorldGen.cs` have different ownership and restart semantics.

**Files:**

- Modify: `docs/research/2026-08-22-initialize-almost-everything-owner-matrix.md`
- Modify: `docs/research/2026-08-20-main-member-coverage-matrix.md`
- Modify: `docs/research/2026-08-22-main-queue-caller-inventory.md`
- Create: `docs/research/2026-08-23-main-thread-action-contracts.md`
- Create: `docs/research/2026-08-23-delayed-process-contract-matrix.md`
- Modify only after accepted child: an existing owner in Simulation, Server, or
  Protocol
- Test: `Test/Terraria.Dome.MainBoundary.Verification/Program.cs`
- Test: `Test/Terraria.Dome.TickOrder.Verification/Program.cs`

**Step 1: Give every initializer call one result.** Record source line, mutable
state, server relevance, owner, persistence/protocol impact, verifier, and
   `accepted`, `deferred`, or `excluded` status.

**Step 2: Select exactly one bounded child family.** Possible candidates are a
supported Tile/NPC/Projectile definition subset or a typed TileEntity subset.
Bestiary, backgrounds, creative catalogs, mounts, minecarts, UI, and social
setup are excluded or deferred.

**Step 3: Test then implement a named owner.** Use immutable definition
registries or typed Server startup. Never create an aggregate initializer.

**Step 4: Require caller-specific queue contracts.** A candidate queue caller
must have input identity, owner, phase, ordering, cancellation, restart,
persistence, and protocol visibility. Reject arbitrary `Action` and
`IEnumerator`; implement at most one fully sourced caller or keep the boundary
deferred.

TrainingDummy activation is now connected to the Simulation tick. Valid typed
entities are evaluated against active player hitboxes and the explicit NPC slot
limit; an eligible entity queues a `TileEntity`-source spawn for definition/net
type `488`, and the deterministic spawn commit records ownership. The NPC
definition is registered in the default catalog with `aiStyle=92`, immortal and
always-replicate authority flags, and the stationary behavior path. Focused
WorldObjects, NPC, Persistence, and WorldObjects Loopback verifiers pass. Tile
placement/removal, a dedicated deactivation loopback, observer publication, and
message `86` remain deferred.

The authoritative player scan and type-488 creation path are now implemented
for valid persisted TrainingDummy entities. The tick queues a
`NpcSpawnSource.TileEntity` command, commits it through the NPC definition
registry, and projects the resulting ownership payload. Automatic cleanup on
invalid links is now performed in the deterministic NPC despawn commit; the
focused tick fixture proves inactive NPC plus cleared `[255, 255]` payload in
one tick. Tile placement/removal, observer publication, dedicated network
coverage, and message `86` publication remain deferred.

The V1456 adapter now provides strict TrainingDummy wire projections for
`TileEntitySharing` (`86`), its removal form, and `TileEntityPlacement` (`87`).
Server sessions maintain a per-entity cursor and project changed type-0
entities only inside visible sections, including removal frames. Focused wire
field and cursor verification plus Protocol/Server builds pass. Placement
authorization and full client handling remain deferred. Initial-section
ownership marking and the server-owned observer loopback are evidenced.

Initial `TileSection` bootstrap now includes supported type-0 TrainingDummy
entities by section and marks them sent in the per-session cursor. Subsequent
visible-section replication therefore sends only changes; entities outside the
initial section set continue through the delta path. Protocol, Server,
WorldObjects, and FullClientBootstrap verification pass. Client placement
authorization and full observer/client mutation handling remain deferred.

The `RequestSection` path uses the same typed entity encoding and cursor marking,
so section-on-demand delivery is consistent with initial bootstrap. Inbound
client `87` placement authorization remains a separate deferred command path.

Typed placement/removal ownership is now connected to the existing validated
tile manipulation path. After tile commit, a placed type-378 tile creates a
type-0 TrainingDummy entity with payload `Int16 -1`; killing the tile removes
the entity and queues owned NPC cleanup. Duplicate and invalid placement are
rejected by the Simulation owner. Direct unvalidated client `87` placement is
still deferred, preserving the server authority boundary.

**Step 5: Verify.** Run the changed child verifier, MainBoundary, TickOrder,
and `git diff --check`.

## Task 9: Make WorldGen Stage-Oriented and Oracle-Driven

**Source anchors:** Version4 `WorldGen.cs:10108` (`GenerateWorld`), `10553`
(`AddPasses`), and `4454-4527` (liquid methods). Current
`WorldGenerationPipeline` supports only default seed, difficulty 0, and
non-hardmode rules.

**Files:**

- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationRequest.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationPipeline.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationStateComponent.cs`
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationRuntimeState.cs`
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationRandomSnapshot.cs`
- Modify: affected systems under `src/Terraria.Dome.Simulation/WorldGeneration/Systems/`
- Test: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`
- Modify: `docs/worldgen/worldgen-source-inventory.json`
- Modify: `docs/worldgen/worldgen-parity-report.md`
- Modify: `docs/worldgen/worldgen-deletion-gate.json`

**Step 1: Build a stage fingerprint harness.** For one supported fixed profile,
capture source and ECS fingerprints after Terrain, Cave, Biome, Ore, Structure,
Tree, Liquid, Frame, and final commit. Include metadata, extended Tile state,
command sequence, and random-state checkpoint when applicable.

**Step 2: Migrate runtime state before more predicates.** Add only source-backed
request/options, stage, deterministic sequence, random state, and checkpoint
state. Do not indiscriminately move `Main` globals or `GenVars` fields.

**Step 3: Recover random consumption before random branches.** Capture every
relevant `Main.rand` and WorldGen consumption for the selected source path. If a
chain crosses unrecovered client/global behavior, preserve that behavior as
deferred rather than emulating one observed `Next` call.

**Step 4: Advance one stage per change.** Each stage follows:

```text
source lines -> failing differential -> immutable snapshot query
-> typed command batch -> atomic commit -> stage fingerprint
-> source-inventory record -> focused verifier
```

**Step 5: Keep deletion gate false.** Do not set it true until target-mode
entries and fields have owners, all required random state is recovered, full
differential evidence agrees, and required regression projects pass.

## Task 10: Build a Version4 Physical Deletion Oracle Ledger

**Files:**

- Read: `D:\TRbackup\Version3删除多余同时人工审查代码`
- Read: `D:\TRbackup\Version4物理删除了某些文件`
- Create: `docs/migrations/version4-physical-deletion-ledger.csv`
- Create: `docs/migrations/version4-physical-deletion-ledger.md`
- Modify: `docs/migrations/main-server-responsibility-ledger.md`
- Create: `Build/diagnostics/server-ecs-convergence/P4-deletion/<run-id>/inventory.json`

**Step 1: Produce a reproducible relative-path and hash inventory.** Record all
535 removals, including namespace, declarations, size, and direct references
from retained Version4 server files.

**Step 2: Assign one classification:**

```text
ClientOnly
ServerRelevant
SharedDefinition
RecoveredFromCompleteOracle
ReplacedWithEvidence
Unknown
```

`ClientOnly` needs source evidence that it cannot mutate authoritative server
state. `ServerRelevant`, `SharedDefinition`, and `Unknown` block any deletion
claim until an oracle/replacement decision exists.

**Step 3: Join every relevant record to ECS evidence.** Record legacy anchor,
current owner, target component/snapshot/definition, command/commit,
persistence/protocol consequence, verifier, and status. Similar naming is not
replacement evidence.

**Step 4: Add a ledger completeness gate.** A read-only verifier or PowerShell
audit fails when a server-relevant deletion lacks required evidence columns.

## Task 11: Reconcile Claims Using Fresh Evidence

**Files:**

- Modify: `docs/server-completion/completion-manifest.json`
- Modify: `docs/server-completion/capability-matrix.md`
- Modify: `docs/migrations/main-server-responsibility-ledger.md`
- Modify: `docs/worldgen/worldgen-deletion-gate.json`
- Modify: `progress.md`
- Create: `Build/diagnostics/server-ecs-convergence/P-final/<run-id>/summary.md`

**Step 1: Recalculate only what fresh evidence proves.** Every capability row
must name a legacy reference, authority owner, projection, verifier command, and
fresh exit code. Keep WorldGen parity and physical-deletion status separate from
the 100-point core capability score.

**Step 2: Run focused gates serially.** After the corresponding work changes,
run PlayerAuthority, PlayerLifecycle Loopback, Combat Loopback, WorldRules
Loopback, WorldClock, WorldImport, MainBoundary, and `git diff --check` using
the repository serial MSBuild properties.

**Step 3: Preserve unresolved status.** Unknown queues, unrecovered RNG,
unmapped WorldGen entries, incomplete deletion classifications, and failed
loopback projects remain `deferred` or `blocked`. They must not be converted
into an estimated migration percentage.

## Exit Criteria

The proposal is implemented only when P0-P4 produce new evidence satisfying the
individual acceptance rules. It does not authorize a claim of full Terraria
compatibility, deletion of the Version4 oracle, deletion of legacy WorldGen
evidence, or a 100% migration rate. Those require a separate acceptance review
with a complete source/runtime oracle and a fresh full differential run.

## Execution Checkpoint: 2026-08-22

This proposal was executed against the existing shared worktree. No unrelated
changes were reverted and no commit was created.

### Verified in this execution

- `Terraria.Dome.Simulation` Release build: exit `0`.
- `Terraria.Dome.Server` Release build: exit `0`.
- PlayerAuthority verifier: pass.
- PlayerLifecycle Loopback verifier: pass.
- Combat Loopback verifier: pass on isolated rerun.
- Completion verifier: pass; manifest `100` rows and core score `92/100`.
- `git diff --check` excluding the byte-preserved legacy `WorldFile.cs` oracle:
  no whitespace errors. Git emitted only existing LF/CRLF conversion warnings.

Evidence is stored under:
`Build/diagnostics/server-ecs-convergence/P0-build/20260822-execution-p0/`,
`Build/diagnostics/server-ecs-convergence/P1-check/`, and
`Build/diagnostics/server-ecs-convergence/P-final/`.

### Gates that remain open

- WorldGen verifier did not complete within the controlled 30-second rerun and
  the authoritative gate remains `canRemoveLegacyWorldGen=false`; the complete
  differential still records `5,040,000` mismatches.
- Physical deletion ledger verifier remains exit `1` with
  `serverRelevantDeferred=57`; this is an intentional safety failure.
- TrainingDummy now has a bounded typed authority chain (validity, persistence,
  ownership, activation/deactivation, NPC 488 lifecycle, placement/removal owner,
  and V1456 86/87 projection), but complete qualification remains `Partial`:
  direct client 87 authorization and client mutation semantics for that
  server-owned message remain deferred. The server-owned observer loopback is
  now evidenced.

Therefore this execution checkpoint proves the bounded server capability gates,
but does not authorize a full-convergence or legacy-removal claim.

### Follow-up progress: Training Dummy validity slice

The first independently verifiable TileEntity contract is now implemented:
`TileEntityTrainingDummyValidityQuery.IsValid(WorldTile)` matches the complete
oracle's type/frame predicate. The WorldObjects verifier passes valid,
inactive, wrong-type, misaligned-frame, and nonzero-frame-row cases; the
Simulation Release build remains warning-free. This is partial evidence only.
Activation/deactivation, NPC-488 linkage, persistence continuation, protocol
projection, and loopback placement/removal remain open, so the TileEntity
qualification and physical-deletion gate are unchanged.

The next typed-state slice is also complete: `TrainingDummyTileEntityState`
parses the source-compatible non-opaque type `0` payload as a little-endian
`Int16 npc`, retaining entity identity and tile position while rejecting
opaque or malformed payloads. WorldObjects, Persistence, and Simulation build
gates pass. This does not establish NPC ownership or restart continuation, so
the TrainingDummy and deletion gates remain open.

The following runtime predicate is now separately evidenced:
`TrainingDummyNpcLinkValidityQuery` requires a non-negative persisted link, an
active NPC of type `488`, and exact equality between the NPC AI tile
coordinates and the entity position. The focused WorldObjects verifier covers
valid, inactive, wrong-type, wrong-coordinate, and unlinked cases. This remains
validation-only; NPC creation, deactivation, repair, projection, and loopback
are still deferred.

The activation decision contract is now separately evidenced. It preserves the
oracle guard order for an existing link, `npcSlotsFull`, nearby active players,
and no-player rejection. The result is validation-only: current
`SpawnNpcCommand` uses `DefinitionId`, while the oracle supplies NPC net type
`488`, and no source-backed mapping between those identities exists. NPC spawn,
AI assignment, lifecycle ownership, and projection therefore remain deferred.

The complementary deactivation decision is also evidenced: a non-negative
TrainingDummy link is marked for deactivation only when the active/type/AI
coordinate predicate fails, while an unlinked entity is left unchanged. This
does not mutate `NpcLifecycleComponent`, clear the persisted link, or project
message `86`; those authority and protocol steps remain deferred.

The complete NPC oracle now provides a usable partial definition boundary for
type `488`: identity `DefinitionId=488`/`NetId=488`, size `18x40`, defense `0`,
life max `1000`, and stationary behavior are evidenced, with a dedicated
`NpcBehaviorId.TrainingDummy` branch and passing NPC verifier. The definition
is intentionally not registered in `DomeSimulation` yet because `aiStyle=92`,
`immortal`, `netAlways`, AI coordinate assignment, and tile-entity lifecycle
ownership are not all represented by the current commit path.

Typed ownership transitions are now separately evidenced through
`TrainingDummyOwnershipState`: link creation requires an unlinked entity, a
valid `NpcHandle`, and a validated type-488 NPC; clear requires matching
identity/position and an invalid link, with revision increments `1 -> 2`.
WorldObjects, Persistence, Loopback, and Simulation build gates pass. This
boundary is not yet integrated with the Simulation tile-entity dictionary or
Arch NPC store, and does not publish V1456 state.

The authority flag slice is now consumed rather than merely stored:
`NpcAuthorityComponent` carries `AiStyle`, `IsImmortal`, and `AlwaysReplicate`
through spawn/restore, and lifecycle prevents immortal NPC death/timeout while
leaving damage resolution active. `NpcSpawnSource.TileEntity` is defined for a
future activation command. The default catalog still does not register
TrainingDummy until AI coordinate binding and tile-entity ownership are proven.

The typed ownership boundary is now wired into the Simulation owner through
`TryLinkTrainingDummyNpc` and `TryClearTrainingDummyNpc`. A snapshot-backed
focused fixture proves active net type `488` validation, duplicate-link
rejection, revision `1` link creation, and revision `2` clear. The generic
payload is now projected from runtime ownership: the fixture observes `[9, 0]`
for a linked NPC handle and `[255, 255]` after clear. Restore reconstruction,
automatic tick activation/deactivation, tile removal, and message `86`
publication remain deferred.

Persisted non-negative TrainingDummy links are now fail-closed reconstructed by
the Simulation owner when an active type-488 NPC replication exists at the
exact entity tile coordinates. The restart fixture round-trips `[9, 0]` and
executes the restored clear transition at revision `2`; absent or mismatched
NPC state is not inferred. Automatic activation/deactivation, NPC creation and
cleanup, tile removal, reconnect/observer publication, and message `86` remain
deferred.

### Execution Checkpoint: 2026-08-22 22:30

Fresh evidence is recorded under
`Build/diagnostics/server-ecs-convergence/P-final/20260822-2230/`.

- Simulation Release build: exit `0`, 0 warnings, 0 errors.
- Server Release build: exit `0`, 0 warnings, 0 errors.
- WorldObjects, Persistence, Protocol Compatibility, FullClientBootstrap, and
  MainBoundary verifiers: all exit `0`.
- Completion verifier: exit `0`; manifest `100`, evidenced core score `92/100`.
- WorldObjects loopback: exit `0`.
- `git diff --check` excluding the byte-preserved WorldFile oracle: exit `0`.
- Physical deletion ledger: exit `1` by safety design, with
  `rows=535`, `serverRelevantDeferred=57`.

This checkpoint closes the currently progressable bounded server gates only. It
does not authorize full convergence, physical deletion, removal of the legacy
WorldGen oracle, or promotion of TrainingDummy from `Partial` to `Evidenced`.

### Current Full Verifier Sweep: 2026-08-23 01:00

The current worktree was run through all 49 verifier projects in fresh serial
processes with `UseSharedCompilation=false` and `MSBuildNodeReuse=false`.
After correcting an invocation that accidentally forwarded `--nologo` as an
application argument to FullClientBootstrap, the clean rerun passed. The
authoritative sweep result is **49 passed, 0 failed**:
`Build/diagnostics/server-ecs-convergence/P-regression/20260823-0100/results.json`.

This does not change the separate WorldGen complete differential or physical
deletion gates.

### Success-Criteria Audit: 2026-08-23 01:30

The current requirement-by-requirement audit is recorded in
`Build/diagnostics/server-ecs-convergence/P-final/20260823-0130/success-criteria-audit.json`.
It confirms the 49-project verifier sweep, Main responsibility classification,
and WLD round-trip boundaries, while explicitly recording WorldGen stage parity
as partial and physical deletion safety as blocked by 57 deferred
`ServerRelevant` rows. The overall proposal status therefore remains
`incomplete` despite green bounded gates.

Activation eligibility now has a separately evidenced pure query using explicit
player hitboxes. It matches the oracle's 32x48 pixel entity rectangle inflated
by 1600 pixels and rejects inactive or out-of-bounds players. The focused
WorldObjects verifier, WorldObjects loopback verifier, and Simulation build all
pass. This does not yet implement the authoritative player scan, NPC slot
capacity, NPC creation, AI assignment, or message 86 projection; those remain
deferred.

### WorldGen Framing Contract Audit: 2026-08-23 02:00

The focused `Terraria.Dome.WorldGeneration.Verification` run passed with exit
code `0`. A source-to-target audit is recorded at
`Build/diagnostics/server-ecs-convergence/P9-worldgen/20260823-0200/verifier.log`.
It proves only the `SquareTileFrame` 3x3 row-major request topology against the
complete Version4 oracle (`WorldGen.cs:67181`) and the immutable ECS request
query. The oracle delegates frame-value semantics to `TileFrame` at line
`68021`; wall framing, range-frame side effects, mutable tile ownership, and
runtime notifications remain unmapped. This is partial topology evidence, so
the WorldGen differential and `canRemoveLegacyWorldGen=false` deletion gate
are unchanged.

### WorldGen Range and Wall Topology Audit: 2026-08-23 04:00

The next source-backed framing slice is recorded at
`Build/diagnostics/server-ecs-convergence/P9-worldgen/20260823-0400/verifier.log`.
The oracle anchors `SquareWallFrame` at `WorldGen.cs:67196` and `RangeFrame` at
`WorldGen.cs:67211`; the ECS queries provide candidate bounded 3x3 and
expanded-rectangle coordinate topology, but the current structured inventory
still leaves both oracle methods `Unmapped`. The focused WorldGeneration
verifier exits `0`. Frame-value semantics, center reset
behavior, map-update queues, and mutable runtime side effects remain deferred,
so the complete differential and `canRemoveLegacyWorldGen=false` gate are
unchanged.

### TileFrameImportant Branch Audit: 2026-08-23 05:00

The focused WorldGeneration verifier again exited `0`. A source-to-target audit
is recorded at
`Build/diagnostics/server-ecs-convergence/P9-worldgen/20260823-0500/verifier.log`.
The private Version4 dispatcher (`WorldGen.cs:71665`) has candidate typed
queries for its type `136`, `184`, `324`, and `529` branches, with source
anchors, bounded random state, and supported/kill verifier cases. The current
structured inventory still leaves the dispatcher `Unmapped`, so these queries
remain candidate evidence rather than accepted replacement mappings. Other
dispatcher branches and mutable `KillTile`/notification behavior remain
deferred. The complete WorldGen differential and deletion gate are unchanged.

### Current-State Regression Audit: 2026-08-23 03:30

The current dirty worktree was re-run after the later Simulation, item,
projectile, NPC, and protocol edits. The fresh audit is recorded at
`Build/diagnostics/server-ecs-convergence/P-final/20260823-0330/success-criteria-audit.json`.
Simulation and Server Release builds both exited `0` with zero warnings and
zero errors. The 49 claimed verifier projects passed from clean processes. The
50th executable test project is `Terraria.Dome.RealClientFixtureHost`, which is
a required-argument infrastructure host rather than a verifier; its no-argument
probe was excluded after reporting the expected missing `--port` error.

The physical deletion safety check remains intentionally nonzero with
`rows=535` and `serverRelevantDeferred=57`. WorldGen remains partial: the new
SquareTileFrame topology audit is source-backed, while the complete differential
still mismatches `5,040,000` of `5,040,000` tiles. The overall proposal status
therefore remains `incomplete`.

### WorldGen Inventory Consistency Correction: 2026-08-23 07:00

The structured inventory was re-counted after the framing audits. The prior
gate metadata had drifted from the actual JSON method statuses. The authoritative
counts are `684` methods, `125 Partial`, and `559 Unmapped`, with `233` fields;
the gate now records these exact values. Evidence is recorded at
`docs/worldgen/worldgen-deletion-gate.json` (counts are checked directly from the structured inventory).
This is a bookkeeping correction only and does not improve semantic parity or
change `canRemoveLegacyWorldGen=false`.

### Fresh Complete WorldGen Differential: 2026-08-23 06:00

The source-derived complete differential was rerun after the framing audits.
Evidence is recorded at
`Build/diagnostics/server-ecs-convergence/P9-worldgen/20260823-0600/differential.log`.
The diagnostic process exited `0`, but the semantic result remains
`5,040,000 / 5,040,000` tile mismatches with `1,046,843` extended-state
mismatches. This confirms that the new topology and typed branch mappings do
not justify a WorldGen deletion claim; `canRemoveLegacyWorldGen=false` remains.
