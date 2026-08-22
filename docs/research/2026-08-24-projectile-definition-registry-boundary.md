# Projectile Definition Registry Boundary

`ProjectileDefinitionRegistry` now validates each definition before registration. Supported
definitions require positive projectile type and behavior IDs, non-negative damage, positive
lifetime, finite positive collider dimensions and the penetration domain `positive or -1`. Invalid
static table entries are rejected at the definition owner instead of failing later during spawn or
motion.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`, SHA256
`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`, projectile identity/lifetime
fields around lines `118`, `138` and default initialization near `460-536`. This is a supported
definition subset, not a complete legacy projectile table.

Accepted: validated immutable definitions and unique types. Rejected: invalid scalar, geometry,
lifetime, behavior or penetration facts before registry ownership. Deferred: complete type/AI
defaults, reflected/hostile branches and client/network tables.
