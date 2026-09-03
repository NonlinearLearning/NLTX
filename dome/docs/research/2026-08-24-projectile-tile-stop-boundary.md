# Projectile tile-stop lifecycle boundary

## Source oracle

- Path: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`
- SHA-256: `97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`
- Generic movement collision call: lines `15635-15704`

The source routes tile-collidable projectiles through `Collision.TileCollision`, with additional
branches for AI style, wet/shimmer/honey state, slopes and specific tile sets. Those branches are
not a single portable projectile type contract.

## Accepted narrow route

The current server path samples a swept segment from the previous to current transform and, when a
solid authoritative tile is encountered, queues a typed entity despawn before NPC damage
resolution. The focused fixture places a solid tile in front of a projectile and proves the
projectile becomes inactive while the NPC behind the tile remains at full health.

This accepts only the server safety invariant “a tile-collidable projectile cannot damage through
an authoritative solid tile.”

## Deferred

- exact `Collision.TileCollision` velocity response and bounce;
- wet, shimmer, honey and slope behavior;
- AI-style and projectile-type branches;
- actuated/door/special tile rules;
- penetration, immunity and client/network presentation parity.

No projectile persistence field is added: the current WLD/persistence contract does not establish
that active projectiles survive a world save, so volatile projectile state remains outside that
boundary.
