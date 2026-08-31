# Pyramid Wall-Frame Evaluation Implementation Plan

**Goal:** Implement and verify the source-backed value and mutation boundary for the Pyramid
`Framing.WallFrame` calls without promoting the broader WorldGen parity gate.

**Strategy:** graph / P2 bounded slice

## Task 1: Freeze the source-backed boundary

Record the exact `Framing.WallFrame` guard, lookup, neighbor, and random branches in the design
document and active P2 notes. Keep `canRemoveLegacyWorldGen=false`, `serverRelevantDeferredCount=44`,
and the existing Pyramid request boundary unchanged.

## Task 2: Write the focused RED verifier

Extend `Test/Terraria.Dome.PyramidStructure.Verification/Program.cs` with a
`--wall-frame-evaluation-only` path. Assert the desired result for:

- ordinary wall `34` with no neighbors and reset-frame random accounting, proving tile frames are
  preserved while wall frames receive the lookup result;
- all four qualifying neighbors and the full-mask center offset;
- truncating tile neighbors with invisible visibility on and off;
- large-frame wall types `179` and `185` using coordinate lookup tables without RNG;
- wall `21` forced-frame branch;
- zero and invalid walls clearing paint/coating without frame or RNG changes;
- edge targets as no-ops;
- ordered duplicate Pyramid requests becoming ordered typed commands;
- atomic commit and section-version/sequence rejection.

Build and run this path before adding the production owner. The expected RED result is a compile
failure for the missing evaluation owner.

## Task 3: Implement the minimal typed owner

Add the immutable truncation and lookup registries, `WorldTile.WallFrameNumber`/
`WorldTile.WallFrameX`/`WorldTile.WallFrameY`, evaluation result, single-target evaluator, Pyramid
command projection, typed command, and atomic commit system. Keep tile `FrameX`/`FrameY` untouched
by wall framing, keep all inputs explicit, and avoid changing the generic y-major
`SquareWallFrameRequestQuery`.

## Task 4: Verify the bounded batch

Run the focused Pyramid evaluation verifier, Pyramid full verifier, serial Simulation and Server
Release builds, bounded WorldGeneration regression, Main inventory regression, scoped style and
hygiene checks, Flowstate manifest/docs verification, and `git diff --check`. Capture all output in
a fresh `Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-wall-frame-evaluation-<date>/`
directory.

## Task 5: Preserve the partial checkpoint

Update the research note, active plan/task, `progress.md`, and private state with the fresh evidence.
Mark this boundary `completed_partial`; leave tunnel/features, client visibility state,
persistence/protocol wall-frame serialization, aggregate RNG/WLD parity, legacy deletion,
`canRemoveLegacyWorldGen`, and all 44 deferred rows open.
