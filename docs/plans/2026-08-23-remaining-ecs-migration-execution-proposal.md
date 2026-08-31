# Remaining ECS Migration Execution Plan


**Goal:** Close the remaining source-backed Terraria server ECS migration gates without guessing legacy behavior or deleting legacy ownership before reproducible evidence exists.

**Architecture:** `Terraria.Dome.Simulation` remains the authoritative owner of deterministic gameplay state, snapshots, typed commands, and commits. Server projects own WLD I/O, persistence orchestration, session state, and protocol projection. Legacy `Main`, client, transport, and presentation behavior stays outside Simulation; unsupported or source-ambiguous behavior fails closed.

**Tech Stack:** .NET 10, C#, Arch ECS, immutable snapshots, deterministic replay fixtures, standalone focused verifiers, source inventories, and JSON/trace evidence under `Build/diagnostics`.

---

## Current Baseline

This plan starts from a dirty worktree. Preserve unrelated user changes and remeasure every gate before attributing a result to a task.

- Full WorldGen differential: `5,040,000` tiles compared, `3,190,404` tile mismatches, and `1,046,843` extended-state mismatches. The authoritative gate is `docs/worldgen/worldgen-deletion-gate.json`; `canRemoveLegacyWorldGen` must remain `false`.
- Physical deletion ledger: `docs/migrations/version4-physical-deletion-ledger.csv` contains `44` `ServerRelevant` rows that still lack source-backed replacement evidence. Classification completeness is not replacement evidence.
- Legacy `Main.rand` ordering: deterministic ECS random streams exist, but there is no executable legacy call-order/call-site/argument/result trace proving global consumption equivalence.
- Static and presentation coverage: NPC/projectile/item definition tables, all AI/type branches, client/presentation branches, complete event effects, and broad two-session PVS evidence remain incomplete.
- Focused verifier passes and MainBoundary success are necessary evidence only; they do not satisfy the large gates above.

Authoritative inputs:

- `docs/plans/2026-08-23-remaining-ecs-migration-summary-and-todos.md`
- `docs/worldgen/worldgen-deletion-gate.json`
- `docs/worldgen/worldgen-source-inventory.json`
- `docs/migrations/version4-physical-deletion-ledger.csv`
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/current-full-differential/`
- `Build/diagnostics/main-migration/`

## Non-Negotiable Execution Rules

1. Before any C# edit, read `AGENTS.md`, `约束/Google-CSharp-Style-Guide-约束.md`, the exact legacy source member, and the current target type.
2. Use `apply_patch`; do not revert or overwrite unrelated dirty files.
3. Run commands from `D:\TRbackup\NLTX` with `-p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -m:1` for serial .NET work.
4. Every task writes fresh evidence to `Build/diagnostics/<area>/<run-id>/` containing source hashes/lines, scenario, command, exit code, and accepted/deferred conclusion.
5. A green focused test proves only its covered behavior. Do not convert a focused pass into a migration-complete claim.
6. If the source oracle, runtime trace, or identity contract is absent, mark the branch deferred and stop implementation for that branch.
7. Do not physically delete legacy files until the deletion gate says every affected row has replacement provenance and the complete differential/acceptance gates pass.
8. Do not create a per-card Markdown progress file. Keep card evidence in diagnostics and produce one final acceptance record only at the end.

## Phase 0: Freeze a Fresh Baseline

### Task 0.1: Capture worktree and gate state

**Files:**

- Read: `AGENTS.md`
- Read: `docs/worldgen/worldgen-deletion-gate.json`
- Read: `docs/migrations/version4-physical-deletion-ledger.csv`
- Create: `Build/diagnostics/server-ecs-convergence/P0-baseline/<run-id>/baseline.md`
- Create: `Build/diagnostics/server-ecs-convergence/P0-baseline/<run-id>/git-status.txt`

**Steps:**

1. Run `git status --short` and record the existing write set.
2. Copy current WorldGen differential, deletion-gate, source-inventory counts, and ledger counts into `baseline.md`.
3. Run the Simulation Release build and MainBoundary verifier.
4. Run `git diff --check` and record warnings separately from failures.

**Expected result:** baseline artifacts are fresh; no code is changed. Any build failure outside the active write set blocks later phases.

## Phase 1: Recover Missing Oracles Before Implementation

### Task 1.1: Build an executable legacy `Main.rand` trace

**Files:**

- Read: Version4 legacy `Main`/WorldGen source referenced by `docs/worldgen/worldgen-source-inventory.json`
- Read: `src/Terraria.Dome.Simulation/WorldGeneration/LegacyPassRandomState.cs`
- Read: `src/Terraria.Dome.Simulation/World/WorldEventRandomState.cs`
- Create: `Build/diagnostics/server-ecs-convergence/P1-random-oracle/<run-id>/legacy-rand-trace.jsonl`
- Create: `Build/diagnostics/server-ecs-convergence/P1-random-oracle/<run-id>/oracle-manifest.json`

**Steps:**

1. Identify the exact legacy random wrapper and every supported call site for one bounded generation/event scenario.
2. Instrument or replay the legacy runtime to emit sequence number, call site, overload, arguments, result, branch context, and seed.
3. Run the same seed/scenario twice and assert byte-identical traces.
4. Compare the trace with the ECS stream checkpoint/replay output.

**Exit criteria:** accept only when call ordering and arguments are executable and repeatable. Otherwise record `deferred: missing legacy runtime oracle`; do not alter random consumption order.

### Task 1.2: Produce source-backed ownership mappings

**Files:**

- Read: `docs/migrations/version4-physical-deletion-ledger.csv`
- Read: `docs/worldgen/worldgen-method-map.md`
- Create: `Build/diagnostics/server-ecs-convergence/P1-ownership/<run-id>/replacement-evidence.json`

**Steps:**

1. For each of the 44 `ServerRelevant` rows, record source member/line, authoritative ECS owner, state or command route, persistence/protocol consequence, and focused evidence path.
2. Reject rows whose proposed owner is inferred only from a name, partial verifier, or reduced source tree.
3. Recompute the ledger summary and list unresolved rows explicitly.

**Exit criteria:** a row is eligible for replacement only with complete source provenance and a falsifiable test; otherwise it remains deferred.

## Phase 2: Close WorldGen Differential Parity

### Task 2.1: Reproduce the failing differential

**Files:**

- Read: `Build/diagnostics/server-ecs-convergence/P9-worldgen/current-full-differential/trace.txt`
- Read: `Build/diagnostics/server-ecs-convergence/P9-worldgen/current-stage-trace/stage-fingerprints.json`
- Read: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationPipeline.cs`
- Test: existing `Test/Terraria.Dome.WorldGeneration.Verification/`
- Create: `Build/diagnostics/server-ecs-convergence/P2-worldgen/<run-id>/differential.json`

**Steps:**

1. Run the complete legacy/ECS comparison from a clean process using a fresh output directory.
2. Partition mismatches by stage, tile value, extended state, coordinate region, and first divergent random checkpoint.
3. Confirm whether the first divergence is attributable to a mapped source predicate, random order, unsupported branch, or missing side effect.

**Expected result:** a reproducible failure classification, not a reduced pass claim.

### Task 2.2: Implement one source-backed parity slice at a time

**Files:**

- Modify only the ECS target files named by the mismatch classification.
- Test: `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`
- Create: `Build/diagnostics/server-ecs-convergence/P2-worldgen/<run-id>/slice-evidence.json`

**Steps:**

1. Add a failing focused case tied to an exact legacy member and seed.
2. Implement the smallest typed system/query/command change that preserves immutable snapshot and commit boundaries.
3. Run the focused verifier, Simulation build, MainBoundary verifier, and the complete differential.
4. Keep the change only if the complete comparison improves without introducing unrelated divergence; otherwise revert only the attempted patch through `apply_patch` and retain the failure evidence.

**Exit criteria:** `mismatchTiles == 0`, extended state parity passes, and the deletion gate independently reports removable. Until then, WorldGen remains blocked.

## Phase 3: Replace Server-Relevant Legacy Responsibilities

### Task 3.1: Migrate a ledger row only with a complete route

**Files:**

- Modify: the exact Simulation/Server owner identified in `replacement-evidence.json`
- Test: the owner domain's focused verifier and loopback verifier when applicable
- Modify: `docs/migrations/version4-physical-deletion-ledger.csv` only after evidence passes

**Steps:**

1. Add RED coverage for accepted, forged, duplicate, invalid-order, and identity-boundary inputs.
2. Implement the typed state transition and deterministic command commit.
3. Add persistence/protocol projection evidence if the legacy member had either consequence.
4. Run focused tests, affected build, MainBoundary, and scoped `git diff --check`.
5. Update the ledger row with source/member lines, owner, evidence path, and status.

**Exit criteria:** no row is marked replaced by classification alone. Rows without exact source-backed evidence remain `ServerRelevant` and deferred.

### Task 3.2: Complete contract families before broad deletion

Prioritize the remaining WorldGen builders and mutation responsibilities: CaveHouse, desert/surface maps, corruption/spike pit builders, dungeon features/settings/rooms/shapes, generation actions/shapes/pass, `FlexibleTileWand`, `MinecartDiggerHelper`, `ShimmerHelper`, `TileFont`, and `SimpleStructure`. For each, first recover the source contract, then assign one owner; do not create generic `Manager`, `Helper`, or aggregate initializer abstractions.

## Phase 4: Complete Entity Tables and Lifecycles

### Task 4.1: Establish complete static definitions

**Files:**

- Read: legacy NPC/projectile/item definition sources and current inventories
- Modify: `src/Terraria.Dome.Simulation/Npc/Definitions/`, `Projectile/`, and `Items/Definitions/`
- Test: `Test/Terraria.Dome.Npc.Verification/`, `Test/Terraria.Dome.Combat.Verification/`, `Test/Terraria.Dome.Items.Verification/`

**Steps:**

1. Generate a source-indexed table of IDs, defaults, supported branches, and intentionally unsupported branches.
2. Add validation for unknown IDs, non-finite values, duplicate identity, invalid ordering, and exhausted revisions.
3. Verify deterministic spawn, AI/update, damage/death, loot/drop, persistence, and replication behavior for every accepted family.

**Exit criteria:** every accepted ID has source provenance and focused evidence; unknown or unsupported IDs fail closed.

### Task 4.2: Prove persistence and two-session projection

**Files:**

- Read/modify: `src/Terraria.Dome.Server/Protocol/`
- Test: existing entity loopback and persistence verifiers
- Create: `Build/diagnostics/server-ecs-convergence/P4-entities/<run-id>/two-session-pvs.json`

**Steps:**

1. Start two deterministic sessions with independent identities.
2. Apply competing commands and record ordered snapshots, revisions, visibility decisions, and projected packets.
3. Restart from persisted state and compare hashes and rejection outcomes.

**Exit criteria:** broad PVS/replication coverage spans all accepted entity families; partial loopback evidence does not satisfy this gate.

## Phase 5: Client/Presentation and World-Event Boundaries

### Task 5.1: Inventory client-only and shared branches

**Files:**

- Read: `docs/research/2026-08-20-main-member-coverage-matrix.md`
- Read: `docs/server-completion/capability-matrix.md`
- Create: `Build/diagnostics/server-ecs-convergence/P5-presentation/<run-id>/branch-inventory.json`

**Steps:**

1. Classify each branch as Simulation authority, Server projection, Client presentation, or unsupported.
2. Prove that client/UI/audio/input/asset branches are absent from Simulation through MainBoundary and dependency scans.
3. Add protocol/presentation verification only for server-observable effects.

**Exit criteria:** all accepted server-observable effects have projection evidence; client-only behavior remains explicitly outside the migration target.

### Task 5.2: Close event lifecycle slices

Use exact source predicates for weather, slime rain, invasion, meteor, wind, and Lantern Night. Persist only fields with exact WLD meaning. Random starts, NPC spawn tables, announcements, and dynamic travel authority remain deferred until their source and random-order oracles exist.

## Phase 6: Final Acceptance Gate

### Task 6.1: Run the full acceptance matrix

**Files:**

- Read: all phase evidence manifests
- Create: `Build/diagnostics/server-ecs-convergence/P-final/<run-id>/acceptance.json`
- Create: `Build/diagnostics/server-ecs-convergence/P-final/<run-id>/acceptance.md`

**Steps:**

1. Re-run the complete WorldGen differential and deletion gate.
2. Recompute the physical-deletion ledger and require zero unresolved `ServerRelevant` rows.
3. Run every accepted entity/event verifier, affected Release builds, MainBoundary, persistence replay, and broad two-session PVS.
4. Verify every objective in `docs/plans/2026-08-23-remaining-ecs-migration-summary-and-todos.md` has accepted evidence or a source-backed unavailable decision.
5. Generate exactly one final acceptance record containing source hashes, accepted responsibilities, evidence paths, deferred branches, command exit codes, and remaining risk.

**Expected result:** only this gate may use completion language. Any failed large gate leaves the migration status `incomplete` and preserves the legacy source.

## Verification Command Set

Use fresh output paths for every run:

```powershell
dotnet build src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -c Release -m:1 -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false --no-restore
dotnet run --project Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false --no-restore
dotnet run --project Test\Terraria.Dome.WorldGeneration.Verification\Terraria.Dome.WorldGeneration.Verification.csproj -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false --no-restore
git diff --check
```

For entity, event, persistence, or protocol changes, add the affected focused verifier and the two-session replay. Do not substitute the focused command set for the complete WorldGen, deletion, static-table, or PVS gates.

## Definition of Done

This plan is complete only when the final acceptance record proves all seven objective requirements in the remaining-work summary, the WorldGen deletion gate is removable, all 44 physical-deletion rows have source-backed replacement evidence, a legacy `Main.rand` ordering oracle exists or every dependent branch is explicitly source-backed unavailable, complete accepted static tables and lifecycles are covered, and broad two-session PVS/client-boundary evidence is reproducible. Until then, report partial progress and keep unresolved gates visible.

Plan complete and saved to `docs/plans/2026-08-23-remaining-ecs-migration-execution-proposal.md`. Two execution options:

1. **Subagent-Driven (this session)** - dispatch a fresh subagent per task, review between tasks, and iterate with checkpoints.
2. **Parallel Session (separate)** - open a new session with executing-plans and execute in batches with checkpoints.

Choose the execution mode before implementation begins.
