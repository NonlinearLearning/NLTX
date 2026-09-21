# NPC death invasion-progress pipeline owner

## Scope

N3.143 connects the existing checkDead invasion-progress decision and command bridge to the
server-authoritative `DomeSimulation` NPC death pipeline. It is intentionally limited to the
death publication boundary; it does not claim complete invasion or NPC parity.

## Source and runtime boundary

`PublishNpcDeaths()` runs in the NPC `Death` stage after lifecycle settlement and before loot.
For each newly published killed NPC it evaluates `NpcCheckDeadInvasionProgressInput` from the
definition `NetId` and current `WorldProgressionState`, allocates a simulation-owned monotonic
sequence, and calls `TryQueueWorldInvasionProgress`. World progression consumes that queue at the
start of the next tick.

## Evidence

Evidence directory: `Build/diagnostics/npc-complete/task-10-interaction/20260831-invasion-progress-pipeline-final/`

- `summary.json`: simulation, NPC verifier, server build, and `git diff --check` all exit `0`.
- `npc-verifier.log`: 29 PASS lines and no FAIL/ERROR lines.
- Runtime fixture starts invasion type 1 at size 10, kills two group-1 NPCs in one tick, observes
  size 10 during the death tick and size 8 on the following tick.
- Negative fixtures confirm mismatched invasion group and zero invasion do not enqueue progress.

## Deferred boundary

The death sequence allocator is not persisted yet, and this slice does not establish cross-restart
sequence continuity, complete invasion event/message parity, full `NPCLoot`, or complete NPC behavior
parity. Overall migration status remains `partial/deferred`.
