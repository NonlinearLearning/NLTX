# NPC Encoded Target Routing Boundary

The ECS now owns the recoverable input boundary for legacy NPC-encoded targets. An encoded NPC
target is accepted only when the NPC definition supports NPC targets, the configured maximum NPC
count is positive, and `300 <= target < 300 + maximumNpcCount`. The translated index is
`target - 300`; resolution then requires a matching active candidate with a non-default entity.

Source: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, SHA256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`, lines `6516-6558`.
The route is implemented by `NpcTargetRoutingSystem` and does not infer target ownership from an
array slot or fabricate a missing NPC.

Focused coverage rejects the player-target range, out-of-range encoded values, unsupported target
families, default entity identities and inactive NPC candidates. This card accepts only the encoded
range/translation/active-identity boundary.

Deferred: `NPCID.Sets.UsesNewTargeting` type-table ownership, target acquisition and AI-style target
changes, line-of-sight, collision, replication, complete NPC lifecycle and client/presentation
branches.
