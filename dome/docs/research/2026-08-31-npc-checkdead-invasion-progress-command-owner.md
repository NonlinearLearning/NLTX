# NPC `checkDead` invasion progress command bridge (2026-08-31)

## Result

This batch adds the typed bridge from the N3.137 invasion-progress decision to the existing
`WorldInvasionProgressCommand`. It preserves the command amount and requires the caller to supply
the authoritative sequence.

## Typed owner

`NpcCheckDeadInvasionProgressCommandPolicy.TryCreate` accepts only an applicable positive-point
decision and a non-negative, non-terminal sequence. It creates the existing world command without
mutating `WorldProgressionState`, inventing an NPC death ordering, or emitting network traffic.

## Verification

- RED build exited `1` with only missing bridge symbols:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-invasion-progress-command-red/`.
- Final serial Simulation/NPC verifier/Server builds and verifier run exited `0`; the verifier
  reports `29 PASS` and `0 FAIL/ERROR`:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-invasion-progress-command-final/`.

## Deliberate stop boundary

The bridge is not a sequence allocator and does not automatically connect NPC death publication to
world progression. Duplicate command rejection, active invasion checks, world mutation, transition
events, and message 78 remain owned by the existing world systems.
