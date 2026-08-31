# NPC `checkDead` Good World projectile intent owner (2026-08-31)

## Result

This batch adds a typed pure owner for the Good World death branch that emits the fixed projectile
spawn intent for NPC type `631`. The actual projectile definition, allocation, authority, and
network replication remain outside this owner.

## Source contract

The source oracle is `D:\\TRbackup\\Version4物理删除了某些文件\\Terraria\\NPC.cs` with current
SHA-256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`.
At `NPC.cs:64755-64758`, `Main.getGoodWorld && type == 631` creates projectile type `99` at
`base.Center`, with zero velocity, damage `70`, knockback `10f`, and owner `Main.myPlayer`.

## Typed owner

`NpcCheckDeadGoodWorldProjectileInput` carries the world gate, NPC type, center, and projectile
owner. `NpcCheckDeadGoodWorldProjectileDecision` carries the immutable intent. The policy validates
the bounded NPC/owner domains and finite position, then preserves the exact type, position,
velocity, damage, and knockback constants without accessing `Main`, ECS world, random state,
network, loot, or event services.

## Verification

- RED verifier build exited `1` with only missing owner/API symbols:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-goodworld-projectile-red/`.
- GREEN focused build/run exited `0` and includes
  `PASS: NPC checkDead Good World projectile intent policy`:
  `Build/diagnostics/npc-complete/task-10-interaction/20260831-checkdead-goodworld-projectile-green/`.
- Final serial Simulation/NPC verifier/Server gate and verifier run are recorded under the final
  batch directory after documentation updates.

## Deliberate stop boundary

This policy is not wired into `DomeSimulation` and does not allocate or replicate a projectile,
resolve Good World state, invoke death/loot/event systems, or claim complete `checkDead` parity.
The overall NPC migration remains partial/deferred.
