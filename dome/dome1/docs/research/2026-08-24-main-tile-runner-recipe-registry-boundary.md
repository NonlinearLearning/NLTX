# Main TileRunner Recipe Registry Boundary

`LegacyTileRunnerPassInputDefinition.CreateDefaultRecipes()` now returns a stable read-only
projection for the five supported TileRunner recipes. Existing recipe validation, source pass
ranges, tile types, random reset flags, and downstream command policies are unchanged.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-tile-runner-registry/20260824-235500/summary.txt`
- `Build/diagnostics/main-tick/task-2-tile-runner-registry/20260824-235500/verifier-build.log`
- `Build/diagnostics/main-tick/task-2-tile-runner-registry/20260824-235500/world-generation-verifier.log`

This is bounded TileRunner input registration coverage. Complete legacy TileRunner mutation,
random consumption, and historical cave parity remain deferred.
