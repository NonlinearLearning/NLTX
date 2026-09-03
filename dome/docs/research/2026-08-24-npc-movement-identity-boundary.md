# NPC Movement Identity Boundary

`NpcMovementIntentSystem.Apply` now fails closed for an entity that is not alive in the supplied
Arch world. The invalid handle returns before any component lookup or movement-intent mutation.
Live NPC entities retain the existing typed behavior evaluation and intent projection, including
the target-alive check already present in the route.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, SHA256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`, `NPC.AI()` entry around
line `18642`. The source performs AI work on an NPC instance; this card establishes the ECS
identity/liveness precondition without copying the legacy AI body.

Accepted: live Arch NPC identity, typed component reads and movement-intent projection. Rejected:
default or destroyed entity before component access. Deferred: complete AI families, tables,
collision/LOS, lifecycle and client/network effects.
