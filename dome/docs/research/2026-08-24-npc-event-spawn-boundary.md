# NPC Event Spawn Boundary

`NpcEventSpawnTable` is the stable per-event definition owner, and `NpcEventSpawnSystem` is the
bounded bridge from `WorldProgressionState` to typed `SpawnNpcCommand` values. The table rejects
duplicate `(eventType, npcDefinitionId)` entries. The system accepts only a matching invasion type,
positive remaining budget, zero invasion delay, finite position, and at least one active player with
the source-backed health qualification. Every emitted command is marked `NpcSpawnSource.Event`;
NPC eligibility and commit remain the downstream owners of capacity, identity, and entity creation.

The NPC focused verifier covers stable table order, global budget, duplicate rejection, registry
validation, one accepted event request, mismatched event type, delay, and inactive-player rejection.
This completes the explicit table owner for currently supported event definitions. Legacy full
invasion tables, Boss/full AI, random event selection, progression mutation, and client
warning/presentation remain deferred.
