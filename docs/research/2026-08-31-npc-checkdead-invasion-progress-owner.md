# NPC `checkDead` invasion progress owner (2026-08-31)

## Result

This batch adds a pure typed owner for the source branch that converts a killed NPC into
invasion progress. It preserves the source invasion-group table, special point values, group
matching, and clamping of the remaining invasion size. World-state mutation, progress reporting,
network message 78, and event presentation remain deferred.

Overall status remains **NPC field/property migration: partial**.

## Source contract

The source oracle is `D:\\TRbackup\\Version4物理删除了某些文件\\Terraria\\NPC.cs`, whose current
SHA-256 is
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`.

- `NPC.cs:64762-64766` obtains the NPC invasion group and exits when it does not match the
  active invasion or when no group exists.
- `NPC.cs:64767-64788` maps types `216` to 5 points, `395`, `491`, and `471` to 10 points,
  `472` and `387` to zero points, and all other grouped types to one point.
- `NPC.cs:64789-64798` subtracts positive points, clamps the remaining size to zero, and
  reports progress using `invasionSizeStart - invasionSize`.
- `NPC.cs:64830-64947` is the exact `GetNPCInvasionGroup` table. Groups 1 through 4 are
  preserved; the source's `-1` and `-2` categories are intentionally excluded because they do
  not participate in the invasion-progress branch.

## Typed owner

The implementation is in `src/Terraria.Dome.Simulation/Npc/Systems`:

- `NpcCheckDeadInvasionProgressInput` carries NPC type and the three invasion counters.
- `NpcCheckDeadInvasionProgressDecision` carries applicability, group, points, remaining size,
  and the source progress-start value.
- `NpcCheckDeadInvasionProgressPolicy` performs the bounded, deterministic query without
  reading `Main`, random state, ECS world state, network, loot, or event services.

The policy rejects out-of-range NPC types and negative counters. It does not mutate its input.

## Verification

- TDD RED build exited `1` with only the missing invasion-progress owner/API symbols:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-invasion-progress-red/`.
- Focused GREEN build exited `0` with zero warnings/errors. The isolated Release DLL run exited
  `0` and includes `PASS: NPC checkDead invasion progress policy`:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-invasion-progress-green/`.
- `source-hash.log` and the source excerpt are stored beside the focused evidence. The full
  Simulation/Protocol/NPC verifier/Server serial Release gate is still required before this batch
  can be recorded as accepted.

## Deliberate stop boundary

This owner does not update `WorldProgressionState`, emit progress messages, run DD2/Frost Moon/
Pumpkin Moon progression, invoke loot, publish death events, or claim complete `checkDead`
parity. `canRemoveLegacyWorldGen=false`, the 44 deferred `ServerRelevant` rows, and all broader
NPC/death/event/network behavior remain unchanged.
