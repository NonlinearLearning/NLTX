# Main Tick Progression Transition Boundary

## Accepted

`DomeSimulation` now publishes one transient `WorldProgressionTransition` after the
authoritative `WorldProgressionSystem.Advance` step when Blood Moon or Eclipse changes edge.
The fact includes the simulation tick, event kind, and started/stopped direction. The fact is
cleared at the beginning of every tick; paused ticks do not advance progression or publish it.
Snapshot continuation preserves the event state and does not emit a duplicate transition when
there is no new edge.

## Evidence

- `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- `Build/diagnostics/main-tick/task-6-progression-transition/20260824-111500/`
- `src/Terraria.Dome.Simulation/World/WorldProgressionTransition.cs`

Verification: focused WorldRules, loopback, MainBoundary, and serial Simulation Release all exited `0`.

## Deferred

Random event starts, legacy random-stream parity, NPC event spawn tables, client chat/sound/UI,
and complete Version4 Main parity remain outside this boundary.
