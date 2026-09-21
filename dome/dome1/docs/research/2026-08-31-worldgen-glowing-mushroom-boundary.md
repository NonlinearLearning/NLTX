# WorldGen GlowingMushroom lower-bound boundary

The legacy Remix loop begins at `GenVars.remixMushroomLayerLow + Next(3)` and
continues while `j < Main.maxTilesY - 10`. The prior owner used a forced
`minimumY + 1` maximum, which could process a row when the source loop should
be empty. `LegacyGlowingMushroomPass` now returns immediately when the layer is
at or below the source upper bound and skips per-column starts that land on the
exclusive bound.

A focused probe with valid Terraria section dimensions verifies the no-row,
no-sequence-consumption edge. This remains a narrow loop-bound correction, not
full GlowingMushroomPatches parity.
