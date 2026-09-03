# Main Tile Category Registry Boundary

`TileTypeCategoryCountQuery` now owns the three bounded Corruption, Crimson, and Hallow tile-index
groups as a frozen read-only registration projection. The existing category formulas, evil-tile
correction, and `TotalGoodEvil` composition remain unchanged. Unknown enum values still fail closed
to zero; full tile catalogs remain outside this slice.

Evidence: `Build/diagnostics/main-tick/task-2-tile-category-registry/20260829-030000/` and
`Build/diagnostics/main-tick/task-2-tile-category-registry/20260829-050000/`.
The full WorldGeneration verifier and serial Simulation Release build exited `0`.
