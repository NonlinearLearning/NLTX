# Weather State Model Design

**Goal:** Make Simulation and persistence able to represent the source WLD weather facts without
claiming that WLD rain has already been imported.

`WorldFile.cs:2180-2183` loads independent `raining`, `rainTime` and `maxRaining` values before
calling `FixEndlessRainWorlds()`. The previous ECS model derived `IsRaining` solely from
`RainTimeTicks > 0` and used one `RainStrength` value for all rain intensity meanings. It therefore
could not carry a valid source distinction such as an inactive saved rain state with a retained
timer and a maximum rain strength separate from current strength.

The accepted model adds two immutable `WorldRuleState` facts:

- `IsRaining`: the raw activity Boolean. When omitted by an old in-process caller it preserves
  the old derivation from `RainTimeTicks > 0`.
- `MaximumRainStrength`: independent raw maximum strength. When omitted it preserves the old
  derivation from `RainStrength`.

`WithRain` remains the runtime mutation route and intentionally produces the existing canonical
active/clear state. `WithRawRain` is a recovery-only state transformation that preserves the raw
facts without giving a protocol handler direct state mutation access.

`DomeStatePersistenceFormat` v21 appends the two fields after the v20 GameMode tail. Readers for
v1-v20 do not consume new bytes and reconstruct the old derived state. The v21 verifier proves
lossless round-trip of `raining=false`, `rainTime=120`, `rainStrength=0.25`,
`maxRaining=0.75`; the v20 fixture proves its historical derived recovery remains active with a
maximum equal to its recorded strength.

This representation is deliberately not a WLD import. The next card must prove WLD source offsets,
version conditions and every applicable `FixEndlessRainWorlds()` repair branch before assigning raw
facts through `LegacyWorldMetadata`, compatibility projection and the authoritative snapshot.
