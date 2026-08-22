# Tick Order World Phase Boundary

## Source Audit

Legacy `DoUpdateInWorld` runs entity update loops, then `UpdateTime`, then
`WorldGen.UpdateWorld/UpdateInvasion`, and finally `UpdateServer`
(`Main.cs:11819-12028`). This is a mixed client/server loop with presentation and error-swallowing
branches, so it is not copied wholesale.

## Current Authority

`SimulationTickSchedule` exposes named phases: world clock/progression first, then player input
and control, collision/AI/movement/projectiles, combat, domain command commit, snapshot publish
and end tick. The TickOrder verifier asserts exact phase traces, paused-tick short-circuit,
deterministic equal-sequence ordering and world invasion completion before projectile advancement.

## Decision

Accept the named phase/order invariants narrowly. This proves the ECS-owned order for the covered
domains, not global `Main.Update` parity. Legacy `WorldGen.UpdateWorld`, network cadence,
client presentation and unmodeled entity phases remain separate cards.

## Focused Evidence

Evidence: `Build/diagnostics/main-migration/task-9-tick-order-world-phase-boundary/20260822-016000/`.
TickOrder and WorldRules verifiers plus scoped diff check pass.
