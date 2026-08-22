# NPC time-left lifecycle boundary

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`
  - SHA-256: `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`
  - `CheckActive` decrement and inactive transition: lines 64529-64545

## Accepted narrow predicate

`NpcLifecycleSystem` now treats every `TimeLeft <= 0` state as timed out after health-death has
been checked. A forged negative timer therefore transitions to inactive with
`NpcDespawnReason.TimedOut` and cannot remain active indefinitely.

## Deferred source branches

This card does not infer NPC type-specific `activeTime` values, AI families, spawn tables, revenge
state, network synchronization, worm construction, loot or client presentation.

## Verification

`Test/Terraria.Dome.Npc.Verification` covers health death, negative timer timeout and the stable NPC
pipeline order.
