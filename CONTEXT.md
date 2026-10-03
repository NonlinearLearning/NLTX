# NLTX World Storage Domain

This context defines the terms for persisted world data and its relationship to the live world state.

## World persistence

**World save**:
A versioned on-disk representation of a world, including world metadata, progress, tiles, and owned records.
_Avoid_: live world, runtime world

**Persisted world data**:
The validated values decoded from a world save and held while world owners prepare to load their state. It is an application-level persistence model, not the authoritative simulation state.
_Avoid_: ECS state, loaded world

**World-data section**:
A named group of persisted facts that one world owner can consume, such as environment, progression, tiles, or chests.
_Avoid_: arbitrary blob, global world object

**Runtime world state**:
The authoritative state used by the running simulation and maintained by the responsible domain owners.
_Avoid_: save document, persistence snapshot

**World-load owner**:
The domain owner responsible for interpreting, validating, and applying its own world-data sections to runtime state.
_Avoid_: coordinator-owned state
