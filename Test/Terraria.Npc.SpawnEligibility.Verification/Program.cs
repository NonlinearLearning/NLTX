using System;
using System.Collections.Generic;
using Terraria.Content;
using Terraria.Npc;
using Terraria.Npc.Queries;
using Terraria.WorldStorage;
using StorageNpcSlot = Terraria.WorldStorage.NpcSlot;

if (args.Length == 1 && args[0] == "--spawn-definition-resolution-checks-only")
{
  RunSpawnDefinitionResolutionCases();
  return;
}

if (args.Length == 1 && args[0] == "--spawn-tower-selection-checks-only")
{
  RunSpawnTowerSelectionCases();
  return;
}

if (args.Length == 1 && args[0] == "--spawn-branch-selection-core-checks-only")
{
  NpcSpawnBranchSelectionCoreChecks.Run();
  return;
}

if (args.Length == 1 && args[0] == "--spawn-branch-dispatch-core-checks-only")
{
  NpcSpawnBranchSelectionCoreChecks.RunDispatchCore();
  return;
}

if (args.Length == 1 && args[0] == "--spawn-candidate-checks-only")
{
  RunSpawnCandidateCheckCases();
  return;
}

if (args.Length == 1 && args[0] == "--spawn-per-player-flags-checks-only")
{
  RunPerPlayerSpawnFlagCases();
  return;
}

if (args.Length == 1 && args[0] == "--spawn-type-resolution-checks-only")
{
  RunSpawnTypeResolutionCases();
  return;
}

if (args.Length == 1 && args[0] == "--spawn-natural-order-checks-only")
{
  RunNaturalSpawnOrderingCases();
  return;
}

if (args.Length == 1 && args[0] == "--spawn-slot-protection-checks-only")
{
  RunSpawnSlotProtectionCases();
  return;
}

if (args.Length == 1 && args[0] == "--specified-slot-allocation-checks-only")
{
  RunSpecifiedSlotAllocationCases();
  return;
}

static void AssertReason(
  NpcSpawnPlayerEligibilitySnapshot snapshot,
  NpcSpawnPlayerEligibilityReason expectedReason,
  string message)
{
  NpcSpawnPlayerEligibilityResult result = NpcSpawnEligibilityQuery.Evaluate(in snapshot);
  if (result.Reason != expectedReason)
  {
    throw new InvalidOperationException(
      $"{message}: expected {expectedReason}, got {result.Reason}.");
  }
}

static void RunSpawnDefinitionResolutionCases()
{
  NpcDefinitionCatalog definitions = new(
  [
    new NpcDefinition(
      typeId: 614,
      netId: 614,
      persistentId: "npc:614",
      defaultLifeMax: 100,
      defaultDamage: 10,
      defaultDefense: 2,
      width: 24,
      height: 18,
      aiStyle: 1,
      isTownNpc: false,
      friendly: false,
      hostile: true),
    new NpcDefinition(
      typeId: 1,
      netId: -4,
      persistentId: "npc:gold-slime",
      defaultLifeMax: 25,
      defaultDamage: 7,
      defaultDefense: 2,
      width: 24,
      height: 18,
      aiStyle: 1,
      isTownNpc: false,
      friendly: false,
      hostile: true),
  ]);

  NpcSpawnPreCommitResult goodWorldRemap = CreatePreCommitResult(
    requestedType: 46,
    preparedType: 46,
    resolvedType: 614,
    hasSlot: true);
  NpcSpawnDefinitionResolutionResult positiveResolution =
    NpcSpawnDefinitionResolutionSystem.Resolve(in goodWorldRemap, definitions);
  if (!positiveResolution.IsResolved ||
    positiveResolution.ResolvedRequest.Type.Value != 614 ||
    positiveResolution.Definition?.TypeId != 614)
  {
    throw new InvalidOperationException(
      "NPC spawn must resolve definitions by the final positive type ID.");
  }

  NpcSpawnPreCommitResult negativeVariant = CreatePreCommitResult(
    requestedType: 1,
    preparedType: -4,
    resolvedType: -4,
    hasSlot: true);
  NpcSpawnDefinitionResolutionResult negativeResolution =
    NpcSpawnDefinitionResolutionSystem.Resolve(in negativeVariant, definitions);
  if (!negativeResolution.IsResolved ||
    negativeResolution.ResolvedRequest.Type.Value != -4 ||
    negativeResolution.Definition?.NetId != -4)
  {
    throw new InvalidOperationException(
      "Negative NPC net-ID variants must resolve through the net-ID catalog path.");
  }

  NpcSpawnPreCommitResult noSlot = CreatePreCommitResult(
    requestedType: 46,
    preparedType: 46,
    resolvedType: 99999,
    hasSlot: false);
  NpcSpawnDefinitionResolutionResult noSlotResolution =
    NpcSpawnDefinitionResolutionSystem.Resolve(in noSlot, definitions);
  if (noSlotResolution.Status != NpcSpawnDefinitionResolutionStatus.NoSlotAvailable ||
    noSlotResolution.Definition is not null)
  {
    throw new InvalidOperationException(
      "Definition lookup must stop when pre-commit found no available slot.");
  }

  NpcSpawnPreCommitResult missingDefinition = CreatePreCommitResult(
    requestedType: 46,
    preparedType: 46,
    resolvedType: 99999,
    hasSlot: true);
  NpcSpawnDefinitionResolutionResult missingResolution =
    NpcSpawnDefinitionResolutionSystem.Resolve(in missingDefinition, definitions);
  if (missingResolution.Status != NpcSpawnDefinitionResolutionStatus.DefinitionNotFound ||
    missingResolution.Definition is not null)
  {
    throw new InvalidOperationException(
      "An unavailable NPC definition must not produce a resolved spawn result.");
  }

  Console.WriteLine("PASS: NPC spawn definition lookup by final type and net ID");
}

static void RunSpawnTowerSelectionCases()
{
  NpcSpawnEventAndTowerEligibilitySnapshot eventAndTower = new(
    ZoneTowerSolar: true,
    ZoneTowerVortex: false,
    ZoneTowerNebula: false,
    ZoneTowerStardust: false,
    ZoneOldOneArmy: false,
    ZoneWaterCandle: false,
    ZonePeaceCandle: false,
    ZoneShadowCandle: false);
  NpcSpawnTowerSelectionInputs inputs = new(
    eventAndTower,
    SpawnTileX: 12,
    SpawnTileY: 34);
  RecordingNpcSpawnTowerSelectionPort port = new(
    [0, 3, 1],
    new Dictionary<int, int>
    {
      [518] = 2,
      [412] = 1,
    });

  NpcSpawnTowerSelectionResult result =
    NpcSpawnTowerSelectionSystem.Select(in inputs, port);
  if (!result.HasRequest || result.SpawnRequest is not { } request ||
    request.Type.Value != 419 ||
    request.PositionX != 200 ||
    request.PositionY != 544 ||
    request.StartIndex != 1 ||
    request.Ai0 != 0f || request.Ai1 != 0f ||
    request.Ai2 != 0f || request.Ai3 != 0f ||
    request.Target != 255)
  {
    throw new InvalidOperationException(
      "Solar tower selection must return the expected pre-commit NPC request.");
  }

  if (!port.RandomBounds.SequenceEqual([7, 7, 7]) ||
    !port.PopulationQueries.SequenceEqual([518, 412]))
  {
    throw new InvalidOperationException(
      "Solar tower selection must preserve weighted reroll and selected-type count order.");
  }

  Console.WriteLine("PASS: NPC solar tower weighted selection and population rerolls");
}

static NpcSpawnPreCommitResult CreatePreCommitResult(
  int requestedType,
  int preparedType,
  int resolvedType,
  bool hasSlot)
{
  NpcSpawnEntityRequest request = new(
    Type: new NpcTypeId(requestedType),
    PositionX: 120,
    PositionY: 80,
    StartIndex: 0,
    Ai0: 1f,
    Ai1: 2f,
    Ai2: 3f,
    Ai3: 4f,
    Target: 255);
  NpcSpawnEntityRequest preparedRequest = request with
  {
    Type = new NpcTypeId(preparedType),
  };
  NpcSpawnEntityPreparationResult entityPreparation = new(
    request,
    new NpcTypeId(requestedType),
    preparedRequest,
    CommonVariantRollConsumed: false,
    AnniversaryVariantRollConsumed: false);
  NpcSpawnTypeResolutionResult typeResolution = new(
    new NpcTypeId(preparedType),
    new NpcTypeId(resolvedType),
    GoodWorldRollConsumed: false,
    GoodWorldRollPassed: false);
  NpcSpawnSlotSelectionResult slotSelection = hasSlot
    ? new NpcSpawnSlotSelectionResult(2, UsedReplacementFallback: false)
    : new NpcSpawnSlotSelectionResult(null, UsedReplacementFallback: false);
  NpcSpawnSlotAcquisitionResult slotAcquisition = new(
    typeResolution,
    new NpcTypeId(resolvedType),
    slotSelection);
  return new NpcSpawnPreCommitResult(entityPreparation, slotAcquisition);
}

AssertReason(
  new NpcSpawnPlayerEligibilitySnapshot(
    IsPlayerActive: false,
    IsPlayerDead: true,
    IsJourneyMode: true,
    IsSpawnRatePowerUnlocked: true,
    DoesSpawnRatePowerDisablePlayer: true,
    IsNearMoonLord: true),
  NpcSpawnPlayerEligibilityReason.PlayerInactive,
  "Inactive player rejection must precede later gates");

AssertReason(
  new NpcSpawnPlayerEligibilitySnapshot(
    IsPlayerActive: true,
    IsPlayerDead: true,
    IsJourneyMode: false,
    IsSpawnRatePowerUnlocked: false,
    DoesSpawnRatePowerDisablePlayer: false,
    IsNearMoonLord: false),
  NpcSpawnPlayerEligibilityReason.PlayerDead,
  "Dead player rejection");

AssertReason(
  new NpcSpawnPlayerEligibilitySnapshot(
    IsPlayerActive: true,
    IsPlayerDead: false,
    IsJourneyMode: true,
    IsSpawnRatePowerUnlocked: true,
    DoesSpawnRatePowerDisablePlayer: true,
    IsNearMoonLord: false),
  NpcSpawnPlayerEligibilityReason.JourneySpawnRateDisabled,
  "Unlocked Journey spawn-rate power rejection");

NpcSpawnPlayerEligibilityResult journeyUnlockedButAllowed =
  NpcSpawnEligibilityQuery.Evaluate(
    new NpcSpawnPlayerEligibilitySnapshot(
      IsPlayerActive: true,
      IsPlayerDead: false,
      IsJourneyMode: true,
      IsSpawnRatePowerUnlocked: false,
      DoesSpawnRatePowerDisablePlayer: true,
      IsNearMoonLord: false));
if (!journeyUnlockedButAllowed.IsEligible)
{
  throw new InvalidOperationException(
    "A locked Journey spawn-rate power must not suppress spawning.");
}

AssertReason(
  new NpcSpawnPlayerEligibilitySnapshot(
    IsPlayerActive: true,
    IsPlayerDead: false,
    IsJourneyMode: false,
    IsSpawnRatePowerUnlocked: true,
    DoesSpawnRatePowerDisablePlayer: true,
    IsNearMoonLord: true),
  NpcSpawnPlayerEligibilityReason.NearMoonLord,
  "Nearby Moon Lord rejection outside Journey mode");

NpcSpawnPlayerEligibilityResult eligible = NpcSpawnEligibilityQuery.Evaluate(
  new NpcSpawnPlayerEligibilitySnapshot(
    IsPlayerActive: true,
    IsPlayerDead: false,
    IsJourneyMode: false,
    IsSpawnRatePowerUnlocked: false,
    DoesSpawnRatePowerDisablePlayer: false,
    IsNearMoonLord: false));
if (!eligible.IsEligible)
{
  throw new InvalidOperationException("A player passing every gate must be eligible.");
}

Console.WriteLine("PASS: NPC spawn player eligibility core cases");

NpcSpawnRateInputs defaultRateInputs =
  RecordingNpcSpawnPassPort.CreateDefaultRateInputs();
NpcSpawnRateInputs baseRateInputs = defaultRateInputs with
{
  Player = defaultRateInputs.Player with { NearbyActiveNpcSlots = 5f },
};
var baseRateRandom = new RecordingNpcSpawnRateRandomPort([], luckResult: 1);
NpcSpawnRateResult baseRate =
  NpcSpawnRateSystem.Calculate(in baseRateInputs, baseRateRandom);
if (baseRate.SpawnRate != 100 || baseRate.MaxSpawns != 5 ||
  baseRateRandom.LuckRolls.Count != 1 ||
  baseRateRandom.LuckRolls[0] != (0f, 50))
{
  throw new InvalidOperationException(
    $"Base spawn-rate mismatch: rate={baseRate.SpawnRate}, max={baseRate.MaxSpawns}, " +
    $"luck-rolls={string.Join(";", baseRateRandom.LuckRolls)}.");
}

var lowPopulationRandom = new RecordingNpcSpawnRateRandomPort([], luckResult: 1);
NpcSpawnRateResult lowPopulationRate =
  NpcSpawnRateSystem.Calculate(in defaultRateInputs, lowPopulationRandom);
if (lowPopulationRate.SpawnRate != 60 || lowPopulationRate.MaxSpawns != 5)
{
  throw new InvalidOperationException(
    "Zero nearby NPC slots must apply the legacy 0.6 spawn-rate multiplier.");
}

NpcSpawnRateInputs hardModeRateInputs = baseRateInputs with
{
  World = baseRateInputs.World with { HardMode = true },
};
var hardModeRateRandom = new RecordingNpcSpawnRateRandomPort([], luckResult: 1);
NpcSpawnRateResult hardModeRate =
  NpcSpawnRateSystem.Calculate(in hardModeRateInputs, hardModeRateRandom);
if (hardModeRate.SpawnRate != 90 || hardModeRate.MaxSpawns != 6)
{
  throw new InvalidOperationException(
    "Hardmode spawn rate must retain legacy integer truncation and cap increment.");
}

NpcSpawnRateInputs townRateInputs = baseRateInputs with
{
  Policy = baseRateInputs.Policy with { TownNpcCount = 1 },
  Player = baseRateInputs.Player with
  {
    CenterY = (baseRateInputs.World.UnderworldLayer * 16f) + 16f,
  },
};
var townRateRandom = new RecordingNpcSpawnRateRandomPort([0, 0], luckResult: 1);
NpcSpawnRateResult townRate =
  NpcSpawnRateSystem.Calculate(in townRateInputs, townRateRandom);
if (townRate.SpawnRate != 100 || townRate.MaxSpawns != 2 ||
  !townRate.NoWorms || !townRate.SpawnFriendly ||
  !townRateRandom.NextBounds.SequenceEqual([2, 10]) ||
  townRateRandom.NextDrawCount != 2 ||
  townRateRandom.LuckRolls.Count != 0)
{
  throw new InvalidOperationException(
    "Town-NPC rate selection must preserve its ordered random draws and short circuit.");
}

Console.WriteLine("PASS: NPC spawn-rate defaults, hardmode, and town random ordering");

NpcSpawnAreaInputs zoomAreaInputs = new(
  ScreenWidthPixels: 1920,
  ScreenHeightPixels: 1200,
  PlayerTileX: 500,
  PlayerTileY: 500,
  SelectedItemType: 1254,
  PlayerScope: true,
  DualDungeonsSeed: true,
  ZoneOverworldHeight: false,
  ZoneSkyHeight: false,
  MaxTilesX: 1000,
  MaxTilesY: 1000);
NpcSpawnAreaResult zoomArea = NpcSpawnAreaQuery.Calculate(in zoomAreaInputs);
if (zoomArea.SpawnArea != new NpcSpawnTileRectangle(368, 418, 264, 164) ||
  zoomArea.SafeArea != new NpcSpawnTileRectangle(445, 466, 110, 69) ||
  zoomArea.SafeRangeX != 110 || zoomArea.SafeRangeY != 69)
{
  throw new InvalidOperationException(
    $"Zoomed spawn area mismatch: spawn={zoomArea.SpawnArea}, safe={zoomArea.SafeArea}.");
}

NpcSpawnAreaInputs clampedAreaInputs = zoomAreaInputs with
{
  PlayerTileX = 5,
  PlayerTileY = 5,
  SelectedItemType = 0,
  PlayerScope = false,
  DualDungeonsSeed = false,
};
NpcSpawnAreaResult clampedArea =
  NpcSpawnAreaQuery.Calculate(in clampedAreaInputs);
if (clampedArea.SpawnArea.X != 0 || clampedArea.SpawnArea.Y != 0 ||
  clampedArea.SpawnArea.Right != 89 || clampedArea.SpawnArea.Bottom != 57)
{
  throw new InvalidOperationException(
    $"Spawn area must clamp each edge to the world: {clampedArea.SpawnArea}.");
}

Console.WriteLine("PASS: NPC spawn area zoom, dual-dungeon safe bounds, and world clamp");

NpcSpawnRateInputs defaultTileSearchRateInputs =
  RecordingNpcSpawnPassPort.CreateDefaultRateInputs();
NpcSpawnRateInputs tileSearchRateInputs = defaultTileSearchRateInputs with
{
  Context = defaultTileSearchRateInputs.Context with
  {
    SpawnSpaceX = 2,
    SpawnSpaceY = 3,
  },
  World = defaultTileSearchRateInputs.World with { WorldSurface = 150 },
};
NpcSpawnRateResult tileSearchRateResult = new(
  SpawnRate: 100,
  MaxSpawns: 5,
  NoWorms: false,
  SpawnFriendly: false);
int checkedSpawnCells = 0;
var stickySkyPort = new RecordingNpcSpawnPassPort(
  CreatePlayerSnapshots(),
  slimeRainActive: false,
  areaInputs: CreateSmallSpawnAreaInputs(),
  spawnTileRandomValues: [43, 43, 43, 53],
  activeSolidTile: (x, y) => x == 43 && y == 54,
  tileSpaceFacts: (_, _) => new NpcSpawnTileSpaceFacts(
    IsActive: checkedSpawnCells++ == 0,
    IsSolid: true,
    HasAnyLava: false));
NpcSpawnAreaInputs smallAreaInputs = CreateSmallSpawnAreaInputs();
NpcSpawnTileSearchResult stickySkyResult = NpcSpawnTileSearchSystem.Find(
  in smallAreaInputs,
  in tileSearchRateInputs,
  in tileSearchRateResult,
  stickySkyPort);
string[] expectedTileChecks =
[
  "solid:43:43",
  "wall:43:43",
  "space:42:40",
  "solid:43:53",
  "wall:43:53",
  "solid:43:53",
  "solid:43:54",
  "space:42:51",
  "space:42:52",
  "space:42:53",
  "space:43:51",
  "space:43:52",
  "space:43:53",
];
if (!stickySkyResult.Found || stickySkyResult.TileX != 43 ||
  stickySkyResult.TileY != 54 || !stickySkyResult.SkyMob ||
  stickySkyResult.XRange ||
  !stickySkyPort.SpawnTileRangeBounds.SequenceEqual(
    [(43, 57), (43, 57), (43, 57), (43, 57)]) ||
  !stickySkyPort.TileCalls.SequenceEqual(expectedTileChecks))
{
  throw new InvalidOperationException(
    $"Tile search order or sticky sky flag differed: {stickySkyResult}; " +
    $"checks={string.Join(",", stickySkyPort.TileCalls)}.");
}

NpcSpawnAreaInputs nearSkyAreaInputs = smallAreaInputs with
{
  PlayerTileX = 500,
  PlayerTileY = 500,
};
NpcSpawnRateInputs nearSkyRateInputs = tileSearchRateInputs with
{
  World = tileSearchRateInputs.World with
  {
    HardMode = true,
    WorldSurface = 1200,
  },
};
var nearSkyPort = new RecordingNpcSpawnPassPort(
  CreatePlayerSnapshots(),
  slimeRainActive: false,
  areaInputs: nearSkyAreaInputs,
  spawnTileRandomValues: [493, 493]);
NpcSpawnTileSearchResult nearSkyResult = NpcSpawnTileSearchSystem.Find(
  in nearSkyAreaInputs,
  in nearSkyRateInputs,
  in tileSearchRateResult,
  nearSkyPort);
if (!nearSkyResult.Found || !nearSkyResult.SkyMob ||
  nearSkyResult.TileX != 493 || nearSkyResult.TileY != 493 ||
  !nearSkyPort.SpawnRateBounds.SequenceEqual([10]) ||
  !nearSkyPort.SpawnTileRangeBounds.SequenceEqual([(493, 507), (493, 507)]))
{
  throw new InvalidOperationException(
    "The hardmode near-sky branch must consume its 1-in-10 roll after x/y draws.");
}

var rejectedTilePort = new RecordingNpcSpawnPassPort(
  CreatePlayerSnapshots(),
  slimeRainActive: false,
  areaInputs: smallAreaInputs,
  activeSolidTile: (_, _) => true);
NpcSpawnTileSearchResult noTileResult = NpcSpawnTileSearchSystem.Find(
  in smallAreaInputs,
  in tileSearchRateInputs,
  in tileSearchRateResult,
  rejectedTilePort);
if (noTileResult.Found || noTileResult.TileX != 0 || noTileResult.TileY != 0 ||
  noTileResult.XRange || rejectedTilePort.SpawnTileRangeBounds.Count != 100 ||
  rejectedTilePort.TileCalls.Count != 50 ||
  rejectedTilePort.TileCalls.Any(call => call.StartsWith("wall:", StringComparison.Ordinal)))
{
  throw new InvalidOperationException(
    "Tile search must stop after 50 blocked samples and preserve short-circuit wall reads.");
}

Console.WriteLine("PASS: NPC spawn-area and ordered tile-search behavior");

NpcSpawnPlayerEligibilitySnapshot[] capacitySnapshots = CreatePlayerSnapshots();
capacitySnapshots[0] = CreatePlayerSnapshot();
NpcSpawnRateInputs capacityRateInputs = defaultRateInputs with
{
  Player = defaultRateInputs.Player with { NearbyActiveNpcSlots = 5f },
};
var capacityPort = new RecordingNpcSpawnPassPort(
  capacitySnapshots,
  slimeRainActive: false,
  rateInputs: capacityRateInputs);
var capacitySystem = new NpcSpawnSystem(capacityPort);
capacitySystem.ProcessNaturalSpawnPass();
if (!capacityPort.Calls.Contains("rate:0") ||
  capacityPort.Calls.Contains("continue:0") ||
  capacityPort.Calls.Contains("area:0") ||
  capacityPort.SpawnRateBounds.Count != 0)
{
  throw new InvalidOperationException(
    "The capacity gate must precede the natural spawn-rate roll and continuation.");
}

Console.WriteLine("PASS: NPC natural-spawn capacity gate");

RunNaturalSpawnOrderingCases();

NpcSpawnPlayerEligibilitySnapshot[] fullPassSnapshots = CreatePlayerSnapshots();
for (var index = 0; index < fullPassSnapshots.Length; index++)
{
  fullPassSnapshots[index] = CreatePlayerSnapshot();
}

var fullPassPort = new RecordingNpcSpawnPassPort(
  fullPassSnapshots,
  slimeRainActive: false,
  spawnRateRollValues: Enumerable.Repeat(1, 255).ToArray());
var fullPassSystem = new NpcSpawnSystem(fullPassPort);
int? noLoopControlPlayer = fullPassSystem.ProcessNaturalSpawnPass();
int playerCaptureCount = fullPassPort.Calls.Count(
  call => call.StartsWith("capture:", StringComparison.Ordinal));
int flagPreludeCount = fullPassPort.Calls.Count(
  call => call.StartsWith("flags-prelude:", StringComparison.Ordinal));
int invasionInputCaptureCount = fullPassPort.Calls.Count(
  call => call.StartsWith("invasion-inputs:", StringComparison.Ordinal));
int flagPostludeCount = fullPassPort.Calls.Count(
  call => call.StartsWith("flags-postlude:", StringComparison.Ordinal));
int rateCaptureCount = fullPassPort.Calls.Count(
  call => call.StartsWith("rate:", StringComparison.Ordinal));
int areaCaptureCount = fullPassPort.Calls.Count(
  call => call.StartsWith("area:", StringComparison.Ordinal));
if (noLoopControlPlayer.HasValue || playerCaptureCount != 255 ||
  flagPreludeCount != 255 || invasionInputCaptureCount != 255 ||
  flagPostludeCount != 255 || rateCaptureCount != 255 || areaCaptureCount != 0)
{
  throw new InvalidOperationException(
    "A rejected pass must inspect slots, prepare flags, and capture rates without tile search.");
}

Console.WriteLine("PASS: NPC natural-spawn full 255-slot rejection pass");

NpcSpawnPlayerEligibilitySnapshot[] liveSnapshots = CreatePlayerSnapshots();
liveSnapshots[0] = CreatePlayerSnapshot();
liveSnapshots[1] = CreatePlayerSnapshot();
var liveSlimeRainPort = new RecordingNpcSpawnPassPort(
  liveSnapshots,
  slimeRainActive: true,
  disableSlimeRainAfterPlayer: 0);
var liveSlimeRainSystem = new NpcSpawnSystem(liveSlimeRainPort);
liveSlimeRainSystem.ProcessNaturalSpawnPass();
if (!liveSlimeRainPort.Calls.Contains("slime:0") ||
  liveSlimeRainPort.Calls.Contains("slime:1") ||
  liveSlimeRainPort.Calls.IndexOf("slime:0") >=
    liveSlimeRainPort.Calls.IndexOf("flags-prelude:0") ||
  liveSlimeRainPort.Calls.IndexOf("flags-prelude:0") >=
    liveSlimeRainPort.Calls.IndexOf("rate:0"))
{
  throw new InvalidOperationException(
    "Per-player flags must be prepared after slime-rain effects and before rate capture.");
}

Console.WriteLine("PASS: NPC natural-spawn reads live slime-rain state");

static void RunNaturalSpawnOrderingCases()
{
  NpcSpawnPlayerEligibilitySnapshot[] orderedSnapshots = CreatePlayerSnapshots();
  orderedSnapshots[0] = CreatePlayerSnapshot(isPlayerActive: false);
  orderedSnapshots[1] = CreatePlayerSnapshot();
  orderedSnapshots[2] = CreatePlayerSnapshot();

  var orderedPort = new RecordingNpcSpawnPassPort(
    orderedSnapshots,
    slimeRainActive: true,
    spawnRateRollValues: [1, 0]);
  var orderedSystem = new NpcSpawnSystem(orderedPort);
  int? loopControlPlayer = orderedSystem.ProcessNaturalSpawnPass();
  if (loopControlPlayer != 2)
  {
    throw new InvalidOperationException(
      "The natural-spawn pass must stop at the first true loop-control result.");
  }

  string[] expectedCalls =
  [
    "capture:0",
    "capture:1",
    "slime:1",
    "flags-prelude:1",
    "invasion-inputs:1",
    "flags-postlude:1",
    "rate:1",
    "capture:2",
    "slime:2",
    "flags-prelude:2",
    "invasion-inputs:2",
    "flags-postlude:2",
    "rate:2",
    "area:2",
    "screen",
    "post:2",
    "chosen-world:116:48:0",
    "continue:2",
  ];
  if (!orderedPort.Calls.SequenceEqual(expectedCalls))
  {
    throw new InvalidOperationException(
      $"Natural-spawn ordering differed: {string.Join(",", orderedPort.Calls)}.");
  }

  Console.WriteLine("PASS: NPC natural-spawn player ordering and first-true break");
}

static void RunSpawnSlotProtectionCases()
{
  bool[] activeSlots = [true, false, false];
  int[] protectionTicks = [5, 2, 0];
  List<string> calls = [];
  RecordingNpcSpawnSlotProtectionPort port = new(activeSlots, protectionTicks, calls);

  NpcSpawnSlotProtectionSystem.AdvanceTick(port);

  if (protectionTicks[0] != 2 || protectionTicks[1] != 1 || protectionTicks[2] != 0)
  {
    throw new InvalidOperationException(
      "Active slots must refresh to two ticks and inactive slots must decay to a zero floor.");
  }

  string expectedAdvanceOrder =
    "active:0,write:0:2,active:1,read:1,write:1:1,active:2,read:2,write:2:0";
  if (string.Join(",", calls) != expectedAdvanceOrder)
  {
    throw new InvalidOperationException(
      "Slot protection must preserve slot order and skip prior protection reads for active NPCs.");
  }

  calls.Clear();
  NpcSpawnSlotProtectionSystem.ProtectSelectedSlot(1, port);
  if (protectionTicks[1] != 2 || string.Join(",", calls) != "write:1:2")
  {
    throw new InvalidOperationException(
      "ProtectSelectedSlot must write the protected tick count.");
  }

  calls.Clear();
  NpcSpawnSlotProtectionSystem.ResetSlotProtection(2, port);
  if (protectionTicks[2] != 0 || string.Join(",", calls) != "write:2:0")
  {
    throw new InvalidOperationException(
      "ResetSlotProtection must clear a stale protection value.");
  }

  NpcSpawnSlotFact[] replaceableSlotFacts =
  [
    new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
    new(
      IsActive: false,
      SpawnSlotProtection: 1,
      CanBeReplacedByOtherNPCs: true,
      Generation: 17),
    new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
  ];
  calls.Clear();
  NpcSpawnSlotSelectionResult selectedAndProtected =
    NpcSpawnSlotSelectionSystem.SelectAndProtect(
      replaceableSlotFacts,
      startIndex: 0,
      searchInReverse: false,
      cannotSpawnInSlot0: false,
      protectionPort: port);
  if (selectedAndProtected.SlotIndex != 1 ||
    !selectedAndProtected.UsedReplacementFallback ||
    selectedAndProtected.ExpectedGeneration != 17 ||
    protectionTicks[1] != 2 ||
    string.Join(",", calls) != "write:1:2")
  {
    throw new InvalidOperationException(
      "Selecting a replaceable NPC slot must preserve its generation and protect it.");
  }

  NpcSpawnSlotSelectionResult replacementWithoutGeneration = NpcSpawnSlotSelectionQuery.Select(
    [
      new(IsActive: false, SpawnSlotProtection: 1, CanBeReplacedByOtherNPCs: true),
    ],
    startIndex: 0,
    searchInReverse: false,
    cannotSpawnInSlot0: false);
  if (replacementWithoutGeneration.SlotIndex != 0 ||
    replacementWithoutGeneration.ExpectedGeneration is not null ||
    !replacementWithoutGeneration.Found ||
    replacementWithoutGeneration.IsCommitReady)
  {
    throw new InvalidOperationException(
      "The pure slot query must retain an unguarded replacement candidate without marking it commit-ready.");
  }

  RecordingNpcSpawnSlotAcquisitionPort unguardedReplacementPort = new(
    [
      new(
        IsActive: false,
        SpawnSlotProtection: 1,
        CanBeReplacedByOtherNPCs: true),
    ],
    randomValues: [],
    searchInReverse: false,
    cannotSpawnInSlot0: false);
  NpcSpawnSlotAcquisitionResult unguardedReplacement =
    NpcSpawnSlotAcquisitionSystem.Acquire(
      requestedType: new NpcTypeId(1),
      isGoodWorld: false,
      startIndex: 0,
      port: unguardedReplacementPort);
  string expectedUnguardedReplacementOrder =
    "from-net-id:1,reverse:1,slot-zero:1,capture-slot-facts";
  if (unguardedReplacement.Found ||
    unguardedReplacement.SlotSelection.SlotIndex != 0 ||
    unguardedReplacement.SlotSelection.IsCommitReady ||
    string.Join(",", unguardedReplacementPort.Calls) != expectedUnguardedReplacementOrder)
  {
    throw new InvalidOperationException(
      "A replacement without a generation guard must not protect or expose a commit-ready slot.");
  }

  NpcSpawnSlotFact[] unavailableSlotFacts =
  [
    new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
    new(IsActive: false, SpawnSlotProtection: 1, CanBeReplacedByOtherNPCs: false),
    new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
  ];
  calls.Clear();
  NpcSpawnSlotSelectionResult noAvailableSlot =
    NpcSpawnSlotSelectionSystem.SelectAndProtect(
      unavailableSlotFacts,
      startIndex: 0,
      searchInReverse: false,
      cannotSpawnInSlot0: false,
      protectionPort: port);
  if (noAvailableSlot.Found || calls.Count != 0)
  {
    throw new InvalidOperationException(
      "A failed NPC slot selection must not write slot protection.");
  }

  NpcSpawnSlotSelectionResult reverseSlotZeroRestricted =
    NpcSpawnSlotSelectionQuery.Select(
      [
        new(IsActive: false, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
        new(
          IsActive: true,
          SpawnSlotProtection: 0,
          CanBeReplacedByOtherNPCs: true,
          Generation: 9),
        new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
        new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
      ],
      startIndex: 0,
      searchInReverse: true,
      cannotSpawnInSlot0: true);
  if (reverseSlotZeroRestricted.Found)
  {
    throw new InvalidOperationException(
      "Reverse slot search must keep the normalized start index as an exclusive boundary.");
  }

  bool rejectedMismatchedCapacity = false;
  try
  {
    NpcSpawnSlotSelectionSystem.SelectAndProtect(
      replaceableSlotFacts.AsSpan(0, 1),
      startIndex: 0,
      searchInReverse: false,
      cannotSpawnInSlot0: false,
      protectionPort: port);
  }
  catch (ArgumentException)
  {
    rejectedMismatchedCapacity = true;
  }

  if (!rejectedMismatchedCapacity || calls.Count != 0)
  {
    throw new InvalidOperationException(
      "Mismatched slot facts and protection capacities must be rejected before any write.");
  }

  RecordingNpcSpawnSlotAcquisitionPort acquisitionPort = new(
    [
      new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
      new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
      new(IsActive: false, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
    ],
    randomValues: [1],
    searchInReverse: true,
    cannotSpawnInSlot0: true);
  NpcSpawnSlotAcquisitionResult acquisition = NpcSpawnSlotAcquisitionSystem.Acquire(
    requestedType: new NpcTypeId(46),
    isGoodWorld: true,
    startIndex: 0,
    port: acquisitionPort);
  string expectedAcquisitionOrder =
    "random:3,from-net-id:614,reverse:614,slot-zero:614,capture-slot-facts,write:2:2";
  if (acquisition.TypeResolution.ResolvedType.Value != 614 ||
    acquisition.SlotMetadataType.Value != 614 ||
    acquisition.SlotSelection.SlotIndex != 2 ||
    acquisition.SlotSelection.UsedReplacementFallback ||
    string.Join(",", acquisitionPort.Calls) != expectedAcquisitionOrder)
  {
    throw new InvalidOperationException(
      $"NPC slot acquisition order differed: {string.Join(",", acquisitionPort.Calls)}.");
  }

  RecordingNpcSpawnSlotAcquisitionPort nonZeroStartPort = new(
    [
      new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
      new(IsActive: false, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
      new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
    ],
    randomValues: [],
    searchInReverse: false,
    cannotSpawnInSlot0: true);
  NpcSpawnSlotAcquisitionResult nonZeroStartAcquisition =
    NpcSpawnSlotAcquisitionSystem.Acquire(
      requestedType: new NpcTypeId(20),
      isGoodWorld: false,
      startIndex: 1,
      port: nonZeroStartPort);
  string expectedNonZeroStartOrder =
    "from-net-id:20,reverse:20,capture-slot-facts,write:1:2";
  if (nonZeroStartAcquisition.SlotSelection.SlotIndex != 1 ||
    string.Join(",", nonZeroStartPort.Calls) != expectedNonZeroStartOrder)
  {
    throw new InvalidOperationException(
      "A nonzero start index must skip both the GoodWorld roll and slot-zero metadata lookup.");
  }

  RecordingNpcSpawnSlotAcquisitionPort preCommitPort = new(
    [
      new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
      new(IsActive: false, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
      new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
    ],
    randomValues: [1],
    searchInReverse: false,
    cannotSpawnInSlot0: false,
    luckValues: [0, 0]);
  NpcSpawnEntityRequest originalRequest = new(
    Type: new NpcTypeId(1),
    PositionX: 120,
    PositionY: 80,
    StartIndex: 0,
    Ai0: 1f,
    Ai1: 2f,
    Ai2: 3f,
    Ai3: 4f,
    Target: 255);
  NpcSpawnTargetSelectionSnapshot defaultTarget = new(DefaultTarget: 42);
  NpcSpawnPreCommitResult preCommit = NpcSpawnPreCommitSystem.Prepare(
    in originalRequest,
    isAnniversaryWorld: true,
    isGoodWorld: true,
    targetSnapshot: in defaultTarget,
    port: preCommitPort);
  string expectedPreCommitOrder =
    "from-net-id:1,roll-luck:180,roll-luck:180,random:3,from-net-id:667," +
    "reverse:667,slot-zero:667,capture-slot-facts,write:1:2";
  NpcSpawnEntityRequest expectedPreparedRequest = originalRequest with
  {
    Type = new NpcTypeId(667),
    Target = 42,
  };
  if (preCommit.EntityPreparation.FromNetIdType.Value != 1 ||
    !preCommit.EntityPreparation.CommonVariantRollConsumed ||
    !preCommit.EntityPreparation.AnniversaryVariantRollConsumed ||
    preCommit.EntityPreparation.PreparedRequest != expectedPreparedRequest ||
    preCommit.SlotAcquisition.SlotSelection.SlotIndex != 1 ||
    string.Join(",", preCommitPort.Calls) != expectedPreCommitOrder)
  {
    throw new InvalidOperationException(
      $"NPC pre-commit preparation order differed: {string.Join(",", preCommitPort.Calls)}.");
  }

  RecordingNpcSpawnSlotAcquisitionPort regularSlimePort = new(
    [
      new(IsActive: false, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
    ],
    randomValues: [],
    searchInReverse: false,
    cannotSpawnInSlot0: false,
    luckValues: [1]);
  NpcSpawnEntityRequest regularSlimeRequest = originalRequest with { Target = 17 };
  NpcSpawnEntityPreparationResult regularSlime = NpcSpawnEntityPreparationSystem.Prepare(
    in regularSlimeRequest,
    isAnniversaryWorld: false,
    targetSnapshot: in defaultTarget,
    port: regularSlimePort);
  if (regularSlime.PreparedRequest.Type.Value != 1 ||
    regularSlime.PreparedRequest.Target != 17 ||
    !regularSlime.CommonVariantRollConsumed ||
    regularSlime.AnniversaryVariantRollConsumed ||
    string.Join(",", regularSlimePort.Calls) != "from-net-id:1,roll-luck:180")
  {
    throw new InvalidOperationException(
      "A regular-world slime must consume one variant roll and preserve an explicit target.");
  }

  Console.WriteLine("PASS: NPC slot protection ordering and lifecycle writes");
}

static void RunSpecifiedSlotAllocationCases()
{
  EntitySlotStore<object, StorageNpcSlot> slots = new(
    static slot => slot.Value,
    static value => new StorageNpcSlot(value),
    maximumCapacity: 4);
  object originalState = new();
  StorageNpcSlot selectedSlot = new(2);
  if (!slots.TryAllocateAt(selectedSlot, originalState, out uint generation) ||
    slots.Capacity != 3 ||
    slots.ActiveCount != 1)
  {
    throw new InvalidOperationException(
      "Specified-slot allocation must preserve the requested index and grow only as needed.");
  }

  if (!slots.TryGetOccupiedAt(
      2,
      out StorageNpcSlot storedSlot,
      out uint storedGeneration,
      out object? storedState) ||
    storedSlot != selectedSlot ||
    storedGeneration != generation ||
    !ReferenceEquals(storedState, originalState))
  {
    throw new InvalidOperationException(
      "Specified-slot allocation must expose the stored state and generation at that index.");
  }

  if (slots.TryAllocateAt(selectedSlot, new object(), out uint duplicateGeneration) ||
    duplicateGeneration != 0 ||
    slots.ActiveCount != 1)
  {
    throw new InvalidOperationException(
      "An occupied slot must reject allocation without changing the active count.");
  }

  object replacement = new();
  if (!slots.TryReplace(selectedSlot, generation, replacement, out uint replacementGeneration) ||
    replacementGeneration != generation + 1 ||
    slots.ActiveCount != 1 ||
    slots.TryGet(selectedSlot, generation, out _))
  {
    throw new InvalidOperationException(
      "Replacing a selected slot must advance its generation and invalidate the old handle.");
  }

  if (!slots.TryGet(selectedSlot, replacementGeneration, out object? replacementState) ||
    !ReferenceEquals(replacementState, replacement))
  {
    throw new InvalidOperationException(
      "The replacement state must be reachable through the new generation.");
  }

  if (slots.TryAllocateAt(new StorageNpcSlot(4), new object(), out uint outOfRangeGeneration) ||
    outOfRangeGeneration != 0 ||
    slots.Capacity != 3 ||
    slots.ActiveCount != 1)
  {
    throw new InvalidOperationException(
      "An index at maximum capacity must fail without growing or mutating the store.");
  }

  if (!slots.TryRelease(selectedSlot, replacementGeneration, out object? releasedState) ||
    !ReferenceEquals(releasedState, replacement) ||
    slots.ActiveCount != 0)
  {
    throw new InvalidOperationException(
      "Releasing a selected slot must return its state and decrement the active count.");
  }

  if (!slots.TryAllocateAt(selectedSlot, originalState, out uint nextGeneration) ||
    nextGeneration != replacementGeneration + 1)
  {
    throw new InvalidOperationException(
      "Reallocating a released selected slot must retain its generation history.");
  }

  Console.WriteLine("PASS: specified-slot allocation and generation guards");
}

static void RunSpawnTileSpaceQueryCases()
{
  (NpcSpawnTileSpaceFacts Facts, bool Expected)[] cases =
  [
    (new(IsActive: false, IsSolid: false, HasAnyLava: false), true),
    (new(IsActive: false, IsSolid: true, HasAnyLava: false), true),
    (new(IsActive: true, IsSolid: false, HasAnyLava: false), true),
    (new(IsActive: true, IsSolid: true, HasAnyLava: false), false),
    (new(IsActive: false, IsSolid: false, HasAnyLava: true), false),
    (new(IsActive: false, IsSolid: true, HasAnyLava: true), false),
    (new(IsActive: true, IsSolid: false, HasAnyLava: true), false),
    (new(IsActive: true, IsSolid: true, HasAnyLava: true), false),
  ];

  foreach ((NpcSpawnTileSpaceFacts facts, bool expected) in cases)
  {
    bool actual = NpcSpawnTileSpaceQuery.CanSpawn(in facts);
    if (actual != expected)
    {
      throw new InvalidOperationException(
        $"Tile spawn-space mismatch for {facts}: expected {expected}, got {actual}.");
    }
  }

  Console.WriteLine("PASS: NPC spawn tile-space solid and lava rules");
}

static void RunSpawnTypeResolutionCases()
{
  var noGoodWorldRandom = new RecordingNpcSpawnTypeResolutionRandomPort();
  NpcSpawnTypeResolutionResult unchangedManEater = NpcSpawnTypeResolutionSystem.Resolve(
    new NpcTypeId(46),
    isGoodWorld: false,
    randomPort: noGoodWorldRandom);
  if (unchangedManEater.ResolvedType.Value != 46 ||
    unchangedManEater.GoodWorldRollConsumed ||
    unchangedManEater.GoodWorldRollPassed ||
    noGoodWorldRandom.RequestedUpperBounds.Count != 0)
  {
    throw new InvalidOperationException(
      "Non-GoodWorld type resolution must not consume random values or remap the requested type.");
  }

  var rejectedRollRandom = new RecordingNpcSpawnTypeResolutionRandomPort(0);
  NpcSpawnTypeResolutionResult rejectedManEater = NpcSpawnTypeResolutionSystem.Resolve(
    new NpcTypeId(46),
    isGoodWorld: true,
    randomPort: rejectedRollRandom);
  if (rejectedManEater.ResolvedType.Value != 46 ||
    !rejectedManEater.GoodWorldRollConsumed ||
    rejectedManEater.GoodWorldRollPassed ||
    rejectedRollRandom.RequestedUpperBounds.Count != 1 ||
    rejectedRollRandom.RequestedUpperBounds[0] != 3)
  {
    throw new InvalidOperationException(
      "A zero GoodWorld roll must be consumed once and preserve the requested type.");
  }

  var manEaterRandom = new RecordingNpcSpawnTypeResolutionRandomPort(1);
  NpcSpawnTypeResolutionResult remappedManEater = NpcSpawnTypeResolutionSystem.Resolve(
    new NpcTypeId(46),
    isGoodWorld: true,
    randomPort: manEaterRandom);
  if (remappedManEater.ResolvedType.Value != 614 ||
    !remappedManEater.TypeWasRemapped ||
    !remappedManEater.GoodWorldRollConsumed ||
    !remappedManEater.GoodWorldRollPassed ||
    manEaterRandom.RequestedUpperBounds.Count != 1 ||
    manEaterRandom.RequestedUpperBounds[0] != 3)
  {
    throw new InvalidOperationException("A passing GoodWorld roll must remap NPC type 46 to 614.");
  }

  var angryTrapperRandom = new RecordingNpcSpawnTypeResolutionRandomPort(2);
  NpcSpawnTypeResolutionResult remappedAngryTrapper = NpcSpawnTypeResolutionSystem.Resolve(
    new NpcTypeId(62),
    isGoodWorld: true,
    randomPort: angryTrapperRandom);
  if (remappedAngryTrapper.ResolvedType.Value != 66 ||
    !remappedAngryTrapper.TypeWasRemapped ||
    !remappedAngryTrapper.GoodWorldRollConsumed ||
    !remappedAngryTrapper.GoodWorldRollPassed ||
    angryTrapperRandom.RequestedUpperBounds.Count != 1 ||
    angryTrapperRandom.RequestedUpperBounds[0] != 3)
  {
    throw new InvalidOperationException("A passing GoodWorld roll must remap NPC type 62 to 66.");
  }

  var unrelatedTypeRandom = new RecordingNpcSpawnTypeResolutionRandomPort(1);
  NpcSpawnTypeResolutionResult unrelatedType = NpcSpawnTypeResolutionSystem.Resolve(
    new NpcTypeId(123),
    isGoodWorld: true,
    randomPort: unrelatedTypeRandom);
  if (unrelatedType.ResolvedType.Value != 123 ||
    !unrelatedType.GoodWorldRollConsumed ||
    !unrelatedType.GoodWorldRollPassed ||
    unrelatedTypeRandom.RequestedUpperBounds.Count != 1)
  {
    throw new InvalidOperationException(
      "GoodWorld must consume its roll even when the NPC type has no remapping.");
  }

  Console.WriteLine("PASS: NPC spawn type resolution core cases");
}

static void RunPerPlayerSpawnFlagCases()
{
  NpcSpawnRateInputs rateInputs = RecordingNpcSpawnPassPort.CreateDefaultRateInputs() with
  {
    Policy = RecordingNpcSpawnPassPort.CreateDefaultRateInputs().Policy with
    {
      TownNpcCount = 9,
      SkyMob = true,
      NoWorms = false,
      NoGroundWorms = false,
      SpawnFriendly = true,
      IgnoreSafeWalls = false,
      SpawnSpider = true,
      IsSpawningInWindDirection = true,
    },
    Spatial = RecordingNpcSpawnPassPort.CreateDefaultRateInputs().Spatial with
    {
      SurfaceSpawn = true,
      SpawnUndergroundDesert = true,
      DeeperThanRockLayer = true,
      UnderGround = true,
      IsOcean = true,
      IsBeach = true,
    },
    BiomeAndDungeon = RecordingNpcSpawnPassPort.CreateDefaultRateInputs().BiomeAndDungeon with
    {
      WaterTile = true,
      NearGranite = true,
      NearMarble = true,
    },
    BiomeZones = RecordingNpcSpawnPassPort.CreateDefaultRateInputs().BiomeZones with
    {
      ZoneGranite = true,
      ZoneMarble = true,
    },
  };
  NpcSpawnPerPlayerFlagsPrelude prelude = new(
    Context: rateInputs.Context with
    {
      PlayerTileX = 12,
      PlayerTileY = 34,
      Luck = 0.25f,
      DayTime = false,
      Raining = true,
    },
    BiomeZones: rateInputs.BiomeZones with
    {
      ZoneCorrupt = true,
      ZoneCrimson = true,
      ZoneHallow = true,
      ZoneJungle = true,
      ZoneSnow = true,
      ZoneGlowshroom = true,
      ZoneMeteor = true,
      ZoneGraveyard = true,
      ZoneDungeon = true,
      ZoneLihzhardTemple = true,
      ZoneGranite = false,
      ZoneMarble = false,
      ZoneSandstorm = true,
    },
    EventAndTower: new NpcSpawnEventAndTowerEligibilitySnapshot(
      ZoneTowerSolar: true,
      ZoneTowerVortex: false,
      ZoneTowerNebula: false,
      ZoneTowerStardust: false,
      ZoneOldOneArmy: true,
      ZoneWaterCandle: true,
      ZonePeaceCandle: false,
      ZoneShadowCandle: false),
    DownedPlantBoss: true,
    HardMode: true,
    DualDungeonsSeed: true,
    PlayerInsideUnbreakableWalls: true,
    DungeonProgressCanSafelyMatch: 4,
    DungeonProgressPlayerNeedsToMatch: 5);
  NpcSpawnPerPlayerFlagsPostlude postlude = new(
    PlayerTownNpcCount: 3,
    PlayerTileIsInWorld: true,
    PlayerTileHasHouseWall: true,
    PlayerAfkCounter: 20,
    AfkTimeNeededForNoWormSpawns: 20,
    PlayerTileHasLightWall: true,
    PlayerTileWallType: 244,
    RemixWorld: true,
    PlayerCenterX: 8000f,
    MaxTilesX: 1000,
    ArmorSlot0ItemType: 0,
    ArmorSlot1ItemType: 1283,
    PlayerMaximumLife: 100);
  var calls = new List<string>();
  var port = new RecordingNpcSpawnPassPort(
    CreatePlayerSnapshots(),
    slimeRainActive: false,
    rateInputs: rateInputs,
    calls: calls,
    perPlayerFlagsPrelude: prelude,
    invasionInputs: new NpcSpawnInvasionInputs(
      InvasionType: 0,
      InvasionDelay: 0,
      InvasionSize: 0,
      PlayerPositionX: 0f,
      PlayerPositionY: 0f,
      WorldSurface: 100,
      ScreenHeightPixels: 0,
      SpawnTileY: 0,
      InvasionX: 0,
      MaxTilesX: 1000,
      MaxNpcSlots: 0),
    perPlayerFlagsPostlude: postlude);

  NpcSpawnRateInputs prepared = NpcSpawnPerPlayerFlagsSystem.Prepare(0, port);
  if (!prepared.Policy.Invaders || !prepared.Policy.IgnoreSafeWalls ||
    prepared.Policy.TownNpcCount != 3 || prepared.Policy.SkyMob ||
    !prepared.Policy.NoWorms || !prepared.Policy.NoGroundWorms ||
    prepared.Policy.SpawnFriendly || prepared.Policy.SpawnSpider ||
    !prepared.Policy.OffensiveToTim || !prepared.Policy.PlayerHasStartingHealth ||
    !prepared.Policy.IsSpawningInWindDirection)
  {
    throw new InvalidOperationException(
      "Per-player flags must reset transient policy values, apply tower overrides, and preserve " +
      "fields outside SetSpawnFlags writes.");
  }

  if (prepared.Context.PlayerTileX != 12 || prepared.Context.PlayerTileY != 34 ||
    prepared.Context.Luck != 0.25f || prepared.Context.DayTime || !prepared.Context.Raining ||
    !prepared.Spatial.HardDungeon || prepared.Spatial.SpawnUndergroundDesert ||
    !prepared.Spatial.SkyBehindPlayer || !prepared.Spatial.LivingTree ||
    !prepared.Spatial.InRemixStartingArea || !prepared.Spatial.SurfaceSpawn ||
    !prepared.Spatial.DeeperThanRockLayer || !prepared.Spatial.UnderGround ||
    !prepared.Spatial.IsOcean || !prepared.Spatial.IsBeach)
  {
    throw new InvalidOperationException(
      "Per-player flags must derive their assigned context/spatial values and preserve the " +
      "chosen-tile values not written by SetSpawnFlags.");
  }

  if (prepared.BiomeAndDungeon.WaterTile || prepared.BiomeAndDungeon.NearGranite ||
    prepared.BiomeAndDungeon.NearMarble || !prepared.BiomeAndDungeon.DualDungeonsSpawnRules ||
    !prepared.BiomeAndDungeon.InDualDungeon ||
    !prepared.BiomeAndDungeon.TresspassingDualDungeon ||
    !prepared.BiomeZones.ZoneCorrupt || !prepared.BiomeZones.ZoneCrimson ||
    !prepared.BiomeZones.ZoneHallow || !prepared.BiomeZones.ZoneJungle ||
    !prepared.BiomeZones.ZoneSnow || !prepared.BiomeZones.ZoneGlowshroom ||
    !prepared.BiomeZones.ZoneMeteor || !prepared.BiomeZones.ZoneGraveyard ||
    !prepared.BiomeZones.ZoneDungeon || !prepared.BiomeZones.ZoneLihzhardTemple ||
    !prepared.BiomeZones.ZoneSandstorm || !prepared.BiomeZones.ZoneGranite ||
    !prepared.BiomeZones.ZoneMarble || prepared.EventAndTower != prelude.EventAndTower)
  {
    throw new InvalidOperationException(
      "Per-player flags must copy legacy biome/event values, reset tile flags, and leave " +
      "ZoneGranite/ZoneMarble untouched.");
  }

  if (!calls.SequenceEqual(
    ["flags-prelude:0", "invasion-inputs:0", "flags-postlude:0", "rate:0"]))
  {
    throw new InvalidOperationException(
      $"Inactive invasion must not scan NPC slots or consume random values: {string.Join(";", calls)}.");
  }

  NpcSpawnInvasionInputs directInvasion = new(
    InvasionType: 1,
    InvasionDelay: 0,
    InvasionSize: 1,
    PlayerPositionX: 1000f,
    PlayerPositionY: 0f,
    WorldSurface: 100,
    ScreenHeightPixels: 0,
    SpawnTileY: 0,
    InvasionX: 100,
    MaxTilesX: 1000,
    MaxNpcSlots: 2);
  var directCalls = new List<string>();
  var directPort = new RecordingNpcSpawnPassPort(
    CreatePlayerSnapshots(),
    slimeRainActive: false,
    calls: directCalls,
    invasionInputs: directInvasion);
  NpcSpawnRateInputs directPrepared = NpcSpawnPerPlayerFlagsSystem.Prepare(0, directPort);
  if (!directPrepared.Policy.Invaders ||
    !directCalls.SequenceEqual(
      ["flags-prelude:0", "invasion-inputs:0", "flags-postlude:0", "rate:0"]))
  {
    throw new InvalidOperationException(
      "An invasion-zone hit must short-circuit before the NPC scan and random draw.");
  }

  NpcSpawnInvasionInputs townNpcInvasion = directInvasion with
  {
    PlayerPositionX = 20000f,
    InvasionX = 500,
    MaxNpcSlots = 3,
  };
  var failedTownNpcCalls = new List<string>();
  var failedTownNpcPort = new RecordingNpcSpawnPassPort(
    CreatePlayerSnapshots(),
    slimeRainActive: false,
    spawnRateRollValues: [0],
    calls: failedTownNpcCalls,
    invasionInputs: townNpcInvasion,
    invasionTownNpcSlots: [false, true, true],
    invasionNpcCenterXs: [0f, 17001f, 17001f]);
  NpcSpawnRateInputs failedTownNpcPrepared =
    NpcSpawnPerPlayerFlagsSystem.Prepare(0, failedTownNpcPort);
  if (failedTownNpcPrepared.Policy.Invaders ||
    !failedTownNpcCalls.SequenceEqual(
      [
        "flags-prelude:0",
        "invasion-inputs:0",
        "invasion-town:0",
        "invasion-town:1",
        "invasion-center:1",
        "invasion-random:3",
        "flags-postlude:0",
        "rate:0",
      ]))
  {
    throw new InvalidOperationException(
      "The first qualifying town NPC must consume one invasion draw and stop when it rolls zero.");
  }

  var shadowCandlePrelude = prelude with
  {
    EventAndTower = prelude.EventAndTower with
    {
      ZoneTowerSolar = false,
      ZoneShadowCandle = true,
    },
  };
  var shadowCandlePort = new RecordingNpcSpawnPassPort(
    CreatePlayerSnapshots(),
    slimeRainActive: false,
    rateInputs: rateInputs,
    perPlayerFlagsPrelude: shadowCandlePrelude,
    perPlayerFlagsPostlude: postlude);
  NpcSpawnRateInputs shadowCandlePrepared =
    NpcSpawnPerPlayerFlagsSystem.Prepare(0, shadowCandlePort);
  if (shadowCandlePrepared.Policy.TownNpcCount != 0 ||
    shadowCandlePrepared.Policy.NoWorms || shadowCandlePrepared.Policy.NoGroundWorms)
  {
    throw new InvalidOperationException(
      "Shadow-candle flags must clear town-NPC, house-wall and AFK restrictions.");
  }

  Console.WriteLine("PASS: NPC per-player spawn flags and invasion ordering");
}

static void RunSpawnCandidateCheckCases()
{
  RunSpawnTileSpaceQueryCases();

  NpcSpawnTargetSelectionSnapshot defaultTargetSnapshot = new(DefaultTarget: 42);
  int resolvedDefaultTarget = NpcSpawnTargetSelectionQuery.Select(
    255,
    in defaultTargetSnapshot);
  if (resolvedDefaultTarget != 42)
  {
    throw new InvalidOperationException(
      "An unspecified NPC spawn target must resolve to the captured default target.");
  }

  int explicitTarget = NpcSpawnTargetSelectionQuery.Select(
    17,
    in defaultTargetSnapshot);
  if (explicitTarget != 17)
  {
    throw new InvalidOperationException(
      "An explicit NPC spawn target must take precedence over the captured default.");
  }

  NpcSpawnTargetSelectionSnapshot initialDefaultTargetSnapshot = new(DefaultTarget: 255);
  int unresolvedTarget = NpcSpawnTargetSelectionQuery.Select(
    255,
    in initialDefaultTargetSnapshot);
  if (unresolvedTarget != 255)
  {
    throw new InvalidOperationException(
      "An initial default target must preserve the legacy 255 sentinel.");
  }

  NpcSpawnSlotFact[] freeAfterReplaceableSlot =
  [
    new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: true),
    new(IsActive: false, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
    new(IsActive: false, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
  ];
  NpcSpawnSlotSelectionResult firstFreeSlot = NpcSpawnSlotSelectionQuery.Select(
    freeAfterReplaceableSlot,
    startIndex: 0,
    searchInReverse: false,
    cannotSpawnInSlot0: false);
  if (firstFreeSlot.SlotIndex != 1 || firstFreeSlot.UsedReplacementFallback)
  {
    throw new InvalidOperationException(
      "NPC slot selection must prefer a free slot over an earlier replaceable slot.");
  }

  NpcSpawnSlotFact[] protectedReplaceableSlot =
  [
    new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
    new(IsActive: false, SpawnSlotProtection: 1, CanBeReplacedByOtherNPCs: true),
  ];
  NpcSpawnSlotSelectionResult replacementFallback = NpcSpawnSlotSelectionQuery.Select(
    protectedReplaceableSlot,
    startIndex: 0,
    searchInReverse: false,
    cannotSpawnInSlot0: false);
  if (replacementFallback.SlotIndex != 1 || !replacementFallback.UsedReplacementFallback)
  {
    throw new InvalidOperationException(
      "A protected inactive slot is in use and is selected only in the replacement pass.");
  }

  NpcSpawnSlotFact[] reverseSlotFacts =
  [
    new(IsActive: false, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
    new(IsActive: false, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
    new(IsActive: false, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
    new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
  ];
  NpcSpawnSlotSelectionResult reverseSlot = NpcSpawnSlotSelectionQuery.Select(
    reverseSlotFacts,
    startIndex: 1,
    searchInReverse: true,
    cannotSpawnInSlot0: false);
  if (reverseSlot.SlotIndex != 2 || reverseSlot.UsedReplacementFallback)
  {
    throw new InvalidOperationException(
      "Reverse NPC slot selection must scan downward and exclude the supplied start index.");
  }

  NpcSpawnSlotSelectionResult slotZeroRestricted = NpcSpawnSlotSelectionQuery.Select(
    reverseSlotFacts,
    startIndex: 0,
    searchInReverse: false,
    cannotSpawnInSlot0: true);
  if (slotZeroRestricted.SlotIndex != 1)
  {
    throw new InvalidOperationException(
      "NPC metadata that forbids slot zero must advance a zero start index.");
  }

  NpcSpawnSlotFact[] unavailableSlotFacts =
  [
    new(IsActive: true, SpawnSlotProtection: 0, CanBeReplacedByOtherNPCs: false),
    new(IsActive: false, SpawnSlotProtection: 1, CanBeReplacedByOtherNPCs: false),
  ];
  NpcSpawnSlotSelectionResult noSlot = NpcSpawnSlotSelectionQuery.Select(
    unavailableSlotFacts,
    startIndex: 0,
    searchInReverse: false,
    cannotSpawnInSlot0: false);
  if (noSlot.Found || noSlot.UsedReplacementFallback)
  {
    throw new InvalidOperationException(
      "NPC slot selection must report no slot when every slot is in use and non-replaceable.");
  }

  int legacyNoSlot =
    NpcSpawnSlotLegacyResultAdapter.ToLegacyGetAvailableNpcSlotResult(in noSlot);
  int legacyNewNpcNoSlot =
    NpcSpawnSlotLegacyResultAdapter.ToLegacyNewNpcResult(legacyNoSlot, maxNpcSlots: 255);
  if (legacyNoSlot != -1 || legacyNewNpcNoSlot != 255)
  {
    throw new InvalidOperationException(
      "A missing slot must map to -1 in GetAvailableNPCSlot and maxNPCs in NewNPC.");
  }

  int legacyAvailableSlot =
    NpcSpawnSlotLegacyResultAdapter.ToLegacyGetAvailableNpcSlotResult(in firstFreeSlot);
  int legacyNewNpcAvailableSlot =
    NpcSpawnSlotLegacyResultAdapter.ToLegacyNewNpcResult(
      legacyAvailableSlot,
      maxNpcSlots: 255);
  if (legacyAvailableSlot != 1 || legacyNewNpcAvailableSlot != 1)
  {
    throw new InvalidOperationException(
      "A selected slot must pass unchanged through the legacy slot and NewNPC result mappings.");
  }

  NpcSpawnSlotSelectionResult pastLastForwardSlot = NpcSpawnSlotSelectionQuery.Select(
    unavailableSlotFacts,
    startIndex: unavailableSlotFacts.Length,
    searchInReverse: false,
    cannotSpawnInSlot0: false);
  if (pastLastForwardSlot.Found || pastLastForwardSlot.UsedReplacementFallback)
  {
    throw new InvalidOperationException(
      "A forward start index at capacity must report no slot without reading beyond the facts.");
  }

  NpcSpawnScreenExclusionInputs visibleScreen = new(
    ScreenWidthPixels: 160,
    ScreenHeightPixels: 160,
    SafeRangeX: 0,
    SafeRangeY: 0,
    DualDungeonsSeed: false,
    Players: [new NpcSpawnScreenPlayerSnapshot(true, 240f, 240f, false)]);
  if (NpcSpawnScreenExclusionQuery.IsSpawnTileOutsideScreen(
    10,
    10,
    in visibleScreen))
  {
    throw new InvalidOperationException(
      "An active player's expanded screen must reject an intersecting spawn tile.");
  }

  NpcSpawnScreenExclusionInputs shieldedDualDungeonScreen = visibleScreen with
  {
    DualDungeonsSeed = true,
    Players = [new NpcSpawnScreenPlayerSnapshot(true, 240f, 240f, true)],
  };
  if (!NpcSpawnScreenExclusionQuery.IsSpawnTileOutsideScreen(
    10,
    10,
    in shieldedDualDungeonScreen))
  {
    throw new InvalidOperationException(
      "Players inside unbreakable walls must not screen-block dual-dungeon spawns.");
  }

  NpcSpawnPostCheckInputs sandstoneInputs = new(
    SpawnTileType: 477,
    SpawnWallType: 0,
    ZoneDungeon: false,
    IsDungeonTile: false,
    DualDungeonsSeed: false,
    HasLiquidAtTileAbove: false,
    HasLiquidAtTwoTilesAbove: false,
    TileAboveIsLava: false,
    TileAboveIsShimmer: false,
    TileAboveIsHoney: false,
    BloodMoon: false,
    Eclipse: false,
    InvasionType: 0,
    PumpkinMoon: false,
    SnowMoon: false,
    SlimeRain: false);
  var sandstoneRandom = new RecordingNpcSpawnRateRandomPort([9], luckResult: 1);
  if (NpcSpawnPostCheckSystem.IsAccepted(in sandstoneInputs, sandstoneRandom) ||
    !sandstoneRandom.NextBounds.SequenceEqual([100]))
  {
    throw new InvalidOperationException(
      "The sandstone post-check must consume one 1-in-10 rejection roll.");
  }

  NpcSpawnPostCheckInputs dungeonInputs = sandstoneInputs with
  {
    SpawnTileType = 1,
    ZoneDungeon = true,
    SpawnWallType = 1,
  };
  var dungeonRandom = new RecordingNpcSpawnRateRandomPort([], luckResult: 1);
  if (NpcSpawnPostCheckSystem.IsAccepted(in dungeonInputs, dungeonRandom) ||
    dungeonRandom.NextDrawCount != 0)
  {
    throw new InvalidOperationException(
      "Dungeon tile rejection must precede and avoid later random checks.");
  }

  NpcSpawnPostCheckInputs liquidInputs = sandstoneInputs with
  {
    SpawnTileType = 1,
    HasLiquidAtTileAbove = true,
    HasLiquidAtTwoTilesAbove = true,
    TileAboveIsShimmer = true,
  };
  var liquidRandom = new RecordingNpcSpawnRateRandomPort([], luckResult: 1);
  if (NpcSpawnPostCheckSystem.IsAccepted(in liquidInputs, liquidRandom) ||
    liquidRandom.NextDrawCount != 0)
  {
    throw new InvalidOperationException(
      "Shimmer above a spawn tile must reject before the sandstone random check.");
  }

  NpcSpawnPostCheckInputs eventInputs = sandstoneInputs with { BloodMoon = true };
  var eventRandom = new RecordingNpcSpawnRateRandomPort([], luckResult: 1);
  if (!NpcSpawnPostCheckSystem.IsAccepted(in eventInputs, eventRandom) ||
    eventRandom.NextDrawCount != 0)
  {
    throw new InvalidOperationException(
      "The sandstone rejection roll must be skipped during a blood moon.");
  }

  NpcSpawnPlayerEligibilitySnapshot[] screenBlockedSnapshots = CreatePlayerSnapshots();
  screenBlockedSnapshots[0] = CreatePlayerSnapshot();
  var screenBlockedPort = new RecordingNpcSpawnPassPort(
    screenBlockedSnapshots,
    slimeRainActive: false,
    areaInputs: CreateSmallSpawnAreaInputs(),
    spawnTileRandomValues: [43, 43],
    screenPlayers: [new NpcSpawnScreenPlayerSnapshot(true, 696f, 696f, false)]);
  int? screenBlockedResult = new NpcSpawnSystem(screenBlockedPort)
    .ProcessNaturalSpawnPass();
  if (screenBlockedResult.HasValue ||
    !screenBlockedPort.Calls.Contains("screen") ||
    screenBlockedPort.Calls.Contains("post:0") ||
    screenBlockedPort.Calls.Contains("continue:0"))
  {
    throw new InvalidOperationException(
      "Screen exclusion must stop the attempt before post-check capture and continuation.");
  }

  NpcSpawnPlayerEligibilitySnapshot[] postCheckRejectedSnapshots = CreatePlayerSnapshots();
  postCheckRejectedSnapshots[0] = CreatePlayerSnapshot();
  var postCheckRejectedPort = new RecordingNpcSpawnPassPort(
    postCheckRejectedSnapshots,
    slimeRainActive: false,
    areaInputs: CreateSmallSpawnAreaInputs(),
    spawnTileRandomValues: [43, 43],
    postCheckInputs: dungeonInputs);
  int? postCheckRejectedResult = new NpcSpawnSystem(postCheckRejectedPort)
    .ProcessNaturalSpawnPass();
  if (postCheckRejectedResult.HasValue ||
    !postCheckRejectedPort.Calls.Contains("post:0") ||
    postCheckRejectedPort.Calls.Contains("continue:0"))
  {
    throw new InvalidOperationException(
      "A rejected chosen tile must stop the attempt before continuation.");
  }

  NpcSpawnAreaInputs degenerateAreaInputs = CreateSmallSpawnAreaInputs() with
  {
    MaxTilesX = 0,
    MaxTilesY = 0,
  };
  var invalidDegenerateRandomPort = new RecordingNpcSpawnPassPort(
    CreatePlayerSnapshots(),
    slimeRainActive: false,
    areaInputs: degenerateAreaInputs,
    spawnTileRandomValues: [1, 1]);
  bool rejectedInvalidDegenerateDraw = false;
  NpcSpawnRateInputs degenerateRateInputs =
    RecordingNpcSpawnPassPort.CreateDefaultRateInputs();
  NpcSpawnRateResult degenerateRateResult = new(
    SpawnRate: 100,
    MaxSpawns: 5,
    NoWorms: false,
    SpawnFriendly: false);
  try
  {
    _ = NpcSpawnTileSearchSystem.Find(
      in degenerateAreaInputs,
      in degenerateRateInputs,
      in degenerateRateResult,
      invalidDegenerateRandomPort);
  }
  catch (InvalidOperationException exception) when (
    exception.Message.Contains("degenerate range", StringComparison.Ordinal))
  {
    rejectedInvalidDegenerateDraw = true;
  }

  if (!rejectedInvalidDegenerateDraw)
  {
    throw new InvalidOperationException(
      "A degenerate tile random range must accept only its single endpoint value.");
  }

  RunSpawnEntryCases();
  Console.WriteLine("PASS: NPC spawn slot selection core cases");
  Console.WriteLine("PASS: NPC spawn screen and chosen-tile candidate checks");
}

static void RunSpawnEntryCases()
{
  var suppressedCalls = new List<string>();
  var suppressedSnapshots = CreatePlayerSnapshots();
  var suppressedSpawnPort = new RecordingNpcSpawnPassPort(
    suppressedSnapshots,
    slimeRainActive: false,
    calls: suppressedCalls);
  var suppressedEntryPort = new RecordingNpcSpawnEntryPort(
    noSpawnCycle: true,
    suppressedCalls);
  new NpcSpawnSystem(suppressedSpawnPort).ProcessEntry(suppressedEntryPort);
  if (!suppressedCalls.SequenceEqual(["consume"]) ||
    suppressedEntryPort.NoSpawnCycle)
  {
    throw new InvalidOperationException(
      "A pending no-spawn-cycle flag must be cleared before skipping respawn and natural-spawn work.");
  }

  var orderedCalls = new List<string>();
  var orderedSnapshots = CreatePlayerSnapshots();
  var orderedSpawnPort = new RecordingNpcSpawnPassPort(
    orderedSnapshots,
    slimeRainActive: false,
    calls: orderedCalls);
  var orderedEntryPort = new RecordingNpcSpawnEntryPort(
    noSpawnCycle: false,
    orderedCalls,
    onCheckRespawns: () => orderedSnapshots[0] = CreatePlayerSnapshot());
  new NpcSpawnSystem(orderedSpawnPort).ProcessEntry(orderedEntryPort);
  if (orderedCalls.Count < 3 ||
    orderedCalls[0] != "consume" ||
    orderedCalls[1] != "respawns" ||
    orderedCalls[2] != "capture:0")
  {
    throw new InvalidOperationException(
      "Respawn checks must run between consuming the cycle flag and the ordered natural-spawn pass.");
  }

  var detailedCalls = new List<string>();
  var detailedSnapshots = CreatePlayerSnapshots();
  detailedSnapshots[0] = CreatePlayerSnapshot();
  NpcSpawnRateInputs detailedRateInputs =
    RecordingNpcSpawnPassPort.CreateDefaultRateInputs();
  detailedRateInputs = detailedRateInputs with
  {
    Policy = detailedRateInputs.Policy with { NoGroundWorms = true },
    World = detailedRateInputs.World with { WorldSurface = 100 },
  };
  NpcSpawnPostCheckInputs detailedPostCheckInputs = new(
    SpawnTileType: 367,
    SpawnWallType: 8,
    ZoneDungeon: false,
    IsDungeonTile: false,
    DualDungeonsSeed: false,
    HasLiquidAtTileAbove: false,
    HasLiquidAtTwoTilesAbove: false,
    TileAboveIsLava: false,
    TileAboveIsShimmer: false,
    TileAboveIsHoney: false,
    BloodMoon: false,
    Eclipse: false,
    InvasionType: 0,
    PumpkinMoon: false,
    SnowMoon: false,
    SlimeRain: false);
  var detailedPort = new RecordingNpcSpawnPassPort(
    detailedSnapshots,
    slimeRainActive: true,
    rateInputs: detailedRateInputs,
    areaInputs: CreateSmallSpawnAreaInputs(),
    spawnTileRandomValues: [43, 53],
    activeSolidTile: (x, y) => x == 43 && y == 54,
    postCheckInputs: detailedPostCheckInputs,
    calls: detailedCalls,
    chosenTileWorldInputs: new NpcSpawnChosenTileWorldInputs(
      DontStarveWorld: false,
      WindSpeedTarget: 0f,
      OceanDistance: 120,
      BeachDistance: 120,
      SpawnTileIsSand: false),
    chosenTileFacts: (x, y) =>
    {
      if (x == 43 && y == 54)
      {
        return new NpcSpawnTileFacts(367, 8, 0, 0);
      }

      if (x == 43 && (y == 53 || y == 52))
      {
        return new NpcSpawnTileFacts(0, 0, 1, 0);
      }

      return default;
    });
  var detailedEntryPort = new RecordingNpcSpawnEntryPort(
    noSpawnCycle: false,
    detailedCalls);
  NpcSpawnEntryResult detailedResult = new NpcSpawnSystem(detailedPort)
    .ProcessEntryDetailed(detailedEntryPort);
  if (detailedResult.NoSpawnCycleWasConsumed ||
    !detailedResult.RespawnCheckRan ||
    !detailedResult.Pass.ContinuationWasInvoked ||
    detailedResult.Pass.CreationObservation != NpcSpawnCreationObservation.Unknown)
  {
    throw new InvalidOperationException(
      "The detailed spawn result must keep legacy loop control separate from unknown entity creation.");
  }

  if (detailedPort.ContinuationCandidates.Count != 1)
  {
    throw new InvalidOperationException(
      "The first accepted candidate must be handed off once and stop the player pass.");
  }

  int detailedCaptureIndex = detailedCalls.IndexOf("capture:0");
  int detailedSlimeIndex = detailedCalls.IndexOf("slime:0");
  int detailedFlagsIndex = detailedCalls.IndexOf("flags-prelude:0");
  int detailedInvasionInputsIndex = detailedCalls.IndexOf("invasion-inputs:0");
  int detailedFlagsPostludeIndex = detailedCalls.IndexOf("flags-postlude:0");
  int detailedRateIndex = detailedCalls.IndexOf("rate:0");
  if (detailedCaptureIndex < 0 || detailedSlimeIndex < 0 || detailedFlagsIndex < 0 ||
    detailedInvasionInputsIndex < 0 || detailedFlagsPostludeIndex < 0 ||
    detailedRateIndex < 0 || detailedCaptureIndex >= detailedSlimeIndex ||
    detailedSlimeIndex >= detailedFlagsIndex ||
    detailedFlagsIndex >= detailedInvasionInputsIndex ||
    detailedInvasionInputsIndex >= detailedFlagsPostludeIndex ||
    detailedFlagsPostludeIndex + 1 != detailedRateIndex)
  {
    throw new InvalidOperationException(
      "Per-player flag inputs and invasion effects must remain ordered after slime rain and " +
      "before rate inputs are captured.");
  }

  NpcSpawnAcceptedCandidate candidate = detailedPort.ContinuationCandidates[0];
  if (candidate.PlayerIndex != 0 ||
    candidate.RateInputs != detailedRateInputs ||
    candidate.PostCheckInputs != detailedPostCheckInputs ||
    !candidate.TileSearchResult.Found ||
    candidate.TileSearchResult.SkyMob ||
    candidate.RateResult.NoWorms ||
    !candidate.NoWormsForSpawn ||
    !candidate.ChosenTileFlags.WaterTile ||
    !candidate.ChosenTileFlags.NearMarble ||
    candidate.ChosenTileFlags.NearGranite ||
    !candidate.ChosenTileFlags.UnderGround ||
    !candidate.ChosenTileFlags.SurfaceSpawn ||
    !candidate.ChosenTileFlags.IsBeach ||
    candidate.ChosenTileFlags.IsOcean ||
    detailedPort.ChosenTileRandomBounds.Count != 0 ||
    candidate.RateResult.SpawnRate <= 0 ||
    candidate.RateResult.MaxSpawns <= 0)
  {
    throw new InvalidOperationException(
      $"The continuation must receive the accepted attempt's captured inputs and gate results. " +
      $"player={candidate.PlayerIndex}, rate={candidate.RateResult}, " +
      $"tile={candidate.TileSearchResult}, flags={candidate.ChosenTileFlags}, " +
      $"postCheckMatches={candidate.PostCheckInputs == detailedPostCheckInputs}, " +
      $"chosenTileRandomCalls={detailedPort.ChosenTileRandomBounds.Count}.");
  }

  NpcSpawnAreaInputs chosenFlagsAreaInputs = CreateSmallSpawnAreaInputs();
  NpcSpawnRateInputs defaultChosenFlagsRateInputs =
    RecordingNpcSpawnPassPort.CreateDefaultRateInputs();
  NpcSpawnRateInputs chosenFlagsRateInputs = defaultChosenFlagsRateInputs with
  {
    Context = defaultChosenFlagsRateInputs.Context with
    {
      PlayerTileX = 500,
      PlayerTileY = 500,
    },
  };
  NpcSpawnRateResult chosenFlagsRateResult = new(
    SpawnRate: 100,
    MaxSpawns: 5,
    NoWorms: false,
    SpawnFriendly: false);
  NpcSpawnTileSearchResult chosenFlagsTileResult = new(
    Found: true,
    TileX: 500,
    TileY: 200,
    XRange: false,
    SkyMob: false,
    Area: default);
  NpcSpawnPostCheckInputs chosenFlagsPostCheckInputs = new(
    SpawnTileType: 0,
    SpawnWallType: 0,
    ZoneDungeon: false,
    IsDungeonTile: false,
    DualDungeonsSeed: false,
    HasLiquidAtTileAbove: false,
    HasLiquidAtTwoTilesAbove: false,
    TileAboveIsLava: false,
    TileAboveIsShimmer: false,
    TileAboveIsHoney: false,
    BloodMoon: false,
    Eclipse: false,
    InvasionType: 0,
    PumpkinMoon: false,
    SnowMoon: false,
    SlimeRain: false);
  NpcSpawnChosenTileWorldInputs chosenFlagsWorldInputs = new(
    DontStarveWorld: false,
    WindSpeedTarget: 0f,
    OceanDistance: 120,
    BeachDistance: 120,
    SpawnTileIsSand: false);
  var chosenFlagsPort = new RecordingNpcSpawnPassPort(
    CreatePlayerSnapshots(),
    slimeRainActive: false,
    areaInputs: chosenFlagsAreaInputs,
    chosenTileWorldInputs: chosenFlagsWorldInputs);
  NpcSpawnChosenTileWorldInputs capturedChosenFlagsWorldInputs =
    chosenFlagsPort.CaptureChosenTileWorldInputs(500, 200, 0);
  NpcSpawnChosenTileFlagsResult chosenFlags =
    NpcSpawnChosenTileFlagsSystem.Calculate(
      in chosenFlagsRateInputs,
      in chosenFlagsRateResult,
      in chosenFlagsAreaInputs,
      in chosenFlagsTileResult,
      in chosenFlagsPostCheckInputs,
      in capturedChosenFlagsWorldInputs,
      chosenFlagsPort);
  IReadOnlyList<(int MinimumInclusive, int MaximumExclusive)> chosenBounds =
    chosenFlagsPort.ChosenTileRandomBounds;
  if (chosenFlags.NearMarble || chosenFlags.NearGranite ||
    !chosenFlags.UnderGround || !chosenFlags.SurfaceSpawn ||
    chosenBounds.Count != 66 ||
    chosenBounds[0] != (20, 31) ||
    chosenBounds[1] != (1, 4) ||
    chosenBounds[42] != (1, 4) ||
    chosenBounds[43] != (30, 61) ||
    chosenBounds[44] != (3, 7) ||
    chosenBounds[65] != (3, 7))
  {
    throw new InvalidOperationException(
      "Ordinary chosen tiles must preserve both ordered biome scans and their RNG bounds.");
  }
}

static NpcSpawnPlayerEligibilitySnapshot[] CreatePlayerSnapshots()
{
  var snapshots = new NpcSpawnPlayerEligibilitySnapshot[255];
  for (var index = 0; index < snapshots.Length; index++)
  {
    snapshots[index] = CreatePlayerSnapshot(isPlayerActive: false);
  }

  return snapshots;
}

static NpcSpawnPlayerEligibilitySnapshot CreatePlayerSnapshot(
  bool isPlayerActive = true)
{
  return new NpcSpawnPlayerEligibilitySnapshot(
    IsPlayerActive: isPlayerActive,
    IsPlayerDead: false,
    IsJourneyMode: false,
    IsSpawnRatePowerUnlocked: false,
    DoesSpawnRatePowerDisablePlayer: false,
    IsNearMoonLord: false);
}

static NpcSpawnAreaInputs CreateSmallSpawnAreaInputs()
{
  return new NpcSpawnAreaInputs(
    ScreenWidthPixels: 160,
    ScreenHeightPixels: 160,
    PlayerTileX: 50,
    PlayerTileY: 50,
    SelectedItemType: 0,
    PlayerScope: false,
    DualDungeonsSeed: false,
    ZoneOverworldHeight: false,
    ZoneSkyHeight: false,
    MaxTilesX: 1000,
    MaxTilesY: 1000);
}

internal sealed class RecordingNpcSpawnRateRandomPort : INpcSpawnRateRandomPort
{
  private readonly IReadOnlyList<int> _nextValues;
  private readonly int _luckResult;
  private int _nextIndex;

  public List<int> NextBounds { get; } = new();

  public List<(float Luck, int Range)> LuckRolls { get; } = new();

  public int NextDrawCount => _nextIndex;

  public RecordingNpcSpawnRateRandomPort(
    IReadOnlyList<int> nextValues,
    int luckResult)
  {
    _nextValues = nextValues;
    _luckResult = luckResult;
  }

  public int Next(int exclusiveUpperBound)
  {
    NextBounds.Add(exclusiveUpperBound);
    if (_nextIndex >= _nextValues.Count)
    {
      throw new InvalidOperationException("Unexpected spawn-rate random draw.");
    }

    int value = _nextValues[_nextIndex++];
    if (value < 0 || value >= exclusiveUpperBound)
    {
      throw new InvalidOperationException("Spawn-rate random draw was out of range.");
    }

    return value;
  }

  public int RollOnlyBadLuckExtreme(float luck, int range)
  {
    LuckRolls.Add((luck, range));
    return _luckResult;
  }
}

internal sealed class RecordingNpcSpawnTypeResolutionRandomPort :
  INpcSpawnTypeResolutionRandomPort
{
  private readonly Queue<int> _values;

  public RecordingNpcSpawnTypeResolutionRandomPort(params int[] values)
  {
    _values = new Queue<int>(values);
  }

  public List<int> RequestedUpperBounds { get; } = [];

  public int Next(int exclusiveUpperBound)
  {
    RequestedUpperBounds.Add(exclusiveUpperBound);
    return _values.Dequeue();
  }
}

internal sealed class RecordingNpcSpawnTowerSelectionPort : INpcSpawnTowerSelectionPort
{
  private readonly Queue<int> _randomValues;
  private readonly IReadOnlyDictionary<int, int> _activeCounts;

  public RecordingNpcSpawnTowerSelectionPort(
    IEnumerable<int> randomValues,
    IReadOnlyDictionary<int, int> activeCounts)
  {
    _randomValues = new Queue<int>(randomValues);
    _activeCounts = activeCounts;
  }

  public List<int> RandomBounds { get; } = [];

  public List<int> PopulationQueries { get; } = [];

  public int Next(int exclusiveUpperBound)
  {
    RandomBounds.Add(exclusiveUpperBound);
    int value = _randomValues.Dequeue();
    if (value < 0 || value >= exclusiveUpperBound)
    {
      throw new InvalidOperationException("Tower-selection random draw was out of range.");
    }

    return value;
  }

  public int CountActiveNpcs(int npcTypeId)
  {
    PopulationQueries.Add(npcTypeId);
    return _activeCounts.TryGetValue(npcTypeId, out int count) ? count : 0;
  }
}

internal sealed class RecordingNpcSpawnPassPort : INpcSpawnPassPort
{
  private readonly IReadOnlyList<NpcSpawnPlayerEligibilitySnapshot> _snapshots;
  private readonly IReadOnlyList<NpcSpawnScreenPlayerSnapshot> _screenPlayers;
  private readonly NpcSpawnRateInputs _rateInputs;
  private readonly NpcSpawnPerPlayerFlagsPrelude _perPlayerFlagsPrelude;
  private readonly NpcSpawnInvasionInputs _invasionInputs;
  private readonly NpcSpawnPerPlayerFlagsPostlude _perPlayerFlagsPostlude;
  private readonly IReadOnlyList<bool> _invasionTownNpcSlots;
  private readonly IReadOnlyList<float> _invasionNpcCenterXs;
  private readonly NpcSpawnPostCheckInputs _postCheckInputs;
  private readonly NpcSpawnAreaInputs _areaInputs;
  private readonly int? _disableSlimeRainAfterPlayer;
  private readonly Queue<int> _spawnRateRollValues;
  private readonly Queue<int> _spawnTileRandomValues;
  private readonly Func<int, int, bool> _activeSolidTile;
  private readonly Func<int, int, bool> _houseWallTile;
  private readonly Func<int, int, NpcSpawnTileSpaceFacts> _tileSpaceFacts;
  private readonly NpcSpawnChosenTileWorldInputs _chosenTileWorldInputs;
  private readonly Func<int, int, NpcSpawnTileFacts> _chosenTileFacts;
  private readonly Func<int, bool> _undergroundDesertWall;
  private readonly Func<int, int, bool> _oceanDepths;
  private bool _slimeRainActive;
  private bool _capturingInvasion;
  private bool _capturingChosenTileFlags;

  public RecordingNpcSpawnPassPort(
    IReadOnlyList<NpcSpawnPlayerEligibilitySnapshot> snapshots,
    bool slimeRainActive,
    IReadOnlyList<int>? spawnRateRollValues = null,
    int? disableSlimeRainAfterPlayer = null,
    NpcSpawnRateInputs? rateInputs = null,
    NpcSpawnAreaInputs? areaInputs = null,
    IReadOnlyList<int>? spawnTileRandomValues = null,
    Func<int, int, bool>? activeSolidTile = null,
    Func<int, int, bool>? houseWallTile = null,
    Func<int, int, NpcSpawnTileSpaceFacts>? tileSpaceFacts = null,
    IReadOnlyList<NpcSpawnScreenPlayerSnapshot>? screenPlayers = null,
    NpcSpawnPostCheckInputs? postCheckInputs = null,
    List<string>? calls = null,
    NpcSpawnChosenTileWorldInputs? chosenTileWorldInputs = null,
    Func<int, int, NpcSpawnTileFacts>? chosenTileFacts = null,
    Func<int, bool>? undergroundDesertWall = null,
    Func<int, int, bool>? oceanDepths = null,
    NpcSpawnPerPlayerFlagsPrelude? perPlayerFlagsPrelude = null,
    NpcSpawnInvasionInputs? invasionInputs = null,
    NpcSpawnPerPlayerFlagsPostlude? perPlayerFlagsPostlude = null,
    IReadOnlyList<bool>? invasionTownNpcSlots = null,
    IReadOnlyList<float>? invasionNpcCenterXs = null)
  {
    _snapshots = snapshots;
    _screenPlayers = screenPlayers ?? Array.Empty<NpcSpawnScreenPlayerSnapshot>();
    _rateInputs = rateInputs ?? CreateDefaultRateInputs();
    _perPlayerFlagsPrelude = perPlayerFlagsPrelude ??
      CreateDefaultPerPlayerFlagsPrelude(_rateInputs);
    _invasionInputs = invasionInputs ?? new NpcSpawnInvasionInputs(
      InvasionType: 0,
      InvasionDelay: 0,
      InvasionSize: 0,
      PlayerPositionX: 0f,
      PlayerPositionY: 0f,
      WorldSurface: 0,
      ScreenHeightPixels: 0,
      SpawnTileY: 0,
      InvasionX: 0,
      MaxTilesX: 0,
      MaxNpcSlots: 0);
    _perPlayerFlagsPostlude = perPlayerFlagsPostlude ??
      CreateDefaultPerPlayerFlagsPostlude(_rateInputs);
    _invasionTownNpcSlots = invasionTownNpcSlots ?? Array.Empty<bool>();
    _invasionNpcCenterXs = invasionNpcCenterXs ?? Array.Empty<float>();
    _postCheckInputs = postCheckInputs ?? new NpcSpawnPostCheckInputs(
      SpawnTileType: 0,
      SpawnWallType: 0,
      ZoneDungeon: false,
      IsDungeonTile: false,
      DualDungeonsSeed: false,
      HasLiquidAtTileAbove: false,
      HasLiquidAtTwoTilesAbove: false,
      TileAboveIsLava: false,
      TileAboveIsShimmer: false,
      TileAboveIsHoney: false,
      BloodMoon: false,
      Eclipse: false,
      InvasionType: 0,
      PumpkinMoon: false,
      SnowMoon: false,
      SlimeRain: false);
    _areaInputs = areaInputs ?? CreateDefaultSpawnAreaInputs();
    _slimeRainActive = slimeRainActive;
    _disableSlimeRainAfterPlayer = disableSlimeRainAfterPlayer;
    _spawnRateRollValues = new Queue<int>(spawnRateRollValues ?? Array.Empty<int>());
    _spawnTileRandomValues = new Queue<int>(
      spawnTileRandomValues ?? Array.Empty<int>());
    _activeSolidTile = activeSolidTile ?? ((_, _) => false);
    _houseWallTile = houseWallTile ?? ((_, _) => false);
    _tileSpaceFacts = tileSpaceFacts ??
      ((_, _) => new NpcSpawnTileSpaceFacts(false, false, false));
    _chosenTileWorldInputs = chosenTileWorldInputs ?? new NpcSpawnChosenTileWorldInputs(
      DontStarveWorld: false,
      WindSpeedTarget: 0f,
      OceanDistance: 0,
      BeachDistance: 0,
      SpawnTileIsSand: false);
    _chosenTileFacts = chosenTileFacts ?? ((_, _) => default);
    _undergroundDesertWall = undergroundDesertWall ?? (_ => false);
    _oceanDepths = oceanDepths ?? ((_, _) => false);
    Calls = calls ?? new List<string>();
    TileCalls = new List<string>();
  }

  public List<string> Calls { get; }

  public List<string> TileCalls { get; }

  public List<int> SpawnRateBounds { get; } = new();

  public List<(int MinimumInclusive, int MaximumExclusive)> SpawnTileRangeBounds { get; } = new();

  public List<(int MinimumInclusive, int MaximumExclusive)> ChosenTileRandomBounds { get; } = new();

  public List<string> ChosenTileCalls { get; } = new();

  public bool IsSlimeRainActive => _slimeRainActive;

  public NpcSpawnPlayerEligibilitySnapshot CapturePlayerEligibility(int playerIndex)
  {
    Calls.Add($"capture:{playerIndex}");
    return _snapshots[playerIndex];
  }

  public NpcSpawnPerPlayerFlagsPrelude CapturePerPlayerFlagsPrelude(int playerIndex)
  {
    Calls.Add($"flags-prelude:{playerIndex}");
    return _perPlayerFlagsPrelude;
  }

  public NpcSpawnInvasionInputs CaptureInvasionInputs(int playerIndex)
  {
    Calls.Add($"invasion-inputs:{playerIndex}");
    _capturingInvasion = true;
    return _invasionInputs;
  }

  public bool IsTownNpcSlot(int npcIndex)
  {
    Calls.Add($"invasion-town:{npcIndex}");
    return _invasionTownNpcSlots[npcIndex];
  }

  public float GetNpcCenterX(int npcIndex)
  {
    Calls.Add($"invasion-center:{npcIndex}");
    return _invasionNpcCenterXs[npcIndex];
  }

  public NpcSpawnPerPlayerFlagsPostlude CapturePerPlayerFlagsPostlude(int playerIndex)
  {
    Calls.Add($"flags-postlude:{playerIndex}");
    _capturingInvasion = false;
    return _perPlayerFlagsPostlude;
  }

  public NpcSpawnRateInputs CaptureSpawnRateInputs(int playerIndex)
  {
    Calls.Add($"rate:{playerIndex}");
    return _rateInputs;
  }

  public NpcSpawnAreaInputs CaptureSpawnAreaInputs(int playerIndex)
  {
    Calls.Add($"area:{playerIndex}");
    return _areaInputs;
  }

  public IReadOnlyList<NpcSpawnScreenPlayerSnapshot> CaptureScreenPlayers()
  {
    Calls.Add("screen");
    return _screenPlayers;
  }

  public NpcSpawnPostCheckInputs CapturePostCheckInputs(
    int playerIndex,
    in NpcSpawnTileSearchResult tileSearchResult)
  {
    Calls.Add($"post:{playerIndex}");
    return _postCheckInputs;
  }

  public NpcSpawnChosenTileWorldInputs CaptureChosenTileWorldInputs(
    int spawnTileX,
    int spawnTileY,
    int spawnTileType)
  {
    Calls.Add($"chosen-world:{spawnTileX}:{spawnTileY}:{spawnTileType}");
    _capturingChosenTileFlags = true;
    return _chosenTileWorldInputs;
  }

  public NpcSpawnTileFacts ReadTile(int tileX, int tileY)
  {
    ChosenTileCalls.Add($"tile:{tileX}:{tileY}");
    return _chosenTileFacts(tileX, tileY);
  }

  public bool AllowsUndergroundDesertEnemiesToSpawn(int wallType)
  {
    ChosenTileCalls.Add($"desert-wall:{wallType}");
    return _undergroundDesertWall(wallType);
  }

  public bool IsOceanDepths(int tileX, int tileY)
  {
    ChosenTileCalls.Add($"ocean-depths:{tileX}:{tileY}");
    return _oceanDepths(tileX, tileY);
  }

  public void SpawnSlimeRainForPlayer(int playerIndex)
  {
    Calls.Add($"slime:{playerIndex}");
    if (playerIndex == _disableSlimeRainAfterPlayer)
    {
      _slimeRainActive = false;
    }
  }

  public List<NpcSpawnAcceptedCandidate> ContinuationCandidates { get; } = new();

  public void ContinueSpawnAttempt(in NpcSpawnAcceptedCandidate candidate)
  {
    Calls.Add($"continue:{candidate.PlayerIndex}");
    if (!candidate.TileSearchResult.Found)
    {
      throw new InvalidOperationException(
        "The spawn continuation requires an accepted tile candidate.");
    }

    ContinuationCandidates.Add(candidate);
  }

  public int Next(int exclusiveUpperBound)
  {
    if (_capturingInvasion)
    {
      Calls.Add($"invasion-random:{exclusiveUpperBound}");
    }
    else
    {
      SpawnRateBounds.Add(exclusiveUpperBound);
    }

    int value = _spawnRateRollValues.Count > 0
      ? _spawnRateRollValues.Dequeue()
      : 0;
    if (value < 0 || value >= exclusiveUpperBound)
    {
      throw new InvalidOperationException(
        "The configured spawn-rate roll is outside its requested range.");
    }

    return value;
  }

  public int Next(int minimumInclusive, int maximumExclusive)
  {
    if (_capturingChosenTileFlags)
    {
      ChosenTileRandomBounds.Add((minimumInclusive, maximumExclusive));
    }
    else
    {
      SpawnTileRangeBounds.Add((minimumInclusive, maximumExclusive));
    }

    return _spawnTileRandomValues.Count > 0
      ? _spawnTileRandomValues.Dequeue()
      : minimumInclusive;
  }

  public int RollOnlyBadLuckExtreme(float luck, int range)
  {
    return 1;
  }

  public bool IsActiveSolidTile(int tileX, int tileY)
  {
    TileCalls.Add($"solid:{tileX}:{tileY}");
    return _activeSolidTile(tileX, tileY);
  }

  public bool IsHouseWallTile(int tileX, int tileY)
  {
    TileCalls.Add($"wall:{tileX}:{tileY}");
    return _houseWallTile(tileX, tileY);
  }

  public NpcSpawnTileSpaceFacts CaptureTileSpaceFacts(int tileX, int tileY)
  {
    TileCalls.Add($"space:{tileX}:{tileY}");
    return _tileSpaceFacts(tileX, tileY);
  }

  internal static NpcSpawnRateInputs CreateDefaultRateInputs()
  {
    return new NpcSpawnRateInputs(
      new NpcSpawnContextSnapshot(
        SpawnSpaceX: 2,
        SpawnSpaceY: 3,
        FairyLog: false,
        NumberOfActivePlayers: 1,
        ReachedInvasionBossCap: false,
        PlayerTileX: 0,
        PlayerTileY: 0,
        Luck: 0f,
        DayTime: true,
        Raining: false),
      new NpcSpawnPolicyEligibilitySnapshot(
        TownNpcCount: 0,
        SkyMob: false,
        NoWorms: false,
        NoGroundWorms: false,
        Invaders: false,
        SpawnFriendly: false,
        IgnoreSafeWalls: false,
        SpawnSpider: false,
        IsSpawningInWindDirection: false,
        OffensiveToTim: false,
        PlayerHasStartingHealth: false),
      new NpcSpawnSpatialEligibilitySnapshot(
        SurfaceSpawn: false,
        SpawnUndergroundDesert: false,
        HardDungeon: false,
        DeeperThanRockLayer: false,
        UnderGround: false,
        IsOcean: false,
        IsBeach: false,
        SkyBehindPlayer: false,
        LivingTree: false,
        InRemixStartingArea: false),
      new NpcSpawnBiomeAndDungeonEligibilitySnapshot(
        WaterTile: false,
        NearGranite: false,
        NearMarble: false,
        DualDungeonsSpawnRules: false,
        InDualDungeon: false,
        TresspassingDualDungeon: false),
      new NpcSpawnBiomeZoneEligibilitySnapshot(
        ZoneCorrupt: false,
        ZoneCrimson: false,
        ZoneHallow: false,
        ZoneJungle: false,
        ZoneSnow: false,
        ZoneGlowshroom: false,
        ZoneMeteor: false,
        ZoneGraveyard: false,
        ZoneDungeon: false,
        ZoneLihzhardTemple: false,
        ZoneGranite: false,
        ZoneMarble: false,
        ZoneSandstorm: false),
      new NpcSpawnEventAndTowerEligibilitySnapshot(
        ZoneTowerSolar: false,
        ZoneTowerVortex: false,
        ZoneTowerNebula: false,
        ZoneTowerStardust: false,
        ZoneOldOneArmy: false,
        ZoneWaterCandle: false,
        ZonePeaceCandle: false,
        ZoneShadowCandle: false),
      new NpcSpawnRatePlayerInputs(
        PositionY: 0f,
        CenterY: 0f,
        NearbyActiveNpcSlots: 0f,
        ZoneUndergroundDesert: false,
        IsInvisible: false,
        IsCalmed: false,
        HasSunflower: false,
        HasAnglerSetSpawnReduction: false,
        EnemySpawnsEnabled: false,
        HasNearbyFairy: false),
      new NpcSpawnRateWorldInputs(
        DefaultSpawnRate: 100,
        DefaultMaxSpawns: 5,
        HardMode: false,
        RemixWorld: false,
        WorldSurface: 200,
        RockLayer: 400,
        UnderworldLayer: 600,
        ScreenHeightPixels: 240,
        BloodMoon: false,
        PumpkinMoon: false,
        SnowMoon: false,
        Eclipse: false,
        CloudAlpha: 0f,
        DrunkWorld: false,
        PlayerTileHasDrunkWorldWall: false,
        WallOfFleshPresent: false,
        GetGoodWorld: false,
        JourneyMode: false,
        SpawnRatePowerUnlocked: false,
        HasRemappedJourneySpawnRate: false,
        JourneySpawnRateValue: 1f,
        OldOnesArmyOngoing: false,
        DownedBoss3: false,
        SkyblockLowTiles: false,
        InfectedSeed: false,
        ExpertMode: false));
  }

  private static NpcSpawnPerPlayerFlagsPrelude CreateDefaultPerPlayerFlagsPrelude(
    NpcSpawnRateInputs rateInputs)
  {
    bool trespassing = rateInputs.BiomeAndDungeon.TresspassingDualDungeon;
    return new NpcSpawnPerPlayerFlagsPrelude(
      Context: rateInputs.Context,
      BiomeZones: rateInputs.BiomeZones,
      EventAndTower: rateInputs.EventAndTower,
      DownedPlantBoss: rateInputs.Spatial.HardDungeon && rateInputs.World.HardMode,
      HardMode: rateInputs.World.HardMode,
      DualDungeonsSeed: rateInputs.BiomeAndDungeon.DualDungeonsSpawnRules,
      PlayerInsideUnbreakableWalls: rateInputs.BiomeAndDungeon.InDualDungeon,
      DungeonProgressCanSafelyMatch: trespassing ? 0 : 1,
      DungeonProgressPlayerNeedsToMatch: 1);
  }

  private static NpcSpawnPerPlayerFlagsPostlude CreateDefaultPerPlayerFlagsPostlude(
    NpcSpawnRateInputs rateInputs)
  {
    bool shadowCandle = rateInputs.EventAndTower.ZoneShadowCandle;
    bool offensiveToTim = rateInputs.Policy.OffensiveToTim;
    float playerCenterX = rateInputs.Spatial.InRemixStartingArea ? 8000f : 3200f;
    return new NpcSpawnPerPlayerFlagsPostlude(
      PlayerTownNpcCount: rateInputs.Policy.TownNpcCount,
      PlayerTileIsInWorld: true,
      PlayerTileHasHouseWall: rateInputs.Policy.NoWorms && !shadowCandle,
      PlayerAfkCounter: rateInputs.Policy.NoGroundWorms && !shadowCandle ? 3600 : 0,
      AfkTimeNeededForNoWormSpawns: 3600,
      PlayerTileHasLightWall: rateInputs.Spatial.SkyBehindPlayer,
      PlayerTileWallType: rateInputs.Spatial.LivingTree
        ? 244
        : rateInputs.Spatial.SkyBehindPlayer
          ? 73
          : 0,
      RemixWorld: rateInputs.World.RemixWorld,
      PlayerCenterX: playerCenterX,
      MaxTilesX: 1000,
      ArmorSlot0ItemType: offensiveToTim ? 0 : 238,
      ArmorSlot1ItemType: offensiveToTim ? 4256 : 0,
      PlayerMaximumLife: rateInputs.Policy.PlayerHasStartingHealth ? 100 : 101);
  }

  private static NpcSpawnAreaInputs CreateDefaultSpawnAreaInputs()
  {
    return new NpcSpawnAreaInputs(
      ScreenWidthPixels: 1920,
      ScreenHeightPixels: 1200,
      PlayerTileX: 200,
      PlayerTileY: 100,
      SelectedItemType: 0,
      PlayerScope: false,
      DualDungeonsSeed: false,
      ZoneOverworldHeight: false,
      ZoneSkyHeight: false,
      MaxTilesX: 1000,
      MaxTilesY: 1000);
  }
}

internal sealed class RecordingNpcSpawnEntryPort : INpcSpawnEntryPort
{
  private readonly List<string> _calls;
  private readonly Action? _onCheckRespawns;

  public RecordingNpcSpawnEntryPort(
    bool noSpawnCycle,
    List<string> calls,
    Action? onCheckRespawns = null)
  {
    NoSpawnCycle = noSpawnCycle;
    _calls = calls;
    _onCheckRespawns = onCheckRespawns;
  }

  public bool NoSpawnCycle { get; private set; }

  public bool ConsumeNoSpawnCycle()
  {
    _calls.Add("consume");
    bool wasPending = NoSpawnCycle;
    NoSpawnCycle = false;
    return wasPending;
  }

  public void CheckRespawns()
  {
    _calls.Add("respawns");
    _onCheckRespawns?.Invoke();
  }
}
