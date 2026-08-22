# NPC worm segment follow-up boundary

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`
  - `CheckActive_WormSegments`: lines 64548-64570
  - source SHA-256: `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`
- The source is reached from the inactive/dead path at lines 64535-64545. It follows `ai[0]` as
  a next-segment link, only deactivates active `aiStyle == 6` segments, emits the normal NPC
  synchronization event, and stops on self/invalid/non-worm/broken links.

## Accepted narrow route

`NpcSegmentLifecycleSystem.GetWormFollowUpDespawns` receives an explicit worm-segment set from its
caller, validates the existing reciprocal segment graph, follows `Child`, guards repeated handles,
and emits typed `DespawnNpcCommand(handle, Killed)` values. The caller-supplied classification is
intentional: the current tree has no complete NPC type/AI table and must not infer `aiStyle == 6`
from an unsupported local default.

## Deferred scope

- NPC type table and full AI-family mapping;
- exact `ai[0]` float-to-handle projection at the legacy boundary;
- network packet encoding and client presentation cadence;
- loot, boss, interaction and global random side effects;
- complete worm spawn/segment construction.

This is a lifecycle follow-up child only. It does not establish complete NPC lifecycle parity.
