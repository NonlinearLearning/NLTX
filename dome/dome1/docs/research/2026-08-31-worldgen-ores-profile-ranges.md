# WorldGen OresAndShinies profile ranges

The legacy OresAndShinies source draws its main bands from terrain state rather
than total-height fractions. Surface bands use `worldSurfaceLow` to
`worldSurfaceHigh`; middle bands use `worldSurfaceHigh` to `rockLayerHigh`; and
deep bands use `rockLayerLow` to world height. `LegacyOresAndShiniesPass` now
accepts `LegacyTerrainRuntimeProfile` and resolves these ranges before creating
OreRunner requests. It also models the source special cases: non-Remix
silver/gold surface bands use `0..worldSurfaceLow`, and Remix silver-deep uses
`rockLayer - 100..height - 250`. The existing generic API remains for
profile-less callers.

A focused probe verifies the profile-derived surface range, special ranges, and
source command attribution after a Simulation Release build. Ore-tier state,
full traversal, aggregate ordering, and WLD parity remain open.
