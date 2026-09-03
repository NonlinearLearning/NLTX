# Main tree-leaf checked-type registry boundary

The legacy `Terraria.ID.TileID.Sets.GetsCheckedForLeaves` source defines the shared set as
`5, 72, 323, 583, 584, 585, 586, 587, 588, 589, 596, 616, 634`
(`D:\TRbackup\Version4物理删除了某些文件\Terraria.ID\TileID.cs:169`). Tree-leaf scan and tree-top
classification both consume this predicate.

`TreeLeafCheckedTypeRegistry.RegisterDefaults()` now owns the immutable set. `TreeLeafScanQuery`
and `TreeLeafFrameQuery` expose default overloads while retaining explicit-set overloads for
compatibility fixtures and future projections.

This covers only the shared checked-type predicate. Tree growth, leaf destruction, frame mutation,
random placement, and complete historical WorldGen behavior remain deferred.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-tree-leaf-checked-registry/20260827-080000/summary.txt`
- `Build/diagnostics/main-tick/task-2-tree-leaf-checked-registry/20260827-080000/worldgen-verifier.log`
- `Build/diagnostics/main-tick/task-12-gate/20260827-090000/summary.txt`
