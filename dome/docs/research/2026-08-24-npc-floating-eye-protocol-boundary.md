# NPC FloatingEye protocol boundary

## Scope

This slice projects the typed `NpcFlyingState` used by the Dome simulation into
the four sparse AI fields available on the v1.4.5 `SyncNPC` packet.

The mapping is deliberately stable and lossless for the current movement
contract:

| SyncNPC field | Simulation value |
| --- | --- |
| `Ai0` | `HorizontalAcceleration` |
| `Ai1` | `VerticalAcceleration` |
| `Ai2` | `MaximumHorizontalSpeed` |
| `Ai3` | `MaximumVerticalSpeed` |

Only finite, positive movement parameters are projected. Invalid values are
reported as unsupported instead of being silently converted to wire defaults.

The same four typed values are retained in `NpcReplicationSnapshot`, so a
replication-only persistence restore does not silently reset FloatingEye to
default movement parameters before the next SyncNPC projection.

Replication snapshots also retain `NpcFaction` and `NpcCategory`. These are
simulation authority fields rather than SyncNPC wire fields, but losing them
on restore would incorrectly reclassify town NPCs as hostile enemies.

`FromReplication` rejects undefined behavior, spawn-source, despawn-reason,
faction, and category enum values before creating an ECS entity.

## Boundary

This is a protocol projection, not a claim of legacy `AI_002_FloatingEye`
parity. It does not add ranged attack state, projectile identity, target
validation, or server-side behavior execution to the wire contract. Those remain
separate simulation and Combat/Projectile work items.

## Evidence

The focused protocol verifier checks support, all four sparse AI values,
`SyncNPC` encode/decode round-trip, and rejection of a non-finite acceleration.
The source behavior anchor is `NPC.cs` `AI_002_FloatingEye` (around lines
42409-42620 in the migration source manifest).
