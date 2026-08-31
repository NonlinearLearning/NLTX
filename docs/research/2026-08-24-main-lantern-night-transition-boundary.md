# Main Tick Lantern Night Transition Boundary

## Accepted

The simulation publishes one transient `WorldLanternNightTransition` after authoritative
progression processing. It records next-night schedule state before/after, active Lantern Night
state before/after, schedule consumption, and start/stop edges. Same-Tick weather suppression still
uses the final authoritative progression state. Snapshot continuation does not re-consume an
already consumed schedule.

The direct `TryQueueWorldEvent(LanternNight)` authority boundary now also rejects an active
meteor schedule, Blood Moon, or invasion. This closes a bypass where a caller could skip the
existing `LanternNightEligibilitySystem` conflict guards by submitting a direct event command.
The source guard is anchored by `Main.cs:13724-13732` and the Lantern Night transition path at
`Main.cs:13765-13768`.

The progression commit path also rejects an impossible same-tick Blood Moon/Lantern Night
combination, even when both individually valid commands were queued before the tick began.
Blood Moon now observes the same meteor/invasion/Lantern Night conflict boundary, and Lantern
Night observes Blood Moon/meteor/invasion conflicts at commit time.

Pending `WorldEventStartCommand` inputs now also require unique `Sequence` values across event
kinds. This makes the command identity one-to-one before commit while preserving distinct-sequence
same-tick conflict handling at the progression boundary.

`WorldProgressionTransition` now carries that source `Sequence` for accepted Blood Moon/Eclipse
starts and uses `-1` for boundary-generated stops. The transition therefore exposes the minimum
`WorldTick` + `Sequence` + `Kind` identity required for the authoritative progression fact.

Scheduled Lantern Night transitions also retain the accepted first-clear command `Sequence` until
the later night boundary consumes the schedule. The pending request queue is cleared after each
tick, so this retained sequence is the authoritative source for the one-shot schedule-consumption
fact rather than a stale request-list lookup.

The retained sequence is now part of `WorldProgressionState` and the persisted world format. A
restore before dusk therefore publishes the same scheduled transition identity, while a restore
after consumption persists the cleared `-1` value and cannot repeat the fact.

## Evidence

- `src/Terraria.Dome.Simulation/World/WorldLanternNightTransition.cs`
- `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- `Build/diagnostics/main-tick/task-8-lantern-night-transition/20260824-130000/`
- `Build/diagnostics/main-tick/task-6-lantern-night-eligibility/20260828-050000/`
- `Build/diagnostics/main-tick/task-12-gate/20260828-060000/`
- `Build/diagnostics/main-tick/task-6-event-conflict-guards/20260828-070000/`
- `Build/diagnostics/main-tick/task-12-gate/20260828-080000/`
- `Build/diagnostics/main-tick/task-6-event-sequence-identity/20260828-090000/`
- `Build/diagnostics/main-tick/task-12-gate/20260828-100000/`
- `Build/diagnostics/main-tick/task-6-event-fact-identity/20260828-112000/`
- `Build/diagnostics/main-tick/task-12-gate/20260828-120000/`
- `Build/diagnostics/main-tick/task-6-lantern-fact-identity/20260828-140000/`
- `Build/diagnostics/main-tick/task-12-gate/20260828-140000/`
- `Build/diagnostics/main-tick/task-6-lantern-fact-persistence/20260828-150000/`
- `Build/diagnostics/main-tick/task-12-gate/20260828-150000/`
- `Build/diagnostics/main-tick/task-6-lantern-fact-persistence/20260828-160000/`
- `Build/diagnostics/main-tick/task-12-gate/20260828-160000/`

Verification: focused WorldRules, loopback, MainBoundary, and serial Simulation Release all exited `0`.

## Deferred

Client ambience/UI, complete legacy random/event parity, NPC event spawn tables, and complete Main
parity remain deferred.
