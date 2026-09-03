# Main RoomNeeds registry boundary

The legacy `Terraria.ID.TileID.Sets.RoomNeeds` source defines the static room requirement sets:
6 chair types, 12 table types, 13 door types, and 26 torch types (`TileID.cs:102-129`).

`RoomNeedsTileRegistry` now owns those immutable sets. `RoomNeedsQuery.Evaluate(houseTileTypes)`
provides a default path using the registry, while the existing five-set overload remains available
for custom compatibility fixtures and projections.

This covers room-needs classification only. Housing scans, room scoring, NPC assignment, and full
historical housing behavior remain deferred.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-room-needs-registry/20260827-140000/summary.txt`
- `Build/diagnostics/main-tick/task-2-room-needs-registry/20260827-140000/worldgen-verifier.log`
- `Build/diagnostics/main-tick/task-12-gate/20260827-150000/summary.txt`
