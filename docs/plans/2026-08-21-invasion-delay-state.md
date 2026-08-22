# Invasion Delay State Owner

## Source Boundary

The frozen legacy source decrements positive `invasionDelay` once in
`Main.UpdateTime_StartDay` (`Main.cs:13899-13901`) and clears it when invasion completion is
normalized in `Main.UpdateInvasion` (`Main.cs:12989-12991`). No reliable nonzero initialization
assignment was recovered from the reviewed server source path.

## Green Authority Route

`WorldProgressionState.InvasionDelayTicks` is now the Simulation owner. At the authoritative dawn
boundary (`WorldClockSnapshot.IsDayTime && TimeOfDay == 0`), `WorldProgressionSystem.Advance`
invokes the source-backed delay policy once. Midday and non-day-start ticks leave the value
unchanged. Invasion normalization clears the delay with the other active invasion state.

The field is appended as a v24 persistence tail after the existing v23 invasion-size-start field.
Older formats restore zero and do not reinterpret prior bytes. A v24 round-trip fixture verifies a
nonzero delay survives save/load.

## Verification Scope

WorldRules, Persistence and NPC focused verifiers pass. The WorldRules verifier covers dawn versus
midday behavior and invalid negative state. Two clean WorldRules replays produce identical output
hashes. Simulation and Server Release builds report zero warnings and errors, and scoped diff
check passes.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-delay-state/20260821-232729/`

MainBoundary, root Release, broad regressions and delay-driven NPC spawn integration remain outside
the focused-only validation scope.

## Deferred Branches

Nonzero delay initialization/import, client/world compatibility projection, delay-driven NPC spawn
integration, invasion position/warning integration, random branches and client behavior remain
separate cards.
