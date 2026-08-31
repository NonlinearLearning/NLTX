# Main Tile Definition Registry Boundary

Reference: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`.

This slice gives the server simulation a domain-owned Tile definition registry for the
supported Version4 tile facts. `TileDefinitionRegistry.RegisterDefaults()` is the explicit
startup contract and constructs exactly 753 contiguous tile IDs. `Create(...)` is the guarded
builder entry point for tests or future source-derived registration; duplicate, missing, and
out-of-order IDs are rejected before the registry becomes visible.

The registry exposes a read-only ordered projection and an ID lookup. Liquid, physics,
world-generation, wiring, and tile-state queries consume the same type, while protocol input
has no path to mutate it. Re-registering defaults produces equal ordered definitions and does
not share mutable collection state.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-tile-registry/20260824-220000/tile-verifier.log`
- `Build/diagnostics/main-tick/task-2-tile-registry/20260824-220000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-tile-registry/20260824-220000/world-generation-verifier.log`

This is a bounded registry slice, not proof of the complete Terraria tile static table,
framing table, or client presentation behavior. Those remain deferred until defaults,
consumers, and persistence evidence exist.
