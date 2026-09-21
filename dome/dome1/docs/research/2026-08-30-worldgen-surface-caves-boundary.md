# WorldGen SurfaceCaves generic-owner boundary

**Date:** 2026-08-30  
**Flowstate:** `N6` / `in_progress_with_deferred_findings`  
**Status:** `completed_partial`  
**Batch:** `P9-worldgen/surface-caves-boundary-20260830-04` (latest follow-up)

## Source contract

The captured legacy source is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:12676-12785`.
Its SHA-256 is `C7C2F2196CEA0F6E56C20863A27824917391B74B34232D50590D1DDF4FF321AC`.
The recorded excerpt hash is
`893004829204FD43307A78AB6F768C338EA6CAC4F18619A4E70B80111DCFB185`; it was computed
after LF normalization and with a trailing LF over a 4,248-byte excerpt, not over the
mixed-newline source bytes.

The source guard is `!Skyblock.denyAllGeneration && !SecretSeed.noSurface.Enabled`. The
source order is:

1. narrow, medium, and deep vertical families (the deep family has two follow-up runners);
2. the horizontal `noYChange` family; and
3. the `Caverer` density loop.

The source pass contains no generic `surface-desert` recipe. That recipe is a compatibility
fallback in the ECS cave integration and must not be treated as a second SurfaceCaves owner.

## Runtime owner boundary

`LegacyCavePassSystem` remains the integration owner for the generic compatibility recipes.
When a `LegacyTerrainRuntimeProfile` is present, its `SurfaceCaves` schedule entry is skipped.
The profile-backed pipeline then retains the existing dedicated owners:

- `LegacySurfaceCavesVerticalPass` for the vertical and horizontal runner families;
- `LegacySurfaceCavesCavererPass` for the Caverer helper; and
- `LegacyMountainCavesPass` for the separately owned Mountain helper.

When no profile is supplied, the generic compatibility fallback remains available. This is a
narrow owner-exclusion correction; it does not replace or rewrite the dedicated helpers.

## Evidence

The focused verifier is `--surface-caves-only` in
`Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`. It seeds an `800x300` active
surface with seed `1456`, exercises both profile and no-profile requests, and checks source
attribution. The TDD RED run reached the stale generic owner and failed with:

`Profile-enabled SurfaceCaves emitted the stale generic surface-desert fallback.`

The GREEN run records:

- profile-enabled request: `2,907` commands and `0` generic `surface-desert` commands;
- dedicated vertical owner: source-attributed `worldgen.cave.SurfaceCaves.vertical-*` commands;
- no-profile request: `4,342` generic compatibility commands remain present.

Evidence files:

- `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-01/surface-caves-focused-red.log`
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-02/verifier-build-green-rerun.log`
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-02/surface-caves-focused-green-rerun.log`
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-02/surface-caves-source-contract-20260830.json`
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-03/simulation-build.log`
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-03/verifier-build-final-standard.log`
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-03/surface-caves-focused-final-rerun.log`
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-03/worldgeneration-verifier.log`
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-03/flowstate-docs-verifier-final.log`

## Deferred boundaries

This evidence does not establish a complete SurfaceCaves or WorldGen parity result. The
following remain deferred: one random stream across all dedicated SurfaceCaves families,
Remix and no-surface runtime-rule parity, complete Caverer/Mountain behavior, complete
TileRunner traversal, aggregate cave ordering, full WLD/extended-state differential,
`canRemoveLegacyWorldGen`, deletion of legacy `WorldGen.cs`, and the 44 deferred
`ServerRelevant` rows. The existing Main inventory counts, including `accepted-narrow=108`
for `Main.debuff` and `accepted-narrow=109` for `Main.vanityPet`, are unchanged.
## Follow-up v2: unified owner and pass differential

The latest owner is LegacySurfaceCavesPass in
src/Terraria.Dome.Simulation/WorldGeneration/LegacySurfaceCavesPass.cs. It consumes the
immutable Rock Layer Caves snapshot, one LegacyPassRandomState, and one shared projected-tile
map for the vertical and Caverer families. The pipeline creates one SurfaceCaves
LegacyTileRunnerPassCommandBatch after the RockLayerCaves boundary. MountainCaves remains a
separate downstream owner. LegacySurfaceCavesCavererPass now removes the nested
worldgen.cave.Caverer. prefix before applying the
worldgen.cave.SurfaceCaves.Caverer. attribution, so terminal commands are not reported as
SurfaceCaves.Caverer.Caverer.*.

The horizontal runner correction is source-backed: its noYChange recipe now uses fixed
speedX = 0.0, speedY = 1.0, and does not consume the horizontal speed-X random draw. The
focused verifier remains green after this correction:

- --surface-caves-only: exit 0;
- unified owner fixture: 95,132 tile commands, 21,933 liquid commands, 323,097 random
  samples;
- profile-enabled owner excludes the generic fallback (2,907 commands), while the
  no-profile compatibility fallback remains (4,342 commands).

The pass-specific differential probe is the private scratch program
.agent-workplace/scratch/surface-caves-boundary/Program.cs, built by
.agent-workplace/scratch/surface-caves-boundary/SurfaceCavesBoundaryProbe.csproj. It reads
the WGS1 snapshots from the latest oracle capture:

- input:
  Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-04/
  legacy-mount-caves-snapshot-Rock-Layer-Caves.bin (SHA-256
  F9E362D88D2744288049F22E00ABA067FC3B86B438D4271CBAABBB0E1B086D06);
- expected output:
  legacy-mount-caves-snapshot-Surface-Caves.bin (SHA-256
  60440FAF73F9F1C648ECB5998E05264BBB686140D4A3AB7C54CDFABF228C5D4B).

The probe confirms the input fingerprint
C50CF25A34E8750F049C5BE264BA03BA1EC7348CF38763AA81F5C809312E11FF and records the
source-attributed command batch, commit counts, output fingerprint, wall fingerprint, first
mismatches, and type-transition bands. The current result is intentionally diagnostic partial:

| Field | Value |
| --- | ---: |
| Tile commands / applied | 56,313 / 56,313 |
| Liquid commands / applied | 32,610 / 32,610 |
| Simulation random samples | 194,649 |
| Oracle random samples | 253,304 |
| Compared tiles | 5,040,000 |
| Active mismatches | 12,320 |
| Type mismatches | 7,152 |
| Liquid amount mismatches | 8,108 |
| Liquid type mismatches | 0 |
| Frame X / Frame Y mismatches | 8,306 / 8,306 |
| Wall mismatches | 0 |
| Total field mismatches | 44,192 |

The probe exits 2 with decision partial-surface-caves-traversal-mismatch. The input
fingerprint matches the oracle, while the generated output fingerprint is
CF2F6F24A752684E5F860A110F6B49428B05E8EE530A0BFB884629FF786B10FB, not the oracle output
fingerprint BABCE4821EB0CD7513CC232E7A931218869C1D5E7CFFF8606ABF9DD162BFC77E. This is
evidence of the known shared TileRunner/Caverer traversal and liquid-side-effect gaps, not a
build failure and not an aggregate parity result.

The source-attribution RED/GREEN check is retained in the same evidence directory:

- surface-caves-differential-red-caverer-source.log observes the duplicated
  Caverer.Caverer. hierarchy before the rewrite correction;
- surface-caves-probe-build-green-caverer-source.log and surface-caves-differential.log are
  the post-correction build/run artifacts.

The oracle capture itself remains pass-level only: legacy-random-trace.jsonl reports start
sample count 0 / peek 810676643 and end sample count 253304 / peek 1016925811; no
per-invocation SurfaceCaves hook was added.

## Follow-up v3: projected first-active scan

`LegacySurfaceCavesVerticalPass.FindSurfaceY` now reads the shared `projectedTiles` map before
falling back to the immutable snapshot. The targeted fixture leaves `(x,40)` and `(x,41)` active
in the snapshot, overlays only `(x,40)` as inactive in the projection, and requires the scan to
return `41`. It first failed because the private scan boundary accepted no projection, then passed
after the minimal projected-tile lookup was threaded through the existing shared owner map.

Evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-caves-boundary-20260830-05/`:

- `verifier-build-red-find-surface-y.log`: fixture compiled before production change;
- `surface-caves-focused-red-find-surface-y.log`: expected failure at the missing projected scan
  boundary;
- `verifier-build-green-find-surface-y.log`: Release build exit `0`, 0 warnings, 0 errors;
- `surface-caves-focused-green-find-surface-y.log`: focused verifier exit `0`;
- `worldgeneration-verifier-green-find-surface-y.log`: complete verifier exit `0`.

This repair is limited to a same-pass vertical-family start scan. It does not implement complete
TileRunner/Caverer traversal, liquid side effects, Remix/no-surface runtime projection,
per-invocation oracle hooks, aggregate ordering, WLD parity, legacy deletion, or the 44 deferred
`ServerRelevant` rows.

## Follow-up v4: MountainCaves owner boundary

The next independently owned cave pass was audited against the legacy `MountainCaves` source at
`WorldGen.cs:12119-12186`. Its source guard includes `surfaceIsDesert`, its base density is
`floor(width * 0.001)`, Remix multiplies that truncated count by `1.5`, and the +/-90 center
exclusion applies only outside Remix. The typed owner now accepts explicit world-rule flags,
preserves those guards without consuming random/state, and passes the existing metadata Remix
projection from `WorldGenerationPipeline`.

M1 RED and M2/M3 evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/mountain-caves-boundary-20260831-01/`:

- `verifier-build-red.log` compiled the fixture before the owner correction;
- `mountain-caves-focused-red.log` failed on the missing Remix-aware owner boundary;
- `verifier-build-green-rerun.log` exits `0` with zero warnings and zero errors;
- `mountain-caves-focused-green-rerun.log` exits `0`;
- `worldgeneration-verifier-green.log` exits `0`;
- `surface-caves-focused-regression.log` exits `0`.

This is a pass-contract correction, not Mountinater parity. Mountinater shape/random behavior,
full secret-seed/Skyblock runtime projection, aggregate cave ordering, WLD/extended-state parity,
legacy deletion, and the 44 deferred `ServerRelevant` rows remain deferred.

This follow-up does not alter the repository-wide release state:
in_progress_with_deferred_findings, canRemoveLegacyWorldGen=false, 44 deferred
ServerRelevant rows, aggregate WorldGen/WLD = partial_blocked, 3,190,404 tile mismatches,
and 1,046,843 extended-state mismatches remain unchanged.
