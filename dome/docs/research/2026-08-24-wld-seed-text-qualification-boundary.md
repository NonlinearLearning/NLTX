# WLD Seed-Text Qualification Boundary

The v179+ WLD header carries a textual seed input that is distinct from the numeric
`WorldGeneratorVersion` and UUID fields.

Source facts from `Terraria.IO.WorldFile`:

- v179 reads an `Int32` and converts it to text; v180+ reads a serialized string directly
  (`WorldFile.cs:221-227`).
- The source passes that value through `WorldFileData.SetSeed`, which later splits the original
  seed text on `|` in `WorldFileData.GetSecretSeedCodes` (`WorldFileData.cs:200-207`).
- `FixEndlessRainWorlds` then checks those parsed codes against the source
  `rainsForAYear` registry (`WorldFile.cs:3365-3381`).
- Pre-v179 layouts explicitly use an empty seed (`WorldFile.cs:228-232`).

The raw seed text now has a nullable immutable owner in legacy metadata, compatibility metadata
and `WorldMetadata`, with an append-only v31 persistence tail. This card deliberately does not
parse secret seeds or make the repair predicate authoritative. The existing protocol projection
does not emit raw seed text.

Disposition: raw seed text `accepted`; secret-seed identity and repair remain `blocked` until a
separate contract card defines canonical normalization, validation, privacy/protocol policy, and
the source registry needed by repair consumers. No seed text is fabricated or used to infer
secret-seed state.
