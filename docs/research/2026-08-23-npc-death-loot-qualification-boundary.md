# NPC death and loot qualification boundary

## Source oracle

- Path: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`
- SHA-256: `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`
- `checkDead`: lines `64571-64752`
- `NPCLoot`: lines `65312-65416`

The source first rejects inactive, non-root worm segments and positive-health NPCs, then executes
many type-, world-rule-, player-, event-, random-, network- and presentation-dependent branches
before calling `NPCLoot`. The member is therefore not a single portable loot contract.

## Current authority route

The Simulation route is intentionally narrower:

`NpcLifecycleSystem -> PublishNpcDeaths -> NpcDeathSystem -> CommitNpcLoot -> SpawnNpcLoot`.

`PublishNpcDeaths` keeps a runtime `NpcHandle` set so one killed NPC publishes at most once during
the active simulation lifetime. `NpcDefinition` supplies an explicit loot-table identity; the
server does not infer a complete table from the legacy NPC type number or consume global
`Main.rand` ordering.

## Focused result

`Test/Terraria.Dome.Npc.Verification` passed and covered source contract, target/chase baseline,
death revision and deterministic loot. `Test/Terraria.Dome.MainBoundary.Verification` checked 771
Simulation source files with zero forbidden dependencies. Scoped `git diff --check` passed for the
proposal files.

## Decision

This card remains `unknown/deferred` for full NPC death/loot parity. The accepted evidence is only
the narrow one-shot typed lifecycle route already present in the tree. It does not accept:

- boss and special `checkDead` transformations or follow-up NPC/projectile spawns;
- town NPC tombstones, announcements, sounds, achievements or progression counters;
- invasion, event and player-context branches;
- complete NPC definition and loot tables;
- legacy global random ordering;
- network cadence or client/presentation effects;
- persistence of the in-memory published-death set across a restart.

The next implementation card may only add a source-backed child with a stable identity and a
restart/replay contract; it must not broaden this card into `NPCLoot` parity.
