# Main boulder registry boundary

The legacy `Terraria.ID.TileID.Sets.Boulders` source defines ten boulder Tile types:
`138, 484, 664, 665, 711, 712, 713, 714, 715, 716` (`TileID.cs:197`).

`BoulderTileRegistry.RegisterDefaults()` is now the single immutable owner. The legacy wiring
registry delegates to it, and WorldGeneration default paths consume it for Tile 324 framing,
pile/speleothem invalidity, and tile-pounding eligibility. Explicit-set overloads remain for
custom compatibility fixtures.

This covers the shared boulder predicate only; complete boulder spawning, physics, drops, and
historical WorldGen behavior remain deferred.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-boulder-registry/20260827-120000/summary.txt`
- `Build/diagnostics/main-tick/task-2-boulder-registry/20260827-120000/worldgen-verifier.log`
- `Build/diagnostics/main-tick/task-12-gate/20260827-130000/summary.txt`
