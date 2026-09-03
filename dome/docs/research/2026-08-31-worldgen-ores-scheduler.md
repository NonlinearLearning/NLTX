# WorldGen OresAndShinies scheduler boundary

The source OresAndShinies pass uses separate Remix and non-Remix recipe catalogs.
Each recipe contributes `floor(width * height * density)` invocations, draws a
factor-derived Y range, and creates an OreRunner request. The new
`LegacyOresAndShiniesPass` selects the catalog, computes bounded coordinates,
creates requests through `LegacyOresAndShiniesRecipe`, and emits typed commands
through `LegacyOreRunner`. The Skyblock guard returns before consuming state.

A focused probe verifies non-empty scheduler output, source attribution, command
sequence accounting, and the Skyblock no-op. The profile-backed pipeline now
traverses and commits these requests through the typed OreRunner command
boundary. Ore-tier publication, full traversal, aggregate ordering, and WLD
parity remain open.
