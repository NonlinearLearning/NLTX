# WLD Rain Raw Metadata Boundary

The parser now preserves the source WLD rain facts as nullable compatibility metadata when the
layout contains them: `IsRaining`, `RainTimeTicks`, and `MaximumRainStrength`. Versions before the
source field was introduced leave them `null`; v87 and v319 fixtures prove the field boundary and
values.

This is deliberately not yet a Simulation projection. The source stores `maxRaining`, while the
current ECS `RainStrength` is the runtime/current value consumed by wind smoothing. The source
shown here does not establish that those values are identical. Mapping max directly to current
would invent behavior. The metadata therefore stops at the compatibility boundary until a
source-backed runtime relationship is proved.

`FixEndlessRainWorlds()` remains separate: for versions `<=317` and a rain timer at least
`5184000`, it consults secret-seed membership before clearing the raw triple. The current
compatibility model does not carry that membership, so that repair branch remains deferred and is
not silently applied.

The accepted scope is lossless parser/metadata preservation only. Persistence and WorldData
projection of runtime rain remain governed by the existing weather-state and rain-import cards.
