# NPC death loot restart boundary

## Source oracle

- Path: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`
- SHA-256: `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`
- `checkDead`: lines `64571-64752`
- `NPCLoot` call and final inactive transition: lines `64735-64752`

The source performs the loot call before setting the NPC inactive. The Simulation has the same
observable tick ordering: a killed lifecycle is published in the death phase and its typed loot
command is committed in the following loot phase of the same tick.

## Implemented narrow contract

`DomeSimulation.RestoreNpc` restores an inactive NPC with `NpcDespawnReason.Killed` as already
published. This is a restart marker for the completed death side effect, not a general NPC loot
table or death-state reconstruction. It prevents a snapshot taken after the atomic death/loot
commit from publishing another drop on the next tick.

## Rejected and deferred

- snapshots taken inside an externally interrupted phase are not a supported persistence boundary;
- complete `NPC.checkDead` special branches, event progression and player-context behavior;
- complete NPC and loot tables;
- global `Main.rand` ordering and random drop parity;
- network/client death presentation.

The card accepts only no-duplicate replay of an already committed killed NPC.
