# Main ECS migration M-002 entity ownership initialization boundary

## Narrow proposition

The server-owned entity families use domain stores and Arch entities behind stable value identities,
rather than legacy `Main.player[]`, `Main.npc[]`, `Main.projectile[]` or `Main.item[]` arrays.
Player handles, NPC handles, projectile replication identities and world-item replication IDs remain
separate from Arch entities. Player destruction retains the required inactive snapshot/tombstone
behavior, and duplicate or repeated store removal is rejected or idempotent as appropriate.

## Source boundary

Version4 `Main.Initialize_Entities` (`Main.cs:3860-3913`) allocates menu scales, dust, world-item,
NPC, player, projectile, gore, rain, cloud, combat-text and popup-text arrays. Only the server-owned
entity families are in this card. Dust, rain, cloud, gore, combat-text, popup-text and client/menu
objects remain outside the Simulation authority boundary.

## Owner and verification

Owners are the domain-specific `PlayerStore`, `NpcStore`, `ProjectileStore`, `WorldItemStore` and
their typed Simulation snapshots. Focused checks are `PlayerOwnership.Verification` for store,
lifecycle, tombstone and snapshot behavior, and `Npc.Boundary.Verification` for the Simulation's
legacy-NPC dependency boundary.

## Status

Accepted narrowly. Full entity lifecycle behavior, AI families, projectile motion/collision, world
item interaction parity and all client-only arrays remain separate cards.
