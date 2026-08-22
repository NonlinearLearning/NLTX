# Projectile System Identity Boundary

The direct projectile behavior and lifetime owners now fail closed for an entity that is absent or
destroyed in the supplied Arch world. `TryAdvance` returns `false` with behavior id `0`, and
`Advance` returns `false`; neither performs component lookup or mutation. Live projectile entities
retain the existing typed behavior and decrement semantics.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`, SHA256
`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`, inactive/lifetime/AI
guards around lines `14697`, `15232-15249` and `18706`. This card establishes the ECS entity
precondition without claiming complete projectile AI or lifecycle parity.

Accepted: live entity, typed component access, behavior advance and lifetime decrement. Rejected:
default, absent or destroyed entity before component access. Deferred: complete type/AI tables,
collision, reflection, network cadence and client presentation.
