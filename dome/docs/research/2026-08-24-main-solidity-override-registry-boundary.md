# Main Tile Solidity Override Registry Boundary

`TileSolidityOverrideQuery` now owns the source-derived boulder and cracked-brick Tile sets as
stable read-only registration projections. The boulder solidity projection intentionally contains
the nine IDs assigned by legacy `WorldGen.SetBoulderSolidity` (`138, 484, 664, 711..716`);
it does not reuse the ten-item `TileID.Sets.Boulders` classifier because that source set also
contains `665`, which the solidity mutator omits. `Evaluate` preserves the existing projection
shape and solid flag, while the defaults cannot be mutated through the public list interface.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-solidity-override-registry/20260828-021000/world-generation-build.log`
- `Build/diagnostics/main-tick/task-2-solidity-override-registry/20260828-021000/world-generation-verifier.log`
- `Build/diagnostics/main-tick/task-12-gate/20260828-030000/root-solution-build.log`

This is bounded solidity override registration coverage, not complete TileID set or historical
WorldGen parity.
