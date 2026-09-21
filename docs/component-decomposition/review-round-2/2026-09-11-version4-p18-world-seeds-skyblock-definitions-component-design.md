# P18 World Seeds, Skyblock and Definition Components

## Document Metadata

~~~yaml
documentType: component-design
designStatus: proposed
partitionId: P18
sessionId: 2e09fab3b2e84e1a965d4c1f3a24c32d
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P18-World-Seeds-Skyblock-Definitions.md
outputDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-p18-world-seeds-skyblock-definitions-component-design.md
outputExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-p18-world-seeds-skyblock-definitions-component-execution.md
evidenceStatus: partial
nltxStatus: partial
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
  - Version4 call-site ownership, world reset/cleanup scope, persistence, network replication and exact scheduler order are not closed for all 136 members.
  - The repository-referenced 约束/公共拆分约束.md is absent; AGENTS.md, ECS文件组织设计约束.md and public-decomposition remain the applicable available constraints.
  - C02-C12 definition, query, scan, command, adapter and projection boundaries now have source mappings; the P18 focused verifier covers selected state/query/error boundaries, while complete Version4 behavior equivalence remains unverified. The isolated P18 compile probe and focused verifier pass.
  - GenerateBiggerAbandonedHouses currently depends on Version4 genRand.Next(3); the random draw boundary is not yet an NLTX contract.
  - The current NLTX variation API accepts enabled variants and active count separately; their consistency is not enforced by the API.
  - Version4 Skyblock.ScanTiles/Calculate owns scan reset, active-tile accumulation, dungeon-coordinate mutation and lowTiles replication effects; the cross-partition effect owner is not verified.
  - Current NLTX SkyblockRuleQuery and SkyblockPolicyQuery cover derived outputs only; exact scan bounds, reset lifecycle and Version4 dungeon/wall classification are not represented by one owner. The P18 scan snapshot and rules input now defensively freeze externally supplied sets, and the focused verifier covers selected gating/classification cases, but no behavior matrix has established full Version4 parity.
  - No complete NLTX option catalog mapping was found for the Version4 registration order, key/config metadata, special seed matching and Everything dependency list.
  - Option selection, event callbacks, UI construction and server configuration mutation are external effects whose persistence and compatibility owner remain unresolved.
  - Version4 tree predicate delegates and LandmassData are definition-level declarations, but their complete NLTX predicate adapters, profile enumeration consumers and persistence contract are not verified.
  - Current NLTX tree numeric values are present, but profile declaration order differs from the Version4 declaration order and requires call-site evidence before replacement.
  - `WorldSkyblockGenerationRulesQuery` now explicitly gates all eight no-content/low-tile outputs to `false` for non-Skyblock worlds; the focused behavior matrix for this branch and the remaining Skyblock truth table is still not-run.
  - The affected `Terraria.WorldSession` serial build is blocked by external-session source: the latest run reports one `HellChestLootCycleSystem.cs(16,22)` overload error; an earlier run reported five `WorldGenerationTileFramingAndDebugActionsCommand.cs` `Terraria.Content.ColorRgba` errors. No P18 source error was reported.
blocking-decision:
  - WorldGenerationSecretSeedFlagsComponent now has an immutable data shape at `src/WorldSession/WorldGeneration/Components/WorldGenerationSecretSeedFlagsComponent.cs`, but existing WorldRulesState and WorldRulesSnapshotValue remain partial mappings; the canonical writer, compatibility window, persistence/network policy and scheduler barrier are unresolved, and no second writer is allowed.
  - Random-dependent derived options require an explicit random input or adapter; they must not become hidden global reads in a pure Query.
  - Do not claim deterministic behavior until random-stream version, draw timing and rejection/rollback semantics are specified.
  - Variation evaluation must consume one immutable runtime snapshot; callers may not provide an independently writable count.
  - Scan arrays and active-tile accumulation have one transient scan owner; no pure Query may reset them or write dungeon coordinates, network state or tiles.
  - Skyblock no-content and low-tile values are derived from one committed scan snapshot; compatibility fields are projections and may not become independent mutable flags.
  - `_dependencies` is a derived immutable catalog value, not a second mutable authority; the catalog must not expose `AWorldGenerationOption` instances or a mutable dependency list.
  - Seed parsing and catalog lookup stay pure; option events, UI, AutoGenEnabled mutation and selected-flag commits require explicit adapters and commit systems.
  - Landmass `Top` mutation remains value-local and must preserve Position/RadiusOrHalfSize round trips; authoritative component replacement requires an explicit commit port.
  - GroundTest and WallTest are explicit predicate seams/adapters; no profile definition may read tiles, walls, liquid or global WorldGen state implicitly.
~~~

Types without an implementation checkpoint remain proposed. The checkpoints below record actual
source changes and do not claim complete Version4 behavior equivalence.

## 1. Scope and Design Summary

P18 covers Version4 world-generation seed flags, secret-seed definitions and runtime state,
secret-seed-derived rule queries, Skyblock generation restrictions, world-seed option catalog,
landmass data and tree-generation profiles. The authoritative inventory contains 12 leaf
subsystems, 86 fields, 50 properties and 136 members.

The proposed decomposition keeps four concerns separate:

1. World-scoped authoritative inputs and mutable runtime state: selected generation flags and
   enabled secret seeds.
2. Immutable definition/catalog data: secret-seed metadata, world-seed option metadata, Skyblock
   rule definitions and tree/landmass profiles.
3. Pure or explicitly-input derived queries: secret-seed variations, derived options and
   Skyblock eligibility/results.
4. One-way boundaries: seed-input adapters, registry projections, persistence snapshots and
   client/network projections. None of these projections writes back to authority.

The design does not assign final owners for fields shared with other partitions. Such rows are
marked crossSubsystemOwner: integration-review and must be resolved against P16, P17, P19 and P20
work before implementation.

Excluded from this session are generation pass algorithms, tile mutation commands, liquid
simulation, biome execution, world-file serialization implementation and client presentation.
Those consumers receive stable read-only queries or explicit commands after integration review.

## 2. Evidence Baseline

| Evidence source | Location and lines | Fact used | Readers/writers/lifecycle | Side effects | evidenceStatus |
| --- | --- | --- | --- | --- | --- |
| Version4 secret-seed variation and declarations | D:\TRbackup\Version4\Terraria\WorldGen.cs:40-444 | SecretSeed.Variations, 35 registered seed definitions, definition fields and two derived properties exist. | Variation properties read seed Enabled and activeSecretSeedCount; registration is static initialization. | Derived options can read genRand and Main; registration creates and appends definitions. | confirmed for declaration; partial for ownership |
| Version4 secret-seed lifecycle | D:\TRbackup\Version4\Terraria\WorldGen.cs:463-570 | Input normalization, registration, clear, enable/disable, active-count maintenance and initialization are explicit. | Reset/initialization reads options; CheckInputForSecretSeed writes plaintext/unlock text; Enable/Disable write enabled/count. | Optional sound playback in Enable; randomness in GenerateBiggerAbandonedHouses. | confirmed for local behavior; partial for world scope |
| Version4 selected seed flags | D:\TRbackup\Version4\Terraria\WorldGen.cs:10148-10165 | World-seed options are copied into Main and WorldGen flags before secret-seed initialization. | World reset is the lifecycle boundary; downstream generation reads flags. | Static/global writes and random generator reset occur in the same reset operation. | confirmed for source ordering; partial for NLTX owner |
| Version4 Skyblock rules | D:\TRbackup\Version4\Terraria\WorldGen.cs:3141-3280 | Tile/wall scans derive no-content flags, low-tile state and reset scan buffers. | Scan/update code writes arrays/counter; Calculate reads them and writes derived flags. | Writes Main.dungeonX/Y when no dungeon; effects continue through later consumers. | confirmed for source behavior; partial for cross-partition scan owner |
| Version4 world-seed options | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerationOptions.cs:9-123; AWorldGenerationOption.cs:10-50; WorldSeedOption_*.cs | Registry, selection, seed-text parsing, config names, enabled state and dependency list. | Static registry owns option instances; selection mutates Enabled and events. | Event callbacks and UI element creation are outside the catalog query. | confirmed for declaration; partial for persistence/network |
| Version4 landmass/tree definitions | D:\TRbackup\Version4\Terraria.WorldBuilding\LandmassData.cs:5-25; D:\TRbackup\Version4\Terraria\WorldGen.cs:3838-4008 | Landmass geometry and ten tree profiles are definition/value data; Top is derived from Position/Radius. | Generation passes read profiles/data; Top setter mutates Position. | Delegate fields invoke tile/wall predicates during growth. | confirmed for declaration; partial for consumers |
| Current NLTX secret-seed coverage | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\SecretSeedDefinitionRegistry.cs:1-62; SecretSeedVariationQuery.cs:1-90; SecretSeedRuleSnapshotQuery.cs:1-46 | NLTX has a read-only definition list, snapshot count and variation query for a subset of P18 semantics. | Queries read explicit enabled variants/count; no authoritative world mutation is present. | No I/O; inputs are explicit. | confirmed for existing partial mapping |
| Current NLTX Skyblock/tree coverage | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\SkyblockPolicyQuery.cs:1-35; SkyblockRuleQuery.cs:1-44; LegacyTreeProfileRegistry.cs:1-170 | NLTX has pure Skyblock policy/rule queries and a ten-profile tree registry. | Queries consume explicit scan/variant inputs; registry is read-only. | No I/O in the inspected code. | confirmed for existing partial mapping |
| Current NLTX world input components | D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\Components\WorldSeedComponent.cs:5-23; WorldRuleSnapshotComponent.cs:5-90; WorldGenerationRequest.cs:1-278 | Seed identity and selected world-rule flags already exist, including Skyblock and several secret-seed flags. | Request construction owns validation; generation stage consumes snapshots. | No hidden I/O in the inspected constructors. | confirmed for partial mapping |
| ECS organization reference | C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Burial\Components\GraveComponent.cs:1-41; StrapComponent.cs:1-80 | Reference style uses capability-local components with explicit state fields and separate systems. | Reference only informs granularity, not Terraria semantics. | Reference contains networking attributes, not used as direct NLTX API. | existing-evidence |
| tModLoader API mirror | D:\TRbackup\tmodloader-api-docs-stable\index.html, header tModLoader v2026.07 | Exact P18 private types/members were not found in the visible annotated/member indexes. | The mirror cannot confirm Version4 private implementation or NLTX ownership. | Read-only local documentation. | missing/partial |

### 2.1 Evidence interpretation

Version4 source evidence is sufficient to propose boundaries and preserve the observed formulas,
registration order and mutation points. It is not sufficient to claim a complete ECS ownership
decision because the selected source contains static state, generated/instrumented bodies and
cross-partition consumers. Current NLTX evidence is implementation evidence only for the types
actually found; it does not close the remaining 136-member mapping.

## 3. Complete P18 Member Coverage

The following table covers every member in the authoritative report. Names are grouped for
readability; each row retains the report source sequence and direct source location.

| Leaf subsystem | Proposed boundary | Members and source anchors | Classification |
| --- | --- | --- | --- |
| WorldGenerationSecretSeedFlags | WorldGenerationSecretSeedFlagsComponent | 2532-2541: remixWorldGen, everythingWorldGen, noTrapsWorldGen, drunkWorldGen, getGoodWorldGen, tenthAnniversaryWorldGen, dontStarveWorldGen, notTheBees, skyblockWorldGen, drunkWorldGenText; WorldGen.cs:4306-4324 | authoritative world input; one component, one owner commit system |
| WorldSecretSeedRegistryDefinitions | WorldSecretSeedRegistryDefinitionsProjection plus WorldSecretSeedInputAdapter | 2330: AllSecretSeeds; 2366-2370: Localization, _code, _sound, _plaintext, TextThatWasUsedToUnlock; WorldGen.cs:340, 412-420 | definition registry, protocol adapter and compatibility projection; not authority |
| WorldSecretSeedVisualAndSurfaceRules | WorldSecretSeedVisualAndSurfaceRulesDefinition | 2331-2335, 2340-2341, 2354, 2359: paintEverythingGray, paintEverythingNegative, coatEverythingEcho, coatEverythingIlluminant, noSurface, surfaceIsInSpace, rainsForAYear, rainbowStuff, worldIsFrozen; WorldGen.cs:342-362, 388, 398 | immutable rule references consumed by queries |
| WorldSecretSeedTerrainAndStructureRules | WorldSecretSeedTerrainAndStructureRulesDefinition | 2336-2337, 2342, 2344, 2352-2353, 2355-2358, 2365: extraLivingTrees, extraFloatingIslands, biggerAbandonedHouses, addTeleporters, noSpiderCaves, actuallyNoTraps, digExtraHoles, roundLandmasses, extraLiquid, portalGunInChests, dualDungeons; WorldGen.cs:352-410 | immutable rule references; execution belongs to generation systems in other partitions |
| WorldSecretSeedProgressionAndInfectionRules | WorldSecretSeedProgressionAndInfectionRulesDefinition | 2338-2339, 2343, 2345-2351, 2363-2364: errorWorld, graveyardBloodmoonStart, randomSpawn, startInHardmode, noInfection, hallowOnTheSurface, worldIsInfected, surfaceIsMushrooms, surfaceIsDesert, pooEverywhere, vampirism, teamBasedSpawns; WorldGen.cs:356-382, 406-408 | immutable rule references; progression/infection owners require integration review |
| WorldSecretSeedSeasonalRules | WorldSecretSeedSeasonalRulesDefinition | 2360-2362: halloweenGen, endlessHalloween, endlessChristmas; WorldGen.cs:400-404 | immutable rule references; seasonal runtime owner is outside this partition |
| WorldSecretSeedRuntimeRegistry | WorldSecretSeedRuntimeRegistryComponent | 2371-2372: activeSecretSeedCount, _enabled; 2651: Enabled; WorldGen.cs:422-426 | world-scoped authoritative runtime snapshot; legacy per-definition _enabled is compatibility state |
| WorldSecretSeedDerivedOptions | WorldSecretSeedDerivedOptionsQuery | 2652-2653: GenerateBiggerAbandonedHouses, GenerateRainbowGlowsticks; WorldGen.cs:428-444 | derived query; random source must be explicit for deterministic verification |
| WorldSecretSeedDerivedVariations | WorldSecretSeedDerivedVariationsQuery | 2629-2650: all 22 SecretSeed.Variations properties; WorldGen.cs:44-316 | pure derived query over enabled rules/count/world context |
| WorldSkyblockGenerationRules | WorldSkyblockGenerationScanComponent plus WorldSkyblockGenerationRulesQuery | 2373-2383: noAltars, noDungeon, noTemple, noHellstone, noFossils, noLifeCrystals, noHellforge, lowTiles, hasTile, hasWall, currentActiveTiles; 2654-2656: denyFloatingIslands, denyAllGeneration, denySomeGeneration; WorldGen.cs:3143-3179 | scan authority and derived query; tile/world effects require integration review |
| WorldSeedOptionCatalog | WorldSeedOptionCatalogDefinition plus WorldSeedOptionCatalogQuery | 2329: WorldSeedOption_Everything._dependencies; 2608-2628: KeyName, ServerConfigName, Dependencies for Normal, NotTheBees, Drunk, Anniversary, DontStarve, Everything, ForTheWorthy, NoTraps, Remix and Skyblock | immutable catalog and explicit selection/query boundary; legacy event/UI behavior is adapter scope |
| WorldLandmassAndTreeProfiles | WorldLandmassDefinition plus WorldTreeProfileCatalog | 2235-2238: DataType, Position, RadiusOrHalfSize, Style; 2584: Top; 2391-2407: ten profile fields and seven GrowTreeSettings fields; LandmassData.cs:7-24 and WorldGen.cs:3842-4006 | immutable definition/value data; growth predicates are explicit query seams |

## 4. Boundary and Ordering Model

~~~text
seed text / server config / generation request
        |
        v
WorldSeedOptionCatalogQuery -- selection result --> WorldGenerationSecretSeedFlagsComponent
        |                                             |
        |                                             v
        |                               WorldSecretSeedRuntimeRegistryComponent
        |                                             |
        +--> WorldSecretSeed definition catalogs ----+
                                                      |
                                                      v
                     DerivedOptionsQuery / DerivedVariationsQuery / SkyblockRulesQuery
                                                      |
                                                      v
                 generation eligibility queries and explicit tile/structure commands
~~~

The arrows are dependency direction, not file order. The scheduler must declare these barriers:

1. OptionSelection validates and commits world seed flags.
2. SecretSeedInitialization creates a world-scoped runtime snapshot and active count.
3. DefinitionCatalogReady exposes immutable definitions and option metadata.
4. DerivedRulesEvaluation evaluates pure queries using a stable snapshot and explicit random
   inputs where needed.
5. SkyblockScanCommit commits scan results, then SkyblockRuleEvaluation reads the committed scan
   without resetting it from a query.
6. Generation systems consume read-only results and submit tile/world mutations through their
   own command ports.
7. Persistence/network projections serialize snapshots only after the authority commit barrier.

No proposed Query may write a Component. No projection, adapter or client representation may be a
write owner. No runtime behavior may depend on source-file or directory enumeration order.

## 5. Component: WorldGenerationSecretSeedFlagsComponent

status: implemented-data-shape-integration-blocked.

Implementation source: `src/WorldSession/WorldGeneration/Components/WorldGenerationSecretSeedFlagsComponent.cs`.

### 5.1 Responsibility and state

This world-scoped component owns the selected standard world-generation mode flags that Version4
materializes during WorldGen.Reset. It is authoritative only for values selected for the current
world-generation session. It does not own secret-seed definitions, secret-seed enablement,
Skyblock scan arrays or derived variation values.

| Proposed field | Version4 member | Version4 source line | Default | State kind | Invariant |
| --- | --- | ---: | --- | --- | --- |
| RemixWorldGeneration | remixWorldGen | 4306 | false | authoritative input | Mirrors selected WorldSeedOption_Remix at commit time. |
| EverythingWorldGeneration | everythingWorldGen | 4308 | false | authoritative input | Mirrors selected WorldSeedOption_Everything. |
| NoTrapsWorldGeneration | noTrapsWorldGen | 4310 | false | authoritative input | Mirrors selected WorldSeedOption_NoTraps. |
| DrunkWorldGeneration | drunkWorldGen | 4312 | false | authoritative input | Mirrors selected WorldSeedOption_Drunk. |
| GetGoodWorldGeneration | getGoodWorldGen | 4314 | false | authoritative input | Mirrors selected WorldSeedOption_ForTheWorthy. |
| TenthAnniversaryWorldGeneration | tenthAnniversaryWorldGen | 4316 | false | authoritative input | Mirrors selected WorldSeedOption_Anniversary. |
| DontStarveWorldGeneration | dontStarveWorldGen | 4318 | false | authoritative input | Mirrors selected WorldSeedOption_DontStarve. |
| NotTheBeesWorld | notTheBees | 4320 | false | authoritative input | Mirrors selected WorldSeedOption_NotTheBees. |
| SkyblockWorldGeneration | skyblockWorldGen | 4322 | false | authoritative input | Mirrors selected WorldSeedOption_Skyblock; cross-partition consumer ownership is integration-review. |
| DrunkWorldGenerationText | drunkWorldGenText | 4324 | false | compatibility/presentation input | Derived from DrunkWorldGeneration; it is not an independent user setting or constructor input. |

The component attaches to the world-generation aggregate/entity, not to player, NPC or projectile
entities. It has no persistent entity ID of its own. World identity and seed are supplied by the
existing WorldSeedComponent; this component must not duplicate the numeric seed.

### 5.2 Interface and seam

~~~text
IWorldSeedOptionSelectionQuery
  input: seed text, server flags, catalog definitions
  output: WorldGenerationSecretSeedFlagsCandidate

IWorldGenerationSecretSeedFlagsCommitPort
  input: validated candidate, world-generation session/version
  effect: replace the single world-scoped component value

IWorldGenerationSecretSeedFlagsProjection
  input: committed component
  output: legacy Main/WorldGen compatibility view or persistence/network snapshot
  effect: one-way output; never writes the component
~~~

The commit port is the sole writer. Reads use an immutable value or read-only snapshot. A failed
selection does not partially mutate flags. Repeated commits require an explicit generation version
or idempotency rule; the proposed default is replacement by world-generation session, with no
implicit merge.

### 5.3 Current NLTX mapping and risk

WorldRuleSnapshotComponent already carries IsRemixWorld, IsEverythingWorld, IsNoTrapsWorld,
IsDrunkWorld, IsGoodWorld, IsDontStarveWorld, IsNotTheBeesWorld, IsSkyblockWorld and
IsTenthAnniversaryWorld (WorldRuleSnapshotComponent.cs:5-90). WorldGenerationRequest also exposes
request-level flags (WorldGenerationRequest.cs:53-278). This is a partial existing mapping, not
proof of a dedicated P18 owner. The implementation plan must first decide whether the new component
replaces those fields, becomes their canonical source or remains a compatibility projection. Until
that decision, dual writes are forbidden.

### 5.4 Verification design

The focused verifier must prove:

- all ten report members map exactly once;
- default construction produces ten false flags;
- option selection maps each option to the expected flag and rejects unknown options without a
  partial commit;
- DrunkWorldGenerationText follows the selected drunk flag and cannot diverge through a second
  write path;
- replacement is scoped to one world-generation session and does not leak into a second world;
- the projection is one-way and does not mutate the component;
- cross-partition consumers read the committed snapshot after the explicit barrier.

This verifier has not been run: verificationStatus: not-run.

## 6. Completed Component: WorldSecretSeedRegistryDefinitionsProjection

status: implemented-unverified.

### 6.1 Responsibility and split

The Version4 SecretSeed type combines immutable definition data, mutable per-input fields,
runtime enablement, registry mutation and sound playback. This checkpoint separates only the
definition/registry and input boundary; runtime enablement is deferred to
WorldSecretSeedRuntimeRegistryComponent.

WorldSecretSeedRegistryDefinitionsProjection is a read-only world-generation catalog. Its
proposed definition record contains a stable variant/key, localization key and opaque secret-code
value. It must not expose LegacySoundStyle, Main, WorldGen, mutable lists or setters to domain
queries. AllSecretSeeds becomes a one-way projection of this catalog, not an authority.

WorldSecretSeedInputAdapter owns external input behavior:

- normalize raw world-seed text using the Version4 case and non-alphanumeric rules;
- compare normalized plaintext and transformed code against the immutable catalog;
- return a match result containing the selected variant and normalized input;
- preserve original unlock text as an explicit result/audit value;
- resolve and play the legacy sound only at the adapter/effect boundary, if requested.

The adapter may submit an explicit enable command, but it does not mutate the catalog or hidden
static fields. Rejected input returns a stable negative result and leaves all state unchanged.

### 6.2 Member ownership map

| Version4 member | Proposed destination | State kind | Read/write and lifecycle |
| --- | --- | --- | --- |
| AllSecretSeeds | WorldSecretSeedRegistryDefinitionsProjection.Definitions | immutable catalog projection | Built once for a catalog version; readers enumerate a read-only snapshot; no runtime writer. |
| Localization | WorldSecretSeedDefinition.LocalizationKey | immutable definition | Read by localization/UI adapter; set at catalog construction. |
| _code | WorldSecretSeedDefinition.OpaqueCode | immutable definition credential/value | Read only by input adapter for matching; never exposed to unrelated queries or logs. |
| _sound | WorldSecretSeedInputAdapter sound-resolution port | external adapter dependency | Read only when an enable request explicitly requests feedback; sound is not definition authority. |
| _plaintext | WorldSecretSeedInputMatch.NormalizedPlaintext | transient input/compatibility value | Written by the adapter for a successful match; scoped to the input result, not the catalog. |
| TextThatWasUsedToUnlock | WorldSecretSeedInputMatch.OriginalUnlockText | transient audit/compatibility value | Written by the adapter after successful match; persistence is integration-review and explicit. |

### 6.3 Interface and seam

~~~text
IWorldSecretSeedRegistryDefinitionsProjection
  output: IReadOnlyList<WorldSecretSeedDefinition>
  guarantee: stable order, immutable definition values, no write-back

IWorldSecretSeedInputAdapter
  input: raw seed text, definition projection, optional sound port
  output: match/rejection result or enable command
  effects: optional sound and explicit command submission only
~~~

The source Register operation is replaced by catalog construction before world generation.
Registration order must remain the report/Version4 order because callers may observe enumeration,
but order is an explicit catalog contract rather than a static-field side effect. ClearAllSeeds is
not implemented here; it is a runtime-registry command in the next runtime-state boundary.

### 6.4 Current NLTX mapping and risks

SecretSeedDefinitionRegistry already exposes an immutable list of 35 variant/source anchors
(SecretSeedDefinitionRegistry.cs:6-62), and SecretSeedRuleSnapshotQuery copies definition
values into read-only rule snapshots (SecretSeedRuleSnapshotQuery.cs:29-46). This is a useful
partial mapping, but the current definition record does not carry the Version4 localization,
opaque code, sound boundary or unlock-input result. It must be extended or wrapped only after
the canonical owner decision; a second registry with diverging order is prohibited.

Key risks are secret-code exposure, accidental mutable collection leakage and treating input
audit text as persisted world authority. The adapter must also specify whether normalization and
code transformation are deterministic and whether malformed input consumes external/random state.
Version4 evidence confirms normalization and comparison, but not persistence or network semantics.

### 6.5 Verification design

- compare projected catalog count/order and all 35 Version4 definition anchors;
- verify successful plaintext and transformed-code matches, case/punctuation normalization and
  rejection without mutation;
- verify OriginalUnlockText preserves the compatibility value without changing the catalog;
- verify optional sound is invoked only through the adapter port and not by queries;
- verify returned collections and definition values cannot be mutated by callers;
- verify no runtime enable/disable operation changes the definition projection.

This verifier has not been run: verificationStatus: not-run.

## 7. Completed Component: WorldSecretSeedVisualAndSurfaceRulesDefinition

status: implemented-unverified.

### 7.1 Responsibility and state

This boundary owns the immutable rule-role mapping for the nine visual and surface secret seeds.
It does not own whether a rule is enabled. Each proposed field is a stable reference/key into
WorldSecretSeedRegistryDefinitionsProjection; enabled state is read from
WorldSecretSeedRuntimeRegistryComponent after that component is implemented.

| Proposed rule reference | Version4 member | Rule domain | Lifecycle and invariant |
| --- | --- | --- | --- |
| PaintEverythingGray | paintEverythingGray | world visual/coating | Immutable catalog reference; no query writes its enabled state. |
| PaintEverythingNegative | paintEverythingNegative | world visual/coating | Same definition version as the registry catalog. |
| CoatEverythingEcho | coatEverythingEcho | world coating | Consumed by variation query; must not carry an enabled boolean copy. |
| CoatEverythingIlluminant | coatEverythingIlluminant | world coating | Consumed by variation query; no direct tile writes. |
| NoSurface | noSurface | surface generation | Read by surface/structure eligibility queries; execution remains outside P18. |
| SurfaceIsInSpace | surfaceIsInSpace | surface generation | Definition only; consumer determines world-generation phase. |
| RainsForAYear | rainsForAYear | weather/surface setup | Definition only; weather owner is integration-review. |
| RainbowStuff | rainbowStuff | visual/content generation | Definition only; derived glowstick query reads the runtime snapshot. |
| WorldIsFrozen | worldIsFrozen | surface/environment | Definition only; Skyblock and variation consumers read explicit runtime state. |

This is intentionally a small rule-reference definition, not a nine-boolean component. Boolean
copies would create a second authority and could diverge from the enabled variant set.

### 7.2 Interface and access contract

~~~text
WorldSecretSeedVisualAndSurfaceRulesDefinition
  input: immutable registry definition projection
  output: nine stable rule references

WorldSecretSeedVisualAndSurfaceRulesQuery
  input: definition references, runtime enabled-set snapshot, world context
  output: read-only eligibility/result values
  effect: none
~~~

The definition is created at catalog initialization. The query may be evaluated repeatedly and
must be deterministic for the same snapshots. SurfaceIsInSpace, RainsForAYear and WorldIsFrozen
may have consumers in P16/P17 or other world-generation partitions; their final owner is
integration-review, not this component.

### 7.3 Current NLTX mapping and risks

The current SecretSeedDefinitionRegistry contains the corresponding variant anchors and
SecretSeedVariationQuery already evaluates the paint/coating/no-surface subset from explicit
enabled variants (SecretSeedVariationQuery.cs:6-90). This supports the proposed read-only
boundary, but it does not prove that all nine rules are consumed by the current generation
pipeline. No direct persistence or network evidence was found for these definition references.

The main behavior risk is confusing a rule definition with its enabled runtime state. The second
risk is moving WorldIsFrozen or weather/surface effects into P18 when another partition owns the
world progression or environmental state. Such consumers must receive a query result or explicit
event/command and remain marked integration-review.

### 7.4 Verification design

- verify all nine references resolve to unique registry definitions;
- verify a definition catalog is stable across repeated construction and cannot be mutated;
- verify enabled-set changes affect query results only through the runtime snapshot;
- verify query calls have no writes, random reads or external effects;
- verify all 22 variation formulas that consume these references retain the expected truth table;
- verify cross-partition consumers use read-only results and cannot replace the registry definition.

This verifier has not been run: verificationStatus: not-run.

## 8. Completed Component: WorldSecretSeedTerrainAndStructureRulesDefinition

status: implemented-unverified.

### 8.1 Responsibility and state

This definition groups the eleven secret-seed references that alter terrain, structures, resource
availability or generation-side content. It owns rule metadata only. It does not own tile arrays,
liquid state, dungeon placement, chest contents or generation pass execution.

| Proposed rule reference | Version4 member | Consumer seam | Cross-subsystem owner |
| --- | --- | --- | --- |
| ExtraLivingTrees | extraLivingTrees | tree/vegetation eligibility query | P19/P20 integration-review |
| ExtraFloatingIslands | extraFloatingIslands | Skyblock/landmass eligibility query | Skyblock and terrain integration-review |
| BiggerAbandonedHouses | biggerAbandonedHouses | derived house-size query | generation execution integration-review |
| AddTeleporters | addTeleporters | structure placement command/query | P20 or structure owner integration-review |
| NoSpiderCaves | noSpiderCaves | cave eligibility/variation query | P19 integration-review |
| ActuallyNoTraps | actuallyNoTraps | trap eligibility/variation query | P20 integration-review |
| DigExtraHoles | digExtraHoles | terrain carving query | P19 integration-review |
| RoundLandmasses | roundLandmasses | landmass shape query | landmass/generation integration-review |
| ExtraLiquid | extraLiquid | liquid generation query | P01/P19 integration-review |
| PortalGunInChests | portalGunInChests | chest content query | P09/P20 integration-review |
| DualDungeons | dualDungeons | dungeon placement query | P19/P20 integration-review |

References are stable keys into the registry projection. No rule reference stores a second
enabled boolean or mutable consumer cache. A consumer may derive an eligibility result, but the
result is not authority unless a separate owner explicitly commits it.

### 8.2 Interface and effect boundary

~~~text
WorldSecretSeedTerrainAndStructureRulesDefinition
  output: eleven immutable rule references

WorldSecretSeedTerrainAndStructureRuleQuery
  input: definition, enabled-set snapshot, world-generation context
  output: eligibility or command intent
  effect: none; command execution belongs to the consumer system
~~~

ExtraLiquid must be passed to a liquid-generation seam rather than toggling a liquid solver
directly. PortalGunInChests must produce a chest-content decision or command and must not write
inventory state from a Query. AddTeleporters, DualDungeons, RoundLandmasses and
ExtraFloatingIslands require explicit scheduling/ordering contracts with the generation owners.

### 8.3 Current NLTX mapping and risks

Current NLTX has registry anchors for these variants, but no complete dedicated definition or
consumer manifest. Existing world-generation code already passes Skyblock context through explicit
requests, and its tree/generation policies are capability-local. This supports the proposed
read-only rule boundary but leaves execution coverage partial.

The largest risks are hidden cross-domain writes and accidental rule duplication: a Skyblock
policy must read ExtraFloatingIslands from the same runtime snapshot as the variation query, and
the liquid/chest/dungeon owners must not each invent their own static flag. P01, P09, P19 and P20
must approve the handoff before implementation.

### 8.4 Verification design

- resolve all eleven references against the registry projection exactly once;
- verify each enabled flag changes only its intended query/command result;
- verify no definition or query writes tiles, liquids, chests, dungeons or teleporter state;
- verify ExtraFloatingIslands has identical input to Skyblock policy and variation queries;
- verify command intents are explicit, ordered and idempotency/retry semantics are documented;
- verify consumers in P01, P09, P19 and P20 cannot mutate the definition or runtime registry.

This verifier has not been run: verificationStatus: not-run.

## 9. Completed Component: WorldSecretSeedProgressionAndInfectionRulesDefinition

status: implemented-unverified.

### 9.1 Responsibility and state

This definition groups twelve rule references whose consumers affect world progression, infection,
surface composition, spawning or player-facing world modes. It is a catalog boundary only. It
does not become the owner of hardmode, infection, biome, spawn, vampirism or team state.

| Proposed rule reference | Version4 member | Primary consumer seam | crossSubsystemOwner |
| --- | --- | --- | --- |
| ErrorWorld | errorWorld | error-world generation query | P19/P20 integration-review |
| GraveyardBloodmoonStart | graveyardBloodmoonStart | event/progression eligibility query | P16 integration-review |
| RandomSpawn | randomSpawn | spawn-point selection query | P14 integration-review |
| StartInHardmode | startInHardmode | progression initialization command | P16 integration-review |
| NoInfection | noInfection | infection transition query | P16/P17 integration-review |
| HallowOnTheSurface | hallowOnTheSurface | surface biome query | P17 integration-review |
| WorldIsInfected | worldIsInfected | infection/world-rule projection | P16/P17 integration-review |
| SurfaceIsMushrooms | surfaceIsMushrooms | surface composition query | P17/P19 integration-review |
| SurfaceIsDesert | surfaceIsDesert | surface composition query | P17/P19 integration-review |
| PooEverywhere | pooEverywhere | terrain decoration query | P19/P20 integration-review |
| Vampirism | vampirism | player/combat rule projection | P06/P12 integration-review |
| TeamBasedSpawns | teamBasedSpawns | spawn/network rule projection | P07/P12/P14 integration-review |

Each field is an immutable rule reference. The selected rule set is read through the runtime
registry snapshot. A consumer that needs a persistent outcome must submit an explicit command to
its owning component; it must not write a field in this definition.

### 9.2 Boundary contract

~~~text
WorldSecretSeedProgressionAndInfectionRulesDefinition
  output: twelve immutable rule references

WorldSecretSeedProgressionAndInfectionRuleQuery
  input: rule references, enabled-set snapshot, world/progression context
  output: eligibility, initialization intent or projection value
  effect: none
~~~

The StartInHardmode result is an initialization intent, not a second hardmode authority.
NoInfection, WorldIsInfected, HallowOnTheSurface, SurfaceIsMushrooms and SurfaceIsDesert must be
resolved with the world progression/ecology owners. Vampirism and TeamBasedSpawns may be
networked/player-visible and therefore need an explicit replication decision. The query remains
pure and can return a reasoned result for unsupported combinations.

### 9.3 Current NLTX mapping and risks

Current WorldRuleSnapshotComponent already carries NoInfection, StartInHardmode,
IsSurfaceDesertWorld and related world flags (WorldRuleSnapshotComponent.cs:5-90), while
LegacyStartInHardmodePolicy.cs:1-20 applies a policy by returning a new snapshot. This is
partial evidence for explicit input/output calculation, not proof that the twelve P18 references
have a single owner. No current NLTX evidence was found for the full spawn/network/vampirism
projection.

The main risk is silently moving progression authority into a seed-definition catalog. The second
is semantic overlap between the seed rule and a post-generation world fact. Keep those distinct:
the definition says which rule was selected; the owning subsystem decides and persists the world
fact.

### 9.4 Verification design

- resolve all twelve references uniquely and preserve their selected variant identity;
- verify seed-rule queries return intents/results without mutating progression, infection, biome,
  spawn, player or network state;
- verify StartInHardmode composes with existing hardmode policy without duplicate writes;
- verify infection/surface combinations are deterministic and explicitly reject unsupported mixes;
- verify Vampirism and TeamBasedSpawns outputs have an explicit replication seam;
- verify cross-partition handoffs are read-only until an owner command commits them.

This verifier has not been run: verificationStatus: not-run.

## 10. Completed Component: WorldSecretSeedSeasonalRulesDefinition

status: implemented-unverified.

### 10.1 Responsibility and state

This definition owns the three seasonal secret-seed references:

| Proposed rule reference | Version4 member | State kind | Effect owner |
| --- | --- | --- | --- |
| HalloweenGeneration | halloweenGen | immutable selected-rule definition | seasonal/world-generation system, integration-review |
| EndlessHalloween | endlessHalloween | immutable selected-rule definition | event/calendar system, integration-review |
| EndlessChristmas | endlessChristmas | immutable selected-rule definition | event/calendar system, integration-review |

The definition contains no current date, event timer, weather state, particle/audio effect or
client presentation. Enabled state is obtained from the runtime registry snapshot. If a seasonal
system needs to persist an active event, it commits its own world-calendar state through an
explicit command.

### 10.2 Interface and verification

~~~text
WorldSecretSeedSeasonalRulesDefinition
  output: three stable rule references

WorldSecretSeedSeasonalRuleQuery
  input: definition, enabled-set snapshot, calendar context
  output: seasonal eligibility/result
  effect: none
~~~

The query must be deterministic for a fixed snapshot and calendar context. It must not obtain
system time or publish an event directly. A calendar adapter may provide an explicit time/context
value and a separate seasonal system owns effect ordering, persistence, replication and retry.

Current NLTX has the seasonal variants in SecretSeedDefinitionRegistry but no inspected
season-specific owner or persistence mapping. This is partial evidence. Verification must cover
the three variants, empty selection, simultaneous selection and one-way handoff to the calendar
owner.

This verifier has not been run: verificationStatus: not-run.

## 11. Completed Component: WorldSecretSeedRuntimeRegistryComponent

status: implemented-unverified.

### 11.1 Responsibility and authoritative state

This component is the single world-scoped owner for the enabled secret-seed set and its derived
active count. It replaces the authority role of Version4 SecretSeed._enabled and static
activeSecretSeedCount, while preserving a compatibility projection for legacy readers during a
bounded migration window.

Proposed state:

| Field | Type shape | Default | State kind | Invariant |
| --- | --- | --- | --- | --- |
| EnabledVariants | immutable/read-only set of stable variant keys | empty | authoritative world state | No duplicate keys; every key resolves in the immutable registry catalog. |
| ActiveSecretSeedCount | int, derived from EnabledVariants.Count | 0 | derived snapshot value | Never independently written; equals the enabled-set cardinality. |
| RuntimeVersion | monotonic generation/session version | explicit initial version | lifecycle/compatibility | A snapshot belongs to one world-generation session. |

The component attaches to the world-generation aggregate/entity. It must be reset when a new world
generation session starts and disposed/invalidated when that session ends. It must not be process
global, shared across worlds or attached to a player entity.

### 11.2 Commit and read seams

~~~text
WorldSecretSeedRuntimeRegistryCommitSystem
  input: EnableSecretSeedCommand, DisableSecretSeedCommand, ClearSecretSeedsCommand
  reads: immutable definition projection and current component
  writes: replacement component snapshot only

WorldSecretSeedRuntimeRegistryQuery
  input: component snapshot
  output: read-only enabled-set/count view
  effect: none

WorldSecretSeedRuntimeProjection
  input: component snapshot
  output: legacy SecretSeed.Enabled and active-count compatibility view
  effect: one-way projection; never authoritative
~~~

Enable is idempotent for an already-enabled variant, Disable is idempotent for an absent variant,
and Clear produces an empty set. Commands must include the world/session version so a late command
from an old generation cannot mutate a new world. The commit system is the only writer. It may
return a rejected result for an unknown variant, stale version, duplicate sequence or invalid
lifecycle state; it must not partially apply a batch.

Sound playback, logging, network publication and persistence are explicit adapters after commit.
The commit system does not assume exactly-once delivery. A command sequence or idempotency key is
required if commands can cross a queue or retry boundary.

### 11.3 Member map and compatibility

| Version4 member | Proposed owner | Compatibility handling |
| --- | --- | --- |
| activeSecretSeedCount | EnabledVariants.Count | Expose a read-only projection only; prohibit a second counter writer. |
| _enabled | membership of the world-scoped enabled set | Legacy instance property is a compatibility view keyed by variant; it is not stored independently. |
| Enabled | EnabledVariants.Contains(variant) | Read-only query result; no per-definition setter. |

Version4 Enable/Disable methods become command creation plus commit, while ClearAllSeeds becomes
ClearSecretSeedsCommand. InitializeSecretSeeds becomes a projection/initialization step that
writes downstream compatibility or world-rule owners through explicit commands. It is not allowed
to hide mutations inside a read query.

### 11.4 Current NLTX mapping and risks

SecretSeedRuleSnapshotSet already computes ActiveSecretSeedCount from a list of
SecretSeedRuleSnapshot values (SecretSeedRuleSnapshotQuery.cs:6-24), and its query constructs the
set from explicit enabled variants (SecretSeedRuleSnapshotQuery.cs:29-46). This is a useful
read-side partial mapping. It is not yet a mutable world component or a command/commit owner.

The key risk is replacing process-global state with a mutable singleton that still leaks across
worlds. Another is maintaining both a set and a manually writable count. Only the set is
authoritative; count is derived. Persistence/network serialization must identify variants and
runtime version, not serialize third-party SecretSeed instances.

### 11.5 Verification design

- empty, enable, duplicate-enable, disable, duplicate-disable and clear transitions;
- active count equals set cardinality after every accepted transition;
- unknown variant, stale version and invalid lifecycle reject without partial state;
- two concurrent world sessions cannot observe or mutate one another's enabled sets;
- old compatibility projections reflect the snapshot but cannot write it back;
- retrying the same command with the same idempotency key has the specified result;
- persistence/network adapters serialize stable variant keys and version only after commit.

This verifier has not been run: verificationStatus: not-run.

### 11.6 Implementation checkpoint

The runtime registry boundary is implemented in the root `src` project. The component stores only
the frozen enabled-variant set as mutable authority; `ActiveSecretSeedCount` is derived from that
set and cannot be assigned independently. Enable, disable and clear commands carry the generation
identity, runtime version and idempotency key. The commit system validates lifecycle and version
before writing, rejects unknown variants and idempotency-key conflicts without partial mutation,
and treats repeated enable/disable/clear operations as idempotent. Reset and close operations clear
the enabled set and reset command history at the world-generation boundary.

Actual source files:

- `src/WorldSession/WorldGeneration/Components/WorldSecretSeedRuntimeRegistryComponent.cs`
- `src/WorldSession/WorldGeneration/Components/WorldSecretSeedRuntimeRegistryLifecycle.cs`
- `src/WorldSession/WorldGeneration/Components/WorldSecretSeedRuntimeRegistrySnapshot.cs`
- `src/WorldSession/WorldGeneration/Actions/EnableSecretSeedCommand.cs`
- `src/WorldSession/WorldGeneration/Actions/DisableSecretSeedCommand.cs`
- `src/WorldSession/WorldGeneration/Actions/ClearSecretSeedsCommand.cs`
- `src/WorldSession/WorldGeneration/Systems/WorldSecretSeedRuntimeRegistryCommitStatus.cs`
- `src/WorldSession/WorldGeneration/Systems/WorldSecretSeedRuntimeRegistryCommitResult.cs`
- `src/WorldSession/WorldGeneration/Systems/WorldSecretSeedRuntimeRegistryCommitSystem.cs`
- `src/WorldSession/WorldGeneration/Queries/WorldSecretSeedRuntimeRegistryQuery.cs`
- `src/WorldSession/WorldGeneration/Adapters/WorldSecretSeedRuntimeProjection.cs`
- `src/WorldSession/WorldGeneration/Adapters/WorldSecretSeedRuntimeRegistrySerializedState.cs`
- `src/WorldSession/WorldGeneration/Adapters/WorldSecretSeedRuntimeRegistrySerializationAdapter.cs`

Dependency impact: the new boundary reads the existing immutable secret-seed definition
projection, but does not modify `WorldRulesState`, `WorldRulesSnapshotValue`, legacy static seed
state, sound, logging, persistence or network writers. The projection and serialization adapter
are one-way read boundaries. VerificationStatus remains `not-run`; compilation, focused tests and
legacy behavior comparison are still pending.

## 12. Completed Component: WorldSecretSeedDerivedOptionsQuery

status: implemented-unverified.

### 12.1 Responsibility and inputs

This Query owns only the two derived option decisions in the report. It does not own enabled
secret-seed state, world flags, random state or presentation. Its input is an immutable snapshot:

| Input | Version4 dependency | Meaning |
| --- | --- | --- |
| BiggerAbandonedHousesEnabled | biggerAbandonedHouses.Enabled | Explicit runtime registry membership. |
| ErrorWorldEnabled | errorWorld.Enabled | Explicit runtime registry membership. |
| BiggerAbandonedHousesRandomRoll | genRand.Next(3) | A supplied integer in 0..2 when the fallback branch is evaluated. |
| RainbowStuffEnabled | rainbowStuff.Enabled | Explicit runtime registry membership. |
| TenthAnniversaryWorld | Main.tenthAnniversaryWorld | World-generation flags snapshot, not a global read. |

The output contains GenerateBiggerAbandonedHouses and GenerateRainbowGlowsticks. The random roll
is required only when BiggerAbandonedHouses is disabled and ErrorWorld is enabled; the adapter
that owns the random stream must still account for the Version4 draw at that branch. The Query
must reject a roll outside 0..2 and must not itself obtain or advance a random source.

### 12.2 Preserved behavior

~~~text
GenerateBiggerAbandonedHouses =
  BiggerAbandonedHousesEnabled
  OR (ErrorWorldEnabled AND BiggerAbandonedHousesRandomRoll == 0)

GenerateRainbowGlowsticks =
  RainbowStuffEnabled OR TenthAnniversaryWorld
~~~

The second expression preserves the Version4 fallback to the anniversary world. A pure Query
receives this context as a value and cannot read Main. Randomness, draw accounting and any
instrumentation belong to an explicit random adapter. The output is a derived result, not a
persisted authority.

### 12.3 Current NLTX mapping and risks

No dedicated current NLTX derived-options Query was found in the inspected WorldGeneration
directory. Existing secret-seed snapshots and variation queries provide the input pattern, but
these two outputs are not yet represented. This is a missing implementation mapping with
confirmed Version4 source semantics.

The primary risk is moving genRand access into a nominally pure query and thereby changing random
stream position. A second risk is reading anniversary state from the wrong world snapshot. The
execution plan must add a verifier that records both output and random-draw count.

### 12.4 Verification design

- test all four truth combinations for BiggerAbandonedHousesEnabled and ErrorWorldEnabled with
  rolls 0, 1 and 2;
- reject invalid rolls without an output or state mutation;
- verify the random adapter advances exactly once only for the Version4 fallback branch;
- test RainbowStuffEnabled and TenthAnniversaryWorld combinations;
- verify repeated evaluation with the same explicit input is deterministic and has no side effect;
- verify no Query path reads Main, WorldGen, system time, I/O or a global random source.

This verifier has not been run: verificationStatus: not-run.

### 12.5 Implementation checkpoint

The pure derived-options boundary is implemented with an explicit input value and output value.
The query preserves the Version4 formulas for bigger abandoned houses and rainbow glowsticks. It
requires a caller-supplied roll only for the error-world fallback branch, validates the roll as
`0..2`, and performs no random, global-world, I/O, logging or event access.

Actual source files:

- `src/WorldSession/WorldGeneration/Definitions/WorldSecretSeedDerivedOptionsInput.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldSecretSeedDerivedOptionsSelection.cs`
- `src/WorldSession/WorldGeneration/Queries/WorldSecretSeedDerivedOptionsQuery.cs`

Dependency impact: the query has no component writes and does not connect to `Main`, `WorldGen`,
the runtime random source, persistence, UI or event callbacks. The random adapter and any draw
accounting remain outside this component. VerificationStatus remains `not-run`; focused truth-table
tests and build evidence are pending.

## 13. Completed Component: WorldSecretSeedDerivedVariationsQuery

status: implemented-unverified.

### 13.1 Responsibility and input ownership

This pure Query preserves all 22 Version4 SecretSeed.Variations properties. It accepts the
immutable WorldSecretSeedRuntimeRegistryComponent snapshot, the nine visual/surface rule
definitions, the terrain/structure and progression rule definitions needed by the formulas, and
an explicit world context containing Skyblock state. ActiveSecretSeedCount is read from the
runtime snapshot and cannot be supplied independently.

The output is a read-only value with these 22 members:

| Output group | Version4 properties |
| --- | --- |
| Paint gray | paintEverythingGrayJustTheSurface, paintEverythingGrayJustTreasure, paintEverythingGrayUseWhite |
| Paint negative | paintEverythingNegativeJustUnderground, paintEverythingNegativeJustSomeThings |
| Echo/illuminant coating | coatEverythingJustInnerBlocks, coatEverythingEchoJustSomeThings, coatEverythingIlluminantJustRandomSpots, coatEverythingIlluminantJustSomeThings |
| No-surface | noSurfaceNoFloatingIslands, noSurfaceNoLivingTrees, noSurfaceNoPyramids, noSurfaceNoSwordShrines |
| Extra content | extraLivingTreesReducedAmount, extraFloatingIslandsNormalAmount, extraFloatingIslandsReducedAmount |
| Error/spider/trap | errorWorldBalancedChests, noSpiderCavesActuallyNoSpiderCaves, noSpiderCavesILiedMoreSpiderCaves, actuallyNoTrapsForRealIMeanIt |
| Desert | surfaceIsDesertNormalFunction, surfaceIsDesertSwapDesertAndSnowBiomes |

### 13.2 Formula and side-effect contract

The Query preserves the Version4 thresholds and conflicts:

- counts 3, 4 and 6 remain semantic boundaries for coating, limited content and balancing;
- gray/negative/echo/illuminant combinations retain their priority conditions;
- no-surface results continue to be suppressed by ErrorWorld or enabled extra content;
- extra-floating-island normal amount requires Skyblock context;
- desert normal/swap results depend on the no-surface combination;
- no output writes rule enablement, tiles, biomes, world progression or presentation state.

The Query must not read Main, WorldGen, genRand, system time, a global registry, files, network or
logging. Any later cache must declare ownership, invalidation on runtime snapshot/version change,
and deterministic replacement semantics.

### 13.3 Current NLTX mapping and risks

Current SecretSeedVariationQuery and SecretSeedVariationSnapshot already expose the same 22
boolean outputs plus ActiveSecretSeedCount (SecretSeedVariationQuery.cs:6-90 and
SecretSeedVariationSnapshot.cs:3-34). This is strong existing calculation evidence, but the API
currently accepts enabled variants and count as separate arguments. The proposed boundary changes
the source of truth to the runtime snapshot and requires the count invariant from checkpoint 7.

The main risk is formula drift while changing the input shape. The second is using a snapshot from
one world with definitions or Skyblock context from another. Every result must carry or be checked
against a common runtime/version identity.

### 13.4 Verification design

- truth-table all 22 outputs at active counts 0, 1, 3, 4 and 6;
- cover conflicting visual rules, no-surface plus extra content, spider/trap thresholds and both
  desert outcomes;
- verify extra-floating-island normal amount with Skyblock true and false;
- verify count is derived from the enabled set and inconsistent external count input is impossible;
- verify same snapshot/context produces equal output and no side effects;
- compare the proposed result with the current NLTX query across a generated fixture matrix before
  changing its call sites.

This verifier has not been run: verificationStatus: not-run.

## 14. Completed Component: WorldSkyblockGenerationRulesDefinition

status: implemented-unverified.

### 14.1 Responsibility and member ownership

This boundary separates the transient Skyblock tile/wall scan from the pure rule and policy
results. It covers all 11 fields and three properties in the report. The proposed names below are
domain names; they do not claim that these C# files exist yet.

| Version4 member group | Proposed owner | Ownership rule |
| --- | --- | --- |
| `hasTile`, `hasWall`, `currentActiveTiles` | `WorldSkyblockGenerationScanComponent` | Mutable only during one explicit scan window; arrays and count are cleared by the scan/commit system at declared boundaries. |
| `noAltars`, `noDungeon`, `noTemple`, `noHellstone`, `noFossils`, `noLifeCrystals`, `noHellforge`, `lowTiles` | `WorldSkyblockGenerationRulesQuery` output, optionally committed as a versioned snapshot | Derived from one immutable scan snapshot and world dimensions; no independent writers or cached booleans without invalidation. |
| `denyFloatingIslands`, `denyAllGeneration`, `denySomeGeneration` | `SkyblockPolicyQuery` output | Derived from the standard Skyblock generation flag and one immutable secret-seed runtime snapshot. |

The scan component is attached to the world-generation aggregate and is not a player, NPC or
tile entity component. Its public read view must expose immutable tile/wall presence and the active
tile count for one scan version. It must not expose mutable arrays to a caller.

### 14.2 Preserved Version4 rules

The rules query must preserve the exact observed tests:

- `noDungeon` becomes false when any present tile is classified by `Main.tileDungeon` or any
  present wall is classified by `Main.wallDungeon`.
- `noAltars`, `noTemple`, `noHellstone`, `noFossils`, `noLifeCrystals` use tile types 26, 226,
  58, 404 and 12 respectively.
- `noTemple` also becomes false for wall type 87.
- `noHellforge` becomes false for tile types 77 or 133.
- `lowTiles` is true only when the world is Skyblock and
  `currentActiveTiles / (maxTilesX * maxTilesY) < 0.1f`. The zero-area input contract must be
  decided before implementation rather than hidden in a global read.

`ScanTiles` also has an explicit spatial contract: it visits x in `[40, maxTilesX - 40)` and y in
`[40, maxTilesY - 40)`, increments the active count only for active tiles, records active tile
types and records wall types for every visited tile. That boundary belongs to the scan system and
must be represented by an injected world-grid reader.

The policy query preserves the three property formulas. `denyAllGeneration` is the Skyblock flag;
`denyFloatingIslands` is true for Skyblock unless `extraFloatingIslands` is enabled;
`denySomeGeneration` is false when any of `worldIsFrozen`, `surfaceIsDesert`,
`surfaceIsMushrooms`, `worldIsInfected`, `hallowOnTheSurface`, `noInfection`,
`extraFloatingIslands`, `extraLiquid` or `extraLivingTrees` is enabled, and otherwise follows the
Skyblock exception rule. Non-Skyblock worlds return false for all three properties.

### 14.3 Scan, query and effect seams

The runtime sequence is explicit:

1. `WorldSkyblockGenerationScanSystem` reads a world-grid snapshot through a tile/wall reader and
   commits the scan facts to `WorldSkyblockGenerationScanComponent`.
2. `WorldSkyblockGenerationRulesQuery` evaluates the no-content and low-tile result from that
   snapshot. It does not clear arrays, mutate world coordinates, send network messages or write
   tiles.
3. `WorldSkyblockGenerationRulesCommitSystem` may publish a versioned result snapshot and owns
   the post-evaluation reset of the transient scan state.
4. An explicit `SkyblockGenerationEffectsPort` handles the observed `Main.dungeonX/Y = -1` action
   when no dungeon is present and the `NetMessage.SendData(7)` action when `lowTiles` changes.

The policy query receives `SkyblockWorldGeneration` from the generation flags component and
secret-seed enablement from the runtime registry snapshot. It cannot inspect `Main`, `WorldGen`,
the tile grid, network state or a mutable global collection. Any persistence or replication reads
the committed result after the authority barrier.

### 14.4 Current NLTX mapping and risk

`SkyblockRuleQuery` already calculates the eight no-content/low-tile outputs using explicit scan
sets, dungeon sets and world dimensions. `SkyblockPolicyQuery` already calculates the three deny
outputs from explicit Skyblock and enabled-variant inputs. `WorldGenerationRequest` supplies a
Skyblock flag to the pipeline. These are partial mappings: the scan component, Version4 scan
bounds, reset timing, exact `Main.tileDungeon`/`Main.wallDungeon` classification, dungeon-coordinate
effect and low-tile network notification are not represented by one canonical owner.

P19/P20 must consume the result snapshot or query output and submit generation commands; they may
not write the scan component or compatibility fields. No new canonical writer is authorized until
the existing `SkyblockRuleQuery`/`SkyblockPolicyQuery` call sites and the world-generation scheduler
barrier are resolved.

### 14.5 Verification design

- map all 11 fields and three properties exactly once and verify the scan/result ownership split;
- verify the 40-tile scan border, active-only tile counting and all-wall recording;
- verify tile types 12, 26, 58, 77, 133, 226 and 404, wall types 87 and dungeon classifications;
- verify the strict 10% threshold, Skyblock gating, reset timing and zero-area decision;
- verify all deny-property exception combinations against one immutable secret-seed snapshot;
- verify a pure query produces no array reset, tile change, dungeon coordinate write, network call or
  logger call;
- compare the proposed outputs with current `SkyblockRuleQuery` and `SkyblockPolicyQuery` across
  a generated scan and enabled-variant matrix before changing call sites.

This verifier has not been run: verificationStatus: not-run.

### 14.6 Implementation checkpoint

The Skyblock boundary is implemented as an explicit scan component, injected grid reader and
versioned scan snapshot, plus pure rules/policy queries and a commit system. Scan writes are allowed
only between `BeginScan` and `ResetAfterCommit`; tile presence is recorded only for active tiles,
wall presence is recorded for every visited tile, and the scan system preserves the Version4
`[40, maxTilesX - 40)` / `[40, maxTilesY - 40)` traversal. Rules preserve the listed tile/wall
IDs, dungeon classification, strict `< 0.1f` low-tile threshold and Skyblock gating. Effects for
dungeon-coordinate clearing and low-tile notification are explicit ports after validation.

Actual source files:

- `src/WorldSession/WorldGeneration/Definitions/WorldSkyblockGenerationScanLifecycle.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldSkyblockGenerationDimensions.cs`
- `src/WorldSession/WorldGeneration/Adapters/WorldSkyblockGenerationTileObservation.cs`
- `src/WorldSession/WorldGeneration/Adapters/IWorldSkyblockGenerationGridReader.cs`
- `src/WorldSession/WorldGeneration/Components/WorldSkyblockGenerationScanComponent.cs`
- `src/WorldSession/WorldGeneration/Components/WorldSkyblockGenerationScanSnapshot.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldSkyblockGenerationRulesInput.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldSkyblockGenerationRulesSelection.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldSkyblockGenerationPolicySelection.cs`
- `src/WorldSession/WorldGeneration/Queries/WorldSkyblockGenerationRulesQuery.cs`
- `src/WorldSession/WorldGeneration/Queries/WorldSkyblockGenerationPolicyQuery.cs`
- `src/WorldSession/WorldGeneration/Systems/WorldSkyblockGenerationScanSystem.cs`
- `src/WorldSession/WorldGeneration/Adapters/ISkyblockGenerationEffectsPort.cs`
- `src/WorldSession/WorldGeneration/Systems/WorldSkyblockGenerationRulesCommitResult.cs`
- `src/WorldSession/WorldGeneration/Systems/WorldSkyblockGenerationRulesCommitSystem.cs`

Dependency impact: the boundary is independent of `Main`, tile globals, network and logging. The
existing legacy Skyblock queries and world descriptor are not changed, so no second runtime writer
is activated. VerificationStatus remains `not-run`; exact legacy comparison and focused scan tests
are pending.

## 15. Completed Component: WorldSeedOptionCatalogDefinition

status: implemented-unverified.

### 15.1 Responsibility and exact option metadata

This boundary covers the `WorldSeedOption_Everything._dependencies` field and the 21 reported
`KeyName`, `ServerConfigName` and `Dependencies` properties. The catalog is immutable after
construction and preserves Version4 registration order. The protected Version4 `KeyName` values
are exposed only as read-only metadata; they are not localization objects and do not create UI
side effects.

| Registration order | Option | `KeyName` | `ServerConfigName` |
| ---: | --- | --- | --- |
| 1 | Normal | `Seed_Normal` | `null` |
| 2 | NotTheBees | `Seed_NotTheBees` | `notthebees` |
| 3 | Drunk | `Seed_Drunk` | `drunk` |
| 4 | Anniversary | `Seed_Celebration` | `celebration` |
| 5 | DontStarve | `Seed_TheConstant` | `theconstant` |
| 6 | ForTheWorthy | `Seed_ForTheWorthy` | `fortheworthy` |
| 7 | NoTraps | `Seed_NoTraps` | `notraps` |
| 8 | Remix | `Seed_Remix` | `remix` |
| 9 | Everything | `Seed_Everything` | `zenith` |
| 10 | Skyblock | `Seed_Skyblock` | `skyblock` |

`Everything.Dependencies` is the immutable ordered list `Remix`, `Drunk`, `NotTheBees`,
`NoTraps`, `DontStarve`, `Anniversary`, `ForTheWorthy`. It intentionally excludes Normal,
Everything and Skyblock. The Version4 `_dependencies` lazy cache is treated as a construction
detail: the proposed catalog derives this list from option IDs once and returns an immutable view.
It is not an independently writable field and must not leak the legacy option objects.

For selection compatibility, the catalog also carries the source metadata used by the adapter:
special names `notthebees`, `drunk` value `5162020`, `celebrationmk10` with values `5162021` and
`5162011`, DontStarve names `constant`, `theconstant`, `eye4aneye`, `eyeforaneye`, Everything
name `getfixedboi`, ForTheWorthy name `fortheworthy`, NoTraps name `notraps`, Remix name
`dontdigup` and Skyblock name `skyblock`. Normal has no special name or value. This metadata is
read-only catalog input; it does not itself select an option.

### 15.2 Query and adapter boundaries

`WorldSeedOptionCatalogQuery` returns an explicit selection candidate from seed text, translated
seed value or a server configuration record. It preserves the Version4 adapter semantics without
mutating catalog state:

- normalize seed text by lower-casing and removing non-alphanumeric characters before matching;
- compare translated numeric seed values and then special names in registration order;
- accept `seed_` configuration lines only at the server-config adapter, parse the integer and
  clamp it to 0 or 1 before returning an `AutoGenerationCandidate`;
- return Normal when the selection command explicitly requests reset, while leaving the actual
  `Enabled` transition to a commit system;
- expand Everything through the ordered dependency IDs, not through recursive mutable option
  objects.

The `WorldSeedOptionSelectionCommitSystem` is the sole writer of the selected option/flag
snapshot. A compatibility adapter may perform the Version4 `SelectOption` sequence (reset Normal,
then enable the selected option), publish `OnOptionStateChanged`, or update `AutoGenEnabled`, but
those effects are ports outside the pure catalog query. UI element creation, localization and
server-config persistence are also outside this boundary.

### 15.3 Current NLTX mapping and risks

The current NLTX WorldGenerationRequest and WorldRuleSnapshotComponent carry several selected
world flags, but no complete immutable option catalog with the ten-entry registration order and
the Everything dependency metadata was found. The proposed catalog must therefore be introduced
as a single read-only source and mapped to the existing flags only through the approved
WorldGenerationSecretSeedFlags commit seam. Dual writers for request flags and catalog selection
are not allowed.

The main risks are changing matching precedence, losing the null Normal server-config name,
returning a mutable dependency list, or firing option events during a pure lookup. Persistence and
network owners must decide whether the selected option ID, expanded dependencies and AutoGen flags
are stored or recomputed from the world seed.

### 15.4 Verification design

- verify the ten registration entries, exact order, key names and server-config names including
  Normal's null value;
- verify the seven Everything dependencies and their order, immutability and absence of mutable
  legacy objects;
- verify special-name normalization, translated numeric values, registration-order precedence and
  no-match behavior;
- verify `seed_` parsing, integer clamping and ignored unknown/null server-config names;
- verify selection reset/enable semantics through an adapter while proving the pure query emits no
  event, UI object, persistence write or catalog mutation;
- verify one canonical selected-flag commit and compatibility projection against the existing
  request/snapshot types before changing call sites.

This verifier has not been run: verificationStatus: not-run.

### 15.5 Implementation checkpoint

The immutable ten-entry Version4 option catalog is implemented in registration order. Each option
preserves its key name, nullable server configuration name, special seed names and translated
numeric values. Everything exposes the ordered seven-ID dependency view without legacy option
instances. The pure catalog query normalizes seed text, checks translated values and names in
registration order, and parses `seed_` lines with the Version4 clamp to `0` or `1`. It returns
selection candidates only; it does not mutate enabled flags, AutoGen state, UI, events,
persistence or network state.

Actual source files:

- `src/WorldSession/WorldGeneration/Definitions/WorldSeedOptionId.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldSeedOptionDefinition.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldSeedOptionCatalogDefinition.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldSeedOptionMatch.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldSeedOptionSeedMatchResult.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldSeedOptionAutoGenerationResult.cs`
- `src/WorldSession/WorldGeneration/Queries/WorldSeedOptionCatalogQuery.cs`

Dependency impact: the catalog is independent of the unresolved standard-seed flags owner and does
not change current request/snapshot writers or Version4 UI/event adapters. VerificationStatus
remains `not-run`; metadata, precedence and server-line focused tests are pending.

## 16. Completed Component: WorldLandmassAndTreeProfilesDefinition

status: implemented-unverified.

### 16.1 Responsibility and value ownership

This boundary contains the landmass value definition and the immutable tree-profile catalog. It
covers the final four fields and one property in `LandmassData`, all ten static tree profiles and
the seven `GrowTreeSettings` fields in the authoritative report. It contains no tile mutation and
does not own a world-generation pass.

`WorldLandmassDefinition` is a value type with the following members:

| Version4 member | Proposed value semantics |
| --- | --- |
| `DataType` | Preserve the `LandmassDataType` value without interpreting it in the definition. |
| `Position` | Preserve the center/position vector. |
| `RadiusOrHalfSize` | Preserve the signed integer magnitude used by the source formulas. |
| `Style` | Preserve the style discriminator as data; style interpretation belongs to the consuming generation query. |
| `Top` | Getter returns `Position - (0, RadiusOrHalfSize)`; setter/value replacement assigns `Position = value + (0, RadiusOrHalfSize)`. |

The `Top` setter is a value-local mutation for compatibility with the source API. If the value is
stored in an authoritative component, a replacement command must write the new value through that
component's commit port; the property must not mutate another component or global world state.
The round-trip invariant is `landmass.Top = top; landmass.Top == top` and the inverse relation
must preserve `RadiusOrHalfSize`.

### 16.2 Tree profile catalog

`WorldTreeProfileCatalog` exposes immutable profile values and a lookup by tree tile type. The
Version4 declaration order is retained independently from lookup order:

| Profile | `TreeTileType` | `SaplingTileType` | `TreeHeightMin..Max` | `TreeTopPaddingNeeded` | Ground predicate | Wall predicate |
| --- | ---: | ---: | --- | ---: | --- | --- |
| `GemTree_Ruby` | 587 | 590 | 7..12 | 4 | `GemTreeGroundTest` | `GemTreeWallTest` |
| `GemTree_Diamond` | 588 | 590 | 7..12 | 4 | `GemTreeGroundTest` | `GemTreeWallTest` |
| `GemTree_Topaz` | 583 | 590 | 7..12 | 4 | `GemTreeGroundTest` | `GemTreeWallTest` |
| `GemTree_Amethyst` | 584 | 590 | 7..12 | 4 | `GemTreeGroundTest` | `GemTreeWallTest` |
| `GemTree_Sapphire` | 585 | 590 | 7..12 | 4 | `GemTreeGroundTest` | `GemTreeWallTest` |
| `GemTree_Emerald` | 586 | 590 | 7..12 | 4 | `GemTreeGroundTest` | `GemTreeWallTest` |
| `GemTree_Amber` | 589 | 590 | 7..12 | 4 | `GemTreeGroundTest` | `GemTreeWallTest` |
| `VanityTree_Sakura` | 596 | 595 | 7..12 | 4 | `VanityTreeGroundTest` | `DefaultTreeWallTest` |
| `VanityTree_Willow` | 616 | 615 | 7..12 | 4 | `VanityTreeGroundTest` | `DefaultTreeWallTest` |
| `Tree_Ash` | 634 | 20 | 7..12 | 4 | `AshTreeGroundTest` | `DefaultTreeWallTest` |

The type mapping must preserve the Version4 `TryGetFromTreeId` cases for tile types 583, 584,
585, 586, 587, 588, 589, 596, 616 and 634. Catalog enumeration order is an explicit contract,
not a property of file or directory order.

`TreeTileType`, `TreeHeightMin`, `TreeHeightMax`, `TreeTopPaddingNeeded` and `SaplingTileType`
are immutable numeric values. `GroundTest` and `WallTest` correspond to the source delegate types
`IsTileFitForTreeGroundTest(int)` and `IsWallTypeFitForTreeBack(int)`, but they must be represented
as explicit predicate IDs or injected callable ports. A compatibility adapter may materialize
delegates for a legacy caller; the definition itself must not close over `Main`, `WorldGen`, a tile
grid, a wall grid or a mutable registry.

### 16.3 Query and effect seams

The catalog supplies profile values to a tree-growth eligibility query. That query receives the
world-grid snapshot and explicit ground/wall definition maps or predicate ports, performs tile and
wall reads through those inputs, and returns an eligibility result. Tree placement, sapling
replacement, liquid changes and network/persistence effects remain in their owning systems.

Landmass generation receives a value copy and returns a new value when it changes `Top`, `Style` or
position. It does not use the `Top` setter as a hidden cross-entity write. Profile lookup failure
returns an explicit no-profile result rather than a default profile that could silently place the
wrong tree.

### 16.4 Current NLTX mapping and risks

`LegacyTreeProfileDefinition` and `LegacyTreeProfileRegistry` already cover the ten numeric tile,
sapling, height and padding values. `TreeProfileGrowthEligibilityQuery`,
`TreeGroundSuitabilityQuery` and `TreeWallSuitabilityQuery` provide explicit world-snapshot and
definition-map seams for much of the predicate behavior. This is partial evidence: the existing
registry's default enumeration order differs from the Version4 declaration order, it does not
represent the source delegate fields, and no complete `LandmassData` value mapping was found.

Before implementation, compare every profile consumer to determine whether declaration order is
observable, then choose one canonical catalog. P19/P20 or tree-generation owners must consume the
catalog and predicate ports without writing profile definitions. Persistence may store a profile
ID and landmass value only after the world-generation owner defines versioning and compatibility.

### 16.5 Verification design

- map all four LandmassData fields, `Top`, ten profile entries and seven GrowTreeSettings fields;
- verify `Top` getter/setter round trips, Position translation and preservation of radius, type and
  style;
- verify all ten numeric profiles, declaration order, tile-ID lookup and unknown-ID behavior;
- verify predicate IDs/delegates are explicit inputs and perform no hidden tile/wall/global reads;
- compare tree eligibility results with the existing NLTX query across ground, wall, sapling,
  liquid, slope and boundary fixtures;
- verify no definition or lookup mutates tiles, world state, persistence, network or UI objects.

This verifier has not been run: verificationStatus: not-run.

### 16.6 Implementation checkpoint

The landmass/tree boundary is implemented in the root `src` project. `WorldLandmassDefinition`
preserves the three Version4 landmass data-type values, the position/radius/style fields and the
value-local `Top` getter/setter translation. `WorldTreeProfileCatalog` preserves the ten Version4
profile values, declaration order, tile-ID lookup and explicit ground/wall predicate IDs without
reading tiles, walls, `Main`, `WorldGen` or mutable registries. The catalog and lookup expose
defensive read-only views, and unknown tree IDs return no profile.

Actual source files:

- `src/WorldSession/WorldGeneration/Definitions/WorldLandmassDataType.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldLandmassDefinition.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldTreeProfileId.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldTreeGroundPredicateId.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldTreeWallPredicateId.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldTreeProfileDefinition.cs`
- `src/WorldSession/WorldGeneration/Definitions/WorldTreeProfileCatalog.cs`
- `src/WorldSession/WorldGeneration/Queries/WorldTreeProfileCatalogQuery.cs`

Dependency impact: this definition/query boundary does not modify the existing landmass component,
legacy tree registry, tree placement systems, tile or wall state, persistence, network or UI. The
predicate delegates remain explicit IDs until an owning adapter is approved. A static review also
changed the scan presence-set loop to use an `int` index before converting to the `ushort` tile/wall
ID, preventing 65536-entry array wraparound.

Evidence-gap: profile consumer order, predicate adapter equivalence and landmass persistence/version
ownership remain unresolved. VerificationStatus remains `not-run`; compilation and focused behavior
checks are pending.

Blocking-decision: do not replace the legacy tree registry or connect landmass persistence until the
profile consumer and value-serialization owners are identified. Keep all tile/wall reads and tree
placement effects outside the definition/catalog/query boundary.

## 17. Remaining Deferred Boundary

`WorldGenerationSecretSeedFlagsComponent` remains deferred because its canonical owner,
compatibility window and persistence/network contract are unresolved. All other 11 implemented
boundaries have source mappings and checkpoints, but ownership, persistence/network policy and
focused verification remain open as described below.

## 18. Integration Handoff

- P16/P17: resolve ownership of world progression, infection, surface and biome flags consumed by
  secret-seed queries.
- P19/P20: consume Skyblock and seed rule queries through explicit read seams and submit world
  generation actions through commands; do not write P18 components directly.
- Persistence/network owner: decide whether selected flags, enabled secret-seed set, option names,
  scan-derived results and tree/landmass definitions are persisted, replicated or recomputed.
- Runtime scheduler owner: register the barriers in Section 4 explicitly and test that generation
  pass order is not inferred from file order.

## 19. Evidence Gaps and Blocking Decisions

The remaining ownership block is closure of the world-scoped ECS persistence/network contract for
the selected flags and runtime compatibility fields. Definition-only boundaries may be implemented
without adding a second authority. The metadata evidence-gap and blocking-decision fields are
updated at every checkpoint.

## 20. Verification Status

The latest serial build was run through `Build/Tools/Invoke-SerialDotnet.ps1` for
`src/WorldSession/Terraria.WorldSession.csproj` with `--no-restore`, single-node execution,
shared compilation disabled and node reuse disabled. It reached `CoreCompile` and exited with code
1 with `0` warnings and `1` error in the external-session file
`WorldGeneration/Systems/HellChestLootCycleSystem.cs(16,22)`: the call to
`TryAdvanceAfterSuccessfulPlacement` has no overload accepting one argument. An earlier run was
blocked by five `Terraria.Content`/`ColorRgba` errors in
`WorldGeneration/Actions/WorldGenerationTileFramingAndDebugActionsCommand.cs`. No P18 source file
was reported in either failure.
The existing WorldSession verifiers contain no P18-specific behavior matrix and were not run against
the failed-build artifacts. The expected full-project output path,
`Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`, is not trusted evidence
because the build failed.

After the shared checkout had no active `dotnet.exe`/`csc.exe` compiler owner, the selected P18
source boundary was compiled through the repository wrapper with this focused probe command:

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

The probe exited `0`, reported `0` warnings and `0` errors, and produced
`Build/bin/P18WorldGenerationCompileProbe/Debug/net10.0/P18WorldGenerationCompileProbe.dll`.
This verifies that the selected P18 source files compile in isolation; it does not prove full
WorldSession integration or behavior equivalence. P18 verification is therefore
`partially-verified`, not complete. The focused behavior matrices remain `not-run`.

## 21. Implementation Checkpoints

### 21.1 2026-09-12T00:20:00Z: WorldSecretSeedRegistryDefinitionsProjection

- Source files added under `src/WorldSession/WorldGeneration`: the immutable definition, projection,
  input result/rejection types, code-transformer and sound-port interfaces, and
  `Adapters/WorldSecretSeedInputAdapter.cs`.
- Core behavior: 35 Version4 definitions retain registration order, localization keys, opaque codes
  and source anchors; the projection defensively copies custom input into a read-only list; the adapter performs explicit
  normalization and plaintext/code matching, returns unlock text in a result, and calls a sound port
  only when explicitly requested.
- Dependency impact: no existing writer or compatibility field was changed; no third-party sound
  type enters the definition.
- Actual follow-up fix: the projection constructor now copies caller-provided definition lists before
  exposing `Definitions`, preventing a mutable input list from becoming a second catalog authority.
- Evidence-gap: the Version4 code transformation implementation is not available as a root NLTX
  dependency, so the adapter requires an injected transformer; plaintext persistence and runtime
  enablement remain outside this checkpoint.
- Blocking-decision: do not connect the result to runtime registry or legacy static state until the
  world-scoped owner and persistence/network policy are closed.
- VerificationStatus: not-run; no build or focused verifier has been executed yet.

### 21.2 2026-09-12T00:35:00Z: WorldSecretSeedVisualAndSurfaceRulesDefinition

- Source files added: `Definitions/WorldSecretSeedVisualAndSurfaceRulesDefinition.cs`,
  `Definitions/WorldSecretSeedVisualAndSurfaceRulesSelection.cs`, and
  `Queries/WorldSecretSeedVisualAndSurfaceRulesQuery.cs`.
- Core behavior: nine visual/surface rules resolve to stable catalog definitions and a pure query
  projects enabled membership from an explicit read-only variant set.
- Dependency impact: no `Enabled` copy, tile write, weather effect, progression write, registry
  mutation or external I/O was added.
- Evidence-gap: final consumers for weather, surface and frozen-world effects remain integration
  review with P16/P17; this checkpoint exposes only definition references and a read-only result.
- Blocking-decision: runtime enablement remains owned by the future runtime registry; no consumer
  may write the definition or catalog.
- VerificationStatus: not-run.

### 21.5 2026-09-12T01:20:00Z: WorldSecretSeedSeasonalRulesDefinition

- Source files added: `Definitions/WorldSecretSeedSeasonalRulesDefinition.cs`,
  `Definitions/WorldSecretSeedSeasonalRulesSelection.cs`, and
  `Queries/WorldSecretSeedSeasonalRulesQuery.cs`.
- Core behavior: three seasonal rule references resolve from the immutable catalog and a pure query
  returns selected membership from explicit input.
- Dependency impact: no clock, event, weather, audio, UI, persistence or replication effect was
  introduced.
- Evidence-gap: calendar/event ownership and active-event persistence remain integration-review.
- Blocking-decision: seasonal consumers must receive explicit calendar context and own effects;
  this definition is not an event-state owner.
- VerificationStatus: not-run.

### 21.4 2026-09-12T01:05:00Z: WorldSecretSeedProgressionAndInfectionRulesDefinition

- Source files added: `Definitions/WorldSecretSeedProgressionAndInfectionRulesDefinition.cs`,
  `Definitions/WorldSecretSeedProgressionAndInfectionRulesSelection.cs`, and
  `Queries/WorldSecretSeedProgressionAndInfectionRulesQuery.cs`.
- Core behavior: twelve progression/infection rule references resolve from the immutable catalog;
  the pure query reports selected rules without committing hardmode, infection, biome, spawn,
  player or network state.
- Dependency impact: existing `WorldRulesState` and progression components remain untouched; no
  duplicate world-fact writer was introduced.
- Evidence-gap: final cross-partition owners and replication semantics for P06/P07/P12/P14/P16/P17/
  P19/P20 remain unresolved.
- Blocking-decision: `StartInHardmode` and all post-generation facts remain intents/results only;
  owning systems must commit them.
- VerificationStatus: not-run.

### 21.3 2026-09-12T00:50:00Z: WorldSecretSeedTerrainAndStructureRulesDefinition

- Source files added: `Definitions/WorldSecretSeedTerrainAndStructureRulesDefinition.cs`,
  `Definitions/WorldSecretSeedTerrainAndStructureRulesSelection.cs`, and
  `Queries/WorldSecretSeedTerrainAndStructureRulesQuery.cs`.
- Core behavior: eleven terrain/structure rule references resolve from the immutable catalog and a
  pure query returns explicit enabled membership.
- Dependency impact: no tile, liquid, chest, dungeon, teleporter, landmass or runtime registry
  writer was added; cross-partition consumers remain read-only integration seams.
- Evidence-gap: execution ownership and scheduler/command contracts for P01/P09/P19/P20 remain
  unresolved.
- Blocking-decision: `ExtraFloatingIslands` is represented only once and must be supplied to later
  Skyblock/variation queries from the same runtime snapshot.
- VerificationStatus: not-run.

### 21.6 2026-09-11T21:31:52Z: P18 isolated compile verification

- Verification command: `Build/Tools/Invoke-SerialDotnet.ps1` with the
  `Build/Diagnostics/P18WorldGenerationCompileProbe/P18WorldGenerationCompileProbe.csproj`
  project, `--no-restore`, `-m:1`, `-nr:false`, `UseSharedCompilation=false`,
  `MSBuildNodeReuse=false` and `BuildInParallel=false`.
- Result: exit code `0`, `0` warnings, `0` errors. The verified artifact was
  `Build/bin/P18WorldGenerationCompileProbe/Debug/net10.0/P18WorldGenerationCompileProbe.dll`.
- Scope: the probe explicitly includes the P18 Definitions, Components, Queries, Systems,
  Adapters and SecretSeed Commands. It does not establish full-project integration or runtime
  behavior equivalence.
- Remaining decision: `WorldGenerationSecretSeedFlagsComponent` stays deferred because the
  existing world-rule owners, compatibility window and persistence/network contract are not
  uniquely identified. No second authority was added.

### 21.7 2026-09-11T22:12:14Z: Skyblock scan snapshot ownership hardening

- Source files modified: `Components/WorldSkyblockGenerationScanSnapshot.cs` and
  `Definitions/WorldSkyblockGenerationRulesInput.cs`.
- Core behavior: both boundaries now defensively copy caller-provided tile/wall sets into
  `FrozenSet<ushort>` values. A caller retaining a mutable set can no longer mutate a committed
  scan snapshot or alter a rules-query input after construction.
- Dependency impact: no tile/world write, scheduler registration, compatibility field,
  persistence format, network message or existing call site changed. The scan component remains
  the only transient scan accumulator; the query remains pure.
- Verification: `Terraria.WorldSession` serial build was attempted through
  `Invoke-SerialDotnet.ps1` and failed with exit code `1`, `0` warnings and `1` external error at
  `Systems/HellChestLootCycleSystem.cs(16,22)`; no P18 file was reported. The isolated P18 probe
  was then rebuilt through the same wrapper and passed with exit code `0`, `0` warnings and
  `0` errors, producing
  `Build/bin/P18WorldGenerationCompileProbe/Debug/net10.0/P18WorldGenerationCompileProbe.dll`.
- Evidence-gap: focused behavior tests for source scan/reset/effect parity remain `not-run`.
- Blocking-decision: `WorldGenerationSecretSeedFlagsComponent` remains deferred; no canonical
  standard-flag owner, compatibility window, persistence/network contract or scheduler barrier
  was discovered, so no additional flag writer was created.

### 21.8 2026-09-11T22:50:28Z: WorldSkyblockGenerationRulesQuery non-Skyblock gating

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

### 21.9 2026-09-11T22:56:30Z: P18 compile verification after Skyblock query gating

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
  `HellChestLootCycleSystem.cs(16,22)` overload error documented in Section 20, and no P18
  focused behavior verifier has run.

### 21.10 2026-09-11T23:01:41Z: full WorldSession build recheck

- Verification command: `Build/Tools/Invoke-SerialDotnet.ps1` with
  `build src/WorldSession/Terraria.WorldSession.csproj --no-restore -m:1 -nr:false`,
  `UseSharedCompilation=false`, `MSBuildNodeReuse=false` and `BuildInParallel=false`.
- Result: exit code `1`, `0` warnings and `1` error. The failure remains external to P18 at
  `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs(16,22)` with `CS1501`
  because `TryAdvanceAfterSuccessfulPlacement` has no one-argument overload. No P18 source error
  was reported; the expected full-project DLL is not trusted because the build failed.
- VerificationStatus: `partially-verified`; isolated P18 compilation passed, but full integration
  and focused Skyblock behavior matrices remain unverified.

### 21.11 2026-09-12T01:53:45Z: P18 focused behavior verifier

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

### 21.12 2026-09-12T07:03:03Z: WorldGenerationSecretSeedFlagsComponent data shape

- Actual source file saved:
  `src/WorldSession/WorldGeneration/Components/WorldGenerationSecretSeedFlagsComponent.cs`.
- Implemented data shape: a sealed, read-only world-generation component with the nine selected
  Version4 generation inputs `RemixWorldGeneration`, `EverythingWorldGeneration`,
  `NoTrapsWorldGeneration`, `DrunkWorldGeneration`, `GetGoodWorldGeneration`,
  `TenthAnniversaryWorldGeneration`, `DontStarveWorldGeneration`, `NotTheBeesWorld` and
  `SkyblockWorldGeneration`. All constructor inputs default to `false`.
- `DrunkWorldGenerationText` is a read-only derived property that returns `DrunkWorldGeneration`;
  it has no independent constructor input or setter, preserving the Version4 relationship at
  `WorldGen.cs:4324` to `drunkWorldGen` at `WorldGen.cs:4312`.
- Version4 field mapping is preserved at `WorldGen.cs:4306`, `4308`, `4310`, `4312`, `4314`,
  `4316`, `4318`, `4320`, `4322` and `4324`.
- No writer, commit system, snapshot, projection, query, adapter, command, test, interface or
  compatibility bridge was added. No existing overlapping component was modified.
- The data shape is implemented, but the component remains pending for canonical integration:
  `currentComponent: none`, `implementedDataShapes` contains this component, and
  `pendingComponents` retains `WorldGenerationSecretSeedFlagsComponent` with canonical-owner
  blocking.
- `implementationStatus: in-progress` and `verificationStatus: partially-verified` remain
  truthful. Canonical owner, compatibility window, persistence/network policy and scheduler
  barrier are unresolved; no dual writer is permitted. Fresh verification results are recorded
  in the execution checkpoint after the affected-project build.

### 21.13 2026-09-12T07:16:31Z: WorldGenerationSecretSeedFlagsComponent serial verification

- The affected `Terraria.WorldSession` project was built from the repository root through
  `Build/Tools/Invoke-SerialDotnet.ps1` with `build`, `--no-restore`, `-m:1`, `-nr:false`,
  `UseSharedCompilation=false`, `MSBuildNodeReuse=false` and `BuildInParallel=false`.
- The build exited `1` with `0` warnings and `1` error in the external workspace file
  `src/WorldSession/WorldGeneration/Systems/HellChestLootCycleSystem.cs(16,22)`, `CS1501`:
  `TryAdvanceAfterSuccessfulPlacement` has no overload accepting one argument. No P18 source
  error was reported. The existing
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` predates this failed
  build and is not trusted as a successful full-project artifact.
- The existing P18 focused verifier was run through the same wrapper with `run`, `--project`,
  `--no-build`, `--no-restore` and the same serial MSBuild properties. It exited `0` and printed
  `P18 focused verifier passed.` This is regression evidence for existing P18 boundaries only;
  the verifier does not directly include or validate `WorldGenerationSecretSeedFlagsComponent`.
- The data shape remains implemented but integration-blocked. `implementationStatus` remains
  `in-progress`, `verificationStatus` remains `partially-verified`, and
  `pendingComponents` retains `WorldGenerationSecretSeedFlagsComponent`. Canonical owner,
  compatibility window, persistence/network policy and scheduler barrier remain unresolved;
  no writer or dual authority was added.
