# Projectile Friendly Target Boundary

Legacy projectile state has independent `friendly` and `hostile` flags. A projectile that is not
friendly must not enter the generic NPC damage route; hostile/player damage is a separate branch.

Source: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`, SHA256
`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`, declarations at lines
`148-154` and initialization/type branches beginning at lines `513-537`.

The ECS route is `ProjectileTargetEligibilitySystem.CanDamageNpc`, consumed before
`DomeSimulation` creates NPC damage candidates. This accepts only the generic friendly-to-NPC
gate. Hostile-to-player damage, reflected projectiles, type/AI branches and friendly+hostile
special cases remain separate and deferred.
