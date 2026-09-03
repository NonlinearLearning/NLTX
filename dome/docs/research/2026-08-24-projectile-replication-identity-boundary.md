# Projectile Replication Identity Boundary

`ProjectileReplicationSystem.Project` now rejects an entity that is absent or destroyed in the
supplied Arch world before reading projectile components. The rejection is an explicit
`ArgumentException` at the replication authority boundary, rather than an incidental component
lookup failure. Live projectile projection remains field-for-field unchanged.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`, SHA256
`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`, inactive and update guards
around lines `14697`, `15249` and `18706`. This card covers identity precondition only; it does not
claim packet cadence or complete projectile replication parity.

Accepted: live entity and existing typed snapshot projection. Rejected: default, absent or
destroyed entity before component access. Deferred: replication cadence, type/AI tables, collision,
network and client presentation.
