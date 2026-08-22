# Invasion Original Size State

## Source Boundary

The frozen legacy source writes `invasionSizeStart = invasionSize` during
`Main.StartInvasion` (`Main.cs:13047-13072`). `Main.UpdateInvasion` consumes and reduces the
remaining `invasionSize`, but does not rewrite the original-size field while travelling or
completing (`Main.cs:12958-13033`). Completion clears the active invasion state.

## Green Narrow Route

`WorldProgressionState.InvasionSizeStart` is now the immutable owner of the original invasion
size. `WorldProgressionSystem.AdvanceInvasion` initializes it from the accepted start command,
preserves it for progress commands, and resets it to zero when the invasion normalizes to an
inactive state. A zero value on an active state is retained as an explicit unknown sentinel for
pre-v23 snapshots that had no source-backed original-size field; non-zero values must be at least
the current remaining size.

The field is persisted as an append-only v23 tail field after the existing world-event random
state. Older formats restore zero/unknown and do not reinterpret bytes from their existing
layouts. Current snapshots round-trip the field through `DomeStatePersistenceFormat`.

## Verification Scope

The focused WorldRules verifier checks start, progress preservation, snapshot continuation and
completion clearing. The Persistence verifier covers current round-trip and legacy version
fixtures. Two clean WorldRules runs produce identical output hashes:

`Build/diagnostics/main-migration/task-9-invasion-size-start/20260821-225419/`

Simulation and Server Release builds pass with zero warnings and errors, and the scoped diff check
passes. MainBoundary, root Release, broad regressions and multiplayer/PVS evidence are outside the
current focused-only validation scope.

## Deferred Branches

`invasionDelay`, `invasionX`, `invasionWarn`, random start side for invasion types 1-3, NPC spawn
tables, named progression flags, achievements, client announcements and complete runtime
travel/warning integration remain separate cards. This card does not claim full invasion lifecycle
parity.
