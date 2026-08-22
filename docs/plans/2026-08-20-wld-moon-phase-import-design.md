# WLD Moon Phase Import Design

**Goal:** Preserve the source-owned moon phase when a supported legacy WLD becomes a
Dome simulation snapshot.

**Source contract:** `Terraria.IO.WorldFile` writes `_tempMoonPhase` as an `Int32`
immediately after saved time and day-time state, then reads it at the same position. The
current V319 reader consumes those bytes but discards their value.

**Selected design:** Read the integer in both supported Header readers, reject values
outside `0..7`, and carry the validated `byte` through `LegacyWorldMetadata` and
`CompatibilityWorldMetadata`. `CompatibilityToDomeProjection` creates the existing
`WorldClockSnapshot` with that value. Existing server persistence and WorldData projection
already consume the snapshot-owned phase.

**Alternatives rejected:** Leaving the phase at zero loses an authoritative saved value.
Deriving a phase from seed or time invents behavior. Keeping an unchecked integer until the
Simulation boundary risks a malformed WLD producing a partially accepted import.

**Scope:** This batch does not import legacy time-of-day, blood moon, eclipse, moon type,
visual rendering, or event eligibility.
