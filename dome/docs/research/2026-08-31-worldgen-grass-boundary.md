# WorldGen Grass placement boundary

The instrumented legacy source at
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:12939+`
uses a placement loop sized by `width * height * 0.002`. Each iteration makes
two candidate draws. The first Y draw uses the half-open interval
`[worldSurfaceLow, worldSurfaceHigh)`; the second uses `[5, worldSurfaceLow)`.
The candidate X is drawn from the interior columns. The pass is skipped for
Skyblock worlds. A candidate becomes grass only when the center tile is empty
and its four cardinal neighbors are active type-0 soil.

`LegacyGrassPass` maps this contract to source-attributed `TileChangeCommand`
values and reserves deterministic sequence numbers through
`WorldGenerationStateComponent`. The focused scratch probe verifies the two Y
ranges; the Simulation Release build is warning-free. The profile-backed
`WorldGenerationPipeline` now also invokes the existing
`LegacyMudCavesToJungleGrassPass` owner after IceBiome. Its focused probe
verifies one type-59 to type-60 command and confirms the Skyblock guard leaves
the command list and sequence unchanged.

This does not establish full legacy behavior. The source also has world-shaping
side effects (`digExtraHoles`, `roundLandmasses`), grass spread/clump cleanup,
tile framing/liquid behavior, aggregate pass ordering, and WLD parity that are
not represented here. The boundary therefore remains `completed_partial`, and
legacy deletion is prohibited while those contracts and the 44 deferred
ServerRelevant rows remain open.
