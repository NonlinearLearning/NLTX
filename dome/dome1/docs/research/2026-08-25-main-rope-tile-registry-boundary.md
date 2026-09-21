# Main Rope Tile Registry Boundary

`TileRopeQuery` now owns a frozen registration projection for the nine source-derived rope Tile
types. Rope endpoint detection, one-by-two top validation, and frame classification all consume
the same registry owner; platform bridge and frame-dependent support rules remain explicit.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-rope-registry/20260825-030000/summary.txt`
- `Build/diagnostics/main-tick/task-2-rope-registry/20260825-030000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-rope-registry/20260825-030000/worldgen-build.log`
- `Build/diagnostics/main-tick/task-2-rope-registry/20260825-030000/worldgen-verifier.log`

This is bounded rope classification registration coverage; complete historical WorldGeneration
static parity remains deferred.
