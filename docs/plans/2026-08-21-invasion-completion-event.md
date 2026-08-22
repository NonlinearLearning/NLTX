# Invasion Completion Event Slice

## Source Boundary

`Main.UpdateInvasion` returns while no invasion is active and, when an active invasion reaches
`invasionSize <= 0`, performs type-specific completion work before clearing `invasionType` and
`invasionDelay` (`Main.cs:12958-13033`). The retained source distinguishes the invasion type at
the transition; a generic zeroed state is not enough for later progression consumers.

## Accepted Narrow Route

When the authoritative progression system changes an active invasion (`InvasionType > 0` and
`InvasionSize > 0`) to the normalized zero state in one tick, `DomeSimulation` publishes exactly
one `WorldInvasionCompletedEvent` carrying the previous type. The event is exposed by
`DomeServer`, and the event list is cleared at the next tick boundary like the other server events.
Pending progress commands also reject duplicate `Sequence` values before they enter the private
queue, preventing one client fact from being applied twice in the same tick.
The completion event also carries the typed `WorldInvasionClearFlag` mapping so consumers do not
need to reinterpret the numeric invasion type.

## Deferred Behavior

This card does not invent or persist `downedGoblins`, `downedFrost`, `downedPirates` or
`downedMartians`. Travel position, delay/warning counters, qualified-player sizing, random start
side, NPC damage tracking, announcements, achievements and `FakeLoadInvasionStart` remain separate
cards.

## Verification Scope

The WorldRules verifier proves the existing start/progress/persistence path and requires one typed
completion event for a type-2 invasion. The affected Simulation and Server projects build in
Release; broad regressions, MainBoundary and the root solution build remain outside this focused
validation.
