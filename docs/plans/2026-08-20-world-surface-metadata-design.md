# World Surface Metadata Design

## Decision

Preserve the V319 WLD `worldSurface` value as an optional `double? WorldSurface` through the
Compatibility projection, Dome Simulation metadata, embedded world persistence, and outer Dome
state persistence. A value is present only when an authoritative source supplied it. Newly
generated worlds and v1-v17 Dome snapshots use `null`, which means unknown; no height/spawn-based
fallback is permitted.

## Boundaries

`Terraria.WorldFile.V319` owns binary decoding, `Terraria.WorldCompatibility` owns import mapping,
`Terraria.Dome.Simulation` owns the immutable metadata contract, and
`Terraria.Dome.Server.Persistence` owns versioned serialization. This batch does not enable Type
226 actuator behavior. Type 226 remains fail-closed until the surface value and the remaining tile
definition authority are both available at the runtime call site.

## Compatibility

The embedded `WorldPersistenceFormat` receives a new version that stores the optional surface
after the existing world name and before tile payload. Its legacy reader keeps v1 and v2 readable,
returning `null` for v1/v2. `DomeStatePersistenceFormat` advances from v17 to v18 and appends the
surface after the complete existing progression record, preserving the v17 byte layout as an exact
prefix. Readers for v1-v17 do not consume that field and restore `null`; no existing field order is
reinterpreted.

## Verification

World import proves an exact fractional value survives WLD metadata -> Compatibility -> Dome.
Persistence proves v18 exact round-trip, embedded world round-trip, and a hand-built v17 payload
restoring unknown. Existing world import, persistence, wiring, boundary, and serial Release gates
must pass before this batch is accepted.
