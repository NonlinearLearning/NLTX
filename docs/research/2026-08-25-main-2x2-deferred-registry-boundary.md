# Main 2x2 Deferred Special-Case Registry Boundary

`Tile2x2ValidationQuery` now owns a frozen registration projection for the four Tile types whose
special 2x2 behavior remains explicitly deferred. The support direction and footprint validation
are unchanged; the registry only makes the deferred marker bounded and stable.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-2x2-deferred-registry/20260825-220000/summary.txt`
- `Build/diagnostics/main-tick/task-2-2x2-deferred-registry/20260825-220000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-2x2-deferred-registry/20260825-220000/worldgen-build.log`
- `Build/diagnostics/main-tick/task-2-2x2-deferred-registry/20260825-220000/worldgen-verifier.log`

Complete historical 2x2 special-case behavior remains deferred.
