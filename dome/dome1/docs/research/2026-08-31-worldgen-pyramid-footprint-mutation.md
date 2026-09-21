# WorldGen Pyramid footprint mutation boundary

## Source authority

The current instrumented legacy source is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs` with
SHA-256 `C7C2F2196CEA0F6E56C20863A27824917391B74B34232D50590D1DDF4FF321AC`.
The footprint excerpt at lines `28392-28427` has SHA-256
`F40DD96F351C2E86391329BBCD81F8CE9783C1BB419E8D47B67E569BDC3C1912C` when normalized
to the source text plus a terminal line feed.
The mutation seam begins at lines `28392-28427`, immediately after the
pre-mutation guards:

- `num2 = j - genRand.Next(0, 7)` chooses the top offset.
- `num3 = genRand.Next(9, 13)` chooses the source tunnel width used by the
  later tunnel branch.
- `num5 = j + genRand.Next(pyramidMinDepth, pyramidMaxDepth)` chooses the
  exclusive bottom row.
- For each row `k` in `[num2, num5)`, the inner range
  `l = i - num4; l < i + num4 - 1; l++` writes type `151`, activates the tile,
  clears half-brick state, and sets slope `0`, then increments `num4`.
- The following scan covers `m = i - num4 - 5 .. i + num4 + 5` and
  `n = j - 1 .. num5 + 1`. It writes wall `34` only when every tile in the
  inclusive 3x3 neighborhood around `(m,n)` is active type `151`.

The body after line `28429` changes direction and enters the tunnel/feature
logic. That logic includes tunnel carving, buried chest placement, piles,
plants, pots, framing, and the final extended tunnel; it is outside this
boundary.

## Typed owner

`src/Terraria.Dome.Simulation/WorldGeneration/LegacyPyramidFootprintMutation.cs`
owns the first mutation seam. It consumes the existing readonly
`LegacyPyramidStructureRequest`, an immutable `WorldGridSnapshot`, the existing
`LegacyPassRandomState`, generation state, and a typed tile-command list.

The owner preserves the three source random draws in source order, enters the
`Structure` stage, emits two commands per pillar tile (`UpdateTileType` followed
by `UpdateTileShape`), and projects those commands before evaluating the wall
neighborhood. Wall commands are emitted in the source `m`-then-`n` order with
wall type `34` and source attribution `worldgen.Pyramid.footprint`.

The result type records `TopY`, `BottomYExclusive`, the drawn tunnel width,
and the pillar/wall command counts. Validation is fail-closed before any random
draw, state transition, or command publication when the origin/envelope is
outside the immutable world or sequence capacity is unavailable. Pending
commands are accumulated privately and appended only after the entire slice has
validated and emitted successfully.

`TileMutationProjection` preserves fields that the legacy writes do not touch:
liquid, frames, wires, paint/coating, and inactive metadata survive the type and
shape updates; wall commands change only `WallType`.

## Verification

The focused verifier is
`Test/Terraria.Dome.PyramidStructure.Verification/Program.cs`. It checks:

- source-order random draws and derived top/bottom coordinates;
- odd-width pillar rows and the exact 3x3 wall predicate/order;
- source attribution, command kinds, structure-stage advancement, and sequence
  accounting;
- commit behavior and preservation of untouched tile fields;
- out-of-world envelopes and exhausted sequence space failing closed without
  consuming random samples, state, or commands.

Fresh evidence for this batch is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-footprint-mutation-20260831-02/`.

The earlier `...-01/` directory is retained as historical batch evidence; the `...-02/`
directory is the current-tree rerun after the conservative horizontal-envelope and wall-row
capacity corrections.

- `pyramid-footprint-verifier-build.log` records the Release verifier build,
  exit `0`, with zero warnings and zero errors.
- `pyramid-footprint-verifier-run.log` records the focused run, exit `0`:
  `PASS: Pyramid request and source-backed pillar/wall mutation preserve typed contracts`.
- `simulation-build.log` records the direct Simulation Release build, exit `0`,
  with zero warnings and zero errors.
- `main-inventory-verifier.log` records
  `members=696 migratedScope=695 identityExcluded=1 accepted-narrow=125` and
  `DEFERRED-GUARD-AUDIT: 0/0`.
- `flowstate-manifest-generation.log` and `flowstate-docs-verifier.log` record
  the generated documentation inventory and its verification.
- `style-check.log` records zero tabs and zero lines over 100 characters for
  the new/changed C# files; `git-diff-check.log` exits `0` with only the
  existing LF/CRLF conversion warnings.

The formal batch evidence above predates the current-tree bounded WorldGeneration
rerun. The current tree now also has a fresh rebuild and run under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-footprint-mutation-20260831-02/`:
both commands exit `0`, the verifier emits `280 PASS` and `40 CHECK` lines, and
there are no anchored `FAIL:` or `ERROR:` lines. This bounded verifier result is
contextual regression evidence, not complete Pyramid or WorldGen oracle parity.

## Status and deferred scope

Status: `completed_partial` for the Pyramid pillar and 3x3 wall-command
boundary only. The current-tree focused and bounded verifier reruns remain
green, while the overall WorldGen differential and deletion gate remain open.

Still deferred are wall-frame execution, the direction/tunnel loops, chest,
pile, plant, pot, and other feature side effects, `noTunnel` behavior beyond
the request handoff, exact global random/checkpoint parity, candidate and
`GenVars` publication, aggregate WorldGen ordering, full WLD differential,
legacy `WorldGen.cs` deletion, `canRemoveLegacyWorldGen`, and the 44 deferred
`ServerRelevant` rows.
