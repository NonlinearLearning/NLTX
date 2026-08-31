# Pyramid noTunnel opening side-effect implementation plan

**Goal:** add a bounded, source-backed tile-command owner for the Pyramid opening loop while
preserving the explicit partial boundary around deferred structure features.

**Strategy:** graph / P2 bounded slice

## Task 1: Freeze the source seam

Record the three random draws, direction/start coordinates, source scan order, type-151 clearing,
wall-34 writes, type-53 conversion, and the later `noTunnel` handoff in the design note. Keep
`canRemoveLegacyWorldGen=false` and the 44 deferred `ServerRelevant` rows unchanged.

## Task 2: Focused RED verifier

Add `--tunnel-opening-only` to the Pyramid verifier. Assert source draw order, direction/start
coordinates, noTunnel boundary marker, projected tile semantics, command ordering/attribution,
snapshot purity, untouched-field preservation, atomic commit, and out-of-envelope rejection.
Build the verifier before adding the owner and capture the expected missing-owner failure.

## Task 3: Minimal typed owner

Add a readonly request/result contract and `LegacyPyramidTunnelOpening`. Validate the post-footprint
envelope and sequence capacity before drawing, project repeated writes against an immutable
snapshot, emit only typed tile commands, and stop safely at malformed world edges. Do not add
feature-object or final-tunnel behavior in this batch.

## Task 4: Green verification

Run the focused verifier, dependent Simulation/Pyramid builds, a commit smoke test, scoped style and
`git diff --check`, and the bounded WorldGeneration regression only after the focused gate is green.
Capture all output in a fresh
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-tunnel-opening-<date>/` directory.

## Task 5: Preserve partial state

Update the active convergence plan/task, research evidence, `progress.md`, and private checkpoint
with the fresh artifacts. Keep chest/pile/plant/pot features, final extended tunnel, client
visibility, aggregate ordering, exact RNG/WLD differential, legacy deletion, and all 44 deferred
rows open.
