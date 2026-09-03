# Main Tree Canopy Registry Boundary

`TreeCanopyClearanceQuery` now stores its 23 source-derived plant exception Tile IDs in a frozen
set and exposes `RegisterPlantExceptionDefaults()`. Canopy evaluation retains the existing
snapshot rectangle, common-sapling, and ignored-tile behavior.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-tree-canopy-registry/20260824-255000/summary.txt`
- `Build/diagnostics/main-tick/task-2-tree-canopy-registry/20260824-255000/verifier-build.log`
- `Build/diagnostics/main-tick/task-2-tree-canopy-registry/20260824-255000/world-generation-verifier.log`

This is bounded canopy exception registration coverage; complete tree content and historical
WorldGen parity remain deferred.
