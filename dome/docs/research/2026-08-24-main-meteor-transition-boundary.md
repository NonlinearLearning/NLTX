# Main Tick Meteor Transition Boundary

## Accepted

The simulation publishes one transient `WorldMeteorTransition` for authoritative meteor schedule
start, cutoff cancellation, and scheduled impact resolution. It records the Tick, schedule state
before/after, the schedule edge, and whether an impact was queued for the existing tile-commit
owner. The fact also carries the source schedule or impact command `Sequence`; boundary-generated
cancellation keeps `-1`. Snapshot continuation preserves a pending schedule without repeating its
transition.

## Evidence

- `src/Terraria.Dome.Simulation/World/WorldMeteorTransition.cs`
- `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- `Build/diagnostics/main-tick/task-8-meteor-transition/20260824-120000/`
- `Build/diagnostics/main-tick/task-8-meteor-fact-identity/20260828-170000/`

Verification: focused WorldRules, loopback, MainBoundary, and serial Simulation Release all exited `0`.

## Deferred

Random meteor triggering, client messages/ambience, and complete legacy Main parity remain
deferred. Meteor tile mutation remains owned by the existing world command commit path.
