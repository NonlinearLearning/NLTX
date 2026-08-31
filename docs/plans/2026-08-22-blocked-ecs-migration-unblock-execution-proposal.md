# Blocked ECS Migration Unblock Implementation Plan


**Goal:** Resolve the evidence and ownership gaps that prevent the remaining ECS migration cards
from advancing without inventing legacy behavior.

**Architecture:** Treat each blocker as an independent authority contract. First restore a
buildable Simulation verification boundary without absorbing unrelated WorldGeneration work. Then
model only source-backed values as immutable Simulation, Server, or Compatibility facts; arbitrary
delegates, iterators, legacy global random order, and client state remain outside Simulation until
a typed owner and replay contract exist.

**Tech Stack:** .NET 10, C#, Arch ECS, immutable snapshots, WLD V319 compatibility models,
deterministic verifier projects, PowerShell, Version4 source oracle.

---

## Operating Constraints

1. Read `AGENTS.md`, `约束/Google-CSharp-Style-Guide-约束.md`,
   `docs/plans/2026-08-20-remaining-ecs-migration-execution.md`, and the exact Version4 member
   before every source edit.
2. Preserve the dirty worktree. In particular, do not overwrite or fold unowned WorldGeneration
   files into migration behavior without an ownership decision.
3. Use `apply_patch` for deliberate source and documentation changes. Create fresh evidence under
   `Build/diagnostics/main-migration/<card>/<run-id>/`.
4. Maintain the reduced validation scope: run the changed card's focused verifier, at most one
   directly affected verifier, one affected project build when required, `MainBoundary`, and scoped
   `git diff --check`. Do not run the full regression matrix, duplicate replay matrix, client suite,
   or root Release build unless the user changes scope.
5. An item is `accepted` only when a unique owner, source predicate, rejected case, persistence or
   projection effect, and focused GREEN evidence agree. Otherwise record `unknown`, `deferred`, or
   `excluded`; never normalize a missing source fact to a default.

## Current Blockers

| Id | Scope | Current blocker | Required result |
|---|---|---|---|
| B-001 | Focused verification | Untracked `CoatingColorSelectionQuery.cs` cannot resolve `WorldTile` | Separate build-ownership fix or explicit exclusion, then focused verifiers compile |
| B-002 | M-009 invasion travel | No authoritative equivalent of mutable `Main.dayRate` | Approved versioned time-rate snapshot and one phase consumer |
| B-003 | M-007 WLD clock | WLD `Double` time cannot round-trip the `Int32` clock | Integral invariant proof or a versioned fractional authority |
| B-004 | M-008 WLD rain | Saved maximum/current strength and secret-seed repair inputs are incomplete | Lossless raw-to-runtime mapping and explicit repair eligibility |
| B-005 | M-001 initialization | Mixed startup responsibilities have no aggregate server owner | One independently owned family per card; no aggregate initializer |
| B-006 | M-014/M-024 queues | Arbitrary `Action`/`IEnumerator` have no typed/replayable owner | Caller-specific typed contracts, or retained deferred boundary |
| B-007 | Meteor probability | Domain RNG does not reproduce global `Main.rand` order | Executable legacy RNG oracle, or retained deferred branch |

## Task 1: Attribute and Restore the Focused Build Boundary (B-001)

**Files:**

- Read: `src/Terraria.Dome.Simulation/WorldGeneration/CoatingColorSelectionQuery.cs`
- Read: `src/Terraria.Dome.Simulation/World/WorldTile.cs`
- Read: `src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj`
- Test: `Test/Terraria.Dome.WorldClock.Verification/Program.cs`
- Test: `Test/Terraria.Dome.MainBoundary.Verification/Program.cs`
- Create: `Build/diagnostics/main-migration/task-9-build-ownership-boundary/<run-id>/scenario.yaml`

**Step 1: Record the ownership baseline.** Capture `git status --short` for the query, its
consumer set, and `WorldTile`; record whether each is user-owned/untracked before editing.

**Step 2: Reproduce the narrow build failure.** Run the WorldClock verifier and retain the exact
`CS0246` output. Do not edit an unowned file solely to silence a broad build.

**Step 3: Select the owner.** If the query belongs to the current migration scope, add only the
namespace/import required for the existing `WorldTile` type. If it belongs to another WorldGeneration
card, record the build dependency and assign it to that card's owner instead of modifying it here.

**Step 4: Write the focused regression.** Add or update a small query-level assertion proving
`null` returns the default selection and a tile projects its four coating flags. The test must use
the real `WorldTile` value type rather than a local surrogate.

**Step 5: Verify the boundary.** Run the query's focused verifier or its affected project build,
then WorldClock and MainBoundary. Record compilation and test exit codes separately.

**Step 6: Stop condition.** If no owner authorizes the query edit, retain B-001 as `blocked`;
do not use a duplicate `WorldTile` type, a type alias, or a conditional compilation workaround.

## Task 2: Approve and Model Mutable Time-Rate Authority (B-002)

**Files:**

- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:3567-3595,12958-13046`
- Read: `docs/plans/2026-08-22-invasion-travel-integration-boundary.md`
- Create: `docs/plans/2026-08-22-world-time-rate-authority-design.md`
- Create later: `src/Terraria.Dome.Simulation/World/WorldTimeRateSnapshot.cs`
- Modify later: `src/Terraria.Dome.Simulation/World/WorldClock.cs`
- Modify later: `src/Terraria.Dome.Simulation/Snapshots/DomeSimulationSnapshot.cs`
- Modify later: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`
- Modify later: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Test later: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- Test later: `Test/Terraria.Dome.TickOrder.Verification/Program.cs`

**Step 1: Obtain design approval.** Present the three candidates: versioned explicit rate snapshot,
fixed `TicksPerUpdate` substitution, and retained unknown. Proceed only after approval of the
versioned snapshot; the fixed substitution is source-incompatible.

**Step 2: Write RED cases.** Add named cases for normal target rate, fast-forward `60`, frozen `0`,
sleep acceleration, game-menu fallback `1`, and invasion movement using `max(rate, 1)`. Verify the
existing system lacks an integrated travel update.

**Step 3: Add immutable input and output contracts.** Define source-backed inputs for fast-forward,
freeze, target rate, active/sleeping players, and menu state. Define a validated output rate and
its exact snapshot/persistence representation. Do not expose a mutable global or client command.

**Step 4: Add one policy.** Implement a named time-rate policy mirroring `UpdateTimeRate` and make
`ApplyWorldClock` calculate its snapshot before the single invasion-travel consumer executes.

**Step 5: Persist and restore.** Append a versioned rate field; older persistence must carry an
explicit unavailable/legacy policy state, not a fabricated contemporary rate.

**Step 6: GREEN and scope.** Prove snapshot restart continuity, pause behavior, all five rate cases,
and same-tick ordering. Keep random start side, NPC tables, warning/chat and player-stat production
as separate M-009 branches.

## Task 3: Resolve the Fractional WLD Clock Contract (B-003)

**Files:**

- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria.IO\WorldFile.cs:1312-1313,2125-2126`
- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:13525-13527`
- Read/modify: `docs/plans/2026-08-22-wld-clock-fraction-boundary.md`
- Modify later: `src/Terraria.Dome.Simulation/World/WorldClock.cs`
- Modify later: `src/Terraria.Dome.Simulation/World/WorldClockSnapshot.cs`
- Modify later: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`
- Modify later: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Test: `Test/Terraria.Dome.WorldClock.Verification/Program.cs`
- Test: `Test/Terraria.WorldFile.V319.Verification/Program.cs`

**Step 1: Preserve the `1.5d` RED fixture.** The test must demonstrate loss through the current
integer model without introducing a cast, rounding rule, or silent threshold into production code.

**Step 2: Complete the source-use inventory.** Record every supported source use of time in save,
packet, event boundary, day/night transition, pause, and restart paths. Search all retained source
versions for an integral-only invariant.

**Step 3: Choose a representation by evidence.** If integral-only storage is proven, encode the
predicate and reject unsupported saves explicitly. Otherwise introduce a versioned fractional
authoritative value plus explicit packet projection behavior.

**Step 4: Add append-only persistence and protocol tests.** Prove a fractional save/restart
round-trip, day/night transition, pause continuation, and V1456 projection semantics.

**Step 5: Accept only the chosen contract.** Update M-007 only after the focused WLD and WorldClock
verifiers pass against the final source tree.

## Task 4: Complete WLD Rain Import Without Conflation (B-004)

**Files:**

- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria.IO\WorldFile.cs:3365-3383,3685-3690`
- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:12462-12465,13325-13337`
- Modify later: `src/Terraria.WorldFile.V319/Model/LegacyWorldMetadata.cs`
- Modify later: `src/Terraria.WorldCompatibility/Model/CompatibilityWorldMetadata.cs`
- Modify later: `src/Terraria.WorldCompatibility/Projection/CompatibilityToDomeProjection.cs`
- Modify later: `src/Terraria.Dome.Simulation/World/WorldRuleState.cs`
- Test: `Test/Terraria.WorldFile.V319.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldImport.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`

**Step 1: Write a raw-state matrix.** Cover independent valid triples, invalid non-finite/out of
range values, pre-field layout absence, and lossless metadata propagation.

**Step 2: Model secret-seed eligibility.** Identify the canonical compatibility owner for the
active secret-seed code set. If that owner cannot be restored from the WLD/current server context,
represent the repair result as unavailable and reject/defer the branch.

**Step 3: Prove maximum versus current strength.** Trace the source transition that determines
current cloud/rain strength after load. Do not assign saved `maxRaining` to `RainStrength` until
this relation is sourced.

**Step 4: Implement only the proven route.** Project raw facts through the recovery-only API;
repair clears all three fields atomically only when version, duration, and secret-seed predicates
are available.

**Step 5: Verify parser/import/rules behavior.** Retain rejected unsupported repairs as a passing
fail-closed outcome. Add restart continuity only for values with an actual owner.

## Task 5: Decompose Remaining Initializer Families (B-005)

**Files:**

- Read: `docs/research/2026-08-22-initialize-almost-everything-owner-matrix.md`
- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:3732-3859`
- Modify: `docs/research/2026-08-22-initialize-almost-everything-owner-matrix.md`
- Modify only after an accepted child: `docs/research/2026-08-20-main-member-coverage-matrix.md`
- Test: `Test/Terraria.Dome.MainBoundary.Verification/Program.cs`

**Step 1: Select exactly one unresolved row.** A row is eligible only with one server/domain owner,
bounded source calls, observable state, and a focused verifier. Candidate families include a
supported tile-entity subset, a supported torch definition subset, or a supported shop contract;
do not choose a mixed catalog or UI family.

**Step 2: Create a child scenario card and RED.** Record source anchors, preconditions, required
state, rejected inputs, persistence/projection needs, and all deferred sibling calls.

**Step 3: Implement a named owner only.** Place one immutable definition or typed command under its
existing domain. Never create `Simulation.InitializeAlmostEverything` or an orchestration facade.

**Step 4: Run the child verifier and MainBoundary.** Mark only the selected child `accepted`; leave
M-001 `planned` until all server-relevant initializer rows have their own evidence.

## Task 6: Establish Caller-Specific Queue Contracts (B-006)

**Files:**

- Read: `docs/research/2026-08-23-main-thread-action-contracts.md`
- Read: `docs/research/2026-08-23-delayed-process-contract-matrix.md`
- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs:10529,26103-26125`
- Create later: caller-specific contract under the owning Server or WorldGeneration domain
- Test later: a focused owner verifier

**Step 1: Find a named caller, not a container.** Recover an in-tree source caller with stable
input identity, phase, cancellation, retry, restart, persistence, and protocol visibility.

**Step 2: Write a rejection-first contract.** Demonstrate that arbitrary `Action` or `IEnumerator`
cannot enter the typed owner. Define exact ordering and pause behavior for the named caller.

**Step 3: Place the type in the actual owner.** Section completion remains host/client state;
background WorldGen continuation remains WorldGeneration/Server state unless source proves a
Simulation value route.

**Step 4: Add restart and cancellation tests.** If no complete caller contract is recovered,
retain M-014/M-024 as deferred and do not add `Queue<Action>` or a generic coroutine scheduler.

## Task 7: Recover or Reject Legacy Global RNG for Meteor Probability (B-007)

**Files:**

- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:13739-13762`
- Read: `docs/plans/2026-08-20-simulation-random-stream-contract.md`
- Modify: `Build/diagnostics/main-migration/task-9-probabilistic-meteor-scheduling-boundary/<run-id>/scenario.yaml`
- Modify later: a named meteor eligibility/scheduling system only after oracle recovery
- Test later: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`

**Step 1: Build an executable source trace.** Capture every `Main.rand` consumption in the relevant
night-start path and its ordering prerequisites. A single `Next(50)` observation is insufficient.

**Step 2: Compare trace state.** Prove the trace's seeded state can be restored independently of
client/UI/WorldGen consumers. If not, document the first non-recoverable consumer.

**Step 3: Implement only with equivalence proof.** A compatible stream must reproduce the qualified
and unqualified branch without rescheduling after persistence restore. Otherwise preserve the
explicit command path and retain automatic probability as deferred.

**Step 4: Verify the chosen outcome.** Use a fixed trace, an unqualified rejection, a persisted
mid-trace continuation, MainBoundary, and scoped diff check.

## Task 8: Focused Acceptance Review

**Files:**

- Modify: `docs/research/2026-08-20-main-member-coverage-matrix.md`
- Modify: `progress.md`
- Create: `Build/diagnostics/main-migration/task-9-blocked-unblock-review/<run-id>/status.txt`

**Step 1: Reconcile every B-001 through B-007 outcome.** Cite its source hash, owner, scenario,
focused output, deferred branches, and exact status.

**Step 2: Verify status claims.** An unresolved branch must remain `unknown`/`deferred`; an
accepted branch must have a unique owner, focused GREEN, and required persistence/projection test.

**Step 3: Run reduced gates.** Run each changed card's verifier, one affected build only when a
source project changed, MainBoundary, and scoped `git diff --check`.

**Step 4: Record the next dependency.** Do not claim broad migration completion, a percentage, or
root Release readiness from this focused review.

## Definition of Done

This proposal is complete when every blocker has either an evidence-backed implementation path or
an explicit, tested deferred boundary. The broader migration remains incomplete until every
server-relevant Main responsibility has a source-backed owner and all required behavior cards are
accepted independently.
