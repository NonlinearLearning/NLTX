# WorldGen Pyramid structure request boundary

## Source authority

The legacy `WorldGen.Pyramid` signature and structure constants are in
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:28348-28391`:

- `Pyramid(int i, int j, int pyramidMinDepth = 75, int pyramidMaxDepth = 125,
  bool noTunnel = false)` is the invocation boundary.
- The structure tile is type `151` and the structure wall is type `34`.
- Dual-dungeon callers can supply `pyramidMaxDepth = 100` after the upstream
  potential-bounds adjustment and set `noTunnel = true`.

The current full instrumented source capture is SHA-256
`C7C2F2196CEA0F6E56C20863A27824917391B74B34232D50590D1DDF4FF321AC`.
The body after these declarations performs the pillar, wall, tunnel, frame,
chest, pot, plant, and other mutations; those operations are not represented by
this boundary.

Version3 `D:\TRbackup\Version3删除多余同时人工审查代码\Terraria.WorldBuilding\SimpleStructure.cs`
is a neighboring structure reference: it confirms that structure execution is a
separate placement concern (pattern/action application and protected regions),
not a reason to put mutable structure state back into `Main`.

## Typed owner

`src/Terraria.Dome.Simulation/WorldGeneration/LegacyPyramidStructureRequest.cs`
owns a readonly `LegacyPyramidStructureRequest`. It preserves the two origin
coordinates, minimum/maximum depth arguments, and `NoTunnel` mode, and exposes
the source tile/wall identities. `TryCreateDefault` freezes the source defaults
(`75`, `125`), while `TryCreate` rejects negative coordinates, non-positive
minimum depth, and a maximum depth below the minimum before returning a request.
The contract has no `WorldGrid`, random-state, command, or publication dependency;
it is intentionally a handoff object for a future structure owner.

## Verification

The focused verifier is
`Test/Terraria.Dome.PyramidStructure.Verification/Program.cs`:

- `Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-structure-contract-20260831-01/pyramid-structure-verifier-build.log`
  records the Release verifier build (exit `0`, zero warnings/errors).
- `.../pyramid-structure-verifier-run.log` records the source-argument,
  dual-dungeon, value-semantics, and fail-closed validation assertions (exit
  `0`).
- `.../simulation-build.log` records the Simulation Release build (exit `0`,
  zero warnings/errors).

The final current-tree rerun is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-structure-contract-20260831-02/`;
its focused verifier, Simulation build, Main inventory (`696/695/1/125`),
state JSON, style, Flowstate docs, manifest, and diff logs all exit `0`.

The TDD RED run in this turn failed only because
`LegacyPyramidStructureRequest` did not yet exist; the subsequent GREEN run
passed after the minimal request type was added.

## Status and deferred scope

Status: `completed_partial` for the typed invocation contract only.

Still deferred are Pyramid footprint and tile/wall/frame mutation, tunnel and
feature side effects, `SimpleStructure` action execution parity, random and
checkpoint parity, aggregate ordering, full WLD differential, legacy deletion,
`canRemoveLegacyWorldGen`, and the 44 deferred `ServerRelevant` rows.
