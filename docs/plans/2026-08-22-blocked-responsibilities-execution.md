# Remaining Blocked Responsibilities Execution Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Resolve or explicitly re-qualify the remaining migration blockers without inventing legacy
semantics, while keeping validation limited to the approximately 40 percent focused test policy.

**Architecture:** Each blocker is treated as a source-backed responsibility contract. A card may add a
small owner and verifier only after its source evidence establishes identity, ordering, persistence,
replay, and projection semantics. Missing legacy or client evidence remains a recorded blocker; it is
never replaced with a guessed default. `Initialize_AlmostEverything` remains decomposed into distinct
owners, and Simulation remains free of `Main`, client, transport, and server-host dependencies.

**Tech Stack:** C#/.NET 10, Arch ECS, existing focused verification projects, Markdown evidence cards,
MainBoundary verifier, deterministic scenario YAML.

---

## Operating Rules

- Execute in batches of at most three cards, then stop for review.
- Preserve unrelated dirty changes. Manual edits use `apply_patch`.
- Before C# edits read `AGENTS.md` and `约束/Google-CSharp-Style-Guide-约束.md`.
- Inspect `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs` and the relevant legacy owner
  before writing a contract.
- Use focused verification only; do not run the root Release/client/full regression suite.
- Every accepted card must include a research note, fresh `scenario.yaml`, `progress.md` entry,
  coverage-matrix update, MainBoundary output, and scoped `git diff --check`.
- A card is not accepted when its required oracle or source contract is unavailable.

## Batch 1: Establish the Missing Contracts

### Task 1: M-001 initializer responsibility-family inventory

**Purpose:** Prove whether a new independent initializer family exists. Do not add an aggregate
`InitializeAlmostEverything` owner.

**Files:**

- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`
- Read: current owner files under `src/Terraria.Dome.Simulation`
- Create: `docs/research/2026-08-22-initializer-responsibility-family-inventory.md`
- Create: `Build/diagnostics/main-migration/task-13-initializer-family-inventory/<run-id>/scenario.yaml`
- Modify: `docs/research/2026-08-20-main-member-coverage-matrix.md`
- Modify: `progress.md`

**Steps:**

1. Extract each remaining initializer call and classify it by unique state owner, inputs, output
   observables, and restart behavior.
2. Write a RED inventory assertion that fails if a proposed family has no unique owner or observable
   contract.
3. Add no production initializer unless all four fields are source-backed.
4. Run the smallest existing coverage verifier plus MainBoundary and scoped diff check.
5. Mark the result `deferred` when no new family is proven; record the next evidence requirement.

**Acceptance:** The matrix explicitly shows why no aggregate initializer is added and names any
independent family that is actually supported by evidence.

## Batch 2: Typed Caller Contracts

### Task 2: M-014 caller-specific MainThreadAction contract

**Purpose:** Replace the unresolved arbitrary-action question with a caller inventory and a typed,
replayable contract only where the legacy caller has recoverable semantics.

**Files:**

- Read: legacy `Main.QueueMainThreadAction` and all call sites
- Read: `docs/research/2026-08-24-main-thread-action-caller-inventory.md`
- Create: `docs/research/2026-08-22-main-thread-action-contract.md`
- Create: `Build/diagnostics/main-migration/task-14-main-thread-action-contract/<run-id>/scenario.yaml`
- Modify: the owning Simulation command/phase files only if a typed caller is proven
- Modify: `Test/Terraria.Dome.TickOrder.Verification/Program.cs` when ordering is affected
- Modify: `progress.md` and the coverage matrix

**Steps:**

1. Build a table of caller identity, phase, ordering key, cancellation, restart, persistence, and
   client/projection side effects.
2. Add RED cases for unknown caller, duplicate replay key, and wrong phase.
3. Implement the smallest typed command contract for a caller with complete evidence.
4. Replay the same command sequence twice and compare ordered commit hashes.
5. Keep unsupported client/UI and WorldGen follow-ups deferred.

**Acceptance:** No `Queue<Action>` is introduced; accepted commands have typed identity and replay
behavior, while unresolved callers remain explicitly deferred.

### Task 3: M-024 delayed-process phase/cancellation/restart contract

**Purpose:** Establish whether any delayed process can be represented as a typed ECS process. Do not
introduce a generic `IEnumerator` scheduler.

**Files:**

- Read: legacy `DelayedProcesses` and `DelayedProcessesInGame` update loops and call sites
- Read: `docs/research/2026-08-24-delayed-process-caller-inventory.md`
- Create: `docs/research/2026-08-22-delayed-process-contract.md`
- Create: `Build/diagnostics/main-migration/task-24-delayed-process-contract/<run-id>/scenario.yaml`
- Modify: the owning Simulation phase only if a typed process is fully evidenced
- Modify: `progress.md` and the coverage matrix

**Steps:**

1. Record phase, first/next resume point, cancellation trigger, restart state, and presentation
   effects for every recovered caller.
2. Add RED cases for phase mismatch, cancellation after restart, and duplicate resume.
3. Implement one typed process owner only when all required fields are known.
4. Run a two-replay focused verifier and MainBoundary.
5. Otherwise leave the row deferred with the missing evidence named.

**Acceptance:** No generic coroutine list or guessed restart behavior enters Simulation.

## Batch 3: Randomness Oracle

### Task 4: B-007 legacy Main.rand ordering oracle

**Purpose:** Determine whether exact global random call-order parity is measurable. Domain-scoped
random streams remain the supported route when it is not.

**Files:**

- Read: legacy `Main.rand` initialization and source call sites in selected server paths
- Read: current random stream snapshot/persistence owner
- Create: `docs/research/2026-08-22-main-rand-ordering-oracle.md`
- Create: `Build/diagnostics/main-migration/task-7-main-rand-ordering-oracle/<run-id>/scenario.yaml`
- Modify: `progress.md` and the coverage matrix

**Steps:**

1. Select one bounded server path and instrument source-side random calls without changing behavior.
2. Capture seed, call index, consumer identity, and output for two deterministic replays.
3. Compare the trace with the ECS domain stream and document any irreducible global ordering.
4. Add no compatibility shim when the source trace cannot be reproduced.

**Acceptance:** Either a replayable oracle is accepted for the bounded path, or B-007 remains deferred
with a concrete unavailable-oracle statement.

## Batch 4: Lifecycle and Branch Inventory

### Task 5: Entity/event lifecycle gap matrix

**Purpose:** Close accounting gaps without claiming complete lifecycle parity.

**Files:**

- Read: legacy NPC/projectile/item/player lifecycle and event branches
- Modify: `docs/research/2026-08-20-main-member-coverage-matrix.md`
- Create: `docs/research/2026-08-22-entity-event-lifecycle-gap-matrix.md`
- Create: `Build/diagnostics/main-migration/task-13-entity-event-lifecycle-gap/<run-id>/scenario.yaml`
- Modify: `progress.md`

**Steps:**

1. Enumerate lifecycle transitions, persistence points, replication points, and client-only branches.
2. Map each transition to an existing owner or a named future card.
3. Add focused accounting assertions for missing owner and unsupported branch states.
4. Keep full static tables, random starts, AI families, and client/presentation branches explicitly
   open until their source evidence is recovered.

**Acceptance:** Every known server-relevant branch is `accepted`, `planned`, `deferred`, or
`excluded`; none is silently omitted.

## Required Verification Per Batch

```powershell
dotnet run --project <focused-verifier> -c Release `
  -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false --no-restore
dotnet run --project Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj `
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false --no-restore
git diff --check -- <changed paths>
```

Do not run root Release, full regression, or client suite under the current reduced validation policy.

## Stop Conditions

Stop the current batch and report `blocked` when any required legacy source, seed/table, caller
identity, phase transition, persistence, restart, or client projection contract cannot be recovered.
Do not replace the missing fact with a default, fabricated table, arbitrary action queue, or global
random-order claim.

## Deferred Completion Boundary

This plan does not claim completion of M-001, M-014, M-024, B-007, M-007, M-008, M-009, complete
entity/event lifecycle, NPC/projectile/item tables, random starts, or client/presentation branches.
Completion requires independent source-backed cards and their evidence gates.
