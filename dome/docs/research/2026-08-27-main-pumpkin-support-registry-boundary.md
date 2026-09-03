# Main Pumpkin Support Registry Boundary

`Tile2x2StyleValidationQuery` now owns a frozen registration projection for the four support Tile
types accepted by the pumpkin-specific 2x2 path. General bottom-slope validation and all other
style footprint behavior remain unchanged.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-pumpkin-support-registry/20260827-040000/summary.txt`
- `Build/diagnostics/main-tick/task-2-pumpkin-support-registry/20260827-040000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-pumpkin-support-registry/20260827-040000/worldgen-build.log`
- `Build/diagnostics/main-tick/task-2-pumpkin-support-registry/20260827-040000/worldgen-verifier.log`

This is bounded pumpkin support registration coverage; complete historical WorldGeneration parity
remains deferred.
