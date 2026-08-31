# Main Platform Support Registry Boundary

`Tile1x2TopValidationQuery` now owns a frozen registration projection for the seven one-by-two
Tile types that require platform support. Type 698 still additionally requires rope support, while
special platform type 380 remains an explicit support definition rather than a target-type default.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-platform-support-registry/20260825-110000/summary.txt`
- `Build/diagnostics/main-tick/task-2-platform-support-registry/20260825-110000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-platform-support-registry/20260825-110000/worldgen-build.log`
- `Build/diagnostics/main-tick/task-2-platform-support-registry/20260825-110000/worldgen-verifier.log`

This is bounded one-by-two platform-support registration coverage; complete historical
WorldGeneration parity remains deferred.
