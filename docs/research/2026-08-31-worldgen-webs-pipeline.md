# WorldGen Webs pipeline boundary

`WorldGenerationPipeline` now schedules Webs after Ores using the terrain
profile's `WorldSurface`, `WorldSurfaceLow`, and world-height bounds. Candidate
selection and TileRunner traversal share the same `LegacyPassRandomState`, and
typed tile commands are committed through the existing TileRunner command
boundary. The integration keeps the Skyblock no-op.

MountainCaves now publishes accepted cave coordinates through the typed
`LegacyCaveCoordinate` history boundary, and the integrated Webs path consumes
the indexed coordinates after preserving its initial random X/Y draws. A focused
probe verifies the override and candidate recovery contract. Exact
`GenVars.mCaveX/mCaveY` traversal/RNG behavior, aggregate ordering, WLD parity,
and legacy deletion remain deferred.
