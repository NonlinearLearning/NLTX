# Legacy Player Behavior Map

This map freezes the externally supplied `Terraria.Player` source as a
read-only behavior oracle. The source is not copied into this repository's
simulation projects and is not a runtime dependency.

## Oracle

| Field | Value |
| --- | --- |
| Source | `D:\\TRbackup\\Version4物理删除了某些文件\\Terraria\\Player.cs` |
| Bytes | `634315` |
| Last write (local time) | `2026-08-13 20:47:18` |
| SHA-256 | `AF4C868BE773599E271853A549791405AF8F9CD315191B1FE4357E92BD84D7C1` |
| Retrieval | `Get-Content -Raw 'D:\\TRbackup\\Version4物理删除了某些文件\\Terraria\\Player.cs'` |
| Declaration inventory | `1092` public/protected member declarations, including nested presentation and compatibility types |

The declaration inventory is intentionally counted from the external artifact;
the CSV below classifies behavior clusters and names representative public or
protected entry points. A cluster status applies to every declaration listed in
its source-line range. New declarations are added to the CSV before a phase is
marked complete.

The migration evidence also mechanically checks that every public/protected
source line matched by `^\s*(public|protected)\b` is covered by at least one
CSV source-line range. The matcher currently finds 1,344 source lines, including
nested types, fields, properties, and methods. This is a coverage check rather
than a replacement for the syntax-level declaration inventory above.

## Cluster Map

| Cluster | Representative legacy entry points | Target ownership | Status |
| --- | --- | --- | --- |
| Identity and lifecycle | `PlayerConnect`, `PlayerDisconnect`, `Spawn`, `KillMe`, `dead`, `respawnTimer` | `PlayerIdentityComponent`, `PlayerLifecycleComponent`, server session lifecycle | Migrated |
| Input and control | `Update`, `UpdateControlHolds`, `HorizontalMovement`, `JumpMovement`, `controlLeft`, `controlRight`, `controlJump` | player input/control systems and typed input batch | Migrated |
| Movement and collision | `Gravity`, `TileCollision`, `FloorVisuals`, `velocity`, `position` | movement, physics, and world-grid collision systems | Migrated |
| Health and mana | `Hurt`, `Heal`, `HealEffect`, `ManaEffect`, `statLife`, `statMana` | combat components, damage commit, vital projections | Migrated |
| Damage, death, respawn | `KillMe`, `UpdateDead`, `Respawn`, immunity and hurt cooldown state | typed commands, lifecycle systems, committed events | Migrated |
| Inventory and item use | `GetItem`, `ItemSpace`, `ConsumeItem`, `ItemCheck`, `QuickSpawnItem` | inventory transfer, selection, item-use command/system | Migrated |
| Equipment and buffs | `UpdateEquips`, `UpdateArmorSets`, `AddBuff`, `UpdateBuffs` | equipment and status-effect domains | Delegated |
| Persistence | `SavePlayerFile`, `LoadPlayerFile`, profile/loadout fields | `PlayerPersistentState` import/restore and snapshot persistence | Migrated |
| Replication | `SyncPlayer`, `NetDefaults`, packet-facing player state | server snapshot and V1.4.5 protocol projections | Migrated |
| World interaction | `TileInteraction`, chest, sign, door, pickup, teleport requests | validated interaction commands and deterministic world commit | Migrated |
| Presentation and social cosmetics | `Draw`, camera modifiers, dust, audio, UI, map and social cosmetics | client adapter and compatibility layer | ClientOnly |
| Specialized mechanics | golf, fishing visuals, wings, ropes, mounts, emotes, mod hooks | named follow-up domain or legacy compatibility dependency | Blocked: specialized rule oracle and client adapter are not in the first core |

## Status Rules

- `Migrated` means authoritative mutable state and the command/commit boundary
  exist in `Terraria.Dome.Simulation` and have a focused verifier.
- `Delegated` means the behavior is intentionally owned by another server,
  protocol, persistence, or compatibility assembly.
- `ClientOnly` means it must not be reintroduced into the simulation assembly.
- `Blocked` is reserved for a named dependency. It is not evidence that a core
  behavior is complete; Phase 7 must leave no unexplained blocked core cluster.

## Verification Anchors

- Core replay and duplicate-input rules: `PlayerSimulation.Verification`.
- Lifecycle and account ownership: `PlayerLifecycle.Verification` and
  `PlayerAuthority.Verification`.
- Vitals and command ordering: `Combat.Protocol.Verification` and
  `Combat.Loopback.Verification`.
- Inventory and import preservation: `Items.Verification` and
  `PlayerAuthority.Verification`.
- World interaction: `TileInteraction.Verification` and
  `WorldObjects.Verification`.
- Snapshot-only replication: `SessionReplication.Verification` and
  `FullClientBootstrap.Verification`.
