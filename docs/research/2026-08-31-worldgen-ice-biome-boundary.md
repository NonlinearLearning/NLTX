# WorldGen IceBiome Vertical Boundary

**Date:** 2026-08-31  
**Flowstate:** `N6` / `in_progress_with_deferred_findings`  
**Status:** `completed_partial`

Legacy `WorldGen.cs:12828+` sets `num2 = lavaLine` (or `maxTilesY - 250` in Remix) and then
sets `num = num2 - Next(160,200)`. The main loop is inclusive through `num2 - 140`; after
`num`, the lower-band height random walk is updated once per row.

`LegacyIceBiomeSurfacePass` now accepts the metadata Remix flag, computes these bounds through a
`CalculateBottom` and `CalculateConversionBottom` methods, uses the inclusive loop endpoint, and
tracks optional snow origins with previous-row averaging, and updates the lower-band height once
per row. The deterministic probe uses seed `451` and proves
both random branches; Simulation Release build exits `0` with no warnings or errors.

This does not claim full IceBiome parity. Real `GenVars.snowOrigin*` initialization, dungeon-side
drift, complete lower-band vertical mutation and tile/frame state, aggregate WorldGen,
WLD parity, and legacy deletion remain deferred.
