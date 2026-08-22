# WLD Rain Runtime Boundary

## Source Audit

The WLD reader obtains `raining`, `rainTime` and `maxRaining` and then invokes
`FixEndlessRainWorlds()` (`WorldFile.cs:3685-3690`, `3365-3383`). The repair clears all
three values for worlds at version `<=317` with a long rain timer unless the active
secret-seed set contains `rainsForAYear`.

Legacy `Main.UpdateWeather` uses `maxRaining` in the wind target formula
`windSpeedTarget * (1f + 5f / 9f * maxRaining)` (`Main.cs:12462-12465`). `ChangeRain`
also generates and mutates that value (`Main.cs:13325-13337`), while the duration
decrements and random weather starts are handled in `UpdateTime` (`Main.cs:13431-13491`).

## Decision

Accept the direct, lossless WLD rain route. `CompatibilityToDomeProjection` now maps a complete
`raining`/`rainTime`/`maxRaining` triple into `WorldRuleState.IsRaining`, `RainTimeTicks`, and
`MaximumRainStrength`. `RainStrength` remains a separate runtime transition value, so the saved
maximum is not conflated with it. The existing version-21 persistence record preserves the two
raw facts that are distinct from the existing duration/runtime record.

Keep the source repair branch fail-closed. For WLD version `<= 317` with `rainTime >= 5184000`,
legacy `FixEndlessRainWorlds()` requires the active `rainsForAYear` secret-seed membership. That
membership is not in the Compatibility import contract, so the projection rejects the import
instead of either clearing all three fields or accepting a potentially corrupted state. A later
card may add a canonical, sourced membership owner and recover this branch atomically.

## Focused Evidence

Evidence: `Build/diagnostics/main-migration/task-9-wld-rain-runtime-boundary/20260822-003000/`.

The 2026-08-22 B-001 build-ownership repair removed the unrelated `WorldTile` compilation block.
The source anchors, RED/GREEN import evidence, persistence/WorldRules follow-up output and scoped
diff check are under
`Build/diagnostics/main-migration/task-9-wld-rain-runtime-boundary/20260822-151500/`.
