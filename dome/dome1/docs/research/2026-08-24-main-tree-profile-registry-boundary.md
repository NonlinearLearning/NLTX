# Main Tree Profile Registry Boundary

`LegacyTreeProfileRegistry.RegisterDefaults()` now exposes the ten supported source-derived
tree profiles in deterministic registration order. The ordered projection is read-only and is
shared by the existing Tile ID and profile-kind lookup paths; unknown Tile IDs remain
fail-closed. Existing growth, ground, wall, and trunk consumers continue to use the same
profile values.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-tree-profile-registry/20260824-231500/summary.txt`
- `Build/diagnostics/main-tick/task-2-tree-profile-registry/20260824-231500/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-tree-profile-registry/20260824-231500/world-generation-verifier.log`

This is bounded tree-profile registration coverage, not complete Terraria tree content or
client presentation parity.
