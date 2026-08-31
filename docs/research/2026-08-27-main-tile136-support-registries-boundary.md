# Main Tile 136 support registries boundary

The legacy `Terraria.ID.TileID.Sets.IsBeam` source defines beam support types
`124, 561, 574, 575, 576, 577, 578` (`TileID.cs:241`). Its `IsATreeTrunk` set defines
tree bridge types `5, 72, 583, 584, 585, 586, 587, 588, 589, 596, 616, 634`
(`TileID.cs:163`). The legacy Tile 136 framing path consumes both predicates.

`BeamTileRegistry.RegisterDefaults()` and `TreeTrunkTileRegistry.RegisterDefaults()` now provide
immutable defaults, and `TileFrameImportant136Query` has a default overload using them. Explicit
set overloads remain available for compatibility fixtures.

This covers only Tile 136 support classification. Complete framing, tree bridge mutation, and
historical WorldGen parity remain deferred.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-tile136-support-registries/20260827-100000/summary.txt`
- `Build/diagnostics/main-tick/task-2-tile136-support-registries/20260827-100000/worldgen-verifier.log`
- `Build/diagnostics/main-tick/task-12-gate/20260827-110000/summary.txt`
