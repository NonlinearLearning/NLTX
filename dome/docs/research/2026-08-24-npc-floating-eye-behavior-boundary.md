# NPC FloatingEye Behavior Boundary

Source oracle: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, `AI_002_FloatingEye`
around lines `42409-42620`. The accepted slice owns typed horizontal/vertical pursuit intent,
finite acceleration parameters and explicit speed limits through `NpcFlyingState`.

The Simulation deliberately excludes the legacy `ai[]` timers, random sound, lighting, alpha and
tile-collision flags. `NpcBehaviorSystem` emits only movement intent; shared movement/collision
remains the downstream owner. The focused NPC verifier covers valid target pursuit, direction and
speed bounds. Full type table coverage, collision response, LOS, despawn encouragement and client
presentation remain deferred.
