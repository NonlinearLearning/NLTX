# WorldGen Pyramid noTunnel opening boundary

**Date:** 2026-08-31  
**Flowstate:** `N6` / `in_progress_with_deferred_findings`  
**Status:** `completed_partial`  
**Owner:** NLTX WorldGen migration / P9 Pyramid structure batch

## Source evidence

The instrumented Version4 source capture is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:28429-28466`.
The complete capture is `2,278,888` bytes with SHA-256
`C7C2F2196CEA0F6E56C20863A27824917391B74B34232D50590D1DDF4FF321AC`. The normalized UTF-8
hash for the inclusive 39-line opening block is
`DB65B1DF4BAE009B2C615ABD47EDF7BB301781C03C879A0E5EEF18239FDE32D9`.

The source initializes `num8` to `1` and changes it to `-1` when `Next(2)` returns zero. It
then derives `num9 = i - num3 * num8` and `num10 = j + num3`, draws `num11` from
`Next(5, 8)`, and consumes `num12` from `Next(20, 30)`. The opening loop scans each column's
`num10..num10+num11` rows. It remembers whether an active type-53 tile was observed above the
current row; active type-151 tiles write wall `34` below and on the direction-adjacent side,
then become inactive. Once sand has been observed above a row, every remaining row in that
column is rewritten as active type `53` with half-brick and slope cleared. The loop continues
only when the column contained an active type-151 tile and then steps one column opposite the
direction.

The `noTunnel` flag is not consulted by this opening loop. The source consumes it later, before
the final extended tunnel (`WorldGen.cs:28490-28595`); that later feature boundary is outside
this slice.

## Typed owner

`src/Terraria.Dome.Simulation/WorldGeneration/LegacyPyramidTunnelOpening.cs` accepts the existing
readonly `LegacyPyramidStructureRequest`, the footprint-drawn tunnel width, a committed immutable
`WorldGridSnapshot`, an explicit `LegacyPassRandomState`, generation state, and a tile-command
list. It validates the world envelope, random sample capacity, stage, and sequence capacity before
drawing. It uses a private projected tile map so a repeated source write observes earlier pending
commands, and appends commands only after the complete opening pass succeeds.

`LegacyPyramidTunnelOpeningResult` records direction, start coordinates, opening height, initial
delay, visited columns, type-151 clear count, wall-write count, type-53 conversion count, and the
`NoTunnelMode` handoff marker. The owner emits only `SetWall`, `UpdateTileType`, and
`UpdateTileShape` commands, all attributed to `worldgen.Pyramid.tunnel-opening`.

The accepted narrow boundary covers:

- source-order direction, opening-height, and delay draws;
- direction-derived start coordinates and bounded column traversal;
- projected type-151 clearing and wall-34 writes;
- source-shaped type-53 conversion and shape reset;
- sequential source attribution, structure-stage advancement, and atomic command publication;
- immutable snapshot behavior, untouched tile-field preservation, and fail-closed malformed edges.

## Continued buried-chest intent boundary

The source call at `WorldGen.cs:28555` is represented by the immutable
`LegacyPyramidBuriedChestIntent` and `LegacyPyramidBuriedChestIntentPolicy`. The owner derives
`(min(tunnelStartX, tunnelEndX) + max(tunnelStartX, tunnelEndX)) / 2`, preserves `openingY`,
`notNearOtherChests=false`, `chestStyle=1`, `trySlope=false`, and `chestTileType=0`, and records
the source metadata `worldgen.Pyramid.buried-chest`.

It preserves the source random seam: `Next(3)`, a second `Next(3)` only when the first selection
is zero, and the tenth-anniversary zero-to-one remap. Selection values map to item types
`848`, `857`, and `934`. The owner validates the envelope before drawing, advances a working
state to `Structure`, and publishes the intent/sequence only on success. It does not scan tiles,
create a chest object, fill items, or execute `AddBuriedChest` placement.

## Verification evidence

Evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-tunnel-opening-20260831-01/`:

- `pyramid-tunnel-opening-verifier-build-red-rerun.log` and
  `pyramid-tunnel-opening-verifier-build-red-rerun.exit` record the TDD RED compile (exit `1`)
  with only the expected missing tunnel owner/result symbols.
- `pyramid-tunnel-opening-verifier-build-green-rerun.log` and its `.exit` record the corrected
  Release Pyramid verifier build (exit `0`, zero warnings and zero errors).
- `pyramid-tunnel-opening-focused.log` and its `.exit` record the focused GREEN run (exit `0`):
  two visited columns, 15 typed commands, three random samples, projected writes, commit
  preservation, and out-of-envelope rejection.
- `simulation-build.log`, `pyramid-verifier-build.log`, and `server-build.log` each record a
  serial Release build with exit `0`, zero warnings, and zero errors.
- `worldgeneration-verifier-build.log` and `worldgeneration-verifier-run.log` record the bounded
  regression rebuild/run (exit `0`, `280` `PASS`, `40` `CHECK`, and no anchored `FAIL`/`ERROR`).
- `style-check-rerun.log` and `git-diff-check.log` record scoped style and diff checks with exit
  `0`. The diff check retains only the repository's existing LF/CRLF conversion warnings.

### Buried-chest continuation evidence

The fresh continuation evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-buried-chest-intent-20260831-01/`.
The corrected TDD RED build exits `1` only for the two new owner symbols; the Pyramid verifier
build, buried-chest focused run, Pyramid full run, Simulation/Server builds, and bounded
WorldGeneration build/run exit `0`. The focused run covers forward and reversed midpoint bounds,
source item selection, anniversary retry accounting, `Structure`/sequence publication, and
fail-closed invalid input. Scoped style, diff, JSON/artifact, manifest, and Flowstate document
checks also exit `0`; the bounded run remains `280 PASS / 40 CHECK / 0 FAIL / 0 ERROR`.

## Current limits and gate state

This is a source-backed Pyramid opening and buried-chest-intent boundary, not complete Pyramid or
WorldGen parity. Actual `AddBuriedChest` tile/object placement, chest filling/entity creation,
small piles, plants, pots, the final extended tunnel, aggregate pipeline wiring, exact global
RNG/checkpoint parity, WLD/extended-state parity, client SceneMetrics/map/network behavior, and
persistence/protocol projection remain deferred.
`canRemoveLegacyWorldGen = false`, legacy `WorldGen.cs` deletion is not authorized, and all `44`
`ServerRelevant` rows remain deferred. Green focused or bounded verifiers do not promote this
slice beyond `completed_partial`.
