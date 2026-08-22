# Remaining ECS Open Responsibilities Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Advance the remaining ECS migration blockers only where a source-backed owner,
observable contract, and focused verifier can be recovered, while preserving explicit deferred
boundaries for legacy behavior that lacks those facts.

**Architecture:** Treat M-001, M-014/M-024, and B-007 as independent authority investigations,
not as reasons to add generic Simulation infrastructure. Entity/event lifecycle work is split by
domain (player, NPC, projectile, item, world object), with typed commands and immutable snapshots
at each boundary. Client/presentation behavior stays excluded unless a server-observable state or
protocol projection is independently proven.

**Tech Stack:** .NET 10, C#, Arch ECS, immutable snapshots, typed command buffers, WLD V319
compatibility models, deterministic focused verifiers, PowerShell, Version4 source oracle.

---

## Operating Constraints

1. Preserve the dirty worktree and existing user-owned files. Never introduce a generic
   `InitializeAlmostEverything`, `Queue<Action>`, or `IEnumerator` scheduler to improve a status
   line.
2. Before any C# edit, read `AGENTS.md` and `约束/Google-CSharp-Style-Guide-约束.md`.
3. Every accepted card must cite a source anchor, unique owner, observable state transition,
   rejected/forged case, and focused GREEN evidence. Missing facts remain `unknown` or `deferred`.
4. Keep validation at the requested reduced scope: changed-card verifier, at most one directly
   affected verifier/build, `MainBoundary`, and scoped `git diff --check`. Do not run the root
   Release build, full regression matrix, client suite, or duplicate replay matrix.
5. Use fresh evidence under `Build/diagnostics/main-migration/<card>/<run-id>/` and use
   `-p:UseSharedCompilation=false -p:MSBuildNodeReuse=false` for serial .NET commands.

## Task 1: Qualify an Independent M-001 Initializer Family

**Files:**

- Read: `docs/research/2026-08-22-initialize-almost-everything-owner-matrix.md`
- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:3732-3859`
- Create: `Build/diagnostics/main-migration/task-10-initializer-family-qualification/<run-id>/scenario.yaml`
- Modify only if a family qualifies: its existing domain owner and focused verifier

**Step 1: Enumerate candidates.** Consider only unresolved server/domain rows: tile entities,
torch definitions, leashed entities, NPC interactions, fish rules, pylons, shops/travel shops,
armor sets, WorldGen hooks, item repair, and chat projection.

**Step 2: Apply the qualification gate.** A candidate must have exactly one owner, bounded source
calls, an observable state, a falsifiable invalid/duplicate case, and a persistence or protocol
effect when the source survives restart or crosses a session boundary.

**Step 3: Record RED or rejection.** Add the source line, owner decision, missing predicate, and
   rejected case to the scenario card. If no candidate passes, record a cardinality/ownership
   discovery result and leave M-001 `planned`.

**Step 4: Implement only a qualifying child.** Add one named definition or typed command under
   the existing domain; never add an aggregate initializer facade.

**Step 5: Verify.** Run the child verifier if implemented, then
   `dotnet run --project Test/Terraria.Dome.MainBoundary.Verification/Terraria.Dome.MainBoundary.Verification.csproj`
   with the required MSBuild properties and scoped diff checks.

## Task 2: Recover Caller-Specific M-014 Main-Thread Contracts

**Files:**

- Read: `docs/research/2026-08-23-main-thread-action-contracts.md`
- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs:10529,26103-26125`
- Create only after recovery: a contract under the owning Server or WorldGeneration domain
- Create only after recovery: one focused owner verifier and scenario card

**Step 1:** Inventory each named caller and its phase, identity, ordering, cancellation, retry,
restart, persistence, and protocol visibility.

**Step 2:** Reject the arbitrary `Action` shape with a typed-contract RED case. The section-loaded
caller remains host/client state; the background WorldGen continuation remains WorldGeneration or
Server state unless a source-backed Simulation value is proven.

**Step 3:** Implement one caller-specific contract only if all required fields are source-backed.
Otherwise retain M-014 as `deferred` and document the first missing field.

**Step 4:** Verify acceptance/rejection and restart semantics with the changed-card verifier and
`MainBoundary` only.

## Task 3: Recover Caller-Specific M-024 Delayed-Process Contracts

**Files:**

- Read: `docs/research/2026-08-23-delayed-process-contract-matrix.md`
- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:242-244,11629-11636,11733-11739`
- Create only after recovery: a typed state machine in the actual owning domain
- Create only after recovery: focused phase/pause/cancel/restart verifier

**Step 1:** Search for concrete `.Add` callers and record the absence as evidence when none exist.

**Step 2:** Require stable identity, deterministic ordering, phase, pause behavior, cancellation,
disconnect/restart continuation, and serialization before introducing any owner.

**Step 3:** If the public mutable lists remain the only source surface, keep M-024 `deferred`;
do not wrap them in a generic coroutine queue.

## Task 4: Build or Reject the B-007 Legacy Main.rand Oracle

**Files:**

- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:13739-13762`
- Read: `docs/plans/2026-08-20-simulation-random-stream-contract.md`
- Modify: `Build/diagnostics/main-migration/task-9-probabilistic-meteor-scheduling-boundary/<run-id>/scenario.yaml`
- Create only after oracle recovery: a named meteor eligibility/scheduling system

**Step 1:** Trace every `Main.rand` consumption before the meteor `Next(50)` branch, including
qualified and unqualified night-start paths.

**Step 2:** Prove seeded state restoration independently of client, UI, and WorldGen consumers.
Identify the first unrecoverable consumer if the trace cannot be closed.

**Step 3:** Implement automatic probability only with fixed-trace equivalence and persisted
mid-trace continuation. Keep the explicit typed meteor command as the supported path regardless.

**Step 4:** Verify scheduled/unscheduled traces, rejection, restart continuation, and
`MainBoundary`. Otherwise retain B-007 deferred.

## Task 5: Complete Entity and Event Lifecycle Slices in Dependency Order

**Files:**

- Read: `docs/research/2026-08-18-npc-migration-coverage.md`
- Read: `src/Terraria.Dome.Simulation/`
- Modify only in the named domain owner and its focused verifier

Execute separate cards in this order:

1. Player movement/input/lifecycle gaps.
2. NPC spawn, identity, AI, combat, death, and loot gaps.
3. Projectile motion, collision, damage, and termination gaps.
4. Item inventory, use, drop, stacking, and persistence gaps.
5. World object/tile command commits and section revisions.
6. Event lifecycle gaps such as invasion random starts and completion side effects.

Each card must include accepted, invalid/forged, duplicate, and deterministic replay cases. Add
persistence/restart or two-session/PVS assertions only when the behavior crosses that boundary.
Do not expand a narrow card into full NPC tables or client behavior.

## Task 6: Model Random Starts and Presentation Only with an Authority

**Step 1:** For every random start, identify a source oracle and an independent deterministic
domain stream before implementing. A probability statement without ordering/state evidence stays
deferred.

**Step 2:** Keep warning/chat, ambience, UI, rendering, and client branches excluded unless a
server-observable value and protocol projection are separately identified.

**Step 3:** Update the coverage matrix only for the accepted narrow child card, not for broad
parity claims.

## Task 7: Reduced Acceptance Review

**Files:**

- Modify: `docs/research/2026-08-20-main-member-coverage-matrix.md`
- Modify: `progress.md` only for verified status changes
- Create: `Build/diagnostics/main-migration/task-10-open-responsibilities-review/<run-id>/status.txt`

Reconcile each card with source anchor, owner, focused output, deferred branches, and exact status.
Run only the changed-card verifier, one affected verifier/build when required, `MainBoundary`, and
scoped `git diff --check`. Do not claim broad migration completion or a percentage from this review.

## Definition of Done

Every listed blocker has either an evidence-backed implementation path or an explicit, tested
deferred boundary. The migration remains incomplete until every server-relevant Main responsibility
has an independent source-backed owner and lifecycle parity is covered by separate accepted cards.
