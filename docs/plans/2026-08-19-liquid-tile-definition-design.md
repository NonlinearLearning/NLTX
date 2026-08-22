# Liquid Tile Definition Design

## Scope

This design covers the ordinary Liquid propagation boundary that is currently marked `partial` in
the Wiring/Liquid/Chest migration coverage table. It introduces a source-derived base definition
for all Version4 Tile IDs `0..752` and uses it to distinguish blocking solid blocks from liquid-
passable tiles and solid-top platforms.

The design does not claim to migrate `QuickWater`, `StartPanic`, world-generation solidity toggles,
`TileObjectData` death/container effects, or underground-desert behavior. Those remain a separate
dynamic policy and legacy environment behavior family.

## Source facts

- `Terraria.ID.TileID.Count` is `753` in the Version4 source.
- Legacy `Liquid.AddWater` rejects an active target when `tileSolid[type]` is true and
  `tileSolidTop[type]` is false, except for the explicit type `546` path.
- Legacy world generation temporarily changes solidity, including types `379` and `546`; those
  changes must not be encoded as immutable ordinary-propagation definitions.
- `WorldTile.IsActive` is a world-state bit, not a substitute for the legacy type tables.

## Decision

Add an immutable `TileDefinitionRegistry` under `Terraria.Dome.Simulation.WorldModel.Definitions`.
Every Tile ID in `0..752` has a definition. A definition contains the Tile ID, base
`BlocksLiquid` state, and `IsPlatform` (`tileSolidTop`) state. The registry exposes a lookup that
returns false for out-of-range IDs and never falls back to a guessed default.

Liquid propagation will use the following ordinary-world predicate for an active target:

```text
target is liquid-passable when !BlocksLiquid || IsPlatform
```

Inactive targets remain liquid-passable. Unknown or out-of-range active Tile IDs are rejected by
the propagation target search and reported through the existing bounded settle/retry path rather
than silently treated as air.

The registry is static data. A future `LiquidCollisionPolicy` may layer temporary WorldGen or
QuickWater overrides over it, but that policy is outside this slice and is not invoked by the
ordinary simulation tick.

## Architecture and data flow

```text
Version4 Main tileSolid/tileSolidTop facts
  -> checked-in source-derived registry data
  -> TileDefinitionRegistry
  -> LiquidPropagationSystem target predicate
  -> LiquidChangeCommand
  -> LiquidCommitSystem / WorldGrid
```

The registry is read-only simulation data. It does not reference `Main`, `Tile`, `WorldGen`,
`NetMessage`, or client/UI types. No protocol field changes are required because the registry
affects server-owned simulation decisions, not the shape of liquid snapshots.

## Failure handling

- Duplicate or missing registry IDs fail registry construction rather than producing an incomplete
  table.
- Out-of-range lookup returns `false` and does not throw from propagation's neighbor search.
- Active unknown tiles block ordinary propagation and are retried through the existing bounded
  settle system; they are diagnosable and never silently pass liquid.
- Dynamic WorldGen behavior remains explicitly unimplemented instead of being approximated by
  mutating the base registry.

## Verification boundary

The Liquid verifier will assert:

1. The registry contains exactly 753 definitions with unique IDs.
2. A known ordinary solid definition blocks active-target propagation.
3. A known solid-top/platform definition permits active-target propagation.
4. Inactive targets remain passable.
5. Unknown/out-of-range active targets do not receive liquid.
6. Existing deterministic propagation, merge, commit, replication, and loopback fixtures remain
   green.

The coverage table will continue to mark full legacy Tile/environment side effects as `partial`.
