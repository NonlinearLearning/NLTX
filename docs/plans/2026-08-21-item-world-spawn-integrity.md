# World Item Spawn Integrity Slice

`WorldItemSpawnSystem` now rejects non-finite X/Y positions before allocating a replication ID or
active world-item component. The source oracle is the server-owned `Terraria/WorldItem.cs`; this
card covers only numeric input integrity at the world-item creation boundary. Stack validation,
pickup delay, ownership, stacking, pickup and persistence remain separate contracts.

The focused Items verifier proves a `NaN` position is rejected. No broad loopback, MainBoundary or
root solution validation is included in this card.
