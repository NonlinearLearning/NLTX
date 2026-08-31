# Server ECS Convergence Task Graph

> Cross-session continuation for the current Pyramid buried-chest intent slice:
> `.agent-workplace/docs/task/worldgen-pyramid-tunnel-opening-continuation-20260831.md`. Read it before
> changing state or rerunning a dependent build.

| Batch | Depends on | Acceptance evidence | Status |
| --- | --- | --- | --- |
| B1 | — | Current metadata, manifests, source/runtime pointers, clean diff, verifier | completed |
| B2 | B1 | Source-derived WorldGen fingerprints and RED differential | blocked |
| B3 | B2 | Complete oracle differential with zero mismatches | blocked |
| B4 | B1, B2 | 44 ServerRelevant rows each have replacement evidence or remain deferred | deferred |
| B5 | B1 | Source-backed TrainingDummy direction, guards, rejection behavior, and loopback | completed |
| B6 | B3, B4, B5 | Fresh full sweep, builds, differential, deletion gate, DoD | pending |

## Gate policy

Each batch needs a reproducible artifact and scoped self-check. Blocked or partial batches cannot be
promoted by unrelated green verifiers.

## Current checkpoints

- WorldGen fingerprints are observation points only (`oracleParity=not-compared`); full differential
  is blocked by large tile and extended-state mismatch.
- The deletion audit is accounting evidence, not permission to remove legacy code; 44 rows remain
  `ServerRelevant/deferred`.
- TrainingDummy message-87 has bounded valid/duplicate/invalid/session/PVS loopback coverage; the
  source has no explicit rejection frame or range predicate. Broader family parity is deferred.

## Current P9 checkpoint

SandPatches focused RED/GREEN and bounded verification are complete for the accepted partial
boundary. The source guard, integer count/Remix reduction, default/Remix/retry Y ranges, strict
central retry, request shape, random accounting, deterministic traversal, and typed commit are
evidenced under `Build/diagnostics/server-ecs-convergence/P9-worldgen/`
`sand-patches-boundary-20260831-01/`. The corrected focused run reports `13` default
invocations, `54` scheduling samples, and `14,594` bounded commands; the complete corrected
verifier reports `281 PASS / 40 CHECK` (including the stage-trace PASS) with no anchored
`FAIL:`/`ERROR:` lines. Fresh Simulation, verifier, and
Server Release builds also exit `0` with zero warnings/errors. The aggregate WorldGen
differential remains negative and the full TileRunner/WLD/deletion gates remain blocked. The
earlier concurrent Arch component failure is retained only as historical diagnostic evidence.

The current Pyramid footprint mutation slice is `completed_partial`: focused request/mutation
verification, Simulation rebuild, and the bounded WorldGeneration verifier are green in the
current-tree rerun. The Pyramid wall-framing request boundary is also `completed_partial`: its
focused gate expands two centers to 18 requests in source x-major order, preserves metadata and
duplicates, and rejects an edge-crossing envelope atomically without touching snapshot, random, or
generation state. Fresh Pyramid, Simulation, Server, Main inventory, and bounded WorldGeneration
evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-wall-framing-boundary-20260831-01/`.
At that point `Framing.WallFrame` calculation/mutation, wall-frame RNG, tunnel/features, exact
global parity, aggregate WLD parity, and deletion remained deferred; the value boundary is now
recorded below. The request-only design and implementation plan are recorded in
`docs/plans/2026-08-31-worldgen-pyramid-wall-framing-design.md` and
`docs/plans/2026-08-31-worldgen-pyramid-wall-framing.md`.

The Pyramid wall-frame evaluation boundary is now `completed_partial`. Its design and execution
plan are `docs/plans/2026-08-31-worldgen-pyramid-wall-frame-evaluation-design.md` and
`docs/plans/2026-08-31-worldgen-pyramid-wall-frame-evaluation.md`. The batch observed the focused
RED verifier before implementation, then verified immutable truncation/lookup owners, explicit
wall-frame number and coordinate state, source-shaped evaluation, independent tile-vs-wall frame
preservation, ordered command projection, complete 3x3 request-envelope validation,
empty/post-framing/malformed-input rejection, mask/lookup consistency, and atomic commit. The
`...-03` stale-fixture failure is retained as diagnostic evidence; the corrected committed-center
assertion compares the final command result and separately preserves tile `FrameX/Y`. Fresh
focused RED/GREEN, dependent-build, bounded-regression, style, manifest, state, and diff evidence
is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-wall-frame-evaluation-20260831-08/`.
Pyramid tunnel/features, client visibility state, persistence/protocol wall-frame serialization,
aggregate RNG/WLD parity, legacy deletion, `canRemoveLegacyWorldGen=false`, and 44 deferred
`ServerRelevant` rows remain outside the batch.

The current tree also contains the preceding Pyramid neighbor-classification sub-boundary from
`docs/plans/2026-08-31-worldgen-pyramid-wall-frame-neighbor.md`: the exact truncating-tile set,
strict-interior guard, source bit order, invisible-wall mode, and zero/invalid center branches
are owned by `LegacyWallFrameNeighborQuery` and covered by `--wall-neighbor-only`. The value
evaluation batch reuses that owner rather than introducing a second neighbor predicate.

## Current bounded P9 slice

| Batch | Depends on | Acceptance evidence | Status |
| --- | --- | --- | --- |
| P9 Pyramid noTunnel opening | Pyramid footprint mutation | Focused RED/GREEN verifier, serial Simulation and Pyramid builds, commit smoke test, scoped style and diff checks | completed_partial |

The batch is limited to the source opening tunnel loop (`WorldGen.cs:28429-28466`) and its
`noTunnel` handoff. It preserves source random draw order, direction, tunnel-width/delay inputs,
projected tile state, wall-34 writes, type-151 clearing, type-53 sand conversion, and atomic
command publication. The chained buried-chest intent sub-boundary at `WorldGen.cs:28555` is now
`completed_partial`: it preserves the midpoint, source item choices, retry draw, fixed call
arguments, and atomic intent publication without executing chest placement. Chest tile/object
placement, pile/plant/pot features, final extended tunnel, aggregate ordering, global RNG/WLD
parity, and deletion remain deferred. The focused and bounded opening evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-tunnel-opening-20260831-01/`; the
continuation evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-buried-chest-intent-20260831-01/`; source contract is in
`docs/research/2026-08-31-worldgen-pyramid-tunnel-opening.md`.
