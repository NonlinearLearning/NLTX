# Projectile Motion Input Boundary

The supported linear and gravity projectile behaviors now fail closed before mutating motion state
when transform coordinates or velocity components are non-finite, or when the tick is negative.
Valid inputs retain the existing movement equations and deterministic phase update. A rejected
advance leaves the caller-owned components unchanged.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`, SHA256
`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`, movement branches around
lines `16030-16060` and gravity updates around `16151-16166`. The source performs motion in the
projectile update phase; this card protects the ECS equivalent from forged numeric state without
claiming type-specific AI parity.

Accepted: finite transform/velocity, non-negative tick, linear movement, gravity movement and
deterministic behavior phase. Deferred: complete projectile AI/type tables, collision response,
reflection/bounce, random motion, lifetime/network cadence and client presentation.
