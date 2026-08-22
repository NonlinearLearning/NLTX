# Liquid Contact Destruction Design

## Approved scope

This slice adds ordinary-runtime Liquid contact destruction from the Version4 global
`Main.tileWaterDeath` and `Main.tileLavaDeath` tables. The tables are source-derived base data for
Tile IDs `0..752`, alongside the existing solidity/platform definitions.

When a queued Liquid source is processed, an active source Tile whose base definition says that the
source Liquid destroys it produces a `TileChangeCommand` with `TileChangeKind.Kill`. The command is
committed by the existing deterministic Tile commit boundary. Water, honey, and shimmer use the
water-death table, matching the legacy `AddWater` branch; lava uses the lava-death table.

The contact command sets `PreserveLiquid`. Its Tile commit clears the active Tile state while keeping
the current liquid amount/type, matching the relevant `WorldGen.KillTile` property without changing
the default behavior of unrelated Kill commands from player interaction, Wiring, meteor, or WorldGen.

## Explicit exclusions

This is not full legacy `TileObjectData` parity. Frame/style/subtile/alternate overrides, custom
object multi-tile destruction, drops, sounds, `NetMessage`, `WorldGen.KillTile`,
`WorldGen.isGeneratingOrLoadingWorld`, `QuickWater`, and other environment side effects remain
outside this slice and continue to be recorded as partial or excluded behavior.

## Data flow

```text
legacy Main.tileWaterDeath/tileLavaDeath
  -> immutable TileDefinitionRegistry
  -> LiquidPropagationSystem contact predicate
  -> TileChangeCommand(Kill, PreserveLiquid)
  -> DomeSimulation CommitCommands -> TileChangeCommitSystem
```

The propagation system does not mutate `WorldGrid` or send protocol messages. Unknown Tile IDs do
not match a death rule and are left unchanged; the existing unknown active-target rejection remains
in force.

## Verification boundary

Focused verification covers source-derived table facts, water-family and lava contact, inactive
and non-death Tiles, command-only propagation, ordinary commit behavior, and existing Liquid
propagation/merge/replication/retry behavior. It does not claim style-aware or full Terraria parity.
