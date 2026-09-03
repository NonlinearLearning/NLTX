# Main Generation Clearability Registry Boundary

`LegacyGenerationClearabilityPolicy` now owns its 17 non-clearable Tile IDs in a frozen set and
exposes them through `RegisterDefaults()`. `CanClear` retains the existing negative-ID,
protected-dungeon, and excluded-Tile guards. The frozen backing set prevents mutation through
the static policy path.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-clearability-registry/20260824-250000/summary.txt`
- `Build/diagnostics/main-tick/task-2-clearability-registry/20260824-250000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-clearability-registry/20260824-250000/world-generation-verifier.log`

This covers bounded clearability exclusions only; complete TileID sets and historical WorldGen
mutation parity remain deferred.
