# M-009 invasion travel integration boundary

## Status update: narrow route accepted

The former day-rate blocker is resolved by the versioned `WorldTimeRateSnapshot` route. This
document now records the current boundary; it no longer describes the travel consumer as absent.

## Source finding

The source `Main.UpdateInvasion` advances `invasionX` every update by `max(dayRate, 1f)` toward
`spawnTileX`, then emits arrival/warning effects. The ECS owns the same bounded travel rule in
`WorldInvasionTravelSystem.Advance` and persists `WorldProgressionState.InvasionX`.

## Resolved authority

`WorldTimeRatePolicy` resolves a validated `WorldTimeRateSnapshot` from trusted server inputs using
the source precedence: fast-forward, target rate, all-active-players sleeping multiplier, freeze,
and menu fallback. `DomeSimulation` resolves this snapshot in `ApplyWorldClock` and passes its
rate to the single invasion-travel consumer. `WorldTimeRateSnapshot` is persisted append-only;
pre-rate snapshots restore `Unavailable` rather than inventing a contemporary rate.

The source audit also confirms that `Main.dayRate` is mutable runtime state: `UpdateTimeRate` changes
it for fast-forward, freeze-time, target time rate, sleeping-player acceleration and game-menu
fallback. `UpdateInvasion` consumes that value directly, so it cannot be treated as a fixed world
metadata field.

## Decision

Accept only the narrow rate/travel integration route. The full M-009 lifecycle remains `deferred`:
qualified-player start production, non-Martian random start side, NPC tables, complete warning and
chat projection, damage tracking, and all client/presentation effects remain separate cards.

Focused evidence (2026-08-22):

- `Test/Terraria.Dome.TickOrder.Verification`: exit 0; rate precedence and same-tick travel pass.
- `Test/Terraria.Dome.WorldRules.Verification`: exit 0; independent travel clamp and invasion
  lifecycle rules pass.
- `Test/Terraria.Dome.MainBoundary.Verification`: required as the reduced acceptance gate.
