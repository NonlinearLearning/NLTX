# WorldGen Corruption and Crimson conversion boundary

The post-Underworld source pass scans each column from a rule-selected start Y
and chooses conversion type `1` for Corruption or `4` for Crimson, including
the Drunk-world side selection. The existing typed owners are
`LegacyWorldInfectionPolicy`, `LegacyWorldInfectionConversionCommandEmitter`,
and `LegacyWorldInfectionConversionCommitBoundary`.

`LegacyWorldInfectionConversionPass` now provides the explicit source-snapshot
to emitter-to-commit handoff without inventing biome authority in the main
pipeline.

A focused probe verifies bounded column command emission, both Corruption (`1`)
and Crimson (`4`) conversion selection, and the Skyblock no-op. The full biome placement loop, secret-seed
authority, conversion-rule coverage, framing/cleanup side effects, aggregate
ordering, and WLD parity remain deferred; `WorldGenerationRequest` does not
yet expose enough authority to integrate this pass safely.

The pipeline now invokes the pass only when `WorldGenParamEvil` is explicitly
set to `0` or `1`; the default `-1` path remains unchanged. A focused Crimson
pipeline generation probe passes. Drunk-world, secret-seed, biome placement,
and complete conversion parity remain deferred.
