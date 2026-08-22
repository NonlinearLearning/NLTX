# Legacy Oracle Contract

The source files under `src/Terraria.WorldFile.V319/LegacyReference` are isolated
format evidence only. They are excluded from all runtime `Compile` items.

`RecordedLegacyOracleSnapshots.json` is a normalized oracle artifact tied to the
legacy source hash in `SourceManifest.md`. The verification project loads it as
copied content under `Oracle/`; `Terraria.WorldFile.V319.csproj` neither includes
it nor references the verification project. The cases cover the v87 legacy path, v88
pointer-table records, and v319 tail records. Each case compares metadata, every
normalized tile field, chests and item slots, signs, NPCs, tile entities, and footer
identity.

`ExpectedVersionLayouts.cs` is a fixed source-reviewed ledger containing one row per
version 1 through 319. It records the dispatch path, expected section count, and
each layout-transition version. Matrix fixtures parse every version, including every
transition fixture and every stable-range representative.
