# Task 0 ECS Migration Execution Protocol Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Establish the repeatable, evidence-backed execution protocol that drives every
subsequent Terraria server gameplay migration from legacy behavior to authoritative ECS state.

**Architecture:** Task 0 does not move gameplay itself. It freezes the behavior oracle and
defines the unit of work as a source-backed scenario card, then requires one authority path:
`input -> validation -> system -> deterministic command commit -> immutable snapshot -> projection`.
The Simulation assembly owns gameplay state; Server owns orchestration, persistence, and protocol
projection. A separate evaluator accepts a batch only from fresh artifacts, not from a changed
status table or an implementer's assertion.

**Tech Stack:** .NET 10, C#, Arch ECS, executable verifier projects, deterministic world
fixtures, PowerShell, Git source identity, `Terraria.Dome.sln`.

---

## Status and Scope

This protocol is the execution form of Task 0 in
[the main migration control manual](2026-08-19-main-server-ecs-migration-execution.md).
Task 0's original reference freeze and responsibility ledger are already accepted. This file
defines how an AI continues from that baseline without requesting an operator to diagnose ordinary
compile, test, or localized implementation failures.

The legacy source reference is:

```text
D:\TRbackup\Version4物理删除了某些文件
```

Its physical deletions are material evidence: a behavior without a readable source method,
recoverable non-deleted reference, recorded trace, or executable oracle is `unknown`. The AI must
not infer it from a class name, packet identifier, current placeholder, or an upstream Terraria
version with unverified semantic equivalence.

The current responsibility map is
[`docs/migrations/main-server-responsibility-ledger.md`](../migrations/main-server-responsibility-ledger.md).
The active task order and accepted-batch record remain in
[the main migration control manual](2026-08-19-main-server-ecs-migration-execution.md) and
[`progress.md`](../../progress.md). This document is additive: it does not convert a partial
behavior family into an accepted one.

## Non-Negotiable Boundaries

| Responsibility | Permitted owner | Prohibited shortcut |
|---|---|---|
| Gameplay state and rules | `Terraria.Dome.Simulation` | `Terraria.Main`, `Main.tile`, static global state |
| Mutation validation and ordering | Simulation commands and named systems | protocol handler or verifier writes to a Store directly |
| State publication | immutable Simulation snapshot | protocol reads mutable ECS state during mutation |
| Session, sockets, packet parsing, save I/O | `Terraria.Dome.Server` and protocol projects | those dependencies entering Simulation |
| Legacy source lookup | frozen oracle and recorded provenance | copying a legacy static class into ECS |
| Unknown source semantics | explicit deferred/unknown state | permissive default or guessed numeric threshold |

The required mutation route is:

```text
decoded client/server input
  -> owner/range/rate validation
  -> Simulation command queue
  -> named tick phase and domain system
  -> ordered Tile/Entity/World command commit
  -> immutable snapshot and revision
  -> Server replication or persistence projection
```

Every new behavior must name the owner at each arrow. A test-only callback, a mutable global
dictionary, or a Server-side mutation that bypasses the command queue is not an implementation of
this route.

## Scenario Card Contract

Each migration batch starts with exactly one machine-readable scenario card. Store the card and its
artifacts beneath a new run directory:

```text
Build/diagnostics/main-migration/task-<n>-<behavior>/<yyyyMMdd-HHmmss>/
```

The card must have all fields below before implementation begins:

```yaml
id: task-<n>-<behavior>
status: proposed | red | green | accepted | deferred | blocked
reference:
  path: D:\TRbackup\Version4物理删除了某些文件\Terraria\<file>.cs
  sha256: <source-file-hash>
  members:
    - <type.member and line range>
  alternative_oracle: <path/hash/trace, or null>
authority:
  input_owner: <protocol/server/scheduled source>
  simulation_owner: <command, system, and state type>
  commit_owner: <deterministic commit system>
  projection_owner: <snapshot/replication/persistence type>
fixture:
  seed: <fixed seed>
  world: <snapshot fixture or exact construction>
  sessions: <handles, ownership, visibility>
  initial_tick: <integer>
input_trace:
  - tick: <integer>
    sequence: <integer>
    session: <handle>
    input: <decoded command or raw packet bytes>
expected:
  accepted: <state, command, snapshot, and frame assertions>
  rejected: <invalid/forged assertion and unchanged-state assertions>
required_gates: [R0, R1, R2, R3]
deferred: <explicitly excluded source branches>
```

`alternative_oracle` may be populated only with evidence whose source/version relationship is
recorded. When it is null, the card must be marked `deferred` rather than producing gameplay.

## Gate Definitions

| Gate | Question | Minimum artifact | Required result |
|---|---|---|---|
| R0 Source identity | What exact tree was evaluated? | `worktree.txt`, source/reference hashes, SDK and config | fresh build identity; no newly changed source after final gate |
| R1 Deterministic replay | Is the Simulation result reproducible? | fixed seed/input trace and per-tick snapshot/command hashes | two clean runs produce identical ordered records |
| R2 Authority and rejection | Can invalid input change state? | accepted and forged/malformed/invalid traces | all invalid cases preserve relevant state/revisions |
| R3 Multiplayer loopback | Is replication scoped and ordered? | two-session frame trace and PVS decisions | eligible peer receives revision; ineligible peer does not |
| R4 Fault matrix | Does transport failure preserve authority and liveness? | delay/duplicate/reorder/truncate/slow-reader cases | bounded queues; failure scoped to affected session |
| R5 Persistence restart | Does recovery preserve or reject state safely? | save/reload/replay-continuation hashes | exact recovery or no mutation of live world |
| R6 Real-client smoke | Does the supported client flow work? | packet trace, server log, client log/crash log | entry, representative action, disconnect/reconnect explain cleanly |
| R7 Soak | Does a fixed workload remain bounded? | duration metrics and periodic hashes | no unbounded growth or state-hash drift |

R0 through R3 are the merge gate for a gameplay family. R4 through R7 are required only when the
card's risk profile says so; they cannot compensate for missing R1, R2, or R3.

## Task 1: Freeze a Single Behavior Family

**Files:**
- Read: `docs/plans/2026-08-19-main-server-ecs-migration-execution.md`
- Read: `docs/migrations/main-server-responsibility-ledger.md`
- Read: `progress.md`
- Read: the exact legacy source member and the current Simulation/Server consumer
- Create: `Build/diagnostics/main-migration/task-<n>-<behavior>/<runId>/scenario.yaml`
- Create: `Build/diagnostics/main-migration/task-<n>-<behavior>/<runId>/source-manifest.txt`

**Step 1: Select one observable behavior, not a legacy class**

Examples of valid units are “an active-above Type 1 actuator transition” or “world rain consumes
one deterministic duration unit.” “Migrate `Wiring.cs`” and “finish world generation” are invalid
because neither supplies a bounded acceptance predicate.

**Step 2: Record the oracle before changing code**

Record the source path, SHA-256, exact member line range, branches used by the card, and all
unresolved dependencies. If the physical deletion removes the member, search only approved
recoverable references and record their provenance. Do not continue with an invented behavior.

**Step 3: Write accepted and rejected outcome assertions**

For every acceptance assertion, include at least one rejection: forged ownership, invalid range,
duplicate sequence, invalid state, or a legacy branch that forbids the action. Each rejected input
must assert both value state and relevant revision/command count stay unchanged.

**Step 4: Capture pre-edit identity**

Run the preflight script in Task 5 and save its outputs. Treat unrelated dirty files as recorded
ambient state; never reset, format, or claim ownership of them.

**Step 5: Commit**

Do not commit user-owned changes. If the branch is intentionally used for commits, stage only the
scenario card and narrowly owned source/test files after an accepted batch.

## Task 2: Establish a Real RED

**Files:**
- Test: the smallest existing `Test/Terraria.Dome.*.Verification/Program.cs` matching the domain
- Create/modify: only a verifier that exercises the exact scenario command path
- Create: `Build/diagnostics/main-migration/task-<n>-<behavior>/<runId>/focused-red.txt`

**Step 1: Add the failing verifier assertion**

Construct an isolated world, fixed random state, explicit player/session ownership, and deterministic
sequence. Exercise the public command or decoded input route, tick through the named phase, then
assert snapshot and emitted commands/frames. Do not call an internal system solely to avoid a
missing authority path.

**Step 2: Run the verifier before implementation**

```powershell
dotnet run --project Test\Terraria.Dome.<Domain>.Verification\Terraria.Dome.<Domain>.Verification.csproj `
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

Expected: non-zero exit caused by the missing behavior or a precise wrong result. Save the complete
output as `focused-red.txt`. A compile failure is a legitimate RED only when its missing symbol is
the intended API; an unrelated failure must be repaired or recorded separately first.

**Step 3: Classify the failure before editing**

| Failure class | AI action |
|---|---|
| Intended behavior absent | implement the smallest authority path |
| Test calls a stale/non-public shape | correct the test to public contract, preserving the behavior predicate |
| Build-output contention | rerun serially with required MSBuild properties |
| Existing unrelated regression | record it; do not weaken its assertion or fold it into this batch |
| Missing or contradictory oracle | mark the card deferred and request an architecture decision |

**Step 4: Commit**

The RED test remains part of the final verifier. Never delete a rejection assertion simply because
the first implementation makes it inconvenient.

## Task 3: Implement the Narrow Authoritative Path

**Files:**
- Modify: the domain command, validation point, named Simulation system, commit system, and snapshot
  projection only when each is required by the scenario card
- Modify: Server adapter or persistence format only when the card requires a real external boundary
- Test: the focused verifier from Task 2

**Step 1: Add or reuse a typed command**

The command carries only validated intent, stable identities, sequence/tick data and values needed by
the domain. It must not carry a mutable Protocol object, `Entity`, socket, `Main` reference, or
filesystem handle.

**Step 2: Validate before enqueueing**

Validate session ownership, numeric range, duplicate/rate policy, current-state preconditions and
visibility where appropriate. Invalid input returns a controlled rejection and leaves Simulation
state untouched.

**Step 3: Apply in the documented tick phase**

The named domain system consumes the command in the phase prescribed by the main control manual.
It produces ordered domain commands or an immutable next state. Do not mutate world/entity state
from the protocol handler or during replication.

**Step 4: Commit deterministically**

Use an explicit, tested ordering key. For tile commands this normally includes sequence and stable
coordinates/kind; for entity commands it includes sequence and stable replication identity. The
snapshot is published only after all accepted mutations commit.

**Step 5: Preserve evolution boundaries**

If the behavior changes persistent state, append version-gated fields, increment the format version,
and add an old-layout reader fixture. Never reinterpret old bytes as a new field. If no authoritative
legacy value exists in old snapshots, restore it as unavailable/unknown, not as a guessed default.

**Step 6: Commit**

Keep the change confined to the selected behavior family. A request to fix unrelated warnings,
refactor all Systems, or copy a definition table is a separate card.

## Task 4: Prove GREEN and the Required Boundaries

**Files:**
- Test: focused domain verifier
- Test: relevant loopback, persistence, protocol, and Main boundary verifiers
- Create: one text artifact per command under the current batch run directory

**Step 1: Run the focused GREEN**

Re-run the Task 2 command after implementation. It must prove both accepted and rejected routes.
Save `focused-final.txt`.

**Step 2: Run deterministic replay twice (R1)**

Run the exact scenario from a clean process twice. Save state hash and command-order records for
each execution; compare them byte-for-byte or with a verifier assertion. A single run is not R1.

**Step 3: Run authority and multiplayer checks (R2/R3)**

Use the domain's verifier and, when it crosses the Server boundary, its loopback verifier. The
trace must show the rejected input did not mutate authority and that PVS/session eligibility governs
outbound revisions.

**Step 4: Run only affected regressions**

Examples: a tile mutation includes Wiring/Liquid/Chest loopback and Persistence; player movement
includes PlayerPhysics and Player lifecycle loopback; a world rule includes WorldRules and
Persistence. Do not use a broad solution build as a substitute for an absent domain assertion.

**Step 5: Run the hard boundary gate and root Release build**

```powershell
dotnet run --project Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj `
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false

dotnet build Terraria.Dome.sln -c Release -m:1 `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:FixtureHostBuild=false
```

Expected: each command exits `0`. Record warnings and errors exactly. Any source change after a
gate invalidates all later acceptance evidence and requires rerunning the chain from focused GREEN.

**Step 6: Commit**

Before a commit or acceptance status, run `git diff --check` and save its output. The repository
may have pre-existing line-ending notices; use the command exit status, not visual noise, as the
format gate result.

## Task 5: Produce an Independent Acceptance Bundle

**Files:**
- Create: `Build/diagnostics/main-migration/task-<n>-<behavior>/<runId>/`
- Modify: `progress.md`
- Modify: `docs/plans/2026-08-19-main-server-ecs-migration-execution.md` only when the task status
  or accepted scope changes

**Step 1: Capture a fresh bundle**

Run this from the repository root, replacing `<task>` and `<behavior>` with the scenario ID. Add
only the affected verifier commands to the indicated section.

```powershell
$runId = Get-Date -Format "yyyyMMdd-HHmmss"
$evidence = "Build\diagnostics\main-migration\task-<task>-<behavior>\$runId"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null

git status --short | Tee-Object "$evidence\worktree.txt"
if ($LASTEXITCODE -ne 0) { throw "git status failed: $LASTEXITCODE" }

git diff --check | Tee-Object "$evidence\diff-check.txt"
if ($LASTEXITCODE -ne 0) { throw "git diff --check failed: $LASTEXITCODE" }

Get-FileHash -Algorithm SHA256 `
  "D:\TRbackup\Version4物理删除了某些文件\Terraria\<file>.cs" |
  Tee-Object "$evidence\reference-hash.txt"
if ($LASTEXITCODE -ne 0) { throw "reference hash failed: $LASTEXITCODE" }

function Invoke-RecordedDotnet {
  param(
    [Parameter(Mandatory)]
    [string] $name,
    [Parameter(Mandatory)]
    [string[]] $arguments)

  & dotnet @arguments 2>&1 | Tee-Object "$evidence\$name.txt"
  $exitCode = $LASTEXITCODE
  if ($exitCode -ne 0) {
    throw "dotnet $name failed: $exitCode"
  }
}
```

**Step 2: Save the required artifacts**

The final directory contains at least `scenario.yaml`, `source-manifest.txt`, `worktree.txt`,
`diff-check.txt`, `focused-red.txt` or an accurate historical RED reference, `focused-final.txt`,
R1 run/comparison records, affected regression outputs, `main-boundary-final.txt`, and
`root-release-build-final.txt`. Add packet/state logs and persistence fixtures for cards that need
R3/R5/R6.

**Step 3: Hand off to a separate evaluator**

The implementer may state `green`, but cannot state `accepted`. The evaluator checks that source,
tests, hashes, and artifacts describe the same post-change tree; validates the card's deferred
branches; and confirms every required gate has a fresh zero exit code.

**Step 4: Update the facts, not a score**

Append a concise `progress.md` entry with: source member/hash, precise accepted predicate,
command/snapshot authority route, gate commands and exit codes, evidence directory, deferred
branches, and the next scenario card. Do not raise broad completion percentage because one slice
passed.

**Step 5: Commit**

Commit only task-owned documentation and source after independent acceptance. Preserve unrelated
dirty worktree state exactly as found.

## Task 6: Autonomous Repair and Escalation Policy

**Files:**
- Read: current scenario card, focused failure output, first compiler/test root cause
- Modify: only files owned by the current card
- Update: scenario card and `progress.md` after each resolved or deferred batch

**Step 1: Repair ordinary failures autonomously**

The AI continues without asking for guidance when it can prove a local cause: missing import,
signature mismatch, wrong constructor/update path, deterministic ordering defect, missing snapshot
projection, stale test expectation, or serial-build contention. It changes one causal boundary at a
time and re-runs the smallest failing verifier first.

**Step 2: Do not create false GREEN**

The AI must not disable an assertion, use `--no-build` after editing source, add a permissive
fallback for unavailable facts, mark a verifier unsupported merely to hide a failure, or repair a
different domain to make the selected test pass.

**Step 3: Escalate only an actual decision**

Ask the operator for a decision only when one of these is evidenced:

1. No usable behavior oracle exists after the approved source search.
2. Two recoverable oracles contradict at an observable branch.
3. The requested behavior requires importing `Main`, client/UI, socket, protocol, or disk behavior
   into Simulation.
4. It changes a public wire/save contract and no compatible versioning choice exists.
5. Three attempts found the same root cause with no new evidence.

The escalation must include the card, source paths/hashes, exact failure point, alternatives, and
the smallest decision required. “How should I fix this?” is not an acceptable escalation.

## Acceptance Checklist

A card is `accepted` only when all relevant statements are true:

- [ ] Source/oracle provenance and explicit line/member scope are recorded.
- [ ] The behavior has an observable, falsifiable accepted and rejected predicate.
- [ ] A real RED was recorded or accurately linked to a prior exact RED.
- [ ] The code follows the command/validation/system/commit/snapshot authority route.
- [ ] No prohibited dependency or direct Server/protocol mutation entered Simulation.
- [ ] R0, R1, R2, and R3 evidence is fresh for the final source tree.
- [ ] Relevant persistence/protocol/loopback gates ran where the behavior crosses those boundaries.
- [ ] Main boundary and serial root Release gates exit `0`.
- [ ] `git diff --check` exits `0` after document updates.
- [ ] `progress.md` states accepted scope and every deferred source branch.
- [ ] An evaluator separate from the implementation pass checked the bundle.

If any check is false, status is `green`, `partial`, `deferred`, or `blocked`; it is not accepted.

## First Cards After Task 0

The next work must be selected from the active main plan, not from this protocol. Use its ordered
Task 7-11 backlog and create one card at a time. The currently active Task 8 work illustrates the
required granularity: an exact legacy `Wiring.DeActive` branch may be migrated as one card, while
the unrecovered Type 226 world-surface dependency remains an explicitly deferred or separate
metadata card. It is not valid to label all wiring, all tile solidity, or all `Main.cs` logic
complete from that result.

Plan complete and saved to
`docs/plans/2026-08-20-task-0-ecs-migration-execution.md`. Execution should proceed one scenario
card at a time under this protocol, with an independent acceptance pass after each batch.
