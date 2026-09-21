# P17 World Terrain, Biomes, and GenVars: Component Design

partitionId: P17
sessionId: e588106a6053430ca46c41cb07e8e4ca
claimMode: manual
ledgerStatus: completed
executionScope: component-only
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P17-World-Terrain-Biomes-GenVars.md
designPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md
evidenceStatus: partial
verificationRecordNote: The long semicolon-separated verification history below includes older checkpoint labels. The authoritative current result is `terminalVerificationStatus`: the fresh C12 and WorldSession focused build/run commands exited 0 with 0 warnings and 0 errors.
verificationStatus: focused-c01-c02-state-passed; c02-calculator-owner-production-build-passed-before-current-source-continuation; c02-calculator-owner-current-isolated-build-passed; c02-calculator-owner-current-isolated-run-passed; c03-spawn-material-liquid-focused-build-passed; c03-spawn-material-liquid-focused-run-passed; c04-boundary-focused-build-passed; c04-boundary-focused-run-passed; c04-beach-calculator-focused-build-passed; c04-beach-calculator-focused-run-passed; c05-state-query-focused-build-passed; c05-state-query-focused-run-passed; c05-treasure-overflow-reset-focused-build-passed; c05-treasure-overflow-reset-focused-run-passed; c05-constraint-calculator-focused-build-passed; c05-constraint-calculator-focused-run-passed; c05-treasure-placement-projection-focused-build-passed; c05-treasure-placement-projection-focused-run-passed; c06-state-query-commit-focused-build-passed; c06-state-query-commit-focused-run-passed; c06-underground-desert-reset-red-build-passed; c06-underground-desert-reset-green-build-passed; c06-underground-desert-reset-focused-run-passed; c07-state-query-focused-passed; c07-region-pyramid-chest-commit-focused-passed; c07-jungle-hut-material-focused-build-passed; c07-jungle-hut-material-focused-run-passed; c07-jungle-hut-selection-focused-build-passed; c07-jungle-hut-selection-focused-run-passed; c07-jungle-region-focused-build-passed; c07-jungle-region-focused-run-passed; c07-jungle-loot-focused-build-passed; c07-jungle-loot-focused-run-passed; c07-jungle-bounds-calculation-focused-build-passed; c07-jungle-bounds-calculation-focused-run-passed; c08-focused-build-passed; c08-focused-run-passed; c09-history-focused-build-passed; c09-history-focused-run-passed; c10-mushroom-log-focused-build-passed; c10-mushroom-log-focused-run-passed; c11-lake-oasis-focused-build-passed-before-result-boundary-migration; c11-lake-oasis-focused-run-passed-before-result-boundary-migration; c11-append-result-system-boundary-migration-focused-build-passed; c11-append-result-system-boundary-migration-focused-run-passed; c12-hell-special-focused-build-passed; c12-hell-special-focused-run-passed; c12-component-api-focused-build-passed; c12-component-api-focused-run-passed; c13-dungeon-derived-properties-focused-build-passed; c13-dungeon-derived-properties-focused-run-passed; c14-core-focused-passed; c14-worldfile-adapter-focused-passed; c14-netmessage-adapter-focused-passed; c06-reservation-gate-focused-build-passed; c06-reservation-gate-focused-run-passed; c06-larva-solidity-projection-focused-build-passed; c06-larva-solidity-projection-focused-run-passed; c06-underground-desert-bounds-query-focused-build-passed; c06-underground-desert-bounds-query-focused-run-passed; c06-larva-component-paired-storage-focused-build-passed; c06-larva-component-paired-storage-focused-run-passed; c06-component-coverage-audit-passed; worldsession-current-build-passed-with-one-preexisting-nullable-warning; worldsession-current-run-passed
implementationStatus: completed
completedComponents: [C01, C02.WorldLayerMetricsComponentAndCommitBoundary, C02.WorldLayerMetricsQueryAndFocusedVerifier, C02.WorldLayerMetricsCalculatorAndOwnerSystem, C03.WorldSpawnAndLandmassCommitBoundary, C03.WorldGenerationLiquidBoundaryCommitBoundary, C03.WorldLandmassDefinitionAdapterAndSurfaceMaterialQuery, C04.BeachBoundaryStateCommitAndQueryBoundary, C04.BeachBoundaryFocusedVerifier, C04.BeachBoundaryCalculatorAndCommitSystem, C05.OceanBiomeStateAndPureQueries, C05.OceanBiomeConstraintCommitBoundaryAndBoundedTreasureRecorder, C05.OceanBiomeStateAndPureQueriesFocusedVerifier, C05.OceanBiomeConstraintCalculationAndCommitSystem, C05.OceanCaveTreasureAttemptOverflowReset, C05.OceanCaveTreasurePlacementProjection, C06.UndergroundDesertStateAndPureQueries, C06.UndergroundDesertStructureCommitBoundary, C06.UndergroundDesertLarvaRecorder, C06.UndergroundDesertStateAndCommitFocusedVerifier, C06.UndergroundDesertResetSentinelBoundary, C06.UndergroundDesertResetSentinelBoundary, C06.UndergroundDesertReservationGateAndLarvaProjection, C06.UndergroundDesertLarvaTileSolidityProjection, C06.UndergroundDesertBoundsQuery, C06.UndergroundDesertLarvaComponentPairedStorage, C07.JungleStructureStateAndPureQueries, C07.JungleRegionStructureCommitBoundary, C07.PyramidPlacementCommitBoundary, C07.JungleChestAndLootCommitBoundary, C07.JungleHutMaterialDefinitionQuery, C07.JungleHutMaterialSelectionQuery, C07.JungleRegionConversionRangeQuery, C07.JungleChestLootSelectionQueryAndCursorCommit, C07.JungleRegionBoundsCalculationQuery, C08.DungeonLayoutRewardAndFloatingIslandStateBoundaries, C09.MountainCaveHistoryStateBoundary, C09.SurfaceTunnelHistoryBoundary, C09.SurfaceOrePatchHistoryBoundary, C10.MushroomBiomeAnchorStateBoundary, C10.FallenLogFlowerHandoffBoundary, C11.LakeAndOasisHistoryBoundary, C11.LakeOasisCommitResultSystemBoundaryMigration, C12.HellChestLootCycleBoundary, C12.StatuePlacementCatalogAndTrapBoundary, C12.InfectionAndSpecialSeedRuleBoundary, C12.ShimmerAnchorBoundary, C13.DungeonDerivedPropertiesBoundary, C14.WorldSavedOreTierRepairAndReset, C14.WorldSavedOreTierCommitBoundary, C14.WorldFileSavedOreTierValueMappingAdapter, C14.NetMessageSavedOreTierOutboundProjection]
currentComponent: none (all independently evidenced P17 ECS component boundaries are implemented)
pendingComponents: []
currentStatusNote: The manual P17 session `e588106a6053430ca46c41cb07e8e4ca` settled successfully with runner status `completed`, `exitCode: 0`, and `lockReleased: true` at `2026-09-12T09:37:09.8280014Z`. The component-only implementation scope is complete (`pendingComponents: []`). The entries in `pendingIntegrationBoundaries` remain integration-only follow-up work and are not additional component gaps. Historical checkpoint prose below is retained for audit and does not override this terminal status.
latestVerificationStatus: Fresh serial C12 focused build exited 0 with 0 warnings and 0 errors; its no-build focused run exited 0. Fresh serial `WorldSessionFocusedVerifier` build exited 0 with 0 warnings and 0 errors; its no-build focused run exited 0. Artifacts are under `Build/bin/` as listed in `currentSessionVerification`.
terminalVerificationStatus: The final current-source verification for this settled session is C12 focused build/run exit 0 with 0 warnings/0 errors and WorldSession focused build/run exit 0 with 0 warnings/0 errors.
pendingIntegrationBoundaries: [C03 external generation/material/liquid ownership, C04 RNG/upstream/shell effects, C05 candidate search and chest/tile/liquid effects, C06 normalization/retry/reservation/tile/entity effects, C07 placement/material/RNG effects, C08 external dungeon-record and island execution ownership, C09 scan/tile/ore handoff, C10 mushroom/log effects, C11 terrain/liquid/vegetation effects, C12 external structure/item/tile effects, C13 production backing and restore ordering, C14 production save/network/load ownership, and legacy parity across C03-C14]
lastCheckpointUtc: 2026-09-12T09:45:41.5358627Z
currentSessionComponentCoverage: The authoritative P17 report contains 146 members. Every member has an implemented root-src component, immutable component snapshot, bounded definition, or explicitly documented external adapter/derived-query boundary; no additional component-only member gap was found.
currentSessionSourceChange: `src/WorldSession/WorldGeneration/Components/HellChestLootCycleComponent.cs` now provides the existing no-argument advance API and the compatible success-result overload used by the current C12 system. No System, Query, Command, Adapter, Projection, test, verifier, project, or configuration file was modified in this session.
currentSessionVerification: C08 focused build/run and C12 focused build/run passed with exit code 0 and 0 warnings/0 errors; the current `WorldSessionFocusedVerifier` build passed with exit code 0, 0 warnings, and 0 errors; its no-build/no-restore run passed with exit code 0. The C08 artifact is `Build/bin/Terraria.WorldGenerationC08FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC08FocusedVerifier.dll`; the C12 artifact is `Build/bin/Terraria.WorldGenerationC12FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC12FocusedVerifier.dll`; the WorldSession artifacts are `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` and `Build/bin/Terraria.WorldSessionFocusedVerifier/Debug/net10.0/Terraria.WorldSessionFocusedVerifier.dll`.
componentScopeDecision: All 146 report members are now accounted for by root-src ECS components, immutable snapshots, bounded definitions, or explicitly non-component adapter/derived-query boundaries. The remaining pending work is integration-only and is intentionally not represented as a component gap.
evidence-gap-c06-component-coverage-audit: The authoritative 146-member inventory has no additional component-only field or property gap. Configuration, structure-map, material-definition, derived-property, persistence, network, and external-effect members remain at their documented non-component or integration boundaries.
blocking-decision-c06-component-coverage-audit: Do not add duplicate components for C01 configuration/structures, C03 material definitions, C13 derived properties, or C14 external save/network projections. Continue only when new evidence identifies a missing authoritative component state member.
evidence-gap-c13: C13 selection, pure active-record query, and explicit control-line forwarding are focused-verified. Production DungeonControlLine backing, complete current-dungeon writer migration, generic snapshot eligibility/restore ordering, scheduler integration, persistence/network ownership, and legacy behavior equivalence remain unverified.
evidence-gap-c03: The focused C03 source boundary does not claim Version4 landmass generation formulas, spawn/RNG ownership, external material catalog binding, boulder entity effects, WorldFile waterLine load ordering, LiquidSimulation handoff, or full 15-member parity.
evidence-gap-c04: C04 beach-boundary state, complete generation-identity commit, raw-value preservation, pure query, and the explicit calculation/commit system are focused-verified. The calculator preserves the sourced random interval, anniversary fixed-boundary branch, dungeon/jungle width adjustments, complete shell-origin propagation, and raw no-clamp outputs. Global RNG ownership and exact stream consumption, upstream C02/C03 integration, shell-origin tile/liquid writing, scheduler order, and legacy behavior equivalence remain unverified.
evidence-gap-c02: C02 calculator control flow, explicit configuration input, terrain/world-generation RNG stream usage, state commit, pure query, and focused verifier are build/run verified. Complete 13-member legacy parity, snow-column lifecycle, tile-history integration, and scheduler/legacy-writer migration remain unverified.
evidence-gap-c05: C05 constraint, bounded treasure, pass-control state, pure query/recording boundaries, explicit constraint calculation/commit system, Version4 ocean-cave attempt overflow reset, and the ordered underwater-chest placement-command projection are focused-verified. The constraint calculator derives all eight values from the committed C04 beach snapshot using the sourced reset constants and consumes no global RNG. The treasure attempt entry clears a full two-slot buffer before each attempt and appends only when the external ocean-cave attempt reports that treasure was generated. Candidate coordinate search, duplicate policy, phase-token lifetime, selected-item RNG ownership, chest/tile/liquid effect execution, save/network ownership, and legacy parity remain unverified.
evidence-gap-c11-commit-result-migration: C11 append-result and status types were moved from Components to Systems, and the focused verifier compile list now references the System boundary. The serial focused build exited 0 with 0 warnings and 0 errors; the no-build focused run exited 0 with C11 lake and oasis focused verifier passed. The migration is verified; candidate scanning, terrain/liquid effects, RNG, reset/restore, vegetation, persistence, network, and legacy parity remain unverified.
evidence-gap-c06: C06 layout, pure query, generation-identity commit, bounded larva recorder, capacity rejection, clear, copied snapshot, reset-sentinel, larva projection, reservation-before-command gate, ordered tile-solidity projection, and the pure rectangle bounds query are saved. The reset uses explicit world dimensions and restores empty rectangles, Version4 hive bounds, and a zero larva count. The solidity projection describes tile types 229 before larva placement and 232/162 after placement; it does not access Main.tileSolid or execute tile writes. The bounds query preserves the evidenced left/top-inclusive and right/bottom-exclusive semantics, positive-area intersection, zero-area rejection, and hive containment predicate. Physical Version4 array reallocation is not claimed. Layout normalization, retry behavior, duplicate policy, concrete reservation identity/bounds mapping, tile/entity execution, persistence/network, and legacy parity remain unverified.
evidence-gap-c07-material: Version4 explicitly selects `jungleHut` from one `genRand.Next(5)` roll and maps roll values `0..4` to tile IDs `119/120/158/175/45`, then maps those tile IDs to wall IDs `23/24/42/45/10`, with wall ID `0` for other values. The explicit-roll selection query, pure definition/query mapping, and isolated verifier are focused-verified. Global RNG ownership and consumption order, external tile/material registration, tile writes, wall writes, and legacy behavior equivalence remain unverified.
evidence-gap-c07-region: Version4 explicitly applies the jungle conversion predicate `x >= jungleMinX && x <= jungleMaxX`; the pure inclusive-range query and the explicit boundary calculation from copied column observations are focused-verified. The calculation preserves the sourced left scan `x = 5; x < maxTilesX - 5`, right scan `x = maxTilesX - 5; x > 5`, and zero defaults when no column matches. The external tile scan adapter, world-surface input owner, wall mutation, RNG/world-rule ownership, scheduler ordering, placement effects, and legacy behavior equivalence remain unverified.
evidence-gap-c07-loot: Version4 explicitly selects jungle chest items from the `JungleItemCount % 4` cycle, applies the 50/15/20 short-circuit overrides, and increments the cursor. The pure selection query and cursor-commit system are focused-verified with explicit roll inputs. Exact random-source consumption order, the `gennedLivingMahoganyWands` unique-result policy, chest/item effect execution, and legacy behavior equivalence remain unverified.
evidence-gap-current: C02 calculator and owner source is saved from the confirmed Version4 TerrainPass control flow, and the current isolated calculator build/run passed with zero warnings and zero errors. The current WorldSession focused build also passed with one pre-existing nullable warning and zero errors; its no-build focused run passed. C03 bounded spawn/landmass, landmass-definition conversion, pure surface-material infection flip, and liquid-boundary query/commit sources are saved and their focused build/run both passed; C03 generation RNG, external material catalog, boulder effects, WorldFile waterLine barrier, and LiquidSimulation handoff remain open. C04 beach-boundary state, complete generation-identity commit, pure query, and explicit calculation/commit sources are saved; the C04 focused build and no-build run passed with zero warnings and zero errors. The calculator's global RNG owner, exact stream consumption, upstream C02/C03 integration, shell-origin tile/liquid writer, scheduler order, and legacy parity remain open. C05 constraint, bounded treasure, pass-control state, pure query/recording boundaries, explicit constraint calculation/commit, ocean-cave attempt overflow reset, and ordered underwater-chest placement-command projection now have serial focused build/run evidence; C05 candidate search, selected-item RNG, phase-token, duplicate, chest/item/tile/liquid effects, persistence/network, and legacy-parity boundaries remain open. C08 and C12 focused component verifiers now pass with zero warnings and zero errors. C10 and C11 focused source boundaries are saved and their fresh focused build/run passed with zero warnings and zero errors; their external effects remain unverified. C12 external chest/item, tile, wire, liquid, protected-structure, persistence/network, snapshot-restore, RNG, and legacy-writer parity remain unproven.
evidence-gap: C02's Version4 calculator control flow, injected dimension/configuration/RNG seam, initial beach padding, feature branch behavior, metric ordering, and right-beach reset are implemented in `src` and passed by the isolated serial build/run; complete 13-member legacy parity, snow-column lifecycle, and tile-history integration remain unproven. C03 spawn/landmass and liquid-boundary commit boundaries are saved and focused-verified, but external LandmassData conversion, spawn/RNG ownership, material selection, boulder entity effects, WorldFile waterLine load ordering, and LiquidSimulation handoff remain unproven. C04 state, commit, pure query, explicit calculation/commit system, and focused verifier boundaries are saved and focused-verified; the sourced interval and fixed-boundary branches are covered, while global RNG ownership, exact stream consumption, upstream C02/C03 routing, shell-origin tile/liquid effects, scheduler order, and legacy parity remain unproven. C05 state, bounded recorder, pass control, pure queries, explicit C04-derived constraint calculation, Version4 ocean-cave attempt overflow reset, and focused verifier boundaries are saved and focused-verified; treasure duplicate policy, phase-token lifetime, placement projection, save/network ownership, item/tile effects, and legacy parity remain unproven. C06 layout state, pure queries, generation-identity commit, bounded larva recorder, capacity rejection, clear, and copied snapshot boundaries are saved and focused-verified; rectangle edge semantics, hive normalization, retry behavior, larva duplicate/commit policy, reservation ordering, and legacy parity remain unproven. C07 region, pyramid, and chest/loot state/query and generation-identity commit boundaries are implemented and passed the fresh serial focused verifier; material mapping, dynamic allocation parity, duplicate/reservation ordering, loot cursor policy, RNG ownership, tile/item/NPC effects, and legacy parity remain unproven. C09 history boundaries and copied strict-distance queries are saved and documented as focused-verified; Version4 overflow/reset behavior, dependent-reader adapters, tunnel scan/TileRunner ordering, OrePatch/tile ordering, C14 saved-tier handoff, rollback semantics, and legacy parity remain unproven. C10 mushroom-anchor and fallen-log handoff boundaries are saved and passed by the isolated focused build/run; ShroomPatch/Flowers wiring, tile/liquid effects, RNG, persistence, network, external transactionality, and legacy parity remain unproven. C11 lake/oasis history, copied snapshots, strict queries, mixed-generation rejection, and append-after-success gates are saved and passed by the isolated focused build/run; candidate scanning, SonOfLakinater/PlaceOasis, terrain/liquid effects, RNG, reset/restore, downstream vegetation, persistence/network, and legacy parity remain unproven. Historical note: an earlier full WorldSession build was blocked by duplicate C10 declarations in the shared checkout; this does not describe the current focused build result and is not C09/C10/C11 source evidence. C14's value-only WorldFile adapter and outbound network projection are implemented and passed the fresh serial focused verifier for confirmed version gates, field order, and signed-short casts; their production BinaryReader/BinaryWriter and transport owners, header/version upgrade policy, load barrier, inbound consumption, capability negotiation, and error/rollback contracts remain unproven. C01 external Version4 WorldGenConfiguration/StructureMap binding, C14 aggregate ownership, and the remaining cross-subsystem ownership gaps remain open.
blocking-decision: Do not infer C02/C03/C07 generation formulas, RNG streams, external registration keys, LandmassData or tile/liquid APIs, or remove legacy writers. C04's sourced calculation branches may remain as an explicit-input pure query and generation-scoped commit system, but its global RNG owner, exact stream consumption, upstream C02/C03 integration, shell-origin tile/liquid writer, scheduler order, and legacy replacement remain at integration-review. Keep C07 material mapping, reservation/placement ordering, tile/item/NPC effects, loot policy, and legacy writer replacement at integration-review. The verified C07 region, pyramid, and chest/loot commit boundaries are limited to generation-identity validation, capacity/count validation, paired-list validation, copied state, and complete state replacement; they do not prove calculator or external placement behavior. C07.JungleRegionCalculatorAndPlacementEffects is now split at the evidence-backed pure boundary calculation: the copied-column scan and inclusive predicate are implemented and focused-verified. The external tile-scan adapter, world-surface owner, world-rule/material/RNG owner, wall mutation, structure/tile effect ports, scheduler ordering, and legacy writer replacement remain at integration-review. The verified C09 boundary is limited to bounded copied histories, pure strict-distance reads, effective-capacity rejection, clear behavior, and the explicit ore append-after-success gate; do not infer Version4 overflow/reset parity, dependent-reader wiring, cave/tunnel/OrePatch scans, TileRunner/tile/liquid integration, C14 saved-tier handoff, or scheduler order. The C10 implementations are limited to bounded copied mushroom-anchor history and the evidenced fallen-log handoff transition: failed placement or rejected random selection does not publish, later accepted coordinates overwrite, consumption clears only LogX, stale LogY is preserved, and reset clears both. Do not infer mushroom/log tile effects, Flowers wiring, RNG, liquid, persistence, network, or rollback ordering. C11 must remain limited to independently evidenced lake/oasis state, copied snapshots, strict queries, and explicit commit-result gates; do not infer terrain/liquid adapters, RNG streams, reset ownership, snapshot restore ordering, or downstream vegetation effects. Keep BinaryReader/BinaryWriter binding, header/version upgrades, waterLine load sequencing, inbound packet consumption, capability negotiation, and all external save/network/liquid/session ownership at integration-review. The C14 WorldFile and NetMessage implementations are limited to verified pure value projections; the fresh verifier does not claim production load/save/network integration. Continue only with independently evidenced state, commit, and pure-query boundaries. C05 ocean-cave attempt overflow reset is limited to clearing a full bounded buffer before an externally reported attempt result and appending only a successful treasure coordinate; it does not claim chest/item/tile placement, random search, duplicate policy, or legacy writer replacement. The runner record is already completed, so this implementation continuation has no valid new settlement operation.
blocking-decision-c07-loot: The focused loot unit is limited to explicit-roll item output, override priority, input validation, and cursor-only state commit. Do not bind a global RNG, infer its exact consumption order, implement wand uniqueness, or write chest/item effects until those owners and contracts are evidenced.
blocking-decision-c07-material-selection: Keep jungle-hut selection as an explicit-input pure query. Do not bind global RNG, external material registration, tile/wall mutation, or legacy writer replacement without an identified owner and focused parity evidence.
blocking-decision-c06: C06 rectangle queries now use only the evidenced left/top-inclusive and right/bottom-exclusive semantics and do not normalize layout inputs. The reservation gate remains limited to the generic reservation port and evidenced larva command projection; it does not identify the DesertBiome reservation key, layout formula, duplicate policy, or external tile/entity executor.
evidence-gap-c06-solidity-projection: The C06 solidity projection source and focused verifier are serial-build/run verified. The constructor's prior CS0118 defect was corrected by assigning the public `Timing` property. The projection emits only the ordered `229` before-placement and `232`/`162` after-placement `Solid=true` commands; it does not identify or invoke a production tile-solidity writer.
blocking-decision-c06-solidity-projection: Keep tile-solidity updates as a pure command projection. Do not access `Main.tileSolid`, add a tile writer, infer scheduler order, or claim production tile mutation until the tile-commit owner and integration contract are evidenced.

## Current Continuation Checkpoint

As of `2026-09-12T09:45:41.5358627Z`, this document is the terminal record for the manual
`P17` session `e588106a6053430ca46c41cb07e8e4ca`. The authoritative report remains `146/146`.
All independently evidenced component boundaries are implemented and `pendingComponents` is
empty. The remaining entries are explicitly integration-only: external generation and material
ownership, RNG and scheduler wiring, tile/liquid/entity effects, persistence/network/load owners,
derived-property backing, and legacy parity. They are not additional component gaps and are not
implemented in this component-only continuation. The runner settlement is complete; historical
notes below do not override the terminal result.

## C06 Underground Desert Bounds Query Checkpoint

Implemented and saved in root `src`:

- `src/WorldSession/WorldGeneration/UndergroundDesertRectangle.cs`
- `src/WorldSession/WorldGeneration/Queries/UndergroundDesertStructureQuery.cs`
- `src/WorldGenerationC06FocusedVerifier/Program.cs`

The C06 component field mapping is complete for the nine authoritative members: the structure
component owns both rectangles and the four hive-bound scalars; the larva component owns separate
capacity-100 `int[]` storage for the `larvaX` and `larvaY` members while exposing only copied
`TilePosition` snapshots.

The rectangle value now exposes the evidence-backed pure geometry used by Version4 readers:
left/top-inclusive and right/bottom-exclusive point containment, positive-area intersection,
zero-area rejection, and nested hive containment. The structure query exposes these predicates
without normalizing layout inputs, changing component state, reserving structures, or invoking
tile/entity effects. The verifier covers the top-left and right/bottom boundaries, touching
rectangles, hive containment, and zero-width rejection.

This checkpoint does not implement rectangle normalization, `DesertBiome` layout calculation,
retry/`skipDesertTileCheck` behavior, concrete reservation identity or bounds mapping, tile/entity
execution, persistence, network, or legacy-writer replacement. The focused serial build and
no-build/no-restore run passed after this source checkpoint was saved.

Verification records:

- Build: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src\\WorldGenerationC06FocusedVerifier\\Terraria.WorldGenerationC06FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; project `src/WorldGenerationC06FocusedVerifier/Terraria.WorldGenerationC06FocusedVerifier.csproj`; exit `0`; warnings/errors `0/0`; artifact `Build/bin/Terraria.WorldGenerationC06FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC06FocusedVerifier.dll`.
- Focused run: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\\src\\WorldGenerationC06FocusedVerifier\\Terraria.WorldGenerationC06FocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; output `C06 underground-desert focused verifier passed.`

The component-only source update after this checkpoint preserves the existing snapshot API while
aligning private storage with the authoritative paired `larvaX`/`larvaY` arrays. No new test,
system, command, adapter, or external writer was added.


## C06 Larva Component Paired Storage Checkpoint

Updated and saved only the component source:

- `src/WorldSession/WorldGeneration/UndergroundDesertLarvaPlacementComponent.cs`

The component now owns separate capacity-100 `int[]` buffers for the authoritative `larvaX`
and `larvaY` members. `TryAppend` writes both coordinates at the same used index, `Clear`
clears both buffers and the used count, and `CreateSnapshot` preserves the existing copied
`TilePosition` snapshot API. No test, System, Command, Adapter, or external writer was added.

Verification records:

- Build: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src\\WorldGenerationC06FocusedVerifier\\Terraria.WorldGenerationC06FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; warnings/errors `0/0`; artifact `Build/bin/Terraria.WorldGenerationC06FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC06FocusedVerifier.dll`.
- Focused run: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\\src\\WorldGenerationC06FocusedVerifier\\Terraria.WorldGenerationC06FocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; output `C06 underground-desert focused verifier passed.`

## 1. Design Status and Scope

This is a proposed decomposition of the 146 members in the P17 authoritative report with a partial
implementation continuation checkpoint. It does not change Version4, alter the authoritative
report, or change the ledger. Implemented source is explicitly marked below; no behavior equivalence
is claimed. Every not-yet-created Component, System, Query, Adapter, Projection, CommitPort, and
Definition remains `status: proposed`.

P17 covers the 14 report leaf groups under `WorldGenerationAndEcology`:

| checkpoint | report leaf group | members | proposed role |
|---|---|---:|---|
| C01 | `GenVarsConfigurationAndOreState` | 10 | configuration boundary, reservation adapter, generation ore selection state |
| C02 | `GenVarsWorldLayerMetrics` | 13 | world layer measurement state |
| C03 | `GenVarsSurfaceAndBiomeState` | 15 | surface, infection, material, and liquid-boundary facts |
| C04 | `GenVarsBeachAndOceanBoundaryState` | 12 | beach boundary and shell/ocean input state |
| C05 | `WorldGenBeachAndOceanBiomeState` | 12 | ocean biome avoidance and treasure scratch state |
| C06 | `WorldGenUndergroundDesertStructureState` | 9 | underground desert structure facts and placement inputs |
| C07 | `WorldGenJungleStructureState` | 15 | jungle, pyramid, and chest structure facts |
| C08 | `GenVarsDungeonAndIslands` | 19 | dungeon control state and floating-island generation state |
| C09 | `GenVarsCaveTunnelAndOrePatchState` | 9 | cave, tunnel, and ore-patch scratch state |
| C10 | `GenVarsMushroomBiomeAndLogState` | 5 | mushroom biome and log placement scratch state |
| C11 | `GenVarsLakeAndOasisState` | 8 | lake and oasis placement scratch state |
| C12 | `GenVarsHellAndSpecialStructures` | 9 | hell chest, statue-trap, infection, shimmer, and special-seed state |
| C13 | `GenVarsDungeonDerivedProperties` | 3 | read-only dungeon selection and dual-dungeon queries/adapters |
| C14 | `WorldSavedOreTierState` | 7 | persistent and network-aware saved ore tiers |

P17 retains terrain and biome facts and their definitions. Generation execution, tile mutation
payloads, liquid propagation, progression/transition side effects, session ownership, NPC/town
effects, and secret-seed policy are integration boundaries. Findings about those boundaries use
`crossSubsystemOwner: integration-review`; this document does not assign another partition an owner.

## 2. Evidence Ledger

| source | evidence used | result | evidenceStatus |
|---|---|---|---|
| `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P17-World-Terrain-Biomes-GenVars.md` | immutable P17 member inventory | 14 leaf groups, 143 fields, 3 properties, 146 members; expected and observed counts both 146 | confirmed |
| `D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs:11-300` | declarations and initializers | public static mutable generation state, readonly capacities/constants, `configuration` with `[JsonIgnore]`, `StructureMap`, dungeon list, and three derived properties | confirmed |
| `D:\TRbackup\Version4\Terraria\WorldGen.cs:3318-3333` | `SavedOreTiers` declaration | seven saved ore tier values are a separate nested state from `GenVars` generation fields | confirmed |
| `D:\TRbackup\Version4\Terraria\WorldGen.cs:6387-6491,10096-10514` | load/generation setup and reset | generation setup loads configuration, clears/reset state, initializes structures, layer values, beach values, and ore selection inputs | confirmed |
| `D:\TRbackup\Version4\Terraria\WorldGen.cs:10960-12653,13651-13969,14331-15222,16380-17604,20145-20150,28493-30410,63808-63809` | generation call sites | P17 fields are read and mutated by terrain, biome, structure, cave, water, ore, dungeon, hell, and special-structure passes; exact complete ownership is not closed | partial |
| `D:\TRbackup\Version4\Terraria\WorldGen.cs:10773-10912,12687-12707,13641-13700,13960-13974,14697-14706,17573-17638` | C09 cave, tunnel, and ore-patch history call sites | Tunnels records midpoint X values with an effective 49-entry threshold; MountainCaves resets and appends paired origins; Lakes/Webs/opening/tree passes consume cave/tunnel history; SurfaceOreAndStone records successful OrePatch origins after tile work | confirmed for call sites; reset/overflow transaction remains partial |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\LegacyMountainCavesPass.cs`, `LegacyTunnelsPass.cs`, `LegacyCaveCoordinate.cs`, `Systems\OrePatchPlacementSystem.cs` | current C09 implementation comparison | local cave history is exposed only through a pipeline list, tunnel history stays local, and ore placement has no `orePatchX` history owner; command boundaries exist but are not GenVars-equivalent | partial |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:658-783,811-850,1353-1429,2184-2407,3693-3707` | world load/save and repair | `SavedOreTiers` has explicit load/save/version-repair paths; `waterLine` is written during load; other GenVars persistence is not established here | confirmed for saved tiers; partial for other members |
| `D:\TRbackup\Version4\Terraria\NetMessage.cs:385-391` | world-state packet | all seven `SavedOreTiers` values are encoded as shorts; no equivalent evidence was found for the transient GenVars scratch fields | confirmed |
| `D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs:286-308`, `D:\TRbackup\Version4\Terraria\WorldGen.cs:10096-10514` | C13 property declarations and selection call sites | `CurrentDungeon` clamps only below zero; `CurrentDungeonGenVars` directly indexes `dungeonGenVars`; generation setup/reset and dual-dungeon paths switch the active selection | confirmed for local property behavior; complete writer and upper-index lifecycle remain partial |
| `D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs:10-50` | C13 dither-control backing state | `DungeonControlLine.NormalizedDistanceSafeFromDither` is the direct static backing value for the forwarding property; no local validation is present | confirmed for forwarding; persistence and cross-system owner remain partial |
| `D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs:28-32,110-132,162-171` | generation snapshot boundary | public writable non-readonly GenVars members are reflected into the snapshot except `[JsonIgnore]` and readonly members; restore calls `WorldGen.Reset()` before applying JSON; implementation of the converter is incomplete in this snapshot | partial |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration` | current NLTX implementation | world-generation components, checkpoint/runtime state, biome surface, ore placement, structure placement, validation, and explicit system order already exist, but the full Version4 field ownership is partial | confirmed current-state inventory |
| `D:\TRbackup\tmodloader-api-docs-stable\annotated.html`, `class_gen_vars.html`, `class_world_gen.html`, `class_structure_map.html` | public API cross-reference | tModLoader v2026.07 confirms public `GenVars`, `StructureMap`, `GenerateWorld`, `SavedOreTiers`, and generation RNG boundaries; it does not prove Version4 private semantics | partial |
| `C:\Users\shan\Downloads\ECS\space-station-14-master` | ECS organization reference | used only for component/system/query and output-boundary patterns; no code, names, or game semantics are copied | existing-evidence |

The authoritative report is the source of truth for membership, source rows, declaration types, and
source locations. Version4 source is read-only evidence for lifecycle and access patterns. Public
tModLoader documentation and SS14 are organizational cross-references only.

## 3. Current NLTX Status

The current Dome contains the following relevant partial boundaries:

- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/WorldGenerationStateComponent.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/WorldGenerationTerrainState.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/BiomeSurfaceComponent.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/TerrainProfileComponent.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/WorldRuleSnapshotComponent.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/WorldGenerationCheckpoint.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationRuntimeState.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/HardmodeOreTierState.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/HardmodeOreTierSelectionPolicy.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationLegacyServerFields.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/WorldGenerationSystemOrder.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/BiomeSurfaceSystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/OrePlacementTransactionSystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/StructurePlacementTransactionSystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/WorldGenerationValidationSystem.cs`

Those files show useful existing seams but do not establish full Version4 equivalence, a unique
tile-write owner for every pass, complete generation snapshot restoration, or complete persistence
and replication. P17 therefore proposes additions and adapters only; it does not claim that any
listed proposed type already exists.

## 4. Authority and Boundary Model

```text
World configuration / seed / world-load input
          |
          v
Configuration Definition Adapter --(immutable definition)--> P17 owner systems
          |                                                     |
          |                                                     +--> layer/surface/beach/biome facts
          |                                                     +--> structure reservation commands
          |                                                     +--> tile/liquid commit commands
          |                                                     +--> saved ore tier commit command
          v                                                     v
WorldStorage / WorldSession / RNG ports                 immutable P17 snapshots
                                                               |
                         +-------------------------------------+----------------------+
                         v                                                            v
                 pure dungeon/terrain queries                         Adapter / Projection outputs
                 (no write-back)                                      save, network, UI, diagnostics
```

The core authority stores stable domain values and bounded generation scratch state. Systems own
state transitions and receive explicit input snapshots. Queries calculate read-only results.
Adapters translate Version4 or external types, including `StructureMap`, `DungeonGenVars`, save
records, and packet representations. Projections publish one-way outputs. Components and queries
do not call clocks, random generators, file APIs, logging, network APIs, or tile/liquid mutation
APIs.

`GenVars.configuration` is a definition/configuration boundary. `GenVars.structures` is a
generation reservation/placement boundary. They are intentionally not one component and neither
external Version4 type is allowed to leak into the core authority. `WorldGen.SavedOreTiers` is an
independent persistent/network state boundary and is not an alias for the generation-time
`copper`, `iron`, `silver`, `gold`, or `*Bar` values.

`CurrentDungeon`, `CurrentDungeonGenVars`, and
`DualDungeon_NormalizedDistanceSafeFromDither` remain query/adapter seams. Their properties must
not be copied into a second writable component without first identifying the backing state and
setter command owner.

## 5. Proposed System Order and Contracts

The following order is a scheduler contract, not a file or directory ordering rule. It is
provisional until integration review closes the missing pass and cross-subsystem evidence.

1. `WorldGenerationConfigurationLoadSystem` loads a definition through a configuration adapter.
2. `WorldGenerationResetSystem` clears generation-session scratch state and establishes bounded
   capacities; it does not write save files or network packets.
3. `WorldLayerMetricsSystem` computes layer measurements from explicit world dimensions and RNG
   inputs, then commits a layer snapshot.
4. `SurfaceBiomeStateSystem` consumes layer facts and commits surface/material/liquid-boundary
   facts.
5. `BeachBoundarySystem` commits beach and shell/ocean boundaries.
6. `BeachOceanBiomeSystem` consumes boundaries and commits ocean avoidance and treasure scratch
   state.
7. `UndergroundDesertStructureSystem` and `JungleStructureSystem` calculate structure intents and
   send reservations through `StructureReservationCommitPort`.
8. `DungeonAndIslandStateSystem` updates dungeon/island facts and emits explicit handoff commands
   for generation execution owned by `crossSubsystemOwner: integration-review`.
9. `CaveTunnelAndOrePatchSystem`, `MushroomAndLogSystem`, and `LakeAndOasisSystem` update their
   bounded scratch components and emit tile/liquid intents through explicit ports.
10. `HellAndSpecialStructureSystem` updates hell/statue/special-seed facts and emits explicit
    structure or item intents; it does not own progression or secret-seed policy.
11. `OreTierSelectionSystem` commits generation-time tier selection. A separate saved-tier adapter
    consumes an explicit commit event only after the persistence policy is approved.
12. `WorldGenerationValidationSystem` checks invariants, reservation overlap results, bounds, and
    required output completeness without becoming a second writer.
13. `WorldGenerationSnapshotProjection` and `WorldSavedOreTierPersistenceAdapter` publish only
    after the relevant commit barrier; the snapshot boundary must be tested separately from save
    serialization.

Minimum interface contracts, all `status: proposed`:

```text
IWorldGenerationConfigurationSource
  Load(input: WorldGenerationConfigurationRequest) -> WorldGenerationConfigurationDefinition

IStructureReservationCommitPort
  Reserve(intent: StructureReservationIntent) -> ReservationResult
  CanPlace(query: StructurePlacementQuery) -> bool

IWorldGenerationTileCommitPort
  Commit(intent: TileMutationIntent) -> void

IWorldGenerationLiquidCommitPort
  Commit(intent: LiquidSourceIntent) -> void

IGenerationRandomSource
  NextInt(stream: GenerationRandomStream, minInclusive, maxExclusive) -> int

ISavedOreTierStore
  Read(worldId) -> SavedOreTierRecord
  Write(worldId, SavedOreTierRecord) -> void

ISavedOreTierNetworkProjection
  Encode(SavedOreTierSnapshot) -> WorldStatePacketFields
```

All interface names and signatures are design placeholders, not existing APIs. Command payloads
must be immutable and include the generation/session identity needed for idempotence. Commit ports
own effect ordering, failure reporting, and retry policy. A rejected reservation or tile intent is
an explicit result, not an exception hidden inside a component.

## 6. C01 GenVarsConfigurationAndOreState

Checkpoint `C01` is complete as a design checkpoint. It deliberately contains three boundaries:

1. `GenVars.configuration` maps to a proposed immutable
   `WorldGenerationConfigurationDefinition` loaded through an `Adapter`; it is not transient
   component authority and `[JsonIgnore]` is retained as evidence that it is excluded from the
   inspected GenVars snapshot.
2. `GenVars.structures` maps to a proposed `StructureReservationAdapter` behind
   `IStructureReservationCommitPort`. Core systems see reservation intents and results, never the
   external `StructureMap` type.
3. `copper`, `iron`, `silver`, `gold`, `copperBar`, `ironBar`, `silverBar`, and `goldBar` map to a
   proposed `WorldGenerationOreSelectionComponent` with one owner system. These values describe
   generation-time selection/material IDs and must not be mechanically double-written with C14
   `WorldGen.SavedOreTiers`.

### C01 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2099 | `configuration` | `Terraria.WorldBuilding.WorldGenConfiguration` | `GenVars.cs:13-14` `[JsonIgnore] public static WorldGenConfiguration configuration;` | Adapter -> `WorldGenerationConfigurationDefinition` | `GenerateWorld` setup and generation passes that consume definitions | generation setup/configuration loader; exact complete writer set partial | per-generation definition; external definition object | configuration source read only through adapter | excluded by inspected GenVars snapshot; save/network status not established |
| 2100 | `structures` | `Terraria.WorldBuilding.StructureMap` | `GenVars.cs:16` `public static StructureMap structures;` | Adapter + `StructureReservationCommitPort` | biome and structure placement paths, `CanPlace` callers | reset initializes; reservation/add-protected-structure paths mutate reservation state | generation-session reservation index | reservation/overlap side effect through port | no direct save/network evidence |
| 2101 | `copper` | `int` | `GenVars.cs:18` `public static int copper;` | Component -> `WorldGenerationOreSelectionComponent` | ore placement and material selection passes | reset and ore-tier selection owner; complete writer set partial | generation-session selection fact | none in component; tile writes delegated | no direct persistence/network evidence |
| 2102 | `iron` | `int` | `GenVars.cs:20` `public static int iron;` | Component -> `WorldGenerationOreSelectionComponent` | ore placement and material selection passes | reset and ore-tier selection owner; complete writer set partial | generation-session selection fact | none in component; tile writes delegated | no direct persistence/network evidence |
| 2103 | `silver` | `int` | `GenVars.cs:22` `public static int silver;` | Component -> `WorldGenerationOreSelectionComponent` | ore placement and material selection passes | reset and ore-tier selection owner; complete writer set partial | generation-session selection fact | none in component; tile writes delegated | no direct persistence/network evidence |
| 2104 | `gold` | `int` | `GenVars.cs:24` `public static int gold;` | Component -> `WorldGenerationOreSelectionComponent` | ore placement and material selection passes | reset and ore-tier selection owner; complete writer set partial | generation-session selection fact | none in component; tile writes delegated | no direct persistence/network evidence |
| 2105 | `copperBar` | `int` | `GenVars.cs:26` `public static int copperBar = 20;` | Component -> `WorldGenerationOreSelectionComponent` | ore placement, item/material selection paths | declaration initializer and selection owner; complete writer set partial | generation-session material mapping | none in component | no direct persistence/network evidence |
| 2106 | `ironBar` | `int` | `GenVars.cs:28` `public static int ironBar = 22;` | Component -> `WorldGenerationOreSelectionComponent` | ore placement, item/material selection paths | declaration initializer and selection owner; complete writer set partial | generation-session material mapping | none in component | no direct persistence/network evidence |
| 2107 | `silverBar` | `int` | `GenVars.cs:30` `public static int silverBar = 21;` | Component -> `WorldGenerationOreSelectionComponent` | ore placement, item/material selection paths | declaration initializer and selection owner; complete writer set partial | generation-session material mapping | none in component | no direct persistence/network evidence |
| 2108 | `goldBar` | `int` | `GenVars.cs:32` `public static int goldBar = 19;` | Component -> `WorldGenerationOreSelectionComponent` | ore placement, item/material selection paths | declaration initializer and selection owner; complete writer set partial | generation-session material mapping | none in component | no direct persistence/network evidence |

### C01 Invariants and Risks

- A configuration definition is immutable after the generation session starts. Reloading it during a
  pass is an explicit unsupported transition until Version4 call-site evidence says otherwise.
- A reservation is not a tile mutation. `CanPlace`, `AddProtectedStructure`, and reservation
  conflict results cross `IStructureReservationCommitPort`; the core authority does not expose
  `StructureMap`.
- The eight generation ore values are one selection snapshot. C14 owns its seven persisted values;
  any conversion between them is a named command/event with a verifier, never field aliasing.
- Static declaration initializers are compatibility facts, not a reason to perform uncontrolled
  writes at type-load time in the proposed ECS implementation.

### C01 Dependency Impact

The proposed configuration adapter depends outward on world configuration storage and content
definitions. The reservation adapter depends outward on the structure implementation and inward
on the generation systems through a narrow port. The ore selection system reads seed/configuration
facts and emits a selection commit; ore tile placement and saved-tier persistence consume separate
snapshots. `WorldStorage`, `WorldSession`, `WorldProgressionAndTransition`, and P18-P20 remain
`crossSubsystemOwner: integration-review`.

### C01 Implementation Checkpoint

Implemented C01 in `src/WorldSession/WorldGeneration` with explicit boundaries:

- `WorldGenerationOreSelectionComponent` and `WorldGenerationOreSelectionSnapshot` own the eight
  generation-session values and expose one validated writer plus a copied snapshot.
- `WorldGenerationConfigurationDefinition`, `WorldGenerationConfigurationSnapshot`,
  `IWorldGenerationConfigurationSource`, `WorldGenerationConfigurationRequest`, and
  `WorldGenerationConfigurationAdapter` keep immutable configuration data at an adapter seam.
- `StructureReservationIntent`, `ReservationResult`, `IStructureReservationCommitPort`,
  `InMemoryStructureReservationAdapter`, `StructureReservationSnapshot`, and
  `WorldGenerationRectangle` make reservation-before-tile behavior explicit without referencing
  the unavailable external `StructureMap` type.
- `OreTierSelectionSystem` is the single C01 selection commit entry and rejects a mismatched
  generation identity.

Focused verification was executed after source save. The serial wrapper built
`src/WorldSession/Terraria.WorldSession.csproj` and
`Test/Terraria.WorldSession.WorldGeneration.Verification/Terraria.WorldSession.WorldGeneration.Verification.csproj`
with exit code 0, zero warnings, and zero errors; artifacts are under
`Build/bin/Terraria.WorldSession/Debug/net10.0` and
`Build/bin/Terraria.WorldSession.WorldGeneration.Verification/Debug/net10.0`.
The no-build/no-restore verifier run also exited 0. The remaining C01 gap is external Version4
type binding and its integration owner; no legacy writer was removed.

## 7. C02 GenVarsWorldLayerMetrics

Checkpoint `C02` is complete as a design checkpoint. The 13 members describe measurements and
bounded snow-region scratch state produced during world generation. They belong together at the
first component boundary because layer metrics are consumed as a coherent set by surface, biome,
ore, cave, and structure passes, while the snow arrays have the same generation-session lifetime as
their scalar bounds. The proposed component stores values only; a `WorldLayerMetricsSystem` owns
calculation and a separate immutable snapshot feeds downstream systems.

The component must distinguish measured facts from definitions. `worldSurface*`, `rockLayer*`, and
`lowestCloud` are measurements or derived thresholds for this generation run, not durable world
configuration. `snowTop`, `snowBottom`, origins, and `snowMinX`/`snowMaxX` are bounded pass scratch
and must not be treated as a public world map. The exact sentinel and range behavior must be
characterized before migration.

### C02 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2115 | `lowestCloud` | `int` | `GenVars.cs:46` `public static int lowestCloud = -1;` | Component -> `WorldLayerMetricsComponent` | cloud/terrain pass consumers and generation predicates | `WorldGen.Reset` initializer and cloud/layer pass; complete writer set partial | per-generation measured sentinel/value | none in component; downstream cloud/tile work via ports | no direct save/network evidence |
| 2125 | `worldSurfaceLow` | `double` | `GenVars.cs:66` `public static double worldSurfaceLow;` | Component -> `WorldLayerMetricsComponent` | terrain, biome, cave, structure, and ore passes | layer measurement owner after reset; complete writer set partial | per-generation lower surface bound | none in component | no direct save/network evidence |
| 2126 | `worldSurface` | `double` | `GenVars.cs:68` `public static double worldSurface;` | Component -> `WorldLayerMetricsComponent` | terrain, biome, cave, structure, and ore passes | layer measurement owner after reset; complete writer set partial | per-generation central surface measurement | none in component | no direct save/network evidence |
| 2127 | `worldSurfaceHigh` | `double` | `GenVars.cs:70` `public static double worldSurfaceHigh;` | Component -> `WorldLayerMetricsComponent` | terrain, biome, cave, structure, and ore passes | layer measurement owner after reset; complete writer set partial | per-generation upper surface bound | none in component | no direct save/network evidence |
| 2128 | `rockLayerLow` | `double` | `GenVars.cs:72` `public static double rockLayerLow;` | Component -> `WorldLayerMetricsComponent` | terrain, biome, cave, structure, and ore passes | layer measurement owner after reset; complete writer set partial | per-generation lower rock-layer bound | none in component | no direct save/network evidence |
| 2129 | `rockLayer` | `double` | `GenVars.cs:74` `public static double rockLayer;` | Component -> `WorldLayerMetricsComponent` | terrain, biome, cave, structure, and ore passes | layer measurement owner after reset; complete writer set partial | per-generation central rock-layer measurement | none in component | no direct save/network evidence |
| 2130 | `rockLayerHigh` | `double` | `GenVars.cs:76` `public static double rockLayerHigh;` | Component -> `WorldLayerMetricsComponent` | terrain, biome, cave, structure, and ore passes | layer measurement owner after reset; complete writer set partial | per-generation upper rock-layer bound | none in component | no direct save/network evidence |
| 2131 | `snowTop` | `int` | `GenVars.cs:78` `public static int snowTop;` | Component -> `WorldLayerMetricsComponent` | snow, surface, biome, and structure passes | reset and snow measurement owner; complete writer set partial | per-generation snow region bound | tile material intents delegated to tile commit port | no direct save/network evidence |
| 2132 | `snowBottom` | `int` | `GenVars.cs:80` `public static int snowBottom;` | Component -> `WorldLayerMetricsComponent` | snow and surface passes | reset and snow measurement owner; complete writer set partial | per-generation snow region bound | tile material intents delegated to tile commit port | no direct save/network evidence |
| 2133 | `snowOriginLeft` | `int` | `GenVars.cs:82` `public static int snowOriginLeft;` | Component -> `WorldLayerMetricsComponent` | snow and surface passes | reset and snow measurement owner; complete writer set partial | per-generation snow placement origin | tile material intents delegated to tile commit port | no direct save/network evidence |
| 2134 | `snowOriginRight` | `int` | `GenVars.cs:84` `public static int snowOriginRight;` | Component -> `WorldLayerMetricsComponent` | snow and surface passes | reset and snow measurement owner; complete writer set partial | per-generation snow placement origin | tile material intents delegated to tile commit port | no direct save/network evidence |
| 2135 | `snowMinX` | `int[]` | `GenVars.cs:86` `public static int[] snowMinX;` | Component -> bounded `SnowColumnBounds` value | snow terrain and biome passes | reset/allocation and snow scan owner; complete writer set partial | per-generation mutable scratch array | tile material intents delegated to tile commit port; array mutation isolated in owner | no direct save/network evidence |
| 2136 | `snowMaxX` | `int[]` | `GenVars.cs:88` `public static int[] snowMaxX;` | Component -> bounded `SnowColumnBounds` value | snow terrain and biome passes | reset/allocation and snow scan owner; complete writer set partial | per-generation mutable scratch array | tile material intents delegated to tile commit port; array mutation isolated in owner | no direct save/network evidence |

### C02 Ownership and Invariants

- `WorldLayerMetricsSystem` is the only proposed writer after reset. Readers consume an immutable
  `WorldLayerMetricsSnapshot`; they do not mutate the component or arrays.
- The six surface/rock metrics must preserve their relative ordering and exact floating-point
  conversion behavior. Sentinel values such as `lowestCloud = -1` are part of the compatibility
  contract until call-site characterization proves a stronger invariant.
- `snowMinX` and `snowMaxX` are a paired bounded structure. Their length, index domain, default
  values, and handling of unvisited columns must be explicit. A snapshot copies the used range or
  exposes an immutable view; it never exposes the owner's mutable array.
- Snow tile writes are commands to `IWorldGenerationTileCommitPort`. The metric component does not
  write tiles, invoke liquid simulation, or register structures.
- C02 does not own world dimensions, save-file fields, cloud rendering, or surface material
  definitions. Those inputs and outputs are integration seams.

### C02 Interface Contract and Dependency Impact

`IWorldLayerMetricsCalculator` (proposed) accepts an immutable world-dimension snapshot, a
configuration definition, and an injected `IGenerationRandomSource`; it returns a complete
`WorldLayerMetricsSnapshot`. `WorldLayerMetricsSystem` validates bounds and commits one snapshot.
`ISnowColumnBoundsSink` (proposed) is internal to that owner and cannot be called by downstream
systems. `WorldGenerationTerrainQuery` (proposed) is pure over the committed snapshot.

The component depends inward only on value definitions and generation identity. Surface/biome,
beach, desert, jungle, dungeon, cave, lake, and ore systems depend on its read-only snapshot. Cloud
rendering, liquid simulation, world storage, world session, and P18-P20 generation execution are
outside P17. Their ownership is `crossSubsystemOwner: integration-review`.

### C02 Focused Verification Plan

Before moving a legacy writer, characterize: reset sentinel values; metric ordering and boundary
rounding; deterministic output for the same dimension/configuration/RNG stream; snow-array length,
defaults, and paired-index behavior; snapshot immutability; and no tile/liquid side effects from
the component or query. A regression fixture must compare the complete 13-member legacy snapshot
with the proposed snapshot at the end of the same generation pass. The isolated C02 verifier has
run successfully for the implemented baseline metric and initial-padding boundaries; complete
legacy-snapshot parity has not run.

### C02 Implementation Checkpoint

Implemented `WorldLayerMetricsComponent`, `WorldLayerMetricsSnapshot`, and the atomic commit boundary
at `src/WorldSession/WorldGeneration/WorldLayerMetricsComponent.cs`,
`src/WorldSession/WorldGeneration/WorldLayerMetricsSnapshot.cs`, and
`src/WorldSession/WorldGeneration/Systems/WorldLayerMetricsSystem.cs`. The component owns the 11 scalar
members and paired snow-column arrays, preserves the `lowestCloud = -1` and zero-valued defaults,
copies both input arrays, rejects mismatched array lengths, and exposes only read-only views. The
snapshot copies the arrays again so downstream readers cannot mutate component-owned storage.

`WorldLayerMetricsSystem.Commit` validates the generation identity and copies the complete snapshot
through one internal component writer. This is a state commit boundary only; it does not calculate
dimensions, consume RNG, write tiles, invoke liquid simulation, or publish persistence/network data.

The C02 calculation boundary is now implemented at
`src/WorldSession/WorldGeneration/Passes/WorldLayerMetricsCalculator.cs`,
`src/WorldSession/WorldGeneration/Passes/WorldLayerMetricsCalculationInput.cs`,
`src/WorldSession/WorldGeneration/Passes/IWorldLayerMetricsCalculator.cs`, and the existing
`WorldLayerMetricsSystem` owner. The calculator accepts copied configuration/beach inputs and an
injected `IGenerationRandomSource`; it preserves Version4 initial beach padding, feature-duration
selection, special Mountain/Valley branches, metric update ordering, and the conditional right-beach
Plateau reset. It remains pure: no tile, liquid, clock, persistence, network, or logging effect is
performed. The C02 focused verifier source is additionally isolated at
`src/WorldGenerationC02FocusedVerifier/` so unrelated shared-project files cannot hide calculator
compile errors.

The existing state/query verifier still checks all 13 members, paired-array length rejection,
input-array isolation, snapshot isolation, and generation identity. The new isolated verifier adds
the Version4 baseline metric fixture and the initial beach-padding call-order fixture. Its serial
build exited `0` with `0` warnings and `0` errors, and its no-build focused run exited `0` with
`C02 world layer metrics focused verifier passed.`. No legacy writer or world descriptor state was
modified. Tile `SurfaceHistory` retargeting, `waterLine`/`lavaLine`, later snow writes, complete
13-member legacy parity, and full-project integration remain outside this pure C02 boundary.

## C03 GenVarsSurfaceAndBiomeState

Checkpoint `C03` is complete as a design checkpoint. The 15 members are intentionally split into
three cohesive proposed boundaries rather than one surface mega-component:

- `WorldSpawnAndLandmassComponent` owns spawn randomization, landmass working data, and Remix
  layer bounds. Its system produces surface facts and spawn decisions but does not own players or
  world-session identity.
- `SurfaceMaterialDefinition` is an immutable generation definition for infection and moss tile
  and wall choices. It is selected from world-rule facts and content definitions; it is not a
  general-purpose mutable component or a tile-write API.
- `WorldGenerationLiquidBoundaryComponent` owns `lavaLine` and `waterLine` as generation/load
  boundary facts. `waterLine` has a separate WorldFile load write in the evidence, so its final
  writer is an integration decision rather than an automatic P17 claim.

`boulderPetsPlaced` is kept with the spawn/surface generation progress boundary because it is a
generation counter, not an NPC or pet authority. Any emitted pet/entity effect crosses an explicit
command port.

### C03 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2109 | `worldSpawnHasBeenRandomized` | `bool` | `GenVars.cs:34` `public static bool worldSpawnHasBeenRandomized = false;` | Component -> `WorldSpawnAndLandmassComponent` | spawn selection and surface generation predicates | reset and spawn randomization path; complete writer set partial | per-generation spawn decision fact | spawn command/event through WorldSession port | no direct save/network evidence; WorldSession boundary |
| 2110 | `landmassData` | `List<LandmassData>` | `GenVars.cs:36` `public static List<LandmassData> landmassData = new List<LandmassData>();` | Component with bounded value list -> `LandmassGenerationState` | terrain, surface, biome, and Remix passes | reset/clear and landmass generation passes; complete writer set partial | per-generation mutable scratch collection | terrain/tile intents through commit port; list mutation isolated in owner | no direct save/network evidence |
| 2111 | `remixSurfaceLayerLow` | `int` | `GenVars.cs:38` `public static int remixSurfaceLayerLow;` | Component -> `WorldSpawnAndLandmassComponent` | Remix surface and terrain passes | reset and Remix layer calculator; complete writer set partial | per-generation derived layer bound | none in component | no direct save/network evidence |
| 2112 | `remixSurfaceLayerHigh` | `int` | `GenVars.cs:40` `public static int remixSurfaceLayerHigh;` | Component -> `WorldSpawnAndLandmassComponent` | Remix surface and terrain passes | reset and Remix layer calculator; complete writer set partial | per-generation derived layer bound | none in component | no direct save/network evidence |
| 2113 | `remixMushroomLayerLow` | `int` | `GenVars.cs:42` `public static int remixMushroomLayerLow;` | Component -> `WorldSpawnAndLandmassComponent` | Remix mushroom and biome passes | reset and Remix layer calculator; complete writer set partial | per-generation derived layer bound | none in component | no direct save/network evidence |
| 2114 | `remixMushroomLayerHigh` | `int` | `GenVars.cs:44` `public static int remixMushroomLayerHigh;` | Component -> `WorldSpawnAndLandmassComponent` | Remix mushroom and biome passes | reset and Remix layer calculator; complete writer set partial | per-generation derived layer bound | none in component | no direct save/network evidence |
| 2116 | `boulderPetsPlaced` | `int` | `GenVars.cs:48` `public static int boulderPetsPlaced = 0;` | Component -> `SurfaceGenerationProgressComponent` | boulder/bonus generation pass and validation | reset and boulder placement owner; complete writer set partial | per-generation placement counter | pet/entity creation only through explicit command port | no direct save/network evidence; NpcAndTownSimulation seam |
| 2117 | `crimStoneWall` | `ushort` | `GenVars.cs:50` `public static ushort crimStoneWall = 83;` | Definition -> `SurfaceMaterialDefinition` | infection and wall placement passes | definition selection/reset; exact writer set partial | generation material definition | tile material intents through tile commit port | no direct save/network evidence |
| 2118 | `crimStone` | `ushort` | `GenVars.cs:52` `public static ushort crimStone = 203;` | Definition -> `SurfaceMaterialDefinition` | infection and tile placement passes | definition selection/reset; exact writer set partial | generation material definition | tile material intents through tile commit port | no direct save/network evidence |
| 2119 | `ebonStoneWall` | `ushort` | `GenVars.cs:54` `public static ushort ebonStoneWall = 3;` | Definition -> `SurfaceMaterialDefinition` | infection and wall placement passes | definition selection/reset; exact writer set partial | generation material definition | tile material intents through tile commit port | no direct save/network evidence |
| 2120 | `ebonStone` | `ushort` | `GenVars.cs:56` `public static ushort ebonStone = 25;` | Definition -> `SurfaceMaterialDefinition` | infection and tile placement passes | definition selection/reset; exact writer set partial | generation material definition | tile material intents through tile commit port | no direct save/network evidence |
| 2121 | `mossTile` | `ushort` | `GenVars.cs:58` `public static ushort mossTile = 179;` | Definition -> `SurfaceMaterialDefinition` | moss and surface material passes | definition selection/reset; exact writer set partial | generation material definition | tile material intents through tile commit port | no direct save/network evidence |
| 2122 | `mossWall` | `ushort` | `GenVars.cs:60` `public static ushort mossWall = 54;` | Definition -> `SurfaceMaterialDefinition` | moss and wall material passes | definition selection/reset; exact writer set partial | generation material definition | tile material intents through tile commit port | no direct save/network evidence |
| 2123 | `lavaLine` | `int` | `GenVars.cs:62` `public static int lavaLine;` | Component -> `WorldGenerationLiquidBoundaryComponent` | lava/ocean, tile-runner, cave, and validation passes | reset and terrain/liquid boundary calculation; complete writer set partial | per-generation liquid boundary fact | liquid source/propagation through `IWorldGenerationLiquidCommitPort` | no direct save/network evidence; LiquidSimulation seam |
| 2124 | `waterLine` | `int` | `GenVars.cs:64` `public static int waterLine;` | Component + load adapter -> `WorldGenerationLiquidBoundaryComponent` | water, tile-runner, cave, and load-time liquid setup | reset/generation paths and `WorldFile.LoadWorld` write at `WorldFile.cs:751`; authority unresolved | generation and load boundary fact | `Liquid.QuickWater`/water checks through load and liquid ports | no direct member save evidence; WorldStorage/LiquidSimulation seam |

### C03 Ownership and Invariants

- `WorldSpawnAndLandmassSystem` owns the mutable list and publishes an immutable landmass snapshot;
  no downstream system receives the live `List<LandmassData>` reference.
- Remix bounds are derived generation facts. They cannot replace C02 surface metrics or become
  persisted world configuration without a separate schema decision.
- `SurfaceMaterialDefinitionSystem` selects infection/material values from explicit world-rule facts.
  The `flipInfections` transition is an input, not a hidden write from a tile pass.
- `lavaLine` and `waterLine` are boundary values, not liquid volume. Tile and liquid side effects
  use separate ports. `waterLine` cannot be migrated until the generation reset writer and the
  WorldFile load writer have a single documented authority or an explicit phase-scoped protocol.
- `boulderPetsPlaced` is a counter only. It cannot create NPC, pet, or town state directly.

### C03 Interface Contract and Dependency Impact

Proposed interfaces are `IWorldSpawnStateOwner`, `ISurfaceMaterialDefinitionSource`, and
`ILiquidBoundaryCommitPort`. Spawn calculation accepts world dimensions, seed facts, and a random
source; material selection accepts infection/world-rule facts and a content catalog adapter; liquid
boundary calculation returns values but does not propagate liquid. Queries over all three snapshots
are pure.

C02 is an input to the layer and spawn calculations. C04/C05 consume beach and ocean facts after
surface metrics are committed. WorldStorage owns load sequencing, LiquidSimulation owns propagation,
WorldSession owns spawn/session identity, and NpcAndTownSimulation owns entity effects. P18-P20 may
consume commands but are not assigned by this document. Each unresolved boundary is
`crossSubsystemOwner: integration-review`.

### C03 Focused Verification Plan

Characterize reset values and list clearing; Remix bounds and spawn randomization idempotence;
material selection under normal, flipped-infection, and secret-seed inputs; counter monotonicity and
reset; `lavaLine`/`waterLine` bounds; and the load barrier where `waterLine` is set before liquid
settling. Verify that snapshots do not alias lists and that components emit no tile, liquid, spawn,
NPC, persistence, or network effects directly. No verifier has run.

### C03 Implementation Checkpoint

Implemented the state/value and spawn/landmass commit portions of C03 in `src/WorldSession/WorldGeneration`:

- `WorldSpawnAndLandmassComponent.cs` owns the spawn-randomized flag, Remix layer bounds,
  `boulderPetsPlaced`, and a copied list of `WorldGenerationLandmassValue` records.
- `WorldGenerationLandmassValue.cs` is the explicit current-project value boundary for the
  Version4 `LandmassData` adapter; it does not expose `Terraria.WorldBuilding` or XNA types.
- `WorldSpawnAndLandmassSnapshot.cs` is a copied read snapshot.
- `Systems/WorldSpawnAndLandmassSystem.cs` validates generation identity and is the only proposed
  commit boundary for replacing the complete spawn/Remix/landmass snapshot.
- `SurfaceMaterialDefinition.cs` is the immutable six-value material definition and preserves the
  Version4 defaults `83, 203, 3, 25, 179, 54` through `SurfaceMaterialDefinition.Version4`.
- `WorldGenerationLiquidBoundaryComponent.cs` and
  `WorldGenerationLiquidBoundarySnapshot.cs` own and copy `lavaLine`/`waterLine` values only.
- `Systems/WorldGenerationLiquidBoundarySystem.cs` validates generation identity and commits the
  complete liquid-boundary snapshot through one internal writer. It does not establish the
  WorldFile load barrier or call LiquidSimulation.

`WorldLandmassDefinitionAdapter` now maps the confirmed current-project
`WorldLandmassDefinition` fields into copied `WorldGenerationLandmassValue` records without
exposing XNA or legacy types. `SurfaceMaterialDefinitionQuery` applies only the evidenced
Version4 `flipInfections` swap of crimson/ebon stone and wall values and leaves moss values
unchanged. `WorldSpawnAndLandmassQuery` and `WorldGenerationLiquidBoundaryQuery` expose copied
snapshots only. The C03 focused build exited `0` with `0` warnings and `0` errors; the
serial-wrapper `run --no-build --no-restore` exited `0` with `C03 spawn, material, and
liquid-boundary focused verifier passed.`. These additions still do not implement a spawn
calculator, RNG owner, boulder entity effect, tile/liquid commit port, or the WorldFile
`waterLine` load adapter. Those items remain evidence gaps.

## C04 GenVarsBeachAndOceanBoundaryState

Checkpoint `C04` is complete as a design checkpoint. The 12 members form a beach-boundary value
object: left/right beach extents, sand-width parameters, dungeon/jungle adjustments, shell origins,
and the lower ocean-water random bound. They are inputs to later ocean and structure passes, not
tile storage and not the ocean-biome avoidance state in C05.

### C04 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2137 | `leftBeachEnd` | `int` | `GenVars.cs:90` `public static int leftBeachEnd;` | Component -> `BeachBoundaryComponent` | beach, surface-cave, ocean, dungeon, and structure passes | reset and beach boundary pass; complete writer set partial | per-generation left boundary | beach tile intents via tile commit port | no direct save/network evidence |
| 2138 | `rightBeachStart` | `int` | `GenVars.cs:92` `public static int rightBeachStart;` | Component -> `BeachBoundaryComponent` | beach, surface-cave, ocean, dungeon, and structure passes | reset and beach boundary pass; complete writer set partial | per-generation right boundary | beach tile intents via tile commit port | no direct save/network evidence |
| 2139 | `beachBordersWidth` | `int` | `GenVars.cs:94` `public static int beachBordersWidth;` | Component -> `BeachBoundaryComponent` | beach and ocean width calculations | reset/configuration and beach boundary pass; complete writer set partial | per-generation boundary definition/fact | none in component; tile changes via port | no direct save/network evidence |
| 2140 | `beachSandRandomCenter` | `int` | `GenVars.cs:96` `public static int beachSandRandomCenter;` | Component -> `BeachBoundaryComponent` | beach sand, avoidance, and structure placement passes | reset/configuration and beach calculator; complete writer set partial | per-generation randomization parameter | RNG used only by owner through explicit source | no direct save/network evidence |
| 2141 | `beachSandRandomWidthRange` | `int` | `GenVars.cs:98` `public static int beachSandRandomWidthRange;` | Component -> `BeachBoundaryComponent` | beach sand and ocean boundary passes | reset/configuration and beach calculator; complete writer set partial | per-generation randomization parameter | RNG used only by owner through explicit source | no direct save/network evidence |
| 2142 | `beachSandDungeonExtraWidth` | `int` | `GenVars.cs:100` `public static int beachSandDungeonExtraWidth;` | Component -> `BeachBoundaryComponent` | beach/dungeon placement predicates | reset/configuration and beach calculator; complete writer set partial | per-generation structure adjustment | structure placement only through reservation/tile ports | no direct save/network evidence |
| 2143 | `beachSandJungleExtraWidth` | `int` | `GenVars.cs:102` `public static int beachSandJungleExtraWidth;` | Component -> `BeachBoundaryComponent` | beach/jungle placement predicates | reset/configuration and beach calculator; complete writer set partial | per-generation structure adjustment | structure placement only through reservation/tile ports | no direct save/network evidence |
| 2144 | `shellStartXLeft` | `int` | `GenVars.cs:104` `public static int shellStartXLeft;` | Component -> `BeachBoundaryComponent` | shell/ocean content placement pass | reset and shell boundary calculation; complete writer set partial | per-generation left shell origin | shell placement intent via structure/tile ports | no direct save/network evidence |
| 2145 | `shellStartYLeft` | `int` | `GenVars.cs:106` `public static int shellStartYLeft;` | Component -> `BeachBoundaryComponent` | shell/ocean content placement pass | reset and shell boundary calculation; complete writer set partial | per-generation left shell origin | shell placement intent via structure/tile ports | no direct save/network evidence |
| 2146 | `shellStartXRight` | `int` | `GenVars.cs:108` `public static int shellStartXRight;` | Component -> `BeachBoundaryComponent` | shell/ocean content placement pass | reset and shell boundary calculation; complete writer set partial | reset and shell boundary calculation; complete writer set partial | per-generation right shell origin | shell placement intent via structure/tile ports | no direct save/network evidence |
| 2147 | `shellStartYRight` | `int` | `GenVars.cs:110` `public static int shellStartYRight;` | Component -> `BeachBoundaryComponent` | shell/ocean content placement pass | reset and shell boundary calculation; complete writer set partial | per-generation right shell origin | shell placement intent via structure/tile ports | no direct save/network evidence |
| 2148 | `oceanWaterStartRandomMin` | `int` | `GenVars.cs:112` `public static int oceanWaterStartRandomMin;` | Component -> `BeachBoundaryComponent` | ocean water-depth setup and beach/ocean pass | reset/configuration and beach calculator; complete writer set partial | per-generation lower random bound | RNG and liquid intents through explicit ports | no direct save/network evidence; LiquidSimulation seam |

### C04 Ownership and Invariants

- `BeachBoundarySystem` is the only proposed writer after reset and publishes an immutable
  `BeachBoundarySnapshot`. C05 consumes that snapshot and cannot update it.
- Left/right boundary ordering, world-width clamping, shell-origin pairing, and the relationship
  between `beachBordersWidth`, random center, and extra widths must be characterized from the same
  seed/configuration stream.
- `oceanWaterStartRandomMin` is an input bound, not a water volume. Liquid creation and settling
  cross `IWorldGenerationLiquidCommitPort` and remain outside the component.
- Dungeon and jungle extra widths are boundary inputs only. They do not transfer dungeon or jungle
  ownership to C04.
- C04 does not include C05 avoidance counters or treasure arrays; separating them prevents an ocean
  pass from mutating the beach boundary while searching for a placement.

### C04 Interface Contract and Dependency Impact

`BeachBoundaryCalculationQuery` accepts explicit world dimensions, boundary configuration,
dungeon-side and secret-seed flags, already-consumed random rolls, and shell-origin values, then
returns a complete `BeachBoundarySnapshot`. `BeachBoundaryCalculationSystem` validates generation
identity and commits it. The query does not own or access global RNG. Downstream ocean, desert,
jungle, dungeon, cave, and structure systems depend on the snapshot through pure queries or
explicit intents. Tile, liquid, storage, session, and P18-P20 execution boundaries remain
`crossSubsystemOwner: integration-review`.

### C04 Focused Verification Plan

Verify reset defaults, left/right ordering and clamping, random parameter determinism, paired shell
origins, extra-width effects, and no aliasing of the snapshot. Compare all 12 values at the beach
pass boundary and verify that boundary calculation emits no tile, liquid, reservation, save, or
network effect directly. The focused verifier covers raw-value preservation, complete commit,
pure query behavior, generation-identity rejection, the half-open random interval, the
anniversary branch, and dungeon-side width selection. Global RNG ownership and legacy parity
remain a separate integration verification gate.

### C04 Implementation Checkpoint

Implemented the evidence-backed C04 state, commit, and pure-query boundary in the root `src`
project:

- `src/WorldSession/WorldGeneration/BeachBoundaryComponent.cs` owns all 12 authoritative boundary
  integers for one generation session.
- `src/WorldSession/WorldGeneration/BeachBoundarySnapshot.cs` is the immutable value snapshot.
- `src/WorldSession/WorldGeneration/Systems/BeachBoundarySystem.cs` validates generation identity
  and replaces all 12 values in one commit; it does not sort, clamp, calculate RNG values, write
  tiles/liquid, or reserve structures.
- `src/WorldSession/WorldGeneration/Queries/BeachBoundaryQuery.cs` is a pure read boundary that
  returns the component snapshot and performs no external effect.

The component preserves raw source values because the state commit boundary must not silently sort
or clamp values. The focused verifier source is `src/WorldGenerationC04FocusedVerifier/Program.cs`
with project `src/WorldGenerationC04FocusedVerifier/Terraria.WorldGenerationC04FocusedVerifier.csproj`.
The serial-wrapper build exited `0` with `0` warnings and `0` errors, and the no-build/no-restore
run exited `0` with `C04 beach-boundary focused verifier passed.`. No legacy writer was changed or
removed.

The evidence-backed calculation boundary is now implemented in
`src/WorldSession/WorldGeneration/Queries/BeachBoundaryCalculationInput.cs`,
`src/WorldSession/WorldGeneration/Queries/BeachBoundaryCalculationQuery.cs`, and
`src/WorldSession/WorldGeneration/Systems/BeachBoundaryCalculationSystem.cs`. The query accepts
explicit dimension, boundary configuration, dungeon-side, secret-seed flags, random rolls, and
existing shell-origin values. It preserves the sourced half-open random interval, uses the fixed
`center + width` branch for non-Remix tenth-anniversary worlds, applies the dungeon/jungle extra
width to the matching side, and returns raw no-clamp values. The system validates generation
identity and commits the complete calculated snapshot through `BeachBoundarySystem`; it does not
own global RNG, tile/liquid writes, shell placement, or scheduler ordering.

The calculator-focused verifier is included in the existing
`src/WorldGenerationC04FocusedVerifier/` project. Its serial-wrapper build exited `0` with `0`
warnings and `0` errors, and its no-build/no-restore run exited `0` with
`C04 beach-boundary focused verifier passed.`. Global RNG ownership, upstream C02/C03 routing,
shell-origin tile/liquid effects, scheduler order, and legacy behavior equivalence remain
unverified.

## C05 WorldGenBeachAndOceanBiomeState

Checkpoint `C05` is complete as a design checkpoint. This leaf is split into an ocean avoidance
definition/state boundary and a bounded ocean-cave treasure result boundary. The avoidance values
are read by multiple terrain and cave passes but are not themselves tile mutations. The treasure
array is a short-lived placement result with a count/capacity invariant and must be copied into an
immutable intent snapshot before any item or structure placement effect.

`skipDesertTileCheck` remains a phase-scoped pass-control fact. It is not a world rule, a durable
biome flag, or permission for a downstream system to skip validation globally.

### C05 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2149 | `oceanWaterStartRandomMax` | `int` | `GenVars.cs:114` `public static int oceanWaterStartRandomMax;` | Component -> `OceanBiomeConstraintComponent` | ocean and beach water-depth passes | reset and ocean boundary calculator; complete writer set partial | per-generation upper random bound | RNG/liquid intents through explicit ports | no direct save/network evidence |
| 2150 | `oceanWaterForcedJungleLength` | `int` | `GenVars.cs:116` `public static int oceanWaterForcedJungleLength;` | Component -> `OceanBiomeConstraintComponent` | ocean/jungle boundary and biome passes | reset and ocean boundary calculator; complete writer set partial | per-generation constraint | jungle/ocean tile intents through ports | no direct save/network evidence; WorldProgressionAndTransition seam |
| 2151 | `evilBiomeBeachAvoidance` | `int` | `GenVars.cs:118` `public static int evilBiomeBeachAvoidance;` | Component -> `OceanBiomeConstraintComponent` | evil-biome, beach, cave, and structure predicates | reset and avoidance calculator; complete writer set partial | per-generation spatial constraint | reservation/tile eligibility query only | no direct save/network evidence |
| 2152 | `evilBiomeAvoidanceMidFixer` | `int` | `GenVars.cs:120` `public static int evilBiomeAvoidanceMidFixer;` | Component -> `OceanBiomeConstraintComponent` | evil-biome and mid-world avoidance predicates | reset and avoidance calculator; complete writer set partial | per-generation spatial constraint | reservation/tile eligibility query only | no direct save/network evidence |
| 2153 | `lakesBeachAvoidance` | `int` | `GenVars.cs:122` `public static int lakesBeachAvoidance;` | Component -> `OceanBiomeConstraintComponent` | lake, beach, cave, and structure predicates | reset and avoidance calculator; complete writer set partial | per-generation spatial constraint | placement eligibility query only | no direct save/network evidence |
| 2154 | `smallHolesBeachAvoidance` | `int` | `GenVars.cs:124` `public static int smallHolesBeachAvoidance;` | Component -> `OceanBiomeConstraintComponent` | small-hole and surface-cave passes | reset and avoidance calculator; complete writer set partial | per-generation spatial constraint | placement eligibility query only | no direct save/network evidence |
| 2155 | `surfaceCavesBeachAvoidance` | `int` | `GenVars.cs:126` `public static int surfaceCavesBeachAvoidance;` | Component -> `OceanBiomeConstraintComponent` | surface-cave and beach passes | reset and avoidance calculator; complete writer set partial | per-generation spatial constraint | placement eligibility query only | no direct save/network evidence |
| 2156 | `surfaceCavesBeachAvoidance2` | `int` | `GenVars.cs:128` `public static int surfaceCavesBeachAvoidance2;` | Component -> `OceanBiomeConstraintComponent` | surface-cave and beach passes | reset and avoidance calculator; complete writer set partial | per-generation spatial constraint | placement eligibility query only | no direct save/network evidence |
| 2157 | `maxOceanCaveTreasure` | `int` | `GenVars.cs:130` `public static readonly int maxOceanCaveTreasure = 2;` | Definition -> `OceanCaveTreasureCapacity` | ocean cave treasure pass and validation | declaration constant only | fixed generation capacity | none in definition | no direct save/network evidence |
| 2158 | `numOceanCaveTreasure` | `int` | `GenVars.cs:132` `public static int numOceanCaveTreasure = 0;` | Component -> `OceanCaveTreasureStateComponent` | ocean cave pass, validation, and placement intent builder | reset and treasure recorder; complete writer set partial | per-generation count | item/structure intents emitted after commit | no direct save/network evidence; P18-P20 item handoff |
| 2159 | `oceanCaveTreasure` | `Point[]` | `GenVars.cs:134` `public static Point[] oceanCaveTreasure = new Point[maxOceanCaveTreasure];` | Component -> bounded `OceanCaveTreasureStateComponent` | ocean cave pass and final treasure placement | reset allocation and treasure recorder; complete writer set partial | per-generation bounded coordinate scratch | placement commands through structure/item commit port | no direct save/network evidence |
| 2160 | `skipDesertTileCheck` | `bool` | `GenVars.cs:136` `public static bool skipDesertTileCheck = false;` | Component -> `OceanBiomePassControlComponent` | ocean/desert tile checks and validation | reset and ocean pass control path; complete writer set partial | phase-scoped generation control fact | can alter eligibility only through explicit pass context | no direct save/network evidence |

### C05 Ownership and Invariants

- `OceanBiomeConstraintSystem` owns the eight avoidance/water-bound values and publishes a read-only
  constraint snapshot. It does not write tiles or change C04 boundary state.
- `OceanCaveTreasureStateComponent` stores at most `maxOceanCaveTreasure` coordinates. The count
  must satisfy `0 <= numOceanCaveTreasure <= capacity`; appending is an owner-system operation with
  explicit reject behavior when full.
- A treasure coordinate is a generation result, not an item entity or inventory record. The result
  becomes an immutable command payload only after all eligibility checks pass.
- `skipDesertTileCheck` is scoped to the pass/session token that set it and is reset before a later
  unrelated pass. It must not leak into ordinary desert or surface validation.
- C05 consumes C04's beach snapshot and C03/C02 facts. It does not own jungle progression, desert
  structure placement, liquid settling, item authority, or network replication.

### C05 Interface Contract and Dependency Impact

`OceanBiomeConstraintCalculationQuery` returns an immutable constraint snapshot from an explicit
C04 `BeachBoundarySnapshot`; the current reset formula consumes no RNG. `OceanBiomeConstraintCalculationSystem`
validates generation identity and commits the complete result. `IOceanCaveTreasureRecorder` remains
proposed: it accepts a bounded intent and returns a deterministic append/reject result.
`OceanCaveTreasureProjection` converts committed coordinates to explicit placement commands without
exposing the mutable array. Pass-control reads are pure over a phase token.

Ocean, beach, desert, jungle, cave, and structure systems consume read-only constraints. The item
and tile effects, liquid simulation, WorldStorage, WorldSession, WorldProgressionAndTransition, and
P18-P20 execution are external boundaries. Their owner is
`crossSubsystemOwner: integration-review`.

### C05 Focused Verification Plan

Verify all eight constraint values for reset-formula determinism; boundary interaction with C04;
capacity, count, coordinate order, duplicate handling, reset, and snapshot-copy behavior for
treasure; and phase isolation for `skipDesertTileCheck`. Verify rejected treasure intents have no
item/tile effects and that no save/network path consumes the scratch array directly. The focused
verifier covers complete constraint commit, stale-generation rejection, the explicit C04-derived
calculation, bounded treasure append and clear, copied snapshot isolation, and pass-control reset.

### C05 Implementation Checkpoint

Implemented the evidence-backed C05 state and pure-read boundaries in the root `src` project:

- `src/WorldSession/WorldGeneration/OceanBiomeConstraintComponent.cs` and
  `OceanBiomeConstraintSnapshot.cs` own and copy the eight ocean/biome constraint values.
- `src/WorldSession/WorldGeneration/OceanCaveTreasureStateComponent.cs` and
  `OceanCaveTreasureSnapshot.cs` own a capacity-two, count-prefixed treasure result and copy only
  the used positions into each snapshot. Full-buffer appends return `false` without changing the
  state.
- `src/WorldSession/WorldGeneration/OceanBiomePassControlComponent.cs` owns the generation-scoped
  `skipDesertTileCheck` flag and exposes explicit set/reset operations.
- `src/WorldSession/WorldGeneration/Queries/OceanBiomeConstraintQuery.cs`,
  `OceanCaveTreasureQuery.cs`, and `OceanBiomePassControlQuery.cs` expose pure reads only; none
  writes tiles, liquid, structures, items, persistence, or network state.
- `src/WorldSession/WorldGeneration/Systems/OceanBiomeConstraintSystem.cs` and
  `OceanCaveTreasureSystem.cs` own generation-identity constraint commit and bounded treasure
  append/clear operations without external effects.
- `src/WorldSession/WorldGeneration/Queries/OceanBiomeConstraintCalculationInput.cs`,
  `OceanBiomeConstraintCalculationQuery.cs`, and
  `src/WorldSession/WorldGeneration/Systems/OceanBiomeConstraintCalculationSystem.cs` derive the
  eight C05 values from the committed C04 snapshot. The query preserves the Version4 reset facts:
  `oceanWaterStartRandomMax = oceanWaterStartRandomMin + 40`, forced jungle length `275`, evil
  biome avoidance `beachSandRandomCenter + 60`, middle fixer `50`, and the four remaining
  avoidance values `beachSandRandomCenter + 20`. It is pure and does not access global RNG or
  write terrain/liquid.
- `src/WorldSession/WorldGeneration/Actions/OceanCaveTreasurePlacementCommand.cs` and
  `src/WorldSession/WorldGeneration/Queries/OceanCaveTreasureProjection.cs` project each copied
  treasure anchor, in source order, to the fixed Version4 underwater-chest call contract:
  selected item input, `notNearOtherChests: false`, `chestStyle: 17`, `trySlope: true`, and
  `chestTileType: 0`. The projection is pure and does not choose items, search candidate tiles,
  or apply chest/tile/liquid effects.
- `src/WorldGenerationC05FocusedVerifier/Program.cs` and
  `src/WorldGenerationC05FocusedVerifier/Terraria.WorldGenerationC05FocusedVerifier.csproj`
  verify the evidence-backed state boundary.

The global RNG seam, duplicate policy, phase-token protocol, candidate search, chest/item/tile/liquid
effect adapter, and legacy-parity verifier remain pending because their current-project owners and
behavior contracts are not confirmed. The placement-command projection itself is focused-verified.
The new focused verifier first failed with exit `1` and three expected missing-
type compiler errors (`CS0246`/`CS0103`) before the production calculation files were added.
The serial-wrapper build of
`src/WorldGenerationC05FocusedVerifier/Terraria.WorldGenerationC05FocusedVerifier.csproj` exited
`0` with `0` warnings and `0` errors; the no-build/no-restore run exited `0` with
`C05 ocean-biome focused verifier passed.`. No legacy writer was changed or removed.

## C06 WorldGenUndergroundDesertStructureState

Checkpoint `C06` is complete as a design checkpoint. The nine members are split into two cohesive
state boundaries with one shared phase lifetime:

- `UndergroundDesertStructureComponent` stores the desert and hive rectangles plus the four scalar
  hive bounds. These are layout facts and reservation inputs, not a tile map or a structure
  implementation. The Version4 `Rectangle` representation is translated at an adapter boundary.
- `UndergroundDesertLarvaPlacementComponent` stores the bounded larva count and paired X/Y scratch
  arrays. It produces immutable larva placement intents; it does not create NPCs or items.

`DesertBiome.Place` remains generation execution. Its configuration lookup and `StructureMap`
interaction are represented by definition and reservation ports, not copied into core components.

### C06 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2161 | `UndergroundDesertLocation` | `Rectangle` | `GenVars.cs:138` `public static Rectangle UndergroundDesertLocation = Rectangle.Empty;` | Component -> `UndergroundDesertStructureComponent` | desert biome, structure, cave, and validation passes | reset and desert placement result; complete writer set partial | per-generation layout fact | reservation/tile intents through explicit ports | no direct save/network evidence |
| 2162 | `UndergroundDesertHiveLocation` | `Rectangle` | `GenVars.cs:140` `public static Rectangle UndergroundDesertHiveLocation = Rectangle.Empty;` | Component -> `UndergroundDesertStructureComponent` | hive, larva, desert biome, and validation passes | reset and hive placement result; complete writer set partial | per-generation nested layout fact | reservation/tile/entity intents through explicit ports | no direct save/network evidence; NpcAndTownSimulation seam |
| 2163 | `desertHiveHigh` | `int` | `GenVars.cs:142` `public static int desertHiveHigh;` | Component -> `UndergroundDesertStructureComponent` | hive, larva, and bounds predicates | reset to world height and hive layout owner; complete writer set partial | per-generation scalar bound | none in component; placement eligibility via query | no direct save/network evidence |
| 2164 | `desertHiveLow` | `int` | `GenVars.cs:144` `public static int desertHiveLow;` | Component -> `UndergroundDesertStructureComponent` | hive, larva, and bounds predicates | reset to zero and hive layout owner; complete writer set partial | per-generation scalar bound | none in component; placement eligibility via query | no direct save/network evidence |
| 2165 | `desertHiveLeft` | `int` | `GenVars.cs:146` `public static int desertHiveLeft;` | Component -> `UndergroundDesertStructureComponent` | hive, larva, and bounds predicates | reset to world width and hive layout owner; complete writer set partial | per-generation scalar bound | none in component; placement eligibility via query | no direct save/network evidence |
| 2166 | `desertHiveRight` | `int` | `GenVars.cs:148` `public static int desertHiveRight;` | Component -> `UndergroundDesertStructureComponent` | hive, larva, and bounds predicates | reset to zero and hive layout owner; complete writer set partial | per-generation scalar bound | none in component; placement eligibility via query | no direct save/network evidence |
| 2167 | `numLarva` | `int` | `GenVars.cs:150` `public static int numLarva;` | Component -> `UndergroundDesertLarvaPlacementComponent` | larva placement pass, validation, and entity-intent builder | reset and larva recorder; complete writer set partial | per-generation bounded count | larva/entity command emitted only after commit | no direct save/network evidence; NpcAndTownSimulation seam |
| 2168 | `larvaY` | `int[]` | `GenVars.cs:152` `public static int[] larvaY = new int[100];` | Component -> paired bounded larva coordinate value | larva placement pass and validation | reset allocation and larva recorder; complete writer set partial | per-generation coordinate scratch | entity/tile intent through explicit port; no live array exposure | no direct save/network evidence |
| 2169 | `larvaX` | `int[]` | `GenVars.cs:154` `public static int[] larvaX = new int[100];` | Component -> paired bounded larva coordinate value | larva placement pass and validation | reset allocation and larva recorder; complete writer set partial | per-generation coordinate scratch | entity/tile intent through explicit port; no live array exposure | no direct save/network evidence |

### C06 Ownership and Invariants

- The structure component owns the layout snapshot; the larva component owns only bounded coordinate
  scratch. Neither component owns `StructureMap`, `DesertBiome`, tile storage, NPC state, or item
  inventories.
- Hive bounds must be consistent with `UndergroundDesertHiveLocation` after normalization. The
  adapter must define inclusive/exclusive edge semantics before any query is migrated.
- `larvaX` and `larvaY` are a paired array. `numLarva` is the authoritative used length for the
  generation phase, subject to a capacity of 100; appends beyond capacity are explicit rejects.
- A rectangle or larva coordinate is not a placement result until the reservation and tile/entity
  commit ports accept it. Failed structure reservations cannot leave a larva or tile side effect.
- Reset must create fresh bounded storage and clear prior-generation rectangles and counts. Snapshot
  restore must not retain the prior mutable arrays.

### C06 Interface Contract and Dependency Impact

`IUndergroundDesertLayoutCalculator` (proposed) accepts C02-C05 snapshots, configuration, world
dimensions, and an RNG source and returns a normalized layout snapshot. `ILarvaPlacementRecorder`
(proposed) accepts a bounded coordinate intent and returns append/reject. `IUndergroundDesertQuery`
(proposed) exposes pure bounds and occupancy predicates. `IStructureReservationCommitPort` and
`IWorldGenerationTileCommitPort` remain the only placement effect seams.

C06 consumes beach/ocean constraints and layer/surface facts. C07 jungle placement, C08 dungeon
facts, C09 cave facts, LiquidSimulation, NpcAndTownSimulation, WorldStorage, WorldSession, and
P18-P20 execution are outside this leaf. Each unresolved external owner is
`crossSubsystemOwner: integration-review`.

### C06 Focused Verification Plan

Verify reset sentinels, rectangle normalization and bounds, hive containment, larva capacity and
paired coordinate order, duplicate/rejected placement behavior, snapshot array isolation, and
reservation-before-commit ordering. Characterize the Version4 desert placement retry loop and
`skipDesertTileCheck` interaction before changing any writer. The focused verifier covers complete
layout state/commit, stale-generation rejection, bounded larva append/overflow/clear, and copied
snapshot isolation.

### C06 Implementation Checkpoint

Implemented the evidence-backed C06 state and pure-query boundaries in the root `src` project:

- `src/WorldSession/WorldGeneration/UndergroundDesertStructureComponent.cs`,
  `UndergroundDesertStructureSnapshot.cs`, and `UndergroundDesertRectangle.cs` own the two
  rectangle values and four hive-bound integers using a current-project value type.
- `src/WorldSession/WorldGeneration/UndergroundDesertLarvaPlacementComponent.cs` and
  `UndergroundDesertLarvaPlacementSnapshot.cs` own a capacity-100, count-prefixed coordinate
  buffer and copy only its used positions. Full-buffer appends return `false` without mutation.
- `src/WorldSession/WorldGeneration/Queries/UndergroundDesertStructureQuery.cs` and
  `UndergroundDesertLarvaPlacementQuery.cs` expose pure copied reads.
- `src/WorldSession/WorldGeneration/Systems/UndergroundDesertStructureSystem.cs` and
  `UndergroundDesertLarvaPlacementSystem.cs` own generation-identity commit and bounded
  append/clear operations without external effects.
- `src/WorldGenerationC06FocusedVerifier/Program.cs` and
  `src/WorldGenerationC06FocusedVerifier/Terraria.WorldGenerationC06FocusedVerifier.csproj`
  verify the evidence-backed state and commit boundaries.
- `src/WorldSession/WorldGeneration/Queries/UndergroundDesertResetInput.cs` copies and validates
  the world dimensions required by the sourced reset sentinels.
- `src/WorldSession/WorldGeneration/Systems/UndergroundDesertResetSystem.cs` restores empty
  rectangles, the four dimension-derived hive bounds, and clears the larva used length after
  validating that both state owners belong to the same generation.

No rectangle normalization, inclusive/exclusive edge rule, retry loop, duplicate policy,
reservation, tile, entity, persistence, or network behavior was inferred. The reset RED build
exited `1` with `0` warnings and `2` expected missing-symbol errors. After the source and verifier
project references were added, the serial-wrapper GREEN build exited `0` with `0` warnings and
`0` errors; the no-build/no-restore run exited `0` with
`C06 underground-desert focused verifier passed.`. The artifact was verified at
`Build/bin/Terraria.WorldGenerationC06FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC06FocusedVerifier.dll`.
The physical Version4 array reallocation and all calculator, adapter, and external effect
boundaries remain pending; no legacy writer was changed or removed.

## C06 Reservation Gate And Larva Projection Checkpoint

Implemented and saved in root `src`:

- `src/WorldSession/WorldGeneration/Actions/UndergroundDesertPlacementCommitResult.cs`
- `src/WorldSession/WorldGeneration/Systems/UndergroundDesertPlacementCommitSystem.cs`
- `src/WorldGenerationC06FocusedVerifier/Program.cs`
- `src/WorldGenerationC06FocusedVerifier/Terraria.WorldGenerationC06FocusedVerifier.csproj`

The system validates the complete pure larva projection before calling the supplied
`IStructureReservationCommitPort`. A rejected reservation returns an empty read-only command list;
an accepted reservation returns the ordered larva commands. Mixed-generation reservation and larva
inputs are rejected before the reservation port is called. The implementation does not choose a
DesertBiome reservation ID or bounds, does not normalize rectangles, and does not execute tile or
entity effects. The focused verifier completed immediately after this checkpoint: the serial-wrapper
build exited `0` with `0` warnings and `0` errors, and the no-build/no-restore run exited `0` with
`C06 underground-desert focused verifier passed.`.

Reservation-gate verification records:

- Build: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src\WorldGenerationC06FocusedVerifier\Terraria.WorldGenerationC06FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; warnings/errors `0/0`; artifact `Build/bin/Terraria.WorldGenerationC06FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC06FocusedVerifier.dll`.
- Focused run: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\src\WorldGenerationC06FocusedVerifier\Terraria.WorldGenerationC06FocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; output `C06 underground-desert focused verifier passed.`; no new compile artifact; the verified build artifact remains under `Build/bin/Terraria.WorldGenerationC06FocusedVerifier/Debug/net10.0`.

## C06 Larva Tile Solidity Projection Checkpoint

Implemented and saved in root `src`, with focused verification pending:

- `src/WorldSession/WorldGeneration/Actions/UndergroundDesertLarvaTileSolidityCommand.cs`
- `src/WorldSession/WorldGeneration/Queries/UndergroundDesertLarvaTileSolidityProjection.cs`
- `src/WorldGenerationC06FocusedVerifier/Program.cs`
- `src/WorldGenerationC06FocusedVerifier/Terraria.WorldGenerationC06FocusedVerifier.csproj`

The command is a generation-scoped, ordered description of one tile-solidity update. The pure
projection emits tile type `229` before larva placement, then tile types `232` and `162` after
placement, all with `Solid=true`. It validates generation identity and phase/type combinations,
returns a read-only command list, and does not read or mutate `Main.tileSolid`, execute tile
writes, create entities, reserve structures, persist state, or publish network messages.

Verification status at checkpoint save: `c06-larva-solidity-projection-focused-build-passed` and
`c06-larva-solidity-projection-focused-run-passed`. The verifier asserts command count, read-only
output, generation identity, phase ordering, tile IDs, solid values, and per-command validation.

After the checkpoint's first build attempt, the constructor assignment in
`UndergroundDesertLarvaTileSolidityCommand.cs` was corrected from the nested type name `Phase`
to the public property `Timing`. The serial focused build then exited `0` with `0` warnings and
`0` errors, produced the expected verifier artifact, and the no-build focused run exited `0` with
`C06 underground-desert focused verifier passed.`

Build command: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src\\WorldGenerationC06FocusedVerifier\\Terraria.WorldGenerationC06FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; project: `src/WorldGenerationC06FocusedVerifier/Terraria.WorldGenerationC06FocusedVerifier.csproj`; exit code: `0`; warning/error count: `0/0`; artifact: `Build/bin/Terraria.WorldGenerationC06FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC06FocusedVerifier.dll`.

Focused run command: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\\src\\WorldGenerationC06FocusedVerifier\\Terraria.WorldGenerationC06FocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit code: `0`; output: `C06 underground-desert focused verifier passed.`

## C07 WorldGenJungleStructureState

Checkpoint `C07` is complete as a design checkpoint. The 15 members are split by access pattern
into three proposed boundaries:

- `JungleRegionStructureComponent` owns jungle origin/range facts, hut/material selection, mud-wall
  mode, and the bounded extra-statue progress counter.
- `PyramidPlacementStateComponent` owns the pyramid count and paired coordinate scratch arrays.
  Version4 allocates these arrays from a pass-derived size, so the proposed component must preserve
  the observed capacity rather than inventing a fixed limit.
- `JungleChestAndLootGenerationStateComponent` owns chest coordinates/count and the generation-time
  item selection counters/flags. It emits item/structure intents but never owns inventory or NPC
  entities.

The `jungleHut` value is a selected tile/material definition ID at the legacy boundary, not a
general tile-write authority. The proposed core uses a stable definition value and an adapter for
legacy tile IDs.

### C07 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2170 | `numPyr` | `int` | `GenVars.cs:156` `public static int numPyr;` | Component -> `PyramidPlacementStateComponent` | pyramid structure pass and placement/finalization pass | reset and pyramid candidate recorder; complete writer set partial | per-generation used coordinate count | reservation/tile structure intents through ports | no direct save/network evidence |
| 2171 | `PyrX` | `int[]` | `GenVars.cs:158` `public static int[] PyrX;` | Component -> paired bounded pyramid coordinates | pyramid candidate and placement passes | reset/null then pass allocation/recorder; complete writer set partial | per-generation mutable scratch array | structure reservation/tile intents; snapshot copies array | no direct save/network evidence |
| 2172 | `PyrY` | `int[]` | `GenVars.cs:160` `public static int[] PyrY;` | Component -> paired bounded pyramid coordinates | pyramid candidate and placement passes | reset/null then pass allocation/recorder; complete writer set partial | per-generation mutable scratch array | structure reservation/tile intents; snapshot copies array | no direct save/network evidence |
| 2173 | `extraBastStatueCount` | `int` | `GenVars.cs:162` `public static int extraBastStatueCount;` | Component -> `JungleRegionStructureComponent` | jungle/bast statue generation and validation | reset and statue placement owner; complete writer set partial | per-generation progress counter | structure/item intent only through ports | no direct save/network evidence; progression/NPC seam |
| 2174 | `extraBastStatueCountMax` | `int` | `GenVars.cs:164` `public static int extraBastStatueCountMax;` | Component -> `JungleRegionStructureComponent` | jungle/bast statue generation and validation | reset and world-rule calculator; complete writer set partial | per-generation capacity/rule | none in component; placement eligibility query | no direct save/network evidence |
| 2175 | `jungleOriginX` | `int` | `GenVars.cs:166` `public static int jungleOriginX;` | Component -> `JungleRegionStructureComponent` | jungle, desert, pyramid, cave, and terrain passes | reset and jungle region calculator; complete writer set partial | per-generation region anchor | reservation/tile eligibility queries only | no direct save/network evidence |
| 2176 | `jungleMinX` | `int` | `GenVars.cs:168` `public static int jungleMinX;` | Component -> `JungleRegionStructureComponent` | jungle conversion and structure predicates | reset and jungle boundary pass; complete writer set partial | per-generation region bound | tile/structure intents through ports | no direct save/network evidence |
| 2177 | `jungleMaxX` | `int` | `GenVars.cs:170` `public static int jungleMaxX;` | Component -> `JungleRegionStructureComponent` | jungle conversion and structure predicates | reset and jungle boundary pass; complete writer set partial | per-generation region bound | tile/structure intents through ports | no direct save/network evidence |
| 2178 | `jungleHut` | `ushort` | `GenVars.cs:172` `public static ushort jungleHut;` | Definition -> `JungleHutMaterialDefinition` | jungle hut placement and tile conversion pass | reset RNG selection and definition mapping; complete writer set partial | per-generation selected tile/material definition | tile placement through tile commit port | no direct save/network evidence |
| 2179 | `mudWall` | `bool` | `GenVars.cs:174` `public static bool mudWall;` | Definition/flag -> `JungleRegionStructureComponent` | mud-wall and tile-runner passes | reset and jungle/material pass; complete writer set partial | per-generation material mode | tile material effects through tile commit port | no direct save/network evidence |
| 2180 | `JungleItemCount` | `int` | `GenVars.cs:176` `public static int JungleItemCount;` | Component -> `JungleChestAndLootGenerationStateComponent` | jungle chest item selector and chest placement pass | reset and item selector; complete writer set partial | per-generation loot-selection cursor | item intent only; no inventory mutation | no direct save/network evidence; item boundary |
| 2181 | `gennedLivingMahoganyWands` | `bool` | `GenVars.cs:178` `public static bool gennedLivingMahoganyWands;` | Component -> `JungleChestAndLootGenerationStateComponent` | living-mahogany item generation path and validation | reset and special item selection owner; complete writer set partial | per-generation unique-result flag | item generation command through explicit port | no direct save/network evidence; item boundary |
| 2182 | `JChestX` | `int[]` | `GenVars.cs:180` `public static int[] JChestX = new int[100];` | Component -> bounded paired jungle chest coordinates | jungle structure pass and chest placement pass | reset allocation and chest recorder; complete writer set partial | per-generation coordinate scratch, capacity 100 | chest placement through structure/item commit port | no direct save/network evidence |
| 2183 | `JChestY` | `int[]` | `GenVars.cs:182` `public static int[] JChestY = new int[100];` | Component -> bounded paired jungle chest coordinates | jungle structure pass and chest placement pass | reset allocation and chest recorder; complete writer set partial | per-generation coordinate scratch, capacity 100 | chest placement through structure/item commit port | no direct save/network evidence |
| 2184 | `numJChests` | `int` | `GenVars.cs:184` `public static int numJChests;` | Component -> bounded paired jungle chest coordinates | jungle structure pass and chest placement pass | reset and chest recorder; complete writer set partial | per-generation used coordinate count | chest placement after reservation/commit | no direct save/network evidence |

### C07 Ownership and Invariants

- `JungleRegionStructureSystem` owns region and structure-rule state; it does not own the tile grid,
  chest inventory, NPCs, or item definitions.
- Pyramid X/Y arrays are paired and indexed by `numPyr`; the dynamic allocation size from the legacy
  pass is part of the compatibility evidence. The proposed snapshot copies the used range and
  records capacity explicitly.
- Jungle chest X/Y arrays are paired with a fixed capacity of 100 in the declaration. The count must
  remain within capacity and chest placement must be reservation/commit ordered.
- `JungleItemCount` is a deterministic selection cursor. `gennedLivingMahoganyWands` is a generated
  result flag. Neither may be mapped directly to player inventory or NPC state.
- `jungleHut` and `mudWall` are material/definition inputs. They must not be mutated from a tile
  writer after the definition snapshot is committed.

### C07 Interface Contract and Dependency Impact

`IJungleRegionCalculator` (proposed) returns region/material facts from C02-C06 snapshots, world
rules, and RNG. `IPyramidCoordinateRecorder` and `IJungleChestCoordinateRecorder` are bounded
append/reject ports. `IJungleLootSelectionPolicy` is pure over an explicit cursor and content
definition snapshot. Structure, tile, item, NPC, storage, session, and progression effects remain
adapters or commit ports; unresolved ownership is `crossSubsystemOwner: integration-review`.

C07 consumes C02-C06 terrain and boundary facts and feeds dungeon, cave, chest/item, and structure
passes. P18-P20 generation execution and WorldProgressionAndTransition are integration consumers,
not owners assigned by this partition.

### C07 Focused Verification Plan

The fresh focused verifier covers the state/query boundaries, region generation-identity commit,
pyramid dynamic-capacity and paired-array commit, jungle chest fixed-capacity/count/paired-array
commit, cross-generation rejection, invalid capacity/count rejection, and owner-state preservation
after rejected commits. Snapshot arrays are copied and read-only; the verifier compares list-bearing
snapshots by value rather than relying on record-struct reference equality. Duplicate/reservation
ordering, loot-policy determinism, material mapping, tile/item/NPC effects, and legacy behavior
equivalence remain unverified and are not inferred.

### C07 Implementation Checkpoint

The C07 generation-identity commit boundaries are implemented in root `src`:

- `JungleRegionStructureSystem` validates the generation identity before replacing the complete
  region snapshot.
- `PyramidPlacementCommitSystem` validates generation identity, caller-preserved dynamic capacity,
  count bounds, and exact paired X/Y used ranges before replacing component-owned arrays.
- `JungleChestPlacementSystem` validates generation identity, fixed capacity 100, count bounds, and
  exact paired X/Y used ranges before replacing the loot cursor, wand flag, and component-owned
  chest arrays. The component copies input lists through temporary arrays before state replacement.
- `WorldSessionFocusedVerifier/Program.cs` exercises successful commits and rejected generation,
  capacity, and count inputs without changing the owner state.

  The serial build and no-build focused verifier both passed. The independently evidenced
  `JungleHutMaterialDefinition` and `JungleHutMaterialDefinitionQuery` preserve the selected hut
  tile ID and map the five sourced tile IDs to their wall IDs, with the source default of wall `0`
  for other values. No external material registration, RNG, duplicate policy, reservation, tile,
  item, NPC, persistence, or network behavior was inferred, and no legacy writer was changed or
  removed.

The independently evidenced `JungleRegionStructureQuery.IsWithinJungleConversionRange` preserves
the Version4 inclusive range predicate without normalizing reversed bounds. Its isolated serial
build and no-build run both passed:

- Build: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src\\WorldGenerationC07RegionFocusedVerifier\\Terraria.WorldGenerationC07RegionFocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; warnings/errors `0/0`; artifact `Build/bin/Terraria.WorldGenerationC07RegionFocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC07RegionFocusedVerifier.dll`.
- Focused run: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\\src\\WorldGenerationC07RegionFocusedVerifier\\Terraria.WorldGenerationC07RegionFocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; output `C07 jungle-region focused verifier passed.`

The independently evidenced jungle-boundary calculation is now implemented as a pure query over
explicit copied column observations. `JungleRegionBoundsCalculationInput` validates that the
observations cover the complete `maxTilesX` width and copies the source list before calculation.
`JungleRegionBoundsCalculationQuery` preserves the Version4 scan ranges exactly: the left scan
starts at `x = 5` and uses `x < maxTilesX - 5`, while the right scan starts at
`x = maxTilesX - 5` and uses `x > 5`. It returns the first left hit and first right-to-left hit,
and retains the sourced `0/0` defaults when no column matches. It does not read tiles or
`worldSurface`, mutate walls, consume RNG, or commit region state.

The calculation source files are:

- `src/WorldSession/WorldGeneration/Queries/JungleRegionBoundsCalculationInput.cs`
- `src/WorldSession/WorldGeneration/Queries/JungleRegionBoundsCalculationResult.cs`
- `src/WorldSession/WorldGeneration/Queries/JungleRegionBoundsCalculationQuery.cs`
- `src/WorldGenerationC07RegionFocusedVerifier/Program.cs`
- `src/WorldGenerationC07RegionFocusedVerifier/Terraria.WorldGenerationC07RegionFocusedVerifier.csproj`

The serial-wrapper build exited `0` with `0` warnings and `0` errors, producing
`Build/bin/Terraria.WorldGenerationC07RegionFocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC07RegionFocusedVerifier.dll`.
The no-build/no-restore focused run exited `0` with `C07 jungle-region focused verifier passed.`
The verifier covers the exact scan ranges, no-match defaults, copied input isolation, inclusive
conversion endpoints, and unchanged behavior for inverted committed ranges. The external tile-scan
adapter, `worldSurface` owner, wall mutation, RNG/world-rule owner, scheduler ordering, placement
effects, and legacy behavior equivalence remain unverified.

The material mapping verifier is isolated from the aggregate `WorldSession` project because that
shared checkout still contains unrelated duplicate C10 declarations. Its serial build and no-build
run both passed:

- Build: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src\\WorldGenerationC07MaterialFocusedVerifier\\Terraria.WorldGenerationC07MaterialFocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; warnings/errors `0/0`; artifact `Build/bin/Terraria.WorldGenerationC07MaterialFocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC07MaterialFocusedVerifier.dll`.
- Focused run: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\\src\\WorldGenerationC07MaterialFocusedVerifier\\Terraria.WorldGenerationC07MaterialFocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; output `C07 jungle-hut material focused verifier passed.`

The C07 pure mapping and range sources are saved in `src`: the definition and query files are
`WorldGeneration/Definitions/JungleHutMaterialDefinition.cs`,
`WorldGeneration/Queries/JungleHutMaterialDefinitionQuery.cs`, and the inclusive range method is
in `WorldGeneration/Queries/JungleRegionStructureQuery.cs`. The independent range verifier is
`src/WorldGenerationC07RegionFocusedVerifier/Program.cs` with its matching project file.

The independently evidenced base jungle-chest item selector and cursor transition are implemented
in `src`:

- `src/WorldSession/WorldGeneration/Queries/JungleChestLootSelectionQuery.cs` performs the
  `JungleItemCount % 4` cycle and the sourced 50/15/20 short-circuit result overrides using
  explicit random-roll input.
- `src/WorldSession/WorldGeneration/Queries/JungleChestLootSelectionRandomInput.cs` validates
  the three sourced roll ranges without reading a global random source.
- `src/WorldSession/WorldGeneration/Queries/JungleChestLootSelectionResult.cs` represents the
  selected item and next cursor as a pure result.
- `src/WorldSession/WorldGeneration/Systems/JungleChestLootSelectionSystem.cs` commits only the
  next cursor and preserves the wand flag and chest-coordinate state.
- `src/WorldGenerationC07LootFocusedVerifier/Program.cs` and
  `src/WorldGenerationC07LootFocusedVerifier/Terraria.WorldGenerationC07LootFocusedVerifier.csproj`
  verify the base cycle, override priority, cursor isolation, and invalid roll rejection.

The serial-wrapper build exited `0` with `0` warnings and `0` errors. The no-build/no-restore run
exited `0` with `C07 jungle-loot focused verifier passed.`. This proves the explicit-roll output
mapping and local cursor commit only; it does not prove exact random-source consumption order,
wand uniqueness, chest/item effect execution, or legacy behavior equivalence.

The independently evidenced jungle-hut material selection is implemented as a pure query over one
explicit roll. `JungleHutMaterialSelectionRandomInput` validates the sourced `[0, 5)` roll range;
`JungleHutMaterialSelectionQuery` maps rolls `0..4` to tile IDs `119/120/158/175/45` and reuses
the pure tile-to-wall definition query. It does not read global RNG, register external materials,
write tiles or walls, or commit region state. The serial-wrapper build exited `0` with `0` warnings
and `0` errors; the no-build/no-restore run exited `0` with
`C07 jungle-hut material focused verifier passed.`. This proves the explicit roll-to-definition
mapping only; RNG ownership, consumption order, external registration, writes, and legacy parity
remain pending.

## C08 GenVarsDungeonAndIslands

Checkpoint `C08` is complete as a design checkpoint. The 19 members are split into three proposed
boundaries because dungeon control, special dungeon rewards, and floating-island placement do not
share the same readers or effect policy:

- `DungeonLayoutControlComponent` owns the temporary dungeon bounds/room counters, altar
  coordinates, per-dungeon generation records, and the backing `_currentDungeon` index. The
  `dungeonGenVars` list is translated by an adapter; `CurrentDungeonGenVars` in C13 reads this
  authority and must not create a second list or index.
- `DungeonSpecialRewardGenerationComponent` owns the once-per-generation Shadow Key and Ram Rune
  result flags. It emits explicit item/progression intents and does not own item inventories or
  progression state.
- `FloatingIslandPlacementStateComponent` owns the target sky-lake multiplier/count, island-house
  counts, and paired island-house arrays. It emits bounded structure/tile intents and does not own
  sky rendering.

`dungeonBeachPadding` is a read-only generation definition. It remains separate from the mutable
dungeon layout component even though both are used by the same setup pass.

### C08 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2185 | `tLeft` | `int` | `GenVars.cs:186` `public static int tLeft;` | Component -> `DungeonLayoutControlComponent` | dungeon room/layout and tile placement passes | reset and dungeon generation result path; complete writer set partial | per-generation temporary dungeon bound | dungeon tile/structure intents through ports | no direct save/network evidence |
| 2186 | `tRight` | `int` | `GenVars.cs:188` `public static int tRight;` | Component -> `DungeonLayoutControlComponent` | dungeon room/layout and tile placement passes | reset and dungeon generation result path; complete writer set partial | per-generation temporary dungeon bound | dungeon tile/structure intents through ports | no direct save/network evidence |
| 2187 | `tTop` | `int` | `GenVars.cs:190` `public static int tTop;` | Component -> `DungeonLayoutControlComponent` | dungeon room/layout and tile placement passes | reset and dungeon generation result path; complete writer set partial | per-generation temporary dungeon bound | dungeon tile/structure intents through ports | no direct save/network evidence |
| 2188 | `tBottom` | `int` | `GenVars.cs:192` `public static int tBottom;` | Component -> `DungeonLayoutControlComponent` | dungeon room/layout and tile placement passes | reset and dungeon generation result path; complete writer set partial | per-generation temporary dungeon bound | dungeon tile/structure intents through ports | no direct save/network evidence |
| 2189 | `tRooms` | `int` | `GenVars.cs:194` `public static int tRooms;` | Component -> `DungeonLayoutControlComponent` | dungeon room generation and validation passes | reset and dungeon room recorder; complete writer set partial | per-generation room count | dungeon tile/structure intents through ports | no direct save/network evidence |
| 2190 | `lAltarX` | `int` | `GenVars.cs:196` `public static int lAltarX;` | Component -> `DungeonLayoutControlComponent` | altar tile placement and dungeon finalization | reset and altar placement path; complete writer set partial | per-generation altar coordinate | altar tile intent through tile commit port | no direct save/network evidence |
| 2191 | `lAltarY` | `int` | `GenVars.cs:198` `public static int lAltarY;` | Component -> `DungeonLayoutControlComponent` | altar tile placement and dungeon finalization | reset and altar placement path; complete writer set partial | per-generation altar coordinate | altar tile intent through tile commit port | no direct save/network evidence |
| 2192 | `dungeonGenVars` | `List<DungeonGenVars>` | `GenVars.cs:200` `public static List<DungeonGenVars> dungeonGenVars = new List<DungeonGenVars>();` | Component with adapter-owned per-dungeon records -> `DungeonLayoutControlComponent` | dungeon setup, crawler, biome, room, chest, and validation passes | reset/clear, setup, and dual-dungeon expansion paths; complete writer set partial | per-generation list of mutable dungeon records | dungeon generation commands through P18-P20 handoff/ports | no direct save/network evidence; WorldSession/P18-P20 seam |
| 2193 | `_currentDungeon` | `int` | `GenVars.cs:202` `private static int _currentDungeon;` | Component backing state -> `DungeonLayoutControlComponent` | public `CurrentDungeon` and `CurrentDungeonGenVars` accessors; dungeon passes | `CurrentDungeon` setter and reset/setup paths; complete writer set partial | per-generation active-record selector | selection command may change which record receives writes | no direct save/network evidence |
| 2194 | `dungeonBeachPadding` | `int` | `GenVars.cs:204` `public static readonly int dungeonBeachPadding = 50;` | Definition -> `DungeonBoundaryDefinition` | dungeon location selection and beach exclusion predicates | declaration initializer only | read-only generation definition | none in definition | no direct save/network evidence |
| 2195 | `skyLakes` | `int` | `GenVars.cs:206` `public static int skyLakes;` | Component -> `FloatingIslandPlacementStateComponent` | floating-island count/weight and island placement pass | reset and world-size/secret-seed adjustment path; complete writer set partial | per-generation target count/weight | island placement intents through reservation/tile ports | no direct save/network evidence |
| 2196 | `generatedShadowKey` | `bool` | `GenVars.cs:208` `public static bool generatedShadowKey;` | Component -> `DungeonSpecialRewardGenerationComponent` | dungeon chest loot selector and validation | reset and dungeon reward selection; complete writer set partial | per-generation unique reward result | item/progression intent through explicit port | no direct save/network evidence; WorldProgressionAndTransition seam |
| 2197 | `generatedRamRune` | `bool` | `GenVars.cs:210` `public static bool generatedRamRune;` | Component -> `DungeonSpecialRewardGenerationComponent` | dungeon chest loot selector and validation | reset and dungeon reward selection; complete writer set partial | per-generation unique reward result | item/progression intent through explicit port | no direct save/network evidence; WorldProgressionAndTransition seam |
| 2198 | `numIslandHouses` | `int` | `GenVars.cs:212` `public static int numIslandHouses;` | Component -> `FloatingIslandPlacementStateComponent` | island generation and later island-house placement | reset and island-house recorder; complete writer set partial | per-generation used-house count | structure/tile intents after bounds and reservation | no direct save/network evidence |
| 2199 | `skyIslandHouseCount` | `int` | `GenVars.cs:214` `public static int skyIslandHouseCount;` | Component -> `FloatingIslandPlacementStateComponent` | island-house placement policy and validation | reset and island-house placement path; complete writer set partial | per-generation generated-house progress/count | structure/tile intents through ports | no direct save/network evidence |
| 2200 | `skyLake` | `bool[]` | `GenVars.cs:216` `public static bool[] skyLake = new bool[300];` | Component -> paired bounded island-house metadata | island-house placement and finalization passes | reset allocation and island recorder; complete writer set partial | per-generation bounded metadata, capacity 300 | lake/island tile intents through tile commit port | no direct save/network evidence |
| 2201 | `floatingIslandHouseX` | `int[]` | `GenVars.cs:218` `public static int[] floatingIslandHouseX = new int[300];` | Component -> paired bounded island-house coordinates | island-house placement and `IslandHouse` pass | reset allocation and island recorder; complete writer set partial | per-generation coordinate scratch, capacity 300 | structure/tile intents through reservation/tile ports | no direct save/network evidence |
| 2202 | `floatingIslandHouseY` | `int[]` | `GenVars.cs:220` `public static int[] floatingIslandHouseY = new int[300];` | Component -> paired bounded island-house coordinates | island-house placement and `IslandHouse` pass | reset allocation and island recorder; complete writer set partial | per-generation coordinate scratch, capacity 300 | structure/tile intents through reservation/tile ports | no direct save/network evidence |
| 2203 | `floatingIslandStyle` | `int[]` | `GenVars.cs:222` `public static int[] floatingIslandStyle = new int[300];` | Component -> bounded island-house style metadata | island-house placement and `IslandHouse` pass | reset allocation and island recorder; complete writer set partial | per-generation style metadata, capacity 300 | style-specific structure/tile intents through ports | no direct save/network evidence |

### C08 Ownership and Invariants

- The dungeon record list and active index form one authority. `CurrentDungeon` changes the active
  record; `CurrentDungeonGenVars` is a read-only view, not a new component. The proposed owner must
  reject or explicitly report an index that is outside the current record list before dereference.
- Dungeon bounds and altar coordinates are temporary layout facts. They are not a persisted dungeon
  schema and do not authorize direct tile writes.
- `dungeonGenVars` is a mutable external record type at the legacy boundary. An adapter must copy
  or wrap only the fields needed by the proposed core, with a focused verifier for nested state and
  record switching. The full nested type is not silently flattened into P17.
- Shadow Key and Ram Rune flags are generation results with unique-result invariants. They are not
  inventory entries; item/progression consumers receive explicit commands after dungeon chest
  placement commits.
- `skyLake`, `floatingIslandHouseX`, `floatingIslandHouseY`, and `floatingIslandStyle` are paired
  by `numIslandHouses` and capacity 300. Snapshot and restore must preserve pairing and avoid live
  array aliasing.
- `skyLakes` is a generation target/weight and is not the same value as `numIslandHouses` or
  `skyIslandHouseCount`. No arithmetic relationship is assumed without pass evidence.

### C08 Interface Contract and Dependency Impact

`IDungeonLayoutOwner` (proposed) exposes explicit select-record and commit-layout operations;
`IDungeonRecordAdapter` translates `DungeonGenVars`; `IDungeonLayoutQuery` is pure over a committed
record snapshot. `IDungeonRewardResultSink` accepts explicit unique-reward commands. An
`IFloatingIslandPlacementRecorder` accepts bounded paired metadata and returns append/reject. All
structure/tile/item/progression effects cross commit ports.

C08 consumes C02-C07 terrain and boundary snapshots. C13 consumes its active-record snapshot for
pure derived properties. WorldSession, WorldStorage, WorldProgressionAndTransition, tile/liquid
simulation, and P18-P20 generation execution remain external; each unresolved ownership is
`crossSubsystemOwner: integration-review`.

### C08 Focused Verification Plan

Verify reset/clear order, record creation and active-index switching, lower-bound clamping and
upper-bound rejection, nested record snapshot isolation, dungeon bound/altar pairing, unique reward
flag transitions, island count/metadata capacity and pairing, duplicate/rejected reservations, and
reservation-before-tile commit ordering. Compare all 19 legacy values at dungeon and island pass
barriers. The existing `src/WorldGenerationC08FocusedVerifier` build and no-build run passed with
exit code `0`, `0` warnings, and `0` errors; it printed `C08 dungeon-island focused verifier
passed.`. The artifact is under
`Build/bin/Terraria.WorldGenerationC08FocusedVerifier/Debug/net10.0`.

### C08 Implementation Checkpoint

The independently evidenced C08 component boundaries are implemented in root `src`:

- `src/WorldSession/WorldGeneration/DungeonLayoutControlComponent.cs` owns the seven dungeon
  layout scalars, copied opaque record identifiers, and the active index.
- `src/WorldSession/WorldGeneration/DungeonSpecialRewardGenerationComponent.cs` owns the two
  unique reward result flags without owning inventory or progression effects.
- `src/WorldSession/WorldGeneration/FloatingIslandPlacementStateComponent.cs` owns the sky-lake
  values and capacity-300 paired island-house metadata; snapshots copy only the used entries.
- `src/WorldSession/WorldGeneration/DungeonBoundaryDefinition.cs` preserves the read-only
  `dungeonBeachPadding` definition.

The C08 focused verifier confirms copied state, generation identity, lower-bound selection, record
pairing, reward flags, and floating-island capacity/pairing. It does not prove external
`DungeonGenVars` ownership, dungeon tile/structure execution, secret-seed routing, persistence,
network, or legacy behavior equivalence. Those are integration boundaries, not missing component
members, and no additional C08 component was added in the current session.

## C09 GenVarsCaveTunnelAndOrePatchState

Checkpoint `C09` is complete as a design checkpoint. The nine members are split into three bounded
generation-history boundaries because their producers and consumers differ, even though all three
are transient X-coordinate or coordinate-pair scratch state:

- `MountainCaveHistoryComponent` owns the accepted mountain-cave count and paired X/Y origins. It
  publishes an immutable ordered view for Webs, Lakes, MountainCaveOpenings, and living-tree
  exclusion queries. It does not own the carved tile result produced by `Mountinater`,
  `CaveOpenater`, or `Cavinator`.
- `SurfaceTunnelHistoryComponent` owns the accepted tunnel count and center X origins. Its entries
  are the midpoint X values of the ten-point tunnel candidates and are consumed by the Lakes
  avoidance query. The tunnel TileRunner calls remain generation commands behind a tile commit port.
- `SurfaceOrePatchHistoryComponent` owns the accepted surface ore-patch count and X origins. It is
  consulted by the second surface-patch candidate loop for spacing and emits no ore tile mutation by
  itself. The selected Copper/Iron tile type remains a generation-time input from C01/C14's explicit
  selection boundary; it is not stored in this history component.

The three boundaries share a generation-session lifetime and reset barrier, but they must not be
collapsed into one generic `WorldGenerationScratchComponent`: Webs and tree exclusion need cave
coordinate pairs, Lakes need cave and tunnel X-only exclusion histories, and SurfaceOreAndStone needs
ore-patch X-only spacing with a different acceptance path. The arrays are compatibility projections of
these histories, not public mutable storage.

### C09 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2204 | `numMCaves` | `int` | `GenVars.cs:224` `public static int numMCaves;` | Component -> `MountainCaveHistoryComponent` | Webs override, Lakes avoidance, MountainCaveOpenings, living-tree exclusion | MountainCaves pass reset and accepted-cave append; complete writer set partial | per-generation used length for paired cave origins | no direct effect; gates dependent query/command generation | no direct save/network evidence; included by generic GenVars snapshot path pending characterization |
| 2205 | `mCaveX` | `int[]` | `GenVars.cs:226` `public static int[] mCaveX = new int[30];` | Component -> paired bounded mountain-cave X value | Webs, Lakes, MountainCaveOpenings, living-tree exclusion | MountainCaves accepted-cave append; no initialization reassignment at declaration beyond fixed capacity | per-generation coordinate scratch, capacity 30 | cave origin is input to explicit cave/web/tree tile commands; no live array exposure | no direct save/network evidence; snapshot aliasing and used-range policy remain open |
| 2206 | `mCaveY` | `int[]` | `GenVars.cs:228` `public static int[] mCaveY = new int[30];` | Component -> paired bounded mountain-cave Y value | Webs and MountainCaveOpenings; paired with `mCaveX` | MountainCaves accepted-cave append; no initialization reassignment at declaration beyond fixed capacity | per-generation coordinate scratch, capacity 30 | cave origin is input to explicit cave/web tile commands | no direct save/network evidence; snapshot aliasing and used-range policy remain open |
| 2207 | `maxTunnels` | `int` | `GenVars.cs:230` `public static readonly int maxTunnels = 50;` | Definition -> `SurfaceTunnelHistoryDefinition` | Tunnels capacity guard and bounded-history query | declaration initializer only; no component writer | immutable capacity definition; legacy pass uses `maxTunnels - 1` as stop threshold | none in definition | readonly and excluded from the writable GenVars snapshot field set |
| 2208 | `numTunnels` | `int` | `GenVars.cs:232` `public static int numTunnels;` | Component -> `SurfaceTunnelHistoryComponent` | Tunnels capacity guard/append and Lakes avoidance | Tunnels pass append after ten-point candidate acceptance; reset behavior is not explicit in the inspected `WorldGen.Reset` excerpt | per-generation used length for tunnel X history | no direct effect; gates dependent avoidance query | no direct save/network evidence; generic snapshot treatment pending reset characterization |
| 2209 | `tunnelX` | `int[]` | `GenVars.cs:234` `public static int[] tunnelX = new int[maxTunnels];` | Component -> bounded tunnel center-X history | Lakes avoidance and tunnel pass diagnostics | Tunnels accepted-candidate append; no declaration-time reassignment beyond fixed capacity | per-generation coordinate scratch, capacity 50, effective append threshold 49 in Version4 pass | tunnel history only feeds avoidance; paired TileRunner commands use local ten-point arrays | no direct save/network evidence; snapshot aliasing and capacity boundary remain open |
| 2210 | `maxOrePatch` | `int` | `GenVars.cs:236` `public static readonly int maxOrePatch = 50;` | Definition -> `SurfaceOrePatchHistoryDefinition` | SurfaceOreAndStone capacity guard | declaration initializer only; no component writer | immutable capacity definition; legacy pass uses `maxOrePatch - 1` as append threshold | none in definition | readonly and excluded from the writable GenVars snapshot field set |
| 2211 | `numOrePatch` | `int` | `GenVars.cs:238` `public static int numOrePatch;` | Component -> `SurfaceOrePatchHistoryComponent` | SurfaceOreAndStone first and second candidate loops | successful `OrePatch` candidate path increments only when below `maxOrePatch - 1`; reset behavior is not explicit in the inspected `WorldGen.Reset` excerpt | per-generation used length for surface ore-patch X history | no direct effect; controls spacing queries | no direct save/network evidence; generic snapshot treatment pending reset characterization |
| 2212 | `orePatchX` | `int[]` | `GenVars.cs:240` `public static int[] orePatchX = new int[maxOrePatch];` | Component -> bounded surface ore-patch X history | SurfaceOreAndStone candidate-spacing loops | successful `OrePatch` path appends X; `StonePatch` consumes history but does not append in the inspected code | per-generation coordinate scratch, capacity 50, effective append threshold 49 in Version4 pass | `OrePatch` itself writes ore and calls `OreHelper`; history owner only emits/accepts append facts | no direct save/network evidence; snapshot aliasing and used-range policy remain open |

### C09 Ownership and Invariants

- `numMCaves` is the authoritative used length for the paired `mCaveX`/`mCaveY` entries. The
  proposed recorder must reject an append when the used length is 30, preserve insertion order, and
  return a copied bounded view. Version4's inspected MountainCaves pass does not show an explicit
  `numMCaves < array.Length` guard before append, so the legacy overflow behavior and the intended
  migration policy require a focused characterization test; the new owner must not silently permit
  memory-unsafe behavior.
- MountainCaves resets `numMCaves` at the start of its pass, then calls `Mountinater` before recording
  the accepted origin. The migration must preserve that acceptance boundary: a failed or skipped
  candidate cannot enter history, and a command commit failure must not appear as a successful cave
  history result without an explicit transaction policy.
- `mCaveX` and `mCaveY` have different downstream access patterns from `tunnelX` and `orePatchX`.
  Webs uses the first `numMCaves` entries as exact X/Y overrides, MountainCaveOpenings consumes all
  accepted pairs, and Lakes/living-tree code uses only X-distance constraints. A projection can
  provide X-only predicates, but it must not replace the paired cave record.
- `numTunnels`/`tunnelX` are recorded after the ten-point surface scan succeeds and before the paired
  TileRunner calls. The Version4 pass stops before append when `numTunnels >= maxTunnels - 1`, so the
  proposed recorder must preserve the effective 49-entry boundary until a characterization test
  explicitly approves a different compatibility policy. The local point arrays used to carve each
  tunnel are not members of this component.
- `numOrePatch`/`orePatchX` are appended only after `OrePatch` returns true. `OrePatch` selects
  Copper/Iron from `SavedOreTiers`, writes the initial ore tile, extends the patch, and calls
  `OreHelper`; those effects belong to the ore/tile commit boundary, not to history ownership.
  `StonePatch` is only an `orePatchX` consumer in the inspected pass and must not be assumed to append
  a history entry.
- The strict spacing comparisons are part of the state contract: MountainCaves and Lakes compare
  cave/tunnel X values with `< 100`; living-tree exclusion uses an open `x - 50 < candidate < x + 50`
  range; SurfaceOreAndStone uses `< 200` for ore-patch candidates and `< 100` for stone candidates.
  These predicates belong in pure queries over immutable snapshots, not in mutable components.
- The capacities `maxTunnels` and `maxOrePatch` are readonly definitions. They must not be copied into
  writable state or treated as evidence that the corresponding histories are persistent. No direct
  save or network path was found for C09. The generic Version4 GenVars snapshot mechanism can include
  writable public members, but complete C09 reset/restore and array-reference behavior remains
  unverified because the converter implementation in this checkout is incomplete.

### C09 Interface Contract and Dependency Impact

`IMountainCaveHistoryRecorder` (proposed) accepts an in-bounds `(x, y)` origin after cave acceptance
and returns an append/reject result plus an immutable ordered snapshot. `IMountainCaveHistoryQuery`
provides pure pair lookup and X-distance exclusion predicates. `ISurfaceTunnelHistoryRecorder`
accepts a center X after the ten-point scan and exposes a bounded X snapshot. `ISurfaceOrePatchHistoryRecorder`
accepts an X only after the `OrePatch` commit boundary reports success. The three recorders should be
generation-session scoped and reset through an explicit `BeginGeneration`/reset operation, not static
global mutation.

`MountainCaveHistorySystem` owns candidate spacing, sand veto, recorder append, and the command batch
for `Mountinater`; `SurfaceTunnelHistorySystem` owns candidate scan, recorder append, and paired
TileRunner command batches; `SurfaceOreAndStoneSystem` owns candidate spacing and the command/result
transaction for `OrePatch`. Each system consumes C02-C05 facts and C01's generation ore selection
input as appropriate. Webs, Lakes, living-tree exclusion, and MountainCaveOpenings consume immutable
history snapshots. Tile, liquid, structure, NPC, item, storage, session, and P18-P20 execution remain
external effect boundaries with `crossSubsystemOwner: integration-review`.

Proposed order is: reset the three session histories -> MountainCaves record/commit -> dependent
Webs and Lakes X-distance queries -> Tunnels record/commit -> SurfaceOreAndStone candidate/ore
commit -> MountainCaveOpenings and later consumers according to the verified Version4 pass graph.
This is a provisional dependency order only; the scheduler must characterize the actual Version4
ordering before replacing legacy writers. File order must not define it.

### C09 Focused Verification Plan

Verify fixed capacities and effective `capacity - 1` stop thresholds, reset sentinels and fresh-array
allocation, paired cave coordinate order, bounded append/reject behavior, tunnel midpoint recording,
ore-patch append-after-success semantics, exact `<` spacing predicates, and immutable snapshot
isolation. Characterize whether `numTunnels` and `numOrePatch` are reset by an outer world-generation
barrier, how C09 arrays appear in `WorldGenSnapshot`, and whether a failed tile/ore commit rolls back
the corresponding history append. Compare all nine legacy values at Tunnels, MountainCaves, Lakes,
MountainCaveOpenings, Webs, and SurfaceOreAndStone barriers. The isolated C09 verifier now covers
bounded state and pure strict-distance behavior; legacy pass comparison and reset/restore
characterization remain unverified.

### C09 Implementation Checkpoint

The three independently evidenced C09 history boundaries are saved in root `src`:

- `MountainCaveHistoryComponent` and `MountainCaveHistorySnapshot` own a generation-scoped,
  capacity-30 ordered prefix of paired cave-origin X/Y values. `MountainCaveHistorySystem` exposes
  append and clear operations without calling `Mountinater`, tile, liquid, RNG, logging, persistence,
  or network APIs.
- `SurfaceTunnelHistoryDefinition`, `SurfaceTunnelHistoryComponent`, and
  `SurfaceTunnelHistorySnapshot` preserve the declared capacity 50 and the Version4 effective
  append capacity 49 for ordered tunnel-center X values. `SurfaceTunnelHistorySystem` and
  `SurfaceTunnelHistoryQuery` expose only bounded recording, clearing, and copied reads.
- `SurfaceOrePatchHistoryDefinition`, `SurfaceOrePatchHistoryComponent`, and
  `SurfaceOrePatchHistorySnapshot` preserve the declared capacity 50 and effective append capacity
  49 for ordered ore-patch X values. `SurfaceOrePatchHistorySystem` accepts a history append only
  when the caller supplies an explicit successful external patch result; its query exposes copied
  state only.

All three snapshots copy only their used prefixes into read-only lists. Appends at their effective
capacity are rejected without changing the owner, and clearing resets only the used count. Version4's
inspected passes record history around external `Mountinater`, ten-point tunnel, `TileRunner`, and
`OrePatch` effects; those effect boundaries are intentionally not implemented here. The absence of
legacy overflow/reset characterization, dependent-reader adapters, RNG ownership, saved-tier handoff,
and history-plus-tile/ore transaction policy remains an evidence gap. No legacy writer was changed or
removed. The isolated focused verifier build and run passed through the serial repository wrapper; the
full WorldSession build was separately blocked by duplicate C10 declarations already present in the
shared checkout.

## C10 GenVarsMushroomBiomeAndLogState

Checkpoint `C10` is complete as a design checkpoint. The five members are split into a bounded
mushroom-anchor history and a one-shot fallen-log handoff because their lifetimes and readers are
different:

- `MushroomBiomeAnchorStateComponent` owns the accepted underground mushroom-biome anchor count and
  paired `Point` positions. It is a generation-pass history used for distance exclusion within the
  mushroom pass; it does not own `ShroomPatch` tile mutations.
- `FallenLogFlowerHandoffComponent` owns the pending fallen-log coordinate pair. It is a single-use
  coordination value from `FallenLogsAndWaterFeatures` to `Flowers`, with `logX = -1` as the pending
  sentinel. It does not own the placed log tile, water features, flowers, tree effects, or items.

`maxMushroomBiomes` remains a read-only capacity definition. The surface-is-mushrooms secret-seed
conversion is a world rule and tile pass, not evidence that every mushroom tile belongs to this
component.

### C09 SurfaceOrePatchHistoryBoundary Implementation Checkpoint

The surface ore-patch history boundary is saved in root `src`:

- `src/WorldSession/WorldGeneration/SurfaceOrePatchHistoryDefinition.cs` defines the immutable
  capacity `50` and effective append limit `49`.
- `src/WorldSession/WorldGeneration/SurfaceOrePatchHistoryComponent.cs` owns the generation-scoped,
  ordered X-only history and rejects appends after the effective limit without mutation.
- `src/WorldSession/WorldGeneration/SurfaceOrePatchHistorySnapshot.cs` copies only the used prefix
  and preserves generation identity, declared capacity, effective limit, and count.
- `src/WorldSession/WorldGeneration/Queries/SurfaceOrePatchHistoryQuery.cs` exposes the copied
  snapshot; `src/WorldSession/WorldGeneration/Systems/SurfaceOrePatchHistorySystem.cs` appends only
  when the caller reports that the external patch commit succeeded, and exposes clear behavior.
- `src/WorldGenerationC09FocusedVerifier/Program.cs` contains focused assertions for the effective limit,
  append-after-success gate, insertion order, read-only snapshot data, and reset isolation.

- `src/WorldSession/WorldGeneration/Queries/MountainCaveHistoryQuery.cs` exposes copied cave history,
  and `src/WorldSession/WorldGeneration/Queries/CaveTunnelAvoidanceQuery.cs` provides pure strict-
  distance checks for cave, tunnel, and ore-patch snapshots.

No `OrePatch`, `StonePatch`, saved-tier, RNG, tile, liquid, persistence, network, or legacy-writer
integration was added. The isolated verifier build exited `0` with `0` warnings and `0` errors, and
the no-build focused run exited `0` with `C09 history focused verifier passed.`. A full WorldSession
build attempt exited `1` with `7` errors from duplicate C10 declarations in another shared-workspace
change; no C09 source error was reported by that attempt.

### C10 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2213 | `maxMushroomBiomes` | `int` | `GenVars.cs:242` `public static readonly int maxMushroomBiomes = 50;` | Definition -> `MushroomBiomeCapacityDefinition` | GlowingMushroomPatches target and append guard | declaration initializer only | immutable capacity definition, 50 anchors | none in definition | readonly and excluded from the writable GenVars snapshot field set |
| 2214 | `numMushroomBiomes` | `int` | `GenVars.cs:244` `public static int numMushroomBiomes = 0;` | Component -> `MushroomBiomeAnchorStateComponent` | GlowingMushroomPatches distance loop and capacity guard | GlowingMushroomPatches accepted-anchor append; no explicit reset found in the inspected `WorldGen.Reset` excerpt | per-generation used length for paired anchor positions | no direct effect; controls pure exclusion queries | no direct save/network evidence; generic GenVars snapshot inclusion and reset behavior remain to be characterized |
| 2215 | `mushroomBiomesPosition` | `Point[]` | `GenVars.cs:246` `public static Point[] mushroomBiomesPosition = new Point[maxMushroomBiomes];` | Component -> paired bounded mushroom-biome anchor positions | GlowingMushroomPatches distance checks | accepted-anchor append after center and five auxiliary `ShroomPatch` calls | per-generation coordinate scratch, capacity 50 | anchors feed candidate eligibility only; `ShroomPatch` tile writes cross a commit port | no direct save/network evidence; snapshot aliasing and used-range policy remain open |
| 2216 | `logX` | `int` | `GenVars.cs:248` `public static int logX;` | Component -> `FallenLogFlowerHandoffComponent` X coordinate and pending sentinel | Flowers in Remix and normal branches | WorldGen.Reset sets `-1`; FallenLogsAndWaterFeatures overwrites it after a successful type-488 placement with a 50% random acceptance; Flowers consumes and resets it to `-1` | one-shot per-generation pass handoff; `-1` means no pending log | coordinate selects the later flower-generation region; no direct tile or item effect in state | no direct save/network evidence; generic snapshot inclusion of a pending handoff requires characterization |
| 2217 | `logY` | `int` | `GenVars.cs:250` `public static int logY;` | Component -> paired `FallenLogFlowerHandoffComponent` Y coordinate | Flowers when `logX >= 0` | WorldGen.Reset sets `-1`; FallenLogsAndWaterFeatures writes it with `logX`; Flowers reads it but the inspected consumer clears only `logX` | paired coordinate whose validity is controlled by `logX`, not an independent fact | selects the later flower-generation region; no direct tile or item effect in state | no direct save/network evidence; stale post-consumption Y semantics must be preserved or explicitly adapted |

### C10 Ownership and Invariants

- `mushroomBiomesPosition` is indexed only through the used prefix `numMushroomBiomes`. The proposed
  owner must preserve anchor insertion order, pair each position with exactly one count increment,
  and expose copied immutable views. The fixed capacity is 50; the source also guards append with
  `numMushroomBiomes < maxMushroomBiomes`.
- The GlowingMushroomPatches pass computes a width-derived target capped by 50, retries candidate
  anchors while applying dungeon, existing mushroom tile, underground desert, and prior-anchor
  distance checks, then calls `ShroomPatch` at the anchor and five random offsets before recording
  the anchor. The recorder must not append a rejected candidate or claim that the anchor was
  committed when its tile command batch failed.
- Prior-anchor exclusion is a strict distance comparison against the stored `Point` values. It is a
  pure query over an immutable snapshot; the component does not own dungeon bounds, desert
  rectangles, world-rule flags, or tile classification registries.
- `logX` and `logY` are a pair. A successful type-488 placement may overwrite an earlier pending
  pair when its random branch selects the new log. Flowers consumes the latest pending pair and
  clears `logX` to `-1`; the inspected source does not clear `logY`, so the adapter must not silently
  use `logY` as a standalone pending flag or clear it without compatibility evidence.
- The pending log handoff is an ordering seam, not an entity, inventory, or persistent world object.
  FallenLogsAndWaterFeatures has separate liquid and tile behavior; Flowers has separate plant/tile
  behavior. Both effects remain outside `FallenLogFlowerHandoffComponent`.
- `numMushroomBiomes` and its array are initialized at declaration but no explicit reset assignment
  was found in the inspected `WorldGen.Reset` excerpt. The migration must characterize whether the
  outer generation lifecycle recreates this state. It must not assume that declaration-time zero is
  a sufficient per-world reset.
- No WorldFile or NetMessage path was found for C10. Public writable C10 members can be included by
  the generic Version4 GenVars snapshot reflection, while readonly capacity is excluded. Snapshot
  restore ordering, fresh-array allocation, and whether a pending log should be captured at every
  snapshot barrier require focused verification.

### C10 Implementation Checkpoint

The C10 state boundaries are saved in root `src`:

- `src/WorldSession/WorldGeneration/Components/MushroomBiomeAnchorStateComponent.cs` owns the
  generation-scoped, ordered 50-entry anchor history.
- `src/WorldSession/WorldGeneration/Components/MushroomBiomeAnchorStateSnapshot.cs` and
  `src/WorldSession/WorldGeneration/Queries/MushroomBiomeAnchorQuery.cs` expose a copied used
  prefix and strict Euclidean distance checks; `MushroomBiomeGenerationSystem` appends only after
  the caller reports a successful patch commit.
- `src/WorldSession/WorldGeneration/Components/FallenLogFlowerHandoffComponent.cs` owns the paired
  `logX`/`logY` handoff, preserving the `logX = -1` sentinel and stale `logY` after consumption.
  `FallenLogFlowerHandoffQuery` is read-only and `FallenLogFlowerHandoffSystem` makes publish,
  consume, and reset transitions explicit.

The source and focused assertions are saved. The isolated C10 verifier serial build exited `0` with
`0` warnings and `0` errors, and the no-build focused run exited `0` with
`C10 mushroom and fallen-log focused verifier passed.`. Tile/liquid effects, RNG, external pass
transactionality, persistence/network integration, and legacy-writer replacement remain outside this
checkpoint.

### C10 Interface Contract and Dependency Impact

`IMushroomBiomeAnchorRecorder` (proposed) accepts a candidate anchor only after pure eligibility
checks and the six `ShroomPatch` operations produce a successful command result. It returns an
append/reject result and an immutable ordered view. `IMushroomBiomeAnchorQuery` provides pure prior
anchor distance checks. `IFallenLogHandoffPort` (proposed) records or overwrites a paired coordinate
with the source random-selection result, and `IFallenLogConsumer` consumes it once while preserving
the `logX = -1` sentinel and stale-`logY` compatibility semantics.

`MushroomBiomeGenerationSystem` owns candidate selection and anchor history; `FallenLogGenerationSystem`
owns log-placement result production; `FlowerGenerationSystem` consumes a handoff snapshot and emits
tile commands. `MushroomTileCommitPort`, `LiquidSimulation`, and plant/tile commit adapters own all
effects. C10 consumes C02-C08 terrain, dungeon, desert, and world-rule facts and feeds later flower,
plant, and world-generation execution. External tile/liquid, storage, session, NPC, progression, and
P18-P20 ownership remains `crossSubsystemOwner: integration-review`.

The provisional pass order is reset -> GlowingMushroomPatches anchor history and tile commands ->
FallenLogsAndWaterFeatures log handoff plus separate liquid/tile commands -> Flowers one-shot handoff
consumption -> later Mushrooms and plant passes. The scheduler must preserve this dependency
explicitly; file order must not define it.

### C10 Focused Verification Plan

Verify the 50-entry mushroom capacity, used-prefix and `Point` pairing, strict prior-anchor distance,
candidate retry limit, six-patch acceptance ordering, and reset/snapshot behavior. Verify the log
handoff sentinel, overwrite-last-selected behavior, Remix and normal Flowers consumption, exact
one-shot clearing of `logX`, preservation of `logY` after consumption, and no consumption when the
fallen-log placement fails. Compare all five legacy values at GlowingMushroomPatches,
FallenLogsAndWaterFeatures, and Flowers barriers. The isolated verifier build exited `0` with `0`
warnings and `0` errors, and its no-build focused run exited `0` with
`C10 mushroom and fallen-log focused verifier passed.`. Legacy pass comparison and external effects
remain unverified.

### C10 Implementation Checkpoint

The C10 bounded state boundaries are saved in root `src`:

- `src/WorldSession/WorldGeneration/Components/MushroomBiomeAnchorStateComponent.cs`,
  `MushroomBiomeAnchorStateSnapshot.cs`, and `MushroomBiomeCapacityDefinition.cs` preserve a
  capacity-50 ordered anchor prefix. `MushroomBiomeGenerationSystem` appends only after an explicit
  successful patch-commit result, and `MushroomBiomeAnchorQuery` performs strict Euclidean distance
  checks over copied state.
- `src/WorldSession/WorldGeneration/Components/FallenLogFlowerHandoffComponent.cs` and
  `FallenLogFlowerHandoffSnapshot.cs` preserve the generation-scoped paired `logX`/`logY` handoff.
  `FallenLogFlowerHandoffSystem.TryPublish` requires placement success and random-selection
  acceptance, later accepted publications overwrite the pair, and `TryConsume` clears only `LogX`
  while preserving stale `LogY`; `Reset` clears both coordinates.
- `src/WorldSession/WorldGeneration/Queries/FallenLogFlowerHandoffQuery.cs` exposes a copied
  value snapshot, and `src/WorldGenerationC10FocusedVerifier/Program.cs` contains focused assertions
  for both boundaries. No legacy writer or external tile/liquid/RNG/Flowers integration was changed.

## C11 GenVarsLakeAndOasisState

Checkpoint `C11` is a partial implementation checkpoint. The eight members are split into independent
generation histories for lakes and oases, with readonly definitions kept outside mutable state:

- `LakePlacementHistoryComponent` owns the ordered X-only lake history and its used count. It is
  consulted by the Lakes pass for horizontal spacing and does not own `SonOfLakinater` tile/liquid
  effects.
- `OasisPlacementHistoryComponent` owns accepted oasis centers, widths, and the used count. It is
  read by later cactus and oasis-plant passes and does not own `PlaceOasis` tile/liquid mutations.
- `LakePlacementCapacityDefinition`, `OasisPlacementCapacityDefinition`, and
  `OasisHeightDefinition` expose the three readonly constants without making them writable
  component state.

### C11 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2218 | `maxLakes` | `int` | `GenVars.cs:252` `public static readonly int maxLakes = 50;` | Definition -> `LakePlacementCapacityDefinition` | Lakes pass append guard | declaration initializer only | immutable declared lake capacity, 50 | none in definition | readonly and excluded from writable GenVars snapshot fields |
| 2219 | `numLakes` | `int` | `GenVars.cs:254` `public static int numLakes = 0;` | Component -> `LakePlacementHistoryComponent` | Lakes pass guard and used-prefix loop | Lakes pass increments after `SonOfLakinater` | generation-session used length; explicit Reset assignment not found in inspected excerpt | controls candidate exclusion only | no direct WorldFile/NetMessage path found; generic snapshot inclusion and reset behavior remain to characterize |
| 2220 | `LakeX` | `int[]` | `GenVars.cs:256` `public static int[] LakeX = new int[maxLakes];` | Component -> paired-free X history in `LakePlacementHistoryComponent` | Lakes pass strict `< 150` horizontal spacing loop | Lakes pass writes at `LakeX[numLakes]` before increment | ordered used prefix, declared capacity 50, effective pass stop at 49 entries | history gates later candidate placement; `SonOfLakinater` effects remain external | generic snapshot may capture mutable array; copy/aliasing behavior remains open |
| 2221 | `maxOasis` | `int` | `GenVars.cs:258` `public static readonly int maxOasis = 20;` | Definition -> `OasisPlacementCapacityDefinition` | `PlaceOasis` append guard | declaration initializer only | immutable declared oasis capacity, 20 | none in definition | readonly and excluded from writable GenVars snapshot fields |
| 2222 | `numOasis` | `int` | `GenVars.cs:260` `public static int numOasis = 0;` | Component -> `OasisPlacementHistoryComponent` | `PlaceOasis` spacing/append guard, oasis helpers, CactusPalmTreesAndCoral iteration | `PlaceOasis` increments after terrain/liquid mutation when below capacity | generation-session used length; explicit Reset assignment not found in inspected excerpt | controls candidate exclusion and downstream plant region count | no direct tile/liquid effect in the counter | no direct WorldFile/NetMessage path found; generic snapshot inclusion and reset behavior remain to characterize |
| 2223 | `oasisPosition` | `Point[]` | `GenVars.cs:262` `public static Point[] oasisPosition = new Point[maxOasis];` | Component -> paired oasis center history in `OasisPlacementHistoryComponent` | `PlaceOasis` strict `< 350` center spacing and CactusPalmTreesAndCoral | `PlaceOasis` writes the accepted center before count increment | ordered used prefix, capacity 20 | center drives later plant placement region; oasis terrain/liquid remains external | generic snapshot may capture mutable array; copied snapshots are required |
| 2224 | `oasisWidth` | `int[]` | `GenVars.cs:264` `public static int[] oasisWidth = new int[maxOasis];` | Component -> width paired by oasis index | CactusPalmTreesAndCoral expands each width by 1.5; `PlaceOasis` uses the local random width before recording | `PlaceOasis` writes the width before count increment | width paired with `oasisPosition`, random source range 45..60 inclusive | width changes the later plant iteration bounds, not the component itself | generic snapshot may capture mutable array; pairing and used-range restore remain open |
| 2225 | `oasisHeight` | `int` | `GenVars.cs:266` `public static readonly int oasisHeight = 20;` | Definition -> `OasisHeightDefinition` | `PlaceOasis` geometry and CactusPalmTreesAndCoral vertical range | declaration initializer only | immutable oasis vertical extent, 20 | none in definition | readonly and excluded from writable GenVars snapshot fields |

### C11 Ownership and Invariants

- The lake and oasis histories are not one generic water component. Lakes are an X-only candidate
  history consumed inside the Lakes pass; oases are paired center/width records consumed later by
  the cactus and plant pass. Their readers, append timing, and effective capacity rules differ.
- `maxLakes` is declared as 50, but the Lakes pass stops before attempting another candidate when
  `numLakes >= maxLakes - 1`. The compatible pass therefore records at most 49 X values unless a
  future characterization proves that an outer lifecycle changes this boundary. Preserve this
  apparent off-by-one behavior until a focused verifier explains it.
- A lake candidate is rejected when its X is within strict `< 150` distance of a prior `LakeX`, or
  within strict `< 100` distance of a C09 mountain-cave or surface-tunnel X. The C11 query may read
  immutable C09 snapshots, but neither history component may own the other component's arrays.
- The lake source calls `SonOfLakinater(num4, num5)` before appending `LakeX[numLakes]` and
  incrementing `numLakes`. The proposed system must make the terrain/liquid commit result explicit;
  it must not claim a lake history append when the commit adapter reports failure or retry.
- `PlaceOasis` rejects candidates around existing oasis centers with strict `< 350` distance,
  chooses `num2` from `genRand.Next(45, 61)`, performs extensive tile and liquid mutation, and
  appends `oasisPosition` and `oasisWidth` only afterward when `numOasis < maxOasis`. Once the
  history is full, the source can still mutate tiles and return `true` without recording metadata;
  this asymmetry is a required characterization point, not a license to silently reorder the guard.
- `oasisPosition`, `oasisWidth`, and `numOasis` are one paired used-prefix record. The later
  CactusPalmTreesAndCoral pass expands each stored width by 1.5 and iterates vertical offsets of
  `oasisHeight` (20), so snapshots must preserve index pairing and exact width/height values.
- The inspected `WorldGen.Reset` excerpt does not assign `numLakes`, `LakeX`, `numOasis`,
  `oasisPosition`, or `oasisWidth`. Their declaration-time initializers are not evidence of a
  per-world reset. The migration must characterize repeated generation, snapshot restore, and
  allocation/aliasing before introducing a new reset owner.
- `WorldGenSnapshot.SnapshotGenVars` reflects public static writable fields and non-readonly fields,
  and `Restore` calls `WorldGen.Reset()` before applying the serialized GenVars. C11 arrays and
  counts therefore require explicit fresh-copy and restore-order tests; readonly definitions are
  excluded by the converter filters.
- Current NLTX contains `OasisPlantValidationQuery`, which is a pure tile query and not an oasis
  history owner. It can remain a downstream validation seam and must not be expanded into a
  catch-all C11 component.

### C11 Interface Contract and Dependency Impact

`ILakePlacementHistory` (proposed) exposes `Count`, an immutable used-prefix X snapshot, and an
append result with explicit full/rejected status. `IOasisPlacementHistory` exposes immutable paired
center/width records, capacity, and the same append result. `ILakeOasisAvoidanceQuery` reads C04/C05
boundary facts plus C09 cave/tunnel histories and returns pure eligibility decisions; it does not
write either history.

`LakeGenerationSystem` owns candidate selection and the `SonOfLakinater` command boundary.
`OasisGenerationSystem` owns candidate selection and the `PlaceOasis` terrain/liquid command
boundary. `OasisVegetationSystem` reads the committed oasis snapshot for cactus and plant intents.
`LakeTerrainLiquidCommitPort` and `OasisTerrainLiquidCommitPort` own all tile/liquid effects.
WorldStorage, LiquidSimulation, WorldSession, item/entity effects, and P18-P20 generation execution
remain `crossSubsystemOwner: integration-review`.

The provisional pass order is C09 history snapshot -> Lakes eligibility -> lake terrain/liquid
commit -> LakeX append -> Oasis eligibility/terrain-liquid commit -> oasis metadata append ->
CactusPalmTreesAndCoral read-only vegetation intents. The scheduler must express this order
explicitly; document order and file order do not define execution order.

### C11 Focused Verification Plan

Verify the declared capacities and the effective 49-entry lake stop, used-prefix preservation,
strict `< 150` LakeX spacing, strict `< 100` C09 cave/tunnel avoidance, and append-after-
`SonOfLakinater` behavior. Verify oasis center spacing `< 350`, width range 45..60, capacity-20
paired center/width records, append-after-`PlaceOasis` behavior, and the full-capacity case where
the source mutates terrain without recording a new metadata entry. Verify CactusPalmTreesAndCoral
reads copied center/width/height values without mutating the history. Characterize reset and
WorldGenSnapshot restore for all eight members and compare them at the Lakes, Oasis,
CactusPalmTreesAndCoral, and snapshot barriers. The isolated verifier build exited `0` with `0`
warnings and `0` errors, and its no-build focused run exited `0` with
`C11 lake and oasis focused verifier passed.`. Full reset/restore, external effects, persistence,
network, and legacy parity remain unverified.

### C11 Lake and Oasis History Implementation Checkpoint

The independently evidenced C11 state, query, and commit-result boundaries are saved in root `src`:

The append result/status types are saved under the System ownership boundary:
`src/WorldSession/WorldGeneration/Systems/LakePlacementHistoryAppendResult.cs`,
`LakePlacementHistoryAppendStatus.cs`, `OasisPlacementHistoryAppendResult.cs`, and
`OasisPlacementHistoryAppendStatus.cs`. The focused verifier compile list references these
System paths; no duplicate result types remain under `Components`.

- `src/WorldSession/WorldGeneration/Components/LakePlacementCapacityDefinition.cs` preserves
  the declared capacity `50` and the source pass stop at `maxLakes - 1`, exposed as an effective
  entry limit of `49`.
- `src/WorldSession/WorldGeneration/Components/LakePlacementHistoryComponent.cs` owns a
  generation-scoped, ordered X-only used prefix. `LakeGenerationSystem.TryAppendAfterSuccessfulCommit`
  records only after an explicit external terrain/liquid success result.
- `src/WorldSession/WorldGeneration/Components/OasisPlacementCapacityDefinition.cs` and
  `OasisHeightDefinition.cs` preserve the declared oasis capacity `20` and height `20`.
- `src/WorldSession/WorldGeneration/Components/OasisPlacementHistoryComponent.cs` owns paired
  center/width records, validates the evidenced width range `45..60`, and rejects after capacity
  without changing the used prefix. `OasisGenerationSystem.TryAppendAfterSuccessfulCommit` gates
  metadata recording on the explicit external commit result.
- The four C11 snapshots/queries expose copied read-only used prefixes. `LakeOasisAvoidanceQuery`
  and `OasisPlacementQuery` perform pure strict-distance reads, and both systems expose clear/reset
  of the mutable used counts.
- `src/WorldGenerationC11FocusedVerifier/Program.cs` contains capacity, ordering, strict-distance,
  mixed-generation, append-after-success, full-capacity, paired-list, and snapshot-isolation
  assertions. The fixed width assertion checks the Version4 `45..60` range. The isolated verifier
  serial build exited `0` with `0` warnings and `0` errors, and its no-build focused run exited `0`
  with `C11 lake and oasis focused verifier passed.`.

This checkpoint does not implement lake/oasis candidate scanning, RNG, `SonOfLakinater`,
`PlaceOasis`, terrain or liquid mutation, reset ownership, WorldGenSnapshot restore, downstream
vegetation, persistence, network, or legacy-writer replacement. Those remain evidence gaps and
integration-review boundaries.

## C12 GenVarsHellAndSpecialStructures

Checkpoint `C12` is complete as a design checkpoint. The nine members are split by lifecycle and
reader rather than placed in one broad special-structures component:

- `HellChestLootCycleComponent` owns the shuffled hell-chest item sequence and the current cyclic
  item cursor. It does not own Chest instances, item stacks, or item/progression effects.
- `StatuePlacementCatalogDefinition` and `StatueTrapSelectionDefinition` own read-only catalog
  data and trap-index rules. The trap list indexes `statueList`; it is not a list of tile IDs.
- `InfectionAlignmentComponent` owns the generation-time evil-side orientation and the infection
  reversal input. It does not own converted tiles or world evil persistence.
- `ShimmerBiomeAnchorComponent` owns the generation anchor used by later exclusion queries. The
  protected-structure reservation and shimmer/liquid/tile effects remain explicit external seams.
- `SpecialSeedGenerationRuleFlagsComponent` owns the two combined seed-option gates that are
  computed at reset and read by later generation passes.

### C12 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2226 | `hellChest` | `int` | `GenVars.cs:268` `public static int hellChest;` | Component -> `HellChestLootCycleComponent.CurrentIndex` | `WorldGen.PlaceChest` hell-chest branch | `PlaceChest` increments only after a successful `PlaceChest` call and wraps at `hellChestItem.Length`; no explicit reset assignment found in the inspected `WorldGen.Reset` excerpt | cyclic generation-session cursor; default static value is 0 | selects the next generated hell-chest item; Chest/item creation remains external | no direct WorldFile/NetMessage path; writable GenVars snapshot inclusion and repeat-generation reset remain to characterize |
| 2227 | `hellChestItem` | `int[]` | `GenVars.cs:270` `public static int[] hellChestItem;` | Component -> shuffled `HellChestLootCycleComponent.ItemSequence` | `WorldGen.PlaceChest` reads the current item | `WorldGen.Reset` shuffles five normal or Remix-specific item IDs and assigns a fresh array | generation-session item sequence; permutation length is five in the inspected reset path | item choice is passed to Chest/item effects, not owned by the component | no direct WorldFile/NetMessage path; generic snapshot array copy/restore remains open |
| 2228 | `statueList` | `Point16[]` | `GenVars.cs:272` `public static Point16[] statueList;` | Definition -> `StatuePlacementCatalogDefinition` | `Statue` helper and `Statues` generation pass | `SetupStatueList` constructs and assigns the catalog during `WorldGen.Reset` | fixed ordered tile-type/style catalog; each pair is an option record | statue tile placement and trap wiring remain external commit effects | no direct WorldFile/NetMessage path; generic snapshot inclusion of the public array remains open |
| 2229 | `StatuesWithTraps` | `List<int>` | `GenVars.cs:274` `public static List<int> StatuesWithTraps = new List<int>(new int[4] { 4, 7, 10, 18 });` | Definition -> `StatueTrapSelectionDefinition` | `Statues` pass membership check | declaration initializer only in inspected Version4 sources; no runtime writer found | trap-selection rule keyed by `statueList` index | `PlaceStatueTrap` and wire/pressure-plate effects remain external | no direct WorldFile/NetMessage path; mutable-list snapshot aliasing and mod-facing mutability remain open |
| 2230 | `crimsonLeft` | `bool` | `GenVars.cs:276` `public static bool crimsonLeft = true;` | Component -> `InfectionAlignmentComponent.CrimsonLeft` | infection conversion passes and Remix chest/biome branching | `WorldGen.Reset` selects false/true from `genRand.Next(2)` | per-generation evil-side orientation | controls which half is converted or selected; tile and chest effects remain external | no direct WorldFile/NetMessage path; generic snapshot/reset behavior remains open |
| 2231 | `shimmerPosition` | `Vector2D` | `GenVars.cs:278` `public static Vector2D shimmerPosition;` | Component -> `ShimmerBiomeAnchorComponent.Position` | generation exclusion loops, moss/tree/structure checks, and `ShimmerRemoveWater` | world-clear reset sets `Vector2D.Zero`; `Shimmer` pass assigns after `ShimmerMakeBiome` succeeds | generation anchor, with zero as the clear-world default | `ShimmerMakeBiome`, protected-structure registration, liquid removal, and tile effects remain external | no direct WorldFile/NetMessage path; generic snapshot position restore and protected-structure ordering remain open |
| 2232 | `notTheBeesAndForTheWorthyNoCelebration` | `bool` | `GenVars.cs:280` public static bool | Component -> `SpecialSeedGenerationRuleFlagsComponent` | bee/For-the-Worthy combined generation branches | `WorldGen.Reset` computes `Main.notTheBeesWorld && Main.getGoodWorld && !Main.tenthAnniversaryWorld` | reset-derived generation rule flag | changes wall/tile/pass behavior through explicit generation systems | no direct WorldFile/NetMessage path; recomputation after reset and snapshot restore remain open |
| 2233 | `noTrapsAndForTheWorthyNoCelebration` | `bool` | `GenVars.cs:282` public static bool | Component -> `SpecialSeedGenerationRuleFlagsComponent` | spike-cave and related no-trap generation branches | `WorldGen.Reset` computes `Main.noTrapsWorld && Main.getGoodWorld && !Main.tenthAnniversaryWorld` | reset-derived generation rule flag | changes generation pass selection; trap/tile effects remain external | no direct WorldFile/NetMessage path; recomputation after reset and snapshot restore remain open |
| 2234 | `flipInfections` | `bool` | `GenVars.cs:284` public static bool | Component -> `InfectionAlignmentComponent.FlipInfections` | evil-biome conversion and heart-placement branches | `WorldGen.Reset` computes `Main.drunkWorld && Main.getGoodWorld && !Main.remixWorld` | reset-derived infection rule flag | changes conversion side and whether normal/Crimson placement calls run; tile effects remain external | no direct WorldFile/NetMessage path; generic snapshot inclusion and seed-option dependency remain open |

### C12 Ownership and Invariants

- `hellChestItem` is a five-entry shuffled sequence. The normal reset source starts with item IDs
  `{274, 220, 112, 218, 3019}`; the Remix source uses `{274, 220, 683, 218, 3019}` before
  shuffling. `hellChest` selects the current entry and advances only after a successful chest
  placement, wrapping to zero at the sequence length. A failed placement must not consume an item.
- The inspected `WorldGen.Reset` excerpt assigns `hellChestItem` but does not explicitly assign
  `hellChest`. The proposed owner must characterize repeated generation and snapshot restore before
  introducing a reset write; declaration-time zero is not evidence of a per-world reset.
- `SetupStatueList` rebuilds the ordered `Point16` catalog during reset. The same index is later
  used to select a statue tile/style and to test `StatuesWithTraps`. Preserve catalog order and
  index identity; a tile-ID set or unordered collection would change behavior.
- The local tModLoader `v2026.07` mirror confirms `GenVars.statueList` as statue options where each
  `Point16` is a tile type/style pair, and documents `GenVars.StatuesWithTraps` as statue-list
  indexes that receive a connecting wire and pressure plate. This is public API evidence only;
  Version4 pass order and reset behavior remain sourced from `D:\TRbackup\Version4`.
- `crimsonLeft` is randomized once per reset and is read by both infection generation and Remix
  branching. It is a generation alignment input, not a persisted world-evil component. The
  proposed component must expose a stable read-only snapshot to all consumers.
- `shimmerPosition` is cleared to zero by the world-clear reset path, then assigned only after the
  Shimmer pass's retry loop obtains a successful `ShimmerMakeBiome` result. The source immediately
  adds a protected structure around the anchor. The anchor and reservation must be committed in an
  explicit order, while Shimmer tile/liquid work remains outside the state component.
- The two `*AndForTheWorthyNoCelebration` flags and `flipInfections` are derived from Main seed
  options at reset. They are rule inputs, not independent mutable facts. Replacing them with a
  generic secret-seed lookup without preserving the exact conjunctions would alter generation.
- No direct WorldFile or NetMessage read/write was found for the nine C12 members. The generic
  `WorldGenSnapshot.SnapshotGenVars` reflection considers public writable fields and non-readonly
  fields, so arrays, lists, cursors, coordinates, and flags may still cross snapshot boundaries.
  `WorldGenSnapshot.Restore` calls `WorldGen.Reset` before applying serialized GenVars; fresh-copy,
  list aliasing, and rule recomputation must therefore be tested rather than assumed.

### C12 Interface Contract and Dependency Impact

`IHellChestLootCycle` exposes an immutable item-sequence snapshot, current index, and an explicit
successful-placement advance operation. `IStatuePlacementCatalog` exposes copied ordered
`Point16` options; `IStatueTrapRule` performs a pure index membership query. Neither interface
exposes tile mutation or wire placement.

`IInfectionAlignmentQuery` returns `CrimsonLeft` and `FlipInfections` as one read-only generation
snapshot. `ISpecialSeedGenerationRuleQuery` returns the two combined rule flags. `IShimmerAnchor`
exposes the current anchor and reset state, while `IShimmerBiomeCommitPort` returns an explicit
anchor/terrain result before `StructureReservationPort` records the protected area.

`GenerationRuleInitializationSystem` owns reset-derived flags and catalogs, `HellChestGenerationSystem`
owns the loot cursor, `StatueGenerationSystem` owns catalog consumption, `InfectionGenerationSystem`
owns conversion commands, and `ShimmerGenerationSystem` owns candidate selection and anchor output.
Chest/item, tile, wire, liquid, structure, NPC, progression, storage, session, and P18-P20 effects
remain external adapters or integration-owned systems. `crossSubsystemOwner: integration-review`
remains required for those handoffs.

The provisional dependency order is reset-derived rule/catalog initialization -> infection and
shimmer generation inputs -> Shimmer biome commit -> protected-structure reservation -> statue and
hell-chest placement systems. The scheduler must express actual pass dependencies explicitly;
document or file order does not define runtime order.

### C12 Focused Verification Plan

Verify normal and Remix hell-chest permutations, cursor initialization, no-advance-on-failure, and
wrap-after-success. Verify exact `SetupStatueList` order, copied catalog isolation, trap index
membership, and no tile/wire mutation from a pure catalog query. Verify deterministic
`crimsonLeft`/flag derivation for every relevant seed-option combination and the infection branch
selection. Verify Shimmer retry/commit behavior, zero reset, protected-structure registration
ordering, and exclusion distance reads. Verify generic snapshot serialization/restoration for all
nine members, including fresh arrays, list copies, cursor state, `Vector2D`, and reset-before-
restore semantics. Compare all nine legacy values at reset, Shimmer, Statues, chest-placement,
infection, and snapshot barriers. The existing `src/WorldGenerationC12FocusedVerifier` build and
no-build run passed with exit code `0`, `0` warnings, and `0` errors; it printed `C12 hell and
special-structure focused verifier passed.`. The current session also added only the component API
compatibility overload in `HellChestLootCycleComponent`; it did not add or modify a verifier.

### C12 Hell and Special Structures Implementation Checkpoint

The independently evidenced C12 state, query, and commit boundaries are saved in root `src`:

- `HellChestLootCycleComponent`, `HellChestLootCycleSnapshot`, `HellChestLootCycleQuery`, and
  `HellChestGenerationSystem` own a copied five-entry normal/Remix pool permutation and cyclic
  cursor. The cursor advances only after an explicit successful chest-placement result and wraps
  to zero. The system does not create chests, items, or progression effects.
- `StatuePlacementOption`, `StatuePlacementCatalogDefinition`,
  `StatueTrapSelectionDefinition`, `StatuePlacementCatalogQuery`, `StatueTrapRuleQuery`, and
  `GenerationRuleInitializationSystem` preserve the 73-entry Version4 ordered tile/style catalog
  and the trap index set `{4, 7, 10, 18}` as copied read-only definitions.
- `InfectionAlignmentComponent`, `InfectionAlignmentSnapshot`, `InfectionAlignmentQuery`, and
  `InfectionGenerationSystem` preserve `crimsonLeft` and the exact
  `drunkWorld && getGoodWorld && !remixWorld` flip conjunction.
- `SpecialSeedGenerationRuleFlagsComponent`, its snapshot/query, and
  `GenerationRuleInitializationSystem` preserve the exact two `*AndForTheWorthyNoCelebration`
  conjunctions.
- `ShimmerBiomeAnchorComponent`, `ShimmerBiomeAnchorPoint`, its snapshot/query, and
  `ShimmerGenerationSystem` publish an anchor only after an explicit successful biome commit,
  support strict-distance exclusion reads, and clear the anchor through an explicit reset.

The focused verifier is `src/WorldGenerationC12FocusedVerifier/Program.cs` with project
`src/WorldGenerationC12FocusedVerifier/Terraria.WorldGenerationC12FocusedVerifier.csproj`.
Its serial build exited `0` with `0` warnings and `0` errors; its no-build focused run exited `0`
with `C12 hell and special-structure focused verifier passed.` The build artifact is under
`Build/bin/Terraria.WorldGenerationC12FocusedVerifier/Debug/net10.0`.

This checkpoint does not claim Version4 RNG stream ownership, external chest/item or tile/wire/
liquid effects, protected-structure registration, persistence/network, generic snapshot restore,
or legacy-writer replacement. Those remain evidence gaps and integration-review boundaries.

## C13 GenVarsDungeonDerivedProperties

Checkpoint `C13` is a partial implementation checkpoint. The three report members are a compatibility
and query surface over C08 dungeon record state and `DungeonControlLine`; they do not justify a
second writable derived component. `CurrentDungeon` is a mutable selection command facade, while
`CurrentDungeonGenVars` and the dual-dungeon distance property are read views or forwarding
adapters over existing authorities.

Proposed future files:

- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Queries/DungeonDerivedPropertiesQuery.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/DungeonSelectionControlSystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Adapters/LegacyDungeonDerivedPropertiesAdapter.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Adapters/DungeonControlLineAdapter.cs`

### C13 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|
| 2581 | `CurrentDungeon` | `int` | `GenVars.cs:286-296` `public static int CurrentDungeon { get; set; }` | explicit selection adapter/control port; no writable C13 component | `CurrentDungeonGenVars` and direct active-dungeon consumers across world-generation passes | generation setup/reset and dual-dungeon switching paths; complete writer inventory remains partial | generation-session active-record selector; setter behavior confirmed as lower-bound-only clamp | changes which C08 dungeon record downstream readers resolve; no direct tile or persistence effect | no direct `WorldFile`/`NetMessage` path found; writable static property is eligible for the generic GenVars snapshot filter and restore ordering must be verified |
| 2582 | `CurrentDungeonGenVars` | `Terraria.GameContent.Generation.Dungeon.DungeonGenVars` | `GenVars.cs:298` `public static DungeonGenVars CurrentDungeonGenVars => dungeonGenVars[CurrentDungeon];` | pure query/projection over the C08 dungeon-record snapshot plus legacy adapter; no independent state | dungeon layout, biome, conversion, spawn, chest, and other passes that read the active record | nested fields are written by C08 dungeon-record owners, not by this property | getter-only derived view keyed by the active selector; upper-index behavior must remain direct list-index behavior | exposes the selected record to downstream generation logic; no mutation in the query itself | getter-only property is excluded by the generic `CanWrite` snapshot filter; underlying C08 `dungeonGenVars` persistence/snapshot policy remains C08/integration work |
| 2583 | `DualDungeon_NormalizedDistanceSafeFromDither` | `double` | `GenVars.cs:300-310` forwards to `DungeonControlLine.NormalizedDistanceSafeFromDither` | `IDualDungeonDistanceQuery` plus explicit control port and `DungeonControlLineAdapter`; no cached component field | dual-dungeon distance and boundary checks | generation setup or control paths that set the forwarding property; complete writer inventory remains partial | generation-control threshold with external static backing; no validation/clamp in the property | changes the distance threshold used by dual-dungeon qualification/exclusion logic | no direct `WorldFile`/`NetMessage` path found; writable property is eligible for generic GenVars snapshot reflection, while the external static backing is outside that field set |

### C13 Ownership and Invariants

- The selection adapter must preserve `CurrentDungeon` setter semantics exactly: negative input is
  converted to `0`, and non-negative input is left unchanged. It must not add an upper clamp or
  silently redirect an invalid index to record zero.
- `CurrentDungeonGenVars` must resolve `dungeonGenVars[CurrentDungeon]` through the single C08
  record authority. An index beyond the available record list must surface the same out-of-range
  failure boundary as the source rather than returning a fallback or a nullable synthetic record.
- The C13 query owns no `DungeonGenVars` record, nested dungeon fields, bounds, altar state, reward
  state, or list. It consumes an immutable C08 snapshot and returns a stable domain view; the
  legacy `DungeonGenVars` type is confined to the adapter.
- `DualDungeon_NormalizedDistanceSafeFromDither` is a direct get/set relay. The proposed query and
  control port must not normalize, clamp, cache, or derive a second threshold. Its write effect is
  explicit because it changes the external `DungeonControlLine` static value.
- Snapshot eligibility differs by property: `CurrentDungeon` and the dual-dungeon property have
  setters and therefore meet the inspected `WorldGenSnapshot` `PropertyInfo.CanWrite` filter;
  `CurrentDungeonGenVars` is getter-only and does not. The restore sequence must establish the C08
  record list before applying a selected index, and this ordering remains unverified.
- No direct `WorldFile` or `NetMessage` mapping was found for C13. Save/network ownership must not
  be inferred from generic snapshot eligibility, and no new C13 persistence record may duplicate
  C08 dungeon records or the external control-line value.

### C13 Interface Contract and Dependency Impact

| interface | implementation | seam | depth | leverage | locality |
|---|---|---|---|---|---|
| `IDungeonDerivedPropertiesQuery` | `DungeonDerivedPropertiesQuery` | immutable C08 dungeon-record snapshot and active-index input | pure read/projection | centralizes all active-record reads without copying authority | local to world-generation queries |
| `IDungeonSelectionControlPort` | `DungeonSelectionControlSystem` | C08 record owner plus legacy selection adapter | one explicit selection command | makes lower-clamp and upper-failure behavior testable at one boundary | generation-session scoped |
| `IDualDungeonDistanceQuery` | `DungeonDerivedPropertiesQuery` | read-only `DungeonControlLine` adapter | pure forwarding read | removes direct static reads from generation systems | local to dual-dungeon generation |
| `IDualDungeonDistanceControlPort` | `DungeonControlLineAdapter` | explicit write to the external control-line authority | one external control command | makes the only mutable dither side effect visible | adapter-local; integration-owned backing |

The provisional order is C08 record creation/reset -> explicit active-dungeon selection -> C13
active-record query -> dual-dungeon distance reads and generation qualification. A selection command
must complete before any pass that consumes `CurrentDungeonGenVars`; the scheduler must encode this
dependency rather than relying on source or file order. The C13 adapter may preserve the public
Version4 property names, but it must not become a second C08 writer.

C13 consumes C08's dungeon-record snapshot and feeds dungeon, biome, conversion, spawn, chest, and
dual-dungeon generation queries. It does not own `DungeonGenVars` nested fields, tile/liquid work,
WorldStorage, WorldSession, progression, or P18-P20 execution. Selection/control ownership crossing
those boundaries remains `crossSubsystemOwner: integration-review`.

### C13 Focused Verification Plan

Verify negative, zero, and positive `CurrentDungeon` selection values; verify that an index above
the record count is not clamped and that the active-record query preserves the source list-index
failure. Verify record identity after C08 reset, creation, and dual-dungeon switching, with no
mutable record or list alias escaping the adapter. Verify exact forwarding of dither reads and
writes, including no clamp or cache. Verify generic snapshot inclusion of the two writable
properties and exclusion of the getter-only property, including reset-before-restore ordering and
an empty/short C08 record list. Compare all three property outcomes at setup, reset, dual-dungeon
switch, active-record read, snapshot, and later generation-pass barriers. The focused verifier covers
selection, direct upper-index failure, record identity, and exact control-line forwarding; generic
snapshot eligibility/restore and production caller migration remain unverified.

### C13 Implementation Checkpoint

The root `src` C13 boundary reuses the C08 dungeon layout component and record snapshots. The
selection system preserves the source lower-bound-only clamp, while the pure derived query indexes
the copied record list directly so an invalid upper index still fails at the same boundary. The
legacy adapter exposes the current selector and selected record without owning a second record
list. `DungeonControlLineAdapter` forwards the dual-dungeon normalized-distance value through the
explicit `IDungeonControlLinePort` and does not clamp, cache, or normalize it.

The focused verifier is `src/WorldGenerationC13FocusedVerifier/Program.cs` with project
`src/WorldGenerationC13FocusedVerifier/Terraria.WorldGenerationC13FocusedVerifier.csproj`.
The first build exposed three incorrect `Components` path references in that verifier project;
those references were corrected to the existing C08 root-level files. The corrected serial build
exited `0` with `0` warnings and `0` errors, and the no-build run exited `0` with
`C13 dungeon derived-properties focused verifier passed.` The artifact is under
`Build/bin/Terraria.WorldGenerationC13FocusedVerifier/Debug/net10.0`.

This checkpoint does not claim production `DungeonControlLine` binding, complete current-dungeon
writer migration, generic snapshot eligibility/restore ordering, scheduler integration, or
WorldFile/NetMessage ownership.

## C14 WorldSavedOreTierState

Checkpoint `C14` is a partial implementation checkpoint. The seven `WorldGen.SavedOreTiers` fields are
one persistent, load-repairable, and network-projectable world-state boundary. They are not a second
copy of C01 generation selection, C09 `orePatchX` history, C12/C20 hardmode transition state, or
tile data. The component stores the seven values; systems and adapters own reset, generation and
altar commits, version-aware loading, the historical four-tier repair, save encoding, and packet
projection.

Implemented root-`src` files:

- `src/WorldSession/OreTierState.cs` (existing seven-value immutable value type reused)
- `src/WorldSession/WorldSavedOreTierStateComponent.cs` (single state owner)
- `src/WorldSession/WorldSavedOreTierDefaults.cs` (declaration/bootstrap defaults)
- `src/WorldSession/WorldSavedOreTierTileCounts.cs` (immutable loaded-world count input)
- `src/WorldSession/WorldSavedOreTierRepairQuery.cs` (pure first-four repair calculation)
- `src/WorldSession/WorldSavedOreTierRepairSystem.cs` (single component replacement)
- `src/WorldSession/WorldSavedOreTierResetSystem.cs` (seven-value `-1` reset)
- `src/WorldSession/WorldSavedOreTierQuery.cs` (copied seven-value query)
- `src/WorldSession/ISavedOreTierCommitPort.cs` (explicit write port)
- `src/WorldSession/WorldSavedOreTierGenerationCommit.cs` (copied first-four commit input)
- `src/WorldSession/WorldSavedOreTierAltarCommit.cs` (copied high-tier commit input)
- `src/WorldSession/WorldSavedOreTierCommitSystem.cs` (single writer boundary; present before this continuation and reviewed)
- `src/WorldSession/WorldSavedOreTierQuery.cs` (copied seven-value query)
- `src/WorldSession/ISavedOreTierCommitPort.cs` (explicit write port)
- `src/WorldSession/WorldSavedOreTierGenerationCommit.cs` (copied first-four commit input)
- `src/WorldSession/WorldSavedOreTierAltarCommit.cs` (copied high-tier commit input)
- `src/WorldSession/WorldSavedOreTierCommitSystem.cs` (single writer boundary; present before this continuation and reviewed)

Remaining proposed future files:

- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/WorldSavedOreTierState.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/WorldSavedOreTierStateComponent.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/WorldSavedOreTierLoadSystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Adapters/WorldFileSavedOreTierAdapter.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Adapters/NetMessageSavedOreTierAdapter.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Adapters/LegacySavedOreTierAdapter.cs`

Evidence summary for this checkpoint: Version4 declares the seven defaults at
`WorldGen.cs:3318-3333` and clears all seven to `-1` at `WorldGen.cs:6485-6491`. New-world setup
writes the first four defaults and may select `166/167/168/169` at `WorldGen.cs:10308-10338`;
these saved values are separate from C01 `GenVars` ore and bar fields. `OrePatch` reads Copper or
Iron at `WorldGen.cs:9624-9631`, chest generation reads Silver at `WorldGen.cs:30770-30776`, and
`SmashAltar` initializes or toggles Cobalt, Mythril, and Adamantite using normal IDs
`107/108/111` and alternate IDs `221/222/223` at `WorldGen.cs:41847-41943`.

The load path calls `CheckSavedOreTiers` before `waterLine` and liquid settling at
`WorldFile.cs:723-753`; the repair itself only considers the first four values at
`WorldFile.cs:811-869`. The header reads high tiers at `WorldFile.cs:2184-2186`, reads the first
four only for `versionNumber >= 216` at `WorldFile.cs:2395-2407`, and applies the legacy high-tier
`versionNumber >= 54` / `altarCount` fallback at `WorldFile.cs:3693-3707`. Save order is
Cobalt/Mythril/Adamantite then Copper/Iron/Silver/Gold at `WorldFile.cs:1353-1355` and
`WorldFile.cs:1426-1429`. The world-state packet writes all seven as `short` in
Copper/Iron/Silver/Gold/Cobalt/Mythril/Adamantite order at `NetMessage.cs:385-391`; its complete
inbound consumption path remains unverified.

### C14 Member Assignment

| source row | member | C# type | source declaration | candidate | readers | writers | lifecycle/status type | side effects | persistence/network |
|---:|---|---|---|---|---|---|---|---|---|---|
| 2384 | `Copper` | `int` | `WorldGen.cs:3320` `public static int Copper = 7;` | `WorldSavedOreTierStateComponent.Copper` within one seven-value component | `OrePatch`, first-four repair, WorldFile, NetMessage, saved-tier query | world reset, new-world generation commit, WorldFile load/repair | persistent world tier; `-1` is the uninitialized/repair sentinel | selects the copper tile type used by ore generation; the component does not mutate tiles | WorldFile reads/writes it in the first-four group; packet position 1, encoded as `short` |
| 2385 | `Iron` | `int` | `WorldGen.cs:3322` `public static int Iron = 6;` | `WorldSavedOreTierStateComponent.Iron` within one seven-value component | `OrePatch`, first-four repair, WorldFile, NetMessage, saved-tier query | world reset, new-world generation commit, WorldFile load/repair | persistent world tier; `-1` is the uninitialized/repair sentinel | selects the iron tile type used by ore generation; the component does not mutate tiles | WorldFile reads/writes it in the first-four group; packet position 2, encoded as `short` |
| 2386 | `Silver` | `int` | `WorldGen.cs:3324` `public static int Silver = 9;` | `WorldSavedOreTierStateComponent.Silver` within one seven-value component | chest generation, first-four repair, WorldFile, NetMessage, saved-tier query | world reset, new-world generation commit, WorldFile load/repair | persistent world tier; `-1` is the uninitialized/repair sentinel | affects chest item selection through a read snapshot; no chest or tile side effect belongs to the component | WorldFile reads/writes it in the first-four group; packet position 3, encoded as `short` |
| 2387 | `Gold` | `int` | `WorldGen.cs:3326` `public static int Gold = 8;` | `WorldSavedOreTierStateComponent.Gold` within one seven-value component | first-four repair, WorldFile, NetMessage, saved-tier query | world reset, new-world generation commit, WorldFile load/repair | persistent world tier; `-1` is the uninitialized/repair sentinel | supplies the gold tier to downstream generation queries; no tile mutation belongs to the component | WorldFile reads/writes it in the first-four group; packet position 4, encoded as `short` |
| 2388 | `Cobalt` | `int` | `WorldGen.cs:3328` `public static int Cobalt = 107;` | `WorldSavedOreTierStateComponent.Cobalt` within one seven-value component | `SmashAltar`, WorldFile, NetMessage, saved-tier query | world reset, explicit hardmode/altar commit, WorldFile load | persistent progression-compatible tier; source reset value is `-1` | supplies the selected post-altar tile type; altar progression, broadcast, and tile conversion remain external | WorldFile writes/reads it before the first-four group; packet position 5, encoded as `short` |
| 2389 | `Mythril` | `int` | `WorldGen.cs:3330` `public static int Mythril = 108;` | `WorldSavedOreTierStateComponent.Mythril` within one seven-value component | `SmashAltar`, WorldFile, NetMessage, saved-tier query | world reset, explicit hardmode/altar commit, WorldFile load | persistent progression-compatible tier; source reset value is `-1` | supplies the selected post-altar tile type; altar progression and chat effects remain external | WorldFile writes/reads it before the first-four group; packet position 6, encoded as `short` |
| 2390 | `Adamantite` | `int` | `WorldGen.cs:3332` `public static int Adamantite = 111;` | `WorldSavedOreTierStateComponent.Adamantite` within one seven-value component | `SmashAltar`, WorldFile, NetMessage, saved-tier query | world reset, explicit hardmode/altar commit, WorldFile load | persistent progression-compatible tier; source reset value is `-1` | supplies the selected post-altar tile type; altar progression and tile work remain external | WorldFile writes/reads it before the first-four group; packet position 7, encoded as `short` |

### C14 Ownership and Invariants

- Keep all seven values in one immutable snapshot/value object and one component owner. The shared
  reset, version-gated load, repair predicate, save compatibility, and fixed packet ordering are
  one lifecycle invariant; splitting the fields into a four-value component and a three-value
  hardmode component would make that invariant implicit. The existing three-value hardmode state is
  therefore an integration input, not the C14 authority.
- Preserve the source declaration defaults as bootstrap definitions (`7, 6, 9, 8, 107, 108, 111`)
  but preserve the world-clear reset exactly: all seven values become `-1`. A new world reaches the
  first-four values through an explicit C01 generation commit; C14 does not rerun the random choices
  that select `GenVars.copper`, `iron`, `silver`, `gold`, or their bar values.
- The C01 handoff must be explicit. The generation ore-selection system publishes a copied
  `SavedOreTierGenerationCommit` for the first four values after its source-equivalent selection
  decision. `WorldSavedOreTierCommitSystem` applies that event before any `OrePatch` or chest pass
  reads the snapshot. It does not alias C01 arrays, bars, or RNG state.
- Preserve `CheckSavedOreTiers` exactly. After a successful load, if any of Copper, Iron, Silver,
  or Gold is `-1`, calculate all four replacements from one loaded-world tile-count snapshot over
  `(7,166), (6,167), (9,168), (8,169)`. For each pair, `vanillaCount > alternateCount` selects the
  vanilla ID and every other result selects the alternate ID. This operation does not repair,
  infer, or rewrite Cobalt, Mythril, or Adamantite.
- The four-value repair is a pure calculation followed by one component commit. It must not expose
  a partially repaired quartet to `waterLine`, liquid settling, ore generation, or network output.
  The load barrier remains source-compatible: `CheckSavedOreTiers` runs after the raw world load has
  succeeded and before `waterLine` is set and liquid processing begins.
- Preserve the high-tier lifecycle without moving ownership of `altarCount`, `Main.drunkWorld`, the
  random source, chat messages, progression flags, or tile conversion into C14. The C12/C20 altar
  action calculates a selected value and submits an explicit commit result: cycle 0 publishes
  Cobalt, cycle 1 publishes Mythril, and later cycles publish Adamantite. Existing `-1` values are
  initialized to the source default or alternate through the external action; drunk-world toggles
  are also calculated outside the component.
- Do not alias C14 with C09 `orePatchX`, tile registries, mineral entities, or C01 generation fields.
  C14 is the saved tile-type selection consumed by queries; C09 remains generation history and tile
  commit state. A failed `OrePatch` does not mutate C14, and a failed altar/tile operation does not
  publish a new saved tier.
- The component is not a generic `WorldGenSnapshot` field. Saved-tier load/save and network
  projection have a separate world-state lifetime. Generic snapshot eligibility must not create a
  second persisted copy or cause C14 to be restored after the load/repair and liquid barrier.

### C14 Interface Contract and Dependency Impact

| interface | implementation | seam | depth | leverage | locality |
|---|---|---|---|---|---|
| `ISavedOreTierQuery` | `WorldSavedOreTierQuery` | immutable `WorldSavedOreTierState` snapshot | pure read/projection | centralizes all generation and progression reads without exposing mutable storage | world-generation query boundary |
| `ISavedOreTierCommitPort` | `WorldSavedOreTierCommitSystem` | explicit generation or altar commit event | one state transition | makes the only C14 writer visible and testable | world/session integration boundary |
| `ISavedOreTierRepairQuery` | `WorldSavedOreTierRepairQuery` | loaded tile-type count snapshot | pure four-tier calculation | isolates historical compatibility behavior and tie semantics | WorldFile load adapter |
| `IWorldFileSavedOreTierAdapter` | `WorldFileSavedOreTierAdapter` | Version4 header version and field offsets | version-aware read/write adapter | preserves v216/v54 gates and split save order without leaking `BinaryReader` into state | WorldStorage integration boundary |
| `ISavedOreTierNetworkProjection` | `NetMessageSavedOreTierAdapter` | seven signed `short` packet fields | one-way encode/decode projection | preserves fixed wire order and makes inbound evidence explicit | protocol/session boundary |

The provisional lifecycle order is reset -> version-aware raw load -> four-tier repair -> `waterLine`
and liquid load barrier -> C01 first-four generation commit or C12/C20 high-tier commit -> generation
and progression queries -> save/network projections. The scheduler must encode these dependencies;
file order does not. `WorldFileSavedOreTierAdapter` owns the legacy layout mapping, but the final
WorldStorage/WorldSession owner remains `crossSubsystemOwner: integration-review`.

#### C14 WorldFile value-mapping checkpoint

Implemented only the independently evidenced value seam in the root `src/WorldSession` project:

- `WorldSavedOreTierFileInput` carries the version, altar count, and optional raw tier fields without
  coupling the component to `BinaryReader` or `WorldFile`.
- `WorldFileSavedOreTierAdapter.Read` applies the confirmed rules: Copper/Iron/Silver/Gold are direct
  only at version 216 and above; Cobalt/Mythril/Adamantite are direct at version 54 and above; for
  version 23 through 53, `AltarCount == 0` yields three `-1` values, otherwise the named defaults are
  used. Missing fields on a direct-read path are rejected before a value is returned.
- `WorldFileSavedOreTierAdapter.PrepareSave` returns the confirmed save order
  Cobalt/Mythril/Adamantite followed by Copper/Iron/Silver/Gold in
  `WorldSavedOreTierFileSaveValues`.

This adapter performs no file I/O, header negotiation, load-barrier sequencing, repair, component
replacement, or rollback. Those effects remain at the WorldStorage/WorldSession integration boundary.
The root WorldSession project and focused verifier were compiled serially with zero warnings and
zero errors, and the focused run passed the v23/v54/v216 mapping, missing direct-field rejection,
and high-tier-first save-order assertions.

The current NLTX implementation is partial. `dome/dome1` has a three-value hardmode state and
selection policy, and `dome/dome2` has seven-value record shapes in WorldSession, but neither is
confirmed as the unique C14 authority. `DomeStatePersistenceFormat` does not yet carry all seven
saved tiers. The design therefore proposes a single C14 owner and compatibility adapters rather than
aliasing either existing shape or adding a second writer.

C14 consumes the C01 generation selection handoff and the C12/C20 altar selection result. It feeds
`OrePatch`, chest generation, hardmode tile/progression queries, WorldFile save/load, and the world
state packet. It does not own tile/liquid mutations, chest/item effects, altar count, progression
transitions, WorldStorage, WorldSession, or protocol transport. Those owners remain
`crossSubsystemOwner: integration-review` until the integration review closes them.

### C14 Focused Verification Plan

Verify the seven-member mapping, one-component snapshot identity, fresh-copy behavior, and exact
reset to seven `-1` values. Verify the declaration/bootstrap definitions separately from the world
clear lifecycle. Exercise the legacy version matrix around v23, v54, v216, and current v319:
first-four values are direct only for v216+, high-tier values are direct for v54+, and the
`altarCount == 0` compatibility branch produces high-tier `-1` only for the source's v23-v53 path.

Verify `CheckSavedOreTiers` returns without mutation when all four low tiers are present; when any
is missing, verify one tile-count snapshot, strict `>` tie behavior, replacement of all four low
tiers, no high-tier repair, and execution before `waterLine`/liquid processing. Verify a failed
load or repair cannot publish a partially repaired state.

Verify the C01 generation handoff preserves the supplied selected IDs without rerunning RNG or
aliasing `GenVars` values, and that `OrePatch` and chest readers observe the committed snapshot.
Verify the C12/C20 altar handoff preserves `-1` initialization, normal/alternate IDs, drunk-world
toggle behavior, cycle-to-field mapping, and no commit on a failed external effect. Verify C14 has
no tile, chest, chat, progression, or `orePatchX` side effects of its own.

Verify WorldFile field offsets and split write order, including all seven values, and verify the
NetMessage outbound order and signed-short representation exactly as sourced. The pure value
mapping and outbound projection assertions passed in the focused verifier. The complete inbound
packet consumer and its version/capability negotiation remain an evidence gap and must be closed
before claiming network parity. Compare C14 values at reset, raw load, repair, generation commit,
altar commit, save, and packet projection barriers; production file/transport and load-barrier
behavior remain unverified.

### C14 Implementation Checkpoint

Implemented the independently evidenced C14 core in the root WorldSession project:

- `src/WorldSession/WorldSavedOreTierTileCounts.cs` is an immutable one-shot input snapshot for
  the four Version4 vanilla/alternate tile-count pairs.
- `src/WorldSession/WorldSavedOreTierRepairQuery.cs` is a pure calculation. It repairs all four
  low tiers when any low tier is `-1`, selects the vanilla ID only for strict `vanillaCount >
  alternateCount`, selects the alternate ID on ties, and leaves Cobalt/Mythril/Adamantite
  untouched.
- `src/WorldSession/WorldSavedOreTierRepairSystem.cs` calculates before replacing the component
  value, so callers do not observe a partially repaired quartet.
- `src/WorldSession/WorldSavedOreTierResetSystem.cs` resets all seven values to the source
  world-clear sentinel `-1`.
- `src/WorldSession/WorldSavedOreTierQuery.cs` returns a copied seven-value snapshot without
  exposing component mutation.
- `src/WorldSession/WorldSavedOreTierCommitSystem.cs` is the explicit single writer for copied
  generation and altar commit payloads. It validates every tier before replacing only the relevant
  fields and preserves the other fields.

- `src/WorldSession/WorldSavedOreTierFileInput.cs`,
  `WorldSavedOreTierFileSaveValues.cs`, `IWorldFileSavedOreTierAdapter.cs`, and
  `WorldFileSavedOreTierAdapter.cs` implement the pure version-gated field mapping and confirmed
  high-tier-first save projection. They do not bind file streams, headers, load barriers, repair, or
  rollback.
- `src/WorldSession/WorldSavedOreTierNetworkFields.cs`,
  `ISavedOreTierNetworkProjection.cs`, and `NetMessageSavedOreTierAdapter.cs` implement the pure
  outbound projection in Copper/Iron/Silver/Gold/Cobalt/Mythril/Adamantite order using the sourced
  unchecked `short` casts. They do not implement inbound packet consumption or capability negotiation.

The component remains a seven-value boundary and is not split with the existing three-value
hardmode state. No tile, liquid, file, clock, RNG, progression, chest, chat, or network effect is
performed by these units. The existing `WorldRulesState.SavedOreTiers` field is not silently
rewired because the current root project has no aggregate/caller contract proving which owner must
replace it; this integration gap is recorded above. The commit boundary and pure adapter mappings
are isolated; production WorldFile/NetMessage ownership, inbound consumption, and rollback remain
unverified.

Focused verification passed after a serial build. The verifier covers the seven-value query,
world-clear reset, four-tier repair and tie behavior, generation/altar commit isolation, WorldFile
v23/v54/v216 mapping, high-tier-first save projection, missing direct-field rejection, outbound
packet order, and sourced signed-short conversion. It does not verify production file/transport I/O,
inbound consumption, capability negotiation, liquid load ordering, aggregate ownership, or legacy
behavior equivalence.

## 8. Cross-Subsystem Findings

| finding | P17 boundary | required handoff | owner status |
|---|---|---|---|
| structure reservations and tile writes have different atomicity and retry behavior | `structures` adapter vs tile commit port | reservation result and tile mutation intents must be explicit | `crossSubsystemOwner: integration-review` |
| `waterLine` is both generation state and a load-time liquid boundary input | C03 vs LiquidSimulation/WorldStorage | define load barrier and whether the line is persisted, recomputed, or projected | `crossSubsystemOwner: integration-review` |
| `SavedOreTiers` has save-version repair and network packet encoding | C14 vs WorldStorage/WorldSession | approve one load/repair/write owner and packet compatibility contract | `crossSubsystemOwner: integration-review` |
| Version4 splits saved-tier write order from the declaration and uses different version gates for low and high tiers | C14 vs WorldStorage | preserve v23/v54/v216 behavior, raw offsets, repair timing, and rollback on malformed input | `crossSubsystemOwner: integration-review` |
| outbound saved-tier packet fields are evidenced, but complete inbound consumption and negotiation are not | C14 vs WorldSession/protocol transport | trace and verify the receive path before claiming decode parity | `crossSubsystemOwner: integration-review` |
| dungeon selection and `DungeonControlLine` setter effects cross generation control | C08/C13 vs P18-P20 | expose query snapshot and explicit selection command | `crossSubsystemOwner: integration-review` |
| hell chests, statues, shimmer, infection, and secret-seed flags can affect item/NPC/progression state | C12 vs WorldProgressionAndTransition/NpcAndTownSimulation/P18-P20 | define event and persistence ownership before migration | `crossSubsystemOwner: integration-review` |
| GenVars snapshot restore calls `Reset` before reflection restore | all transient C01-C12 state vs WorldSession | characterize reset/restore ordering and reference copying | `crossSubsystemOwner: integration-review` |
| C09 history append and tile/ore mutation have different commit and retry boundaries | C09 vs LiquidSimulation/P19-P20 | define atomic history-plus-command result handling and preserve pass order | `crossSubsystemOwner: integration-review` |
| `OrePatch` reads C14 saved tiers and mutates tiles before `orePatchX` history append | C09/C14 vs WorldStorage/WorldSession/P19-P20 | approve the generation ore input snapshot and full-capacity/failed-commit behavior | `crossSubsystemOwner: integration-review` |
| Lakes combines C09 cave/tunnel avoidance with a lake-only X history and `SonOfLakinater` effects | C09/C11 vs LiquidSimulation/P19-P20 | expose immutable C09 snapshots and make lake history append conditional on the terrain/liquid commit result | `crossSubsystemOwner: integration-review` |
| `PlaceOasis` mutates terrain/liquid before appending center/width metadata and may succeed without recording when full | C11 vs LiquidSimulation/P19-P20 | approve the metadata-plus-tile transaction and full-capacity compatibility behavior before migration | `crossSubsystemOwner: integration-review` |
| C12 statue catalog indexes drive both statue selection and trap wiring | C12 vs P20/tile/wiring execution | preserve ordered Point16 catalog plus index-based trap query as immutable inputs | `crossSubsystemOwner: integration-review` |
| C12 Shimmer anchor commits a protected structure after a retrying biome placement and is read by later exclusions | C12 vs structure/tile/liquid systems | define anchor/commit/reservation ordering and snapshot restore ownership | `crossSubsystemOwner: integration-review` |

## 9. Compatibility, Non-Split Decisions, and Behavior Risks

Compatibility keeps the original Version4 names, namespaces, and external APIs behind adapters
until a focused verifier proves one new owner. There is no dual-write period for authority: add a
read adapter first, introduce one owner writer, compare snapshots, then remove the old writer only
after the comparison passes. Arrays and lists are copied at the boundary or wrapped with a bounded
view; mutable external references are not stored directly in core components.

The 14 report leaves are checkpoints, not a promise of one component each. C01 is intentionally
split because configuration, reservation, and ore selection have different readers, side effects,
and lifetimes. Future leaves may similarly contain a small number of cohesive components. A field
is deferred when its external owner or durable meaning is not evidenced; deferral is preferable to
inventing a cross-domain component.

Primary behavior risks are reset ordering, shared array mutation, reservation/tile commit ordering,
seed/RNG stream changes, load-time `waterLine` ordering, dungeon index changes, accidental save or
network exposure of scratch state, duplicate saved-tier writes, and hidden setters becoming a
second authority. These risks require focused tests, not a compile-only gate.

## 10. Focused Verifier Plan and Current Verification

Planned verifiers, all unrun in this document session:

- C01 configuration verifier: immutable definition capture, `[JsonIgnore]` compatibility mapping,
  one configuration reader, and no component-level I/O.
- Reservation verifier: `CanPlace` and protected-structure overlap parity, deterministic reject
  results, idempotent reservation command, and no `StructureMap` type in core APIs.
- Ore selection verifier: generation-time selection snapshot is deterministic for a supplied RNG
  stream; C14 saved tiers do not change unless an explicit approved commit occurs.
- Reset/restore verifier: each scratch array/list has a fresh bounded owner; snapshot restore does
  not retain mutable external references and preserves the documented reset order.
- Cross-boundary verifiers: load/save round trip, packet encode/decode, liquid load barrier,
  dungeon index bounds, and tile/structure commit ordering.
- C14 saved-tier verifier: seven-value reset/load/repair/commit snapshots, v23/v54/v216/v319
  version gates, WorldFile offset/order compatibility, outbound packet order, and the unresolved
  inbound packet consumer path.

The C14 WorldFile value-mapping checkpoint additionally requires focused tests for the v23/v54/v216
boundaries, optional-field rejection on direct reads, the three high-tier fallback values, all-four
low-tier `-1` behavior for pre-v216 input, and exact save-order projection. Production file I/O and
load-barrier behavior remain unverified by design.

The repository currently contains candidate projects such as
`dome/Test/Terraria.Dome.WorldGeneration.Verification/`,
`dome/Test/Terraria.Dome.Persistence.Verification/`,
`dome/Test/Terraria.Dome.World.Protocol.Verification/`, and
`dome/Test/Terraria.Dome.Liquid.Verification/`. The exact focused project and test names must be
selected after the proposed source boundaries are approved.

The current continuation ran serial compile-capable builds and no-build focused verifiers for
the isolated C02, C03, C04, C05, C06, C07, C09, C10, C11, C12, C13, and C14 projects or boundaries
recorded above. Each recorded focused build/run passed with zero warnings and zero errors where a
build was run; the verifier outputs are preserved in the execution document. A full `WorldSession`
build attempt was blocked by duplicate C10 declarations already present in the shared checkout.
No runtime generation, production network check, persistence round trip, inbound packet,
load-barrier, or behavior-equivalence check was run. Those remaining checks are intentionally
unverified.

## 11. Checkpoint Log

| checkpoint | state | saved with both documents | evidence-gap / blocking decision |
|---|---|---|---|
| C01 | design checkpoint plus partial implementation | 2026-09-11T17:24:15.0769956Z | `WorldGenerationOreSelectionComponent` source saved; configuration/structure adapter evidence remains missing; compile and focused verification not run |
| C02 | implemented state/query/commit/calculator source plus isolated verifier checkpoint | 2026-09-11T22:49:44.8655420Z | Version4 calculator control flow and explicit input/RNG seam are saved; isolated serial build/run exit 0 with 0 warnings/0 errors and `C02 world layer metrics focused verifier passed.`; full-project build still has unrelated shared-checkout errors; tile-history, snow lifecycle, and complete legacy parity remain open |
| C03 | partial implementation plus fresh focused verification checkpoint | 2026-09-11T22:08:01.6117673Z | bounded spawn/landmass, landmass-definition conversion, pure surface-material/query, and liquid-boundary commit boundaries are implemented and focused-verified; generation formulas, external material catalog, boulder effects, WorldFile/LiquidSimulation handoff, full reset/restore, and legacy parity remain open |
| C04 | partial implementation plus focused verification checkpoint | 2026-09-11T22:15:22.3957768Z | beach state, complete generation-identity commit, raw-value preservation, pure query, and focused verifier are saved; serial build/run exit 0 with 0 warnings/0 errors; calculator formulas, RNG/dimension/configuration owner, shell semantics, liquid handoff, and legacy parity remain open |
| C05 | partial implementation plus focused verification checkpoint | 2026-09-11T22:31:07.2539706Z | ocean constraints, bounded treasure recorder, pass-control flag, pure queries, and focused verifier are saved; serial build/run exit 0 with 0 warnings/0 errors; calculator/RNG, C04 routing, duplicate policy, phase token, placement projection, and legacy parity remain open |
| C06 | partial implementation plus focused verification checkpoint | 2026-09-12T04:06:02.3891860Z | desert state/query/commit, reset sentinels, larva projection, generic reservation gate, and the tile-solidity projection are recorded; the serial focused build exited 0 with 0 warnings/errors, the artifact exists under `Build/bin/Terraria.WorldGenerationC06FocusedVerifier/Debug/net10.0`, and the no-build focused run passed; rectangle semantics, retry/normalization, concrete reservation mapping, external effects, and legacy parity remain open |
| C07 | partial implementation plus fresh focused verification checkpoint | 2026-09-12T00:15:21.7866592Z | region, pyramid, and chest/loot generation-identity commit boundaries, sourced jungle-hut-to-wall mapping, explicit jungle-hut roll selection, inclusive conversion-range query, and base jungle-chest item cursor transition are implemented and focused-verified; region calculation, global RNG ownership/consumption order, external material registration, exact random-source ordering, wand uniqueness, reservation/placement ordering, external effects, and legacy parity remain open |
| C08 | complete design checkpoint | 2026-09-11T15:05:00.0000000Z | dungeon record/index ownership, nested state conversion, unique reward policy, island capacity/pairing, and execution handoff remain open; implementation remains deferred |
| C09 | partial implementation plus isolated focused verification checkpoint | 2026-09-11T20:18:35.1448491Z | three bounded history components, copied snapshots, pure strict-distance queries, and ore append-after-success gate saved; isolated build/run exit 0 with 0 warnings/0 errors and `C09 history focused verifier passed.`; full WorldSession build blocked by duplicate C10 declarations, while Version4 reset/overflow characterization, dependent-reader adapters, external effects, and legacy parity remain open |
| C10 | partial implementation plus focused verification checkpoint | 2026-09-11T23:01:22.6555370Z | bounded mushroom-anchor and fallen-log handoff source saved in root `src`; isolated serial build/run exit 0 with 0 warnings/0 errors and `C10 mushroom and fallen-log focused verifier passed.`; external tile/liquid/RNG wiring, snapshot restore, and legacy parity remain open |
| C11 | partial implementation plus source-boundary migration checkpoint | 2026-09-12T01:52:59.9561478Z | bounded lake/oasis state, copied snapshots, strict queries, mixed-generation rejection, and append-after-success gates remain saved; append-result/status types moved from Components to Systems and focused-project references updated; fresh serial build/run exited 0 with 0 warnings and 0 errors and the focused verifier passed; terrain/liquid/RNG, reset/restore, vegetation, and legacy parity remain open |
| C12 | partial implementation plus fresh focused verification checkpoint | 2026-09-11T21:29:04.9467112Z | hell-chest pool/cursor, statue catalog/trap indexes, infection/special-seed conjunctions, and Shimmer anchor commit/query boundaries are implemented and focused-verified; external effects, RNG stream ownership, snapshot restore, persistence/network, and legacy parity remain open |
| C13 | partial implementation plus focused verification checkpoint | 2026-09-11T21:48:44.2291962Z | lower-bound-only selection, direct upper-index behavior, C08 record authority, and explicit control-line forwarding are implemented and focused-verified; production backing, snapshot eligibility/restore ordering, caller migration, scheduler integration, and legacy parity remain open |
| C14 | partial implementation plus fresh focused verification checkpoint | 2026-09-11T18:49:00.7040540Z | repair/reset, commit/query, WorldFile value mapping, and outbound NetMessage projection saved in root src; serial build exit 0 with 0 warnings/0 errors and focused verifier exit 0; component-to-WorldRules owner integration, inbound packet consumption, save-owner/version-upgrade, liquid barrier, persistence rollback, and legacy behavior equivalence remain open |
