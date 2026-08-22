# WLD UUID Identity Boundary

The v181+ WLD header contains a 16-byte `UniqueId` immediately after the generator-version
field. The legacy source reads it into `WorldFileData.UniqueId` and writes it back as part of the
header; protocol `WorldData` also emits the same UUID. It is therefore a server-visible immutable
world identity fact, not merely presentation state.

Accepted scope:

- v181+ parser capture into nullable `LegacyWorldMetadata.UniqueId`;
- direct compatibility projection into immutable `WorldMetadata.UniqueId`;
- protocol `LegacyWorldDataContext.FromWorldMetadata` projection;
- append-only Dome format v30 UUID tail, only when a UUID is present;
- v180 and older layouts remain `null` without fabrication.

Compatibility details:

- v29 generator-version persistence remains readable;
- v30 reads the generator marker/value and then an optional UUID marker/value;
- snapshots without a UUID emit no UUID tail, preserving existing old-format fixture layouts;
- current-format trailing bytes remain rejected.

Deferred scope:

- UUID-derived map filename policy and client map storage;
- UUID generation for worlds that did not contain a source UUID;
- ore tiers, secret-seed repair and generator-specific behavior.
