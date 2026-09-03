# Projectile Replication Scalar Boundary

`ProjectileReplicationSystem.Project` now rejects non-positive replication IDs and negative
revisions before entity access or snapshot construction. This prevents an otherwise live entity
from producing an unaddressable or regressing replication record. Existing live projection fields
and entity-liveness checks remain unchanged.

Source context: `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`, inactive/update
guards around lines `14697`, `15249` and `18706`. The source keeps projectile identity and active
state as replication-visible facts; this card validates the ECS scalar contract without claiming
packet cadence parity.

Accepted: positive replication ID, non-negative revision and live entity. Rejected: invalid scalar
identity before projection. Deferred: packet cadence, type/AI tables, collision/network/client
branches.
