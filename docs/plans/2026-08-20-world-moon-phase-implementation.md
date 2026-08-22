# World Moon Phase Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Give the server-owned world clock a V1456 moon phase that advances at dawn, persists safely,
and projects through existing WorldData.

**Architecture:** `WorldClock` owns a validated byte phase and changes it only on the night-to-day
transition in `Advance`. Its snapshot is the sole state transfer boundary. Server persistence appends
the byte in format v19, while the V1456 adapter consumes the immutable snapshot to fill its existing
MoonPhase field.

**Tech Stack:** .NET 10, C#, executable verifier projects, binary persistence, V1456 protocol codec.

---

### Task 1: Establish moon-phase RED coverage

**Files:**
- Modify: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- Modify: `Test/Terraria.Dome.Persistence.Verification/Program.cs`
- Modify: `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs`

**Step 1:** Add a clock fixture at the final night tick with phase 7 and assert the next tick is day,
time zero and phase 0. Add a paused fixture asserting no phase change.

**Step 2:** Add a v19 persistence round-trip fixture and a hand-written v18 fixture asserting phase 0.
Add invalid v19 phase/truncation rejection fixtures.

**Step 3:** Add a WorldData fixture that creates a server from a snapshot with phase 4 and asserts the
existing payload/context field is 4.

**Step 4:** Run each focused verifier serially. Save non-zero results as `*-red.txt` in a new Task 9
moon-phase evidence directory.

### Task 2: Add validated clock state

**Files:**
- Modify: `src/Terraria.Dome.Simulation/World/WorldClock.cs`
- Test: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`

**Step 1:** Add a byte moon-phase constructor/snapshot field with `0..7` validation.

**Step 2:** In `Advance`, increment only when the clock changes from night to day, using modulo 8.

**Step 3:** Restore phase only after validation; paused clock calls remain no-op.

**Step 4:** Run WorldRules GREEN serially.

### Task 3: Add append-only persistence compatibility

**Files:**
- Modify: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`
- Test: `Test/Terraria.Dome.Persistence.Verification/Program.cs`

**Step 1:** Introduce v19 and append one phase byte after the v18 world-surface field.

**Step 2:** Read v1-v18 with phase zero; v19 must validate a complete byte in range.

**Step 3:** Run Persistence GREEN serially.

### Task 4: Project the authoritative phase

**Files:**
- Modify: `src/Terraria.Dome.Server/DomeServer.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Compatibility/LegacyWorldDataContext.cs`
- Test: `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs`

**Step 1:** Extend `WithWorldState` with validated moon phase and preserve all existing fields.

**Step 2:** Pass Simulation's immutable clock phase from `DomeServer.CreateWorldDataContext`.

**Step 3:** Run Protocol Compatibility GREEN serially.

### Task 5: Final acceptance

**Files:**
- Create: `Build/diagnostics/main-migration/task-9-moon-phase-state/<runId>/scenario.yaml`
- Modify: `docs/plans/2026-08-19-main-server-ecs-migration-execution.md`
- Modify: `progress.md`

**Step 1:** Run clean deterministic replay, persistence, WorldRules loopback, Protocol Compatibility,
MainBoundary, serial root Release and `git diff --check` using required serial MSBuild properties.

**Step 2:** Record source hashes, RED/GREEN output, v18 compatibility, accepted scope, and deferred
event/random/remix behavior.
