# NPC Migration Coverage

Statuses are deliberately conservative: `partial` means a compatibility slice exists, while
`planned` and `excluded` are not claims of behavior parity.

| Source behavior family | Status | First-slice evidence or reason |
| --- | --- | --- |
| NPC identity, type and defaults | verified | Typed definition, lifecycle and replication components are covered by the NPC verifier. |
| Natural spawning (`Spawner`) | verified | Bounded command eligibility and deterministic commit reject invalid space, budget and identities. |
| Target selection | verified | Nearest live player, invalid-target replacement and stable-ID ties are covered by the NPC verifier. |
| Ordinary chase behavior | verified | Typed chase state produces movement intent and resolves through shared tile collision. |
| Town NPC home behavior | verified | Home, homeless and timeout component state is covered by composition and snapshot-restore verifiers. |
| Segmented NPC relationships | verified | Root/parent/child links, index validation and child-before-root death order are covered by composition verification. |
| Movement and tile collision | verified | Shared movement/collision produces deterministic chase stopping at solid tiles. |
| Contact damage and combat | verified | Contact commands, defense reduction and hit-immunity cooldown are covered by the NPC verifier. |
| Death, despawn and replication revision | verified | Lifecycle, inactive revision and one-time death publication are covered by focused replay checks. |
| Ordinary deterministic loot | verified | Immutable loot definitions preserve deterministic drops for the first NPC type. |
| Boss encounters and Boss AI families | excluded | Separate behavior-family package; global encounter state belongs to resources. |
| Invasions and moon/world events | excluded | Global progression is not NPC instance state; deferred to later batches. |
| Complete `AI_###` families | excluded | 158 source markers are inventoried, not mechanically ported. |
| Complete Buff/status behavior | excluded | Requires a dedicated typed status-effect package. |
| Full town services, dialogue and housing | excluded | Town skeleton deliberately stops at home/return movement. |
| Legacy networking and `NetMessage` calls | excluded | Protocol projection is an adapter; Simulation must not reference legacy APIs. |

No row above implies that the deleted `Terraria.NPC` source is restored or referenced at runtime.
