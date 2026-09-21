# Main Tile Pounding Registry Boundary

`TilePoundingEligibilityQuery` now owns frozen registrations for its nine always-blocked Tile
types and two additional generation/loading-only blocked types. Boulder input, above-tile slope
protection, and the final `CanKillTile` authority callback remain unchanged.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-pounding-registry/20260825-130000/summary.txt`
- `Build/diagnostics/main-tick/task-2-pounding-registry/20260825-130000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-pounding-registry/20260825-130000/worldgen-build.log`
- `Build/diagnostics/main-tick/task-2-pounding-registry/20260825-130000/worldgen-verifier.log`

This is bounded Tile pounding registration coverage; complete historical WorldGeneration parity
remains deferred.
