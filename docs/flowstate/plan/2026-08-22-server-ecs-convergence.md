# Server ECS Convergence Remaining Work

**Strategy:** `graph`  |  **Status:** in_progress

**Objective:** Complete server-authoritative ECS convergence without deleting the legacy WorldGen
oracle until source-backed parity and deletion gates pass.

| Phase | Depends on | Acceptance | Status |
| --- | --- | --- | --- |
| P1 Contract/evidence baseline | — | Fresh inventories and gate metadata | completed |
| P2 WorldGen differential | P1 | Zero tile/state/metadata/command/RNG mismatches | blocked |
| P3 Physical deletion ledger | P1, P2 | No deferred ServerRelevant rows without replacement evidence | deferred |
| P4 TrainingDummy authority | P1 | Source-backed inbound contract and bounded loopback | partial |
| P5 Final acceptance | P2, P3, P4 | Fresh clean-process sweep, builds, differential, deletion gate | pending |

## Non-negotiable gates

- `canRemoveLegacyWorldGen` stays false until the complete oracle matches.
- ServerRelevant rows remain deferred without an evidenced replacement chain.
- Server-to-client traffic does not prove a client-to-server mutation contract.
- Focused verifiers and weighted scores do not substitute for parity.

Current evidence: WorldGen differential remains negative; 44 ServerRelevant rows remain deferred;
message-87 has a bounded contract, while broader TrainingDummy parity is outside this batch. Detailed
traces remain in `Build/diagnostics/` and the linked historical plans.

## Current P9 checkpoint

The SandPatches slice is `completed_partial`: source-backed count, guard, default/Remix/retry Y
ranges, strict central retry, TileRunner request fields, random draw accounting, and bounded
source-attributed traversal commands are implemented and covered by corrected focused RED/GREEN
and complete bounded verifier evidence. Fresh corrected Simulation, verifier, and Server Release
builds exit `0` with zero warnings/errors. Full TileRunner semantics, aggregate ordering, global
RNG/WLD parity, legacy deletion, and the `canRemoveLegacyWorldGen` gate remain deferred. An
earlier concurrent Arch component failure in `DomeSimulation.cs` is retained as historical
diagnostic evidence and was not changed by this batch.

The Pyramid footprint mutation slice is also `completed_partial`: the current tree has a
source-backed typed owner for the first three Pyramid random draws, pillar/type-shape writes,
projected 3x3 wall selection, and atomic command publication. Its focused verifier, Simulation
rebuild, and bounded WorldGeneration verifier all exit `0` in the current-tree rerun directory.
The Pyramid wall-framing request boundary is now `completed_partial`: an immutable,
Pyramid-specific query expands each ordered center into the legacy `SquareWallFrame` x-major
nine-coordinate order from `WorldGen.cs:81820-81834`, validates the complete 3x3 envelope
atomically, and preserves source metadata and duplicates. Its focused Pyramid verifier, Simulation
and Server Release builds, Main inventory regression, and bounded WorldGeneration verifier are
fresh and exit `0` under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-wall-framing-boundary-20260831-01/`.
At that point `Framing.WallFrame` value semantics, frame mutation and RNG, tunnel/features, exact
global parity, aggregate ordering, full WLD differential, and deletion remained deferred; the
value boundary is now recorded below. The request-only design and execution plan are in
`docs/plans/2026-08-31-worldgen-pyramid-wall-framing-design.md` and
`docs/plans/2026-08-31-worldgen-pyramid-wall-framing.md`.

The resumed P2 slice for the source-backed `Framing.WallFrame` value boundary is now
`completed_partial`. Its design and execution plan are
`docs/plans/2026-08-31-worldgen-pyramid-wall-frame-evaluation-design.md` and
`docs/plans/2026-08-31-worldgen-pyramid-wall-frame-evaluation.md`. The typed owners preserve
immutable neighbor/mask evaluation, the exact lookup tables, two-bit non-reset values, reset-frame
random semantics, ordered Pyramid command projection, and atomic commit. `WorldTile.WallFrameX/Y`
remain distinct from tile `FrameX/Y`, and the evaluator preserves the latter while writing wall
coordinates. Request centers now require a complete in-world 3x3 envelope; malformed center/target
metadata, post-framing state, empty-batch state consumption, and command mask/lookup mismatches
fail closed. Fresh focused, dependent-build, bounded-regression, style, manifest, state, and diff
evidence is recorded under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-wall-frame-value-mutation-20260831-01/`.
Client SceneMetrics visibility, persistence/protocol wall-frame serialization, tunnel/features,
aggregate ordering, exact RNG/WLD parity, and all deletion gates remain deferred until
independently evidenced.

The preceding neighbor-only sub-boundary is represented by
`docs/plans/2026-08-31-worldgen-pyramid-wall-frame-neighbor-design.md` and
`docs/plans/2026-08-31-worldgen-pyramid-wall-frame-neighbor.md`. Its frozen truncating-tile
registry and cardinal-mask query are reused by the value evaluator; lookup, RNG, mutation,
tunnel/features, and global parity remain separately gated.

## Current bounded P9 slice: Pyramid noTunnel opening side effect

The execution boundary is intentionally narrower than full Pyramid tunnel/features. The current
owner models the source opening pass at `WorldGen.cs:28429-28466`: direction, width/delay draws,
the source column walk, type-151 clearing, wall-34 writes, and type-53 sand conversion. It
requires a committed or otherwise immutable post-footprint snapshot and emits only typed tile
commands. Chest, pile, plant, pot, final extended tunnel, publication, and global parity remain
outside the batch. `NoTunnel` is retained as an explicit request mode so the caller can stop at
the source's feature boundary without pretending that the deferred feature authorities are
implemented.

The batch is `completed_partial`. Focused RED/GREEN verification, serial Simulation/Pyramid/
Server Release builds, bounded WorldGeneration regression, scoped style, and diff evidence are
recorded under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-tunnel-opening-20260831-01/`.
Source details and the complete deferred list are recorded in
`docs/research/2026-08-31-worldgen-pyramid-tunnel-opening.md`.

## Current continuation: Pyramid buried-chest intent

The follow-up buried-chest seam is also `completed_partial`. `LegacyPyramidBuriedChestIntentPolicy`
preserves the source `WorldGen.cs:28555` midpoint, the three item choices (`848`, `857`, `934`),
the conditional second random draw, tenth-anniversary remap, fixed `AddBuriedChest` arguments,
and atomic `Structure`/sequence publication. It emits an intent only; actual chest placement,
tile scanning, item filling, and entity creation remain deferred. Fresh focused/build/regression,
style, JSON/artifact, manifest, and Flowstate-doc evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-buried-chest-intent-20260831-01/`.
