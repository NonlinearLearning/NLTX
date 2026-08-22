# Wiring, Chest, and TileObject Liquid Design

## Status

Approved behavior families: Wiring lights, Chest locks and keys, and Liquid object rules.

## Wiring Light Tiles

### Decision

Represent source-derived light families as immutable definitions. A definition identifies a
tile type, footprint dimensions, state predicate, and frame offset between unlit and lit
states. A wire hit derives the object's top-left coordinate from the hit tile's frame,
updates every footprint tile in row-major order, and emits deferred `TileFrameCommand`
instances.

### Included Families

The first registry covers the source methods `ToggleHolidayLight`, `ToggleHangingLantern`,
`Toggle2x2Light`, `ToggleLampPost`, `ToggleTorch`, `ToggleCandle`, `ToggleLamp`,
`ToggleChandelier`, `ToggleCampFire`, and `ToggleFirePlace`.

### Boundaries

The system changes frames only. It neither changes tile type nor directly mutates the
world grid. Client light calculation, sound, temporary animation, wire skip bookkeeping,
and network broadcasts remain outside this simulation slice.

## Chest Locks and Keys

### Decision

Chest lock state is server-owned. An immutable lock definition identifies an accepted key
item and whether using it consumes one item. An open request validates sequence, range,
opener ownership, the lock policy, and the authoritative player inventory before it changes
anything. A successful locked open consumes the key, unlocks the chest, opens it, and
increments the chest revision as one operation.

### Boundaries

The initial policy supports world chest locks with explicit Gold Key and Shadow Key item
identifiers. It does not derive lock status from legacy frame ranges, model boss-progression
gates, or reproduce visual frame changes. Chest snapshots and persistence must carry
`IsLocked`; an unavailable or invalid key rejects the request without mutation.

## TileObject Liquid Rules

### Decision

The existing 753-ID global liquid-death tables stay the default. A separate immutable
object-rule registry may override the result for a type and its frame/style range. Rules
resolve a top-left coordinate plus a bounded footprint, then create row-major deferred Kill
commands with `PreserveLiquid=true`.

### Boundaries

An object rule that exists but has no matching frame/style is fail-closed: it does not use
the global table and does not destroy the tile. Rules do not directly write the grid, drops,
audio, effects, network messages, arbitrary TileObjectData alternates, or dynamic sub-tile
registration. A later slice may add source-evidenced alternates without changing the
global-table fallback contract.

## Verification

Focused verifiers must prove all successful mutation paths and the no-mutation rejection
paths. Existing build and loopback verifiers remain the integration gate. The proposal stays
partial: these changes do not claim full legacy object, lighting, permissions, or deletion
parity.
