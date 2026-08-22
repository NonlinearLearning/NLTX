# Projectile Hostile-Player Boundary

The legacy projectile damage gate cannot be reduced to `hostile == true` in the current ECS.

Source: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`, SHA256
`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`, lines `11480-11535`.

`Damage_CanDealDamage` rejects a large set of projectile type, `aiStyle`, `ai` and `localAI`
states before `Damage_PVE`/`Damage_PVP`. The player branch additionally depends on PVP hostility,
owner checks, collision mode, immunity and type-specific effects. The current Simulation has no
complete projectile type/AI table or generic hostile-player collision owner.

Disposition: retain the accepted friendly-to-NPC gate, but do not add a hostile-to-player route
based solely on the `Hostile` flag. A future card requires the missing type/AI and player damage
contract.
