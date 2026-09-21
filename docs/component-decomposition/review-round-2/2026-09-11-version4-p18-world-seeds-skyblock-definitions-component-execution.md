# P18 World Seeds, Skyblock and Definition Component Execution Plan

## Execution Metadata

~~~yaml
documentType: component-execution
partitionId: P18
sessionId: 2e09fab3b2e84e1a965d4c1f3a24c32d
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P18-World-Seeds-Skyblock-Definitions.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-p18-world-seeds-skyblock-definitions-component-design.md
executionStatus: in-progress
implementationStatus: in-progress
verificationStatus: partially-verified
completedComponents:
  - WorldSecretSeedRegistryDefinitionsProjection
  - WorldSecretSeedVisualAndSurfaceRulesDefinition
  - WorldSecretSeedTerrainAndStructureRulesDefinition
  - WorldSecretSeedProgressionAndInfectionRulesDefinition
  - WorldSecretSeedSeasonalRulesDefinition
  - WorldSecretSeedRuntimeRegistryComponent
  - WorldSecretSeedDerivedOptionsQuery
  - WorldSecretSeedDerivedVariationsQuery
  - WorldSkyblockGenerationRulesDefinition
  - WorldSeedOptionCatalogDefinition
  - WorldLandmassAndTreeProfilesDefinition
implementedDataShapes:
  - WorldGenerationSecretSeedFlagsComponent (data shape only; canonical integration pending)
currentComponent: none
pendingComponents:
  - WorldGenerationSecretSeedFlagsComponent (blocked: canonical owner/integration unresolved)
lastCheckpointUtc: 2026-09-12T07:16:31Z
evidence-gap:
  - Exact NLTX canonical owner for existing WorldRuleSnapshotComponent flags is unresolved.
  - Version4 persistence/network and cross-partition scheduler contracts are not verified.
  - 约束/公共拆分约束.md is referenced by the style guide but absent from this checkout.
  - GenerateBiggerAbandonedHouses has an explicit Version4 genRand.Next(3) dependency; draw timing is not yet an NLTX contract.
  - The current NLTX variation API accepts enabled variants and active count separately; their consistency is not enforced by the API.
  - Version4 Skyblock.ScanTiles/Calculate owns scan reset, active-tile accumulation, dungeon-coordinate mutation and lowTiles replication effects; the cross-partition effect owner is not verified.
  - Current NLTX SkyblockRuleQuery and SkyblockPolicyQuery remain legacy compatibility paths; the new scan owner is implemented but exact Version4 behavior equivalence is not verified. P18 now freezes caller-supplied scan presence and dungeon-classification sets at constructor boundaries, and the focused verifier covers selected snapshot/gating/policy cases.
  - No complete NLTX option catalog mapping was found for the Version4 registration order, key/config metadata, special seed matching and Everything dependency list.
  - Option selection, event callbacks, UI construction and server configuration mutation are external effects whose persistence and compatibility owner remain unresolved.
  - Version4 tree predicate delegates and LandmassData are definition-level declarations, but their complete NLTX predicate adapters, profile enumeration consumers and persistence contract are not verified.
  - Current NLTX tree numeric values are present, but profile declaration order differs from the Version4 declaration order and requires call-site evidence before replacement.
  - `WorldSkyblockGenerationRulesQuery` now explicitly gates all eight no-content/low-tile outputs to `false` for non-Skyblock worlds; the focused behavior matrix for this branch and the remaining Skyblock truth table is still not-run.
  - The affected `Terraria.WorldSession` serial build is blocked by external-session source: the latest run reports one `HellChestLootCycleSystem.cs(16,22)` overload error; an earlier run reported five `WorldGenerationTileFramingAndDebugActionsCommand.cs` `Terraria.Content.ColorRgba` errors. No P18 source error was reported.
  - The isolated P18 compile probe and the P18 focused verifier passed with exit code 0; complete behavior matrices and full-project integration remain unverified.
blocking-decision:
  - Implementation is blocked until the world-scoped owner, compatibility window and persistence/network policy are approved.
  - No dual write between legacy fields and proposed components is allowed.
  - Do not place genRand or Main access inside a pure Query; approve the random adapter and world snapshot inputs first.
  - Variation evaluation must consume one immutable runtime snapshot; callers may not provide an independently writable count.
  - `_dependencies` is a derived immutable catalog value, not a second mutable authority; the catalog must not expose `AWorldGenerationOption` instances or a mutable dependency list.
  - Seed parsing and catalog lookup stay pure; option events, UI, AutoGenEnabled mutation and selected-flag commits require explicit adapters and commit systems.
  - Landmass `Top` mutation remains value-local and must preserve Position/RadiusOrHalfSize round trips; authoritative component replacement requires an explicit commit port.
  - GroundTest and WallTest are explicit predicate seams/adapters; no profile definition may read tiles, walls, liquid or global WorldGen state implicitly.
  - Scan arrays and active-tile accumulation have one transient scan owner; no pure Query may reset them or write dungeon coordinates, network state or tiles.
  - Skyblock no-content and low-tile values are derived from one committed scan snapshot; compatibility fields are projections and may not become independent mutable flags.
~~~

This file is an execution record. It must be updated after every source component checkpoint and
must not claim build, test or behavior equivalence without evidence. The initial plan was
read-only; the implementation checkpoints below record the actual source changes made under
`src/WorldSession/WorldGeneration`.

## 1. Scope and Preconditions

The plan covers only the 136 members listed in P18. It is executed as small, reversible changes.
The input report, Version4 source, full-reference source and current NLTX source are read-only
evidence for this session. The implementation checkpoints below record the authorized P18 source
changes; unrelated project files, tests and other partitions remain outside this task.

Before wiring the remaining deferred boundary, obtain decisions for:

1. the world entity or aggregate that owns generation state;
2. the canonical owner of the existing WorldRuleSnapshotComponent flags;
3. persistence and network representation for selected flags and enabled secret seeds;
4. whether legacy Main and WorldGen compatibility fields remain during a bounded migration window;
5. the scheduler barrier API used by P19 and P20 generation execution.

## 2. Proposed File Organization

The following paths are proposals, not existing files. They follow the domain-first rule and keep
small responsibilities flat under the existing WorldGeneration domain. Definitions is already an
established stable boundary in the current Dome tree; Components and Systems are used only where a
persistent state owner or effectful writer exists.

| Proposed type | Proposed path | Namespace | Reason |
| --- | --- | --- | --- |
| WorldGenerationSecretSeedFlagsComponent | src/WorldSession/WorldGeneration/Components/WorldGenerationSecretSeedFlagsComponent.cs | Terraria.WorldGeneration.Components | World-scoped selected-flag data shape; canonical integration remains blocked. |
| WorldSecretSeedRegistryDefinitionsProjection | dome/src/Terraria.Dome.Simulation/WorldGeneration/Definitions/WorldSecretSeedRegistryDefinitionsProjection.cs | Terraria.Dome.Simulation.WorldGeneration | One-way immutable registry view. |
| WorldSecretSeedInputAdapter | dome/src/Terraria.Dome.Simulation/WorldGeneration/WorldSecretSeedInputAdapter.cs | Terraria.Dome.Simulation.WorldGeneration | External seed text/config normalization at the boundary. |
| WorldSecretSeedVisualAndSurfaceRulesDefinition | dome/src/Terraria.Dome.Simulation/WorldGeneration/Definitions/WorldSecretSeedVisualAndSurfaceRulesDefinition.cs | Terraria.Dome.Simulation.WorldGeneration | Visual/surface definitions adjacent to world-generation definitions. |
| WorldSecretSeedTerrainAndStructureRulesDefinition | dome/src/Terraria.Dome.Simulation/WorldGeneration/Definitions/WorldSecretSeedTerrainAndStructureRulesDefinition.cs | Terraria.Dome.Simulation.WorldGeneration | Terrain/structure definition boundary. |
| WorldSecretSeedProgressionAndInfectionRulesDefinition | dome/src/Terraria.Dome.Simulation/WorldGeneration/Definitions/WorldSecretSeedProgressionAndInfectionRulesDefinition.cs | Terraria.Dome.Simulation.WorldGeneration | Progression/infection definitions with integration seam. |
| WorldSecretSeedSeasonalRulesDefinition | dome/src/Terraria.Dome.Simulation/WorldGeneration/Definitions/WorldSecretSeedSeasonalRulesDefinition.cs | Terraria.Dome.Simulation.WorldGeneration | Seasonal definitions. |
| WorldSecretSeedRuntimeRegistryComponent | dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/WorldSecretSeedRuntimeRegistryComponent.cs | Terraria.Dome.Simulation.WorldGeneration | World-scoped enabled set and count. |
| WorldSecretSeedDerivedOptionsQuery | dome/src/Terraria.Dome.Simulation/WorldGeneration/WorldSecretSeedDerivedOptionsQuery.cs | Terraria.Dome.Simulation.WorldGeneration | Pure calculation over explicit random/context inputs. |
| WorldSecretSeedDerivedVariationsQuery | dome/src/Terraria.Dome.Simulation/WorldGeneration/WorldSecretSeedDerivedVariationsQuery.cs | Terraria.Dome.Simulation.WorldGeneration | Pure 22-property projection. |
| WorldSkyblockGenerationScanComponent | dome/src/Terraria.Dome.Simulation/WorldGeneration/Components/WorldSkyblockGenerationScanComponent.cs | Terraria.Dome.Simulation.WorldGeneration | Mutable scan accumulation and reset ownership. |
| WorldSkyblockGenerationRulesQuery | dome/src/Terraria.Dome.Simulation/WorldGeneration/WorldSkyblockGenerationRulesQuery.cs | Terraria.Dome.Simulation.WorldGeneration | Pure result from a committed scan and world context. |
| WorldSeedOptionCatalogDefinition | dome/src/Terraria.Dome.Simulation/WorldGeneration/Definitions/WorldSeedOptionCatalogDefinition.cs | Terraria.Dome.Simulation.WorldGeneration | Immutable option metadata and dependency values. |
| WorldSeedOptionCatalogQuery | dome/src/Terraria.Dome.Simulation/WorldGeneration/WorldSeedOptionCatalogQuery.cs | Terraria.Dome.Simulation.WorldGeneration | Seed text/config selection without event/UI effects. |
| WorldLandmassDefinition | dome/src/Terraria.Dome.Simulation/WorldGeneration/Definitions/WorldLandmassDefinition.cs | Terraria.Dome.Simulation.WorldGeneration | Landmass value data and explicit top calculation. |
| WorldTreeProfileCatalog | dome/src/Terraria.Dome.Simulation/WorldGeneration/Definitions/WorldTreeProfileCatalog.cs | Terraria.Dome.Simulation.WorldGeneration | Ten tree profile definitions and lookup. |

No Shared/Components, Common, Misc, Utility or generic Manager directory is proposed. Each public
type gets a same-named PascalCase file. Existing namespaces and public APIs remain unchanged
during the compatibility phase unless an approved API change says otherwise.

## 3. Implementation Sequence

Each item is a separate rollback unit. Steps 2-12 have implementation checkpoints below. Step 1's
data shape is present, but its canonical integration remains deferred until its owner and
compatibility contract are identified.

| Step | Component/boundary | Planned change | Rollback condition |
| ---: | --- | --- | --- |
| 1 | WorldGenerationSecretSeedFlagsComponent | Add the world-scoped value shape and a candidate/commit seam; map ten legacy fields exactly once. | Existing request or snapshot behavior changes without an equivalence fixture. |
| 2 | WorldSecretSeedRegistryDefinitionsProjection | Extract immutable metadata from static registration and isolate input normalization, sound and unlock-text effects in an adapter. | Registry order, normalization or unlock behavior differs. |
| 3 | WorldSecretSeedVisualAndSurfaceRulesDefinition | Move nine rule references into immutable definitions consumed by a read-only query. | A query mutates enabled state or a visual/surface consumer loses a rule. |
| 4 | WorldSecretSeedTerrainAndStructureRulesDefinition | Move eleven terrain/structure references behind a read-only catalog seam. | Generation pass guard count/order or rule value changes. |
| 5 | WorldSecretSeedProgressionAndInfectionRulesDefinition | Move twelve progression/infection references and hand off shared flags for integration review. | Hardmode/infection owner becomes ambiguous or duplicate. |
| 6 | WorldSecretSeedSeasonalRulesDefinition | Move three seasonal references to immutable definitions; keep seasonal effects in their owner systems. | Event/seasonal state is incorrectly treated as definition data. |
| 7 | WorldSecretSeedRuntimeRegistryComponent | Replace static _enabled/count authority with a world-scoped set/count and explicit commit system. | Enabled count, clear, re-enable or world isolation fails. |
| 8 | WorldSecretSeedDerivedOptionsQuery | Convert two derived options to explicit inputs, including a supplied random decision for the abandoned-house branch. | Random stream consumption or anniversary fallback differs. |
| 9 | WorldSecretSeedDerivedVariationsQuery | Preserve all 22 formulas as deterministic calculation over snapshot inputs. | Boundary counts or formula truth table differs. |
| 10 | WorldSkyblockGenerationRulesDefinition | Separate mutable tile/wall scan accumulation from pure no-content/low-tile results. | Scan reset, ratio threshold, dungeon/temple wall rule or world mutation differs. |
| 11 | WorldSeedOptionCatalogDefinition | Replace inheritance-facing catalog reads with immutable definitions and a selection adapter. | Special seed parsing, config names, dependency expansion or event timing differs. |
| 12 | WorldLandmassAndTreeProfilesDefinition | Preserve landmass top/position relation and ten profile values behind definitions and lookup queries. | Tree profile lookup or Top setter semantics differs. |

### 3.1 Checkpoint 2: WorldSecretSeedRegistryDefinitionsProjection

status: implemented-unverified.

Implementation unit:

1. Add an immutable WorldSecretSeedDefinition value and a read-only
   WorldSecretSeedRegistryDefinitionsProjection in the existing
   WorldGeneration/Definitions domain boundary.
2. Preserve the Version4 35-definition order and source anchors. Do not expose mutable lists or
   the legacy LegacySoundStyle type from the core definition.
3. Add WorldSecretSeedInputAdapter as the only boundary for seed-text normalization, code
   matching, original unlock text and optional sound playback.
4. Return an explicit match/rejection result. A successful result may be converted into a runtime
   enable command, but this checkpoint does not implement the runtime registry.
5. Keep the existing SecretSeedDefinitionRegistry as the compatibility source until equivalence
   checks prove the new projection has the same order and variants. Do not enable dual writers.

Source-to-target mapping for this checkpoint:

| Source member | Target | Dependency/effect note |
| --- | --- | --- |
| AllSecretSeeds | read-only definitions projection | Static registration becomes catalog construction; order is an explicit contract. |
| Localization, _code | immutable definition values | _code remains opaque and is consumed only by the input adapter. |
| _sound | sound-resolution port in input adapter | Keeps third-party audio type outside core definitions. |
| _plaintext, TextThatWasUsedToUnlock | successful input-match result | No catalog or world-state write; persistence requires integration review. |

Focused verifier cases:

- 35 definitions, exact order and unique variant keys;
- plaintext, transformed-code and normalized-input matches;
- invalid/empty input has no side effects;
- original unlock text is returned without mutating the catalog;
- sound port call count is zero for queries and explicit for an enable request;
- definition collection is read-only and runtime enable state cannot alter it.

Implementation checkpoint:

- Source files saved: the eight files listed in the design checkpoint above.
- Core behavior: immutable 35-entry definition projection with exact Version4 order and explicit
  input normalization/matching result; caller-provided definition lists are defensively copied before
  exposure; optional sound is isolated behind a port.
- Dependency impact: no runtime registry, legacy static field, persistence writer, network writer
  or existing call site was changed.
- Evidence-gap: injected code transformation is required because the root project has no verified
  `Terraria.Utilities.Secrets` implementation; runtime enable/persistence semantics remain open.
- Blocking-decision: the result cannot enable a seed until the runtime registry owner and commit
  barrier are implemented.
- VerificationStatus: not-run.

### 3.2 Checkpoint 3: WorldSecretSeedVisualAndSurfaceRulesDefinition

status: implemented-unverified.

Implementation unit:

1. Add a definition that contains nine stable references to the immutable registry catalog:
   PaintEverythingGray, PaintEverythingNegative, CoatEverythingEcho,
   CoatEverythingIlluminant, NoSurface, SurfaceIsInSpace, RainsForAYear, RainbowStuff and
   WorldIsFrozen.
2. Do not duplicate an Enabled boolean in this definition. Runtime enablement is read from the
   world-scoped runtime registry snapshot.
3. Keep visual, coating and surface calculations in read-only queries. Move no tile writes,
   weather effects or progression state into this definition.
4. Preserve the current NLTX explicit-input variation query as the compatibility calculation until
   all nine references are covered.
5. Mark WorldIsFrozen, weather and surface consumers as integration-review where their authority
   belongs to another partition.

Focused verifier cases:

- all nine references resolve uniquely and remain stable;
- enabled-set snapshots change derived results without changing definitions;
- variation truth-table outputs are unchanged for conflicting paint/coating flags;
- the definition/query path performs no writes, random reads or external I/O;
- P16/P17 consumers receive read-only results and cannot write the registry.

Implementation checkpoint:

- Source files saved: `Definitions/WorldSecretSeedVisualAndSurfaceRulesDefinition.cs`,
  `Definitions/WorldSecretSeedVisualAndSurfaceRulesSelection.cs`, and
  `Queries/WorldSecretSeedVisualAndSurfaceRulesQuery.cs`.
- Core behavior implemented: nine immutable rule references and a pure enabled-membership query.
- Dependency impact: definitions consume only the C02 catalog; no existing call site or writer was
  changed.
- Evidence-gap: weather/surface/frozen-world effects remain outside this boundary and are not
  connected to P16/P17.
- Blocking-decision: no runtime writer or compatibility projection is added here.
- VerificationStatus: not-run.

### 3.3 Checkpoint 4: WorldSecretSeedTerrainAndStructureRulesDefinition

status: implemented-unverified.

Implementation unit:

1. Add eleven immutable rule references for ExtraLivingTrees, ExtraFloatingIslands,
   BiggerAbandonedHouses, AddTeleporters, NoSpiderCaves, ActuallyNoTraps, DigExtraHoles,
   RoundLandmasses, ExtraLiquid, PortalGunInChests and DualDungeons.
2. Keep rule references separate from Skyblock scan state, liquid state, chest inventory, dungeon
   placement and tree/landmass definition values.
3. Expose read-only eligibility queries or explicit command intents. Do not let a Query write a
   tile, liquid, chest, dungeon or teleporter store.
4. Route ExtraFloatingIslands through the same runtime snapshot consumed by Skyblock and variation
   queries. Do not add a second flag.
5. Require integration review with P01, P09, P19 and P20 before implementing any consumer adapter.

Focused verifier cases:

- eleven references resolve uniquely;
- each rule changes only its intended decision;
- no hidden tile/liquid/chest/dungeon writes occur;
- cross-partition command ordering, retry and duplicate semantics are explicit;
- all consumers read the same enabled-set snapshot.

Implementation checkpoint:

- Source files saved: `Definitions/WorldSecretSeedTerrainAndStructureRulesDefinition.cs`,
  `Definitions/WorldSecretSeedTerrainAndStructureRulesSelection.cs`, and
  `Queries/WorldSecretSeedTerrainAndStructureRulesQuery.cs`.
- Core behavior implemented: eleven immutable rule references and a pure enabled-membership query.
- Dependency impact: no generation-side effect or existing call site was changed.
- Evidence-gap: P01/P09/P19/P20 execution ownership, command ordering and retry semantics remain
  integration-review.
- Blocking-decision: all consumers must read the same later runtime snapshot; no duplicate
  `ExtraFloatingIslands` authority is permitted.
- VerificationStatus: not-run.

### 3.4 Checkpoint 5: WorldSecretSeedProgressionAndInfectionRulesDefinition

status: implemented-unverified.

Implementation unit:

1. Add immutable references for ErrorWorld, GraveyardBloodmoonStart, RandomSpawn, StartInHardmode,
   NoInfection, HallowOnTheSurface, WorldIsInfected, SurfaceIsMushrooms, SurfaceIsDesert,
   PooEverywhere, Vampirism and TeamBasedSpawns.
2. Keep hardmode, infection, biome, spawn, player and network facts in their existing or approved
   owner components. The definition only reports selected seed rules.
3. Expose initialization intents or query results; do not write progression or player state from a
   definition/query.
4. Reuse the existing explicit LegacyStartInHardmodePolicy shape as a compatibility seam, subject
   to owner review.
5. Require integration review with P06, P07, P12, P14, P16, P17, P19 and P20 before implementing
   consumer adapters.

Focused verifier cases:

- twelve unique references and stable catalog order;
- StartInHardmode has one commit owner;
- infection/surface combinations are deterministic and reject unsupported combinations;
- spawn, Vampirism and TeamBasedSpawns outputs cross explicit read/replication seams;
- no query or definition writes progression, world, player or network state.

Implementation checkpoint:

- Source files saved: `Definitions/WorldSecretSeedProgressionAndInfectionRulesDefinition.cs`,
  `Definitions/WorldSecretSeedProgressionAndInfectionRulesSelection.cs`, and
  `Queries/WorldSecretSeedProgressionAndInfectionRulesQuery.cs`.
- Core behavior implemented: twelve immutable rule references and a pure enabled-membership query.
- Dependency impact: no existing world-rule writer, progression component or network projection was
  changed.
- Evidence-gap: cross-partition owners and replication semantics remain integration-review.
- Blocking-decision: selected seed rules return read-only results/intents; they do not become
  hardmode, infection, biome, spawn, player or network authority.
- VerificationStatus: not-run.

### 3.5 Checkpoint 6: WorldSecretSeedSeasonalRulesDefinition

status: implemented-unverified.

Implementation unit:

1. Add three immutable references for HalloweenGeneration, EndlessHalloween and EndlessChristmas.
2. Read enabled state only from the runtime registry snapshot.
3. Keep calendar/time, event activation, weather, audio, UI, persistence and replication in an
   explicit seasonal owner or adapter.
4. Pass calendar context into a pure seasonal query; do not read system time inside the query.
5. Require integration review before connecting the query to world-calendar or event systems.

Focused verifier cases:

- three unique references and stable order;
- no selected seasonal rule produces no event side effect;
- simultaneous rules return a deterministic result;
- calendar context is explicit and no query reads system time;
- seasonal owner receives a one-way result and owns persistence/replication.

Implementation checkpoint:

- Source files saved: `Definitions/WorldSecretSeedSeasonalRulesDefinition.cs`,
  `Definitions/WorldSecretSeedSeasonalRulesSelection.cs`, and
  `Queries/WorldSecretSeedSeasonalRulesQuery.cs`.
- Core behavior implemented: three immutable seasonal rule references and a pure enabled-membership
  query with no system-time read.
- Dependency impact: no calendar/event owner, runtime registry, persistence or replication writer
  was changed.
- Evidence-gap: seasonal calendar context, event ordering and persistence/replication ownership
  remain unresolved.
- Blocking-decision: only the future seasonal owner may turn this result into an event effect.
- VerificationStatus: not-run.

### 3.6 Checkpoint 7: WorldSecretSeedRuntimeRegistryComponent

status: implemented-unverified.

Implementation unit:

1. Add a world-scoped component whose only authoritative mutable value is an immutable/read-only
   enabled variant set; derive ActiveSecretSeedCount from its cardinality.
2. Add one commit system for enable, disable and clear commands. Include world/session version and
   explicit idempotency handling for retried commands.
3. Replace per-definition _enabled and static count authority with a read-only compatibility
   projection. Do not maintain a second counter or mutable SecretSeed authority.
4. Keep sound, logging, persistence and network publication in explicit post-commit adapters.
5. Reset the component at world-generation lifecycle boundaries; prove that two world sessions
   cannot share state.

Focused verifier cases:

- all enable/disable/clear transitions and count invariants;
- unknown variant, stale version and invalid lifecycle rejection without partial writes;
- world isolation and retry/idempotency behavior;
- compatibility projection is one-way;
- serialized state uses stable variant keys and runtime version.

Implementation checkpoint:

- Source files saved: `Components/WorldSecretSeedRuntimeRegistryComponent.cs`,
  `Components/WorldSecretSeedRuntimeRegistryLifecycle.cs`,
  `Components/WorldSecretSeedRuntimeRegistrySnapshot.cs`,
  `Actions/EnableSecretSeedCommand.cs`, `Actions/DisableSecretSeedCommand.cs`,
  `Actions/ClearSecretSeedsCommand.cs`,
  `Systems/WorldSecretSeedRuntimeRegistryCommitStatus.cs`,
  `Systems/WorldSecretSeedRuntimeRegistryCommitResult.cs`,
  `Systems/WorldSecretSeedRuntimeRegistryCommitSystem.cs`,
  `Queries/WorldSecretSeedRuntimeRegistryQuery.cs`,
  `Adapters/WorldSecretSeedRuntimeProjection.cs`,
  `Adapters/WorldSecretSeedRuntimeRegistrySerializedState.cs`, and
  `Adapters/WorldSecretSeedRuntimeRegistrySerializationAdapter.cs`.
- Core behavior implemented: world/session identity, immutable enabled-set snapshots,
  cardinality-derived active count, version/lifecycle validation, enable/disable/clear idempotency,
  idempotency-key conflict rejection, reset/close boundaries, one-way compatibility projection and
  stable-key serialization.
- Dependency impact: reads the existing definition projection only; no legacy `WorldRulesState`,
  `WorldRulesSnapshotValue`, sound, logging, persistence or network writer was changed.
- Evidence-gap: runtime integration, persistence/network delivery semantics, scheduler barrier and
  behavior-equivalence verification remain open.
- Blocking-decision: the new component is deliberately not connected to the existing legacy flags
  owner; no dual write is introduced while the canonical owner decision remains unresolved.
- VerificationStatus: not-run.

### 3.7 Checkpoint 8: WorldSecretSeedDerivedOptionsQuery

status: implemented-unverified.

Implementation unit:

1. Add an explicit input value containing the four enabled/world booleans and a 0..2 random roll
   supplied by the random adapter only when the Version4 fallback branch is evaluated.
2. Preserve GenerateBiggerAbandonedHouses as enabled-bigger OR error-world-and-roll-zero.
3. Preserve GenerateRainbowGlowsticks as enabled-rainbow OR tenth-anniversary-world.
4. Keep the Query pure; it must not read Main, WorldGen, system time, I/O or a global random
   source.
5. Record random draw count and stream version in the focused verifier before changing any legacy
   compatibility path.

Focused verifier cases:

- all enabled/error combinations with rolls 0, 1 and 2;
- invalid roll rejection;
- exact random draw timing for the fallback branch;
- anniversary fallback truth table;
- repeated explicit evaluation is deterministic and side-effect free.

Implementation checkpoint for Checkpoint 8:

- Source files saved: `Definitions/WorldSecretSeedDerivedOptionsInput.cs`,
  `Definitions/WorldSecretSeedDerivedOptionsSelection.cs`, and
  `Queries/WorldSecretSeedDerivedOptionsQuery.cs`.
- Core behavior implemented: explicit input/output values, Version4 bigger-house fallback,
  anniversary rainbow fallback, conditional explicit random roll validation and pure evaluation.
- Dependency impact: no runtime component, random source, global world state, UI, event, logging,
  persistence or network writer was changed.
- Evidence-gap: random-stream draw accounting and legacy call-site comparison remain unresolved.
- Blocking-decision: the query cannot obtain random state; callers must provide the decision for the
  fallback branch.
- VerificationStatus: not-run.

### 3.8 Checkpoint 9: WorldSecretSeedDerivedVariationsQuery

status: implemented-unverified.

Implementation unit:

1. Preserve all 22 Variations outputs and their Version4 thresholds and conflict rules.
2. Change the conceptual input from independent enabled-set/count parameters to one immutable
   runtime registry snapshot plus explicit definitions and Skyblock context.
3. Keep the Query pure and forbid Main, WorldGen, genRand, time, I/O, network and logging reads.
4. Compare the proposed result against current SecretSeedVariationQuery across a fixture matrix
   before changing call sites.
5. If a cache is introduced later, record owner, invalidation version and replacement semantics.

Focused verifier cases:

- all 22 outputs at counts 0, 1, 3, 4 and 6;
- conflicting visual rules and no-surface/extra-content combinations;
- Skyblock-dependent floating-island result;
- desert, spider-cave and trap threshold cases;
- snapshot-derived count invariant and world/version identity;
- deterministic repeated evaluation with no side effects.

### 3.9 Checkpoint 10: WorldSkyblockGenerationRulesDefinition

status: implemented-unverified.

Implementation unit:

1. Define `WorldSkyblockGenerationScanComponent` as the sole transient owner of tile presence,
   wall presence and active-tile accumulation. Keep its arrays private behind an immutable scan
   snapshot and declare the scan reset boundary explicitly.
2. Implement `WorldSkyblockGenerationRulesQuery` over the scan snapshot and explicit world
   dimensions. Preserve dungeon classification, tile IDs 12/26/58/77/133/226/404, wall ID 87,
   the strict 10% threshold and Skyblock gating.
3. Keep `denyFloatingIslands`, `denyAllGeneration` and `denySomeGeneration` in the policy query;
   feed it the standard Skyblock flag and one immutable secret-seed runtime snapshot.
4. Add a commit/effect seam for post-evaluation array reset, `Main.dungeonX/Y = -1` compatibility
   output and the lowTiles change notification. These effects must not be called by a pure Query.
5. Compare the new boundary with current `SkyblockRuleQuery` and `SkyblockPolicyQuery` before
   selecting a canonical owner or changing their call sites.

Source-to-target mapping:

| Version4 member(s) | Target proposal | Dependency/effect note |
| --- | --- | --- |
| `hasTile`, `hasWall`, `currentActiveTiles` | `WorldSkyblockGenerationScanComponent` | Scan-system state; injected grid reader owns border and tile/wall reads. |
| `noAltars`, `noDungeon`, `noTemple`, `noHellstone`, `noFossils`, `noLifeCrystals`, `noHellforge`, `lowTiles` | `WorldSkyblockGenerationRulesQuery` result | Pure calculation from one committed scan snapshot; no mutable static compatibility authority. |
| `denyFloatingIslands`, `denyAllGeneration`, `denySomeGeneration` | `SkyblockPolicyQuery` result | Pure calculation from standard flags and the runtime secret-seed snapshot. |
| `Skyblock.Calculate` resets and effects | `WorldSkyblockGenerationRulesCommitSystem` plus effect port | Reset, dungeon-coordinate mutation and network notification remain explicit side effects. |

Focused verifier cases:

- exact 11-field/three-property coverage and one owner per member;
- scan bounds `[40, maxTilesX - 40)` and `[40, maxTilesY - 40)`, active-only tile count and
  all-wall recording;
- dungeon tile/wall classification, temple wall 87, hellforge 77/133 and every listed tile ID;
- strict `< 0.1f` low-tile behavior, Skyblock gating, reset and zero-area contract;
- deny-policy truth table for each of the eight exception variants and non-Skyblock worlds;
- pure-query side-effect probe and comparison with the current two NLTX queries.

Implementation checkpoint:

- Source files saved: `Definitions/WorldSkyblockGenerationScanLifecycle.cs`,
  `Definitions/WorldSkyblockGenerationDimensions.cs`,
  `Adapters/WorldSkyblockGenerationTileObservation.cs`,
  `Adapters/IWorldSkyblockGenerationGridReader.cs`,
  `Components/WorldSkyblockGenerationScanComponent.cs`,
  `Components/WorldSkyblockGenerationScanSnapshot.cs`,
  `Definitions/WorldSkyblockGenerationRulesInput.cs`,
  `Definitions/WorldSkyblockGenerationRulesSelection.cs`,
  `Definitions/WorldSkyblockGenerationPolicySelection.cs`,
  `Queries/WorldSkyblockGenerationRulesQuery.cs`,
  `Queries/WorldSkyblockGenerationPolicyQuery.cs`,
  `Systems/WorldSkyblockGenerationScanSystem.cs`,
  `Adapters/ISkyblockGenerationEffectsPort.cs`,
  `Systems/WorldSkyblockGenerationRulesCommitResult.cs`, and
  `Systems/WorldSkyblockGenerationRulesCommitSystem.cs`.
- Core behavior implemented: explicit scan lifecycle, private accumulation arrays, immutable scan
  snapshots, Version4 scan bounds and classification, pure no-content/policy queries, session/version
  checks, post-commit reset and explicit dungeon/low-tile effect ports.
- Dependency impact: existing legacy Skyblock queries, `Main`, network, logging and world descriptor
  writers were not changed; no new call-site owner was selected.
- Evidence-gap: exact current NLTX scan call sites, zero-area policy approval and behavior-equivalence
  matrix remain open.
- Blocking-decision: pure queries do not reset scans or write dungeon coordinates, tiles, network or
  logs; effects remain behind `ISkyblockGenerationEffectsPort`.
- VerificationStatus: not-run.

This verifier has not been run: verificationStatus: not-run.

### 3.10 Checkpoint 11: WorldSeedOptionCatalogDefinition

status: implemented-unverified.

Implementation unit:

1. Add an immutable ten-entry catalog in Version4 registration order: Normal, NotTheBees, Drunk,
   Anniversary, DontStarve, ForTheWorthy, NoTraps, Remix, Everything and Skyblock.
2. Preserve every reported `KeyName` and `ServerConfigName`, including Normal's null config name.
3. Represent `Everything.Dependencies` as the ordered immutable IDs Remix, Drunk, NotTheBees,
   NoTraps, DontStarve, Anniversary and ForTheWorthy. Treat the legacy `_dependencies` field as a
   lazy derived cache, never as a separate authority.
4. Add a pure catalog query/adapter seam for normalized seed names, translated numeric seed values
   and `seed_` server configuration lines. Keep UI, event callbacks, AutoGenEnabled mutation and
   selected-flag commits outside the query.
5. Compare selection precedence and reset/enable behavior with Version4 before mapping the catalog
   to existing NLTX request and snapshot flags.

Source-to-target mapping:

| Version4 member(s) | Target proposal | Dependency/effect note |
| --- | --- | --- |
| `WorldSeedOption_Everything._dependencies` | immutable dependency IDs in `WorldSeedOptionCatalogDefinition` | Derived once from catalog IDs; no mutable legacy option list is exposed. |
| ten `KeyName` properties | option definition metadata | Preserve exact localization keys and registration order. |
| ten `ServerConfigName` properties | option definition metadata | Preserve exact names and Normal's null. |
| `Everything.Dependencies` | `IReadOnlyList<WorldSeedOptionId>` or equivalent immutable view | Preserve the seven-item order and exclude Normal/Everything/Skyblock. |
| `WorldGenerationOptions` selection/parsing effects | `WorldSeedOptionCatalogQuery` plus adapter/commit system | Matching is pure; events, UI and flag mutation remain effects. |

Focused verifier cases:

- exact ten-entry order and all key/config metadata;
- exact dependency IDs/order, stable read-only view and no mutable option instance leakage;
- special seed names, translated values, normalization, registration-order precedence and no match;
- `seed_` config parsing, clamping and unknown-name behavior;
- adapter-only event/UI/AutoGen effects and one canonical selected-flag commit.

Implementation checkpoint:

- Source files saved: `Definitions/WorldSeedOptionId.cs`,
  `Definitions/WorldSeedOptionDefinition.cs`,
  `Definitions/WorldSeedOptionCatalogDefinition.cs`,
  `Definitions/WorldSeedOptionMatch.cs`,
  `Definitions/WorldSeedOptionSeedMatchResult.cs`,
  `Definitions/WorldSeedOptionAutoGenerationResult.cs`, and
  `Queries/WorldSeedOptionCatalogQuery.cs`.
- Core behavior implemented: immutable Version4 registration order and metadata, immutable
  Everything dependency IDs, normalized special-name/numeric matching, `seed_` parsing and value
  clamping, and pure candidate results.
- Dependency impact: no selected-flag writer, AutoGen mutation, UI, event, persistence or network
  adapter was changed.
- Evidence-gap: numeric translation is supplied by the caller, and canonical flag commit,
  persistence and network ownership remain unresolved.
- Blocking-decision: catalog queries do not select, enable or publish options; effects remain at an
  explicit adapter/commit boundary.
- VerificationStatus: not-run.

This verifier has not been run: verificationStatus: not-run.

### 3.11 Checkpoint 12: WorldLandmassAndTreeProfilesDefinition

status: implemented-unverified.

Implementation unit:

1. Add `WorldLandmassDefinition` as a value type preserving `DataType`, `Position`,
   `RadiusOrHalfSize`, `Style` and the exact `Top` getter/setter relation.
2. Add `WorldTreeProfileCatalog` with the ten Version4 profiles and immutable numeric values:
   tree tile, sapling tile, height range and top padding.
3. Preserve Version4 profile declaration order and `TryGetFromTreeId` mapping independently;
   do not let filesystem or type-registration order define behavior.
4. Represent `GroundTest` and `WallTest` as explicit predicate IDs or injected callable seams.
   Legacy delegate materialization belongs to an adapter that receives tile/wall definitions or a
   world-grid query; definitions cannot read global tiles.
5. Compare the proposed catalog with `LegacyTreeProfileRegistry` and existing tree eligibility
   queries, including the known enumeration-order difference, before changing consumers.

Source-to-target mapping:

| Version4 member(s) | Target proposal | Dependency/effect note |
| --- | --- | --- |
| `LandmassData.DataType`, `Position`, `RadiusOrHalfSize`, `Style`, `Top` | `WorldLandmassDefinition` | Value-local Top translation; no hidden cross-component write. |
| ten `GrowTreeSettings.Profiles` fields | `WorldTreeProfileCatalog` | Immutable profile values plus explicit declaration order and tile-ID lookup. |
| `TreeTileType`, `TreeHeightMin`, `TreeHeightMax`, `TreeTopPaddingNeeded`, `SaplingTileType` | profile value fields | Preserve all numeric values and types. |
| `GroundTest`, `WallTest` | predicate IDs/callable ports plus compatibility adapter | Tile/wall reads are explicit query inputs, not definition side effects. |

Focused verifier cases:

- Top/Position/Radius round trips and preservation of DataType/Style;
- all ten profile numeric values, declaration order and tile-ID lookup;
- explicit predicate seam behavior with ground, wall, liquid, slope and boundary fixtures;
- unknown profile rejection without default-profile leakage;
- no definition, catalog or query path mutates tiles, world state, persistence, network or UI.

This verifier has not been run: verificationStatus: not-run.

Implementation checkpoint for Checkpoint 12:

- Source files saved: `Definitions/WorldLandmassDataType.cs`,
  `Definitions/WorldLandmassDefinition.cs`, `Definitions/WorldTreeProfileId.cs`,
  `Definitions/WorldTreeGroundPredicateId.cs`, `Definitions/WorldTreeWallPredicateId.cs`,
  `Definitions/WorldTreeProfileDefinition.cs`, `Definitions/WorldTreeProfileCatalog.cs`, and
  `Queries/WorldTreeProfileCatalogQuery.cs`.
- Core behavior implemented: value-local landmass `Top` translation, the three Version4 landmass
  data types, ten immutable tree profiles in Version4 declaration order, tile-ID lookup, explicit
  ground/wall predicate IDs, defensive read-only catalog views and explicit no-profile results.
- Dependency impact: no existing landmass component, legacy tree registry, tree placement system,
  tile/wall state, persistence, network or UI writer was changed. The scan presence-set loop uses an
  `int` index before conversion to `ushort`, avoiding 65536-entry array wraparound.
- Evidence-gap: profile consumer order, predicate adapter equivalence and landmass persistence/version
  ownership remain unresolved.
- Blocking-decision: do not replace the legacy tree registry or connect landmass persistence until
  the consuming owner and value-serialization contract are identified.
- VerificationStatus: not-run; build and focused behavior checks are pending.

## 4. Source-to-Target and Dependency Impact

| Source | Target proposal | Dependency impact |
| --- | --- | --- |
| Terraria.WorldGen static generation flags | WorldGenerationSecretSeedFlagsComponent plus compatibility projection | WorldGen.Reset becomes an adapter/commit caller; Main consumers read a projection during the compatibility window. |
| WorldGen.SecretSeed registration and fields | registry definition projection, input adapter and runtime component | Sound, regex, random and unlock-text effects leave immutable definitions. |
| WorldGen.SecretSeed.Variations | WorldSecretSeedDerivedVariationsQuery | Consumers depend on a snapshot/query interface, not static SecretSeed fields. |
| WorldGen.Skyblock | scan component plus rules query | Tile/wall scan writers depend on an accumulation API; query cannot reset arrays or write Main.dungeonX/Y. |
| WorldGenerationOptions and AWorldGenerationOption | option definition/catalog query plus adapter | UI/event/config effects stay at the edge; domain selection receives normalized explicit input. |
| LandmassData | WorldLandmassDefinition | Top must preserve Position plus/minus RadiusOrHalfSize semantics; avoid using a property setter as hidden cross-component mutation. |
| GrowTreeSettings and Profiles | WorldTreeProfileCatalog and profile value | Delegate predicates become explicit query dependencies; tile mutation stays in growth systems. |

The current NLTX WorldRuleSnapshotComponent, WorldSeedComponent, secret-seed queries, Skyblock
queries and LegacyTreeProfileRegistry are existing partial mappings. Do not create a second
canonical writer until an API migration decision identifies which existing type is replaced or
extended.

## 5. State Ownership and Effect Boundaries

| State/effect | Single proposed owner | Read path | Write/effect path | Retry/duplicate policy |
| --- | --- | --- | --- | --- |
| Selected standard seed flags | WorldGenerationSecretSeedFlagsComponent | immutable world snapshot | option-selection commit system | replacement per generation session; no merge |
| Enabled secret seeds/count | WorldSecretSeedRuntimeRegistryComponent | variation/derived queries | runtime registry commit system | enable/disable idempotent by variant; count derived from set |
| Secret seed metadata | registry projection | read-only catalog | startup/catalog construction | immutable for a process; no runtime writes |
| Input plaintext/unlock text | input adapter result/command | validation result | adapter captures normalized input | one result per input; no catalog mutation on rejection |
| Skyblock scan arrays/count | WorldSkyblockGenerationScanComponent | scan commit and rules query | tile scan system | reset only at explicit scan boundary |
| Skyblock no-content/low-tile result | rules query output or committed snapshot owned by generation | read-only query | scan rules system if persistence is required | recompute from committed scan; no cache without invalidation |
| Random derived option | WorldSecretSeedDerivedOptionsQuery result | explicit random decision input | random adapter supplies decision | random stream version and consumption recorded |
| Tree/landmass definitions | immutable definition catalogs | generation queries | startup/catalog construction | immutable; version with content if persistence needs it |
| Sound, UI, logging, network, persistence | explicit adapters/projections | snapshot inputs | boundary adapter | document delivery semantics before implementation |

## 6. System and Query Contracts

The implementation must preserve this explicit order:

1. WorldSeedOptionCatalogQuery parses/validates input without writing global state.
2. WorldGenerationSecretSeedFlagsCommitSystem commits the selected flag snapshot.
3. WorldSecretSeedRuntimeRegistryCommitSystem commits enabled secret seeds and count.
4. WorldSecretSeedDerivedOptionsQuery and WorldSecretSeedDerivedVariationsQuery calculate outputs
   from stable snapshots. They must not write components.
5. WorldSkyblockGenerationScanSystem accumulates scan facts and commits/resets them at a known
   boundary; WorldSkyblockGenerationRulesQuery calculates results afterward.
6. Generation systems read definitions/results and submit explicit tile/world commands.
7. Persistence and replication projections read committed snapshots after the authority barrier.

Pure query contract:

~~~text
output = Query(explicit immutable input, explicit random decision if required)
~~~

No query may call Main, WorldGen, system time, random global state, file I/O, network, logging or
an event publisher directly. Adapters may do so, but must record reads, writes, failure, ordering,
retry and duplicate semantics.

## 7. Verification Plan and Actual Result

The affected project build was attempted after the source checkpoints were saved. It was run
serially through the repository wrapper with no restore:

~~~powershell
$dotnetArgs = @(
  'build',
  '.\src\WorldSession\Terraria.WorldSession.csproj',
  '--no-restore',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
.\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs
~~~

Result from the latest run: exit code `1`, `0` warnings and `1` compiler error. The build reached
`CoreCompile` but failed in the external-session file
`src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs(16,22)` because the call to
`TryAdvanceAfterSuccessfulPlacement` has no overload accepting one argument. An earlier run failed
with five `Terraria.Content`/`ColorRgba` errors in
`WorldGeneration/Actions/WorldGenerationTileFramingAndDebugActionsCommand.cs`. No P18 source file
appeared in either diagnostic output. The expected
`Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` is not trusted evidence
because the affected project did not build. The failure is recorded as an external workspace
blocker, not as P18 behavior evidence.

The existing `Test/Terraria.WorldSession.WorldGeneration.Verification` and
`src/WorldSessionFocusedVerifier` projects reference the same full WorldSession project and do not
contain P18-specific cases. No focused behavior verifier was run because the affected project did
not build.

After the shared checkout had no active `dotnet.exe`/`csc.exe` compiler owner, the selected P18
source boundary was compiled with the repository wrapper using this focused probe command:

~~~powershell
$dotnetArgs = @(
  'build',
  '.\\Build\\Diagnostics\\P18WorldGenerationCompileProbe\\P18WorldGenerationCompileProbe.csproj',
  '--no-restore',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs
~~~

The probe exited `0`, with `0` warnings and `0` errors, and produced
`Build/bin/P18WorldGenerationCompileProbe/Debug/net10.0/P18WorldGenerationCompileProbe.dll`.
This is isolated compile evidence for the selected P18 files only; it does not prove full project
integration or behavior equivalence. P18 verification is therefore `partially-verified`, while
the focused behavior matrices remain `not-run`.

Focused checks by boundary:

- flags: ten-member mapping, defaults, replacement, world isolation and projection one-wayness;
- registry: registration order, normalization, invalid input, clear/enable/disable/count and no
  mutation of immutable definitions;
- derived variations: all 22 truth-table outputs at counts 0, 1, 3, 4 and 6 with conflicting flags;
- derived options: explicit random decisions and anniversary fallback;
- Skyblock: tile/wall presence, dungeon wall 87, hellforge 77/133, 10% threshold, reset and
  denySomeGeneration exception set;
- options: all ten catalog options, special seed names/values, server config names and Everything
  dependencies;
- landmass/tree: Top round trip, profile defaults, tile lookup and predicate seam.

Static/document checks before every commit:

- report member count remains 136 and every source sequence 2235..2656 is mapped once;
- proposed type names have one core public type per same-named file;
- no generic catch-all directories or file-order scheduler assumptions;
- no query writes, hidden global reads or mutable collection leakage;
- no source files, tests, build outputs or other partition documents are changed.

## 8. Rollback and Completion Criteria

Rollback one step by restoring the prior adapter/projection and removing only the newly introduced
boundary after the focused verifier confirms the old path still works. Do not delete or rewrite
legacy source until the replacement has a passing behavior matrix and a documented persistence
migration.

The P18 implementation can be considered complete only when every member has one authoritative
owner or an approved deferred/integration-review destination, old and new write paths are not
dual-active, persistence/network behavior is explicit, focused verifiers pass, affected projects
build serially into Build/bin and scheduler order is tested independently of file order.

Current implementation state is intentionally not verification-complete:

~~~yaml
executionStatus: in-progress
implementationStatus: in-progress
verificationStatus: partially-verified
~~~

### 3.12 P18 isolated compile verification checkpoint

- Verification command: `Build/Tools/Invoke-SerialDotnet.ps1` with the
  `Build/Diagnostics/P18WorldGenerationCompileProbe/P18WorldGenerationCompileProbe.csproj`
  project, `--no-restore`, `-m:1`, `-nr:false`, `UseSharedCompilation=false`,
  `MSBuildNodeReuse=false` and `BuildInParallel=false`.
- Result: exit code `0`, `0` warnings, `0` errors. Artifact:
  `Build/bin/P18WorldGenerationCompileProbe/Debug/net10.0/P18WorldGenerationCompileProbe.dll`.
- Scope: Definitions, Components, Queries, Systems, Adapters and SecretSeed Commands selected for
  P18. No runtime behavior matrix or full `Terraria.WorldSession` integration was asserted.
- Remaining block: `WorldGenerationSecretSeedFlagsComponent` remains pending because the
  canonical world-rule writer and persistence/network compatibility contract are unresolved.

### 3.13 2026-09-11T22:12:14Z: WorldSkyblockGenerationRulesDefinition snapshot-boundary hardening

- Actual source files modified:
  `src/WorldSession/WorldGeneration/Components/WorldSkyblockGenerationScanSnapshot.cs` and
  `src/WorldSession/WorldGeneration/Definitions/WorldSkyblockGenerationRulesInput.cs`.
- Core behavior: each constructor now copies incoming `IReadOnlySet<ushort>` values into a local
  `FrozenSet<ushort>`. This preserves the scan component's single-writer invariant and ensures a
  rules query evaluates an immutable input even if the adapter caller still owns a mutable source
  collection.
- Dependency impact: no `Main`, tile grid, dungeon-coordinate, network, persistence or scheduler
  action was added. No legacy compatibility writer or standard-seed flag owner was changed.
- Verification command and result: `Invoke-SerialDotnet.ps1` build of
  `src/WorldSession/Terraria.WorldSession.csproj` with `--no-restore`, `-m:1`, `-nr:false`,
  `UseSharedCompilation=false`, `MSBuildNodeReuse=false` and `BuildInParallel=false` exited `1`
  with `0` warnings and `1` external `HellChestLootCycleSystem.cs(16,22)` error. The isolated
  `P18WorldGenerationCompileProbe` build with the same serial flags exited `0` with `0` warnings
  and `0` errors; artifact:
  `Build/bin/P18WorldGenerationCompileProbe/Debug/net10.0/P18WorldGenerationCompileProbe.dll`.
- VerificationStatus: `partially-verified`; no P18 focused behavior verifier exists or ran.
- Remaining block: `WorldGenerationSecretSeedFlagsComponent` remains pending because its
  canonical world-rule writer, compatibility window, persistence/network policy and scheduler
  barrier are still unresolved. No dual writer was introduced.

### 3.14 2026-09-11T22:50:28Z: WorldSkyblockGenerationRulesQuery non-Skyblock gating

- Source file modified: `src/WorldSession/WorldGeneration/Queries/WorldSkyblockGenerationRulesQuery.cs`.
- Core behavior: `Evaluate` now returns `false` for `NoAltars`, `NoDungeon`, `NoTemple`,
  `NoHellstone`, `NoFossils`, `NoLifeCrystals`, `NoHellforge` and `LowTiles` when the explicit
  `SkyblockWorld` input is false, while preserving the input `GenerationId` and `ScanVersion`.
  Skyblock scans retain the existing tile/wall classification and strict low-tile calculation.
- Dependency impact: the query remains pure; no scan reset, tile/world mutation, dungeon-coordinate
  write, network notification, registry write or standard-flag owner was changed.
- Evidence-gap: the focused behavior matrix, including the non-Skyblock branch and the remaining
  Skyblock truth table, is still not-run; full-project integration remains blocked by the external
  `HellChestLootCycleSystem.cs(16,22)` compile error.
- Blocking-decision: the deferred `WorldGenerationSecretSeedFlagsComponent` remains unimplemented;
  this query change does not introduce a competing world-rule authority.
- VerificationStatus: partially-verified; the serial P18 compile-probe rerun passed, while focused
  behavior matrices and full-project integration remain unverified.

### 3.15 2026-09-11T22:56:30Z: P18 compile verification after Skyblock query gating

- Verification command: `Build/Tools/Invoke-SerialDotnet.ps1` with
  `Build/Diagnostics/P18WorldGenerationCompileProbe/P18WorldGenerationCompileProbe.csproj`,
  `build`, `--no-restore`, `-m:1`, `-nr:false`, `UseSharedCompilation=false`,
  `MSBuildNodeReuse=false` and `BuildInParallel=false`.
- Result: exit code `0`, `0` warnings and `0` errors. The expected artifact exists at
  `Build/bin/P18WorldGenerationCompileProbe/Debug/net10.0/P18WorldGenerationCompileProbe.dll`.
- Scope: the isolated P18 Definitions, Components, Queries, Systems, Adapters and SecretSeed
  Commands included by the probe compile after the `WorldSkyblockGenerationRulesQuery` change.
  No runtime behavior matrix or full `Terraria.WorldSession` integration was asserted.
- VerificationStatus: `partially-verified`; the full project remains blocked by the external
  `HellChestLootCycleSystem.cs(16,22)` overload error documented above, and no P18 focused
  behavior verifier has run.

### 3.16 2026-09-11T23:01:41Z: full WorldSession build recheck

- Verification command: `Build/Tools/Invoke-SerialDotnet.ps1` with
  `build src/WorldSession/Terraria.WorldSession.csproj --no-restore -m:1 -nr:false`,
  `UseSharedCompilation=false`, `MSBuildNodeReuse=false` and `BuildInParallel=false`.
- Result: exit code `1`, `0` warnings and `1` error. The failure remains external to P18 at
  `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs(16,22)` with `CS1501`
  because `TryAdvanceAfterSuccessfulPlacement` has no one-argument overload. No P18 source error
  was reported; the expected full-project DLL is not trusted because the build failed.
- VerificationStatus: `partially-verified`; isolated P18 compilation passed, but full integration
  and focused Skyblock behavior matrices remain unverified.

### 3.17 2026-09-12T01:53:45Z: P18 focused behavior verifier

- Source files added: `src/WorldGenerationP18FocusedVerifier/Program.cs` and
  `src/WorldGenerationP18FocusedVerifier/Terraria.WorldGenerationP18FocusedVerifier.csproj`.
- Coverage: 35-entry secret-seed catalog order and rule mapping; runtime registry enable,
  idempotency, conflict, stale-version, disable and close transitions; immutable Skyblock scan
  snapshot input and non-Skyblock gating; Skyblock policy for Skyblock and non-Skyblock worlds;
  empty-runtime derived variations; ten-option seed catalog matching and server configuration;
  landmass `Top` round-trip and tree profile lookup.
- Build command: `Build/Tools/Invoke-SerialDotnet.ps1` with `build
  src/WorldGenerationP18FocusedVerifier/Terraria.WorldGenerationP18FocusedVerifier.csproj
  --no-restore -m:1 -nr:false`, `UseSharedCompilation=false`, `MSBuildNodeReuse=false` and
  `BuildInParallel=false`.
- Build result: exit code `0`, `0` warnings and `0` errors. Artifact:
  `Build/bin/Terraria.WorldGenerationP18FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationP18FocusedVerifier.dll`.
- Run command: the same wrapper with `run --project
  src/WorldGenerationP18FocusedVerifier/Terraria.WorldGenerationP18FocusedVerifier.csproj
  --no-build --no-restore` and the same serial MSBuild properties.
- Run result: exit code `0`, output `P18 focused verifier passed.`
- VerificationStatus: `partially-verified`; selected P18 state/query/error boundaries now have fresh
  focused evidence, but full Version4 parity, scheduler ordering, persistence/network behavior and
  full `Terraria.WorldSession` integration remain unverified. `WorldGenerationSecretSeedFlagsComponent`
  remains deferred because its canonical owner is unresolved.

### 3.18 2026-09-12T07:16:31Z: WorldGenerationSecretSeedFlagsComponent checkpoint and serial verification

- Actual source file:
  `src/WorldSession/WorldGeneration/Components/WorldGenerationSecretSeedFlagsComponent.cs`.
- Implemented scope: the immutable data shape only. It exposes the nine selected Version4
  generation inputs and derives `DrunkWorldGenerationText` from `DrunkWorldGeneration`; all
  constructor inputs default to `false`. The Version4 anchors are `WorldGen.cs:4306`, `4308`,
  `4310`, `4312`, `4314`, `4316`, `4318`, `4320`, `4322` and `4324`.
- No writer, commit system, snapshot, projection, query, adapter, command, test, interface or
  compatibility bridge was added. Existing overlapping types and non-component code were not
  modified.
- Affected-project build command, run from the repository root through
  `Build/Tools/Invoke-SerialDotnet.ps1`:

  ~~~powershell
  $dotnetArgs = @('build', '.\\src\\WorldSession\\Terraria.WorldSession.csproj', '--no-restore',
    '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
    '-p:BuildInParallel=false')
  & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs
  ~~~

- Build result: exit code `1`, `0` warnings and `1` error at the external workspace file
  `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs(16,22)`, `CS1501`:
  `TryAdvanceAfterSuccessfulPlacement` has no overload accepting one argument. No P18 source
  error was reported. `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`
  exists only as an older artifact from before this failed build and is not trusted evidence.
- Focused verifier regression command, run through the same wrapper with `--no-build --no-restore`:

  ~~~powershell
  $dotnetArgs = @('run', '--project',
    '.\\src\\WorldGenerationP18FocusedVerifier\\Terraria.WorldGenerationP18FocusedVerifier.csproj',
    '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
  & .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs
  ~~~

- Focused verifier result: exit code `0`, output `P18 focused verifier passed.` The artifact
  `Build/bin/Terraria.WorldGenerationP18FocusedVerifier/Debug/net10.0/Terraria.WorldGenerationP18FocusedVerifier.dll`
  is existing regression evidence only; the verifier does not directly include or validate the
  new `WorldGenerationSecretSeedFlagsComponent`.
- VerificationStatus remains `partially-verified`. The component data shape is implemented, but
  canonical owner, compatibility window, persistence/network policy and scheduler barrier remain
  unresolved. The component stays in `pendingComponents`; no dual writer is permitted.
