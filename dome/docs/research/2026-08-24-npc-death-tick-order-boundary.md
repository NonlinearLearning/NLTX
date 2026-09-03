# NPC death tick-order boundary

## Source oracle

- Path: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`
- SHA-256: `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`
- `checkDead`: lines `64571-64752`
- `NPCLoot` and final inactive transition: lines `64735-64752`

The source makes death and loot observable before the killed NPC leaves the active collection.
The ECS equivalent is intentionally narrower but explicit: combat damage resolves first, domain
commands commit the killed lifecycle and drop, and only then is the authoritative snapshot
published.

## Accepted invariant

`ResolveCombat < CommitDomainCommands < PublishSnapshot`, with an inactive NPC replication and
exactly one deterministic world-item drop visible after the completed tick. This is phase-order
evidence, not full source-loop parity.

## Deferred

Special `checkDead` branches, complete NPC/loot tables, global random ordering, event progression,
network cadence and client death presentation remain separate cards.
