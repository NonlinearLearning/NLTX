# World Time-Rate Authority Design

## Decision

`WorldTimeRateSnapshot` is the server-authoritative, versioned representation of the legacy
mutable `Main.dayRate` contract for the supported invasion-travel slice. It is separate from
`WorldClock.TicksPerUpdate`: the latter advances the reduced ECS clock, while the former carries
the source-derived policy value consumed by world-event systems.

The source oracle is `Terraria/Main.cs:3567-3595` (`UpdateTimeRate`) and
`Terraria/Main.cs:12958-13046` (`UpdateInvasion`). The precedence is exact for the admitted
inputs: fast-forward returns rate `60`; otherwise target rate is multiplied by `5` only when all
active players are sleeping, then freeze yields `0`, and game-menu fallback yields `1`. Invasion
travel consumes `max(rate, 1)` and clamps at `spawnTileX`.

## Authority And Data Flow

`WorldTimeRateInput` is a trusted server configuration input. It exposes fast-forward, freeze,
target rate, active/sleeping player counts, and menu state; it is not a client command or mutable
global. `WorldTimeRatePolicy` validates and resolves that input to an immutable
`WorldTimeRateSnapshot`.

During `DomeSimulation.Tick`, the policy runs in `ApplyWorldClock` after the reduced clock update
and before player/gameplay phases. The single admitted consumer is
`WorldInvasionTravelSystem.Advance`, supplied with the resolved snapshot rate. An unavailable
snapshot deliberately prevents this new authority route after restoring an old persistence
format; it is not converted to a contemporary default.

## Persistence And Compatibility

Persistence format version 27 appends `isAvailable` and `rate` after the prior invasion fields.
Older formats restore `WorldTimeRateSnapshot.Unavailable`; current-format values reject invalid
negative or contradictory `(unavailable, nonzero)` combinations. The rate is persisted to make
restart continuation explicit rather than recomputing a potentially changed policy input.

This decision does not import WLD fractional clock time, client warning/chat presentation, random
invasion start behavior, or NPC spawn tables. Those remain separate cards.

## Verification

The focused acceptance set is intentionally reduced: TickOrder proves all five rate cases and
same-tick travel ordering; Persistence proves snapshot restart continuity and invalid recovery;
WorldRules proves the standalone travel clamp rule; MainBoundary prevents legacy/client/server
dependencies from leaking into Simulation. No root Release build or broad regression matrix is
part of this card.
