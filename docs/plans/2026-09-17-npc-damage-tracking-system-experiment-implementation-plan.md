# NPC Damage Tracking System Experiment Implementation Plan

> Continuation scope: do not add test code or run tests. This plan records the
> implementation boundary and the build-only evidence collected for the
> experiment.

**Goal:** Validate a world/session-scoped ECS system boundary for the P12/C17 `NpcDamageRuntimeTracking` slice while preserving ordered credit and lifecycle invariants.

**Architecture:** `NpcDamageTrackingSystem` owns active/recent registries and is the only system that starts, stops, advances, resets, or projects encounter trackers. `NpcDamageEncounterTracker` reuses `EncounterDamageCreditComponent`; explicit single/composite strategies isolate dynamic target grouping. The prototype accepts only already-resolved damage and has no network, persistence, random, or external-clock dependency.

**Tech Stack:** C# `net10.0`, repository serial .NET wrapper, existing `src/Combat` and `src/Npc` project boundaries.

---

### Task 1: Lock The P12/C17 Boundary

**Files:**
- Create: `docs/plans/2026-09-17-npc-damage-tracking-system-experiment-design.md`
- Read-only evidence: `docs/组件文档/迁移参考表/Version4权威模拟系统字段属性逐成员源码声明-20分区/P12-NPC-Combat-Network-Damage.md`
- Read-only source: `D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs`

**Step 1: Record the source-backed scope**

Record P12 session identity, the twelve C17 members, Version4 source hash,
active/recent semantics, contributor order, strict expiry boundary, and the
invasion-strategy evidence gap. Explicitly exclude the P12 ledger, Dome copy,
network, persistence, and legacy deletion.

**Step 2: Compare keep, partial, and separate**

Choose a separate system plus per-encounter tracker because the registry,
clock, lifecycle, dynamic strategy, and snapshot seam are independently
reviewable. Keep the existing credit component as the canonical component.

### Task 2: Focused Verifier (Explicitly Excluded)

No verifier project or test source is added in this continuation. The state
transition cases remain a follow-up evidence gap because the user requested
that test writing and test execution stop.

### Task 3: Add Controlled Component Mutation

**Files:**
- Modify: `src/Combat/EncounterDamageCreditComponent.cs`

**Step 1: Add the smallest controlled mutation**

Add `TryAddCredit` with positive-amount, active-lifecycle, and monotonic-hit
validation. Preserve first-seen contributor order, aggregate world damage, and
update last contributor/time/revision atomically. Make `Credits` a defensive
copy so mutable `DamageCreditEntry` values cannot leak through a read-only
wrapper.

### Task 4: Implement The Strategy And Encounter Seams

**Files:**
- Create: `src/Npc/INpcDamageTrackingStrategy.cs`
- Create: `src/Npc/NpcDamageSingleTypeStrategy.cs`
- Create: `src/Npc/NpcDamageCompositeStrategy.cs`
- Create: `src/Npc/NpcDamageEncounterTracker.cs`
- Create: `src/Npc/NpcDamageTrackerSnapshot.cs`

**Step 1: Implement explicit strategy policy**

Implement single-type and composite inclusion/activity checks and a mutable
killed flag driven only by `OnNpcKilled`.

**Step 2: Implement per-encounter ownership**

Wrap one `EncounterDamageCreditComponent`, expose no mutable collection, and
provide internal lifecycle/credit methods and defensive snapshot creation.

### Task 5: Implement The Registry Owner System

**Files:**
- Create: `src/Npc/NpcDamageTrackingSystem.cs`
- Modify: `src/Npc/Terraria.Npc.csproj`

**Step 1: Implement the system API**

Use a constructor-injected strategy factory, world/session-local encounter IDs,
explicit current ticks, active/recent lists, and constants `3` and `54000`.
Implement `TryRecordAppliedDamage`, `StartTracking`, `MarkKilled`,
`StopTracking`, `AdvanceTo`, reset, and snapshot projection. Reject invalid
clock order and contributor provenance. Transfer only non-empty encounters to
recent history; cap by dropping the oldest and retain at least one during
expiry.

**Step 2: Add the one-way project dependency**

Reference `src/Combat/Terraria.Combat.csproj` from the NPC project. Do not add
a reverse Combat-to-NPC reference and do not include `dome/dome1` in the root
project.

### Task 6: Build-Only Project Verification

**Files:**
- Build: `src/Npc/Terraria.Npc.csproj`

**Step 1: Build the affected NPC project**

Run a serial `build` for `src/Npc/Terraria.Npc.csproj` with the mandated
MSBuild properties. If unrelated dirty source blocks it, record exact errors
and keep source/build evidence separate. Do not run a test or verifier command.

### Task 7: Document, Commit, Merge, And Remove The Experiment Branch

**Files:**
- Create/update: `docs/第一轮审查/报告/2026-09-17-npc-damage-tracking-system-experiment-optimization-report.md`
- Update: this plan with actual command results

**Step 1: Write the optimization report**

Report ownership, read/write/emit sets, phase/barrier, clock/random/I/O,
keep/partial/separate comparison, rollback, provenance and dynamic-strategy
gaps, focused-versus-runtime evidence, and the deletion-gate decision.

**Step 2: Commit only experiment-owned paths**

Use explicit `git add <path>` for the design, plan, tracker source, component
change, and report. Do not stage existing user changes,
ledger/lock files, or generated output.

**Step 3: Fast-forward the experiment commit into `main`**

Confirm the commit contains only the explicit experiment paths, switch to
`main`, and fast-forward to the experiment commit while preserving all dirty
working-tree changes.

**Step 4: Delete the disposable branch**

After confirming the commit is reachable from `main`, delete
`codex/npc-damage-tracking-system-experiment-20260917` with `git branch -d`.
Record the final branch name, reachability, branch absence, and preservation
of pre-existing dirty changes.

### Execution Record

The experiment was executed on `codex/npc-damage-tracking-system-experiment-20260917`,
based on the current `main` commit. P12 remained untouched; its completed ledger
still reported `189/189` members and session
`dec7d02830c14d7bbba328dc0317cef3`.

- No test code or focused verifier was added, and no test command was run.
- The first direct wrapper invocation with unquoted `-p:` arguments exited `1`
  before dotnet execution because PowerShell treated `-p` as an ambiguous
  wrapper parameter. The required argument-array form was then used.
- Production closure: serial build of `src/Npc/Terraria.Npc.csproj` exited
  `0`, with `0` warnings and `0` errors. The produced artifact was
  `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`.
- The implementation reuses `EncounterDamageCreditComponent`, adds no second
  credit component, and does not modify `dome/dome1`, the P12 report, the
  Version4 ledger, or lock files.
- The Version4 deletion gate remains not applicable. The experiment is a
  build-checked source implementation and is not a runtime-integrated migration.
