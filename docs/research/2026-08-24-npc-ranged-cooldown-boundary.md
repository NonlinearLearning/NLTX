# NPC Ranged Attack and Cooldown Boundary

The source family is the projectile-producing branches in Version4 `Terraria/NPC.cs` (for
example `AI_084_LunaticCultist`, where attack timers gate `Projectile.NewProjectile`). The
accepted Simulation slice is `NpcRangedAttackState` plus `NpcRangedAttackSystem`: it validates
identity, finite positions, target/line-of-sight input and projectile parameters, emits a typed
`NpcRangedAttackCommand`, normalizes velocity to the configured speed, and decrements/enforces a
deterministic cooldown.

`NpcRangedAttackCommitSystem` now consumes that command against the typed
`ProjectileDefinitionRegistry` and emits `NpcProjectileSpawnRequest`. Unknown or friendly
projectile definitions are rejected, and the request preserves the `NpcHandle` source without
entering the player-owned `SpawnProjectileCommand` path. The final Projectile/Combat adapter
still needs to own NPC projectile identity, persistence and replication. Legacy random attack
selection, complete projectile tables, expert scaling, animation, sound and client presentation
remain deferred.

`NpcProjectileSpawnSystem` now creates an Arch projectile entity from the typed request with
`NpcProjectileOwnerComponent` and `NpcProjectileNetworkIdentityComponent`. The existing
player-owned projectile spawn and replication path remains unchanged.
`NpcProjectileReplicationSystem` now projects the NPC-owned entity into a typed simulation
snapshot; protocol encoding, persistence, and client replication remain deferred.

Evidence: `Build/diagnostics/npc-complete/task-5-ranged/20260824-113000/`.
