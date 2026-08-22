# WLD Wind Import Design

## Source Decision

The legacy WLD header has two distinct branches:

- `version >= 62`: reads `numClouds` as `Int16`, then reads `Main.windSpeedTarget` as `Single` and
  sets `Main.windSpeedCurrent = Main.windSpeedTarget` (`WorldFile.cs:3762-3767`; modern header
  load `2198-2200`).
- `version < 62`: calls `WorldGen.RandomizeWeather()` (`WorldFile.cs:3768-3770`). That method
  selects cloud count from `genRand.Next(10, 200)`, repeatedly selects a nonzero signed wind
  value from `genRand.NextFloat() * 0.35f * (genRand.Next(2) * 2 - 1)`, sets target equal to
  current, and resets clouds (`WorldGen.cs:7229-7241`).

The modern value is a direct saved fact. The old result is generated state whose exact value also
depends on the legacy `genRand` stream and call context; it is not reconstructed by casting the
world seed or by using the new event stream.

## Accepted Scope

For `version >= 62`, `LegacyWorldMetadata.WindSpeedTarget` and
`CompatibilityWorldMetadata.WindSpeedTarget` carry the exact saved `Single`. Projection sets both
authoritative `WorldRuleState.WindSpeedTarget` and `WindSpeedCurrent` to that source value, matching
the legacy load relation. Existing `WorldRuleState` validation rejects non-finite or out-of-range
values rather than normalizing them.

For `version < 62`, the field remains `null`. No wind import is claimed for that branch.

## Verification

- Parser fixture proves v61 has no wind field and v319 restores `-0.35f` exactly.
- Compatibility projection proves modern target/current equality.
- Historical parser matrix and recorded oracle pass.
- WorldImport and Compatibility projection verifiers pass.

The persistence and server gates must be rerun after this source/projection change before acceptance.

## Deferred

- Exact v1-v61 `RandomizeWeather()` replay.
- Cloud count and client cloud presentation.
- Any weather scheduling or random event behavior.
