# Chest Mutation Design

## Scope

This design completes the world-chest placement and destruction slice from the Wiring, Liquid, and
Chest ECS migration proposal. It covers only `Chest.cs` behaviors that have an authoritative
world-chest counterpart: coordinate uniqueness, empty-chest destruction, and persistence restore
validation. Bank and shop semantics remain excluded.

## Decision

Destroying an opened chest is rejected. A destruction request succeeds only when the chest exists,
has no opener, and all 40 slots are empty. This keeps opener ownership, session teardown, and
replication cursors server-owned without inventing a client force-close protocol.

## Architecture

`ChestIndexSystem` owns the `(TileX, TileY) -> ChestId` index. It rejects duplicate coordinates
both during normal mutation and persistence recovery. `ChestMutationCommitSystem` accepts typed
create/destroy commands and applies them atomically to the chest dictionary and index after
validating the command sequence, bounds, occupancy, and opener state.

`DomeSimulation` remains the sole mutation boundary. Its existing `CreateChest` API delegates to
the typed create command and throws for invalid placement. A new `TryDestroyChest` API consumes a
typed destroy command and returns false for unknown, nonempty, or opened chests. No protocol frame
is added: no current client input calls the destroy path, and no session can observe a deletion of
an opened chest.

## Persistence

During restore, a candidate `ChestComponent` must enter both the ID dictionary and coordinate index
or neither. Duplicate IDs and duplicate coordinates are rejected before a restored simulation is
published. The index itself is derived state and is not serialized.

## Evidence

The WorldObjects verifier will cover duplicate-coordinate rejection, destroy refusal for occupied
and opened chests, successful removal of a closed empty chest, and coordinate reuse after removal.
The persistence verifier will cover duplicate-coordinate recovery rejection. Existing two-session
WorldObjects loopback remains the regression gate for opener/PVS behavior.
