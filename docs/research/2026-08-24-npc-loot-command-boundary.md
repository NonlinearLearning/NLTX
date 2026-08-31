# NPC Loot Command Boundary

## Source and authority

- Source oracle: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`
- Legacy anchor: `checkDead`/`NPCLoot`, lines `64571-64752` and `65312-65416`.
- Death authority: `NpcDeathSystem` publishes `NpcDeathEvent` only after the killed lifecycle
  is committed.
- Loot authority: `NpcLootSystem.CreateDrop` owns the deterministic table roll and emits a
  typed `NpcLootCommand`.
- Item authority: `WorldItemSpawnSystem` validates and commits the embedded
  `CreateWorldItemCommand`; NPC code does not mutate the world-item store directly.

## Accepted evidence

The focused NPC verifier proves that a valid death event preserves NPC identity, section and
deterministic stack output, while an invalid NPC identity is rejected before command creation.
The simulation pipeline consumes the command in the loot stage and publishes the item-drop event
after commit. The existing restart verifier proves a committed killed snapshot cannot publish a
second drop.

The server exposes the existing authoritative `ItemReplicationSnapshot` projection for loopback
verification. Revision assertions therefore use persistent item state rather than the transient
created-event buffer.

## Deferred

Complete legacy loot tables, player/difficulty context, global random ordering, rare drops,
achievement effects and client presentation remain deferred. This boundary is a typed command
slice, not full `NPCLoot` parity.
`NpcLootEmissionLedger` now provides an explicit idempotence boundary keyed by
`(NpcHandle, death Tick)`. A repeated death snapshot cannot emit a second
`NpcLootCommand`; if loot creation fails, the key is rolled back so a corrected
retry remains possible.

Evidence: `Build/diagnostics/npc-complete/task-8-loot/20260824-160000/`.
