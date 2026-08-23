# WorldGen ECS parity report

## Evidence

- Fact source: `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs`
- Source bytes: `1,901,533`
- Source lines: `73,355`
- Source SHA-256: `A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D`
- Scanner: `Terraria.Dome.WorldGeneration.Verification` `1.0.0.0`
- Replay input: seed `1456`, world `400 x 300`, spawn `(200, 80)`, variant `default`,
  random stream version `1`
- Replay snapshot fingerprint: `92BE1D99899B91474367F586A6D369B0ECDB8DBAFF8613B59B1F518990123A32`
- Replay section versions: `(0,0)=14125`, `(1,0)=14119`, `(0,1)=30000`, `(1,1)=30000`
- Complete baseline oracle replay: two fixed-seed `TerrariaServer.exe` runs from
  `D:\TRbackup\无任何删减通过编译\bin\Debug\net40\TerrariaServer.exe` at `4200 x 1200`
  were read as WLD v319. The executable SHA-256 is
  `5DF17C809726AF6EF72EDB893495BAA21DFA5650E29A4C2CC9B39DECD8069F42`; both runs normalized
  to `70306C2429385CEEB073A4963EBC3326BAC13C84BD06CFCEBE36FDD58F2A9917`.
- Cursor restart replay: a Terrain checkpoint restored two immutable worlds and resumed Cave;
  both committed to sequence `88112` and fingerprinted as
  `8A56DC520D3B3B4DB164285DD874D8DA0ACDAB7C9515C23F90DC86E60F11EF54`.
- Fresh full verifier run on 2026-08-22 completed with exit code `0` after correcting
  test-local frame/footprint fixtures and the production orb-origin calculation. This
  verifies the bounded query, command, stage, and restart contracts only; it does not
  change the complete differential result below.
- Complete baseline differential replay: the current comparator ran against the repeat `4200 x 1200`
  WLD v319 and compared all `5,040,000` tiles. It found `3,190,404` mismatches after applying
  the legacy inactive-tile `FrameX`/`FrameY` sentinel, source-derived TerrainPass stone/ground
  layering, and source-aligned cave kill frames, with `2,640,402` active legacy tiles and
  `3,671,879` active generated tiles. The current trace is authoritative for this baseline;
  older intermediate and all-tile mismatch records are retained as historical artifacts.
  The expanded `WorldTile` projection now carries wall,
  liquid, frame, wire, slope, actuator, paint, inactive and visibility state. The complete
  baseline still differs from ECS generation in at least one extended state field on `1,046,843`
  tiles. The overlapping field mismatch counts are `WallType`
  `995,054`, `Slope` `58,753`, `HalfBrick` `12,621`, `Wire` `5,957`, `Actuated` `1,352`,
  `WallColor` `348`, `Inactive` `234`, `Wire3` `192`, and `Wire2` `172`; all other classified
  fields were zero for this fixture. The request used WLD metadata spawn X `2099` and WLD
  `worldSurface` Y `325`; its WLD `rockLayer` Y `397` is also frozen into the ECS request.
- Complete baseline WLD import projection: the repeat baseline was imported into the Dome
  snapshot and all `5,040,000` tile states compared with `0` mismatches. One opaque non-tile
  record was retained. This proves old-world tile-state preservation only; it does not establish
  generated-world semantic parity.
- Differential localization: `legacy-worldgen-differential.json` records all `21` compared
  tile-state fields globally, for every section, and in three WLD metadata regions. The current
  total is `3,190,404 / 5,040,000` mismatching tiles; use the fresh
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/current-full-differential/trace.txt`
  when section or region-level counts are required. The largest
  base-state differences are `TileType` `2,897,554`, `IsActive` `1,583,853`, `WallType`
  `995,054`, `LiquidAmount` `303,611`, `LiquidKind` `196,096`, `FrameX` `94,204`, and
  `FrameY` `91,258`; the remaining classified fields are recorded in the JSON artifact. This
  is diagnostic consistency evidence for selecting a bounded
  migration slice, not semantic parity evidence.

## Implemented coverage

| Proposal stage | Current evidence | Status |
| --- | --- | --- |
| 0: source inventory and replay baseline | Roslyn inventory, method map, source hash, deterministic replay | Passed |
| 1: frozen input and tile commit boundary | immutable request, monotonic state, atomic stable tile commit | Passed |
| 2: terrain/cave/biome | profile-driven terrain, protected cave profile, explicit unsupported biome | Passed |
| 3: ore/tree/structure | definition-driven placement, protected footprint, deterministic prepare/commit structure transaction, explicit Tile/Wall command seam | Passed |
| 4: liquids | source work items, bounded propagation, explicit merge, versioned liquid commit | Passed |
| 5: frame/runtime/protocol boundary | frame commands, validation, runtime rule snapshot wrapper, immutable section projection | Passed |
| 5b: cosmetic neighbor projection | canonical stone neighbor types and slope-aware cardinal filtering; final `TileFrameCosmetic` remains deferred | Partial |
| 5b.1: invisible-block merge culling | center-to-eight-neighbor invisible-block culling with explicit visibility input; host visibility policy remains deferred | Partial |
| 5b.2: tree type classification | non-negative membership against an explicit tree-trunk registry; registry ownership remains deferred | Partial |
| 5b.3: paint color mapping | fixed paint identifier to RGBA mapping; legacy Color type and effect application remain deferred | Partial |
| 5b.4: coating color mapping | fixed coating identifier to RGBA mapping; legacy Color type and application remain deferred | Partial |
| 5b.5: coating selection mapping | immutable fullbright/invisible block and wall selection; mutable legacy Color list remains deferred | Partial |
| 5b.6: forest background style mapping | all fixed styles map to immutable mountain/tree sets; caller array mutation and host application remain deferred | Partial |
| 5b.7: hollow-tree foliage style mapping | hallow background style maps to bounded foliage style; host background and TreeTops registry remain deferred | Partial |
| 5b.8: pile/speleothem invalidity mapping | world-margin, active-tile, and explicit boulder registry predicate; destruction remains deferred | Partial |
| 5b.9: vine framing mapping | support/slope classification, replacement and kill intent; framing and mutation remain deferred | Partial |
| 5b.10: square tile frame requests | deterministic nine-point request topology/order; resetFrame, frame calculation, and mutation remain deferred | Partial |
| 5b.11: square wall frame requests | deterministic nine-point wall topology/order; resetFrame, wall frame calculation, and mutation remain deferred | Partial |
| 5b.12: range frame coordinates | expanded rectangle and column-major traversal; map/frame calculation and mutation remain deferred | Partial |
| 5b.13: tile merge cull application | immutable eight-neighbor mask application; ref mutation and cache ownership remain deferred | Partial |
| 5b.14: spawn-area classification | remix, randomized/no-surface, and ordinary surface predicates with explicit inputs; host state remains deferred | Partial |
| 5b.15: tile category count formulas | fixed category formulas over explicit counts; tile scanning and mutation remain deferred | Partial |
| 5b.16: tile type area counts | active-tile count vector over explicit snapshot rectangle; caller mutation and host scanning remain deferred | Partial |
| 5b.17: housing tested bounds | fixed expansion and world-size clamping over explicit inputs; room scan and scheduling remain deferred | Partial |
| 5b.18: housing home-spot predicate | active type-379 rejection over explicit tile; room scoring and scheduling remain deferred | Partial |
| 5b.19: room-needs classification | four required tile categories over explicit sets; flags, scoring, and scheduling remain deferred | Partial |
| 5b.20: housing room occupancy | explicit room-coordinate membership; mutable room container and scanning remain deferred | Partial |
| 5c: TileFrameImportant bounded branches | pure type-136 support/beam/tree/wall, type-184 moss framing, type-324 boulder rejection, and type-529 conversion-sand support; other branches remain deferred | Partial |
| 5d: 3x1 footprint validation | origin/frame/support validation for `Check3x1`; breakability, destruction, drops, and recursive framing remain deferred | Partial |
| 5e: pile validation | support and snow/ice/sand style-band validation for `CheckPile`; ordinary/table `Check2x1` support is mapped, while type-185 pile validation and side effects remain deferred | Partial |
| 5f: dye frame validation | frame-band support rules for `CheckDye`, including cactus support; kill commit remains deferred | Partial |
| 5g: Rock Golem head validation | bottom-support rule for `CheckRockGolemHead`; kill commit remains deferred | Partial |
| 5h: 2x2 style footprint validation | origin, style band, frame-X, bottom support, and type-254 support validation for `Check2x2Style`; deletion, drops, random state, and recursive framing remain deferred | Partial |
| 5i: orb footprint validation | 2x2 origin, active/type validation, and type-12/type-639 bottom support for `CheckOrb`; deletion, drops, and framing remain deferred | Partial |
| 5j: 4x2 footprint validation | 4x2 origin, style/frame coordinates, and bottom support for `Check4x2` beds, picnic tables, and bathtubs; destruction, drops, and recursive framing remain deferred | Partial |
| 5k: rope endpoint framing boundary | immutable rope endpoint discovery and bounded `RopeEnd` frame requests; final rope frame selection and recursive side effects remain deferred | Partial |
| 5l: generic 2x2 footprint validation | generic 2x2 frame/style and top/bottom support validation for `Check2x2`; boulder, type-132, type-652, deletion, drops, and recursive framing remain deferred | Partial |
| 5m: generic 3x2 footprint validation | generic 3x2 frame/style, footprint-height, and bottom-support validation for `Check3x2`; special object rules, deletion, drops, and recursive framing remain deferred | Partial |
| 5n: generic 3x4 footprint validation | generic 3x4 frame/style and bottom-support validation for `Check3x4`; object deletion, drops, and recursive framing remain deferred | Partial |
| 5o: generic 5x4 footprint validation | generic 5x4 frame/style and bottom-support validation for `Check5x4`; object deletion, drops, and recursive framing remain deferred | Partial |
| 5p: generic 6x3 footprint validation | generic 6x3 frame and bottom-support validation for `Check6x3`; object deletion, drops, and recursive framing remain deferred | Partial |
| 5q: generic 4x3 wall footprint validation | generic 4x3 wall-object frame/style, active/type, and wall presence validation for `Check4x3Wall`; destruction and drops remain deferred | Partial |
| 5r: generic 6x4 wall footprint validation | generic 6x4 wall-object frame/style, active/type, and wall presence validation for `Check6x4Wall`; destruction and drops remain deferred | Partial |
| 5s: generic 3x3 footprint validation | generic 3x3 frame/style and explicit top/bottom support validation for `Check3x3`; generation override, destruction, drops, and recursive framing remain deferred | Partial |
| 5t: generic 3x5 footprint validation | generic 3x5 frame/style and bottom-support validation for `Check3x5`; object deletion, drops, and recursive framing remain deferred | Partial |
| 5u: generic 3x6 footprint validation | generic 3x6 frame/style and bottom-support validation for `Check3x6`; object deletion, drops, and recursive framing remain deferred | Partial |
| 5v: generic 3x3 wall footprint validation | generic 3x3 wall-object frame/style, active/type, and wall presence validation for `Check3x3Wall`; destruction and drops remain deferred | Partial |
| 5w: generic 4x4 footprint validation | generic 4x4 frame/style and bottom-support validation for `Check4x4`; type-specific drops, destruction, and recursive framing remain deferred | Partial |
| 5x: generic wall 2x3 and 3x2 footprints | generic wall-object frame/style, active/type, and wall presence validation for `Check2x3Wall` and `Check3x2Wall`; destruction and drops remain deferred | Partial |
| 5y: generic 2x5 footprint validation | generic 2x5 frame/style and bottom-support validation for `Check2x5`; type-specific drops, destruction, and recursive framing remain deferred | Partial |
| 5z: variable-height 2xX validation | variable-height 2-column frame and top/bottom support validation for `Check2xX`; platform bridge, hammer state, destruction, and drops remain deferred | Partial |
| 5za: variable-height 1xX validation | type-dependent vertical frame, active/type, and bottom-support validation for `Check1xX`; destruction and type-specific drops remain deferred | Partial |
| 5zb: 1x2 footprint validation | two-tile vertical frame/type validation with solid-or-platform support for `Check1x2`; type-20 frame correction, destruction, and drops remain deferred | Partial |
| 5zc: 1x1 support validation | bottom solid-support eligibility for `Check1x1`; Abigail flower ground rules, boulder classification, and destruction remain deferred | Partial |
| 5zd: golf 1x1 validation | frame alignment and bottom solid-support validation for `CheckGolf1x1`; destruction remains deferred | Partial |
| 5ze: logic tile validation | 18-pixel frame alignment and type-419 support pairing for `CheckLogicTiles`; destruction and wiring side effects remain deferred | Partial |
| 5zf: alchemical support validation | style-specific support sets, half-brick rejection, and lava-contact checks for `CheckAlch`; style-5 conversion, network synchronization, and destruction remain deferred | Partial |
| 5zg: banner validation | three-tile vertical footprint, frame-band, and hanging-support validation for `CheckBanner`; destruction, drops, and recursive framing remain deferred | Partial |
| 5zh: weapon-rack validation | 3x3 type-334 footprint, encoded frame normalization, and wall backing checks for `CheckWeaponsRack`; entity inventory, drops, and destruction remain deferred | Partial |
| 5zi: mannequin validation | type-128/type-269 2x3 footprint, encoded frame normalization, and bottom support checks for `CheckMan`/`CheckWoman`; destruction and drops remain deferred | Partial |
| 5zj: top-mounted 1x2 validation | two-tile top-mounted frame validation with platform, rope, or solid support for `Check1x2Top`; entity, destruction, and drops remain deferred | Partial |
| 5zk: sign validation | 2x2 sign footprint/frame, bottom-mounted type-85 support, and oriented attachment checks for `CheckSign`; Sign/entity/destruction side effects remain deferred | Partial |
| 5zl: boulder chest protection | boulder origin and above-tile breakability/container protection checks for `CheckBoulderChest`; break execution, chest mutation, and network effects remain deferred | Partial |
| 5zm: chest validation | 2x2 chest footprint/frame and two-column bottom support checks for `CheckChest`; chest state, destruction, and drops remain deferred | Partial |
| 5zn: trap-door validation | type-387 2x1 and type-386 2x2 footprint/frame and solid-anchor checks for `CheckTrapDoor`; full anchors, destruction, drops, and framing remain deferred | Partial |
| 5zo: generic tile-frame validation | generic bounded active/type/frame formula validation for `CheckTileFrames`; legacy allocation and destruction semantics remain deferred | Partial |
| 5zp: tall-gate validation | fixed 1x5 type-388/389 frame and vertical-anchor validation for `CheckTallGate`; dynamic TileObjectData, destruction, drops, and framing remain deferred | Partial |
| 5zq: generic tile anchors | mode-based border anchor scans and snapshot attachment predicates for `CheckTileAnchors`/`AnchorValid`; table/platform legacy flags and allocation semantics remain deferred | Partial |
| 5zr: stalactite echo validation | type-dependent echo height, orientation, frame sequence, and top/bottom support checks for `CheckStalactiteEcho`; destruction and drops remain deferred | Partial |
| 5zs: stalactite validation | frame-branch orientation, one/two-tile continuity, and slope support checks for `CheckStalactite`; style updates, invalid-pile rules, and destruction remain deferred | Partial |
| 5zt: Christmas tree validation | fixed 4x8 type-171 footprint, frame-pair consistency, and middle-ground support checks for `CheckXmasTree`; destruction and drops remain deferred | Partial |
| 5zu: cannon validation | 4x3 cannon footprint, style/frame bands, and internal bottom support checks for `CheckCannon`; destruction and drops remain deferred | Partial |
| 5zv: music-box validation | 2x2 music-box footprint/frame and bottom support checks for `CheckMB`; table classification, destruction, drops, and framing remain deferred | Partial |
| 5zw: food-platter validation | bottom solid-support eligibility for `CheckFoodPlatter`; entity, destruction, and drops remain deferred | Partial |
| 5zx: bamboo validation | bamboo support, above-bamboo relation, and frame-band checks for `CheckBamboo`; random normalization, network, and destruction remain deferred | Partial |
| 5zy: sand-fall eligibility | below-tile conversion and falling predicates for `BlockBelowMakesSandConvertIntoHardenedSand`/`BlockBelowMakesSandFall`; legacy registry flags remain deferred | Partial |
| 5zz: breakability reason predicate | container/locked-door/protected-top early-return predicate for `CheckTileBreakability_HasReasonToReturnEarly`; orchestration remains deferred | Partial |
| 5zza: breakability survival | explicit chest/dresser, display-doll, and hat-rack survival decisions for `CheckTileBreakability2_ShouldTileSurvive`; runtime entity predicates remain deferred | Partial |
| 5zzb: torch attachment validation | down/left/right/wall torch attachment and suggested frame checks for `CheckTorch`; tree/beam special sets and mutations remain deferred | Partial |
| 5aa: oasis plant validation | 3x2 oasis plant frame and explicit conversion-sand support validation for `CheckOasisPlant`; destruction and recursive framing remain deferred | Partial |
| 5ab: jungle plant validation | 2x2/3x2 jungle plant frame/support validation for `CheckJunglePlant`, including type-702 bottom-slope support; NPC, drops, and destruction remain deferred | Partial |
| 5ac: underwater plant validation | bounded water/support/wall and frame-normalization decisions for `CheckUnderwaterPlant`; kill, network, random selection, and framing remain deferred | Partial |
| 5ad: cactus validation | cactus support, vertical height, side-branch attachment, and supported sand-type checks for `CheckCactus`; kill and recursive framing remain deferred | Partial |
| 5ae: on-table 1x1 validation | support shape, platform-side join, and type-78 bottom-slope decisions for `CheckOnTable1x1`; table registry lookup and destruction remain deferred | Partial |
| 5af: sunflower validation | 2x4 sunflower footprint/frame and allowed solid-ground checks for `CheckSunflower`; destruction, drops, and recursive framing remain deferred | Partial |
| 5ag: gnome validation | type-567 1x2 frame and solid/platform ground checks for `CheckGnome`; destruction, drops, and recursive framing remain deferred | Partial |
| 5ah: anchor orientation validation | directional attachment selection and wall fallback for `CheckAnchor`; empty legacy considered-solid helper and frame mutation remain deferred | Partial |
| 5ai: stinkbug blocker validation | anchor orientation reuse and horizontal style handling for `CheckStinkbugBlocker`; frame mutation and destruction remain deferred | Partial |
| 5aj: chandelier validation | 3/4x3 chandelier footprint and upper solid support for `CheckChand`; style bands, random drops, and destruction remain deferred | Partial |
| 5ak: pot validation | generic 2x2 pot footprint/support and type-653 bottom-slope classification for `CheckPot`; style, drops, destruction, and framing remain deferred | Partial |
| 5al: palm tree validation | palm ground normalization, support eligibility, and special frame correction for `CheckPalmTree`; random frame selection, destruction, and framing remain deferred | Partial |
| 5am: tree frame validation | ordinary tree ground normalization, cardinal neighbors, and bounded branch-frame selection for `CheckTree`; complete settings, destruction, and recursive framing remain deferred | Partial |
| 5an: configured tree validation | injected ground predicate, below classification, and cardinal same-tree neighbors for `CheckTreeWithSettings`; style mutation and destruction remain deferred | Partial |
| 5ao: special-town NPC spawning predicate | non-Truffle allow and Truffle unlock/surface/mushroom threshold predicate for `CheckSpecialTownNPCSpawningConditions`; room scan and NPC scheduling remain deferred | Partial |
| 5ap: town achievement eligibility | active-NPC coverage predicate for real-estate and town-slime achievement sets; NPC scanning and notifications remain deferred | Partial |
| 5aq: underground classification | deep/shallow shortcuts and bounded solid-density query for `checkUnderground`; legacy host fields and effects remain deferred | Partial |
| 5ar: room boundary validation | world-edge, room bounds, tile-count, and room-size gates for `CheckRoom`; recursive scan and housing semantics remain deferred | Partial |
| 5as: secret-seed input normalization | input/display normalization and explicit candidate matching for `CheckInputForSecretSeed`; BCrypt transform and seed mutation remain deferred | Partial |
| 5at: background equivalence | fixed background equivalence groups and exact-match fallback for `IsBackgroundConsideredTheSame` | Partial |
| 5au: jungle chest item rotation | deterministic four-item jungle chest rotation for `GetNextJungleChestItem`; random rare-item overrides and counter mutation remain deferred | Partial |
| 5av: secret-seed code check | code normalization and explicit expected-code comparison for `SecretSeed.Check`; transform and registry lookup remain deferred | Partial |
| 5aw: tile solidity override projection | fixed boulder and cracked-brick Tile ID sets for `SetBoulderSolidity` and `SetCrackedBrickSolidity`; global registry mutation remains deferred | Partial |
| 5ax: alchemy herb harvestability | explicit style/day/weather/time/surface rules for `IsAlchemyPlantHarvestable` and type 83/84 seed rule for `IsHarvestableHerbWithSeed`; host state reads remain deferred | Partial |
| 5ay: chest rigging | type-467 frame-band classification for `IsChestRigged` | Partial |
| 5az: town NPC spawn selector | occupant-priority and ordered candidate fallback for `IsThereASpawnablePrioritizedTownNPC`; TownManager/Main discovery and mutation remain deferred | Partial |
| 6: old-entry parity and deletion gate | complete baseline oracle replay and stage-boundary cursor restart pass; differential comparator ran and found complete mismatch | Open |

## Command and snapshot checks

The WorldGen verifier was run serially from the repository root with
`-p:UseSharedCompilation=false` after a complete Simulation build. It passed these assertions:

- deterministic replay produces the same snapshot fingerprint and section versions;
- generation stages cannot move backwards and command sequences are stable;
- invalid tile/liquid batches are rejected before mutation;
- terrain, cave, biome, ore, tree, structure, liquid, frame and validation paths do not call
  `Main.tile`, `Main`, `NetMessage` or `Liquid.QuickWater`;
- V1456 section encoding continues to consume `WorldSectionSnapshot` in the existing
  `Terraria.Dome.World.Protocol.Verification` project.
- The stage-boundary checkpoint is recorded in `cursor-restart-replay.json`; the restored Cave
  commit matches the uninterrupted commit in tile/section fingerprint and sequence.
- `WorldGenerationRequest` freezes an explicit `RockLayerY`; the full WLD differential supplies
  legacy `rockLayer` Y `397` rather than deriving it from the current fixture height.
- The current pipeline rejects non-default seed variant, secret-seed, difficulty, or hardmode
  rules before allocating a world. These are captured unsupported inputs, not implemented legacy
  generation semantics.

The current Simulation build succeeds with four nullable warnings in unrelated `DomeSimulation`
world-item/inventory code at lines 2742, 2763, 2784 and 2835. This WorldGen slice did not edit
that file; the warnings are retained as current build evidence rather than treated as a clean
warning-free result.

The expanded tile-model slice passed complete-dependency verification: compatibility import copies
every parsed tile state field into `WorldTile`; V1456 section encoding preserves wall/liquid/frame/
wire/shape/actuator/paint/visibility state; and persistence format v2 round-trips it. The relevant
WorldMechanics, WorldObjects, World.Protocol, World.Loopback, WorldImport and Persistence verifiers
all passed serially. The Persistence verifier also writes and reads an explicit version-1 tile
layout artifact, proving legacy active/type persistence is retained while new state defaults.

The proposal regression subset was rerun serially on 2026-08-19 with
`-p:UseSharedCompilation=false --no-build` and passed:

- `Terraria.Dome.WorldMechanics.Verification`
- `Terraria.Dome.WorldObjects.Verification`
- `Terraria.Dome.World.Protocol.Verification`
- `Terraria.Dome.World.Loopback.Verification`

This is a targeted proposal subset, not the complete repository regression suite. The gate
therefore records `proposalRegressionSubset: passed` independently. The complete solution
verification inventory was also attempted: `33` projects were run, `25` passed and `8` failed.
A serial rerun of those 8 reduced the result to `2` recovered projects and `6` stable failures.
The failures are in unrelated Combat, Load, Persistence, Player, base verification, and
WorldRules paths; the detailed output is in `Build/full-regression-20260819-rerun.log`. The
deletion gate records this as `fullRegressionSuite: failed`, so it cannot be used to justify
removing the legacy entry.

The complete source-built baseline produced readable, repeatable WLD v319 artifacts for this
fixture. The two files have different byte hashes because world-file metadata contains run-specific
state, so the evidence uses a normalized tile/entity fingerprint. The complete run record is in
`legacy-worldgen-replay.json`; the prior physical-reduced empty-world evidence is retained in
`legacy-worldgen-replay-physical-reduced.json` and related `*-physical-reduced.json` artifacts.

The baseline differential comparator is recorded in `legacy-worldgen-differential.json`. It uses
WLD metadata spawn X `2099`, `worldSurface` Y `325`, and `rockLayer` Y `397`, rather than the old
Dome fixture center and derived terrain defaults. Non-default secret-seed/difficulty/hardmode
rules are now explicit unsupported inputs, but their legacy behavior is not captured. Even under
these aligned terrain inputs, `3,190,404` of `5,040,000` tiles mismatch and the
legacy projection exposes `1,046,843` tiles with at least one extended-state mismatch when the
captured DirtWallBackgrounds offset artifact is supplied. The artifact only replays one
default-rule seed-1456/full-size random outcome; without it, the prior no-oracle comparison had
`1,050,137` extended-state mismatch tiles. The model, import projection, V1456 section encoding,
and persistence now carry wall, slope, half-brick, wiring, actuator, paint, inactive, and
visibility state, but the generation systems do not yet reproduce those values. This is valid
negative parity evidence, not a claim that the current ECS world is legacy-compatible.

This is not a legacy semantic parity claim. The Version4 source contains 684 methods and 233
fields. The current inventory contains 227 methods marked `Partial` and 457 marked `Unmapped` in
`worldgen-source-inventory.json`. The current partial set includes bounded tile wiring trap/trigger
predicates, dungeon platform/shelf and non-hammered platform support frame classification,
pressure-plate placement, atmospheric surface rules, statue style item mapping, pure candle
and picnic-table, bottle, bench, clock, bed, candelabra, bookcase, chandelier, lantern, lamp,
piano, sink, table, bathtub, workbench, chair, toilet, platform, music-box, dresser, chest,
fake-chest, and campfire style-to-item mappings, rainbow paint coordinate mapping, locked dungeon
biome chest classification, explicit pile generation attempt policy, plant placement and plant-check
decisions with atomic type/frame command intent, tile pounding eligibility, tile sloping
protection, slope commands, half-brick commands, tile merge neighbor rewrites/frame-work
decisions, moss color classification, and independent structure Tile/Wall
prepare/command transactions in addition to the ore, tree, tile-state, liquid, and replay slices
described below. The
`AreAnyTilesInSetNearby` and `IsTileNearby` source entries map only to read-only snapshot
neighborhood queries. They preserve inclusive square scans, active-tile filtering, world-edge
exclusion, and the legacy tile-235 three-column X stride. They explicitly exclude legacy nullable
Tile slots, `Main` dimensions/instrumentation, and their distinct legacy invalid-input behavior.
The three `WorldGen.InWorld` overloads map to explicit coordinate, fluff, and rectangle bounds
contracts; the `setWorldSize`, `GetWorldSize`, and `SetWorldSize` methods map fixed world-size
profiles and immutable pixel/section dimension derivation without mutating legacy global state.
`countTiles` and `nextCount` map to an explicit bounded snapshot region probe that preserves the
legacy left-right-up-down depth-first order, maximum-count/edge termination, lava and shimmer
rejection, jungle wall mode, solid-tile filtering, and category counts. It returns local immutable
results instead of mutating legacy global counters or the `CountedTiles` dictionary. It does not
map the distinct `countDirtTiles`/`nextDirtCount` traversal or caller dependencies on static state.
`countDirtTiles` and `nextDirtCount` are separately mapped to a snapshot dirt-wall region probe.
It retains wall `2`/`59` eligibility, the four orthogonal plus four diagonal plus two-column X
neighbors, solid-tile branch blocking, and ice/forbidden-wall/world-edge maximum termination. It
returns an immutable local result and excludes the legacy global counters, nullable Tile slots,
and downstream generation placement decisions.
The `SolidTile` value/coordinate overloads, `TileEmpty`, `TileType`, and both
`SolidOrSlopedTile` overloads map to `TileStateQuery`. The query uses immutable tile definitions,
preserves inactive, half-brick, slope, platform, and no-doors behavior, and returns `-1` for an
inactive tile type. It explicitly excludes the legacy nullable Tile-as-solid fallback, swallowed
exceptions, `Main.tileSolid` arrays, `Point` adapters, and mutable global timing.
`SolidTile2` (value and coordinate forms), `PlatformProperTopFrame`, and
`SolidTileAllowBottomSlope` extend the same contract with their distinct platform and slope rules:
platform top slopes are admitted by `SolidTile2`; valid frame columns are `0..7`, `12..16`, and
`25..26`; and the bottom-slope predicate returns true beyond snapshot bounds. Legacy nullable
Tile fallbacks, static TileID/Main arrays, exceptions, and the remaining slope/no-platform/edge
predicate family remain outside this mapping.
`SolidTileNoPlatforms` plus the top/left/right slope predicates are now also snapshot queries.
They preserve the legacy solid/platform distinction, directional slope exclusions, top-slope
platform half-brick exception, and the no-platform predicate's conservative out-of-world result.
The `Top/Right/Left/BottomEdgeCanBeAttachedTo` methods remain unmapped because their source
requires the legacy `tileNoAttach` definition set, which is not represented in the current tile
registry.
`SolidTile3` value and coordinate forms are now snapshot predicates. They retain the legacy
active/non-inactive/non-platform solid rule; the coordinate form preserves its one-tile fluff
boundary and returns false outside it. Nullable legacy Tile behavior and global static tile arrays
remain explicitly excluded.
`WorldGen.GenerateWorld`
at source line `10108` maps only the deterministic request-to-snapshot pipeline to
`WorldGenerationPipeline.Generate`. Its inventory record explicitly excludes legacy `Main` state,
pass configuration, secret-seed/options, `clearWorld`/`Reset`/`Finish`, save, audio and callbacks.
`WorldGen.AddPasses` at source line `10553` is `Partial`: it maps the isolated
`DirtWallBackgrounds` neighbor predicate and wall-only command slice. The captured `seed=1456`
default-rule artifact has `4,198` ordered offsets and SHA-256
`A4AB031A512188153A9EED1CE5CE2047F05BC474D22D351F973ADE9A06ACAD5E`; it conditionally enables
the pipeline slice and reduces `WallType` mismatches by `3,372` without changing any other
extended-field mismatch count. It still excludes legacy pass order, `GenVars`, every other
`UnifiedRandom` state, and all non-captured rule/world-size inputs. `EmptyLiquid` and
`PlaceLiquid` are also `Partial`: their bounded ECS targets cover typed liquid
command validation/commit and, for placement, resumable propagation through
`LiquidPropagationSession`; they exclude legacy `Main.tile`, frame/network side effects, global
Liquid queues, and compatibility tile placement. `GetLiquidChangeType` is `Partial`: it maps only
the four-liquid pair table to `LiquidInteractionKind`, excluding legacy protocol enum projection,
Tile mutation, and network notification. The remaining `559` methods and all `233` fields remain
`Unmapped`. The current systems cover only the explicitly implemented profiles and return explicit
failures for unsupported biomes/cave profiles or invalid commands.

## Required next oracle

To close the parity portion of the deletion gate, capture and implement all remaining legacy
generator inputs, especially secret-seed variants, difficulty, evil and world options; expand the
`WorldTile` projection for fields required by the target mode; then compare the complete baseline
section by section. Update inventory statuses with source-line provenance. Until those conditions
are met, the old `WorldGen` entry remains a compatibility concern and is not removed.
