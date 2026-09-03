# Projectile penetration domain boundary

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`
  - SHA-256: `97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`
  - default penetration and type assignments: lines 156, 460-1700
  - generic decrement/stop branch: lines 12858-12874

## Accepted narrow predicate

`ProjectilePenetrationComponent` accepts positive penetration counts and the legacy `-1` infinite
sentinel. Zero and values below `-1` are rejected at the component owner, so composition or restore
routes cannot bypass the spawn validator with an invalid penetration domain.

## Deferred source branches

Projectile-specific immunity, `stopsDealingDamageAfterPenetrateHits`, bounce/transform behavior,
random penetration changes, type/AI tables and complete network/client cadence remain deferred.

## Verification

`Test/Terraria.Dome.Combat.Verification` covers invalid component construction, preservation of
the `-1` sentinel and the existing one-hit penetration/despawn route.
