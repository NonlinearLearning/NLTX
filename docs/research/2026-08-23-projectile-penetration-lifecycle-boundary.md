# Projectile penetration lifecycle boundary

## Source oracle

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`
  - SHA-256: `97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`
  - generic hit penetration decrement/stop branch: lines 12858-12874
- Current Simulation route:
  - `ProjectileDamageSystem.Resolve` decrements positive remaining penetration;
  - `DomeSimulation` queues `DespawnEntityCommand` when it reaches zero;
  - replication exposes inactive/revision state, not internal penetration.

## Accepted predicate

For a typed projectile with `MaximumPenetration: 1`, one authoritative NPC hit applies exactly one
damage event and the projectile becomes inactive in the following committed snapshot. The test
does not require penetration to be a protocol field; the current snapshot contract intentionally
exposes lifecycle visibility rather than every internal combat component.

## Deferred branches

Local/static NPC immunity, projectile-specific `stopsDealingDamageAfterPenetrateHits`, special
type/AI branches, random penetration changes, bounce/transform behavior and full client/network
cadence remain deferred. This child does not claim complete Projectile parity.
