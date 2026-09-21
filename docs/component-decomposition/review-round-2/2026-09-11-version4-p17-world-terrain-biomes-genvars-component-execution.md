# P17 World Terrain, Biomes, and GenVars: Component Execution Plan

partitionId: P17
sessionId: e588106a6053430ca46c41cb07e8e4ca
claimMode: manual
ledgerStatus: completed
executionScope: component-only
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P17-World-Terrain-Biomes-GenVars.md
designPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md
executionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-p17-world-terrain-biomes-genvars-component-execution.md
executionStatus: completed
implementationStatus: completed
verificationStatus: focused-c01-c02-state-passed; c02-calculator-owner-production-build-passed-before-current-source-continuation; c02-calculator-owner-current-isolated-build-passed; c02-calculator-owner-current-isolated-run-passed; c03-spawn-material-liquid-focused-build-passed; c03-spawn-material-liquid-focused-run-passed; c04-boundary-focused-build-passed; c04-boundary-focused-run-passed; c04-beach-calculator-focused-build-passed; c04-beach-calculator-focused-run-passed; c05-state-query-focused-build-passed; c05-state-query-focused-run-passed; c05-treasure-overflow-reset-focused-build-passed; c05-treasure-overflow-reset-focused-run-passed; c05-treasure-placement-projection-focused-build-passed; c05-treasure-placement-projection-focused-run-passed; c06-state-query-commit-focused-passed; c06-underground-desert-focused-build-passed; c06-underground-desert-focused-run-passed; c06-underground-desert-reset-red-build-passed; c06-underground-desert-reset-green-build-passed; c06-underground-desert-reset-focused-run-passed; c07-state-query-focused-passed; c07-region-pyramid-chest-commit-focused-passed; c07-jungle-hut-material-focused-build-passed; c07-jungle-hut-material-focused-run-passed; c07-jungle-hut-selection-focused-build-passed; c07-jungle-hut-selection-focused-run-passed; c07-jungle-region-focused-build-passed; c07-jungle-region-focused-run-passed; c07-jungle-loot-focused-build-passed; c07-jungle-loot-focused-run-passed; c07-jungle-bounds-calculation-focused-build-passed; c07-jungle-bounds-calculation-focused-run-passed; c08-focused-build-passed; c08-focused-run-passed; c09-history-focused-build-passed; c09-history-focused-run-passed; c10-mushroom-log-focused-build-passed; c10-mushroom-log-focused-run-passed; c11-lake-oasis-focused-build-passed-before-result-boundary-migration; c11-lake-oasis-focused-run-passed-before-result-boundary-migration; c11-append-result-system-boundary-migration-focused-build-passed; c11-append-result-system-boundary-migration-focused-run-passed; c12-hell-special-focused-build-passed; c12-hell-special-focused-run-passed; c12-component-api-focused-build-passed; c12-component-api-focused-run-passed; c13-dungeon-derived-properties-focused-build-passed; c13-dungeon-derived-properties-focused-run-passed; c14-core-focused-passed; c14-worldfile-adapter-focused-passed; c14-netmessage-adapter-focused-passed; c06-reservation-gate-focused-build-passed; c06-reservation-gate-focused-run-passed; c06-larva-solidity-projection-focused-build-passed; c06-larva-solidity-projection-focused-run-passed; c06-underground-desert-bounds-query-focused-build-passed; c06-underground-desert-bounds-query-focused-run-passed; c06-larva-component-paired-storage-focused-build-passed; c06-larva-component-paired-storage-focused-run-passed; c06-component-coverage-audit-passed; worldsession-current-build-passed-with-one-preexisting-nullable-warning; worldsession-current-run-passed
evidenceStatus: partial
verificationRecordNote: The long semicolon-separated verification history below includes older checkpoint labels. The authoritative current result is `terminalVerificationStatus`: the fresh C12 and WorldSession focused build/run commands exited 0 with 0 warnings and 0 errors.
evidence-gap-current: C04 beach-boundary state, generation-identity commit, pure query, and explicit calculation/commit boundaries are implemented in src and passed the serial focused build/no-build run; global RNG ownership, exact stream consumption, upstream C02/C03 integration, shell-origin tile/liquid writer, scheduler order, and legacy parity remain pending. C05 constraint state, bounded treasure state, pass-control state, pure queries/recorders, explicit constraint calculation/commit boundaries, and Version4 ocean-cave attempt overflow reset are implemented in src and passed the serial focused build/no-build run; treasure duplicate policy, phase-token lifetime, placement projection, save/network ownership, item/tile effects, and legacy parity remain pending. C07 region, pyramid, and chest/loot generation-identity commit boundaries are implemented in src and passed the fresh serial build and focused verifier; material mapping, allocation parity, reservation ordering, loot-policy determinism, external effects, and RNG/legacy parity remain pending. C08 and C12 component-focused builds/runs passed with exit code 0, 0 warnings, and 0 errors. C09 three bounded history components, copied snapshots, pure strict-distance queries, and the ore append-after-success gate are saved in src and passed by the isolated focused build/run. Version4 overflow/reset behavior, dependent-reader adapters, tunnel scan/TileRunner ordering, OrePatch/tile ordering, saved-tier handoff, rollback semantics, and legacy parity remain pending. C10 mushroom-anchor and fallen-log handoff boundaries are saved and passed by the isolated build/run; ShroomPatch/Flowers wiring, tile/liquid effects, RNG, persistence, network, external transactionality, and legacy parity remain pending. C11 lake/oasis bounded histories, copied snapshots, strict queries, mixed-generation rejection, and append-after-success gates are saved and passed by the isolated build/run; candidate scanning, SonOfLakinater/PlaceOasis, terrain/liquid effects, RNG, reset/restore, downstream vegetation, persistence/network, and legacy parity remain pending. C12 external chest/item, tile/wire/liquid, protected-structure, RNG stream, snapshot restore, persistence/network, and legacy-writer parity remain pending; C13 production backing, generic snapshot restore ordering, complete writer inventory, caller migration, and legacy parity remain pending. The current WorldSession focused build passed with one pre-existing nullable warning and zero errors, and its no-build focused run passed.
completedComponents: [C01, C02.WorldLayerMetricsComponentAndCommitBoundary, C02.WorldLayerMetricsQueryAndFocusedVerifier, C02.WorldLayerMetricsCalculatorAndOwnerSystem, C03.WorldSpawnAndLandmassCommitBoundary, C03.WorldGenerationLiquidBoundaryCommitBoundary, C03.WorldLandmassDefinitionAdapterAndSurfaceMaterialQuery, C04.BeachBoundaryStateCommitAndQueryBoundary, C04.BeachBoundaryFocusedVerifier, C04.BeachBoundaryCalculatorAndCommitSystem, C05.OceanBiomeStateAndPureQueries, C05.OceanBiomeConstraintCommitBoundaryAndBoundedTreasureRecorder, C05.OceanBiomeStateAndPureQueriesFocusedVerifier, C05.OceanBiomeConstraintCalculationAndCommitSystem, C05.OceanCaveTreasureAttemptOverflowReset, C05.OceanCaveTreasurePlacementProjection, C06.UndergroundDesertStateAndPureQueries, C06.UndergroundDesertStructureCommitBoundary, C06.UndergroundDesertLarvaRecorder, C06.UndergroundDesertStateAndCommitFocusedVerifier, C06.UndergroundDesertResetSentinelBoundary, C06.UndergroundDesertResetSentinelBoundary, C06.UndergroundDesertReservationGateAndLarvaProjection, C06.UndergroundDesertLarvaTileSolidityProjection, C06.UndergroundDesertBoundsQuery, C06.UndergroundDesertLarvaComponentPairedStorage, C07.JungleStructureStateAndPureQueries, C07.JungleRegionStructureCommitBoundary, C07.PyramidPlacementCommitBoundary, C07.JungleChestAndLootCommitBoundary, C07.JungleHutMaterialDefinitionQuery, C07.JungleHutMaterialSelectionQuery, C07.JungleRegionConversionRangeQuery, C07.JungleChestLootSelectionQueryAndCursorCommit, C07.JungleRegionBoundsCalculationQuery, C08.DungeonLayoutRewardAndFloatingIslandStateBoundaries, C09.MountainCaveHistoryStateBoundary, C09.SurfaceTunnelHistoryBoundary, C09.SurfaceOrePatchHistoryBoundary, C10.MushroomBiomeAnchorStateBoundary, C10.FallenLogFlowerHandoffBoundary, C11.LakeAndOasisHistoryBoundary, C11.LakeOasisCommitResultSystemBoundaryMigration, C12.HellChestLootCycleBoundary, C12.StatuePlacementCatalogAndTrapBoundary, C12.InfectionAndSpecialSeedRuleBoundary, C12.ShimmerAnchorBoundary, C13.DungeonDerivedPropertiesBoundary, C14.WorldSavedOreTierRepairAndReset, C14.WorldSavedOreTierCommitBoundary, C14.WorldFileSavedOreTierValueMappingAdapter, C14.NetMessageSavedOreTierOutboundProjection]
currentComponent: none (all independently evidenced P17 ECS component boundaries are implemented)
pendingComponents: []
currentStatusNote: The manual P17 session `e588106a6053430ca46c41cb07e8e4ca` settled successfully with runner status `completed`, `exitCode: 0`, and `lockReleased: true` at `2026-09-12T09:37:09.8280014Z`. The component-only execution scope is complete (`pendingComponents: []`). The entries in `pendingIntegrationBoundaries` remain integration-only follow-up work and are not additional component gaps. Historical checkpoint prose below is retained for audit and does not override this terminal status.
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
evidence-gap-c04: C04 beach-boundary state, complete generation-identity commit, raw-value preservation, and pure query are focused-verified. C04 calculation, dimension/configuration/RNG ownership, ordering/clamping, paired shell-origin semantics, downstream intent routing, and legacy parity remain unverified.
evidence-gap-c05: C05 constraint, bounded treasure, pass-control state, pure query/recording boundaries, explicit constraint calculation/commit system, Version4 ocean-cave attempt overflow reset, and the ordered underwater-chest placement-command projection are focused-verified. The constraint calculator derives all eight values from the committed C04 beach snapshot using the sourced reset constants and consumes no global RNG. The treasure attempt entry clears a full two-slot buffer before each attempt and appends only when the external ocean-cave attempt reports that treasure was generated. Candidate coordinate search, duplicate policy, phase-token lifetime, selected-item RNG ownership, chest/tile/liquid effect execution, save/network ownership, and legacy parity remain unverified.
evidence-gap-c11-commit-result-migration: C11 append-result and status types were moved from Components to Systems, and the focused verifier compile list now references the System boundary. The serial focused build exited 0 with 0 warnings and 0 errors; the no-build focused run exited 0 with C11 lake and oasis focused verifier passed. The migration is verified; candidate scanning, terrain/liquid effects, RNG, reset/restore, vegetation, persistence, network, and legacy parity remain unverified.
evidence-gap-c06: C06 layout, pure query, generation-identity commit, bounded larva recorder, capacity rejection, clear, copied snapshot, reset-sentinel, larva projection, reservation-before-command gate, ordered tile-solidity projection, and the pure rectangle bounds query are saved and focused verified. The previous focused build's CS0118 constructor naming defect was corrected by assigning `Timing = phase`; the fresh serial build exited 0 with 0 warnings/errors and the no-build focused run passed. The reset uses explicit world dimensions and restores empty rectangles, Version4 hive bounds, and a zero larva count. The solidity projection describes tile types 229 before larva placement and 232/162 after placement; it does not access Main.tileSolid or execute tile writes. The bounds query preserves the evidenced left/top-inclusive and right/bottom-exclusive semantics, positive-area intersection, zero-area rejection, and hive containment predicate. Physical Version4 array reallocation is not claimed. Layout normalization, retry behavior, duplicate policy, concrete reservation identity/bounds mapping, tile/entity execution, persistence/network, and legacy parity remain unverified.
evidence-gap-c07-material: Version4 explicitly selects jungleHut from one genRand.Next(5) roll and maps roll values 0..4 to tile IDs 119/120/158/175/45, then maps those tile IDs to wall IDs 23/24/42/45/10, with wall ID 0 for other values; the explicit-roll selection query, pure definition/query mapping, and isolated verifiers are focused-verified. Global RNG ownership and consumption order, external tile/material registration, tile writes, wall writes, and legacy behavior equivalence remain unverified.
evidence-gap-c07-region: Version4 explicitly applies the jungle conversion predicate `x >= jungleMinX && x <= jungleMaxX`; the pure inclusive-range query and the explicit boundary calculation from copied column observations are focused-verified. The calculation preserves the sourced left scan `x = 5; x < maxTilesX - 5`, right scan `x = maxTilesX - 5; x > 5`, and zero defaults when no column matches. The external tile scan adapter, world-surface input owner, wall mutation, RNG/world-rule ownership, scheduler ordering, placement effects, and legacy behavior equivalence remain unverified.
evidence-gap-c07-loot: Version4's base jungle chest item selector is implemented as an explicit-input pure query and cursor-only commit; exact random-source consumption order, wand uniqueness, external chest/item effects, and legacy parity remain pending.
evidence-gap-c07-material-selection: The explicit jungle-hut roll-to-tile-to-wall mapping is focused-verified only as a pure query. The global RNG owner, exact consumption order, external material registration, tile/wall writes, and legacy parity remain unverified.
evidence-gap-c04-calculator: The C04 calculation query and commit system are focused-verified for the sourced random interval, anniversary fixed-boundary branch, dungeon/jungle width adjustments, raw no-clamp outputs, complete snapshot propagation, and generation identity. The global RNG owner and exact random stream consumption, upstream C02/C03 integration, shell-origin tile/liquid writer, scheduler order, and legacy behavior equivalence remain unverified.
evidence-gap-c03: The focused C03 source boundary does not claim Version4 landmass generation formulas, spawn/RNG ownership, external material catalog binding, boulder entity effects, WorldFile waterLine load ordering, LiquidSimulation handoff, or full 15-member parity.
evidence-gap: C02 Version4 calculator control flow and explicit configuration/RNG input seam are saved in `src` and the fresh current-source focused build/run passed. Complete 13-member legacy parity, snow-column lifecycle, and tile-history integration remain unproven. C03 spawn/landmass and liquid-boundary commit boundaries are implemented, but external LandmassData conversion, spawn/RNG ownership, material selection, boulder entity effects, WorldFile waterLine load ordering, and LiquidSimulation handoff remain unproven. C04 state, commit, pure query, explicit calculation/commit system, and focused verifier boundaries are implemented and passed; global RNG ownership, exact stream consumption, upstream C02/C03 routing, shell-origin tile/liquid effects, scheduler order, and legacy parity remain unproven. C05 state, bounded recorder, pass control, pure queries, explicit C04-derived constraint calculation, Version4 ocean-cave attempt overflow reset, and focused verifier boundaries are implemented and passed; treasure duplicate policy, phase-token lifetime, placement projection, save/network ownership, item/tile effects, and legacy parity remain unproven. C06 layout state, pure queries, and the generation-identity structure commit boundary are implemented; rectangle edge semantics, hive normalization, retry behavior, larva duplicate/commit policy, reservation ordering, and legacy parity remain unproven. C07 region, pyramid, and chest/loot state/query and generation-identity commit boundaries are implemented and passed the fresh serial focused verifier; material mapping, dynamic allocation parity, duplicate/reservation ordering, loot cursor policy, RNG ownership, tile/item/NPC effects, and legacy parity remain unproven. C07.JungleRegionCalculatorAndPlacementEffects is blocked because current-project evidence does not identify the world-rule/material/RNG owner, boundary formulas, or structure/tile effect ports. C09 history components, copied snapshots, pure strict-distance queries, and the ore append-after-success gate are implemented and passed by the isolated focused build/run. Version4 overflow/reset behavior, dependent-reader adapters, tunnel scan/TileRunner ordering, OrePatch/tile ordering, saved-tier handoff, rollback semantics, and legacy parity remain unproven. Historical note: an earlier full WorldSession build was blocked by duplicate C10 declarations in the shared checkout; this does not describe the current focused build result and is not C09 source evidence. C14's value-only WorldFile adapter and outbound network projection are implemented and passed the fresh serial focused verifier for confirmed version gates, field order, and signed-short casts; their production BinaryReader/BinaryWriter and transport owners, header/version upgrade policy, load barrier, inbound consumption, capability negotiation, and error/rollback contracts remain unproven. C01 external Version4 WorldGenConfiguration/StructureMap binding, C14 aggregate ownership, and the remaining cross-subsystem ownership gaps remain open.
blocking-decision: Keep external configuration, structure, save, network, and liquid types at explicit integration boundaries; do not infer C02/C03/C07 generation formulas, RNG streams, external registration keys, rectangle semantics, LandmassData or tile/liquid APIs, C04's sourced calculation branches may remain as an explicit-input pure query and generation-scoped commit system, but its global RNG owner, exact stream consumption, upstream C02/C03 routing, shell-origin tile/liquid writer, scheduler order, and legacy replacement remain at integration-review. C05's reset-derived constraint calculation is limited to the explicit C04 snapshot input and does not claim global RNG, treasure placement, phase-token, or legacy parity. C05 ocean-cave attempt overflow reset is limited to clearing a full bounded buffer before an externally reported attempt result and appending only a successful treasure coordinate; it does not claim chest/item/tile placement, random search, duplicate policy, or legacy writer replacement; do not bind BinaryReader/BinaryWriter, transport, inbound packet consumption, or header upgrades without current-project owner contracts. The verified C07 region, pyramid, and chest/loot commit boundaries are limited to generation-identity validation, capacity/count validation, paired-list validation, copied state, and complete state replacement. Keep C07 material mapping, reservation/placement ordering, loot policy, tile/item/NPC effects, and legacy writer replacement at integration-review. C07.JungleRegionCalculatorAndPlacementEffects may continue only through evidence-backed pure boundaries: the copied-column scan and inclusive predicate are implemented and focused-verified. The external tile-scan adapter, world-surface owner, world-rule/material/RNG owner, wall mutation, structure/tile effect ports, scheduler ordering, and legacy writer replacement remain blocked pending current-project contracts. The verified C09 boundary now includes only bounded copied histories, pure strict-distance reads, effective-capacity rejection, clear behavior, and the explicit ore append-after-success gate; it does not claim Mountinater, ten-point scan, TileRunner, OrePatch, RNG, saved-tier, rollback, reset/restore, dependent-reader wiring, or legacy parity. The C10 implementation is limited to bounded copied mushroom-anchor history and the evidenced fallen-log handoff transition; it does not claim mushroom/log tile effects, Flowers wiring, RNG, liquid, persistence, network, or rollback ordering. Continue C11 only with bounded lake/oasis history state, copied snapshots, strict queries, mixed-generation rejection, and explicit external-commit result gates; do not infer terrain/liquid adapters, RNG streams, reset ownership, snapshot restore ordering, downstream vegetation, persistence, network, or legacy parity. No new runner settlement is permitted because the historical P17 runner record is already completed.
blocking-decision-c07-loot: Keep the focused loot unit limited to explicit-roll item output, override priority, input validation, and cursor-only state commit. Do not bind a global RNG, infer exact random-source consumption order, implement wand uniqueness, or write chest/item effects until those owners and contracts are evidenced.
blocking-decision-c07-material-selection: Keep jungle-hut selection as an explicit-input pure query. Do not bind global RNG, external material registration, tile/wall mutation, or legacy writer replacement without an identified owner and focused parity evidence.
blocking-decision-c06: C06 rectangle queries now use only the evidenced left/top-inclusive and right/bottom-exclusive semantics and do not normalize layout inputs. The reservation gate remains limited to the generic reservation port and evidenced larva command projection; it does not identify the DesertBiome reservation key, layout formula, duplicate policy, or external tile/entity executor.
evidence-gap-c06-solidity-projection: The C06 solidity projection source and focused verifier are serial-build/run verified. The constructor's prior CS0118 defect was corrected by assigning the public `Timing` property. The projection emits only the ordered `229` before-placement and `232`/`162` after-placement `Solid=true` commands; it does not identify or invoke a production tile-solidity writer.
blocking-decision-c06-solidity-projection: Keep tile-solidity updates as a pure command projection. Do not access `Main.tileSolid`, add a tile writer, infer scheduler order, or claim production tile mutation until the tile-commit owner and integration contract are evidenced.

## Current Continuation Checkpoint

As of `2026-09-12T09:45:41.5358627Z`, this document is the terminal execution record for the manual
`P17` session `e588106a6053430ca46c41cb07e8e4ca`. The authoritative report remains `146/146`.
All independently evidenced component boundaries are implemented and `pendingComponents` is
empty. The remaining entries are explicitly integration-only: external generation and material
ownership, RNG and scheduler wiring, tile/liquid/entity effects, persistence/network/load owners,
derived-property backing, and legacy parity. They are not additional component gaps and are not
implemented in this component-only execution. The runner settlement is complete; historical notes
below do not override the terminal result.

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

## C06 Larva Component Paired Storage Checkpoint

Updated and saved only the component source:

- `src/WorldSession/WorldGeneration/UndergroundDesertLarvaPlacementComponent.cs`

The component now owns separate capacity-100 `int[]` buffers for the authoritative `larvaX` and
`larvaY` members. `TryAppend` writes both coordinates at the same used index, `Clear` clears both
buffers and the used count, and `CreateSnapshot` preserves the existing copied `TilePosition`
snapshot API. No new test, System, Command, Adapter, or external writer was added.

Verification records:

- Build: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src\\WorldGenerationC06FocusedVerifier\\Terraria.WorldGenerationC06FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; warnings/errors `0/0`; artifact `Build/bin/Terraria.WorldGenerationC06FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC06FocusedVerifier.dll`.
- Focused run: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\\src\\WorldGenerationC06FocusedVerifier\\Terraria.WorldGenerationC06FocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; output `C06 underground-desert focused verifier passed.`
The component-only source update after this checkpoint preserves the existing snapshot API while
aligning private storage with the authoritative paired `larvaX`/`larvaY` arrays. No new test,
system, command, adapter, or external writer was added.
evidence-gap-c06-solidity-projection-compile-fix: Resolved. The constructor assignment defect in `UndergroundDesertLarvaTileSolidityCommand.cs` was corrected from the nested `Phase` type name to the public `Timing` property; the subsequent serial focused build/run passed.
blocking-decision-c06-solidity-projection-compile-fix: The compile defect is resolved. Keep the C06 solidity projection at the existing pure command boundary; do not expand into a tile writer or production integration without an evidenced tile-commit owner and contract.

## 1. Plan Boundary

This is a planned and reversible implementation sequence with explicit implementation checkpoints.
It records only the source changes and verifier/build evidence actually performed in this session;
all remaining proposed types and paths are subject to the design document and integration review.

The plan may change only future ECS organization under the `WorldGeneration` capability. It does
not authorize edits to Version4, the authoritative report, another session's document, the ledger,
or unrelated NLTX source.

## 2. Proposed File Organization

The proposed paths are domain-first and keep one core public type per same-named PascalCase file.
Directories deepen only where a stable definition, adapter, query, or independent verifier boundary
exists.

| responsibility | proposed path | namespace | status |
|---|---|---|---|
| configuration definition | `dome/src/Terraria.Dome.Simulation/WorldGeneration/Definitions/WorldGenerationConfigurationDefinition.cs` | `Terraria.Dome.Simulation.WorldGeneration.Definitions` | proposed |
| configuration adapter | `dome/src/Terraria.Dome.Simulation/WorldGeneration/Adapters/WorldGenerationConfigurationAdapter.cs` | `Terraria.Dome.Simulation.WorldGeneration.Adapters` | proposed |
| structure reservation port/adapter | `dome/src/Terraria.Dome.Simulation/WorldGeneration/Adapters/StructureReservationAdapter.cs` | `Terraria.Dome.Simulation.WorldGeneration.Adapters` | proposed |
| generation ore selection state | `dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/WorldGenerationOreSelectionComponent.cs` | `Terraria.Dome.Simulation.WorldGeneration.Components` | proposed |
| generation owner systems | `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/` | `Terraria.Dome.Simulation.WorldGeneration.Systems` | proposed |
| read-only derived queries | `dome/src/Terraria.Dome.Simulation/WorldGeneration/Queries/` | `Terraria.Dome.Simulation.WorldGeneration.Queries` | proposed |
| save/network projections | `dome/src/Terraria.Dome.Simulation/WorldGeneration/Adapters/` and `Projections/` | domain-specific namespaces | proposed |

No `Shared/Components/`, `Common/`, `Misc/`, or file-order scheduling is proposed.

## 3. Global Migration Invariants

- Keep Version4 public names and namespaces behind compatibility adapters until the focused
  verifier passes.
- Introduce one owner writer before removing a legacy writer; never dual-write authority.
- Copy or expose immutable bounded views for arrays and lists; do not retain mutable external
  references in core state.
- Pass RNG, clock, persistence, network, logging, tile, liquid, and structure effects through
  explicit ports.
- Commit source facts before dependent queries and projections; express the order in the
  scheduler, never by source-file order.
- Treat generation scratch, save state, and network projection as different lifetimes unless
  evidence proves they are the same.
- Stop a migration at an evidence gap and record the gap in both documents.

## 4. Planned System Order

The proposed scheduler sequence is configuration load -> reset -> layer metrics -> surface/biome
facts -> beach boundary -> beach/ocean state -> desert/jungle reservations -> dungeon/island facts
-> cave/ore/mushroom/lake scratch -> hell/special facts -> ore selection commit -> validation ->
snapshot/save/network projections. The order is provisional and must be verified with call-site
characterization tests before implementation.

The structure reservation port must commit reservation results before any tile mutation intent that
depends on placement eligibility. Liquid commit must occur through a separate port. The saved-tier
adapter must not observe or write generation-time ore fields by reference.

## 5. C01 GenVarsConfigurationAndOreState

Status: `implemented checkpoint`; implementation is `implemented` and focused verification passed.

Proposed future files:

- `src/WorldSession/WorldGeneration/Definitions/WorldGenerationConfigurationDefinition.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldGenerationConfigurationSnapshot.cs`
- `src/WorldSession/WorldGeneration/Adapters/WorldGenerationConfigurationAdapter.cs`
- `src/WorldSession/WorldGeneration/Adapters/WorldGenerationConfigurationRequest.cs`
- `src/WorldSession/WorldGeneration/Adapters/StructureReservationIntent.cs`
- `src/WorldSession/WorldGeneration/Adapters/ReservationResult.cs`
- `src/WorldSession/WorldGeneration/Adapters/IStructureReservationCommitPort.cs`
- `src/WorldSession/WorldGeneration/Adapters/InMemoryStructureReservationAdapter.cs`
- `src/WorldSession/WorldGeneration/Adapters/StructureReservationSnapshot.cs`
- `src/WorldSession/WorldGeneration/Adapters/WorldGenerationRectangle.cs`
- `src/WorldSession/WorldGeneration/WorldGenerationOreSelectionComponent.cs`
- `src/WorldSession/WorldGeneration/WorldGenerationOreSelectionSnapshot.cs`
- `src/WorldSession/WorldGeneration/Systems/OreTierSelectionSystem.cs`

Planned sequence:

1. Capture the existing Version4 configuration setup and `JsonIgnore` snapshot exclusion in a
   characterization test without changing Version4.
2. Define the immutable configuration value and adapter contract; keep external configuration
   and content types at the adapter boundary.
3. Define reservation intents/results and an adapter around `StructureMap`; verify `CanPlace`,
   protected-structure registration, overlap behavior, and failure results.
4. Define one generation ore selection component for the eight `GenVars` values and a single owner
   system with an injected RNG/configuration snapshot. The state component is implemented; the
   owner system remains pending because no current-project RNG/configuration seam is confirmed.
5. Add a compatibility read projection, compare old and proposed snapshots, and only then plan the
   legacy writer removal.
6. Defer any persistence/network mapping to C14 and integration review; do not add a second saved
   tier writer in C01.

Dependency impact: configuration depends on external definitions and world-session inputs;
reservation depends on the structure implementation and emits results to generation systems; ore
selection depends on seed/configuration facts and is consumed by ore placement and later explicit
saved-tier commit. P18-P20 and WorldStorage remain integration inputs.

Actual checkpoint: C01 source and verifier files were saved under `src/WorldSession/WorldGeneration`
and `Test/Terraria.WorldSession.WorldGeneration.Verification`. The configuration adapter is a
neutral immutable-source seam and the reservation adapter is an in-memory commit-port
implementation; neither claims to be the unavailable Version4 external type. The ore component
and system preserve generation identity and prevent C01/C14 aliasing. The serial focused build
and no-build verifier both exited 0 with zero warnings/errors and artifacts under `Build/bin`.
No legacy source writer was changed or removed. Rollback is limited to the newly introduced C01
files and verifier project.

## 6. C02 GenVarsWorldLayerMetrics

Status: `partial implementation checkpoint`; C02 state/query/commit and calculator source are saved,
and the current-source isolated focused build/run passed. Full-project integration and legacy parity
remain unverified.

Proposed future files:

- `src/WorldSession/WorldGeneration/WorldLayerMetricsComponent.cs` (implemented)
- `src/WorldSession/WorldGeneration/WorldLayerMetricsSnapshot.cs` (implemented)
- `src/WorldSession/WorldGeneration/Systems/WorldLayerMetricsSystem.cs` (implemented commit boundary)
- `src/WorldSession/WorldGeneration/Passes/WorldLayerMetricsCalculationInput.cs` (implemented input seam)
- `src/WorldSession/WorldGeneration/Passes/IWorldLayerMetricsCalculator.cs` (implemented calculator seam)
- `src/WorldSession/WorldGeneration/Passes/WorldLayerMetricsCalculator.cs` (implemented pure calculator)
- `src/WorldGenerationC02FocusedVerifier/` (implemented isolated focused verifier source)

Planned sequence:

1. Capture reset sentinels, scalar metric precision/ordering, and snow-array allocation/default
   behavior in a characterization fixture.
2. Define `WorldLayerMetricsComponent` with value-owned scalars and a bounded immutable snapshot
   boundary for `snowMinX` and `snowMaxX`. The component, snapshot, commit boundary, pure query,
   and state verifier are implemented.
3. Inject copied dimension/configuration/beach inputs and RNG through `WorldLayerMetricsCalculationInput`
   and `IGenerationRandomSource`; the pure calculator and owner system now preserve the confirmed
   Version4 TerrainPass control flow without direct global reads.
4. Route downstream terrain/biome consumers through the read-only snapshot and keep snow tile
   mutations on `IWorldGenerationTileCommitPort`.
5. Compare the complete 13-member legacy and proposed snapshots at the pass boundary, then plan
   removal of the old writer only if the focused verifier passes.

Dependency impact: C02 feeds C03-C12 terrain and generation systems. It consumes configuration,
world dimensions, and RNG ports but does not own them. Cloud rendering, liquid simulation,
WorldStorage, WorldSession, and P18-P20 remain integration inputs with
`crossSubsystemOwner: integration-review`.

Actual checkpoint: the existing state/query/commit files remain unchanged, and the calculator/input
seam is saved under `src/WorldSession/WorldGeneration/Passes`. The calculator preserves the
Version4 initial beach padding, feature-duration selection, special Mountain/Valley branch, metric
ordering, rock/surface clamps, and conditional right-beach Plateau reset. It performs no tile,
liquid, persistence, network, clock, or logging effect. The existing verifier remains at
`Test/Terraria.WorldSession.WorldGeneration.Verification/Program.cs`; the isolated C02 verifier is
`src/WorldGenerationC02FocusedVerifier/Program.cs` and covers the baseline metrics, initial padding,
paired snow-column copies, and generation identity. The current isolated serial build exited `0`
with `0` warnings and `0` errors, and the no-build focused run exited `0` with
`C02 world layer metrics focused verifier passed.`. No legacy source writer was changed or removed.
Tile-history, later snow writes, complete 13-member legacy parity, and full-project integration
remain unverified.

Verification records for this checkpoint:

- Full focused project command: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build','.\Test\Terraria.WorldSession.WorldGeneration.Verification\Terraria.WorldSession.WorldGeneration.Verification.csproj','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')"`; exit `1`, `0` warnings, `5` errors; no fresh C02 artifact. Errors were the unrelated missing `Terraria.Content`/`ColorRgba` symbols in `WorldGenerationTileFramingAndDebugActionsCommand.cs`.
- Controlled focused retry command: the same serial wrapper/project with `-p:DefaultItemExcludes=**/WorldGeneration/Actions/WorldGenerationTileFramingAndDebugActionsCommand.cs`; exit `1`, `0` warnings, `3` errors; no fresh C02 artifact. Errors were the unrelated missing `TilePosition` symbols in `WorldGeneration/Terrain/CrimsonHeartPlacementScratch.cs`.
- Isolated C02 build command: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src\WorldGenerationC02FocusedVerifier\Terraria.WorldGenerationC02FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`, `0` warnings, `0` errors; artifact `Build/bin/Terraria.WorldGenerationC02FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC02FocusedVerifier.dll` verified.
- Isolated C02 focused run command: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\src\WorldGenerationC02FocusedVerifier\Terraria.WorldGenerationC02FocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; output `C02 world layer metrics focused verifier passed.`

## C03 GenVarsSurfaceAndBiomeState

Status: `partial implementation checkpoint`; bounded C03 state, adapter, query, and commit
sources are implemented and focused-verified. Generation calculation and external handoff remain
pending.

Proposed future files:

- `src/WorldSession/WorldGeneration/WorldGenerationLandmassValue.cs` (implemented)
- `src/WorldSession/WorldGeneration/WorldSpawnAndLandmassComponent.cs` (implemented)
- `src/WorldSession/WorldGeneration/WorldSpawnAndLandmassSnapshot.cs` (implemented)
- `src/WorldSession/WorldGeneration/Systems/WorldSpawnAndLandmassSystem.cs` (implemented commit boundary)
- `src/WorldSession/WorldGeneration/SurfaceMaterialDefinition.cs` (implemented)
- `src/WorldSession/WorldGeneration/WorldGenerationLiquidBoundaryComponent.cs` (implemented)
- `src/WorldSession/WorldGeneration/WorldGenerationLiquidBoundarySnapshot.cs` (implemented)
- `src/WorldSession/WorldGeneration/Systems/WorldGenerationLiquidBoundarySystem.cs` (implemented commit boundary)
- `src/WorldSession/WorldGeneration/Adapters/WorldLandmassDefinitionAdapter.cs` (implemented bounded adapter)
- `src/WorldSession/WorldGeneration/Queries/WorldSpawnAndLandmassQuery.cs` (implemented pure query)
- `src/WorldSession/WorldGeneration/Queries/SurfaceMaterialDefinitionQuery.cs` (implemented pure query)
- `src/WorldSession/WorldGeneration/Queries/WorldGenerationLiquidBoundaryQuery.cs` (implemented pure query)
- `src/WorldGenerationC03FocusedVerifier/Program.cs` (implemented focused verifier)
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/WorldSpawnAndLandmassSystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/SurfaceMaterialDefinitionSystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/WorldGenerationLiquidBoundarySystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Adapters/SurfaceMaterialCatalogAdapter.cs`

Planned sequence:

1. Characterize reset/list-clearing, Remix layer, material-selection, counter, and liquid-boundary
   behavior before creating any new source files.
2. Introduce an immutable landmass snapshot and one owner for spawn/Remix facts; keep the legacy
   list behind a compatibility adapter. The component, domain value, copied snapshot, bounded
   adapter, pure query, and generation-identity commit are implemented; the generation calculator
   and legacy writer migration remain pending.
3. Introduce a material definition boundary backed by an explicit world-rule/content adapter; do
   not expose tile IDs as a general write API. The immutable definition and Version4 defaults are
   implemented; the catalog adapter remains pending.
4. Characterize generation and load-time `waterLine` writes, then add the liquid boundary component
   only after the load barrier and LiquidSimulation handoff are approved. The value component and
   snapshot are implemented; the load/commit adapters remain pending.
5. Compare all 15 legacy values with the proposed snapshots and remove no legacy writer until the
   focused verifier passes.

Dependency impact: C03 consumes C02 and world-rule/configuration facts; C04/C05 consume its
committed boundaries. WorldStorage, LiquidSimulation, WorldSession, NpcAndTownSimulation, and
P18-P20 remain integration inputs with `crossSubsystemOwner: integration-review`.

Actual checkpoint: the six state/value files under `src/WorldSession/WorldGeneration`,
`Systems/WorldSpawnAndLandmassSystem.cs`, and
`Systems/WorldGenerationLiquidBoundarySystem.cs` were created. They own only copied state/value
boundaries and have no external effects. `WorldLandmassDefinitionAdapter` now maps confirmed
current-project definition fields into copied value records. `SurfaceMaterialDefinitionQuery`
applies only the evidenced infection flip, while `WorldSpawnAndLandmassQuery` and
`WorldGenerationLiquidBoundaryQuery` expose copied snapshots and perform no external effects.
The C03 focused build exited `0` with `0` warnings and `0` errors; the built verifier apphost
exited `0` with `C03 spawn, material, and liquid-boundary focused verifier passed.`. The wrapper
`run` form exited `0` through the serial wrapper with the focused verifier result above. The
artifact is under `Build/bin/Terraria.WorldGenerationC03FocusedVerifier/Debug/net10.0`. Spawn
calculation, RNG ownership, boulder entity effects, material catalog integration, and the
WorldFile/LiquidSimulation load barrier remain pending.

## C04 GenVarsBeachAndOceanBoundaryState

Status: `partial implementation checkpoint`; implementation remains `in-progress`; the state and
calculator focused verifier passed, while legacy behavior parity remains `not-run`.

Implemented root-`src` files:

- `src/WorldSession/WorldGeneration/BeachBoundaryComponent.cs`
- `src/WorldSession/WorldGeneration/BeachBoundarySnapshot.cs`
- `src/WorldSession/WorldGeneration/Systems/BeachBoundarySystem.cs`
- `src/WorldSession/WorldGeneration/Queries/BeachBoundaryQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/BeachBoundaryCalculationInput.cs`
- `src/WorldSession/WorldGeneration/Queries/BeachBoundaryCalculationQuery.cs`
- `src/WorldSession/WorldGeneration/Systems/BeachBoundaryCalculationSystem.cs`
- `src/WorldGenerationC04FocusedVerifier/Program.cs`
- `src/WorldGenerationC04FocusedVerifier/Terraria.WorldGenerationC04FocusedVerifier.csproj`

Remaining C04 work is legacy parity and integration characterization; no additional C04 production
source file is claimed until the global RNG owner, upstream C02/C03 routing, shell-origin writer,
and scheduler contract are identified.

Planned sequence:

1. Preserve explicit calculation inputs and already-consumed random rolls at the query boundary;
   do not hide global RNG access in the calculator.
2. Keep `BeachBoundaryComponent` as the sole generation-scoped state owner; the C05 avoidance
   counters and ocean treasure scratch state remain separate.
3. Route ocean, cave, dungeon, and jungle predicates through the snapshot; route shell and beach
   mutations through reservation/tile commit ports.
4. Compare all 12 legacy values with the calculated snapshot before planning removal of legacy
   writers. Defer water-volume behavior to LiquidSimulation integration review.

Dependency impact: C04 consumes C02/C03 and feeds C05-C09. It does not own ocean liquid, dungeon,
jungle, storage, session, or P18-P20 execution. Unresolved ownership is
`crossSubsystemOwner: integration-review`.

Rollback: retain legacy beach and shell reads, disable the proposed boundary owner, and remove only
new C04 files if ordering, clamping, RNG, or shell-origin tests diverge.

### C04 Implementation Checkpoint

Implemented the evidence-backed state, commit, and pure-query portion of C04 in the root `src`
project:

- `src/WorldSession/WorldGeneration/BeachBoundaryComponent.cs`
- `src/WorldSession/WorldGeneration/BeachBoundarySnapshot.cs`
- `src/WorldSession/WorldGeneration/Systems/BeachBoundarySystem.cs`
- `src/WorldSession/WorldGeneration/Queries/BeachBoundaryQuery.cs`

The component owns all 12 authoritative boundary integers and returns a value snapshot. The commit
system validates generation identity and replaces the complete boundary in one operation. The pure
query returns a copied snapshot. The focused verifier is
`src/WorldGenerationC04FocusedVerifier/Program.cs` with project
`src/WorldGenerationC04FocusedVerifier/Terraria.WorldGenerationC04FocusedVerifier.csproj`.
Its serial-wrapper build exited `0` with `0` warnings and `0` errors, and its no-build/no-restore
run exited `0` with `C04 beach-boundary focused verifier passed.`. These state units do not sort,
clamp, write tiles/liquid, or reserve structures. No legacy writer was changed.

The calculation unit is implemented by
`src/WorldSession/WorldGeneration/Queries/BeachBoundaryCalculationInput.cs`,
`BeachBoundaryCalculationQuery.cs`, and
`src/WorldSession/WorldGeneration/Systems/BeachBoundaryCalculationSystem.cs`. It accepts explicit
dimension, boundary configuration, dungeon-side, secret-seed flags, already-consumed random rolls,
and shell-origin values. It preserves the sourced half-open interval, applies the non-Remix
tenth-anniversary fixed-boundary branch, selects dungeon versus jungle extra width by dungeon side,
and returns raw no-clamp values. The system validates generation identity and commits the complete
snapshot through `BeachBoundarySystem`; it has no global RNG, tile/liquid, shell-placement, or
scheduler side effect.

The calculator-focused verifier reuses
`src/WorldGenerationC04FocusedVerifier/Terraria.WorldGenerationC04FocusedVerifier.csproj` and
passed the serial-wrapper build with exit `0`, warnings/errors `0/0`, followed by a no-build /
no-restore run with exit `0` and output `C04 beach-boundary focused verifier passed.`. Global RNG
ownership, upstream C02/C03 integration, shell-origin tile/liquid effects, scheduler order, and
legacy behavior equivalence remain unverified.

Verification records:

- Build: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src\WorldGenerationC04FocusedVerifier\Terraria.WorldGenerationC04FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; warnings/errors `0/0`; artifact `Build/bin/Terraria.WorldGenerationC04FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC04FocusedVerifier.dll`.
- Focused run: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\src\WorldGenerationC04FocusedVerifier\Terraria.WorldGenerationC04FocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; output `C04 beach-boundary focused verifier passed.`; no new compile artifact; the verified build artifact remains under `Build/bin/Terraria.WorldGenerationC04FocusedVerifier/Debug/net10.0`.

## C05 WorldGenBeachAndOceanBiomeState

Status: `partial implementation checkpoint`; evidence-backed state, pure-read, and explicit
constraint-calculation boundaries are present in root `src`; focused compile and behavior
verification passed.

Root `src` files present for this checkpoint:

- `src/WorldSession/WorldGeneration/OceanBiomeConstraintComponent.cs`
- `src/WorldSession/WorldGeneration/OceanBiomeConstraintSnapshot.cs`
- `src/WorldSession/WorldGeneration/OceanCaveTreasureStateComponent.cs`
- `src/WorldSession/WorldGeneration/OceanCaveTreasureSnapshot.cs`
- `src/WorldSession/WorldGeneration/OceanBiomePassControlComponent.cs`
- `src/WorldSession/WorldGeneration/Queries/OceanBiomeConstraintQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/OceanBiomeConstraintCalculationInput.cs`
- `src/WorldSession/WorldGeneration/Queries/OceanBiomeConstraintCalculationQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/OceanCaveTreasureQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/OceanBiomePassControlQuery.cs`
- `src/WorldSession/WorldGeneration/Systems/OceanBiomeConstraintCalculationSystem.cs`
- `src/WorldSession/WorldGeneration/Actions/OceanCaveTreasurePlacementCommand.cs`
- `src/WorldSession/WorldGeneration/Queries/OceanCaveTreasureProjection.cs`
- `src/WorldGenerationC05FocusedVerifier/Program.cs`
- `src/WorldGenerationC05FocusedVerifier/Terraria.WorldGenerationC05FocusedVerifier.csproj`

Remaining proposed future files:

- `src/WorldSession/WorldGeneration/Queries/OceanBiomePassPhaseQuery.cs` (blocked: phase-token
  lifetime is not confirmed)

Planned sequence:

1. Preserve the C04 snapshot as the explicit input to the eight-value constraint calculation; do
   not hide global RNG access in the calculator.
2. Introduce bounded treasure state with capacity, count, append/reject, duplicate, reset, and
   immutable-snapshot tests before routing placement commands.
3. Scope `skipDesertTileCheck` to an explicit generation phase token and verify reset on phase exit.
4. Compare all 12 legacy values at the ocean pass barrier; remove no legacy writer or scratch array
   until the focused verifier passes.

Dependency impact: C05 consumes C02-C04 and feeds desert, jungle, cave, lake, and structure
intents. Item/entity effects, tile and liquid commits, WorldStorage, WorldSession,
WorldProgressionAndTransition, and P18-P20 remain integration inputs with
`crossSubsystemOwner: integration-review`.

Rollback: retain legacy avoidance and treasure paths, disable new C05 owners, and remove only new
C05 files if capacity, coordinate order, eligibility, phase isolation, or RNG tests diverge.

### C05 Implementation Checkpoint

Implemented the evidence-backed C05 state, pure-read, and explicit constraint-calculation portion
in the root `src` project:

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
- `src/WorldSession/WorldGeneration/Queries/OceanBiomeConstraintCalculationInput.cs`,
  `OceanBiomeConstraintCalculationQuery.cs`, and
  `src/WorldSession/WorldGeneration/Systems/OceanBiomeConstraintCalculationSystem.cs` derive and
  commit all eight constraint values from an explicit C04 `BeachBoundarySnapshot`. They preserve
  the sourced reset formulas: ocean random maximum is the minimum plus `40`, forced jungle length
  is `275`, evil-biome avoidance is the beach random center plus `60`, the middle fixer is `50`,
  and the four remaining avoidance values are the center plus `20`. The query is pure and consumes
  no global RNG; the system validates generation identity before complete commit.
- `src/WorldSession/WorldGeneration/Actions/OceanCaveTreasurePlacementCommand.cs` and
  `src/WorldSession/WorldGeneration/Queries/OceanCaveTreasureProjection.cs` project each copied
  treasure anchor, in source order, to the fixed Version4 underwater-chest call contract:
  selected item input, `notNearOtherChests: false`, `chestStyle: 17`, `trySlope: true`, and
  `chestTileType: 0`. The projection is pure and does not choose items, search candidate tiles,
  or apply chest/tile/liquid effects.

The new calculator-focused verifier was first run as a RED check: the serial-wrapper build exited
`1` with three expected missing-type compiler errors (`CS0246`/`CS0103`) before the production
calculation files existed. After implementation, the serial-wrapper build exited `0` with `0`
warnings and `0` errors; the no-build/no-restore run exited `0` with
`C05 ocean-biome focused verifier passed.`. The global RNG seam, duplicate policy, phase-token
protocol, candidate search, chest/item/tile/liquid effect adapter, and legacy-parity verifier
remain pending because their current-project owners and behavior contracts are not confirmed. The
placement-command projection itself is focused-verified. No legacy writer was changed or removed.

Verification records:

- Build: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src\WorldGenerationC05FocusedVerifier\Terraria.WorldGenerationC05FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; warnings/errors `0/0`; artifact `Build/bin/Terraria.WorldGenerationC05FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC05FocusedVerifier.dll`.
- Focused run: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\src\WorldGenerationC05FocusedVerifier\Terraria.WorldGenerationC05FocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; output `C05 ocean-biome focused verifier passed.`; no new compile artifact; the verified build artifact remains under `Build/bin/Terraria.WorldGenerationC05FocusedVerifier/Debug/net10.0`.

### C05 Placement Projection Continuation Checkpoint

`OceanCaveTreasurePlacementCommand` and `OceanCaveTreasureProjection` now form the pure
placement-intent boundary for committed ocean-cave anchors. The projection preserves generation
identity and coordinate order, emits the evidenced underwater-chest constants (`notNearOtherChests:
false`, style `17`, `trySlope: true`, tile type `0`), and rejects negative item IDs even for an
empty snapshot. It does not call RNG, tile/liquid, chest, item, persistence, or network owners.

The verifier also covers the empty projection result and input validation. The serial-wrapper build
and no-build focused run above both exited `0` with `0` warnings and `0` errors, with output
`C05 ocean-biome focused verifier passed.`. Candidate coordinate search, duplicate policy,
phase-token lifetime, selected-item RNG ownership, and real placement effects remain pending.

## C06 WorldGenUndergroundDesertStructureState

Status: `partial implementation checkpoint`; state, pure-query, generation-identity commit, and
bounded recorder boundaries are implemented in root `src`; focused verification passed. Calculator,
adapter, and external effect integration remain pending.

Implemented root-`src` files:

- `src/WorldSession/WorldGeneration/UndergroundDesertRectangle.cs`
- `src/WorldSession/WorldGeneration/UndergroundDesertStructureComponent.cs`
- `src/WorldSession/WorldGeneration/UndergroundDesertStructureSnapshot.cs`
- `src/WorldSession/WorldGeneration/UndergroundDesertLarvaPlacementComponent.cs`
- `src/WorldSession/WorldGeneration/UndergroundDesertLarvaPlacementSnapshot.cs`
- `src/WorldSession/WorldGeneration/Queries/UndergroundDesertStructureQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/UndergroundDesertLarvaPlacementQuery.cs`
- `src/WorldGenerationC06FocusedVerifier/Program.cs`
- `src/WorldGenerationC06FocusedVerifier/Terraria.WorldGenerationC06FocusedVerifier.csproj`

Remaining proposed future files:

- `src/WorldSession/WorldGeneration/Adapters/UndergroundDesertRectangleAdapter.cs` (blocked:
  Version4 rectangle edge semantics are not closed)

Planned sequence:

1. Characterize reset values, rectangle edge semantics, hive normalization, the desert retry loop,
   and the `skipDesertTileCheck` phase interaction.
2. Define layout values with a stable rectangle type at the adapter boundary; keep `StructureMap`
   and `DesertBiome` external to core authority.
3. Define bounded larva coordinate state with paired-array, capacity, duplicate, and reset tests.
4. Route structure reservations before tile/entity intents and compare the complete nine-member
   legacy snapshot at the desert pass barrier.
5. Remove no legacy writer or mutable array until the focused verifier and integration review pass.

Dependency impact: C06 consumes C02-C05 and feeds C07-C09 structure/cave inputs. Tile, entity,
liquid, storage, session, and P18-P20 execution remain integration inputs with
`crossSubsystemOwner: integration-review`.

Rollback: retain the legacy desert and larva paths, disable new C06 owners, and remove only new C06
files if bounds, retry, reservation, coordinate order, or reset/restore tests diverge.

### C06 Implementation Checkpoint

Implemented the evidence-backed state and pure-query portion in root `src`:

- The structure component, snapshot, and explicit rectangle value own the two rectangle values and
  four hive-bound integers.
- The larva component and snapshot own a capacity-100, count-prefixed coordinate buffer and copy
  only used positions; full appends return `false` without mutation.
- The two query files expose pure copied reads.
- `UndergroundDesertStructureSystem.cs` and `UndergroundDesertLarvaPlacementSystem.cs` own the
  generation-identity commit and bounded append/clear operations without external effects.
- `src/WorldGenerationC06FocusedVerifier/Program.cs` and
  `src/WorldGenerationC06FocusedVerifier/Terraria.WorldGenerationC06FocusedVerifier.csproj`
  verify these boundaries.
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

Verification records:

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

The first focused build attempt exposed CS0118 at the constructor assignment `Phase = phase`.
The saved source now assigns the public `Timing` property. The subsequent serial focused build
exited `0` with `0` warnings and `0` errors and produced
`Build/bin/Terraria.WorldGenerationC06FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC06FocusedVerifier.dll`.
The no-build focused run exited `0` with `C06 underground-desert focused verifier passed.`

The first focused build attempt exposed CS0118 at the constructor assignment `Phase = phase`.
The saved source now assigns the public `Timing` property instead. The compile defect is recorded
as corrected, but no fresh build or run result is claimed yet.

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

## C07 WorldGenJungleStructureState

Status: `partial implementation checkpoint`; state, pure-query, generation-identity commit, and
the independently evidenced jungle-hut material mapping are implemented in root `src`; calculator,
placement effects, and legacy parity remain pending.

Implemented root-`src` files:

- `src/WorldSession/WorldGeneration/JungleRegionStructureComponent.cs`
- `src/WorldSession/WorldGeneration/JungleRegionStructureSnapshot.cs`
- `src/WorldSession/WorldGeneration/PyramidPlacementStateComponent.cs`
- `src/WorldSession/WorldGeneration/PyramidPlacementSnapshot.cs`
- `src/WorldSession/WorldGeneration/JungleChestAndLootGenerationStateComponent.cs`
- `src/WorldSession/WorldGeneration/JungleChestAndLootGenerationSnapshot.cs`
- `src/WorldSession/WorldGeneration/Queries/JungleRegionStructureQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/PyramidPlacementQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/JungleChestAndLootGenerationQuery.cs`
- `src/WorldSession/WorldGeneration/Systems/JungleRegionStructureSystem.cs`
- `src/WorldSession/WorldGeneration/Systems/PyramidPlacementCommitSystem.cs`
- `src/WorldSession/WorldGeneration/Systems/JungleChestPlacementSystem.cs`
- `src/WorldSession/WorldGeneration/Definitions/JungleHutMaterialDefinition.cs`
- `src/WorldSession/WorldGeneration/Queries/JungleHutMaterialDefinitionQuery.cs`
- `src/WorldSessionFocusedVerifier/Program.cs`
- `src/WorldGenerationC07MaterialFocusedVerifier/Program.cs`
- `src/WorldGenerationC07MaterialFocusedVerifier/Terraria.WorldGenerationC07MaterialFocusedVerifier.csproj`

Remaining proposed future files:

- `src/WorldSession/WorldGeneration/Systems/JungleRegionStructureCalculatorSystem.cs` (blocked:
  material, world-rule, and RNG owner is not confirmed)

Planned sequence:

1. Characterize reset and material mapping, region bounds, dynamic pyramid array allocation, chest
   capacity, and loot cursor/flag transitions.
2. Define separate region, pyramid, and chest/loot snapshots with paired-array ownership and no
   external tile or inventory types in core state.
3. Route pyramid and jungle structure candidates through reservation results before tile commits.
4. Route chest coordinates to explicit item/structure commands; keep `JungleItemCount` and the wand
   flag behind a pure loot policy and a generated-result commit.
5. Compare all 15 legacy values at the jungle/pyramid/chest pass barriers; remove no writer until
   focused verifiers and integration review pass.

Dependency impact: C07 consumes C02-C06 and feeds C08-C09 structure/cave and item handoffs. Tile,
item, NPC, storage, session, progression, and P18-P20 execution remain integration inputs with
`crossSubsystemOwner: integration-review`.

Rollback: retain legacy jungle/pyramid/chest paths, disable new C07 owners, and remove only new C07
files if capacity, ordering, RNG, loot, reservation, or snapshot tests diverge.

### C07 Implementation Checkpoint

Implemented the evidence-backed state and pure-query portion in root `src`:

- The region component/snapshot own the seven scalar region/material facts and mud-wall flag.
- The pyramid component/snapshot preserve the caller-supplied dynamic capacity and paired coordinate
  order.
- The jungle chest/loot component/snapshot preserve the capacity-100 paired coordinates, count,
  selection cursor, and unique wand flag.
- The three query files expose copied snapshots only.

The generation-identity commit boundaries are also implemented:

- `src/WorldSession/WorldGeneration/PyramidPlacementStateComponent.cs` accepts only bounded,
  paired coordinate lists and copies them into component-owned storage.
- `src/WorldSession/WorldGeneration/Systems/PyramidPlacementCommitSystem.cs` validates generation
  identity, caller-preserved dynamic capacity, count, and exact paired-list length before committing
  the snapshot.
- `src/WorldSession/WorldGeneration/Systems/JungleChestPlacementSystem.cs` validates generation
  identity, fixed capacity 100, count, and exact paired-list length before committing the loot cursor,
  wand result, and chest coordinates.
- `src/WorldSession/WorldGeneration/Queries/JungleRegionStructureQuery.cs` also exposes the pure
  inclusive `jungleMinX..jungleMaxX` conversion-range predicate.
- `JungleChestAndLootGenerationStateComponent.ReplaceState` copies validated coordinate lists through
  temporary arrays before replacing component-owned state.
- The focused verifier covers successful commits, cross-generation rejection, invalid capacity/count
  rejection, and owner-state preservation after rejection. List-bearing snapshots are compared by
  value because record-struct equality otherwise compares list references.

The final serial build and no-build focused verifier passed. The pure material mapping verifier
also passed: it preserves the selected hut tile ID, maps `119/120/158/175/45` to wall
`23/24/42/45/10`, and preserves the sourced wall `0` default for other tile IDs. This checkpoint
also tightened
`PyramidPlacementStateComponent.ReplaceState` to require exact X/Y used-range lengths, matching
the commit system's rejection-before-write contract. No external material registration, RNG,
duplicate policy, reservation, tile, item, NPC, persistence, or network behavior was inferred. No
legacy writer was changed or removed.

The material verifier was isolated from the aggregate `WorldSession` project because the shared
checkout still contains unrelated duplicate C10 declarations. Its serial build and no-build run
both passed:

- Build: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src\\WorldGenerationC07MaterialFocusedVerifier\\Terraria.WorldGenerationC07MaterialFocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; warnings/errors `0/0`; artifact `Build/bin/Terraria.WorldGenerationC07MaterialFocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC07MaterialFocusedVerifier.dll`.
- Focused run: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\\src\\WorldGenerationC07MaterialFocusedVerifier\\Terraria.WorldGenerationC07MaterialFocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; output `C07 jungle-hut material focused verifier passed.`

The region-range verifier source is `src/WorldGenerationC07RegionFocusedVerifier/Program.cs` with
project `src/WorldGenerationC07RegionFocusedVerifier/Terraria.WorldGenerationC07RegionFocusedVerifier.csproj`.

The independently evidenced `JungleRegionStructureQuery.IsWithinJungleConversionRange` preserves
the Version4 inclusive range predicate without normalizing reversed bounds. Its isolated serial
build and no-build run both passed:

- Build: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src\\WorldGenerationC07RegionFocusedVerifier\\Terraria.WorldGenerationC07RegionFocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; warnings/errors `0/0`; artifact `Build/bin/Terraria.WorldGenerationC07RegionFocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC07RegionFocusedVerifier.dll`.
- Focused run: `pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\\src\\WorldGenerationC07RegionFocusedVerifier\\Terraria.WorldGenerationC07RegionFocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit `0`; output `C07 jungle-region focused verifier passed.`

### C07 Jungle Boundary Calculation Checkpoint

The independently evidenced jungle-boundary calculation is implemented as a pure query over
explicit copied column observations. `JungleRegionBoundsCalculationInput` validates that the
observations cover the complete `maxTilesX` width and copies the source list before calculation.
`JungleRegionBoundsCalculationQuery` preserves the Version4 scan ranges exactly: the left scan
starts at `x = 5` and uses `x < maxTilesX - 5`, while the right scan starts at
`x = maxTilesX - 5` and uses `x > 5`. It returns the first left hit and first right-to-left hit,
and retains the sourced `0/0` defaults when no column matches. It does not read tiles or
`worldSurface`, mutate walls, consume RNG, or commit region state.

Actual source changes in this checkpoint:

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

The independently evidenced base jungle-chest item selector and cursor transition are implemented
in `src`:

- `src/WorldSession/WorldGeneration/Queries/JungleChestLootSelectionQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/JungleChestLootSelectionRandomInput.cs`
- `src/WorldSession/WorldGeneration/Queries/JungleChestLootSelectionResult.cs`
- `src/WorldSession/WorldGeneration/Systems/JungleChestLootSelectionSystem.cs`
- `src/WorldGenerationC07LootFocusedVerifier/Program.cs`
- `src/WorldGenerationC07LootFocusedVerifier/Terraria.WorldGenerationC07LootFocusedVerifier.csproj`

The pure query preserves the sourced `JungleItemCount % 4` cycle, the 50/15/20 short-circuit
override priority, and the next-cursor result. The system commits only the cursor and preserves
the wand flag and chest coordinates. The serial-wrapper build exited `0` with `0` warnings and
`0` errors; the no-build/no-restore run exited `0` with `C07 jungle-loot focused verifier passed.`.
Exact random-source consumption order, wand uniqueness, external chest/item effects, and legacy
parity remain pending.

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

Status: `component boundary implemented`; the independently evidenced dungeon/island components,
snapshots, and focused verifier are present in root `src`; production dungeon-record and island
execution integration remains pending.

Existing root-src component files:

- `src/WorldSession/WorldGeneration/DungeonLayoutControlComponent.cs`
- `src/WorldSession/WorldGeneration/DungeonSpecialRewardGenerationComponent.cs`
- `src/WorldSession/WorldGeneration/FloatingIslandPlacementStateComponent.cs`
- `src/WorldSession/WorldGeneration/DungeonBoundaryDefinition.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/DungeonLayoutControlSystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/DungeonSpecialRewardGenerationSystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/FloatingIslandPlacementSystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Adapters/DungeonGenVarsAdapter.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Queries/DungeonLayoutQuery.cs`

Planned sequence:

1. Characterize reset/clear, dungeon-record creation, active-index switching, lower clamping,
   upper bounds, altar/bound pairing, and nested `DungeonGenVars` mutation.
2. Define a single dungeon-record authority and adapter; expose `CurrentDungeon` and
   `CurrentDungeonGenVars` through one query seam rather than duplicating state.
3. Define unique reward result state and explicit item/progression commands; do not map flags to
   inventory or progression directly.
4. Define bounded floating-island metadata and paired coordinate snapshots with capacity and
   duplicate-reservation tests.
5. Compare all 19 legacy values at the dungeon/island barriers before planning legacy writer
   removal. Defer execution ownership to integration review.

Dependency impact: C08 consumes C02-C07 and feeds C09/C12 structure and item handoffs; C13 reads
its record snapshot. WorldStorage, WorldSession, WorldProgressionAndTransition, LiquidSimulation,
and P18-P20 execution remain integration inputs with
`crossSubsystemOwner: integration-review`.

Rollback: retain legacy dungeon/island/reward paths, disable only the isolated C08 component
boundary if index, nested-record, reward, capacity, pairing, reservation, or tile-order evidence
diverges. No C08 component rollback is indicated by the current focused verifier.

### C08 Implementation Checkpoint

The root `src` C08 components are implemented and focused-verified. `DungeonLayoutControlComponent`
owns the seven dungeon layout values, copied opaque record identifiers, and active index;
`DungeonSpecialRewardGenerationComponent` owns the two reward flags; and
`FloatingIslandPlacementStateComponent` owns the sky-lake values and capacity-300 paired house
metadata. `DungeonBoundaryDefinition` preserves the read-only beach-padding definition.

Serial build command:
`pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src\\WorldGenerationC08FocusedVerifier\\Terraria.WorldGenerationC08FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`

Result: exit `0`, `0` warnings, `0` errors; artifact:
`Build/bin/Terraria.WorldGenerationC08FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC08FocusedVerifier.dll`.

Focused run command:
`pwsh -NoProfile -Command "& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\\src\\WorldGenerationC08FocusedVerifier\\Terraria.WorldGenerationC08FocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`

Result: exit `0`; output `C08 dungeon-island focused verifier passed.` External
`DungeonGenVars` ownership, dungeon/island tile and structure effects, secret-seed routing,
persistence, network, and legacy behavior equivalence remain integration boundaries.

## C09 GenVarsCaveTunnelAndOrePatchState

Status: `partial implementation checkpoint`; bounded state boundaries are implemented, while fresh
verification and external effect integration remain pending.

Proposed future files:

- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/MountainCaveHistoryComponent.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/SurfaceTunnelHistoryComponent.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/SurfaceOrePatchHistoryComponent.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/MountainCaveHistorySystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/SurfaceTunnelHistorySystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Systems/SurfaceOreAndStoneSystem.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Queries/CaveTunnelAvoidanceQuery.cs`
- `dome/src/Terraria.Dome.Simulation/WorldGeneration/Adapters/LegacyCaveTunnelOreHistoryAdapter.cs`

Planned sequence:

1. Characterize the nine member declarations, every Version4 reader/writer, effective capacity
   thresholds, and the pass ordering around Tunnels, MountainCaves, Webs, Lakes,
   MountainCaveOpenings, and SurfaceOreAndStone.
2. Define three generation-session-scoped bounded histories. Keep the cave X/Y arrays paired and
   preserve ordered used prefixes; keep tunnel and ore-patch histories as X-only values with their
   declared capacity definitions separate from writable counts.
3. Add explicit reset and snapshot-copy seams. Confirm the source behavior for `numTunnels` and
   `numOrePatch`, which are not assigned in the inspected `WorldGen.Reset` excerpt, before removing
   or changing any legacy initialization behavior.
4. Adapt MountainCaves and Tunnels to return history append results alongside tile/liquid command
   batches. Ensure failed candidates do not append and a failed commit cannot leave an unverified
   history result.
5. Adapt SurfaceOreAndStone so the C14 saved-tier selection is an explicit input, `OrePatch` tile
   work stays behind the ore/tile commit port, and `orePatchX` appends only after the source success
   condition. Preserve the source's full-capacity ordering until it is characterized.
6. Verify immutable history readers for Webs, Lakes, MountainCaveOpenings, living-tree exclusion,
   and StonePatch spacing. Compare all nine legacy values at each relevant pass barrier before
   planning legacy writer removal.

Dependency impact: C09 consumes C01 generation ore-selection input and C02-C08 terrain, boundary,
and structure facts. It feeds Webs, Lakes, MountainCaveOpenings, living-tree exclusion,
SurfaceOreAndStone, and later generation execution. Tile, liquid, storage, session, and P19-P20
execution remain integration inputs with `crossSubsystemOwner: integration-review`.

Rollback: retain legacy cave, tunnel, and surface-ore history paths, disable only new C09 owners, and
remove only newly introduced C09 files if capacity, reset, spacing, random-stream, snapshot, or
history-to-command ordering tests diverge.

### C09 Implementation Checkpoint

The three independently evidenced C09 history boundaries are saved in root `src`:

- `MountainCaveHistoryComponent` and `MountainCaveHistorySnapshot` own a generation-scoped,
  capacity-30 ordered prefix of paired cave-origin X/Y values. `MountainCaveHistorySystem` exposes
  append and clear operations without applying cave, tile, liquid, RNG, persistence, or network
  effects.
- `SurfaceTunnelHistoryDefinition`, `SurfaceTunnelHistoryComponent`, and
  `SurfaceTunnelHistorySnapshot` preserve the declared capacity 50 and Version4's effective append
  capacity 49 for ordered tunnel-center X values. `SurfaceTunnelHistorySystem` and
  `SurfaceTunnelHistoryQuery` expose bounded recording, clearing, and copied reads only.
- `SurfaceOrePatchHistoryDefinition`, `SurfaceOrePatchHistoryComponent`, and
  `SurfaceOrePatchHistorySnapshot` preserve the declared capacity 50 and effective append capacity
  49 for ordered ore-patch X values. `SurfaceOrePatchHistorySystem` accepts an append only when the
  caller supplies an explicit successful external patch result; its query exposes copied state only.

All three snapshots copy only their used prefixes into read-only lists. Appends at effective capacity
are rejected without changing the owner, and clearing resets only the used count. Version4 records
history around external `Mountinater`, ten-point tunnel, `TileRunner`, and `OrePatch` effects; those
effect boundaries are intentionally not implemented. Legacy overflow/reset characterization,
dependent-reader adapters, RNG ownership, saved-tier handoff, and history-plus-tile/ore transaction
policy remain unverified. No legacy writer was changed or removed. The isolated C09 verifier build
and run passed through the serial repository wrapper; the full WorldSession build was separately
blocked by duplicate C10 declarations already present in the shared checkout.

## C10 GenVarsMushroomBiomeAndLogState

Status: `implemented focused-verification checkpoint`; the bounded history implementation is saved
and the isolated focused verifier passed.

Saved source files:

- `src/WorldSession/WorldGeneration/Components/MushroomBiomeCapacityDefinition.cs`
- `src/WorldSession/WorldGeneration/Components/MushroomBiomeAnchorStateComponent.cs`
- `src/WorldSession/WorldGeneration/Components/MushroomBiomeAnchorStateSnapshot.cs`
- `src/WorldSession/WorldGeneration/Components/FallenLogFlowerHandoffComponent.cs`
- `src/WorldSession/WorldGeneration/Components/FallenLogFlowerHandoffSnapshot.cs`
- `src/WorldSession/WorldGeneration/Systems/MushroomBiomeGenerationSystem.cs`
- `src/WorldSession/WorldGeneration/Systems/FallenLogFlowerHandoffSystem.cs`
- `src/WorldSession/WorldGeneration/Queries/MushroomBiomeAnchorQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/FallenLogFlowerHandoffQuery.cs`
- `src/WorldGenerationC10FocusedVerifier/Program.cs`
- `src/WorldGenerationC10FocusedVerifier/Terraria.WorldGenerationC10FocusedVerifier.csproj`

The saved code covers bounded copied mushroom-anchor state and the explicit fallen-log handoff
transition. Its isolated serial build exited `0` with `0` warnings and `0` errors, and the no-build
focused run exited `0` with `C10 mushroom and fallen-log focused verifier passed.`. It does not wire
ShroomPatch, Flowers, tile/liquid effects, RNG, persistence, network, or legacy writers.

Planned sequence:

1. Characterize the five declarations, the 50-anchor cap, strict prior-anchor distance checks,
   six `ShroomPatch` calls, the missing explicit mushroom reset, and generic GenVars snapshot
   behavior before adding a new owner.
2. Define an ordered, bounded mushroom-anchor component plus a read-only capacity definition. Keep
   `Point` pairing and used-prefix semantics explicit; do not store tile registries, dungeon bounds,
   desert rectangles, or mutable external arrays in the component.
3. Make `MushroomBiomeGenerationSystem` append an anchor only after the six patch commands have a
   successful commit result. Preserve candidate retry order and route tile work through a tile
   commit port.
4. Define `FallenLogFlowerHandoffComponent` as a paired one-shot handoff. Preserve the `logX = -1`
   sentinel, overwrite-last-selected behavior, and source compatibility in which Flowers clears
   `logX` but may leave `logY` unchanged.
5. Route `FallenLogsAndWaterFeatures` and both Remix/normal Flowers branches through explicit
   producer/consumer systems. Keep placed-log tiles, liquid effects, flowers, tree effects, items,
   NPCs, and persistence outside the handoff component.
6. Compare all five legacy values at the GlowingMushroomPatches, FallenLogsAndWaterFeatures, and
   Flowers barriers before removing a legacy writer. Retain a compatibility adapter until reset,
   snapshot-copy, and commit-order verifiers pass.

Dependency impact: C10 consumes C02-C08 terrain, dungeon, desert, and world-rule facts and feeds
flower/plant generation and later execution. It does not own tile, liquid, storage, session, NPC,
progression, or P18-P20 effects; unresolved ownership remains
`crossSubsystemOwner: integration-review`.

Rollback: retain the legacy mushroom and fallen-log paths, disable only the new C10 owners, and
remove only new C10 files if reset, anchor ordering, six-patch commit, handoff sentinel, overwrite,
or stale-`logY` compatibility tests diverge.

## C11 GenVarsLakeAndOasisState

Status: `implemented focused-verification checkpoint`; the bounded lake/oasis state, query, and
commit-result boundaries are saved and the isolated focused verifier passed.

Saved source files:

- `src/WorldSession/WorldGeneration/Components/LakePlacementHistoryComponent.cs`
- `src/WorldSession/WorldGeneration/Components/LakePlacementCapacityDefinition.cs`
- `src/WorldSession/WorldGeneration/Components/LakePlacementHistorySnapshot.cs`
- `src/WorldSession/WorldGeneration/Components/OasisPlacementHistoryComponent.cs`
- `src/WorldSession/WorldGeneration/Components/OasisPlacementCapacityDefinition.cs`
- `src/WorldSession/WorldGeneration/Components/OasisHeightDefinition.cs`
- `src/WorldSession/WorldGeneration/Components/OasisPlacementHistorySnapshot.cs`
- `src/WorldSession/WorldGeneration/Systems/LakeGenerationSystem.cs`
- `src/WorldSession/WorldGeneration/Systems/LakePlacementHistoryAppendResult.cs`
- `src/WorldSession/WorldGeneration/Systems/LakePlacementHistoryAppendStatus.cs`
- `src/WorldSession/WorldGeneration/Systems/OasisGenerationSystem.cs`
- `src/WorldSession/WorldGeneration/Systems/OasisPlacementHistoryAppendResult.cs`
- `src/WorldSession/WorldGeneration/Systems/OasisPlacementHistoryAppendStatus.cs`
- `src/WorldSession/WorldGeneration/Queries/LakePlacementHistoryQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/OasisPlacementHistoryQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/OasisPlacementQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/LakeOasisAvoidanceQuery.cs`
- `src/WorldGenerationC11FocusedVerifier/Program.cs`
- `src/WorldGenerationC11FocusedVerifier/Terraria.WorldGenerationC11FocusedVerifier.csproj`

The saved code owns separate lake X-only and oasis paired center/width histories. It preserves
lake's effective 49-entry stop, oasis capacity 20, width range 45..60, strict distance queries,
copied used-prefix snapshots, mixed-generation rejection, append-after-success gates, and clear
behavior. It does not call `SonOfLakinater`, `PlaceOasis`, RNG, tile/liquid APIs, vegetation, or
legacy writers.

Planned sequence:

1. Characterize the eight declarations, C09 cave/tunnel avoidance, strict spacing comparisons,
   lake `maxLakes - 1` stop behavior, oasis full-capacity metadata asymmetry, and missing explicit
   reset assignments before introducing an owner.
2. Define separate bounded lake-X and paired oasis center/width histories. Keep readonly capacities
   50, 20, and oasis height 20 as definitions; do not merge lake and oasis state merely because both
   are used by world-generation water/biome passes.
3. Route Lakes candidate checks through a pure query over C04/C05 and immutable C09 snapshots.
   Execute `SonOfLakinater` through `LakeTerrainLiquidCommitPort`, then append `LakeX` only after a
   successful commit while preserving the source's effective stop threshold.
4. Route `PlaceOasis` through `OasisTerrainLiquidCommitPort`, preserving random width 45..60,
   strict prior-center distance, terrain/liquid mutation ordering, and the source behavior when the
   metadata array is full. Append the paired center/width snapshot only at the compatible point.
5. Route CactusPalmTreesAndCoral through a read-only oasis snapshot and keep cactus, sea-oat, plant,
   tile, liquid, and framing effects behind their existing explicit adapters.
6. Compare all eight values at Lakes, Oasis, CactusPalmTreesAndCoral, and snapshot barriers before
   removing a legacy writer. Retain compatibility adapters until reset, copy-isolation, capacity,
   avoidance, and commit-order verifiers pass.

Dependency impact: C11 consumes C04/C05 boundary facts and C09 cave/tunnel histories and feeds
downstream oasis vegetation and later generation execution. It does not own LiquidSimulation,
WorldStorage, WorldSession, tile commits, item/entity effects, or P18-P20 execution; unresolved
ownership remains `crossSubsystemOwner: integration-review`.

Rollback: retain legacy lake/oasis paths, disable only the new C11 owners, and remove only new C11
files if reset, capacity, C09 avoidance, strict spacing, paired metadata, full-capacity, or
tile/liquid commit-order tests diverge.

### C11 Implementation Checkpoint

The root `src` implementation contains the independently evidenced C11 state, query, and
commit-result boundaries. Lake history preserves capacity 50 and the effective 49-entry source
stop; oasis history preserves capacity 20, height 20, paired center/width records, and the
accepted width range 45..60. Snapshots copy only the used prefix. The pure queries enforce strict
lake, cave, tunnel, and oasis spacing, and reject mixed-generation lake/cave/tunnel inputs.
The lake and oasis systems append metadata only after an explicit external terrain/liquid success
result; they do not own RNG, tile, liquid, `SonOfLakinater`, `PlaceOasis`, or vegetation effects.
The focused verifier covers capacity, ordering, append-after-success, strict boundaries, paired
metadata, full-capacity rejection, mixed-generation rejection, and snapshot isolation. Its isolated
serial build exited `0` with `0` warnings and `0` errors, and the no-build focused run exited `0`
with `C11 lake and oasis focused verifier passed.`.

Candidate scanning, RNG ownership, terrain/liquid mutation, reset and snapshot-restore ownership,
downstream vegetation, persistence, network, and legacy-writer replacement remain evidence gaps and
integration-review boundaries.

## C12 GenVarsHellAndSpecialStructures

Status: `partial implementation checkpoint`; C12 bounded state, query, and explicit commit
boundaries are implemented and focused-verified. External effects and legacy replacement remain
pending.

Saved source files under `src/WorldSession/WorldGeneration`:

- `Components/HellChestLootCycleComponent.cs`
- `Components/HellChestLootCycleSnapshot.cs`
- `Components/StatuePlacementOption.cs`
- `Components/StatuePlacementCatalogDefinition.cs`
- `Components/StatueTrapSelectionDefinition.cs`
- `Components/InfectionAlignmentComponent.cs`
- `Components/InfectionAlignmentSnapshot.cs`
- `Components/SpecialSeedGenerationRuleFlagsComponent.cs`
- `Components/SpecialSeedGenerationRuleFlagsSnapshot.cs`
- `Components/ShimmerBiomeAnchorPoint.cs`
- `Components/ShimmerBiomeAnchorComponent.cs`
- `Components/ShimmerBiomeAnchorSnapshot.cs`
- `Queries/HellChestLootCycleQuery.cs`
- `Queries/StatuePlacementCatalogQuery.cs`
- `Queries/StatueTrapRuleQuery.cs`
- `Queries/InfectionAlignmentQuery.cs`
- `Queries/SpecialSeedGenerationRuleQuery.cs`
- `Queries/ShimmerAnchorQuery.cs`
- `Systems/HellChestGenerationSystem.cs`
- `Systems/GenerationRuleInitializationSystem.cs`
- `Systems/InfectionGenerationSystem.cs`
- `Systems/ShimmerBiomeCommitStatus.cs`
- `Systems/ShimmerBiomeCommitResult.cs`
- `Systems/ShimmerGenerationSystem.cs`
- `../WorldGenerationC12FocusedVerifier/Program.cs`
- `../WorldGenerationC12FocusedVerifier/Terraria.WorldGenerationC12FocusedVerifier.csproj`

Planned sequence:

1. Characterize reset-derived flags, hell-chest table construction, cursor advancement, statue
   catalog order, trap-index membership, Shimmer retry/commit, and the absence of direct WorldFile/
   NetMessage paths before introducing new owners.
2. Define immutable copied statue catalog and trap-index definitions. Preserve `Point16` tile/style
   pairing and list-index identity; keep tile, wire, and pressure-plate effects external.
3. Define `HellChestLootCycleComponent` with an immutable sequence snapshot and explicit successful
   chest-placement advance. Preserve normal versus Remix item pools and the no-advance-on-failure
   rule; characterize the missing explicit cursor reset before changing it.
4. Define infection alignment and special-seed rule snapshots from the exact `WorldGen.Reset`
   conjunctions. Route conversion, chest branching, spike-cave, and related generation decisions
   through pure queries and explicit tile/structure commands.
5. Define Shimmer anchor reset, candidate selection, commit result, and protected-structure
   reservation as separate seams. The anchor is published only after successful `ShimmerMakeBiome`
   behavior and before later exclusion readers.
6. Compare all nine legacy values at reset, Shimmer, Statues, infection, chest-placement, and
   snapshot barriers before removing any legacy writer. Retain compatibility adapters until cursor,
   index, seed-rule, reset, snapshot-copy, and anchor/reservation verifiers pass.

Dependency impact: C12 consumes C08 dungeon and world-rule facts and feeds C13 queries, P20
generation actions, infection, structure, tile, liquid, item, NPC, progression, storage, and
session handoffs. Those external owners remain unresolved with
`crossSubsystemOwner: integration-review`.

Rollback: retain legacy hell-chest, statue, infection, special-rule, and Shimmer paths, disable
only the new C12 owners, and remove only new C12 files if cursor advancement, catalog order, trap
indexes, seed-rule combinations, reset/restore, Shimmer commit, or protected-structure ordering
tests diverge.

### C12 Implementation Checkpoint

The root `src` implementation contains the independently evidenced C12 state, query, and
commit-result boundaries. Hell chest state copies and validates the five-entry normal/Remix item
pool and advances its cursor only after a successful placement, with wrap-around and failure
preservation. Statue definitions copy the exact 73-entry Version4 tile/style order and trap index
set `{4, 7, 10, 18}`. Infection and special-seed components preserve the exact reset conjunctions
for `flipInfections`, `notTheBeesAndForTheWorthyNoCelebration`, and
`noTrapsAndForTheWorthyNoCelebration`. Shimmer state publishes only after an explicit successful
biome commit and exposes strict-distance exclusion reads plus explicit reset.

The verifier is `src/WorldGenerationC12FocusedVerifier/Program.cs` with project
`src/WorldGenerationC12FocusedVerifier/Terraria.WorldGenerationC12FocusedVerifier.csproj`.
The serial build exited `0` with `0` warnings and `0` errors; the no-build focused run exited `0`
with `C12 hell and special-structure focused verifier passed.` The build artifact is under
`Build/bin/Terraria.WorldGenerationC12FocusedVerifier/Debug/net10.0`.

No Chest/item, tile, wire, liquid, protected-structure, RNG stream, persistence/network,
WorldGenSnapshot restore, or legacy-writer integration is implemented by this checkpoint.

## C13 GenVarsDungeonDerivedProperties

Status: `partial implementation checkpoint`; C13 query/control/adapters are implemented and
focused-verified. Production backing and snapshot integration remain pending.

Saved source files under `src/WorldSession/WorldGeneration`:

- `Adapters/DungeonControlLineAdapter.cs`
- `Adapters/IDualDungeonDistanceControlPort.cs`
- `Adapters/IDualDungeonDistanceQuery.cs`
- `Adapters/IDungeonControlLinePort.cs`
- `Adapters/LegacyDungeonDerivedPropertiesAdapter.cs`
- `Queries/DungeonDerivedPropertiesQuery.cs`
- `Systems/DungeonSelectionControlSystem.cs`
- `../WorldGenerationC13FocusedVerifier/Program.cs`
- `../WorldGenerationC13FocusedVerifier/Terraria.WorldGenerationC13FocusedVerifier.csproj`

The verifier project path references were corrected from `WorldGeneration/Components` to the
existing root-level C08 dungeon files `DungeonLayoutControlComponent.cs`, `DungeonLayoutSnapshot.cs`,
and `DungeonRecordSnapshot.cs`.

Planned sequence:

1. Characterize the three properties and all active-record selection sites. Preserve the exact
   `CurrentDungeon` lower-bound-only clamp and record the complete writer set before changing any
   legacy setter.
2. Define `DungeonDerivedPropertiesQuery` over an immutable C08 dungeon-record snapshot. Keep
   `CurrentDungeonGenVars` as a projection/legacy adapter and do not create a writable derived
   component or duplicate the `dungeonGenVars` list.
3. Implement `DungeonSelectionControlSystem` behind an explicit selection port. It applies only
   the source lower clamp; it must preserve direct upper-index failure rather than sanitizing an
   invalid selection.
4. Implement `DungeonControlLineAdapter` and a separate distance query/control port. Forward
   `DualDungeon_NormalizedDistanceSafeFromDither` exactly, with no validation, cached copy, or
   second authority.
5. Adapt current-record and dual-dungeon callers through the query/adapter seam. Make the scheduler
   dependency explicit: record creation and selection precede any `CurrentDungeonGenVars` reader.
6. Verify snapshot eligibility and restore ordering: the two writable properties are eligible for
   the generic GenVars snapshot filter, the getter-only record property is not, and C08 records must
   exist before applying the selected index. The bounded C13 focused verifier has run; generic
   snapshot eligibility/restore ordering remains not verified.

Dependency impact: C13 consumes C08 dungeon-record snapshots and feeds dungeon, biome, conversion,
spawn, chest, and dual-dungeon generation reads. It does not own nested dungeon fields, tile/liquid
effects, WorldStorage, WorldSession, progression, or P18-P20 execution; unresolved ownership remains
`crossSubsystemOwner: integration-review`.

Rollback: retain the legacy properties and C08 record path, disable only the new C13 query/control
adapters, and remove only new C13 files if lower/upper index behavior, record identity, dither
forwarding, snapshot ordering, or pass scheduling diverges.

### C13 Implementation Checkpoint

The selection system preserves negative-to-zero lower clamping and leaves positive values unchanged.
The query resolves the selected copied C08 record directly, preserving an upper-index failure
instead of silently selecting record zero. The control-line adapter forwards reads and writes
without validation, clamping, or cached state.

Serial build command:
`pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src\WorldGenerationC13FocusedVerifier\Terraria.WorldGenerationC13FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`

Result: exit `0`, `0` warnings, `0` errors; artifact under
`Build/bin/Terraria.WorldGenerationC13FocusedVerifier/Debug/net10.0`.

Focused run command:
`pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\src\WorldGenerationC13FocusedVerifier\Terraria.WorldGenerationC13FocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`

Result: exit `0`; output `C13 dungeon derived-properties focused verifier passed.` Production
`DungeonControlLine` binding, complete caller migration, snapshot eligibility/restore ordering,
scheduler integration, persistence/network, and legacy behavior equivalence remain unverified.

## C14 WorldSavedOreTierState

Status: `component boundary implemented`; the seven-value component, repair/reset boundaries,
value-only save mapping, and outbound packet projection are present in root `src` and focused-
verified. Production persistence and protocol ownership remain pending.

Existing root-src files:

- `src/WorldSession/OreTierState.cs`
- `src/WorldSession/WorldSavedOreTierStateComponent.cs`
- `src/WorldSession/WorldSavedOreTierDefaults.cs`
- `src/WorldSession/WorldSavedOreTierRepairQuery.cs`
- `src/WorldSession/WorldSavedOreTierRepairSystem.cs`
- `src/WorldSession/WorldSavedOreTierResetSystem.cs`
- `src/WorldSession/WorldSavedOreTierCommitSystem.cs`
- `src/WorldSession/WorldFileSavedOreTierAdapter.cs`
- `src/WorldSession/NetMessageSavedOreTierAdapter.cs`

Planned sequence:

1. Add characterization fixtures for the seven report rows and capture the Version4 declaration
   defaults, world-clear `-1` reset, all direct readers/writers, and the separation from C01,
   C09, and C12/C20 state. Do not add a second owner to the existing three-value hardmode policy.
2. Define an immutable seven-value `WorldSavedOreTierState` and a single component owner. Give the
   component an explicit uninitialized/reset snapshot and retain the declaration defaults as named
   compatibility definitions; do not store tile arrays, RNG instances, altar counters, or mutable
   external references.
3. The root project now contains `ISavedOreTierCommitPort`, copied generation/altar commit payloads,
   `WorldSavedOreTierQuery`, and `WorldSavedOreTierCommitSystem`. The commit system accepts the
   first-four generation result or the three high-tier altar result, validates every value before
   replacement, and preserves the other fields in one seven-value replacement. It does not rerun
   RNG, mutate tiles, advance progression, or broadcast messages. These commit files were already
   present in the shared checkout when this implementation continuation began; this session did
   not overwrite them.
4. Define `WorldSavedOreTierRepairQuery` as a pure calculation over one loaded-world tile-count
   snapshot. Preserve the source predicate that repairs all four low tiers when any is `-1`, the
   strict `>` tie behavior, and the fact that the high three tiers are untouched. Add one atomic
   load/repair barrier before `waterLine` and liquid settling.
5. The root `WorldFileSavedOreTierAdapter` now maps raw optional fields by the evidenced version
   gates: direct Copper/Iron/Silver/Gold reads only for version 216+, direct Cobalt/Mythril/
   Adamantite reads only for version 54+, the v23-v53 `altarCount == 0` sentinel branch, and save
   order Cobalt/Mythril/Adamantite followed by Copper/Iron/Silver/Gold. Missing directly required
   fields are rejected before a value is returned. The adapter is pure value mapping; it does not
   own `BinaryReader`/`BinaryWriter`, header/version upgrades, load barriers, or rollback.
6. Implement the `NetMessageSavedOreTierAdapter` plan with the exact seven outbound signed-short
   positions `Copper, Iron, Silver, Gold, Cobalt, Mythril, Adamantite`. Keep packet encoding out of
   the component, characterize cast/range behavior, and do not claim inbound decode parity until
   the receive consumer and capability/version negotiation are traced.
7. Add read-only projections for `OrePatch`, chest generation, hardmode selection, save, and packet
   tests. Verify the scheduler order reset -> raw load -> low-tier repair -> liquid barrier ->
   generation/altar commit -> dependent reads -> save/network projection. File order must not be
   used as a runtime ordering mechanism.
8. Compare the legacy and proposed seven-value snapshots at reset, raw load, repair, generation
   commit, altar commit, save, and network barriers. Remove the legacy writer only after the focused
   tests pass and integration review assigns the WorldStorage, WorldSession, progression, and
   protocol owners.

Dependency impact: C14 consumes the C01 generation ore-selection handoff and the C12/C20 altar
selection result. It feeds `OrePatch`, chest generation, hardmode tile/progression queries, WorldFile
save/load, and the world-state packet. It does not own tile/liquid mutation, chest/item effects,
altar count, progression transitions, WorldStorage, WorldSession, or protocol transport; those
handoffs remain `crossSubsystemOwner: integration-review`.

Rollback: keep the legacy `WorldGen.SavedOreTiers`, WorldFile mapping, and outbound packet adapter
behind compatibility seams; disable only the proposed C14 owner and projections if reset, version
gates, four-tier repair, hardmode commit, save order, signed-short encoding, inbound consumption, or
load-barrier tests diverge. Remove only newly introduced C14 files after the affected readers are
returned to the legacy adapter.

### C14 Implementation Checkpoint

The independently evidenced C14 core is saved in the root WorldSession project. The component
stores one `OreTierState` snapshot and exposes only controlled internal replacement. The named
defaults preserve the seven Version4 declaration values without changing the source world-clear
sentinel. The immutable count value contains the four vanilla/alternate pairs required by the
historical repair calculation.

`WorldSavedOreTierRepairQuery` is pure: it returns the loaded state unchanged when all four low
tiers are present; when any low tier is `-1`, it calculates all four from the same count snapshot,
uses strict `vanillaCount > alternateCount` selection, uses alternate IDs on ties, and preserves
the three high-tier values. `WorldSavedOreTierRepairSystem` calculates before one replacement,
and `WorldSavedOreTierResetSystem` restores all seven values to `-1`. No file, packet, clock, RNG,
tile, liquid, progression, chest, chat, or network effect is performed.

The component is intentionally not wired to `WorldRulesState.SavedOreTiers`: the current root
project has no aggregate or production caller contract identifying the authoritative replacement
owner. The commit port/system is an isolated boundary, not proof of C01/C12/C20 scheduler
integration. The load/repair barrier, WorldFile adapter, inbound packet consumer, and rollback
behavior remain pending and are not represented by placeholder APIs.

Actual source changes in this checkpoint:

- `src/WorldSession/OreTierState.cs` was reused; no duplicate seven-value value type was added.
- `src/WorldSession/WorldSavedOreTierStateComponent.cs`
- `src/WorldSession/WorldSavedOreTierDefaults.cs`
- `src/WorldSession/WorldSavedOreTierTileCounts.cs`
- `src/WorldSession/WorldSavedOreTierRepairQuery.cs`
- `src/WorldSession/WorldSavedOreTierRepairSystem.cs`
- `src/WorldSession/WorldSavedOreTierResetSystem.cs`
- `src/WorldSession/WorldSavedOreTierQuery.cs`
- `src/WorldSession/ISavedOreTierCommitPort.cs`
- `src/WorldSession/WorldSavedOreTierGenerationCommit.cs`
- `src/WorldSession/WorldSavedOreTierAltarCommit.cs`
- `src/WorldSession/WorldSavedOreTierCommitSystem.cs` (present before this continuation; reviewed)
- `src/WorldSession/IWorldFileSavedOreTierAdapter.cs`
- `src/WorldSession/WorldFileSavedOreTierAdapter.cs` (value mapping present in the shared checkout; reviewed)
- `src/WorldSession/WorldSavedOreTierFileInput.cs`
- `src/WorldSession/WorldSavedOreTierFileSaveValues.cs`
- `src/WorldSessionFocusedVerifier/Program.cs` (C14 assertions added in this continuation)

The C14 outbound network projection checkpoint additionally saved:

- `src/WorldSession/WorldSavedOreTierNetworkFields.cs`
- `src/WorldSession/ISavedOreTierNetworkProjection.cs`
- `src/WorldSession/NetMessageSavedOreTierAdapter.cs`

`NetMessageSavedOreTierAdapter.Encode` returns the seven fields in the sourced
Copper/Iron/Silver/Gold/Cobalt/Mythril/Adamantite order and applies the Version4 unchecked signed
`short` casts. It performs no transport I/O, inbound decode, capability negotiation, component
replacement, or retry behavior.

The C14 serial build and focused verification were rerun after these adapter boundaries were
present. The build exited `0` with `0` warnings and `0` errors. The no-build focused run exited
`0` and its output included `WorldFileSavedOreTierAdapter C14 focused verifier passed.` and
`NetMessageSavedOreTierAdapter C14 focused verifier passed.` The assertions cover v23/v54/v216
version gates, the legacy `altarCount` sentinel, high-tier-first save ordering, rejection of a
missing direct field, outbound packet order, and the sourced signed-short conversion. Production
BinaryReader/BinaryWriter binding, aggregate wiring, liquid load barrier, inbound packet
consumption, capability negotiation, rollback, and legacy behavior-equivalence verification remain
blocked by missing current-project contracts.
Production BinaryReader/BinaryWriter binding, aggregate wiring, liquid load barrier, inbound packet
consumption, capability negotiation, rollback, and legacy behavior-equivalence verification remain
blocked by missing current-project contracts; these are integration boundaries, not unimplemented
P17 component members.

## 6. Remaining Checkpoints

| checkpoint | boundary | planned future work | status |
|---|---|---|---|
| C02 | `GenVarsWorldLayerMetrics` | preserve the bounded layer/snow component and focused calculator boundary; integration and legacy parity remain external | implemented; focused-verified |
| C03 | `GenVarsSurfaceAndBiomeState` | preserve state/material-definition/liquid boundaries; external generation and liquid handoff remain integration-only | implemented; focused-verified |
| C04 | `GenVarsBeachAndOceanBoundaryState` | preserve beach/shell state and explicit-input calculator; RNG ownership and external effects remain integration-only | implemented; focused-verified |
| C05 | `WorldGenBeachAndOceanBiomeState` | preserve ocean constraint, treasure, and pass-control components; candidate search and placement effects remain integration-only | implemented; focused-verified |
| C06 | `WorldGenUndergroundDesertStructureState` | preserve layout/larva components and pure projections; normalization, retry, reservation mapping, and effects remain integration-only | implemented; focused-verified |
| C07 | `WorldGenJungleStructureState` | preserve region, pyramid, chest/loot components and pure boundaries; material/RNG/placement effects remain integration-only | implemented; focused-verified |
| C08 | `GenVarsDungeonAndIslands` | preserve dungeon, reward, and floating-island component boundaries; external dungeon-record/island execution remains integration-only | implemented; focused-verified |
| C09 | `GenVarsCaveTunnelAndOrePatchState` | preserve three bounded history components and append gates; scan/tile/ore handoff remains integration-only | implemented; focused-verified |
| C10 | `GenVarsMushroomBiomeAndLogState` | preserve mushroom-anchor and fallen-log handoff components; tile/liquid/RNG effects remain integration-only | implemented; focused-verified |
| C11 | `GenVarsLakeAndOasisState` | preserve lake/oasis histories and commit-result gates; terrain/liquid/vegetation effects remain integration-only | implemented; focused-verified |
| C12 | `GenVarsHellAndSpecialStructures` | preserve hell chest, statue, infection, shimmer, and seed-rule boundaries; external effects remain integration-only | implemented; focused-verified |
| C13 | `GenVarsDungeonDerivedProperties` | preserve pure derived queries and explicit forwarding; production backing and restore ordering remain integration-only | implemented; focused-verified |
| C14 | `WorldSavedOreTierState` | preserve seven-value component, repair/reset, value mappings, and outbound projection; production save/network ownership remains integration-only | implemented; focused-verified |

## 7. Planned Verification Commands

These are command plans only and were not executed in this session. Compile-capable commands must
run serially from the repository root through the repository wrapper, after inspecting active
`dotnet.exe` and `csc.exe` processes:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  build .\dome\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj `
  -m:1 -nr:false -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false

pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\dome\Test\Terraria.Dome.WorldGeneration.Verification\Terraria.Dome.WorldGeneration.Verification.csproj `
  --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

Additional planned verifiers may use the existing persistence, world protocol, liquid, world
import, and loopback projects once the affected boundary is known. Each executed build record must
include command, project, exit code, warning/error counts, and artifact path under `Build/bin/`.
The root `WorldSession` project and its focused verifier are authorized for this implementation
continuation. Commands must run serially through the repository wrapper; the verification record is
updated only after fresh command output is inspected.

Executed verification record:

- Build command: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src\WorldSessionFocusedVerifier\Terraria.WorldSessionFocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; project: `src/WorldSessionFocusedVerifier/Terraria.WorldSessionFocusedVerifier.csproj`; exit code: `0`; warning/error count: `0/0`; artifacts: `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` and `Build/bin/Terraria.WorldSessionFocusedVerifier/Debug/net10.0/Terraria.WorldSessionFocusedVerifier.dll`.
- Focused verifier command: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\src\WorldSessionFocusedVerifier\Terraria.WorldSessionFocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit code: `0`; output: `WorldTileMergeCullStateQuery focused verifier passed.`, `WorldSavedOreTier C14 focused verifier passed.`, `WorldFileSavedOreTierAdapter C14 focused verifier passed.`, `NetMessageSavedOreTierAdapter C14 focused verifier passed.`, `WorldGenJungleStructureState C07 focused verifier passed.`
- An earlier focused run exited `1` at the verifier's invalid-pyramid-capacity owner-state assertion because the test compared list-bearing pyramid record structs by reference. The verifier was corrected to compare C07 pyramid snapshots by value; the production component was also tightened to reject non-exact used-range lists before writing. The final run passed.
- C09 isolated build command: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src\WorldGenerationC09FocusedVerifier\Terraria.WorldGenerationC09FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; project: `src/WorldGenerationC09FocusedVerifier/Terraria.WorldGenerationC09FocusedVerifier.csproj`; exit code: `0`; warning/error count: `0/0`; artifact: `Build/bin/Terraria.WorldGenerationC09FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationC09FocusedVerifier.dll`.
- C09 focused verifier command: `pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\src\WorldGenerationC09FocusedVerifier\Terraria.WorldGenerationC09FocusedVerifier.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`; exit code: `0`; output: `C09 history focused verifier passed.`
- A full `src\WorldSession\Terraria.WorldSession.csproj` build was attempted through the serial wrapper and exited `1` with `0` warnings and `7` errors, all from duplicate C10 `MushroomBiomeAnchorStateComponent`/snapshot declarations already present in the shared checkout. It is recorded as an unrelated workspace blocker, not as C09 evidence.

## 8. Acceptance Gates Before Implementation

- authoritative report rows and source declarations remain unchanged;
- every P17 member has one documented candidate and lifecycle;
- each proposed state has one owner writer and explicit reset/restore behavior;
- all external effects use ports/adapters/projections;
- structure reservations are not confused with tile commits;
- C14 saved tiers have an evidenced save-version and packet contract;
- C13 properties have no hidden writable query state;
- focused tests exist before each legacy writer is removed;
- all compile/test records use the serial repository wrapper and artifact paths.

## 9. Checkpoint Log

| checkpoint | saved with both documents | executionStatus | implementationStatus | verificationStatus |
|---|---|---|---|---|
| C01 | 2026-09-11T17:52:29.5164917Z | planned | implemented | focused-c01-passed |
| C02 | 2026-09-12T01:14:24.3994976Z | in-progress | implemented | state-focused-passed; c02-calculator-owner-current-isolated-build-passed; c02-calculator-owner-current-isolated-run-passed; full-project integration and legacy parity pending |
| C03 | 2026-09-11T22:08:01.6117673Z | in-progress | implemented | c03-spawn-material-liquid-focused-build-passed; c03-spawn-material-liquid-focused-run-passed; generation-calculation-and-external-handoff-pending |
| C04 | 2026-09-11T22:15:22.3957768Z | in-progress | implemented | c04-boundary-focused-build-passed; c04-boundary-focused-run-passed; calculator/RNG/dimension/configuration and external handoff pending |
| C05 | 2026-09-11T22:31:07.2539706Z | in-progress | implemented | c05-state-query-focused-build-passed; c05-state-query-focused-run-passed; calculator/RNG/phase/placement and legacy parity pending |
| C06 | 2026-09-12T04:06:02.3891860Z | in-progress | implemented | c06-state-query-commit-focused-build-passed; c06-state-query-commit-focused-run-passed; c06-larva-solidity-projection-focused-build-passed; c06-larva-solidity-projection-focused-run-passed; rectangle/retry/reservation/effect integration pending |
| C07 | 2026-09-12T00:15:21.7866592Z | in-progress | in-progress | focused-region-pyramid-chest-passed; c07-jungle-hut-material-focused-build-passed; c07-jungle-hut-material-focused-run-passed; c07-jungle-hut-selection-focused-build-passed; c07-jungle-hut-selection-focused-run-passed; c07-jungle-region-focused-build-passed; c07-jungle-region-focused-run-passed; c07-jungle-loot-focused-build-passed; c07-jungle-loot-focused-run-passed; c07-jungle-bounds-calculation-focused-build-passed; c07-jungle-bounds-calculation-focused-run-passed; external effects and legacy parity pending |
| C08 | 2026-09-12T07:53:23.2889859Z | in-progress | completed | c08-focused-build-passed; c08-focused-run-passed; external dungeon-record/island execution and legacy parity pending |
| C09 | 2026-09-12T07:53:23.2889859Z | in-progress | completed | c09-history-focused-build-passed; c09-history-focused-run-passed; current WorldSession focused build passed with one pre-existing nullable warning; scan/tile/ore handoff and legacy parity pending |
| C10 | 2026-09-11T21:11:25.3972400Z | planned | implemented | c10-mushroom-log-focused-build-passed; c10-mushroom-log-focused-run-passed |
| C11 | 2026-09-12T01:52:59.9561478Z | in-progress | implemented | c11-lake-oasis-focused-build-passed-before-result-boundary-migration; c11-lake-oasis-focused-run-passed-before-result-boundary-migration; c11-append-result-system-boundary-migration-focused-build-passed; c11-append-result-system-boundary-migration-focused-run-passed; terrain/liquid/RNG, reset/restore, vegetation, and legacy parity pending |
| C12 | 2026-09-12T07:53:23.2889859Z | in-progress | completed | c12-hell-special-focused-build-passed; c12-hell-special-focused-run-passed; c12-component-api-focused-build-passed; c12-component-api-focused-run-passed; external-effects-and-snapshot-integration-pending |
| C13 | 2026-09-11T21:48:44.2291962Z | in-progress | implemented | c13-dungeon-derived-properties-focused-build-passed; c13-dungeon-derived-properties-focused-run-passed; generic-snapshot-restore-ordering-pending |
| C14 | 2026-09-12T07:53:23.2889859Z | in-progress | completed | c14-worldfile-adapter-focused-passed; c14-netmessage-adapter-focused-passed; current WorldSession focused build/run passed; production save/network/load ownership and legacy parity pending |
