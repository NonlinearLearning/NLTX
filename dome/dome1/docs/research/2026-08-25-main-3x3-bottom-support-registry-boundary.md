# Main 3x3 Bottom-Support Registry Boundary

`Tile3x3ValidationQuery` now owns a frozen registration projection for the 27 3x3 Tile types
that use bottom support. The row-major frame validation and the alternate top-support path remain
unchanged.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-3x3-support-registry/20260825-170000/summary.txt`
- `Build/diagnostics/main-tick/task-2-3x3-support-registry/20260825-170000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-3x3-support-registry/20260825-170000/worldgen-build.log`
- `Build/diagnostics/main-tick/task-2-3x3-support-registry/20260825-170000/worldgen-verifier.log`

This is bounded 3x3 support registration coverage; complete historical WorldGeneration parity
remains deferred.
