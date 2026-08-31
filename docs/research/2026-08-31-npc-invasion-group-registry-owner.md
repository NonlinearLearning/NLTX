# NPC invasion-group registry owner (2026-08-31)

## Result

This batch extracts the exact grouped NPC type table used by `checkDead` invasion progress into a
versioned immutable registry. The registry is consumed by the N3.137 progress policy and keeps
the table separate from world-event mutation.

## Source contract

The source oracle is `D:\\TRbackup\\Version4物理删除了某些文件\\Terraria\\NPC.cs` with current
SHA-256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`.
`NPC.cs:64830-64947` maps 32 NPC types into four invasion groups. The source's `-1`, `-2`, and
`-3` categories are intentionally excluded because `checkDead` only advances groups `1..4`.

## Typed owner

`LegacyNpcInvasionGroupRegistry` exposes a bounded `0..696` NPC domain, an immutable dictionary,
the exact 32 grouped types, and fail-closed lookup for ordinary/non-invasion types. It has no
world, network, random, loot, or event dependency.

## Verification

- TDD RED build exited `1` after temporarily removing only the new registry file; errors were
  limited to the missing registry symbol in the progress policy:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-invasion-group-registry-red/`.
- Final serial Simulation/NPC verifier/Server builds and verifier run exited `0`; the verifier
  reports `27 PASS` and `0 FAIL/ERROR`:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-invasion-group-registry-final/`.
- Style and forbidden-reference checks report zero issues.

## Deliberate stop boundary

The registry does not update `WorldProgressionState`, emit message 78, run invasion/event
progression, or claim complete `checkDead` or NPC parity. The overall migration remains partial.
