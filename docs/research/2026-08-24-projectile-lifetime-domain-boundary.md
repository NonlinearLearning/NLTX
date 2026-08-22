# Projectile lifetime domain boundary

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`
  - SHA-256: `97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`
  - active projectile `timeLeft` field and defaults: lines 138, 460-536
  - generic decrement/expiry branch: lines 15232-15233

## Accepted narrow predicate

`ProjectileLifetimeComponent` accepts only positive remaining ticks. Zero and negative values are
rejected before an active projectile component can be composed outside the spawn validator. The
existing lifetime system still expires a projectile at the first tick where the decremented value
reaches zero.

## Deferred source branches

Type-specific lifetime assignments, random lifetime changes, transform/bounce behavior, full AI
tables, network cadence and client presentation remain deferred.

## Verification

`Test/Terraria.Dome.Combat.Verification` covers zero/negative component rejection and the existing
one-tick expiry replication boundary.
