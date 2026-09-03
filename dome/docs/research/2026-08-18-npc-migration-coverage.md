# NPC Migration Coverage

Statuses are deliberately conservative: `partial` means a compatibility slice exists, while
`planned` and `excluded` are not claims of behavior parity.

| Source behavior family | Status | First-slice evidence or reason |
| --- | --- | --- |
| NPC identity, type and defaults | verified | Typed definition, lifecycle and replication components are covered by the NPC verifier. |
| Natural spawning (`Spawner`) | verified | Bounded command eligibility and deterministic commit reject invalid space, budget and identities. |
| Target selection | verified | Nearest live player, invalid-target replacement and stable-ID ties are covered by the NPC verifier. |
| Behavior dispatch authority | verified | `NpcBehaviorRegistry` explicitly registers the supported handlers; unknown IDs fail closed. The fixed pipeline keeps target selection before behavior and movement intent after it. Evidence: `Build/diagnostics/npc-complete/task-4-behavior-registry/20260824-064500/`. |
| Ordinary chase behavior | verified | Typed chase state produces movement intent and resolves through shared tile collision. |
| FloatingEye aerial pursuit (`AI_002`) | partial | `NpcFlyingState` and bounded movement intent are verified; legacy timers, LOS, collision flags and presentation remain deferred. |
| Town NPC home behavior | verified | Home, homeless and timeout component state is covered by composition and snapshot-restore verifiers. |
| Segmented NPC relationships | partial | Root/parent/child links, unique per-root indexes, child-before-root death order, and typed bounded parent-follow calculation are covered; Arch parent-position wiring, complete worm movement and shared-life parity remain deferred. |
| Movement and tile collision | verified | Shared movement/collision produces deterministic chase stopping at solid tiles. |
| Contact damage and combat | partial | Hostile-only contact commands, defense reduction and hit-immunity cooldown are covered; PvP policy and complete collision families remain deferred. |
| Death, despawn and replication revision | verified | Lifecycle, inactive revision and one-time death publication are covered by focused replay checks. |
| Ordinary deterministic loot | verified | Immutable loot definitions preserve deterministic drops for the first NPC type. |
| Boss encounters and Boss AI families | excluded | Separate behavior-family package; global encounter state belongs to resources. |
| Invasions and moon/world events | excluded | Global progression is not NPC instance state; deferred to later batches. |
| Complete `AI_###` families | excluded | 158 source markers are inventoried, not mechanically ported. |
| Complete Buff/status behavior | excluded | Requires a dedicated typed status-effect package. |
| Full town services, dialogue and housing | excluded | Town skeleton deliberately stops at home/return movement. |
| Legacy networking and `NetMessage` calls | excluded | Protocol projection is an adapter; Simulation must not reference legacy APIs. |

No row above implies that the deleted `Terraria.NPC` source is restored or referenced at runtime.
