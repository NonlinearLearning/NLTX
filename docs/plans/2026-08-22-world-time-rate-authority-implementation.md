# World Time-Rate Authority Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use `executing-plans` to implement this plan task-by-task.

**Goal:** Verify and complete the approved versioned time-rate authority so invasion travel uses
the source-derived `dayRate` semantics without conflating it with clock tick rate.

**Architecture:** Retain one immutable `WorldTimeRateSnapshot`, resolve it only from trusted
server input in `ApplyWorldClock`, persist it append-only, and expose it to exactly one travel
consumer. Preserve unavailable recovery for old snapshots and leave unrelated invasion behavior
deferred.

**Tech Stack:** .NET 10, C#, immutable snapshots, deterministic verifier executables.

---

### Task 1: Verify Source-Precedence And Tick Consumer

**Files:**
- Read: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs:3567-3595,12958-13046`
- Read: `src/Terraria.Dome.Simulation/World/WorldTimeRatePolicy.cs`
- Read: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Test: `Test/Terraria.Dome.TickOrder.Verification/Program.cs`

**Step 1:** Confirm RED/acceptance assertions cover configured, fast-forward, frozen, all-sleeping,
and menu rates, plus `max(rate, 1)` travel in the `ApplyWorldClock` phase.

**Step 2:** Run:

```powershell
dotnet run --project Test/Terraria.Dome.TickOrder.Verification/Terraria.Dome.TickOrder.Verification.csproj `
  -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

Expected: all named phase, policy-precedence, and invasion-travel assertions pass.

### Task 2: Verify Versioned Restart Contract

**Files:**
- Read: `src/Terraria.Dome.Simulation/Snapshots/DomeSimulationSnapshot.cs`
- Read: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`
- Test: `Test/Terraria.Dome.Persistence.Verification/Program.cs`

**Step 1:** Confirm v27 persists availability and rate, while a pre-v27 payload reconstructs
`Unavailable` rather than inventing a value.

**Step 2:** Run the Persistence verifier with the standard serial properties.

Expected: valid round-trip/restart continuity passes and malformed rate payloads are rejected.

### Task 3: Reduced Acceptance Boundary

**Files:**
- Test: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- Test: `Test/Terraria.Dome.MainBoundary.Verification/Program.cs`
- Create: `Build/diagnostics/main-migration/task-9-world-time-rate-authority/<run-id>/scenario.yaml`

**Step 1:** Run WorldRules to retain the independent source travel/clamp rule.

**Step 2:** Run MainBoundary and scoped `git diff --check` after final edits.

**Step 3:** Mark only the rate/travel authority path accepted. Keep clock fraction import, random
start behavior, NPC tables, and presentation deferred.
