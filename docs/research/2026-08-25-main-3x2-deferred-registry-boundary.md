# Main 3x2 Deferred Special-Case Registry Boundary

`Tile3x2ValidationQuery` now owns a frozen registration projection for the seven Tile types whose
special handling remains explicitly deferred. The projection only preserves the deferred marker;
it does not claim special-case generation, destruction, or historical parity.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-3x2-deferred-registry/20260825-190000/summary.txt`
- `Build/diagnostics/main-tick/task-2-3x2-deferred-registry/20260825-190000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-3x2-deferred-registry/20260825-190000/worldgen-build.log`
- `Build/diagnostics/main-tick/task-2-3x2-deferred-registry/20260825-190000/worldgen-verifier.log`

Complete historical 3x2 special-case behavior remains deferred.
