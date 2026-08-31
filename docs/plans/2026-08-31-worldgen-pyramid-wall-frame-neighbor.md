# Pyramid Wall-Frame Neighbor Classification Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a source-backed immutable query for the cardinal neighbor mask used by Pyramid
`Framing.WallFrame`, without claiming frame lookup or mutation parity.

**Architecture:** A frozen Simulation registry owns the exact four legacy truncating tile types.
A typed query validates a strict interior coordinate, normalizes invalid center wall types to the
legacy zero-wall branch, and computes the source bit mask from an immutable snapshot. The query
receives invisible-wall visibility explicitly so it has no client/global dependency.

**Tech Stack:** C# 13/.NET 10, `Terraria.Dome.Simulation`, immutable `WorldGridSnapshot`, and a
top-level focused verifier.

---

### Task 1: Freeze the design boundary

**Files:**
- Create: `docs/plans/2026-08-31-worldgen-pyramid-wall-frame-neighbor-design.md`
- Create: `docs/plans/2026-08-31-worldgen-pyramid-wall-frame-neighbor.md`

**Step 1: Record source evidence and exclusions**

Record the `Framing.WallFrame` branch order, the `TruncatesWalls` set, and the `WallID.Count`
value. Keep lookup, RNG, mutation, and deletion gates deferred.

**Step 2: Verify the plan text**

Run:

```powershell
git diff --check -- docs/plans/2026-08-31-worldgen-pyramid-wall-frame-neighbor-design.md `
  docs/plans/2026-08-31-worldgen-pyramid-wall-frame-neighbor.md
```

Expected: exit `0`.

### Task 2: Add focused RED assertions

**Files:**
- Modify: `Test/Terraria.Dome.PyramidStructure.Verification/Program.cs`

**Step 1: Add a `--wall-neighbor-only` entry point**

Add a focused fixture with an interior center wall, positive-wall neighbors, active truncating
neighbors, and a non-qualifying inactive neighbor. Assert source bit order, exact registry
membership, and snapshot immutability.

**Step 2: Add visibility and center-normalization checks**

Run the same fixture with `showInvisibleWalls` false and true. Assert an invisible qualifying
neighbor is omitted/present accordingly. Assert center wall type `0` and `WallID.Count` produce a
successful empty-mask no-frame result, while border coordinates fail atomically.

**Step 3: Prove RED before production code**

Run:

```powershell
dotnet run --project Test/Terraria.Dome.PyramidStructure.Verification/Terraria.Dome.PyramidStructure.Verification.csproj `
  -c Release --no-restore -- -p:UseSharedCompilation=false -p:BuildInParallel=false `
  -p:NodeReuse=false -m:1 -nr:false --wall-neighbor-only
```

Expected: compile failure because the registry/query/result owner does not yet exist.

### Task 3: Implement the minimal pure owner

**Files:**
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/LegacyTruncatingWallTileRegistry.cs`
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameNeighborMask.cs`
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameNeighborResult.cs`
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameNeighborQuery.cs`

**Step 1: Add the frozen truncating registry**

Expose `RegisterDefaults()` and `IsTruncatingTile(ushort)` over exactly `{54, 328, 459, 748}`.

**Step 2: Add the typed mask/result**

Use flags `Above=1`, `Left=2`, `Right=4`, and `Below=8`. The result records coordinate,
original/effective center wall, mask, visibility mode, and normalization state.

**Step 3: Add the source-order query**

Implement `TryEvaluate(snapshot, x, y, showInvisibleWalls, out result, out failureReason)`.
Validate the strict interior before reading; normalize `wall >= 367` to zero; return an empty
mask for effective zero; then evaluate four neighbors with the exact source predicate and bits.
Do not read frame fields, random state, or pending mutations.

### Task 4: Verify GREEN and regressions

**Files:**
- No additional source files.

**Step 1: Run focused GREEN**

Run the `--wall-neighbor-only` command from Task 2. Expected: exit `0` with all focused checks.

**Step 2: Build Simulation serially**

```powershell
dotnet build src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj `
  -c Release --no-restore -p:UseSharedCompilation=false -p:BuildInParallel=false `
  -p:NodeReuse=false -m:1 -nr:false
```

Expected: exit `0`, zero warnings, zero errors.

**Step 3: Run Pyramid and bounded WorldGeneration regressions**

Rebuild dependent verifiers serially and run the existing Pyramid verifier plus the complete
bounded WorldGeneration verifier into a fresh evidence directory. Preserve the prior wall-request
evidence and do not overwrite concurrent artifacts.

### Task 5: Record the partial checkpoint

**Files:**
- Create: `docs/research/2026-08-31-worldgen-pyramid-wall-frame-neighbor-boundary.md`
- Modify: `docs/flowstate/plan/2026-08-22-server-ecs-convergence.md`
- Modify: `docs/flowstate/task/2026-08-22-server-ecs-convergence.md`
- Modify: `progress.md`
- Modify: `.agent-workplace/state/checkpoint.json`

**Step 1: Record fresh evidence**

Capture exact source hashes, focused counts, build exit codes, warning/error counts, and evidence
paths.

**Step 2: Preserve the release boundary**

Record `completed_partial`; retain `canRemoveLegacyWorldGen=false`, 44 deferred rows, and all
lookup/mutation/tunnel/features/RNG/WLD blockers.

**Step 3: Run final checks**

Run scoped Google-style checks, Flowstate docs verification, state JSON parsing, and `git diff --check`.
Report each exit status before describing the batch as green.
