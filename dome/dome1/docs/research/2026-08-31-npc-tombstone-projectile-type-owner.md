# NPC town tombstone projectile type owner (2026-08-31)

## Result

This batch extracts the deterministic projectile-type mapping inside `DropTombstoneTownNPC`.
The random sample is supplied by the caller; this owner only validates the domain and resolves
the source mapping.

## Source contract

The source oracle is `D:\\TRbackup\\Version4物理删除了某些文件\\Terraria\\NPC.cs` with current
SHA-256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`.
At `NPC.cs:64815-64824`, type `17` or `441` uses `Next(5) + 527`; all other types use
`Next(6)`, mapping sample `0` to projectile `43` and samples `1..5` to `201..205`.

## Typed owner

`NpcTombstoneProjectileTypePolicy.Resolve` validates the NPC type and sampled range, then returns
the exact source projectile type. It has no random generator, position, text, projectile
allocator, network, or world dependency.

## Verification

- RED build exited `1` after temporarily removing only the new policy file; failures were limited
  to missing policy symbols:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-tombstone-projectile-red/`.
- Final serial Simulation/NPC verifier/Server builds and verifier run exited `0`; the verifier
  reports `28 PASS` and `0 FAIL/ERROR`:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-tombstone-projectile-final/`.

## Deliberate stop boundary

This owner does not run `Main.rand`, create tombstones, choose death text, spawn projectiles, or
publish network messages. Complete town-NPC death, loot, event, and NPC parity remain deferred.
