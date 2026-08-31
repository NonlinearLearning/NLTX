# Project Progress Summary

> This file is the model-facing current-state card. Historical batch narratives remain in the
> linked `docs/research/`, `docs/server-completion/`, `docs/worldgen/`, and archive records.

**Last consolidated:** 2026-08-31
**Flowstate:** `N6` / `in_progress_with_deferred_findings`
**Plan:** `docs/flowstate/plan/2026-08-22-server-ecs-convergence.md`
**Task graph:** `docs/flowstate/task/2026-08-22-server-ecs-convergence.md`
**Build concurrency contract:** [`AGENTS.md#dotnet-build-concurrency-contract`](AGENTS.md#dotnet-build-concurrency-contract)

Current Main inventory gate: `members=696`, `migratedScope=695`, `identityExcluded=1`,
`accepted-narrow=127`; the six latest static/capacity owners plus the no-traps, skyblock metadata,
invasion-progress-wave projection, and GameUpdateCount owners are included in the resolver hash.

## Current architecture

- `Terraria.Dome.sln` is the build entry point and targets `net10.0`.
- `Terraria.Dome.Simulation` owns Arch ECS state, systems, commands, and immutable snapshots.
- `Terraria.Dome.Server` owns the server loop, sessions, authority, persistence, and replication.
- `Terraria.Dome.Protocol.V1456` owns protocol codecs and compatibility projections.
- `Terraria.WorldFile.V319` and `Terraria.WorldCompatibility` own WLD parsing and compatibility.

## Accepted evidence

The following focused domains have fresh exit-code-0 evidence: MainBoundary, Main.pvpBuff,
WorldObjects,
Persistence, WorldRules, WorldClock, WorldImport, PlayerLifecycle, Combat, Completion, and the
PlayerAuthority bootstrap queue repair. Evidence paths are recorded in the corresponding research
or server-completion document and under `Build/diagnostics/`.

Accepted means only the evidenced server-owned slice. It does not imply full Terraria parity,
complete entity behavior, or release readiness.

## Current blockers and deferred scope

- WorldGen oracle differential remains negative; tile, extended-state, metadata, command-sequence,
  and random-checkpoint parity are not complete.
- `canRemoveLegacyWorldGen` remains false.
- Forty-four `ServerRelevant` ledger rows remain deferred; accounting evidence does not authorize
  physical deletion.
- Complete NPC, Projectile, Item, TileEntity, initialization-family, and event behavior remains
  partial or deferred.
- TrainingDummy message-87 has a bounded source-backed contract and loopback coverage; broader
  TrainingDummy family parity remains outside the batch.
- Client presentation, full static tables, global random ordering, and unsupported protocol
  branches remain deferred unless separately evidenced.

Recent NPC boundary: N3.143 wires `PublishNpcDeaths()` to the typed invasion-progress command
bridge. A real pipeline fixture proves two same-tick group-1 deaths queue independent commands,
which settle on the next tick (10 -> 8), while mismatched and inactive invasions remain unchanged.
Evidence is under `Build/diagnostics/npc-complete/task-10-interaction/20260831-invasion-progress-pipeline-final/`;
cross-restart sequence persistence and complete invasion/NPC parity remain deferred.

Recent WorldGen boundary: WavyCaves now has a typed owner with explicit Remix/Skyblock/DontStarve
guards, request-level Skyblock propagation, source-shaped invocation count, the
`UnderworldLayer - 100` Y bound, and WavyCaverer command emission. Focused and complete
WorldGeneration verification passed (`280 PASS`, `40 CHECK`), with Simulation/Server builds and
diff check exit `0`. Evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/wavy-caves-boundary-20260831-02/`;
aggregate cave RNG/TileRunner/WLD parity and the deletion gate remain deferred.
The request-level Skyblock guard is also propagated to every profile-enabled cave owner; the fresh
regression gate is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/skyblock-cave-guard-propagation-20260831-01/`.
SmallHoles now has an explicit Skyblock execution guard that leaves random/state/tile/liquid output
unchanged; DirtLayerCaves receives the same request-level guard through `LegacyCavePassSystem`.
The final serial gate is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/smallholes-skyblock-guard-20260831-01/`.

Recent WorldGen boundary: OceanSand now has a source-backed typed scheduling owner in
`LegacyOceanSandPass`. It preserves the `Skyblock`/`noSurface` guards, three source iterations,
central `40%..60%` retry exclusion, `35..90` width draws, center scaling/doubling, left/right
beach forcing, center-iteration skip, depth drift/clamp, first-active scan, midpoint pyramid
signals, deterministic random accounting, and source-attributed type-53 tile writes. The focused
verifier passes with `200` columns and `11,308` random samples, confirms actual type-53 command
commit, and the rebuilt bounded WorldGeneration verifier passes with `280 PASS`, `40 CHECK`, and no
anchored `FAIL`/`ERROR` lines. Post-projection evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/ocean-sand-boundary-20260831-03/`; scheduling
history remains under `...-20260831-02/`, with source and boundary details in
`docs/research/2026-08-31-worldgen-ocean-sand-boundary.md`. The same rerun hardens the
source-compatible Underworld evil runner edge-start path while retaining public negative-coordinate
validation. Aggregate pipeline wiring, full TileRunner/pyramid/dune behavior, aggregate RNG/WLD
parity, legacy deletion, `canRemoveLegacyWorldGen=false`, and 44 deferred `ServerRelevant` rows
remain open; this is `completed_partial`, not full OceanSand parity.

Recent WorldGen boundary: SandPatches now has a source-backed typed scheduling owner in
`LegacySandPatchesPassDefinition` and `LegacySandPatchesPass`. It preserves the Skyblock-only
guard, `(int)(width * 0.013)` invocation count, Remix `/4` reduction, default surface-to-rock and
Remix `rockLayer-100..maxTilesY-350` Y ranges, strict `46%..54%` central retry, default-Y retry
fallback, `15..70` strength, `20..130` steps, type-53 TileRunner request fields, deterministic
draw accounting, and source-attributed bounded traversal commands. The corrected focused
RED/GREEN run reports `13` default invocations, `54` scheduling samples, and `14,594` bounded
commands, with deterministic replay and actual typed tile commit. The corrected Remix draw order
is independently verified by the current source probe (`17` scheduling draws). Fresh corrected
Simulation, verifier, and Server Release builds exit `0` with zero warnings/errors; the complete
bounded verifier reports `281 PASS` (including the stage-trace PASS), `40 CHECK`, and no anchored
`FAIL:`/`ERROR:` lines under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/sand-patches-boundary-20260831-01/`.
The current-tree rerun under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/sand-patches-boundary-20260831-02/`
passes the focused gate with the same `13` invocations, `54` scheduling samples, and `14,594`
commands; the current bounded WorldGeneration verifier is `280 PASS / 40 CHECK`, exit `0`, with
no anchored `FAIL:`/`ERROR:` lines. Details are in
`docs/research/2026-08-31-worldgen-sand-patches-boundary.md`. An earlier concurrent Arch component
failure in `DomeSimulation.cs` remains historical diagnostic evidence. Full
TileRunner parity, aggregate ordering, global RNG/WLD parity, legacy deletion,
`canRemoveLegacyWorldGen=false`, and 44 deferred `ServerRelevant` rows remain open; this is
`completed_partial`.

Recent WorldGen boundary: Dunes and Pyramids now have separate source-backed execution gates.
`ShouldRunDunes` preserves the `!Skyblock && !noSurface` guard, while `ShouldRunPyramids`
preserves `!Skyblock && !noSurfaceNoPyramids`; the aggregate helper is compatibility-only. The
Pyramid candidate owner preserves strict 300-tile edge bounds, dungeon-side 15% exclusion,
anniversary Underground Desert veto, and 220/110 prior-pyramid spacing. The snapshot scan stops
at the first active tile, requires sand type 53, and returns the exact `activeY - 1` placement row.
The Dunes candidate selector also preserves the source X-then-Y `RandomWorldPoint` loop, scaled
jungle/center/snow vetoes, and the `width`/`2*width` retry threshold transitions. The dual-dungeon
owner preserves the two-step potential-bounds check (`fluff=5`, then `k -= 50` and `num2 = 100`).
Focused RED/GREEN assertions, the Simulation Release build, and the Tunnels regression all exit
`0` under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/dunes-pyramid-candidate-20260831-01/`;
details are in `docs/research/2026-08-31-worldgen-dunes-pyramid-gate.md` and
`docs/research/2026-08-31-worldgen-pyramid-candidate-boundary.md` plus
`docs/research/2026-08-31-worldgen-dunes-candidate-retry-boundary.md`. DunesBiome/Pyramid structure
mutation, per-iteration chance/publication, full RNG and aggregate ordering, full WLD differential,
legacy deletion,
`canRemoveLegacyWorldGen=false`, and 44 deferred `ServerRelevant` rows remain open; this is
`completed_partial`.

The surface-desert extra-Pyramid branch now has a separate typed candidate owner. It preserves the
source `Next(5,8)` count draw, integer `width / 4200` scaling, X range `[300,width-300)`, strict
`0.47..0.53` center retry, deterministic replay, and a fail-closed guard for dimensions with no
selectable X. The owner records candidates only; `FindLowestCloud`, sand scanning, and
`Pyramid(...)` mutation remain outside its boundary. Focused TDD RED/GREEN verification and the
Simulation Release build exit `0` are under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-desert-pyramid-20260831-01/`, with
details in `docs/research/2026-08-31-worldgen-surface-desert-pyramid-candidate.md`. Error World
extra pyramids, complete structure mutation, global RNG/checkpoint parity, aggregate ordering, full
WLD differential, legacy deletion, `canRemoveLegacyWorldGen=false`, and 44 deferred
`ServerRelevant` rows remain open; this is `completed_partial`.

The Error World extra-Pyramid branch now has a separate typed candidate owner. It preserves the
source `Next(5,8)` count, integer `width / 4200 / errorWorldAdjustment` scaling, X-then-Y draws from
the cloud-to-rock-layer range, joint redraws for the strict center veto or `< 300` shimmer distance,
and deterministic retry accounting. It validates the random coordinate ranges and fails closed for
malformed dimensions without reading tiles or mutating structures. Focused TDD RED/GREEN verification,
the final probe, and the Simulation Release build all exit `0` under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-desert-pyramid-20260831-01/`, with
details in `docs/research/2026-08-31-worldgen-error-world-pyramid-candidate.md`. Shimmer publication,
first-active scan/`n--`, `Pyramid(...)` mutation, exact RNG/checkpoint parity, aggregate ordering,
full WLD differential, legacy deletion, `canRemoveLegacyWorldGen=false`, and 44 deferred
`ServerRelevant` rows remain open; this is `completed_partial`.

The beginning of `WorldGen.Pyramid` now has a separate typed pre-mutation
eligibility owner. `LegacyPyramidPreMutationEligibilityPolicy` preserves the
source order for active type/wall-151 overlap, dual-dungeon potential bounds,
Surface Is Desert/Error nearby types 151/203/25, and the universal nearby
dungeon-brick types 41/43/44. It reads only an immutable snapshot, returns a
typed rejection reason, and emits no random or tile/structure mutation. The
focused probe and Simulation Release build exit `0`; Tunnels regression also
passes. Evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-pre-mutation-20260831-01/`,
with the source contract in
`docs/research/2026-08-31-worldgen-pyramid-pre-mutation-eligibility.md`.
An earlier verifier attempt in that historical batch predates the SandPatches owner and is
retained as a diagnostic; the current bounded WorldGeneration verifier is available and passes
under the SandPatches corrected evidence directory and the current-tree Pyramid rerun directory.
Full Pyramid footprint/mutation,
RNG/checkpoint and WLD parity,
aggregate ordering, legacy deletion, `canRemoveLegacyWorldGen=false`, and 44
deferred `ServerRelevant` rows remain open; this is `completed_partial`.

## Recent verified boundaries

- `Main.GameUpdateCount` now has a source-backed transient server cursor owner:
  `WorldGameUpdateCountProjection` is instance-local, `DomeSimulation.GameUpdateCount` is
  read-only, active unpaused outer ticks advance once after the player phase, paused ticks do not
  advance, and WorldTimeRate `0`/`3` does not change the one-per-tick rule. The explicit `uint`
  wrap boundary is covered; the cursor is excluded from persistence and restored instances start
  at `0`. Focused WorldClock, adjacent WorldRules, Simulation Release builds, and Main inventory
  generation pass under
  `Build/diagnostics/main-field-property/game-update-count-20260831-01/`; current inventory is
  `members=696`, `migratedScope=695`, `identityExcluded=1`, `accepted-narrow=127`.
  `ResetGameCounter`, client/UI semantics, full Main parity, and legacy deletion remain deferred;
  this is `completed_partial`.

- `Main.invasionProgressWave` now has a source-backed transient projection boundary. The existing
  `WorldInvasionProgressProjectionSystem` preserves the supplied wave for valid invasion progress,
  retains the no-wave overload's `0` default, and suppresses the wave when progression is
  unavailable without widening `WorldProgressionState`. The focused WorldRules verifier and
  Simulation Release build exit `0`; Main inventory reports `members=696`, `migratedScope=695`,
  `identityExcluded=1`, and `accepted-narrow=126`. Evidence is under
  `Build/diagnostics/main-field-property/invasion-progress-wave-20260831-01/`, with the source
  contract in `docs/research/2026-08-31-main-invasion-progress-wave.md`. Wave scheduling, NPC wave
  counters, display lifetime, outbound publication, persistence, full protocol loopback, complete
  invasion parity, and legacy `Main.cs` deletion remain deferred; this is `completed_partial`.

- `Main.tileBlendAll` now has a source-backed immutable `LegacyTileBlendAllRegistry` with the
  exact sole explicit assignment, tile type `357`. Focused verification passed with the exact
  count/positive/negative membership contract, and the Simulation Release build passed with
  `0` warnings and `0` errors. Evidence is under
  `Build/diagnostics/main-field-property/tile-blend-all-20260831-01/`. The shine and glow tables
  remain deferred because they are client-rendering semantics without complete Simulation owners.

- `Main.tileShine2` now has a source-backed immutable `LegacyTileShine2Registry`. The current
  source contains 52 explicit true assignments plus the deterministic `262..268` loop, yielding
  59 unique tile types. Focused verification and the Simulation Release build both exited `0`,
  with `0` warnings/errors. Evidence is under
  `Build/diagnostics/main-field-property/tile-shine2-20260831-01/`. Numeric `tileShine` values,
  `tileGlowMask`, and client rendering parity remain deferred.

- `Main.tileShine` now has a source-backed `LegacyTileShineRegistry` with the exact 55-entry
  tile-to-value map from the current legacy source. The focused verifier checks representative
  values and zero defaults for missing types; the Simulation Release build exited `0` with zero
  warnings/errors. Evidence is under
  `Build/diagnostics/main-field-property/tile-shine-20260831-01/`. `tileGlowMask` and client
  rendering parity remain deferred.

- `Main.tileGlowMask` now has a source-backed `LegacyTileGlowMaskRegistry` with the exact 37
  legacy override values and explicit default `-1` semantics for unregistered or invalid tile ids.
  Focused verification and the Simulation Release build both exited `0` with zero warnings/errors.
  Evidence is under `Build/diagnostics/main-field-property/tile-glow-mask-20260831-01/`. Client
  glow-mask rendering parity remains deferred.

- Audited `Main.townNPCCanSpawn` and confirmed it is a dynamic initialization candidate table,
  distinct from the 39-entry `NPC.townNPC` static owner in `LegacyNpcTownRegistry`; it remains
  deferred until it has an independent dynamic owner and lifecycle verifier.

- `Main.townNPCCanSpawn` now has a separate `LegacyTownNpcSpawnCandidateRegistry` containing the
  exact 37-type candidate universe. It is intentionally not the runtime spawn array: unlock
  predicates, occupancy, priority, housing/world eligibility, and lifecycle remain deferred.
  Focused verification and Main inventory verification both exited `0`; evidence is under
  `Build/diagnostics/main-field-property/town-npc-candidate-20260831-01/`.

- `Main.maxLiquidTypes` now has a compatibility-capacity owner,
  `LegacyLiquidTypeCapacity.MaxLiquidTypes = 15`, kept separate from the four modeled
  `LiquidType` enum values. Focused verification and the Simulation Release build exited `0` with
  zero warnings/errors. Evidence is under
  `Build/diagnostics/main-field-property/max-liquid-types-20260831-01/`. This does not claim
  fifteen implemented liquid behaviors.

- `Main.noTrapsWorld` now has a persisted metadata and outbound protocol boundary:
  legacy WLD v266+ header metadata, compatibility import, `WorldMetadata`, the versioned Dome
  world snapshot, and V1456 WorldData flags15 bit0 preserve the flag. Trap placement and complete
  WorldGen behavior remain deferred. Focused import, protocol compatibility, inventory, and
  Simulation Release build evidence is under
  `Build/diagnostics/main-field-property/no-traps-world-20260831-01/`.

- `Main.skyblockWorld` now has a WLD v302+ metadata owner, v7 Dome persistence field,
  WorldGenerationRequest inheritance, and V1456 WorldData flags15 bit6 projection. Focused
  import/projection, persistence, protocol, inventory, and Simulation build evidence is recorded
  under `Build/diagnostics/main-field-property/skyblock-world-20260831-01/`; full Skyblock
  generation parity remains deferred.

- `Main.getGoodWorld` now has a source-backed WLD/import/generation-request and persistence owner:
  WLD v227+ header index 1 is preserved through `LegacyWorldMetadata`, compatibility projection,
  `WorldMetadata`, and `WorldGenerationRequest.IsGoodWorld`; `WorldPersistenceFormat` v8 and the
  optional Dome outer tail v37 preserve explicit true/false values while v226 and V36 remain
  unknown. Focused Persistence, WorldImport, and WLD verifier runs all exit `0`, and direct
  Simulation/Server/WorldCompatibility/Protocol Release builds are clean. The V1456 context now
  carries the field, but WorldData wire projection remains deferred because the current synthetic
  variant byte does not represent legacy `bitsByte12..bitsByte16`; no synthetic bit 7 was added.
  Evidence, source contract, fresh reruns, and the adjacent terrain spawn-clear regression check
  are under `Build/diagnostics/main-field-property/good-world-20260831-01/`,
  `...-20260831-02/`, and the final current-tree rerun `...-20260831-03/`.

- WorldGen Pyramid now has a readonly typed invocation contract in
  `LegacyPyramidStructureRequest`. It preserves the legacy origin, depth arguments, `noTunnel`
  mode, structure tile `151`, structure wall `34`, and default depths `75..125`, with fail-closed
  validation for malformed coordinates/depths. The focused verifier and Simulation Release build
  exit `0` under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-structure-contract-20260831-01/`.
  This is only a handoff contract after candidate/pre-mutation checks; Pyramid footprint,
  tile/wall/frame mutation, tunnel/features, RNG/checkpoint parity, publication, aggregate WLD
   parity, legacy deletion, and `canRemoveLegacyWorldGen` remain deferred.

- WorldGen Pyramid now has a separate source-backed footprint mutation owner in
  `LegacyPyramidFootprintMutation`. It preserves the first three random draws, odd-width
  type-151 pillar rows, source shape normalization, projected 3x3 active/type-151 wall
  selection, source loop order, typed sequence attribution, commit behavior, and fail-closed
  world/sequence bounds. The focused verifier exercises untouched tile-state preservation and
  exits `0`; the direct Simulation Release build and Main inventory verifier also exit `0`
  (`members=696`, `migratedScope=695`, `identityExcluded=1`, `accepted-narrow=126`). Evidence is
  under `Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-footprint-mutation-20260831-02/`,
  with the source contract in `docs/research/2026-08-31-worldgen-pyramid-footprint-mutation.md`.
  Wall-frame execution, tunnel/feature side effects, `noTunnel` behavior beyond the request
  handoff, exact global RNG/checkpoint parity, publication, aggregate ordering, full WLD
  differential, legacy deletion, `canRemoveLegacyWorldGen=false`, and 44 deferred
  `ServerRelevant` rows remain open; this is `completed_partial`.

- WorldGen Pyramid now has a dedicated wall-framing request boundary in
  `LegacyPyramidWallFrameRequestQuery`. For each ordered wall center emitted by the footprint
  owner, it expands the nine `SquareWallFrame` targets in the legacy x-major order, preserves
  duplicate targets and source metadata, and validates the complete 3x3 envelope atomically.
  The focused verifier covers two centers and 18 requests, unchanged snapshot/random/generation
  state, and fail-closed edge input. Fresh focused Pyramid, complete bounded WorldGeneration,
  Simulation, WorldGeneration verifier, and Server Release evidence exits `0`; the bounded
  verifier reports `280 PASS`, `40 CHECK`, and no anchored `FAIL`/`ERROR` lines. Evidence is under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-wall-framing-boundary-20260831-01/`,
  with the source contract in
  `docs/research/2026-08-31-worldgen-pyramid-wall-framing-boundary.md`.
  At that point `Framing.WallFrame` lookup/value semantics, frame mutation, wall-frame RNG,
  tunnel/features, aggregate ordering, full WLD differential, legacy deletion,
  `canRemoveLegacyWorldGen=false`, and 44 deferred `ServerRelevant` rows remained open; the
  value-evaluation boundary is recorded immediately below.

- WorldGen Pyramid now has a source-backed `Framing.WallFrame` value and typed mutation boundary.
  `LegacyWallFrameEvaluationQuery` preserves invalid/zero-wall paint and coating cleanup, the
  four cardinal mask bits, truncating/invisible-neighbor filtering, the complete large-wall and
  center-offset lookup tables, ordinary reset draws, wall-21's second draw, and two-bit
  non-reset `WallFrameNumber` semantics. `WorldTile.WallFrameX/Y` remain separate from tile
  `FrameX/Y`, so wall framing preserves tile-object coordinates while writing wall coordinates.
  `LegacyPyramidWallFrameCommandProjection` requires a
  complete in-world 3x3 center envelope and direct-neighbor targets, rejects malformed metadata,
  post-framing state, and empty-batch state consumption, and emits ordered duplicate-preserving
  commands with section guards. `LegacyWallFrameCommandCommitSystem` preflights sequence,
  mask/lookup, metadata, and section-version invariants before applying immutable tile results.
  The focused Pyramid evaluation, neighbor, framing, and full verifiers all exit `0`; the fresh
  serial Simulation, Pyramid verifier rebuild, Server, Main inventory, bounded WorldGeneration,
  style, manifest, state, and diff checks are recorded under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-wall-frame-evaluation-20260831-08/`.
  Pyramid tunnel/features, full SquareWallFrame integration outside Pyramid, client SceneMetrics
  visibility/network behavior, persistence/protocol serialization of the ephemeral wall-frame
  coordinates, aggregate ordering, exact global RNG/checkpoint parity, full WLD differential,
  legacy deletion, `canRemoveLegacyWorldGen=false`, and 44 deferred
  `ServerRelevant` rows remain open; this is `completed_partial`. The fresh current-tree
  Simulation, Pyramid verifier, Server, and WorldGeneration verifier Release builds exit `0`
  with `0` warnings and `0` errors. The bounded WorldGeneration rerun exits `0` with
  `280 PASS`, `40 CHECK`, and no anchored `FAIL`/`ERROR` or stale-constructor exception.

  Cross-session continuation instructions are in
  `docs/flowstate/task/2026-08-31-pyramid-wall-frame-evaluation-continuation.md`.

- WorldGen Pyramid now has a source-backed noTunnel opening side-effect boundary in
  `LegacyPyramidTunnelOpening`. It preserves the source `Next(2)` direction mapping, the
  footprint-supplied tunnel width, `Next(5,8)` opening height, `Next(20,30)` delay, source
  column walk, active type-151 clearing, wall-34 writes, and active type-53 conversion with
  shape reset. A projected tile map preserves repeated source writes, and commands publish
  atomically only after bounded immutable-snapshot validation. The focused verifier reports
  two columns, 15 typed commands, and three random samples; serial Simulation, Pyramid, and
  Server Release builds exit `0`, and the bounded WorldGeneration verifier reports `280 PASS`,
  `40 CHECK`, and no anchored `FAIL`/`ERROR` under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-tunnel-opening-20260831-01/`.
  The source contract is recorded in
  `docs/research/2026-08-31-worldgen-pyramid-tunnel-opening.md`. Buried chests, small piles,
  plants, pots, the final extended tunnel, aggregate ordering, exact RNG/checkpoint and
  WLD/extended-state parity, client visibility, persistence/protocol projection, legacy
  deletion, `canRemoveLegacyWorldGen=false`, and 44 deferred `ServerRelevant` rows remain open;
  this is `completed_partial` for the opening tile-side-effect boundary only.

- The Pyramid noTunnel continuation now has a `completed_partial` buried-chest tile boundary.
  `LegacyPyramidBuriedChestIntentPolicy` preserves the source `WorldGen.cs:28555` midpoint,
  item choices `848/857/934`, conditional retry draw, tenth-anniversary remap, and fixed
  `AddBuriedChest` arguments, with atomic `Structure`/sequence publication.
  `LegacyPyramidBuriedChestCommandProjection` maps the source `(i - 1, j - 1)` origin to a
  typed 2x2 type-21 footprint, source line `28555`, section guards, and deterministic tile
  frames, then commits it through `TileChangeCommitSystem`. Fresh focused/build evidence is under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-buried-chest-tile-20260831-01/`;
  downward safety scan, chest entity/item-fill registration, remaining pile/plant/pot/tunnel
  features, and the remaining Pyramid/WorldGen parity gates stay deferred.

- WorldGen cave random consumption now has a fresh source-only checkpoint artifact. The
  instrumented legacy `WorldGenerator.RunPass` resets `Main.rand` from the world seed before
  every pass, and each pass consumes one continuous `UnifiedRandom` stream until `Apply` returns;
  the default seed-1456 trace records the same initial state (`810676643`) for all ten cave
  passes. Nine passes consume samples and default `Wavy Caves` consumes none because its
  `dontStarveWorldGen` guard is false. This confirms only the random reset contract; cave
  schedule, TileRunner semantics, missing passes, side effects, and tile/extended-state parity
  remain blocked. Evidence is under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/resume-20260830-differential/` in
  `legacy-pass-random-trace-20260830-01.jsonl` and `cave-random-contract-20260830-01.json`.
  An independent raw-trace consistency check records 20 checkpoints, all 10 expected sample
  counts, and the `RandNext`/`Peek` normalization relation in
  `cave-random-consistency-20260830-01.json`. The bounded WorldGeneration verifier was rerun
  from the Release DLL with explicit `exitCode=0`, `279` PASS lines, and `0` FAIL/ERROR lines;
  `worldgeneration-verifier-run-summary-20260830-01.json` records that run. These results do
  not close generated-world tile, extended-state, metadata, command-sequence, or side-effect
  parity.
- The Version4 `DirtLayerCaves` pass now has a fresh source-backed typed boundary. The extracted
  owner preserves the `!Skyblock.denyAllGeneration` guard, `151/302` default/Remix invocation
  counts, exact beach and inclusive `0.45..0.55` center retry predicate, `-1/-2` selection,
  strength/step scaling, source random draw order (type, candidate X/Y, retry pairs, strength,
  steps), immutable Mount-stage snapshot input, and deterministic source-attributed
  `TileChangeCommand` emission. Focused verification passed with `3046` commands; the complete
  WorldGeneration verifier was rerun with exit `0`, `280` PASS lines, no anchored FAIL/ERROR lines,
  and empty stderr. This remains `completed_partial`: full TileRunner behavior, aggregate cave
  ordering, type-`-2` liquid side effects, WLD differential, and legacy deletion stay blocked.
  Evidence is under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/dirt-layer-caves-20260830-01/`, with the
  source contract in `dirt-layer-caves-source-contract-20260830.json` and boundary research in
  `docs/research/2026-08-30-worldgen-dirt-layer-caves-boundary.md`.
  Source provenance is now explicit: the historical `12406-12446` / `2460C020...` capture is
  retained, while the latest stable current file is `12492-12532` /
  `37374274D836785E33370F3CD9DD2A733D048930DCE65689A9DC69FD1E052FF3` after an 86-line shift.
  Prior current samples (`AB912157D8D9C5C738CACE7EF3863F04BBD2427A535B89C2851ECBB10B76F27B` and
  `90CDCBFBEF663C43AEAB8DA3C762C9FE8AEBAABC0736FD7B69BEB1D27D78D4E9`) grew by 41 and 4 lines
  while this block remained unchanged. The current block's contract fields match, but byte-identical
  historical snapshot equivalence is not established; all current provenance captures and the
  immutable excerpt are recorded beside the batch evidence. This does not change the partial
  boundary or deletion gate.
- The Version4 `RocksInDirt` pass now has a verified pass-specific typed boundary after the Dirt
  Wall snapshot. `LegacyRocksInDirtPassDefinition` preserves the three source densities/ranges,
  and `LegacyRocksInDirtPass` emits source-attributed commands through deterministic commit. The
  current focused verifier passes with `2916` commands on its bounded fixture; the default seed-1456
  `4200x1200` oracle comparison emits `285014` commands, applies all `285014`, matches all
  `24444` invocation/random checkpoints, and reports zero tile-field mismatches. Generic
  TileRunner semantics, extended state, aggregate cave ordering, full WLD differential, and
  legacy deletion remain deferred. Evidence is under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/rocks-in-dirt-boundary-20260830-12/`,
  `...-20260830-16/`, and `...-20260830-17/`; research is in
  `docs/research/2026-08-30-worldgen-rocks-in-dirt-boundary.md`.
- The Version4 `DirtInRocks` pass now has a verified pass-specific typed boundary after the
  RocksInDirt snapshot. `LegacyDirtInRocksPassDefinition` preserves density `0.005`, the
  `25,200` default invocations, source random draw order, target type `0`, and the Skyblock
  guard. `LegacyDirtInRocksPass` emits source-attributed commands through deterministic commit;
  its focused fixture also covers the independent Remix type `0`/`1` toggle contract. The
  default seed-1456 `4200x1200` probe consumes `3,724,072` random samples, emits/applies
  `515,925` commands, and reports zero active/type/liquid/frame/wall mismatches against the
  immutable DirtInRocks oracle snapshot. The corrected owner uses the legacy
  `Main.worldSurface=(int)(worldSurfaceHigh+25)` value (`325`, distinct from `GenVars.worldSurface`
  `229`) so type-53 preservation matches the oracle. Fresh focused/full verifier and build logs,
  plus the zero-mismatch JSON, are under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/dirt-in-rocks-boundary-20260830-08/`;
  research is in `docs/research/2026-08-30-worldgen-dirt-in-rocks-boundary.md`. Full Remix oracle
  parity, complete TileRunner semantics, aggregate cave ordering, full WLD parity, and legacy
  deletion remain deferred.
- The Version4 `Clay` pass now has a verified pass-specific typed boundary after the DirtInRocks
  snapshot. `LegacyClayPassDefinition` preserves the four source recipes and floor-truncated
  counts (`100`, `252`, and `100` for the captured non-Remix `4200x1200` profile), while
  `LegacyClayPass` preserves the pass-reset random draw order, projected writes, first-active
  five-row cleanup, priority ordering, and Skyblock no-op guard. The production owner consumes
  `522,600` random samples, emits/applies `63,216` commands, and compares zero active/type/liquid/
  frame/wall mismatches against the immutable Clay oracle snapshot. Focused and complete bounded
  verifier runs exit `0`; the production-vs-oracle artifact is under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/clay-boundary-20260830-03/`, with
  research in `docs/research/2026-08-30-worldgen-clay-boundary.md`. Full Remix oracle parity,
  complete TileRunner semantics, aggregate cave ordering, full WLD parity, and legacy deletion
  remain deferred.
- The Version4 `RockLayerCaves` base loop now has a pass-specific typed boundary after the Clay
  snapshot. `LegacyRockLayerCavesPassDefinition` preserves density `0.00013`, floor-truncated
  default cardinality (`655` invocations for `4200x1200`), `-1/-2` selection, strength `[6,20)`,
  steps `[50,300)`, and source random draw order `type -> strength -> steps -> X -> Y`.
  `LegacyRockLayerCavesPass` consumes an immutable Clay-stage snapshot, resets a pass-scoped
  random stream from the world seed, tracks projected writes privately, and emits only
  source-attributed kill commands through deterministic commit. Negative TileRunner commands carry
  `PreserveTileState`; projection clears only `IsActive`, retains type/frame/wall/other state,
  emits nothing for already-inactive candidates, and skips active type `53`. Profile-enabled generic
  fallback is skipped and the dedicated owner runs once after Clay. The fresh oracle under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/rock-layer-caves-boundary-20260830-05/`
  reaches the Rock Layer Caves stage, includes Clay input and Rock output snapshots, records 655
  invocations, and records seed-1456 random start peek `810676643` and end sample count `6945153`.
  Focused and complete bounded WorldGeneration verifier runs exit `0`; the focused fixture emits
  historical B4 focused evidence records `22445` commands; a fresh current-tree lightweight
  rerun in `Build/diagnostics/server-ecs-convergence/P9-worldgen/rock-layer-caves-boundary-20260830-08/`
  exits `0` with one command because its `800x300` fixture seeds one active tile. The formal
  pass-specific differential in `...-20260830-07/rock-layer-caves-boundary-green.json` has a
  successful probe build but intentionally exits `2` for the known traversal mismatch: it emits/
  applies `526468` commands and reports `774708` active, `191078` liquid-amount, and `84463`
  liquid-type mismatches across `5040000` tiles, with zero type/frame/wall mismatches. Fresh
  `...-20260830-08/` Simulation/verifier Release builds exit `0` with zero warnings/errors; the
  focused verifier exits `0`, and the complete verifier exits `0` with `280` PASS, `40` CHECK,
  and no FAIL/ERROR lines. This is `completed_partial`: Remix's paired no-Y-change loop and extra
  density, `-2` liquid side effects, complete TileRunner traversal, aggregate cave ordering, full
  WLD differential, legacy deletion, `canRemoveLegacyWorldGen`, and 44 deferred `ServerRelevant`
  rows remain open.
- The Version4 `SurfaceCaves` source block (`WorldGen.cs:12676-12785`) has a focused
  profile-owner correction. The source guard is `!Skyblock.denyAllGeneration &&
  !SecretSeed.noSurface.Enabled`, and the source order is vertical families, horizontal
  `noYChange`, then Caverer; the generic `surface-desert` recipe is not present in that source
  pass. `LegacyCavePassSystem` now skips only the profile-backed generic SurfaceCaves entry so
  the existing dedicated vertical/Caverer/Mountain owners remain authoritative, while no-profile
  compatibility remains. The focused RED/GREEN verifier uses seed `1456` and an `800x300` fixture:
  profile-enabled output has `2,907` commands and `0` generic commands, dedicated vertical
  provenance is present, and no-profile output retains `4,342` generic commands. The fresh
  `...-20260830-03/` rerun also records `125,479` commands and `412,918` random samples for the
  unified-owner replay, Release builds with zero warnings/errors, and the bounded verifier at
  `280 PASS / 40 CHECK / 0 FAIL/ERROR`. Evidence is under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-02/`
  and `...-20260830-03/`, with research in
  `docs/research/2026-08-30-worldgen-surface-caves-boundary.md`. This is `completed_partial`;
  shared random coordination, Remix/no-surface parity, complete
  Caverer/Mountain/TileRunner behavior, aggregate ordering, full WLD differential, legacy
  deletion, `canRemoveLegacyWorldGen=false`, and 44 deferred rows remain open.
- The Version4 `MountainCaves` source block (`WorldGen.cs:10839-10912`) now has a typed
  `LegacyMountainCavesPass` owner. It preserves truncated density plus Remix `1.5x`, strict
  center/history candidate vetoes, fail-closed Skyblock/no-surface/surface-desert guards, the
  `y < WorldSurface` scan, active tile veto types `53/151/274`, and Mountinater command handoff.
  The focused `--mountain-caves-only` verifier and serial Simulation build both exit `0`; evidence
  is under `Build/diagnostics/server-ecs-convergence/P9-worldgen/mountain-caves-20260831-02/`.
  This is `completed_partial`; Mountinater/TileRunner parity, aggregate ordering, global RNG/WLD
  differential, legacy deletion, `canRemoveLegacyWorldGen=false`, and 44 `ServerRelevant` rows
  remain deferred.
- The `WavyCaves` pass now has an explicit `LegacyWavyCavesPass` owner and request-level
  `IsDontStarveWorld` guard. Default worlds remain a verified no-op with no random consumption;
  enabled worlds preserve the quadratic `35 * (width / 4200)^2` count, Remix `/3` adjustment,
  linear `startX`, Y spacing retry, and `WavyCaverer` source commands. This is completed_partial;
  WavyCaverer/TileRunner parity, full DontStarve oracle differential, aggregate cave ordering,
  legacy deletion, and 44 deferred rows remain open.
  Focused harness evidence is under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/wavy-caves-boundary-20260831-01/`;
  it exits `0` with `20180` source-attributed commands and `1593` random samples.
- The IceBiome surface conversion now has a typed `LegacyIceBiomeSurfacePass` owner and
  immutable definition. It preserves the source tile mapping `0/2/23/40/53 -> 147`, `1 -> 161`,
  wall `2 -> 40`, lava-line random offset, snow drift clamp, and Skyblock no-op guard. The
  focused harness exits `0` with two commands on the seeded fixture; full snow-boundary state,
  snow arrays, biome structures, later Grass ordering, oracle parity, and legacy deletion remain
  deferred. Evidence is under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/ice-biome-boundary-20260831-01/`.
  The request now exposes an explicit `IsIceBiomeWorld` switch and the pipeline commits this
  owner only when enabled; default generation remains unchanged.
- The legacy `Grass` pass now has a typed `LegacyGrassPass` owner. It preserves the source
  `0.002` attempt density, two random Y ranges, center-empty/four-neighbor type-0 predicate,
  type-2 placement, source attribution, and Skyblock guard. The focused harness exits `0` with
  the WavyCaves, IceBiome, and Grass checks green; full Grass scheduling and global RNG parity
  remain deferred.
- The `MudCavesToJungleGrass` pass now has a bounded `LegacyMudCavesToJungleGrassPass` owner
  for the source-backed active mud `59 -> 60` conversion, deterministic scan, command provenance,
  and Skyblock guard. Focused harness evidence is green with one conversion command. `NotTheBees`,
  SurfaceIsMushrooms, SpreadGrass recursion, clump elimination, and full pass ordering remain
  deferred.
  The next legacy `DesertBiome` pass was inspected and remains deferred because its authoritative
  behavior is structure placement with failure/retry and dungeon-side state, not a standalone
  tile mapping contract.
- `LegacyDesertPlacementSelector` now captures the source candidate-position contract without
  pretending to implement `DesertBiome.Place`: dungeon-side signed offset, initial/retry random
  ranges, failure threshold direction flip, and `skipDesertTileCheck` after two flips. Focused
  harness exits `0`; structure blueprints, placement collision semantics, and full DesertBiome
  oracle parity remain deferred.
- `LegacyGlowingMushroomPass` now captures the Remix `GlowingMushroomPatches` empty-tile
  conversion (`type 0 -> 59`) with world-flag guards and source-attributed commands. The focused
  harness is green; full mushroom biome placement, SpreadGrass coupling, and global RNG parity
  remain deferred.
- IceBiome and GlowingMushroom type-only rewrites now preserve the source tile active bit via
  `IsActive`; the combined focused harness remains green after this state-preservation hardening.
- `LegacyDirtToMudPassDefinition` and `LegacyDirtToMudInvocationFactory` now preserve the
  source `0.001` density (`5040` invocations at `4200x1200`), target type `59`, override type
  `53`, strength `2..6`, steps `2..40`, and Remix/non-Remix Y-range input contract. The focused
  harness exits `0`; TileRunner traversal and full global RNG parity remain deferred.
- `LegacySiltPassDefinition` and `LegacySiltInvocationFactory` now preserve the source `0.0001`
  density (`504` invocations at `4200x1200`), target type `123`, strength `5..12`, steps
  `15..50`, `addTile` behavior, and wall veto for `187/216`. The focused harness exits `0`;
  full TileRunner traversal and global RNG parity remain deferred.
- `LegacyOresAndShiniesRecipe` and its Remix catalog now preserve the first nine source recipe
  densities, Y-band factors, strength/step ranges, normal/Drunk tile alternatives, and truncated
  invocation counts. The focused harness exits `0`; complete ore schedule, tier selection,
  TileRunner traversal, and global RNG parity remain deferred.
- Ores recipes now expose `CreateInvocation`, producing the existing `LegacyOreRunnerRequest`
  with deterministic X/Y, strength/steps and Drunk tile selection. The focused harness remains
  green; complete twelve-stage scheduling, tier mutation, and ore traversal are deferred.
- `LegacyOresAndShiniesRecipeCatalog.CreateNonRemixRecipes` now preserves the 13 source-confirmed
  non-Remix configuration entries: copper/iron/silver/gold layer bands, surface silver/gold, and
  crimson/corruption deep ore parameters. The focused harness and Simulation Release build both
  exit `0`; complete secret-seed/drunk scheduling, crimson selection, TileRunner traversal, RNG
  parity, and aggregate WorldGen parity remain deferred.
- `LegacyWebsPassDefinition` and `LegacyWebsInvocationFactory` now preserve the source Webs
  invocation boundary: density `0.0006` with source truncation (`3023` at `4200x1200`), x inset
  `20`, Y input range, strength `4..11`, steps `2..4`, type `51`, add-tile, horizontal direction,
  downward speed, and no-Y-change/overwrite flags. The focused harness and Simulation Release
  build exit `0`; cave-coordinate overrides, active/wall candidate scans, horizontal recovery,
  actual TileRunner traversal, RNG parity, and aggregate WebGen parity remain deferred.
- `LegacyWebsCandidateSelector` now preserves the source candidate gate and recovery chain:
  inactive initial tile, surface/wall veto, downward scan to `worldSurfaceLow`, post-scan `y++`,
  random horizontal direction, bounded inactive-tile recovery, and final surface/wall veto. The
  focused harness now has 12 PASS checks and the Simulation Release build remains green; cave
  coordinate overrides, complete TileRunner traversal, wall semantics, RNG parity, and aggregate
  WorldGen parity remain deferred.
- `LegacyWebsPass.CreateInvocations` now connects the source random initial point, candidate
  selector, strength/steps draws, and `LegacyTileRunnerPassInvocation` provenance. The focused
  harness reaches 13 PASS checks and the Simulation Release build exits `0`; cave-coordinate
  overrides, full TileRunner geometry/side effects, and aggregate RNG/WLD parity remain deferred.
- `LegacyUnderworldSurfaceColumnPolicy` now preserves the source Underworld top-column math:
  initial `height - Next(150,190)`, per-column `Next(-3,4)`, clamp to `height-190..height-160`,
  and the NotTheBees effective-top offset of `-30`. The focused harness reaches 14 PASS checks
  and the Simulation Release build exits `0`; terrain/liquid writes, TileRunner, houses, secret
  seed variants, RNG parity, and aggregate WorldGen parity remain deferred.
- `LegacyUnderworldLavaColumnPolicy` now preserves the second Underworld column track: initial
  `height - Next(40,70)`, per-column `Next(-10,11)`, upper clamp to `height-60`, lower clamp to
  `height-120`, and inactive-tile lava eligibility. The focused harness reaches 15 PASS checks
  and the Simulation Release build exits `0`; lava mutation/amount/type writes, `QuickWater`,
  TileRunner, secret-seed variants, RNG parity, and aggregate WorldGen parity remain deferred.
- `LegacyUnderworldAshRunnerFactory` now preserves the source ash-runner gate and request shape:
  `Next(50)==0`, scan from `height-65` down toward `height-135`, random X, Y offset `20..50`,
  strength `15..20`, steps `1000`, type `57`, `addTile=true`, zero horizontal speed, vertical
  speed `1..3`, and `noYChange=true`. The focused harness reaches 16 PASS checks and the
  Simulation Release build exits `0`; actual TileRunner traversal, ash/lava mutation, QuickWater,
  and aggregate WorldGen parity remain deferred.
- `LegacyUnderworldLiquidCaveOriginPolicy` now preserves the liquid-cave loop's `Next(13)==0`
  gate, `height-65` downward scan while liquid or active, and Drunk/Remix central-band
  suppression (`Next(3)==0` inside 40%..60%). The focused harness reaches 17 PASS checks and the
  Simulation Release build exits `0`; the multiple surface/evil TileRunner branches, liquid
  mutation, `QuickWater`, global RNG parity, and aggregate WorldGen parity remain deferred.
- `LegacyUnderworldSurfaceRunnerFactory` now preserves the first liquid-cave surface runner:
  `y-Next(2,5)`, strength `5..30`, steps `1000`, type `57`, `addTile=true`, zero horizontal
  speed, vertical speed `1..3`, and `noYChange=true`, while honoring the upstream central-band
  suppression. The focused harness reaches 18 PASS checks and the Simulation Release build exits
  `0`; the paired runners, evil runners, liquid mutation, QuickWater, and aggregate parity remain
  deferred.
- `LegacyUnderworldPairedSurfaceRunnerFactory` now preserves the two optional paired surface
  runners: `num6` initialization with optional half-scale, central-band suppression, independent
  1-in-2 emission gates, Y offset `2..5`, scaled strength/steps, and right/left speeds `+1/-1`
  with `speedY=0.3`. The focused harness reaches 19 PASS checks and the Simulation Release build
  exits `0`; evil runners, full TileRunner traversal, liquid mutation, and aggregate parity remain
  deferred.
- `LegacyUnderworldEvilRunnerFactory` now preserves the three evil-liquid runner configurations:
  unconditional base runner, optional `1/3` runner, optional `1/5` runner, their distinct X/Y
  offset ranges, strength/steps ranges, type `-2`, `addTile=false`, and random speed ranges.
  The focused harness reaches 20 PASS checks and the Simulation Release build exits `0`; actual
  liquid mutation, TileRunner traversal, QuickWater, and aggregate WorldGen parity remain deferred.
- `LegacyUnderworldLowerEvilRunnerFactory` now preserves the unconditional lower-world runner:
  X range `20..maxTilesX-20`, Y range `maxTilesY-180..maxTilesY-10`, strength/steps `2..7`, type
  `-2`, `addTile=false`, and zero-speed default flags. The focused harness reaches 21 PASS checks
  and the Simulation Release build exits `0`; actual TileRunner traversal, liquid mutation,
  QuickWater, and aggregate WorldGen parity remain deferred.
- `LegacyUnderworldExtraEvilRunnerFactory` now preserves the Drunk/Remix-only extra loop: count
  `maxTilesX * 2`, central X range `35%..65%`, lower-world Y range, strength `5..20`, steps
  `5..10`, type `-2`, and no-op default TileRunner flags; ordinary worlds produce zero requests.
  The focused harness reaches 22 PASS checks and the Simulation Release build exits `0`; actual
  traversal, liquid mutation, QuickWater, and aggregate WorldGen parity remain deferred.
- `LegacyUnderworldLavaShelfPolicy` now preserves the two fixed lava shelf rows at
  `height-145` and `height-144`, with inactive-tile-only eligibility. The focused harness reaches
  23 PASS checks and the Simulation Release build exits `0`; actual liquid amount/type writes,
  `QuickWater`, and aggregate WorldGen parity remain deferred.
- `LegacyUnderworldObsidianRunnerFactory` now preserves the fixed type-58 runner loop: density
  `0.0008` (source-truncated count `4032` at `4200x1200`), X/Y ranges, strength `2..7`, steps
  `3..7`, type `58`, and default non-overwrite request flags. The focused harness reaches 24 PASS
  checks and the Simulation Release build exits `0`; actual TileRunner traversal, mutation,
  `QuickWater`, and aggregate WorldGen parity remain deferred.
- `LegacyUnderworldAshBrickPolicy` now preserves the non-Remix/Drunk ash-brick eligibility:
  outer columns (`25..width-25` and outside 17%..83%), active type `57`, and any inactive tile in
  the eight-neighbor set. Remix worlds fail closed. The focused harness reaches 25 PASS checks
  and the Simulation Release build exits `0`; type-633 mutation, tree growth, house placement,
  and aggregate WorldGen parity remain deferred.
- `LegacyUnderworldAshTreeGrowthPolicy` now preserves the ash-tree growth eligibility: outer
  columns, Y range `height-200..height-50`, active type `633`, inactive tile above, and the
  `Next(3)==0` gate; Remix worlds fail closed. The focused harness reaches 26 PASS checks and the
  Simulation Release build exits `0`; tree structure/frame placement and aggregate parity remain
  deferred.
- `LegacyBuffNoTimeDisplayRegistry` now owns the exact 118-entry `Main.buffNoTimeDisplay` set
  from the Version4 initialization source, including the contiguous `284..304` range, with a
  frozen membership projection and boundary checks. The focused harness reaches 27 PASS checks
  and the Simulation Release build exits `0`; buff UI timing, effect lifecycle, persistence,
  protocol projection, and complete Main parity remain deferred.
- `LegacyBuffNoSaveRegistry` now owns the exact 99-entry `Main.buffNoSave` set from the Version4
  initialization source, including the contiguous `173..181` range, with frozen membership and
  boundary checks. The focused harness reaches 28 PASS checks and the Simulation Release build
  exits `0`; save/load lifecycle, player reset integration, protocol projection, and complete
  Main parity remain deferred.
- Registry ownership was centralized for tile categories, paint/coating/moss colors, hollow-tree
  styles, tree supports, boulders, room-needs, and related Main/WorldGen predicates.
- Projectile motion and world-item pickup reject non-finite input before mutation.
- NPC target routing and selection fail closed for invalid encoded targets, identities, lifecycle
  state, stable IDs, and positions.
- NPC slot allocation now has a typed, deterministic narrow boundary: inactive replication slots
  are reused before active `CanBeReplaced` slots, reused handles advance revisions, and stale
  Training Dummy ownership is cleared before replacement; `spawnSlotProtected` timing and full
  `NPC.Spawner` semantics remain deferred. Fresh gates are under
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-slot-replacement/`.
- NPC `homelessDespawn` now has a typed lifecycle owner and an explicit spawn-input chain; mark/
  clear operations and in-memory state snapshots preserve the flag. `WorldGen.UnspawnHomelessNPC`,
  player safe-area checks, relocation clearing, and world-save/network parity remain deferred.
  Fresh gates are under `Build/diagnostics/npc-complete/task-10-interaction/20260830-homeless-despawn/`.
- NPC `lookForHomeTimeout` now has a typed `NpcHomeComponent` owner with source-backed kick-out
  (`3600`) and move-room (`0`) transitions, a zero-ready predicate, and a lifecycle-stage tick
  restricted to active NPCs that actually have Home state. TownManager/room scanning, player-safe
  areas, relocation/unspawn, and network/save projection remain deferred. Fresh gates are under
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-home-search-timeout-green/`.
- NPC home publication now has a separate `NpcHomePublicationComponent` baseline owner for the
  source `oldHomeless/oldHomeTileX/oldHomeTileY` tuple. `NpcHomePublicationSystem` preserves the
  compare-before-write update shape and explicit baseline capture; `NpcStateSnapshot` retains the
  sidecar only when Home state is explicitly present. `NpcHomeSnapshot`, SyncNPC, and network send
  scheduling remain unchanged/deferred. Fresh focused gates are under
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-home-publication-baseline-green/`.
- NPC `trapImmune` now has an explicit definition capability and immutable authority owner. The
  authoritative spawn commit copies the capability, and projectile NPC eligibility rejects only
  the source-backed `IsTrap && IsTrapImmune` combination. Complete trap damage/reflection, dynamic
  AI writes, type tables, and network/save projection remain deferred. Fresh focused gates are
  under `Build/diagnostics/npc-complete/task-10-interaction/20260830-trap-immunity-green/`.
- NPC `lavaImmune` now has an explicit definition capability and immutable authority owner. The
  authoritative spawn and registered-definition restore paths copy the capability; finite lava-tile
  overlap produces a typed `Lava` NPC damage command, and settlement revalidates source identity,
  active state, immunity, and the capability before applying bounded source damage. Fresh serial
  Simulation/NPC/Combat/Server/NPC protocol gates, verifier runs, diff check, and checkpoint parse
  are under `Build/diagnostics/npc-complete/task-10-interaction/20260830-lava-immunity-final-green-rerun/`.
  Full `Collision_LavaCollision`, `lavaWet`, buff/network effects, all static/dynamic type defaults,
  liquid physics, and complete NPC/AI parity remain `partial/deferred`.
- NPC `reflectsProjectiles` now has a typed mutable owner on
  `NpcBehaviorStateComponent` (default `false`) and a source-backed pure
  `ProjectileReflectionEligibilityPolicy.CanBeReflectedByNpc` query. The policy
  requires the NPC flag, active projectile, friendly/non-hostile flags, positive
  runtime damage, and the legacy type/AI-style whitelist. Fresh TDD RED/GREEN,
  serial Simulation/Combat/Server clean builds, Combat verifier, and NPC protocol
  shape evidence are under
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-reflect-projectiles-{red,green}/`.
  Full NPC hitbox reflection, `Main.player`/random/FX side effects, damage and
  penetration mutation, replication/persistence, dynamic AI writes, and complete
  AI families remain `partial/deferred`.
- NPC inactivity preservation now has a source-backed
  `LegacyNpcInactivityRegistry` and lifecycle gate: the 61 unconditional types, type
  `139`/`134` and `552..563`/`566..578` companion gates, the unconditional `564/565` gap,
  and the separate type `668` slot-counting exception are covered. Out-of-range types
  and null active-type context are rejected. `DomeSimulation` supplies the per-tick
  active typed `NpcDefinitionComponent.NetId` set; `NpcLifecycleSystem` skips only the
  inactivity timer for positive-health protected NPCs while preserving lethal/immortal
  ordering. Full `CheckActive`, player-range, AI/event, worm cleanup,
  network/persistence, and full NPC parity remain `partial/deferred`. Fresh TDD and serial
  gates are under `Build/diagnostics/npc-complete/task-10-interaction/20260830-inactivity-lifecycle-green/`.
- NPC static `townNPC` inactivity preservation now has a separate
  `LegacyNpcTownRegistry` with the exact 39 effective IDs from 32 `SetDefaults` branches
  (including `637 || 638` and `678..684`). `DomeSimulation` passes the
  typed net-id result into `NpcLifecycleSystem`; positive-health town NPCs keep `TimeLeft`, while
  lethal and immortal ordering remains unchanged. `isLikeATownNPC` type 453, type 690's AI guard,
  dynamic `townNPCCanSpawn`, full `CheckActive`, housing/services, and complete NPC parity remain
  `partial/deferred`. Fresh TDD RED/GREEN, the corrected source-set audit
  (`source-audit-corrected.log`), corrected NPC/Simulation/Server/NPC protocol gates, and
  hygiene evidence are under
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-town-inactivity-lifecycle-green/`.
- The following `CheckActive` read, `type == 690 && ai[0] == 0f`, was audited and intentionally
  remains deferred. Legacy `SetDefaults` gives type 690 `aiStyle = 126`, `immortal = true`, and
  `dontTakeDamage = true`, while current Simulation has no complete AI-126 definition or generic
  `ai[0]` state owner. The broad immortal timer priority already exists, but it is not type-690
  parity; evidence is under
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-type690-inactivity-audit/`.
 - The following `NPC.CheckActive` player-range/timer-refresh chain was audited as
   `completed_partial` for source/runtime inventory. A new `NpcActivityRangePolicy` now owns the
   source-backed pixel-space rectangles and pure active/screen intersection decision, with
   `Version1456` constants and finite/overflow-safe validation. It is not wired into lifecycle.
   Legacy still uses a fixed `Main.player[0..254]` scan, per-player `nearbyActiveNPCs`, timer
   refresh, boss/type/AI exceptions, and deactivation side effects. Current Simulation still has
   no closed pixel-to-tile integration, fixed player-slot mirror, per-player accounting, generic
   AI/boss owner, or typed deactivation side-effect chain. Source audit evidence is under
   `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-player-range-audit/`;
   TDD RED/GREEN and serial owner gates are under
   `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-pixel-range-{red,green}/`.
- `NPC.CheckActive` 的 nearby-slot contribution 分支现在有独立的
  `NpcActivitySlotContributionInput`、`NpcActivitySlotContribution` 和
  `NpcActivitySlotContributionPolicy`。该 owner 保留 NPC active guard、type `25/30/33` 排除、
  `releaseOwner == 255`、`lifeMax > 0`，并在已知 active-range 命中后仅对 Slime Rain type `1`
  应用 `0.65f` 权重；它不写 `PlayerStore`、`nearbyActiveNPCs`、timer 或生命周期。focused
  NPC verifier、Simulation/Protocol/Server 串行 Release builds 均为 exit `0`，证据在
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-slot-contribution-green/`。
  完整 fixed player-slot mirror、per-player counter、CheckActive side effects、Slime Rain
  event 和 NPC parity 仍为 `partial/deferred`；central checkpoint 保留已记录的 WorldGen
  active batch，不因该独立 NPC slice 覆盖。
- `NPC.Spawner` 的 `npcSlots` invasion-boss cap 现在有 source-backed typed owner：
  `LegacyNpcInvasionBossRegistry` 固定 exact 七类型，`NpcInvasionBossCapPolicy` 保留 active
  过滤、single-precision per-player/global cap 公式与 weighted `npcSlots` 比较，
  `NpcInvasionSpawnState.ReachedInvasionBossCap` 由 `NpcSpawnEligibilitySystem` 作为 invasion
  candidate gate。focused verifier 的 RED/GREEN/build/run 证据在
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-invasion-boss-cap-{red,green}/`；
  完整 `NPC.Spawner`、event spawn、`nearbyActiveNPCs`、invasion table、动态 spawn-rate 曲线和
  NPC parity 仍为 `partial/deferred`，总体状态不变。
- `NPC.CheckActive` 的 active-player keep-alive 分支现在有独立的 typed pure owner：
  `LegacyNpcCheckActiveKeepAliveRegistry` 固定 17 个 source 类型
  (`7,10,13,35,36,39,87,127,128,129,130,131,392,393,394,491,492`)，
  `NpcCheckActiveKeepAlivePolicy` 保留 active-player gate、Boss/static keep-alive、type `399`
  的 `ai[0] == 1f || 2f` refresh，以及 types `583..585` 夜间 `ai[2] == 0f` keep/refresh。
  focused verifier、Simulation/Protocol/NPC verifier/Server 串行 Release builds 均为 exit `0`，
  最终 fresh 复跑也全部通过；RED/GREEN/final/source audit 证据在
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-keepalive-{red,green,final}/`；
  完整 `CheckActive`、timer/deactivation、player-slot mirror、generic AI、event/boss parity 和
  NPC parity 仍为 `partial/deferred`，central WorldGen active batch 与
  `canRemoveLegacyWorldGen=false` 不变。
- `NPC.CheckActive` 的 screen-range timer refresh 现在有独立的 typed pure owner：
  `NpcCheckActiveTimerRefreshPolicy` 消费前置 geometry query 的 screen-range hit，保留
  `activeTime=750`、active-NPC gate、type `668` slot-counting bypass，并在命中时返回
  `timeLeft=750` 与 `despawnEncouraged=false`。focused verifier、Simulation/Protocol/NPC
  verifier/Server 串行 Release builds 均为 exit `0`；TDD RED/GREEN/final/source boundary
  证据在 `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-timer-refresh-{red,green,final}/`
  与 `docs/research/2026-08-30-npc-checkactive-timer-refresh-owner.md`。该 owner 不写
  `NpcLifecycleComponent` 或玩家集合，完整 timer/deactivation integration、fixed player
  slots、nearby accounting、AI/event/boss side effects 和完整 NPC parity 仍为
  `partial/deferred`。
- `NPC.CheckActive` 的 timeout deactivation 现在有独立的 typed pure owner：
  `NpcCheckActiveDeactivationPolicy` 保留 player-loop 后的一次 `timeLeft--`、keep-alive/
  timer-zero 判定，以及 timeout 时的 `active=false`、`life=0` 和 skip-next-cycle intent；
  `NpcSpawnCycleStateComponent` 以显式 world gate 保留 legacy static `noSpawnCycle` 的
  mark/consume-once 语义。TDD RED/GREEN/final、Simulation/Protocol/NPC verifier/Server
  串行 Release builds 均为 exit `0`，focused verifier 为 20 PASS、0 FAIL/ERROR；证据在
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-deactivation-{red,green,final}/`
  与 `docs/research/2026-08-30-npc-checkactive-deactivation-owner.md`。该 owner 不接入
  `DomeSimulation`/`NpcLifecycleSystem`/spawn pipeline，也不写 SyncNPC、revenge、worm 或
  玩家集合；完整 `CheckActive`/spawn/deactivation integration 和 NPC parity 仍为
  `partial/deferred`，`canRemoveLegacyWorldGen=false` 不变。
- `NPC.CheckActive_WormSegments` 现在有独立的 typed chain owner：
  `NpcCheckActiveWormSegmentPolicy` 保留 `aiStyle == 6` 触发、`(int)ai[0]` 截断投影、
  `num != whoAmI && 0 < num < maxNPCs` 范围、active-worm child gate、self/repeated-link
  termination，并为每个合格 child 产生 `SegmentRootRemoved` despawn command。TDD RED/GREEN
  focused verifier build/run 均为 exit `0`（RED 以 16 个缺失 owner/API 符号退出 `1`）；证据
  位于 `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-worm-segments-{red,green}/`，
  source boundary 见 `docs/research/2026-08-30-npc-checkactive-worm-segment-owner.md`。该 owner
  不接入 `DomeSimulation`、`NpcLifecycleSystem`、message-23、worm 构造/shared-life/movement、
  loot、revenge 或网络；完整 worm/CheckActive/NPC parity 继续 `partial/deferred`。
- `NPC.checkDead` 的四组 source-backed special transform/spawn 前置分支现在有独立的 typed
  pure owner：`NpcCheckDeadSpecialTransitionPolicy` 保留 `396/397` 的 `ai[0] = -2`、lifeMax、
  damage immunity/net-update 和 type-400 child spawn intent（含中心坐标向零截断与 child
  `ai[3]`），`398` 的 `ai[0] = 2`，`517/422/507/493` 的 `ai[2] = 1` + `ai[1] = 0`，以及
  `548` 的 `ai[1] = 1` + `ai[0] = 0`。TDD RED/GREEN focused verifier、source audit 和
  fresh Simulation/Protocol/NPC verifier/Server 串行 gate 均为 exit `0`（RED 仅因 21 个新
  owner/API 符号缺失退出 `1`），证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkdead-special-transition-{red,green,final-rerun-03}/`，
  source boundary 见 `docs/research/2026-08-30-npc-checkdead-special-transition-owner.md`。
  该 owner 不接入 death pipeline、spawn allocator、type-400 child initialization、message-23、
  Good World、town tombstone、loot、事件/invasion 或最终 active/noSpawnCycle 写入；完整
  `checkDead`、death/loot/network/persistence parity、AI/NPC parity 继续 `partial/deferred`。
- `NPC.checkDead` 的入侵进度扣减分支现在有独立的
  `NpcCheckDeadInvasionProgressPolicy`。它保留 `GetNPCInvasionGroup` 四组映射、组匹配 guard、
  `216 -> 5`、`395/491/471 -> 10`、`472/387 -> 0` 与其他入侵类型的默认 1 点，正点数扣减
  时将剩余规模下限截到零并计算 source progress-start。TDD RED/GREEN focused build/run
  均通过；证据在
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-invasion-progress-{red,green}/`，
  研究见 `docs/research/2026-08-31-npc-checkdead-invasion-progress-owner.md`。WorldProgression、
  message-78、事件、loot 与完整 `checkDead` 接入继续 deferred。
- `NPC.checkDead` 的 `noSpawnCycle = true` 写入现在有独立的
  `NpcCheckDeadSpawnCyclePolicy`。qualified death 始终产生 mark intent，未通过 qualification
  时 fail closed，实际存储/consume-once 仍由 `NpcSpawnCycleStateComponent` 负责。TDD RED/GREEN
  及最终 Simulation/NPC verifier/Server 串行门均通过，证据在
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-spawn-cycle-{red,green,final}/`，
  研究见 `docs/research/2026-08-31-npc-checkdead-spawn-cycle-owner.md`。spawn scheduler、
  CheckActive integration 与完整 checkDead 继续 deferred。
- `NPC.checkDead` 的 Good World type-631 projectile 分支现在有独立的
  `NpcCheckDeadGoodWorldProjectilePolicy`。它保留 type `99`、NPC center、零速度、伤害 `70`、
  击退 `10f` 与 owner intent，并拒绝非 finite 坐标；Good World runtime gate、allocator、网络、
  loot/event pipeline 仍未接入。TDD RED/GREEN、Simulation/NPC verifier/Server 最终门证据位于
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-goodworld-projectile-{red,green,final}/`，
  研究见 `docs/research/2026-08-31-npc-checkdead-goodworld-projectile-owner.md`。
- `GetNPCInvasionGroup` 的四组 source-backed 类型表现在由
  `LegacyNpcInvasionGroupRegistry` 独立持有，并由 N3.137 progress policy 复用。registry
  固定完整 32 个 grouped types、bounded `0..696` domain 和 fail-closed lookup；TDD RED/final
  serial gates 均通过，证据在
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-invasion-group-registry-{red,final}/`，
  研究见 `docs/research/2026-08-31-npc-invasion-group-registry-owner.md`。world invasion
  mutation、message-78、事件和完整 checkDead 仍 deferred。
- `DropTombstoneTownNPC` 的 projectile type 选择现在由
  `NpcTombstoneProjectileTypePolicy` 独立持有，保留 type `17/441` 的 `527..531` 与其他类型
  的 `43/201..205` 映射，并校验外部随机样本范围。TDD RED/final serial gates 均通过，证据在
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-tombstone-projectile-{red,final}/`，
  研究见 `docs/research/2026-08-31-npc-tombstone-projectile-type-owner.md`。随机源、tombstone
  实体、死亡文本和完整 town-NPC death pipeline 继续 deferred。
- N3.137 的 invasion progress decision 现在有
  `NpcCheckDeadInvasionProgressCommandPolicy` typed bridge，可构造现有
  `WorldInvasionProgressCommand`；sequence 由上游 authority 提供，bridge 不修改世界状态。
  RED/final serial gates 均通过，证据在
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-invasion-progress-command-{red,final}/`，
  研究见 `docs/research/2026-08-31-npc-checkdead-invasion-progress-command-owner.md`。death
  publication 接入、sequence allocator、world mutation 和 message-78 仍 deferred。
- Server outbound flushing uses a bounded writer queue without blocking the Simulation tick loop.
- World event transitions, clock/rate restoration, and bounded authority projections remain
  source-backed slices, not complete Main parity.
- `Main.lightPet` now has a completed narrow static-classification slice: the Version4
  `BuffID.Count=389` table's exact 12-entry set is owned by the immutable
  `LegacyLightPetBuffRegistry`; the focused Combat verifier covers exact membership, default and
  out-of-range `false` behavior, capacity, and the `FrozenSet` return. Fresh serial Simulation,
  Combat, and Main inventory gates are recorded under
  `Build/diagnostics/main-field-property/light-pet-20260830-01/`; inventory remains
  `members=696`, `migratedScope=695`, `identityExcluded=1`; the lightPet-only baseline was
  `accepted-narrow=106`; the current inventory is `accepted-narrow=110` after the independent
  persistentBuff, debuff, and vanityPet slices. Buff-instance
  lifecycle, pet spawning, lighting/rendering, protocol/persistence, and complete Main parity stay
  `partial/deferred`.
- `Main.persistentBuff` now has a completed narrow static-classification slice: the Version4
  `BuffID.Count=389` table's exact eight-entry set (`71,73,74,75,76,77,78,79`) is owned by the
  immutable `LegacyPersistentBuffRegistry`. The source-backed consumer is `Player.cs:10018`,
  where the table filters buff instances during player reset; this slice does not claim complete
  buff-instance lifecycle, save/load, network persistence, or player-reset parity. Fresh serial
  Simulation and Combat focused gates plus the updated Main inventory gate are recorded under
  `Build/diagnostics/main-field-property/persistent-buff-20260830-01/`; the persistentBuff batch
  baseline was `members=696`, `migratedScope=695`, `identityExcluded=1`, `accepted-narrow=107`;
  current inventory is now `accepted-narrow=110`, and
  `DEFERRED-GUARD-AUDIT: 0/0`. Full Buff persistence and Main field/property parity remain
  `partial/deferred`.
- `Main.debuff` now has a completed narrow static-classification slice: the Version4
  `BuffID.Count=389` table's exact 71-entry set is owned by the immutable
  `LegacyDebuffRegistry`. The source-backed consumers are `Player.cs:3581` and
  `NPC.cs:76411`, where the classification participates in Buff-slot selection; this slice does
  not claim complete debuff effect, immunity, duration, lifecycle, save/load, or protocol parity.
  Fresh serial Simulation/Combat focused gates and the updated Main inventory gate are recorded
  under `Build/diagnostics/main-field-property/debuff-20260830-01/`; current inventory is
  `members=696`, `migratedScope=695`, `identityExcluded=1`, `accepted-narrow=110`, and
  `DEFERRED-GUARD-AUDIT: 0/0`. Full debuff behavior, Buff persistence, and Main field/property
  parity remain `partial/deferred`.
- `Main.vanityPet` now has a completed narrow static-classification slice: the Version4
  `BuffID.Count=389` table's exact 74-entry set is owned by the immutable
  `LegacyVanityPetBuffRegistry`. The source-backed consumers are `Item.cs:978` and
  `Player.cs:3641/3647`, where the classification distinguishes vanity-pet Buff slots; this
  slice does not claim complete vanity-pet spawning, Buff effects, AI, rendering, save/load, or
  protocol parity. Fresh serial Simulation/Combat focused gates and the updated Main inventory
  gate are recorded under
  `Build/diagnostics/main-field-property/vanity-pet-20260830-01/`; current inventory is
  `members=696`, `migratedScope=695`, `identityExcluded=1`, `accepted-narrow=110`, and
  `DEFERRED-GUARD-AUDIT: 0/0`. Full vanity-pet behavior, Buff persistence, and Main field/property
  parity remain `partial/deferred`.
- `Main.pvpBuff` is now a `completed_partial` server-authoritative narrow slice. Legacy declares
  `new bool[BuffID.Count]` at `Main.cs:479`, with `BuffID.Count=389` and exactly 16 static `true`
  entries (`20,24,30,31,36,39,44,69,70,103,119,120,137,320,323,324`) at `Main.cs:6109-6124`.
  The confirmed consumer is message 55 relay filtering in `MessageBuffer.cs:2018-2027`; its
  wire shape is `byte player`, `ushort buff`, `int duration`.
  `AddPlayerBuffPvpPacket` and the catalog/codec/session/dispatcher chain preserve the target,
  while `NetworkInboundEnvelope.PlayerSlot` remains sender provenance. `DomeServer` validates
  sender/target active state and distinctness, `LegacyPvpBuffRegistry` owns the immutable
  allowlist, and `DomeSimulation` commits a deterministic status command visible through
  `PlayerStatusEffectStateSnapshot`.
  Fresh evidence is under `Build/diagnostics/main-field-property/pvp-buff-20260830-02/`:
  `verifier-run.log` includes the TCP loopback path and exits `0`; Protocol, Simulation, Server,
  and inventory builds/runs also exit `0`, with inventory `members=696`, `migratedScope=695`,
  `identityExcluded=1`, `accepted-narrow=110`, `DEFERRED-GUARD-AUDIT: 0/0`.
  Client relay/presentation, message-50 bootstrap, full Buff lifecycle/effects/immunity/stacking/
  duration, world-level PvP gate, save/load, complete Main parity, Legacy deletion, and WorldGen
  deletion remain `partial/deferred`.

- `Item.flaskTime` now has a source-backed `completed_partial` duration boundary. The immutable
  `ItemDefinition.FlaskDurationTicks` owns `72000` ticks; `LegacyFlaskDefinitionRegistry` preserves
  eight item-to-buff mappings (`1340 -> 71`, `1353 -> 73`, `1354 -> 74`, `1355 -> 75`, `1356 -> 76`,
  `1357 -> 77`, `1358 -> 78`, `1359 -> 79`); the legacy adapter rejects conflicting metadata;
  `DomeSimulation` registers the definitions and `ItemUseSystem` applies the same duration through
  `BuffCollectionComponent`. Fresh serial Simulation, Items Definitions, Items, Items Loopback and
  Server Release gates exit `0`; evidence is under
  `Build/diagnostics/item-flask-duration-final-20260831/`. Full Flask effects/modifiers, target
  filtering, damage/hit behavior, UI, protocol, persistence and complete Item parity remain
  `deferred`; `noWet` still lacks a real wet-state owner.
  The current-tree rerun is under `Build/diagnostics/item-flask-duration-final-20260831-rerun-01/`;
  its final gate-status records all build/run gates as `0` (an initial incremental Definitions
  `CS0006` was resolved by the direct serial Definitions rerun).

- `Item.OnPurchase` now has a source-backed `completed_partial` typed cleanup/receipt boundary.
  Legacy `Item.cs:270-272,49689-49708` is represented by immutable `ShopPurchaseReceipt`: only an
  authoritative offer with `CustomPriceCopper.HasValue` resets both custom price and special
  currency in the cleanup projection. Ordinary-currency and Item-backed special-currency purchases
  return the receipt only after atomic settlement; queued, forged-session, invalid-NPC and other
  rejected paths publish no receipt. Items verifier reports `67 PASS`; Simulation, Items Definitions,
  Items Loopback and Server Release gates exit `0`, with evidence under
  `Build/diagnostics/item-shop-onpurchase-final-20260831-03/`. Dynamic catalog/cache mutation,
  dynamic OnPurchase reset consumption, discounts/price adjustments, client/UI, protocol,
  persistence, bank inventories and complete Terraria shop parity remain `deferred`.

- Batch FZ now consumes the `OnPurchase` cleanup receipt through the authoritative
  `ShopOfferCatalogSystem`. A custom-price receipt rewrites the registered offer to
  `CustomPriceCopper = null` and `SpecialCurrencyId = -1`; no-custom special-currency receipts are
  no-ops, and mismatched receipt identity fails closed. `DomeSimulation` applies the consumer only
  after accepted inventory settlement, so queued/rejected purchases leave the catalog unchanged and
  same-tick follow-up purchases read the cleaned offer. Items verifier reports `69 PASS`; Simulation
  and Items Release builds exit `0`; the Items Loopback serial rerun exits `0`. Evidence is under
  `Build/diagnostics/item-shop-catalog-cleanup-final-20260831/`. Dynamic NPC catalog generation,
  discounts/price adjustments, client/UI, protocol, persistence, bank inventories and complete
  Terraria shop parity remain `deferred`.

- Batch GA now gives the resolved generated-shop result a narrow authoritative refresh/cache owner.
  `DomeSimulation.TryRefreshGeneratedShopCatalog` validates every offer against the ItemDefinition
  registry and StackLimit before `ShopOfferCatalogSystem.ReplaceGeneratedOffers` replaces the shared
  catalog; duplicate, invalid, unknown-item, and partial candidates leave the previous catalog
  unchanged, while an empty result clears it. Items verifier reports `70 PASS`; Items, Simulation,
  and Definitions focused builds/runs exit `0`. Evidence is under
  `Build/diagnostics/item-shop-catalog-refresh-20260831-01/`. NPC condition evaluation, slot/travel
  shop projection, dynamic NPC catalog generation, discounts/price adjustments, client/UI,
  protocol, persistence, bank inventories and complete Terraria shop parity remain `deferred`.

- Projectile B248 now has a bounded `netUpdate` scheduling handoff. Oracle
  `Projectile.cs:15245-15269` promotes `netUpdate2`, sends while `netSpam < 60` with a `+5`
  increment, defers at saturation, decays the budget each tick, and clears the primary flag at
  tick end. `ProjectileNetworkUpdateComponent.SendRequested` retains that per-tick policy result;
  ordinary/NPC replication snapshots carry `NetworkUpdateReady`, and
  `CombatReplicationAssembler` gates already-sent active revisions while preserving first-send,
  PVS re-entry, and tombstone delivery. Focused Combat/Combat.Protocol verifiers, serial
  Simulation/Server/Protocol/Combat/Combat.Protocol Release builds, and the full Combat.Protocol
  smoke pass with zero warnings/errors under
  `Build/diagnostics/projectile-b248-network-scheduling-20260831-01/` and the fresh serial
  rerun under `Build/diagnostics/projectile-b248-network-scheduling-20260831-03/`. The -03
  builds and focused gates exit `0` with zero warnings/errors; full Combat remains an isolated
  baseline failure at `VerifyBoundedVitalRegeneration` (`Program.cs:654`, before Projectile
  checks, exit `-532462766`).
  Full 255-slot
  `netSyncSkippedForPlayer` cadence, complete V1456 packet/reconnect scheduling, broader NPC
  projectile extension policy, complete Projectile parity, and the unapproved atomic
  `TileObject.CanPlace -> multi-tile placement -> Sign.TextSign -> object placement replication
  -> projectile tombstone` command/commit boundary remain `deferred/blocked`.

## Verification policy

- Build concurrency, command shape, preflight, and overlap recovery are defined by the single
  normative [`AGENTS.md#dotnet-build-concurrency-contract`](AGENTS.md#dotnet-build-concurrency-contract).
- Launch every compile-capable or shared-output `dotnet` command through
  `Build/Tools/Invoke-SerialDotnet.ps1`; it queues compliant processes on the checkout-specific
  named Mutex described by that contract.
- Run compile-capable commands one affected project at a time; use the required serial flags and
  `--no-build` for verifier runs after the build.
- Keep builds, generated files, packages, and diagnostics under `Build/`.
- Add a new claim only with a fresh `Build/diagnostics/` artifact and a source-backed boundary.
- Record `verified`, `partial`, `deferred`, or `blocked` explicitly; a focused green verifier is
  never a full lifecycle or parity claim.

## Next work

 1. Continue a source-backed NPC lifecycle read using the typed coordinate/player-range,
    timer-refresh, deactivation, and worm-chain owners; preserve deferred full CheckActive
    integration, recovery, collision, unspawn, and complete AI families.
  2. Continue WorldGen differential recovery one source-backed slice at a time where structure
     authority is available. The Pyramid invocation, pre-mutation, pillar/wall footprint, request,
     neighbor, and value/commit boundaries are now typed; keep tunnel/features, exact traversal/RNG
     parity, aggregate ordering, full WLD differential, legacy deletion,
     `canRemoveLegacyWorldGen`, and the 44 `ServerRelevant` rows deferred.
3. Re-audit deferred ServerRelevant rows only when a complete replacement chain is evidenced.
4. Run the final clean-process sweep only after WorldGen, deletion, and convergence gates close.

## Canonical entry points

- Context and authority: `docs/flowstate/README.md`
- Requirements/scope/risks: `docs/flowstate/requirements.md`, `scope.md`, `risks.md`
- Acceptance: `docs/flowstate/dod-checklist.md`
- Build concurrency: [`AGENTS.md#dotnet-build-concurrency-contract`](AGENTS.md#dotnet-build-concurrency-contract)
- Serial dotnet wrapper: `Build/Tools/Invoke-SerialDotnet.ps1`
- Current private checkpoint: `.agent-workplace/state/checkpoint.json`
- Historical progress archive: `docs/archive/progress-2026-08-22-full.md`
