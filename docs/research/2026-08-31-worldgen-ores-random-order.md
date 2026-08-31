# WorldGen OresAndShinies random order

The legacy OresAndShinies source selects a drunk-world ore tile (`Next(2)`) at
the start of each loop iteration, then draws X, Y, strength, and steps. The
owner selected the tile variant after those four draws, shifting the random
stream. `LegacyOresAndShiniesRecipe.CreateInvocation` now performs tile
selection first. A focused probe confirms five samples, valid tile variants,
and requested coordinate bounds; the Simulation Release build is warning-free.

Recipe ordering does not establish full ore generation parity. Scheduling,
state publication, traversal, aggregate ordering, and WLD parity remain open.
