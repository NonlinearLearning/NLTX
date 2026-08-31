# Main Framing Registry Boundary

`TileFrameImportantRegistry` owns the supported source-derived `FrameImportant` tile IDs used
by `TileFrameClassificationQuery`. `RegisterDefaults()` exposes a stable read-only ordered
projection, while `Contains` performs the same sorted binary-search classification as the
previous local table. The framing query now consumes this domain registry instead of carrying
its own mutable array.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-framing-registry/20260824-230500/summary.txt`
- `Build/diagnostics/main-tick/task-2-framing-registry/20260824-230500/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-framing-registry/20260824-230500/world-generation-verifier.log`

This is bounded framing classification coverage. Complete Terraria framing tables, mutable
legacy Tile ownership, map-update side effects, and client presentation remain deferred.
