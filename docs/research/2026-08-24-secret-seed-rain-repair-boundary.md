# Secret-Seed Endless-Rain Repair Boundary

The source repair is deliberately fail-closed in the current import model.

`Terraria.IO.WorldFile.FixEndlessRainWorlds` (`WorldFile.cs:3365-3381`) returns without repair
only when the WLD version is greater than 317, the temporary saved rain duration is below
`5184000`, or one of the active seed codes matches `WorldGen.SecretSeed.rainsForAYear`. Otherwise
it clears all three temporary rain facts: `raining`, `rainTime`, and `maxRaining`.

The active seed-code input is not a standalone WLD header field. `WorldFileData.GetSecretSeedCodes`
(`WorldFileData.cs:200-207`) splits the original seed text on `|`; seed parsing and activation are
performed by `TryApplyingCopiedSeed` and `WorldGen.SecretSeed`. Raw seed text is now preserved by
the compatibility snapshot, but it still does not carry parsed secret-seed identities or the
source secret-seed registry. Therefore it cannot yet prove the exception branch.

The exact source comparison is also non-trivial: `WorldGen.SecretSeed.Check` normalizes the input,
calls `Terraria.Utilities.Secrets.ToSecret`, and compares the result with the registered encoded
code. `Secrets.ToSecret` (`Terraria.Utilities/Secrets.cs:7-31`) performs two
`BCrypt.Net.BCrypt.CryptRaw` operations with the fixed salt `fT2JQQzNMJl2NRoMbo9RjA==`, cost `4`,
and a 1000-iteration byte-swap loop between them. The current Simulation has no BCrypt dependency
or source-compatible crypto owner. A plain-text comparison or hand-rolled substitute would not
be a source-backed repair predicate.

Accepted boundary:

- modern direct rain triples remain lossless;
- an old-layout endless-rain triple without explicit secret-seed context is rejected/fails closed;
- callers may provide an explicit `isRainsForAYearSecretSeedActive` fact only where an upstream
  source-backed owner has established it.

Rejected boundary:

- treating the world numeric seed, generator version, or `IsRemixWorld` as secret-seed proof;
- clearing rain unconditionally for v317 and older imports;
- adding a new generic seed parser or secret-seed registry as part of the WLD rain card.

This card keeps the source repair predicate auditable without claiming complete secret-seed or
weather parity.
