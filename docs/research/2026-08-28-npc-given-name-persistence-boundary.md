# NPC GivenName Persistence Boundary

## Evidence

- `DomeStatePersistenceFormat.WriteNpcs` serializes the replication identity, position, health,
  revision, definition fields, behavior fields, faction, and category from `NpcReplicationSnapshot`.
- `DomeStatePersistenceFormat.Read` reconstructs `NpcReplicationSnapshot[]`; the V36 format has no
  `NpcStateSnapshot` segment and no text field associated with an NPC replication identity.
- `NpcGivenNameComponent` is an authoritative Simulation component and `NpcStateSnapshot` now carries
  its value for in-memory create/restore, while `NpcReplicationSnapshot` intentionally excludes it.

## Implemented boundary

`NpcGivenNamePersistenceFormat` V1 now provides a separate bounded sidecar format for
`(ReplicationId, GivenName)` entries. It validates count, name length, positive and unique IDs,
and rejects trailing bytes. The format is intentionally not part of SyncNPC or the V36 main file.

## Coordinator boundary

`NpcGivenNameSaveCoordinator` now provides atomic temporary-file replacement/recovery and joins
sidecar entries to authoritative `NpcStateSnapshot` IDs. Unknown sidecar IDs are ignored during the
join, while duplicate authoritative IDs or invalid entries are rejected.

`DomeServer` now calls the coordinator during pre-start configuration and disposal. The configured
sidecar is joined only against NPCs already present in the restored Simulation snapshot; dynamic spawn
inheritance is intentionally not inferred from reused replication IDs.

## Decision

Keep main-file integration deferred until the sidecar path is part of the broader persistence
configuration contract. Do not add `GivenName` to `NpcReplicationSnapshot` or the
SyncNPC codec: that would cross the protocol boundary and change the established V36 layout.

## Current status

The field migration is partial: null normalization, typed ownership, and in-memory state restore are
verified; disk persistence, localization, network text, and full dialogue remain deferred.
