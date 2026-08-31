# NPC Projectile AI Replication Design

## Decision

Keep `NpcProjectileReplication` V1 unchanged. Negotiate NPC projectile version 2 and
emit a separate `NpcProjectileReplicationV2` extension frame only to sessions that
advertise V2. V1 sessions continue receiving the existing typed NPC projectile frame.

## Data Flow

`NpcProjectileReplicationSystem` and `DomeSimulation` project the already-authoritative
`ProjectileBehaviorComponent` through `ProjectileBehaviorStateProjection`. V2 carries
the resulting `Ai0`, `Ai1`, and `Ai2` beside the V1 state, including tombstones.
The codec rejects non-finite AI values. No V1456 `SyncProjectile`, legacy short UUID,
or opaque behavior blob is changed.

## Compatibility

The existing V1 body length remains fixed. A V2-capable session is selected from the
NPC projectile version bitset; V1 remains the fallback. An unknown V2 frame is never
sent to a V1-only negotiated session.

## Focused Acceptance

- A V2 envelope round-trips active and tombstone AI state and rejects non-finite state.
- NPC lifecycle updates project Linear and Gravity state into the cached replication
  snapshot and preserve that state when the projectile terminates.
