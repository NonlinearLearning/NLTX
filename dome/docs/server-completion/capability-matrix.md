# Core Server Capability Matrix

This matrix is derived from
`docs/plans/2026-08-13-dome-core-server-90-percent-implementation.md`.
Only an `evidenced` row in `completion-manifest.json` can receive its weight. Every such row must
identify its legacy reference, Dome authority owner, V1456 projection and executable verifier.

| ID | Family | Weight | Reference | Authority | Projection | Verifier | Status | Score |
|---|---|---:|---|---|---|---|---|---:|
| A | Tick/session/ordering/disconnect | 8 | `MessageBuffer.cs` session handling; `RemoteClient.cs` lifecycle | monotonic protocol sequence, bounded queue, simulation tick and teardown | V1456 session command routes and per-session replication state | Tile ordering, load flood, and session replication verifiers | Evidenced | 8 |
| B | World tiles/section PVS/mutation | 12 | `MessageBuffer.cs:17` | `WorldGrid`, validator, session cursor | `TileManipulationIntent`, `TileSection` | Tile interaction and session replication verifiers | Evidenced | 12 |
| C | Tile collision/player lifecycle | 10 | `Player.cs` movement/lifecycle; `MessageBuffer.cs:13,14,16` | `TileCollisionSystem`, `PlayerLifecycleComponent`, server contact damage and automatic respawn | `PlayerActive`, `PlayerLifeMana`, authoritative player state projection | PlayerAuthority and player lifecycle loopback verifiers | Evidenced | 10 |
| D | Inventory/items/pickups | 12 | `MessageBuffer.cs:5,21`, `NetMessage.cs:5,21` | `InventoryComponent`, `ItemDefinitionRegistry`, `UseItemCommand`, `ItemInteractionValidator`, deterministic proximity pickup | `PlayerControls.UseItem/SelectedItem`, server-only `SyncItem` | Items unit and two-session item loopback verifiers | Evidenced | 12 |
| E | NPC/combat/drops/PVS | 12 | `NetMessage.cs:23` and PVS send | Replication snapshot, damage, loot, world-owned NPC | `SyncNPC` | Combat, protocol and loopback verifiers | Evidenced | 12 |
| F | Projectile/collision/ownership/PVS | 8 | `NetMessage.cs:27,29` and PVS send | Owner, cooldown, swept collision, tombstone | `SyncProjectile`, `KillProjectile` | Combat, protocol and loopback verifiers | Evidenced | 8 |
| G | Containers/signs/tile entities | 8 | `Chest.cs`; `MessageBuffer.cs:31`; `NetMessage.cs:32` | stable chest ID, `PlayerHandle` opener, range/PVS validation | V1456 `RequestChestOpen` / `SyncChestItem` | WorldObjects unit and two-session loopback verifiers | Partial | 0 |
| H | Persistence/generation/recovery | 10 | `WorldFile.cs` and metadata call sites | `DomeSimulationSnapshot`, `WorldGridSnapshot`, versioned format, atomic save coordinator | restored world/NPC/world-item values adopted by `DomeServer`; sessions recreated | Persistence and save/restart loopback verifiers | Evidenced | 10 |
| I | Liquid/wire/door/tile objects | 8 | `WorldGen.cs` tile action paths; `MessageBuffer.cs:19` | stable server-owned door ID, range validation, deterministic revision | V1456 `ToggleDoorState` request and server projection; liquid/wire/actuator inputs remain rejected | WorldMechanics unit and two-session loopback verifiers | Evidenced | 8 |
| J | World rules/events/spawning | 6 | `Main.cs` world time state; `NetMessage.cs:SetTime` | deterministic `WorldRuleSnapshot` and bounded session rule cursor | V1456 `SetTime` | WorldRules unit and active-session loopback verifiers | Evidenced | 6 |
| K | V1456 authority/hardening | 6 | `MessageBuffer.cs` packet validation; `RemoteClient.cs` isolation | `TerrariaPacketDispatcher`, `DomeServer` bounded commands, `SessionReplicationState` bounded async per-session writer | typed V1456 routes, explicit rejection, server-owned state frames | Hardening loopback and protocol verifiers | Evidenced | 6 |
| **Total** |  | **100** |  |  |  |  |  | **92** |

The current 92-point score reflects fresh focused regression evidence. Families C, E, H, and J
were re-run from the current worktree and their required PlayerAuthority, combat loopback,
persistence/load, and WorldRules loopback paths exited `0`. Existing partial paths need a complete
authority and execution proof chain before they can contribute to the 90-percent gate.

The physical deletion audit is separate from this weighted score. A fresh G child trace at
`Build/diagnostics/server-ecs-convergence/P5-worldobjects/20260822-2200/trace.md` now proves an
atomic expected-revision guard for Simulation chest transfers. G remains `Partial` because the
existing V1456 transfer wire shape does not carry that revision, the inbound Training Dummy
route is intentionally limited to the source-backed type-0/visible-section path, and complete
client-side tile-entity mutation authorization across all entity types is still absent; no score
change is claimed. The ledger at
`docs/migrations/version4-physical-deletion-ledger.csv` has 535 rows, and its completeness gate
currently fails with 48 explicitly deferred `ServerRelevant` rows; `ExtractinatorHelper`,
`ItemTrader`, and the transient `Terraria.Net.Ping` protocol behavior are recorded as replaced
with evidence. WorldGen remains governed by
`docs/worldgen/worldgen-deletion-gate.json`.
