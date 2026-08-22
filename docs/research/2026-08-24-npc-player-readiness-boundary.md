# NPC Player-Readiness Spawn Boundary

The source `NPC.Spawner.CanSpawnEnemiesNear(Player)` predicate is a complete, independently
owned guard and does not require NPC type tables or spawn-rate formulas.

Source: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, SHA256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`, lines `256-275`.

Accepted predicate:

- inactive or dead player rejects spawning;
- Journey mode rejects only when the per-player spawn-rate power is unlocked and disables
  spawning for that player;
- a player near NPC type 398 (Moon Lord) rejects spawning;
- otherwise the player is spawn-ready.

The ECS route is `NpcSpawnPlayerReadiness` plus
`NpcSpawnPlayerReadinessQuery.CanSpawnEnemiesNear`. `NpcSpawnEligibilitySystem` consumes the
typed snapshot when present and retains the existing boolean compatibility input for callers that
have not projected player facts yet.

Deferred: spawn tile search/rate formulas, player stat predicates, NPC definition tables, and
client/UI side effects.
