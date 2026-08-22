# Projectile Spawn Input Boundary

`ProjectileSpawnSystem.Spawn` now validates direct-call inputs before Arch allocation. It rejects
invalid owner handles, non-positive identity/lifetime, invalid penetration values (`0` and values
below `-1`), non-finite position/initial velocity/speed and mismatched projectile type. The direct
route therefore cannot bypass the higher-level simulation command validation. Valid definitions
retain the existing typed component composition and motion defaults.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`, SHA256
`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`, owner/default and lifetime
fields around lines `126-138`, `535-536`, plus `NewProjectile` call sites. This card covers direct
input integrity only, not complete source type/AI initialization.

Accepted: valid owner, positive identity/lifetime, supported penetration domain, finite numeric
inputs and matching type. Rejected: forged owner/identity, invalid domain or non-finite numeric
input before entity creation. Deferred: complete type/AI defaults, owner routing, network cadence
and client presentation.
