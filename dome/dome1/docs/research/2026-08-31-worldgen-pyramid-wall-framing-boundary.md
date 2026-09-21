# WorldGen Pyramid wall-framing request boundary

## Source authority

The current instrumented legacy source is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs` with SHA-256
`C7C2F2196CEA0F6E56C20863A27824917391B74B34232D50590D1DDF4FF321AC`.

The Pyramid call site is `WorldGen.cs:28425`. The source writes wall type `34` and calls
`SquareWallFrame(m, n)` for each qualifying wall center. The `SquareWallFrame` implementation is
at `WorldGen.cs:81820-81834`; its nine `Framing.WallFrame` calls are emitted in this order:

```text
(x-1,y-1), (x-1,y), (x-1,y+1),
(x,y-1),   (x,y),   (x,y+1),
(x+1,y-1), (x+1,y), (x+1,y+1)
```

The normalized UTF-8 SHA-256 of the inclusive `WorldGen.cs:81820-81834` excerpt, with a
terminal line feed, is
`177844816F11962C82459C96BD1673539FD65921E928C628BD37CBCE71ACA6A6`.

## Typed owner

The request boundary is split into:

- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyPyramidWallFrameRequest.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyPyramidWallFrameRequestQuery.cs`

`LegacyPyramidWallFrameRequest` is a readonly record struct carrying the center, target, reset
flag, source name, and source line. `LegacyPyramidWallFrameRequestQuery.TryCreateRequests` accepts
an immutable `WorldGridSnapshot` and an ordered `IReadOnlyList<WallFrameCoordinate>` of Pyramid
wall centers. It validates all centers and their complete 3x3 neighborhoods before allocating the
result, then expands each center in x-major order with:

- `ResetFrame = true`;
- `Source = "worldgen.Pyramid.wall-frame"`;
- `SourceLine = 28425`.

The query preserves duplicate targets and caller order. It does not read tile contents, consume a
`LegacyPassRandomState`, advance `WorldGenerationStateComponent`, mutate a snapshot, or enter the
generic `SquareWallFrameRequestQuery` or tile-frame command pipeline. An invalid center,
edge-crossing neighborhood, or request-capacity overflow returns `false`, an empty list, and a
failure reason; no partial output is returned.

## Verification

Fresh evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-wall-framing-boundary-20260831-01/`:

- `pyramid-verifier-rebuild.log` and `pyramid-verifier-rebuild.exit.txt` record the Release
  Pyramid verifier rebuild, exit `0`, with zero warnings and zero errors.
- `pyramid-wall-framing-focused.log` and
  `pyramid-wall-framing-focused.exit.txt` record the focused gate, exit `0`: two centers expand
  to 18 requests in the exact x-major order, with metadata, duplicate preservation, snapshot
  preservation, zero random samples, unchanged generation state, atomic edge failure, and an
  empty-list no-op.
- `simulation-rebuild.log` and `simulation-rebuild.exit.txt` record the serial Simulation Release
  rebuild, exit `0`, with zero warnings and zero errors.
- `pyramid-verifier-full.log` and `pyramid-verifier-full.exit.txt` record the existing Pyramid
  footprint/request regression, exit `0`.
- `worldgeneration-verifier-rebuild.log` and
  `worldgeneration-verifier-rebuild.exit.txt` record the bounded WorldGeneration verifier rebuild,
  exit `0`, with zero warnings and zero errors.
- `worldgeneration-bounded-verification.log`, its exit file, and
  `worldgeneration-bounded-verification.summary.txt` record the bounded regression: `280 PASS`,
  `40 CHECK`, `0` anchored `FAIL`/`ERROR`, exit `0`.
- `main-inventory-verifier-rebuild.log` and `main-inventory-verifier.exit.txt` record the
  associated Main inventory build and run, both exit `0`; the current working tree reports
  `members=696`, `migratedScope=695`, `identityExcluded=1`, and `accepted-narrow=126` with a
  `0/0` deferred-guard audit.
- `wall-framing-style-check.log` and `wall-framing-style-check.exit.txt` record the scoped Google
  C# style check for the two owners and Pyramid verifier: zero tabs and zero lines over 100
  characters.

## Status and deferred scope

Status: `completed_partial` for the immutable Pyramid wall-framing request expansion only.

Still deferred are `Framing.WallFrame` neighbor classification and value calculation,
truncating-wall and lookup-table semantics, `wallFrameNumber` and reset-frame random consumption,
`TileFrameCommand` generation and frame-field mutation, Pyramid tunnel direction and feature
side effects, exact global RNG/checkpoint parity, publication and aggregate ordering, full WLD and
extended-state differential parity, legacy `WorldGen.cs` deletion,
`canRemoveLegacyWorldGen = false`, and all 44 deferred `ServerRelevant` rows. The focused and
bounded green verifiers do not authorize a full Pyramid or WorldGen parity claim.
