# Server ECS Convergence Task Graph

| Batch | Dependencies | Task | Acceptance evidence | Status |
| --- | --- | --- | --- | --- |
| B1 | none | Synchronize current gate metadata, manifests and source/runtime inventory pointers | `completion-manifest.json`, current regression summary, `git diff --check`, flowstate verifier (`docs=252`, `manifest=252`, `buildReferences=460`) | completed |
| B2 | B1 | Build source-derived WorldGen stage fingerprints and immutable runtime/random checkpoints for one supported profile | `docs/worldgen/worldgen-stage-contract.json`, RED differential, focused verifier, stage/final fingerprint artifact | blocked |
| B3 | B2 | Re-run complete oracle differential and classify every mismatch across tile, extended state, metadata, command sequence and random checkpoints | fresh `legacy-worldgen-differential.json` with zero mismatches | blocked |
| B4 | B1, B2 | Audit all 44 remaining ServerRelevant physical deletions and attach source/replacement evidence or retain deferred | ledger verifier: `serverRelevantWithoutEvidence=0`; no unjustified promotions | deferred |
| B5 | B1 | Qualify TrainingDummy inbound placement direction and implement only a source-backed command path | protocol direction evidence plus valid/duplicate/invalid/session/PVS loopback traces | completed |
| B6 | B3, B4, B5 | Execute final clean-process acceptance review and publish conclusion only when all gates are true | fresh full sweep, builds, differential gate, deletion gate, DoD checklist | pending |

## Gate policy

Each batch requires a reproducible artifact and scoped self-check before its dependents start.
Blocked or partial batches stay explicitly marked and cannot be promoted by green unrelated
verifiers.

## B1 metadata checkpoint

`docs/worldgen/worldgen-deletion-gate.json` now has a `currentBaseline` section pointing to
the 2026-08-22 full sweep (`50 total`, `49 passed`, `0 failed`, `1 infrastructure skip`) and
the current ledger audit (`44` deferred ServerRelevant, `0` without evidence, `0` unknown).
The older nested WorldGen verification records remain unchanged and are explicitly historical.

## B2 checkpoint

The current `Terraria.Dome.WorldGeneration.Verification` run exited 0 and emitted bounded
determinism, checkpoint, atomic-commit, and stage-inventory passes. It does not satisfy B2/B3:
the complete oracle differential remains the authoritative parity gate and is still recorded as
blocked in the semantic remaining-work plan.

The fresh full-size differential rerun is recorded at
`Build/diagnostics/server-ecs-convergence/P9-worldgen/current-full-differential/trace.txt`:
the process exited `0`, compared `5,040,000` tiles, and found `3,190,404` tile mismatches plus
`1,046,843` extended-state mismatches. This is current negative parity evidence and leaves B2
and B3 blocked; it does not authorize changing the deletion gate.

Two source-derived Terrain predicates are now implemented and measured in the same differential:
`WldTileRleReader`'s inactive/non-important `FrameX/FrameY=-1` sentinel, and
`TerrainPass.FillColumn`'s surface-to-rock tile type `0` versus rock-below tile type `1` layering
with `-1` frame sentinels. The first reduced tile mismatches from `5,040,000` to `4,024,633`; the
second reduced them to `3,190,834`; source-aligned cave kill frames reduce them further to
`3,190,404`. The remaining active/type/wall/liquid differences are still
not parity evidence and require further source-derived passes.
The cave kill projection now accepts explicit frame sentinels, preserving the oracle's
`-1/-1` frames when a carving command deactivates a terrain tile; this accounts for the latest
`430`-tile reduction without changing ordinary runtime kill semantics.
The current cave audit records why the remaining gap cannot be closed by speculative carving:
`Build/diagnostics/server-ecs-convergence/P9-worldgen/current-full-differential/cave-parity-audit.json`.

The pipeline now emits nine deterministic ECS stage fingerprints at
`Build/diagnostics/server-ecs-convergence/P9-worldgen/current-stage-trace/stage-fingerprints.json`.
Each entry is marked `oracleParity=not-compared`; these fingerprints establish the immutable
query/commit observation points needed for a future source-backed RED differential, but do not
claim that any stage matches the legacy oracle.
The trace contract now exposes only a cloned read-only stage list and a `FinalSnapshot`; it does
not leak the mutable `WorldGrid` instance.

## B4 checkpoint

The current ledger audit is recorded at
`Build/diagnostics/server-ecs-convergence/P4-deletion/current/verify-ledger.log` and reports
535 rows, zero missing evidence fields for the classified `ServerRelevant` rows, 44 deferred
`ServerRelevant` rows, and zero unknown rows. This is an accounting gate only; it does not
promote any deferred row or authorize physical deletion.

The audit qualified seven `Terraria.GameContent.LootSimulation` rows as `ClientOnly` from the
complete source: they run offline loot analysis and clipboard/report generation only, with no
authoritative server owner or mutation path. The ledger is now `425 ClientOnly`, `44
ServerRelevant`, `60 SharedDefinition`, and `4 ReplacedWithEvidence`; all 44 remaining server
rows stay deferred.
The source-role evidence is registered at
`Build/diagnostics/server-ecs-convergence/P4-deletion/current/loot-simulation-client-only.json`.

The remaining 44 rows were re-audited by source group and retained as
`ServerRelevant/deferred`; no row was promoted without a replacement owner, command/commit
path, and parity verifier. The ruled-out audit is recorded at
`Build/diagnostics/server-ecs-convergence/P4-deletion/current/remaining-server-relevant-audit.json`.

The stage contract was subsequently source-anchored against the complete instrumented oracle.
Terrain now identifies `TerrainPass.ApplyPass`, `FillColumn`, and
`GenerateWorldSurfaceOffset` with line-level anchors; Cave, Biome, Structure, Liquid, Frame,
and FinalCommit identify their pass scheduling, mutation, and random-state boundaries. This is
contract evidence only: stage fingerprints remain ECS observation artifacts with
`oracleParity=not-compared`, and the full differential remains blocked until legacy pass state
and RNG consumption are replayed.

The Terrain random boundary was measured and recorded at
`Build/diagnostics/server-ecs-convergence/P9-worldgen/current-stage-trace/terrain-random-algorithm-gap.json`.
The legacy pass resets `UnifiedRandom` before each pass, while the current ECS terrain consumer
uses the independent LCG `GenerationRandomState`; this is a confirmed source gap, not a parity
claim. The existing LCG policies remain unchanged until a source-backed pass-scoped replay can be
introduced without invalidating unrelated bounded random contracts.
The prerequisite now has a minimal implementation in
`src/Terraria.Dome.Simulation/WorldGeneration/LegacyPassRandomState.cs`. It reproduces the
oracle's 56-slot seed initialization and range sampling for pass-owned consumers while keeping
the existing LCG policies separate. The reduced focused verifier passed `32/81` sections
(39.5%), recorded at
`Build/diagnostics/server-ecs-convergence/P9-worldgen/current-stage-trace/worldgen-reduced.txt`.
This closes only the RNG primitive prerequisite; WorldGen oracle parity remains blocked.

The default `TerrainPass.GenerateWorldSurfaceOffset` predicate family is now implemented as
`LegacyTerrainSurfaceOffsetPolicy` with `LegacyTerrainFeatureKind` (Plateau, Hill, Dale,
Mountain, Valley). It is intentionally an isolated query and is not yet wired into
`TerrainBaseSystem`, because the complete pass still requires source-backed beach bounds,
surface-history retargeting, and pass scheduling. The reduced verifier now covers the seed-1456
Plateau/Mountain sequence and still passes `32/81` sections.

The source `TerrainPass.SurfaceHistory` ring and `RetargetSurfaceHistory` behavior are now
isolated in `LegacySurfaceHistory`. The reduced verifier covers wrap-around ordering and the
source retarget callback sequence; its artifact is
`Build/diagnostics/server-ecs-convergence/P9-worldgen/current-stage-trace/worldgen-reduced-surface-history.txt`.
The type remains unconnected to the pipeline until complete terrain inputs are available.
It covers biome placement, CaveHouse builders, dungeon generation, generation actions/shapes,
and gameplay authority/tile interaction helpers. All 47 remaining ledger paths are represented;
the ledger remains `44` deferred and therefore the
physical-deletion gate remains failed.

The next audit group (`DesertDescription`, `SurfaceMap`, `CorruptionPitBiome`, and
`SpikePitBiome`) was explicitly retained as ServerRelevant: the complete source reads world
tiles/random/secret-seed state and performs SetTile/ClearTile/PlaceWall or protected-structure
mutations. The ruled-out classification facts are recorded at
`Build/diagnostics/server-ecs-convergence/P4-deletion/current/biome-server-relevant-audit.json`.

The completion manifest now distinguishes the current physical-deletion failure reason as
`failed-server-relevant-deferred`: the ledger has zero missing evidence fields and zero unknown
rows, but 44 server-relevant semantics remain intentionally deferred.

The trace API integration gate also passed direct Release builds for Simulation, Server,
Protocol.V1456, WorldFile.V319, and WorldCompatibility; the command flags and zero-warning
results are recorded at
`Build/diagnostics/server-ecs-convergence/P9-worldgen/current-stage-trace/direct-consumer-builds.txt`.

## B5 checkpoint

`docs/research/2026-08-23-tile-entity-authority-qualification.md` records the complete
`TETrainingDummy` oracle and the bounded server-owned chain. Focused and loopback evidence
covers tile validity, typed state, NPC-488 ownership, restart reconstruction, persistence,
message-86 projection, and server tile-interaction placement/removal. A source audit now proves
the basic client-to-server wire direction and fields for message 87, but the ECS route still
lacks session/tile/range/PVS validation, typed command/atomic commit, rejection, and loopback
evidence. Typed five-byte decoding, malformed-length rejection, dispatcher/session enqueue,
and server-side type/session/bounds/PVS guards are now implemented and built. The raw TCP
loopback proves valid visible placement, duplicate suppression, invalid-tile rejection,
foreign hidden-session rejection, and state survival across client disconnect. The complete
oracle's message-87 receive branch contains no interaction-distance predicate, so range is not
a source-backed requirement for this packet. Reconnect reconstruction is covered by the same
loopback. The oracle has no explicit rejection frame; failed guards silently return, and the
current route matches that behavior. This closes the bounded message-87 contract; broader
TrainingDummy family parity remains outside this batch. The audit
is recorded at
`Build/diagnostics/server-ecs-convergence/P5-tileentity/20260822-training-dummy-message87-direction.json`.
