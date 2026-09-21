# Main Liquid Registry Boundary

`TileObjectLiquidRuleRegistry` now materializes rules once, preserves source order through
`Rules`, and stores each tile-type group as an immutable list behind a read-only dictionary.
`LiquidRuleRegistry` now wraps its definition and merge maps in read-only dictionaries and exposes
the deterministic Water/Lava/Honey/Shimmer order.

Evidence:

- WorldObjects verifier: exit `0`
- Simulation Release build: exit `0`, `0` warnings, `0` errors
- `git diff --check`: exit `0`
- Liquid verifier: final rerun exit `0`; the initial fixture failure was resolved by making the
  `FixtureNpcType` definition explicitly town faction/category compliant
- Evidence: `Build/diagnostics/main-tick/task-2-liquid-registries/20260824-160000/`

Full legacy Liquid static parity and client/environment presentation remain deferred.
