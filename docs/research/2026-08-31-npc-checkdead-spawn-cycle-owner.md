# NPC `checkDead` no-spawn-cycle intent owner (2026-08-31)

## Result

This batch adds a typed pure owner for the unconditional `noSpawnCycle = true` write in the
qualified `NPC.checkDead` path. The owner emits an idempotent mark intent and leaves the existing
`NpcSpawnCycleStateComponent` responsible for storage and consume-once behavior.

## Source contract

The source oracle is `D:\\TRbackup\\Version4物理删除了某些文件\\Terraria\\NPC.cs` with current
SHA-256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`.
At `NPC.cs:64670`, after entry qualification and the special transform/Good World branches,
the legacy method unconditionally assigns `noSpawnCycle = true`. The source static is declared
at `NPC.cs:6053`; it is consumed by the spawn-cycle logic outside this owner.

## Typed owner

`NpcCheckDeadSpawnCycleInput` carries the qualification result and current mark state.
`NpcCheckDeadSpawnCycleDecision` reports whether the mark should be emitted and whether a mark was
already present. `NpcCheckDeadSpawnCyclePolicy` emits no intent before qualification, and emits
the source mark intent for every qualified death without touching world state, spawn allocation,
network, loot, or event services.

## Verification

- RED verifier build exited `1` with only the three missing owner/API symbols:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-spawn-cycle-red/`.
- Focused GREEN build/run exited `0` and includes `PASS: NPC checkDead noSpawnCycle intent policy`:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-spawn-cycle-green/`.
- Final serial Simulation/NPC verifier/Server builds, verifier run (`25 PASS`, `0 FAIL/ERROR`),
  and `git diff --check` all exited `0`:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-spawn-cycle-final/`.

## Deliberate stop boundary

The policy is not wired into `DomeSimulation`, does not mutate `NpcSpawnCycleStateComponent`, and
does not claim spawn-cycle scheduling, `CheckActive` integration, complete `checkDead`, or NPC
parity. WorldGen deletion remains gated by `canRemoveLegacyWorldGen=false`, and the 44 deferred
`ServerRelevant` rows remain unchanged.
