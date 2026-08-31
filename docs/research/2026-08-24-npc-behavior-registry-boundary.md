# NPC behavior registry boundary

`NpcBehaviorSystem` now dispatches through an explicit `NpcBehaviorRegistry`
rather than a fallback switch. The default registry has typed handlers for
`OrdinaryChase`, `TownHome`, `FloatingEye`, `Segment`, and `TrainingDummy`.

An undefined or unregistered `NpcBehaviorId` fails closed with
`InvalidOperationException`; it no longer silently produces a zero movement
result. The registry constructor also rejects undefined IDs and null handlers.

Focused evidence:

- `Build/diagnostics/npc-complete/task-4-behavior-registry/20260824-064500/simulation-build.log`
- `Build/diagnostics/npc-complete/task-4-behavior-registry/20260824-064500/npc-focused.log`
- `Build/diagnostics/npc-complete/task-4-behavior-registry/20260824-064500/summary.txt`
- `Build/diagnostics/npc-complete/task-4-behavior-registry/20260824-073000/simulation-build.log`
- `Build/diagnostics/npc-complete/task-4-behavior-registry/20260824-073000/npc-focused.log`
- `Build/diagnostics/npc-complete/task-4-behavior-registry/20260824-073000/summary.txt`

The serial Simulation build and NPC verifier both exit `0`. This closes the
behavior dispatch authority boundary only; it does not claim complete AI-family
parity. Boss phases, full worm movement, and the NPC-owned ranged projectile
adapter remain deferred.
