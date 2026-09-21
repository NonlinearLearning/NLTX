# Main Tick Slime Rain Transition Boundary

## Accepted

The simulation publishes one transient `WorldSlimeRainTransition` after authoritative progression
processing. It records rain duration, cooldown and warning counters before/after the Tick, plus
start/stop, cooldown start/end, and warning publication edges. The existing
`WorldSlimeRainWarningEvent` remains the committed warning fact; the new transition does not add
client presentation behavior. Start and stop edges carry their source command `Sequence`; natural
expiry, cooldown, and warning-only changes retain `-1`.

Snapshot continuation preserves active rain and cooldown state without repeating a start/stop edge
or warning publication. Silent starts remain warning-free and announce-enabled expiry publishes
exactly once.

## Evidence

- `src/Terraria.Dome.Simulation/World/WorldSlimeRainTransition.cs`
- `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- `Build/diagnostics/main-tick/task-8-slime-rain-transition/20260824-123000/`
- `Build/diagnostics/main-tick/task-8-slime-fact-identity/20260828-190000/`

Verification: focused WorldRules, loopback, MainBoundary, and serial Simulation Release all exited `0`.

## Deferred

Random Slime Rain triggering, NPC spawn tables, client ambience/UI, and complete legacy Main parity
remain deferred.
