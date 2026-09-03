# Main conversion-sand registry boundary

The legacy `Terraria.ID.TileID.Sets.Conversion.Sand` source defines the shared conversion-sand
set as Tile types `53`, `112`, `116`, and `234` (`D:\TRbackup\Version4物理删除了某些文件\Terraria.ID\TileID.cs:30`).
The legacy `WorldGen.PlaceOasisPlant`, `CanUnderwaterPlantGrowHere`, and Tile 529 framing paths
all consume that set through the same conversion predicate.

`ConversionSandTileRegistry.RegisterDefaults()` now owns this bounded immutable set. Oasis plant,
underwater plant, and Tile 529 frame queries expose default overloads that consume the registry;
their existing explicit-set overloads remain available for caller-specific fixtures and future
compatibility projections.

This is a shared conversion predicate boundary, not complete plant framing, random placement,
liquid behavior, or historical WorldGen parity. Those larger responsibilities remain deferred.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-conversion-sand-registry/20260827-060000/summary.txt`
- `Build/diagnostics/main-tick/task-2-conversion-sand-registry/20260827-060000/worldgen-verifier.log`
- `Build/diagnostics/main-tick/task-12-gate/20260827-070000/summary.txt`
