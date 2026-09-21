# Main Non-Sloping Tile Registry Boundary

`TileSlopingQuery` now owns a frozen registration projection for the thirteen Tile types that
forbid slope changes. The tile-pounding eligibility path continues to consume the same predicate;
other pounding guards and support conditions remain explicit.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-nonsloping-registry/20260825-100000/summary.txt`
- `Build/diagnostics/main-tick/task-2-nonsloping-registry/20260825-100000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-nonsloping-registry/20260825-100000/worldgen-build.log`
- `Build/diagnostics/main-tick/task-2-nonsloping-registry/20260825-100000/worldgen-verifier.log`

This is bounded non-sloping Tile registration coverage; complete historical WorldGeneration
parity remains deferred.
