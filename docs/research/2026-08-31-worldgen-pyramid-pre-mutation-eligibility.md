# WorldGen Pyramid pre-mutation eligibility boundary

## Source authority

The source-backed boundary is the beginning of `WorldGen.Pyramid` in
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:28348-28391`.
The current full source capture is SHA-256
`C7C2F2196CEA0F6E56C20863A27824917391B74B34232D50590D1DDF4FF321AC`; the
UTF-8 excerpt for those lines is SHA-256
`57ef5cb3a3dd6f7a5d252ead40a2c04f85ecc1d08a35c1fa5fbe504dd465d943`.

Before the first random draw or tile write, the legacy method performs these
checks in order:

1. It rejects an active tile at `(i, j)` when its type or wall is `151`.
2. When dual dungeons are enabled, it rejects
   `DungeonUtils.InAnyPotentialDungeonBounds(i, j + pyramidMaxDepth, 5)`.
3. In Surface Is Desert or Error World variants, it rejects an active nearby
   tile of type `151`, then `203`, then `25`, each with distance `100`.
4. In any Surface Is Desert, Error World, or dual-dungeon variant, it rejects
   an active nearby tile of type `41`, `43`, or `44`, also with distance `100`.

`IsTileNearby` scans inclusive square endpoints, skips out-of-world cells, and
only matches active tiles. The direct overlap test is also source-shaped: the
wall `151` check is conditional on the origin tile being active.

## Typed owner

`src/Terraria.Dome.Simulation/WorldGeneration/LegacyPyramidPreMutationEligibilityPolicy.cs`
owns this read-only boundary. The adjacent
`LegacyPyramidPreMutationDecision.cs` and
`LegacyPyramidPreMutationRejectionReason.cs` files keep the result contract
and reason vocabulary as separate types. `Evaluate` accepts an immutable
`WorldGridSnapshot`, the candidate origin, `pyramidMaxDepth`, the three secret
variant flags, and an optional list of published potential dungeon bounds. It
returns `LegacyPyramidPreMutationDecision` with a typed
`LegacyPyramidPreMutationRejectionReason`.

The dual-dungeon branch fails closed when the bounds authority is missing or
empty. The potential Y addition is evaluated in `long`; an unrepresentable
value also rejects rather than wrapping. Coordinates and a non-positive depth
are invalid inputs and throw before the snapshot is read. No random state,
structure command, tile command, wall/frame write, tunnel, chest, pot, plant,
or GenVars publication is performed.

## Verification

The scratch probe is
`.agent-workplace/scratch/underworld-policy-boundary/Program.cs`, using the
existing `UnderworldPolicyBoundaryProbe.csproj`:

- `Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-pre-mutation-20260831-01/pyramid-pre-mutation-red.log`
  is the TDD RED run and fails only because the typed owner is absent.
- `.../pyramid-pre-mutation-green.log` records the first GREEN run.
- `.../pyramid-pre-mutation-focused-final.log` records the expanded focused
  run, exit `0`, covering direct active tile/wall overlap, inactive-wall
  compatibility, each nearby veto and source variant gate, inclusive distance,
  dual-bounds rejection, missing-bounds fail-closed behavior, and non-dual
  bypass.
- `.../pyramid-pre-mutation-focused-final-rerun.log` repeats that expanded
  focused run after input-validation hardening, exit `0`.
- `.../simulation-build-final.log` records the initial Simulation Release
  build, exit `0`, with zero warnings and zero errors. The later stable rerun is
  `.../simulation-build-final-rerun-02.log`, also exit `0` with zero warnings
  and zero errors; an intermediate concurrent working-tree compile was not
  used as acceptance evidence.
- `.../tunnels-regression-final.log` records the existing Tunnels and adjacent
  WorldGen boundary regression, exit `0`; the stable rerun is
  `.../tunnels-regression-final-rerun.log`, also exit `0`.
- `.../worldgeneration-verifier-build-final.log` records the historical bounded
  WorldGeneration verifier attempt from before the SandPatches owner existed. Its compile gap
  is retained as historical evidence; the owner is now present and the current bounded verifier
  build/run is recorded under
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/sand-patches-boundary-20260831-01/`.
  This Pyramid batch does not change the SandPatches files.
- `.../flowstate-manifest-generation-final-03.log` and
  `.../flowstate-docs-verifier-final-03.log` record the latest fresh
  manifest/docs checks: `DOCS=459`, `BUILD_REFS=1270`, and verifier exit `0`.
  Earlier rerun pairs remain under the same batch directory.
- `.../git-diff-check-rerun.log` records a fresh repository diff check, exit `0`
  with the existing LF/CRLF conversion warnings only.

## Status and deferred scope

Status: `completed_partial` for the pre-mutation eligibility boundary only.

Still deferred are the Pyramid footprint and all mutation after these guards,
including random draw/checkpoint parity, the pillar/tunnel loops, walls and
frames, chest/pot/plant side effects, publication, aggregate ordering, full
WLD/state differential, legacy `WorldGen.cs` deletion,
`canRemoveLegacyWorldGen`, and the 44 deferred `ServerRelevant` rows.
