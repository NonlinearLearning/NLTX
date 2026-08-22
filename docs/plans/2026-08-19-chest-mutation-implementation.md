# Chest Mutation Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make world-chest placement and destruction deterministic, coordinate-unique, and
server-owned without adding a client force-close protocol.

**Architecture:** `ChestIndexSystem` owns coordinate uniqueness. `ChestMutationCommitSystem`
validates typed commands and updates the chest dictionary and index together. `DomeSimulation`
uses these systems for normal creation, destruction, and persistence restoration.

**Tech Stack:** C# net10.0, Arch ECS host, `WorldGrid`, focused console verifiers.

---

### Task 1: Freeze Chest Mutation Contracts

**Files:**
- Create: `src/Terraria.Dome.Simulation/WorldObjects/Chest/Commands/ChestCreateCommand.cs`
- Create: `src/Terraria.Dome.Simulation/WorldObjects/Chest/Commands/ChestDestroyCommand.cs`

**Step 1: Write the failing test**

Add a WorldObjects verifier fixture that attempts a duplicate-coordinate create and a destroy request
for an unknown chest.

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet run --project .\Test\Terraria.Dome.WorldObjects.Verification\Terraria.Dome.WorldObjects.Verification.csproj -p:UseSharedCompilation=false
```

Expected: build failure because the typed mutation APIs do not exist.

**Step 3: Write minimal implementation**

Define immutable command structs containing a non-negative sequence and the required chest identity
and coordinates.

**Step 4: Run test to verify it passes**

Run the focused verifier and expect the new contract fixture to compile.

### Task 2: Add Coordinate Index and Mutation Commit

**Files:**
- Create: `src/Terraria.Dome.Simulation/WorldObjects/Chest/Systems/ChestIndexSystem.cs`
- Create: `src/Terraria.Dome.Simulation/WorldObjects/Chest/Systems/ChestMutationCommitSystem.cs`
- Test: `Test/Terraria.Dome.WorldObjects.Verification/Program.cs`

**Step 1: Write the failing test**

Assert that a duplicate coordinate is rejected, an occupied chest cannot be destroyed, an opened
empty chest cannot be destroyed, and a closed empty chest is removed.

**Step 2: Run test to verify it fails**

Run the focused verifier and expect the mutation assertions to fail.

**Step 3: Write minimal implementation**

Use a private coordinate dictionary in `ChestIndexSystem`. `ChestMutationCommitSystem` validates
the command, world bounds, 40-slot emptiness, and `Opener == null` before applying create/remove.

**Step 4: Run test to verify it passes**

Run the focused verifier and expect all WorldObjects assertions to pass.

### Task 3: Integrate Simulation and Persistence Recovery

**Files:**
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Test: `Test/Terraria.Dome.WorldObjects.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Persistence.Verification/Program.cs`

**Step 1: Write the failing test**

Add `TryGetChestIdAt`, coordinate reuse after successful removal, and a duplicate-coordinate
persistence snapshot rejection fixture.

**Step 2: Run tests to verify they fail**

Run the WorldObjects and Persistence verifiers serially.

**Step 3: Write minimal implementation**

Delegate `CreateChest` and `TryDestroyChest` to the mutation system, maintain a monotonic mutation
sequence, and register restored chests in the coordinate index before accepting them.

**Step 4: Run tests to verify they pass**

Run both focused verifiers and expect exit 0.

### Task 4: Regression and Documentation

**Files:**
- Modify: `docs/research/2026-08-18-wiring-liquid-chest-coverage.md`

**Step 1: Run regressions**

```powershell
dotnet build .\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -p:UseSharedCompilation=false
dotnet build .\src\Terraria.Dome.Server\Terraria.Dome.Server.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.WorldObjects.Verification\Terraria.Dome.WorldObjects.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.WorldObjects.Loopback.Verification\Terraria.Dome.WorldObjects.Loopback.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj -p:UseSharedCompilation=false
```

**Step 2: Record evidence**

Update the coverage table to distinguish verified world-chest placement/destruction from excluded
bank and shop behavior.
