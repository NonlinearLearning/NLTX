# Liquid Runtime Panic Design

## Approved scope

This slice implements the approved ordinary-runtime Panic policy for the current bounded
Liquid queue. It is not a `QuickWater` port and it does not model WorldGeneration Liquid
settling.

The policy is explicit immutable configuration attached to `LiquidWorldStateComponent`:

- high-water queue length;
- required number of consecutive high-water observations;
- fixed Panic drain budget; and
- lower recovery queue length.

While normal, a queue at or above the high-water length increments the sustained-pressure
counter. A lower observation resets it. Reaching the configured sustained count transitions the
state to `Panic`. During Panic, `LiquidPropagationSystem` drains at the configured Panic budget,
using the existing stable `Sequence -> X -> Y` queue order. The state returns to `Normal` only
when the observed queue length is at or below the recovery length.

## Defaults and authority

The default policy uses the existing queue capacity as its high-water threshold, `3601` sustained
observations as the source-derived runtime duration (`panicCounter > 3600`), five times the normal
tick budget as its bounded Panic drain budget, and half the high-water threshold as recovery.
Tests use small explicit policy values rather than depending on production defaults.

Only `LiquidWorldStateComponent` may transition between `Normal` and `Panic`. The propagation
system observes state at the start of an advance and asks it for the effective drain budget. Queue
ordering, source validation, bounded retries, Liquid commit, Tile command commit, merges, dirty
sections, and replication remain unchanged.

## Explicit exclusions

This does not call or recreate legacy `QuickWater`, `SettleWaterAt`, `UpdateLiquid`,
`WorldGen.WaterCheck`, Shimmer water cleanup, boulder changes, `tileSolid[379]`,
`tilesIgnoreWater`, `worldGenTilesIgnoreWater`, player-count capacity adjustment, or section/PVS
resets. The 753-ID Tile-definition registry remains immutable base data. Dynamic 379/546 and
generation-only policies remain partial behavior in the coverage ledger.

## Data flow

```text
LiquidUpdateQueueComponent.Count
  -> LiquidWorldStateComponent.ObserveQueueLength
  -> Normal or Panic effective drain budget
  -> LiquidUpdateQueueComponent.Drain (Sequence -> X -> Y)
  -> LiquidPropagationSystem
  -> existing Liquid/Tile command commits and replication
```

## Verification boundary

Focused Liquid verification proves: the high-water duration is consecutive; normal mode retains
the normal budget; Panic drains the configured bounded budget in the established queue order; and
recovery returns to normal without clearing or mutating queued sources. Existing propagation,
merge, contact-destruction, retry, commit, and loopback checks remain regression coverage.
