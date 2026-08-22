# WLD Generator Version Boundary

The Terraria v179+ header stores `WorldGeneratorVersion` as an unsigned 64-bit value after
the seed representation. The source model uses it as immutable world identity metadata; it is
not a runtime random stream or a seed substitute. The v181+ UUID remains no-owner and is only
consumed for parser alignment.

Accepted scope:

- v179+ parser capture into nullable `LegacyWorldMetadata.WorldGeneratorVersion`;
- direct `LegacyWorldMetadata -> CompatibilityWorldMetadata -> WorldMetadata` projection;
- append-only Dome state format v29 tail field when the value is present;
- missing pre-v179 values remain `null` and are never fabricated.

Deferred scope:

- UUID ownership and protocol exposure;
- ore tiers, secret-seed repair and generator-specific world behavior;
- global `Main.rand` ordering or random stream derivation from this value.

Source oracle: `src/Terraria.WorldFile.V319/LegacyReference/WorldFile.cs`, v179 header read
around lines 221-239 and write around lines 1268-1271. The implementation source is
`src/Terraria.WorldFile.V319/Format/WldHeaderReader.cs`.
