# NPC death input boundary

## Source and owner

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`
  - SHA-256: `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`
  - `checkDead` entry: lines 64571-64754
- ECS owner: `src/Terraria.Dome.Simulation/Npc/Systems/NpcDeathSystem.cs`

## Accepted narrow predicate

Death publication requires a valid positive NPC identity, finite position, positive loot-table ID,
an active source state and non-positive health. Invalid identity, geometry or loot facts fail closed
before a typed death event can reach loot commit.

## Deferred source branches

Full `checkDead` death reasons, type-specific drops, bosses, achievements, network synchronization,
random effects and client presentation remain deferred.

## Verification

`Test/Terraria.Dome.Npc.Verification` covers valid death publication and rejection of invalid handle,
non-finite position and missing loot-table identity.
