# Main Tick Invasion Transition Boundary

## Accepted

The simulation publishes one transient `WorldInvasionTransition` after authoritative invasion
progression and travel processing. It records the Tick, previous/current invasion type and size,
previous/current travel position, start/progress/complete edges, and the clear flag for a completed
invasion. It carries the source start/progress command `Sequence`; travel-only changes and
generated completion without a progress request retain `-1`. Travel consumes the resolved
simulation time rate and runs after the world-clock phase.
Existing completion events remain authoritative and are cleared at Tick start.

Snapshot continuation preserves active invasion state without repeating a committed transition or
completion event. Clear flags remain mapped by the source invasion type.

## Evidence

- `src/Terraria.Dome.Simulation/World/WorldInvasionTransition.cs`
- `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- `Build/diagnostics/main-tick/task-7-invasion-transition/20260824-114500/`
- `Build/diagnostics/main-tick/task-7-invasion-fact-identity/20260828-180000/`

Verification: focused WorldRules, loopback, MainBoundary, and serial Simulation Release all exited `0`.

## Deferred

Random invasion starts, NPC spawn-table selection, and complete NPC-driven invasion parity remain
deferred.
