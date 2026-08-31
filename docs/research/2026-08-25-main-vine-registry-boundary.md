# Main Vine Registry Boundary

`VineFrameQuery` now owns a frozen registration projection for the eight vine Tile types. Frame
classification reuses that owner, while replacement and support compatibility mappings remain
explicit per-vine rules.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-vine-registry/20260825-230000/summary.txt`
- `Build/diagnostics/main-tick/task-2-vine-registry/20260825-230000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-vine-registry/20260825-230000/worldgen-build.log`
- `Build/diagnostics/main-tick/task-2-vine-registry/20260825-230000/worldgen-verifier.log`

This is bounded vine classification registration coverage; complete historical WorldGeneration
parity remains deferred.
