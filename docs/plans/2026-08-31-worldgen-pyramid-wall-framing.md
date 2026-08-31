# Pyramid Wall-Framing Request Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a source-backed, immutable Pyramid wall-framing request boundary that preserves the
legacy nine-coordinate x-major invocation order without claiming wall-frame calculation parity.

**Architecture:** A dedicated Simulation record represents one legacy `Framing.WallFrame` request.
A dedicated query validates the immutable world envelope up front, then expands ordered Pyramid
wall centers into nine requests per center. It does not read or mutate tiles, consume random state,
advance generation state, or enter the existing generic tile-frame pipeline.

**Tech Stack:** C# 13/.NET 10, `Terraria.Dome.Simulation`, top-level focused verifier, immutable
`WorldGridSnapshot`, `WallFrameCoordinate`, and repository serial `dotnet` commands.

---

### Task 1: Freeze the design boundary

**Files:**
- Create: `docs/plans/2026-08-31-worldgen-pyramid-wall-framing-design.md`
- Create: `docs/plans/2026-08-31-worldgen-pyramid-wall-framing.md`
- Modify: `docs/flowstate/plan/2026-08-22-server-ecs-convergence.md`
- Modify: `docs/flowstate/task/2026-08-22-server-ecs-convergence.md`

**Step 1: Record source evidence and exclusions**

Record the Pyramid call site (`WorldGen.cs:28425`), the x-major `SquareWallFrame` order
(`WorldGen.cs:81820-81834`), and the deferred `Framing.WallFrame` semantics in the design and
active Flowstate notes. Keep the current `completed_partial` status and deletion gates unchanged.

**Step 2: Verify the plan text**

Run:

```powershell
git diff --check -- docs/plans/2026-08-31-worldgen-pyramid-wall-framing-design.md `
  docs/plans/2026-08-31-worldgen-pyramid-wall-framing.md `
  docs/flowstate/plan/2026-08-22-server-ecs-convergence.md `
  docs/flowstate/task/2026-08-22-server-ecs-convergence.md
```

Expected: exit `0`.

### Task 2: Write the focused RED assertions

**Files:**
- Modify: `Test/Terraria.Dome.PyramidStructure.Verification/Program.cs`

**Step 1: Add the request expansion contract**

Use the existing Pyramid footprint fixture's ordered `actualWallCoordinates` as centers. Assert
that the new query returns nine requests per center, preserves the exact x-major sequence and
duplicates, records center/target/source/source-line/reset metadata, and leaves the snapshot,
random sample count, and generation state unchanged.

**Step 2: Add atomic failure checks**

Pass a center whose nine-point envelope crosses the world boundary. Assert `TryCreateRequests`
returns `false`, an empty output, and a non-empty failure reason. Also cover a capacity-safe empty
center list as a successful no-op.

**Step 3: Build and run the verifier to prove RED**

Run:

```powershell
dotnet run --project Test/Terraria.Dome.PyramidStructure.Verification/Terraria.Dome.PyramidStructure.Verification.csproj `
  -c Release --no-restore -- `
  --wall-framing-only
```

Expected: a compile failure because the new request owner does not exist yet. Do not implement
production code before this failure is observed.

### Task 3: Implement the minimal immutable request owner

**Files:**
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/LegacyPyramidWallFrameRequest.cs`
- Create: `src/Terraria.Dome.Simulation/WorldGeneration/LegacyPyramidWallFrameRequestQuery.cs`

**Step 1: Add the request record**

Define a readonly value record with `WallFrameCoordinate Center`, `WallFrameCoordinate Target`,
`bool ResetFrame`, `string Source`, and `int SourceLine`. Keep the source constants on the query or
record as `const` values and do not expose mutable collections.

**Step 2: Add atomic x-major expansion**

Implement `TryCreateRequests` with `IReadOnlyList<WallFrameCoordinate>` input. Validate the snapshot
and every center plus target before appending. Iterate `offsetX` outermost and `offsetY` innermost,
matching legacy order. Return a read-only result on success and an empty result on failure.

**Step 3: Keep the boundary pure**

Do not call `GetTile`, `TileFrameEvaluationQuery`, `TileFrameCommandSystem`, or
`TileChangeCommitSystem`; do not consume `LegacyPassRandomState`; do not modify generation state or
the generic square-wall query.

### Task 4: Verify GREEN and regressions

**Files:**
- No additional source files.

**Step 1: Run the focused verifier**

```powershell
dotnet run --project Test/Terraria.Dome.PyramidStructure.Verification/Terraria.Dome.PyramidStructure.Verification.csproj `
  -c Release --no-restore -- `
  --wall-framing-only
```

Expected: exit `0`, with the new wall-framing request checks passing.

**Step 2: Build Simulation serially**

```powershell
dotnet build src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj `
  -c Release -p:UseSharedCompilation=false -p:BuildInParallel=false `
  -p:NodeReuse=false -m:1 -nr:false
```

Expected: exit `0`, zero warnings, zero errors, and output only under `Build/bin` and `Build/obj`.

**Step 3: Run the dependent focused and bounded verifiers**

Rebuild the Pyramid verifier and the WorldGeneration verifier serially with
`-p:UseSharedCompilation=false -p:BuildInParallel=false -p:NodeReuse=false -m:1 -nr:false`, then
run the focused Pyramid check and complete bounded WorldGeneration check into a fresh
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-wall-framing-boundary-<date>/`
directory.

### Task 5: Record the partial checkpoint

**Files:**
- Modify: `docs/research/2026-08-31-worldgen-pyramid-wall-framing-boundary.md`
- Modify: `docs/flowstate/plan/2026-08-22-server-ecs-convergence.md`
- Modify: `docs/flowstate/task/2026-08-22-server-ecs-convergence.md`
- Modify: `progress.md`
- Modify: `.agent-workplace/state/checkpoint.json`

**Step 1: Record fresh evidence**

Capture exit codes, warning/error counts, request count, center count, order checks, and atomic
failure checks with exact artifact paths.

**Step 2: Preserve the release boundary**

State `completed_partial`; keep actual wall-frame semantics, tunnel/features, exact global RNG and
WLD parity, legacy deletion, `canRemoveLegacyWorldGen=false`, and 44 deferred `ServerRelevant`
rows explicitly open.

**Step 3: Run final repository checks**

Run the scoped style check for the two new owner files, Flowstate docs verification, and
`git diff --check`. Report every command's exit status and warnings/errors before claiming the
batch is green.
