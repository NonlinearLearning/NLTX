# Invasion Delay Day-Start Policy

## Source Boundary

The frozen legacy source decrements `invasionDelay` in `Main.UpdateTime_StartDay` only when the
value is positive (`Main.cs:13899-13901`). `Main.UpdateInvasion` clears the value on invasion
completion (`Main.cs:12989-12991`). No source assignment that initializes a nonzero delay is
available in the reviewed server path.

## Green Narrow Route

`WorldInvasionDelaySystem.AdvanceAtDayStart` captures the known pure policy: a positive delay is
decremented once, zero stays zero, and negative input is rejected as invalid state. It does not
pretend to own delay initialization, persistence or the exact runtime dawn phase integration.

## Verification Scope

Verification: WorldRules/NPC focused runs and two clean replays pass; Simulation/Server Release
builds are warning-free and the scoped diff check passes.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-delay-policy/20260821-232034/`

MainBoundary, root Release, broad regressions and runtime invasion-delay integration remain outside
the focused-only validation scope.

## Deferred Branches

Delay initialization, progression state ownership, persistence, exact dawn phase wiring, invasion
position/warning integration, NPC tables, random branches and client behavior remain separate
cards.
