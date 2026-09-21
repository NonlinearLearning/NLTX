# Main common-sapling registry boundary

The legacy `Terraria.ID.TileID.Sets.CommonSapling` source defines four shared sapling types:
`20, 590, 595, 615` (`TileID.cs:175`).

`CommonSaplingTileRegistry.RegisterDefaults()` now owns the immutable set. Tree canopy clearance
has a default overload using the registry, and ordinary-tree placement plus underground-tree
eligibility and trunk command emission no longer carry separate hard-coded sapling sets. Explicit
canopy-set input remains available for compatibility fixtures.

The WorldGeneration verifier also asserts every registered legacy tree profile references a sapling
from the shared common-sapling registry, preventing the profile table and predicate from drifting.

This covers the shared sapling predicate only. Complete tree growth, placement randomness,
destruction, and historical WorldGen parity remain deferred.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-common-sapling-registry/20260827-220000/summary.txt`
- `Build/diagnostics/main-tick/task-2-common-sapling-registry/20260827-220000/worldgen-verifier.log`
- `Build/diagnostics/main-tick/task-12-gate/20260827-230000/summary.txt`
- `Build/diagnostics/main-tick/task-2-common-sapling-registry/20260827-240000/summary.txt`
- `Build/diagnostics/main-tick/task-12-gate/20260827-250000/summary.txt`
- `Build/diagnostics/main-tick/task-2-common-sapling-registry/20260828-000000/summary.txt`
- `Build/diagnostics/main-tick/task-12-gate/20260828-010000/summary.txt`
