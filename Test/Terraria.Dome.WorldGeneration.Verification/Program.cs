using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Liquid.Definitions;
using Terraria.Dome.Simulation.World;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Simulation.WorldGeneration.Definitions;
using Terraria.Dome.Simulation.WorldGeneration.Systems;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;
using Terraria.Dome.Simulation.WorldObjects.FoodPlatter;
using Terraria.Dome.Simulation.WorldObjects.FoodPlatter.Commands;
using Terraria.Dome.Simulation.WorldObjects.FoodPlatter.Systems;
using Terraria.Dome.Simulation.WorldObjects.Definitions;
using Terraria.WorldFile.V319;
using Terraria.WorldFile.V319.Model;

const int replayWidth = 400;
const int replayHeight = 300;
const int replaySpawnX = 200;
const int replaySurfaceY = 80;
string repositoryRoot = FindRepositoryRoot();
string sourcePath = Path.Combine(
  repositoryRoot,
  "..",
  "Version4物理删除了某些文件",
  "Terraria",
  "WorldGen.cs");
sourcePath = Path.GetFullPath(sourcePath);
if (!File.Exists(sourcePath))
{
  throw new FileNotFoundException("WorldGen fact source was not found.", sourcePath);
}

string? legacyOracleArgument = args.FirstOrDefault(
  argument => argument.StartsWith("--legacy-oracle=", StringComparison.Ordinal));
string? legacyDifferentialArgument = args.FirstOrDefault(
  argument => argument.StartsWith("--legacy-differential=", StringComparison.Ordinal));
string? legacyDifferentialSpawnArgument = args.FirstOrDefault(
  argument => argument.StartsWith("--legacy-differential-spawn=", StringComparison.Ordinal));
string? legacyDifferentialSurfaceYArgument = args.FirstOrDefault(
  argument => argument.StartsWith("--legacy-differential-surface-y=", StringComparison.Ordinal));
string? dirtWallOffsetsArgument = args.FirstOrDefault(
  argument => argument.StartsWith("--dirt-wall-offsets=", StringComparison.Ordinal));
bool reducedVerification = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--reduced"));
if (legacyDifferentialArgument is not null)
{
  string legacyDifferentialPath = legacyDifferentialArgument["--legacy-differential=".Length..];
  if (!Path.IsPathFullyQualified(legacyDifferentialPath))
  {
    throw new ArgumentException(
      "The legacy differential path must be absolute.",
      nameof(legacyDifferentialArgument));
  }

  bool useLegacyMetadataSpawn = legacyDifferentialSpawnArgument is not null &&
    legacyDifferentialSpawnArgument["--legacy-differential-spawn=".Length..] == "metadata";
  if (legacyDifferentialSpawnArgument is not null && !useLegacyMetadataSpawn &&
      legacyDifferentialSpawnArgument["--legacy-differential-spawn=".Length..] != "center")
  {
    throw new ArgumentException(
      "The legacy differential spawn must be either 'center' or 'metadata'.",
      nameof(legacyDifferentialSpawnArgument));
  }

  int? requestedSurfaceY = null;
  bool useLegacyMetadataSurfaceY = legacyDifferentialSurfaceYArgument is not null &&
    legacyDifferentialSurfaceYArgument["--legacy-differential-surface-y=".Length..] == "metadata";
  if (legacyDifferentialSurfaceYArgument is not null && !useLegacyMetadataSurfaceY)
  {
    string surfaceYText =
      legacyDifferentialSurfaceYArgument["--legacy-differential-surface-y=".Length..];
    if (!int.TryParse(surfaceYText, out int parsedSurfaceY) || parsedSurfaceY < 0)
    {
      throw new ArgumentException(
        "The legacy differential surface Y must be 'metadata' or a non-negative integer.",
        nameof(legacyDifferentialSurfaceYArgument));
    }

    requestedSurfaceY = parsedSurfaceY;
  }

  string? dirtWallOffsetsPath = null;
  if (dirtWallOffsetsArgument is not null)
  {
    dirtWallOffsetsPath = dirtWallOffsetsArgument["--dirt-wall-offsets=".Length..];
    if (!Path.IsPathFullyQualified(dirtWallOffsetsPath) ||
        !File.Exists(dirtWallOffsetsPath))
    {
      throw new ArgumentException(
        "The dirt wall offset artifact path must be an existing absolute file.",
        nameof(dirtWallOffsetsArgument));
    }
  }

  RunLegacyDifferential(
    legacyDifferentialPath,
    repositoryRoot,
    useLegacyMetadataSpawn,
    useLegacyMetadataSurfaceY,
    requestedSurfaceY,
    dirtWallOffsetsPath);
  Environment.Exit(0);
}

if (legacyOracleArgument is not null)
{
  string legacyOraclePath = legacyOracleArgument["--legacy-oracle=".Length..];
  if (!Path.IsPathFullyQualified(legacyOraclePath))
  {
    throw new ArgumentException(
      "The legacy oracle path must be absolute.",
      nameof(legacyOracleArgument));
  }

  using FileStream oracleStream = new(
    legacyOraclePath,
    FileMode.Open,
    FileAccess.Read,
    FileShare.Read);
  LegacyWorldDocument legacyOracle = WldWorldReader.Read(oracleStream);
  LegacyOracleEvidence oracleEvidence = new(
    legacyOraclePath,
    new FileInfo(legacyOraclePath).Length,
    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(legacyOraclePath))),
    legacyOracle.Version,
    legacyOracle.FormatVersion.ToString(),
    legacyOracle.Metadata,
    legacyOracle.Tiles.Count,
    legacyOracle.Tiles.Count(tile => tile.LiquidAmount > 0),
    CreateLegacyOracleFingerprint(legacyOracle));
  string oracleEvidenceDirectory = Path.Combine(repositoryRoot, "docs", "worldgen");
  Directory.CreateDirectory(oracleEvidenceDirectory);
  JsonSerializerOptions oracleOptions = new() { WriteIndented = true };
  File.WriteAllText(
    Path.Combine(oracleEvidenceDirectory, "legacy-worldgen-oracle.json"),
    JsonSerializer.Serialize(oracleEvidence, oracleOptions));
  Console.WriteLine(
    $"PASS: legacy oracle WLD v{legacyOracle.Version} " +
    $"{legacyOracle.Metadata.Width}x{legacyOracle.Metadata.Height} " +
    $"fingerprint {oracleEvidence.Fingerprint}");
  Environment.Exit(0);
}

string source = File.ReadAllText(sourcePath);
byte[] sourceBytes = File.ReadAllBytes(sourcePath);
SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, path: sourcePath);
CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
List<MethodInventory> methods = root.DescendantNodes()
  .OfType<MethodDeclarationSyntax>()
  .Select(CreateMethodInventory)
  .OrderBy(method => method.Line)
  .ThenBy(method => method.Name, StringComparer.Ordinal)
  .ToList();
List<FieldInventory> fields = root.DescendantNodes()
  .OfType<FieldDeclarationSyntax>()
  .SelectMany(CreateFieldInventory)
  .OrderBy(field => field.Line)
  .ThenBy(field => field.Name, StringComparer.Ordinal)
  .ToList();
List<ReferenceInventory> references = CreateReferenceInventory(root);
WorldGenerationRequest replayRequest = new(
  new WorldMetadata("worldgen-stage0", new WorldSeed(1456), replayWidth, replayHeight),
  replaySpawnX,
  replaySurfaceY);
WorldGrid firstWorld = new WorldGenerationPipeline().Generate(replayRequest);
WorldGrid secondWorld = new WorldGenerationPipeline().Generate(replayRequest);
WorldGridSnapshot firstSnapshot = firstWorld.CreateSnapshot(replayRequest.Metadata);
WorldGridSnapshot secondSnapshot = secondWorld.CreateSnapshot(replayRequest.Metadata);
string firstFingerprint = CreateSnapshotFingerprint(firstSnapshot);
string secondFingerprint = CreateSnapshotFingerprint(secondSnapshot);
if (!StringComparer.Ordinal.Equals(firstFingerprint, secondFingerprint))
{
  throw new InvalidOperationException("Stage 0 replay was not deterministic.");
}

VerifyTorchDefinitions();
VerifyTileEntityDefinitions();

MethodInventory? generateWorldMethod = methods.SingleOrDefault(method =>
  method.Name == "GenerateWorld" && method.Line == 10108);
if (generateWorldMethod?.Status != "Partial" || generateWorldMethod.Mapping is null ||
    generateWorldMethod.Mapping.TargetMembers.Count != 1 ||
    generateWorldMethod.Mapping.ExcludedLegacyResponsibilities.Count != 3)
{
  throw new InvalidOperationException(
    "The legacy GenerateWorld partial mapping did not retain its provenance boundary.");
}

MethodInventory? emptyLiquidMethod = methods.SingleOrDefault(method =>
  method.Name == "EmptyLiquid" && method.Line == 4454);
MethodInventory? placeLiquidMethod = methods.SingleOrDefault(method =>
  method.Name == "PlaceLiquid" && method.Line == 4478);
MethodInventory? liquidChangeTypeMethod = methods.SingleOrDefault(method =>
  method.Name == "GetLiquidChangeType" && method.Line == 4527);
MethodInventory? pointInWorldMethod = methods.SingleOrDefault(method =>
  method.Name == "InWorld" && method.Line == 8944);
MethodInventory? coordinateInWorldMethod = methods.SingleOrDefault(method =>
  method.Name == "InWorld" && method.Line == 8951);
MethodInventory? rectangleInWorldMethod = methods.SingleOrDefault(method =>
  method.Name == "InWorld" && method.Line == 8962);
MethodInventory? setWorldSizeMethod = methods.SingleOrDefault(method =>
  method.Name == "setWorldSize" && method.Line == 6221);
MethodInventory? getWorldSizeMethod = methods.SingleOrDefault(method =>
  method.Name == "GetWorldSize" && method.Line == 6231);
MethodInventory? setWorldSizeIndexMethod = methods.SingleOrDefault(method =>
  method.Name == "SetWorldSize" && method.Line == 6246);
MethodInventory? areAnyTilesInSetNearbyMethod = methods.SingleOrDefault(method =>
  method.Name == "AreAnyTilesInSetNearby" && method.Line == 8153);
MethodInventory? isTileNearbyMethod = methods.SingleOrDefault(method =>
  method.Name == "IsTileNearby" && method.Line == 8183);
MethodInventory? countTilesMethod = methods.SingleOrDefault(method =>
  method.Name == "countTiles" && method.Line == 8799);
MethodInventory? nextCountMethod = methods.SingleOrDefault(method =>
  method.Name == "nextCount" && method.Line == 8814);
MethodInventory? countDirtTilesMethod = methods.SingleOrDefault(method =>
  method.Name == "countDirtTiles" && method.Line == 8894);
MethodInventory? nextDirtCountMethod = methods.SingleOrDefault(method =>
  method.Name == "nextDirtCount" && method.Line == 8904);
MethodInventory? solidTileValueMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTile" && method.Line == 58615);
MethodInventory? tileEmptyMethod = methods.SingleOrDefault(method =>
  method.Name == "TileEmpty" && method.Line == 58636);
MethodInventory? solidOrSlopedTileValueMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidOrSlopedTile" && method.Line == 58647);
MethodInventory? tileTypeMethod = methods.SingleOrDefault(method =>
  method.Name == "TileType" && method.Line == 58658);
MethodInventory? solidOrSlopedTileCoordinateMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidOrSlopedTile" && method.Line == 58669);
MethodInventory? solidTileCoordinateMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTile" && method.Line == 58770);
MethodInventory? solidTile2ValueMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTile2" && method.Line == 58795);
MethodInventory? platformProperTopFrameMethod = methods.SingleOrDefault(method =>
  method.Name == "PlatformProperTopFrame" && method.Line == 58816);
MethodInventory? solidTileAllowBottomSlopeMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTileAllowBottomSlope" && method.Line == 58832);
MethodInventory? solidTile2CoordinateMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTile2" && method.Line == 59064);
MethodInventory? solidTileNoPlatformsMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTileNoPlatforms" && method.Line == 58858);
MethodInventory? solidTileAllowTopSlopeMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTileAllowTopSlope" && method.Line == 58884);
MethodInventory? solidTileAllowLeftSlopeMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTileAllowLeftSlope" && method.Line == 58906);
MethodInventory? solidTileAllowRightSlopeMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTileAllowRightSlope" && method.Line == 58928);
MethodInventory? solidTile3CoordinateMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTile3" && method.Line == 59038);
MethodInventory? solidTile3ValueMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTile3" && method.Line == 59049);
MethodInventory? hasAnyWireNearbyMethod = methods.SingleOrDefault(method =>
  method.Name == "HasAnyWireNearby" && method.Line == 60717);
MethodInventory? getRopeEndsMethod = methods.SingleOrDefault(method =>
  method.Name == "GetRopeEnds" && method.Line == 58676);
MethodInventory? isRopeCoordinateMethod = methods.SingleOrDefault(method =>
  method.Name == "IsRope" && method.Line == 58736);
MethodInventory? isRopeConvenienceMethod = methods.SingleOrDefault(method =>
  method.Name == "IsRope" && method.Line == 58727);
MethodInventory? countNearBlocksTypesMethod = methods.SingleOrDefault(method =>
  method.Name == "CountNearBlocksTypes" && method.Line == 58214);
MethodInventory? getWorldUpdateRateMethod = methods.SingleOrDefault(method =>
  method.Name == "GetWorldUpdateRate" && method.Line == 59850);
MethodInventory? topAttachMethod = methods.SingleOrDefault(method =>
  method.Name == "TopEdgeCanBeAttachedTo" && method.Line == 58950);
MethodInventory? rightAttachMethod = methods.SingleOrDefault(method =>
  method.Name == "RightEdgeCanBeAttachedTo" && method.Line == 58972);
MethodInventory? leftAttachMethod = methods.SingleOrDefault(method =>
  method.Name == "LeftEdgeCanBeAttachedTo" && method.Line == 58994);
MethodInventory? bottomAttachMethod = methods.SingleOrDefault(method =>
  method.Name == "BottomEdgeCanBeAttachedTo" && method.Line == 59016);
MethodInventory? isSafeFromRainMethod = methods.SingleOrDefault(method =>
  method.Name == "IsSafeFromRain" && method.Line == 60739);
MethodInventory? errorWorldAdjustmentMethod = methods.SingleOrDefault(method =>
  method.Name == "errorWorldAdjustment" && method.Line == 328);
MethodInventory? randomRectangleMethod = methods.SingleOrDefault(method =>
  method.Name == "RandomRectanglePoint" && method.Line == 22181);
MethodInventory? randomRectangleCoordinatesMethod = methods.SingleOrDefault(method =>
  method.Name == "RandomRectanglePoint" && method.Line == 22188);
MethodInventory? randomWorldPointMethod = methods.SingleOrDefault(method =>
  method.Name == "RandomWorldPoint" && method.Line == 22195);
MethodInventory? randomGemMethod = methods.SingleOrDefault(method =>
  method.Name == "randGem" && method.Line == 8997);
MethodInventory? randomGemTileMethod = methods.SingleOrDefault(method =>
  method.Name == "randGemTile" && method.Line == 9009);
MethodInventory? randomMossMethod = methods.SingleOrDefault(method =>
  method.Name == "randMoss" && method.Line == 9028);
MethodInventory? tryGetTreeProfileMethod = methods.SingleOrDefault(method =>
  method.Name == "TryGetFromTreeId" && method.Line == 3951);
MethodInventory? gemTreeGroundTestMethod = methods.SingleOrDefault(method =>
  method.Name == "GemTreeGroundTest" && method.Line == 24863);
MethodInventory? vanityTreeGroundTestMethod = methods.SingleOrDefault(method =>
  method.Name == "VanityTreeGroundTest" && method.Line == 24878);
MethodInventory? ashTreeGroundTestMethod = methods.SingleOrDefault(method =>
  method.Name == "AshTreeGroundTest" && method.Line == 24893);
MethodInventory? defaultTreeWallTestMethod = methods.SingleOrDefault(method =>
  method.Name == "DefaultTreeWallTest" && method.Line == 24815);
MethodInventory? gemTreeWallTestMethod = methods.SingleOrDefault(method =>
  method.Name == "GemTreeWallTest" && method.Line == 24826);
MethodInventory? tryGrowingTreeByTypeMethod = methods.SingleOrDefault(method =>
  method.Name == "TryGrowingTreeByType" && method.Line == 24908);
MethodInventory? growTreeWithSettingsMethod = methods.SingleOrDefault(method =>
  method.Name == "GrowTreeWithSettings" && method.Line == 24955);
MethodInventory? emptyTileCheckMethod = methods.SingleOrDefault(method =>
  method.Name == "EmptyTileCheck" && method.Line == 25960);
MethodInventory? tileTypeFitForTreeMethod = methods.SingleOrDefault(method =>
  method.Name == "IsTileTypeFitForTree" && method.Line == 24245);
MethodInventory? growTreeMethod = methods.SingleOrDefault(method =>
  method.Name == "GrowTree" && method.Line == 24323);
MethodInventory? treeBranchMethod = methods.SingleOrDefault(method =>
  method.Name == "IsTileATreeBranch" && method.Line == 24269);
MethodInventory? treeRootMethod = methods.SingleOrDefault(method =>
  method.Name == "IsTileATreeRoot" && method.Line == 24296);
MethodInventory? leafyTreeTopMethod = methods.SingleOrDefault(method =>
  method.Name == "IsTileALeafyTreeTop" && method.Line == 24227);
MethodInventory? growUndergroundTreeMethod = methods.SingleOrDefault(method =>
  method.Name == "GrowUndergroundTree" && method.Line == 25416);
MethodInventory? getTreeLeafMethod = methods.SingleOrDefault(method =>
  method.Name == "GetTreeLeaf" && method.Line == 24008);
MethodInventory? treeGrowFxCheckMethod = methods.SingleOrDefault(method =>
  method.Name == "TreeGrowFXCheck" && method.Line == 23974);
MethodInventory? orePatchMethod = methods.SingleOrDefault(method =>
  method.Name == "OrePatch" && method.Line == 9624);
MethodInventory? isItATrapMethod = methods.SingleOrDefault(method =>
  method.Name == "IsItATrap" && method.Line == 22015);
MethodInventory? isItATriggerMethod = methods.SingleOrDefault(method =>
  method.Name == "IsItATrigger" && method.Line == 22034);
MethodInventory? dungeonPlatformMethod = methods.SingleOrDefault(method =>
  method.Name == "IsDungeonPlatformOrShelf" && method.Line == 10533);
MethodInventory? atmosphericSurfaceMethod = methods.SingleOrDefault(method =>
  method.Name == "IsSurfaceForAtmospherics" && method.Line == 10033);
MethodInventory? pressurePlateMethod = methods.SingleOrDefault(method =>
  method.Name == "CanGeneratePressurePlateAt" && method.Line == 10072);
MethodInventory? statueStyleItemMethod = methods.SingleOrDefault(method =>
  method.Name == "StatueStyleToItem" && method.Line == 31437);
MethodInventory? nonHammeredPlatformMethod = methods.SingleOrDefault(method =>
  method.Name == "IsBelowANonHammeredPlatform" && method.Line == 31812);
MethodInventory? candleItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Candles" && method.Line == 32845);
MethodInventory? picnicTableItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_PicnicTables" && method.Line == 33374);
MethodInventory? bottleItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Bottles" && method.Line == 34364);
MethodInventory? benchItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Benches" && method.Line == 33296);
MethodInventory? clockItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Clocks" && method.Line == 33214);
MethodInventory? bedItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Beds" && method.Line == 33028);
MethodInventory? candelabraItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Candelabras" && method.Line == 33385);
MethodInventory? bookcaseItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Bookcases" && method.Line == 33567);
MethodInventory? chandelierItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Chandeliers" && method.Line == 33769);
MethodInventory? lanternItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Lanterns" && method.Line == 33968);
MethodInventory? lampItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Lamps" && method.Line == 34184);
MethodInventory? pianoItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Pianos" && method.Line == 34399);
MethodInventory? sinkItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Sinks" && method.Line == 34572);
MethodInventory? tableItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Tables" && method.Line == 35240);
MethodInventory? bathtubItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Bathtubs" && method.Line == 35433);
MethodInventory? workbenchItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Workbenches" && method.Line == 35602);
MethodInventory? chairItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Chair" && method.Line == 35792);
MethodInventory? toiletItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Toilet" && method.Line == 35932);
MethodInventory? platformItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Platforms" && method.Line == 36046);
MethodInventory? musicBoxItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_MusicBoxes" && method.Line == 36259);
MethodInventory? dresserItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Dressers" && method.Line == 42757);
MethodInventory? chestItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Chests" && method.Line == 34701);
MethodInventory? fakeChestItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_FakeChests" && method.Line == 34990);
MethodInventory? campfireItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetCampfireItemDrop" && method.Line == 42943);
MethodInventory? rainbowPaintMethod = methods.SingleOrDefault(method =>
  method.Name == "GetRainbowPaintIDForPosition" && method.Line == 21860);
MethodInventory? lockedDungeonBiomeChestMethod = methods.SingleOrDefault(method =>
  method.Name == "IsLockedDungeonBiomeChest" && method.Line == 29381);
MethodInventory? pileGenerationAttemptsMethod = methods.SingleOrDefault(method =>
  method.Name == "GetPileGenerationAttempts" && method.Line == 21715);
MethodInventory? plantPlacementMethod = methods.SingleOrDefault(method =>
  method.Name == "PlantCheck_CanPlaceHook" && method.Line == 67332);
MethodInventory? plantCheckMethod = methods.SingleOrDefault(method =>
  method.Name == "PlantCheck" && method.Line == 67360);
MethodInventory? plantTypeConversionMethod = methods.SingleOrDefault(method =>
  method.Name == "PlantCheck_TryGetNewType" && method.Line == 67430);
MethodInventory? plantTypeMatchMethod = methods.SingleOrDefault(method =>
  method.Name == "PlantCheck_IsBadTypeMatch" && method.Line == 67434);
MethodInventory? canPoundTileMethod = methods.SingleOrDefault(method =>
  method.Name == "CanPoundTile" && method.Line == 67449);
MethodInventory? forbidsSlopingMethod = methods.SingleOrDefault(method =>
  method.Name == "ForbidsSloping" && method.Line == 67501);
MethodInventory? slopeTileMethod = methods.SingleOrDefault(method =>
  method.Name == "SlopeTile" && method.Line == 67526);
MethodInventory? poundTileMethod = methods.SingleOrDefault(method =>
  method.Name == "PoundTile" && method.Line == 67565);
MethodInventory? tileMergeFrametestSingleMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttemptFrametest" && method.Line == 67602);
MethodInventory? tileMergeFrametestSetMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttemptFrametest" && method.Line == 67656);
MethodInventory? tileMergeCardinalSingleMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttempt" && method.Line == 67710);
MethodInventory? tileMergeCardinalSetMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttempt" && method.Line == 67732);
MethodInventory? tileMergeAllSingleMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttempt" && method.Line == 67754);
MethodInventory? tileMergeAllSetMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttempt" && method.Line == 67792);
MethodInventory? tileMergeAllSingleExcludeMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttempt" && method.Line == 67830);
MethodInventory? tileMergeAllSetExcludeMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttempt" && method.Line == 67868);
MethodInventory? tileMergeWeirdMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttemptWeird" && method.Line == 67906);
MethodInventory? tileMossColorMethod = methods.SingleOrDefault(method =>
  method.Name == "GetTileMossColor" && method.Line == 67944);
if (emptyLiquidMethod?.Status != "Partial" || emptyLiquidMethod.Mapping is null ||
    !emptyLiquidMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/Systems/LiquidChangeCommitSystem.cs:TryCommit"
    }) ||
    placeLiquidMethod?.Status != "Partial" || placeLiquidMethod.Mapping is null ||
    !placeLiquidMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/LiquidPropagationSession.cs:" +
      "Advance",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/LiquidPropagationSystem.cs:" +
      "TryAppendCommands",
      "src/Terraria.Dome.Simulation/World/Systems/LiquidChangeCommitSystem.cs:TryCommit"
    }) ||
    liquidChangeTypeMethod?.Status != "Partial" || liquidChangeTypeMethod.Mapping is null ||
    !liquidChangeTypeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/Liquid/Definitions/" +
      "LiquidInteractionClassifier.cs:GetKind"
    }) ||
    pointInWorldMethod?.Status != "Partial" || pointInWorldMethod.Mapping is null ||
    coordinateInWorldMethod?.Status != "Partial" || coordinateInWorldMethod.Mapping is null ||
    rectangleInWorldMethod?.Status != "Partial" || rectangleInWorldMethod.Mapping is null ||
    setWorldSizeMethod?.Status != "Partial" || setWorldSizeMethod.Mapping is null ||
    getWorldSizeMethod?.Status != "Partial" || getWorldSizeMethod.Mapping is null ||
    setWorldSizeIndexMethod?.Status != "Partial" || setWorldSizeIndexMethod.Mapping is null ||
    areAnyTilesInSetNearbyMethod?.Status != "Partial" ||
    areAnyTilesInSetNearbyMethod.Mapping is null ||
    !areAnyTilesInSetNearbyMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileNeighborhoodQuery.cs:" +
      "AreAnyTilesInSetNearby"
    }) ||
    isTileNearbyMethod?.Status != "Partial" || isTileNearbyMethod.Mapping is null ||
    !isTileNearbyMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileNeighborhoodQuery.cs:IsTileNearby"
    }) ||
    hasAnyWireNearbyMethod?.Status != "Partial" || hasAnyWireNearbyMethod.Mapping is null ||
    !hasAnyWireNearbyMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileWireQuery.cs:HasAnyWireNearby"
    }) ||
    getRopeEndsMethod?.Status != "Partial" || getRopeEndsMethod.Mapping is null ||
    !getRopeEndsMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileRopeQuery.cs:FindEnds"
    }) ||
    isRopeCoordinateMethod?.Status != "Partial" || isRopeCoordinateMethod.Mapping is null ||
    !isRopeCoordinateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileRopeQuery.cs:IsRope"
    }) ||
    isRopeConvenienceMethod?.Status != "Partial" ||
    isRopeConvenienceMethod.Mapping is null ||
    !isRopeConvenienceMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileRopeQuery.cs:IsRope"
    }) ||
    countNearBlocksTypesMethod?.Status != "Partial" || countNearBlocksTypesMethod.Mapping is null ||
    !countNearBlocksTypesMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileNeighborhoodQuery.cs:CountNearbyTileTypes"
    }) ||
    getWorldUpdateRateMethod?.Status != "Partial" || getWorldUpdateRateMethod.Mapping is null ||
    !getWorldUpdateRateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/WorldUpdateRatePolicy.cs:GetRate"
    }) ||
    topAttachMethod?.Status != "Partial" || rightAttachMethod?.Status != "Partial" ||
    leftAttachMethod?.Status != "Partial" || bottomAttachMethod?.Status != "Partial" ||
    topAttachMethod.Mapping is null || rightAttachMethod.Mapping is null ||
    leftAttachMethod.Mapping is null || bottomAttachMethod.Mapping is null ||
    !topAttachMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:CanAttachToTop"
    }) ||
    !rightAttachMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:CanAttachToRight"
    }) ||
    !leftAttachMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:CanAttachToLeft"
    }) ||
    !bottomAttachMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:CanAttachToBottom"
    }) ||
    isSafeFromRainMethod?.Status != "Partial" || isSafeFromRainMethod.Mapping is null ||
    !isSafeFromRainMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileLineTraceQuery.cs:IsSafeFromRainPath"
    }) ||
    errorWorldAdjustmentMethod?.Status != "Partial" || errorWorldAdjustmentMethod.Mapping is null ||
    !errorWorldAdjustmentMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/SecretSeedAdjustmentPolicy.cs:Adjust"
    }) ||
    randomRectangleMethod?.Status != "Partial" || randomRectangleMethod.Mapping is null ||
    randomRectangleCoordinatesMethod?.Status != "Partial" ||
    randomRectangleCoordinatesMethod.Mapping is null ||
    !randomRectangleMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/RandomRectanglePointPolicy.cs:Next"
    }) ||
    !randomRectangleCoordinatesMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/RandomRectanglePointPolicy.cs:Next"
    }) ||
    randomWorldPointMethod?.Status != "Partial" || randomWorldPointMethod.Mapping is null ||
    !randomWorldPointMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/RandomWorldPointPolicy.cs:Next"
    }) ||
    randomGemMethod?.Status != "Partial" || randomGemMethod.Mapping is null ||
    randomGemTileMethod?.Status != "Partial" || randomGemTileMethod.Mapping is null ||
    !randomGemMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/GemTileRandomPolicy.cs:NextGemIndex"
    }) ||
    !randomGemTileMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/GemTileRandomPolicy.cs:NextTile"
    }) ||
    randomMossMethod?.Status != "Partial" || randomMossMethod.Mapping is null ||
    !randomMossMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/MossSelectionPolicy.cs:Next"
    }) ||
    tryGetTreeProfileMethod?.Status != "Partial" || tryGetTreeProfileMethod.Mapping is null ||
    !tryGetTreeProfileMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/Definitions/" +
      "LegacyTreeProfileRegistry.cs:TryGet"
    }) ||
    tryGetTreeProfileMethod.Mapping.ExcludedLegacyResponsibilities.Count != 2 ||
    gemTreeGroundTestMethod?.Status != "Partial" || gemTreeGroundTestMethod.Mapping is null ||
    vanityTreeGroundTestMethod?.Status != "Partial" ||
    vanityTreeGroundTestMethod.Mapping is null ||
    ashTreeGroundTestMethod?.Status != "Partial" || ashTreeGroundTestMethod.Mapping is null ||
    !gemTreeGroundTestMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeGroundSuitabilityQuery.cs:IsSuitable"
    }) ||
    !vanityTreeGroundTestMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeGroundSuitabilityQuery.cs:IsSuitable"
    }) ||
    !ashTreeGroundTestMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeGroundSuitabilityQuery.cs:IsSuitable"
    }) ||
    defaultTreeWallTestMethod?.Status != "Partial" ||
    defaultTreeWallTestMethod.Mapping is null ||
    gemTreeWallTestMethod?.Status != "Partial" || gemTreeWallTestMethod.Mapping is null ||
    !defaultTreeWallTestMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeWallSuitabilityQuery.cs:IsSuitable"
    }) ||
    !gemTreeWallTestMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeWallSuitabilityQuery.cs:IsSuitable"
    }) ||
    tryGrowingTreeByTypeMethod?.Status != "Partial" ||
    tryGrowingTreeByTypeMethod.Mapping is null ||
    !tryGrowingTreeByTypeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeGrowthDispatchQuery.cs:TryGet"
    }) ||
    growTreeWithSettingsMethod?.Status != "Partial" ||
    growTreeWithSettingsMethod.Mapping is null ||
    !growTreeWithSettingsMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeProfileGrowthEligibilityQuery.cs:Evaluate",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "TreeProfileTrunkCommandSystem.cs:TryAppendCommands"
    }) ||
    emptyTileCheckMethod?.Status != "Partial" || emptyTileCheckMethod.Mapping is null ||
    !emptyTileCheckMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeCanopyClearanceQuery.cs:IsClear"
    }) ||
    tileTypeFitForTreeMethod?.Status != "Partial" ||
    tileTypeFitForTreeMethod.Mapping is null ||
    !tileTypeFitForTreeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "OrdinaryTreeGroundQuery.cs:IsSuitable"
    }) ||
    growTreeMethod?.Status != "Partial" || growTreeMethod.Mapping is null ||
    !growTreeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "OrdinaryTreeGrowthEligibilityQuery.cs:Evaluate",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "OrdinaryTreeHeightPolicy.cs:Next",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrdinaryTreeTrunkCommandSystem.cs:TryAppendCommands",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrdinaryTreePlacementSystem.cs:TryPrepare",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrdinaryTreePlacementSystem.cs:TryPrepareWithHeightSelection",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrdinaryTreePlacementSystem.cs:TryAppendCommands"
    }) ||
    treeBranchMethod?.Status != "Partial" || treeBranchMethod.Mapping is null ||
    !treeBranchMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeTrunkFrameQuery.cs:TryGetBranchOffset"
    }) ||
    treeRootMethod?.Status != "Partial" || treeRootMethod.Mapping is null ||
    !treeRootMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeTrunkFrameQuery.cs:TryGetRootOffset"
    }) ||
    leafyTreeTopMethod?.Status != "Partial" || leafyTreeTopMethod.Mapping is null ||
    !leafyTreeTopMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeLeafFrameQuery.cs:IsLeafyTreeTop"
    }) ||
    growUndergroundTreeMethod?.Status != "Partial" ||
    growUndergroundTreeMethod.Mapping is null ||
    !growUndergroundTreeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "UndergroundTreeGrowthEligibilityQuery.cs:Evaluate",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "UndergroundTreeHeightPolicy.cs:Next",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "UndergroundTreeTrunkCommandSystem.cs:TryAppendCommands"
    }) ||
    getTreeLeafMethod?.Status != "Partial" || getTreeLeafMethod.Mapping is null ||
    !getTreeLeafMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeLeafPassStyleQuery.cs:Evaluate"
    }) ||
    treeGrowFxCheckMethod?.Status != "Partial" || treeGrowFxCheckMethod.Mapping is null ||
    !treeGrowFxCheckMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeLeafScanQuery.cs:Scan"
    }) ||
    orePatchMethod?.Status != "Partial" || orePatchMethod.Mapping is null ||
    !orePatchMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "OrePatchEligibilityQuery.cs:Evaluate",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrePatchPlacementSystem.cs:TryPrepare",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrePatchPlacementSystem.cs:TryAppendCommands"
    }) ||
    isItATrapMethod?.Status != "Partial" || isItATrapMethod.Mapping is null ||
    !isItATrapMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileWiringQuery.cs:IsItATrap"
    }) ||
    isItATriggerMethod?.Status != "Partial" || isItATriggerMethod.Mapping is null ||
    !isItATriggerMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileWiringQuery.cs:IsItATrigger"
    }) ||
    dungeonPlatformMethod?.Status != "Partial" || dungeonPlatformMethod.Mapping is null ||
    !dungeonPlatformMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/DungeonPlatformQuery.cs:IsPlatformOrShelf"
    }) ||
    atmosphericSurfaceMethod?.Status != "Partial" || atmosphericSurfaceMethod.Mapping is null ||
    !atmosphericSurfaceMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/AtmosphericSurfaceQuery.cs:" +
      "IsSurfaceForAtmospherics"
    }) ||
    pressurePlateMethod?.Status != "Partial" || pressurePlateMethod.Mapping is null ||
    !pressurePlateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PressurePlatePlacementQuery.cs:" +
      "CanGenerateAt"
    }) ||
    statueStyleItemMethod?.Status != "Partial" || statueStyleItemMethod.Mapping is null ||
    !statueStyleItemMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/StatueStyleItemQuery.cs:ToItem"
    }) ||
    nonHammeredPlatformMethod?.Status != "Partial" ||
    nonHammeredPlatformMethod.Mapping is null ||
    !nonHammeredPlatformMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PlatformSupportQuery.cs:" +
      "IsBelowANonHammeredPlatform"
    }) ||
    candleItemDropMethod?.Status != "Partial" || candleItemDropMethod.Mapping is null ||
    !candleItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/CandleItemDropQuery.cs:ToItem"
    }) ||
    picnicTableItemDropMethod?.Status != "Partial" ||
    picnicTableItemDropMethod.Mapping is null ||
    !picnicTableItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PicnicTableItemDropQuery.cs:ToItem"
    }) ||
    bottleItemDropMethod?.Status != "Partial" || bottleItemDropMethod.Mapping is null ||
    !bottleItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/BottleItemDropQuery.cs:ToItem"
    }) ||
    benchItemDropMethod?.Status != "Partial" || benchItemDropMethod.Mapping is null ||
    !benchItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/BenchItemDropQuery.cs:ToItem"
    }) ||
    clockItemDropMethod?.Status != "Partial" || clockItemDropMethod.Mapping is null ||
    !clockItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/ClockItemDropQuery.cs:ToItem"
    }) ||
    bedItemDropMethod?.Status != "Partial" || bedItemDropMethod.Mapping is null ||
    !bedItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/BedItemDropQuery.cs:ToItem"
    }) ||
    candelabraItemDropMethod?.Status != "Partial" ||
    candelabraItemDropMethod.Mapping is null ||
    !candelabraItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/CandelabraItemDropQuery.cs:ToItem"
    }) ||
    bookcaseItemDropMethod?.Status != "Partial" || bookcaseItemDropMethod.Mapping is null ||
    !bookcaseItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/BookcaseItemDropQuery.cs:ToItem"
    }) ||
    chandelierItemDropMethod?.Status != "Partial" ||
    chandelierItemDropMethod.Mapping is null ||
    !chandelierItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/ChandelierItemDropQuery.cs:ToItem"
    }) ||
    lanternItemDropMethod?.Status != "Partial" || lanternItemDropMethod.Mapping is null ||
    !lanternItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/LanternItemDropQuery.cs:ToItem"
    }) ||
    lampItemDropMethod?.Status != "Partial" || lampItemDropMethod.Mapping is null ||
    !lampItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/LampItemDropQuery.cs:ToItem"
    }) ||
    pianoItemDropMethod?.Status != "Partial" || pianoItemDropMethod.Mapping is null ||
    !pianoItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PianoItemDropQuery.cs:ToItem"
    }) ||
    sinkItemDropMethod?.Status != "Partial" || sinkItemDropMethod.Mapping is null ||
    !sinkItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/SinkItemDropQuery.cs:ToItem"
    }) ||
    tableItemDropMethod?.Status != "Partial" || tableItemDropMethod.Mapping is null ||
    !tableItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TableItemDropQuery.cs:ToItem"
    }) ||
    bathtubItemDropMethod?.Status != "Partial" || bathtubItemDropMethod.Mapping is null ||
    !bathtubItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/BathtubItemDropQuery.cs:ToItem"
    }) ||
    workbenchItemDropMethod?.Status != "Partial" || workbenchItemDropMethod.Mapping is null ||
    !workbenchItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/WorkbenchItemDropQuery.cs:ToItem"
    }) ||
    chairItemDropMethod?.Status != "Partial" || chairItemDropMethod.Mapping is null ||
    !chairItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/ChairItemDropQuery.cs:ToItem"
    }) ||
    toiletItemDropMethod?.Status != "Partial" || toiletItemDropMethod.Mapping is null ||
    !toiletItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/ToiletItemDropQuery.cs:ToItem"
    }) ||
    platformItemDropMethod?.Status != "Partial" || platformItemDropMethod.Mapping is null ||
    !platformItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PlatformItemDropQuery.cs:ToItem"
    }) ||
    musicBoxItemDropMethod?.Status != "Partial" || musicBoxItemDropMethod.Mapping is null ||
    !musicBoxItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/MusicBoxItemDropQuery.cs:ToItem"
    }) ||
    dresserItemDropMethod?.Status != "Partial" || dresserItemDropMethod.Mapping is null ||
    !dresserItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/DresserItemDropQuery.cs:ToItem"
    }) ||
    chestItemDropMethod?.Status != "Partial" || chestItemDropMethod.Mapping is null ||
    !chestItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/ChestItemDropQuery.cs:ToItem"
    }) ||
    fakeChestItemDropMethod?.Status != "Partial" || fakeChestItemDropMethod.Mapping is null ||
    !fakeChestItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/FakeChestItemDropQuery.cs:ToItem"
    }) ||
    campfireItemDropMethod?.Status != "Partial" || campfireItemDropMethod.Mapping is null ||
    !campfireItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/CampfireItemDropQuery.cs:ToItem"
    }) ||
    rainbowPaintMethod?.Status != "Partial" || rainbowPaintMethod.Mapping is null ||
    !rainbowPaintMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/RainbowPaintQuery.cs:ToPaintId"
    }) ||
    lockedDungeonBiomeChestMethod?.Status != "Partial" ||
    lockedDungeonBiomeChestMethod.Mapping is null ||
    !lockedDungeonBiomeChestMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/DungeonChestQuery.cs:" +
      "IsLockedBiomeChest"
    }) ||
    pileGenerationAttemptsMethod?.Status != "Partial" ||
    pileGenerationAttemptsMethod.Mapping is null ||
    !pileGenerationAttemptsMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PileGenerationAttemptPolicy.cs:" +
      "GetAttempts"
    }) ||
    plantPlacementMethod?.Status != "Partial" || plantPlacementMethod.Mapping is null ||
    !plantPlacementMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PlantPlacementQuery.cs:CanPlace"
    }) ||
    plantCheckMethod?.Status != "Partial" || plantCheckMethod.Mapping is null ||
    !plantCheckMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PlantCheckQuery.cs:Evaluate",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/PlantCheckCommandSystem.cs:" +
      "TryCreateCommand",
      "src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs:TryCommit"
    }) ||
    plantTypeConversionMethod?.Status != "Partial" ||
    plantTypeConversionMethod.Mapping is null ||
    !plantTypeConversionMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PlantTypeConversionQuery.cs:Evaluate"
    }) ||
    plantTypeMatchMethod?.Status != "Partial" || plantTypeMatchMethod.Mapping is null ||
    !plantTypeMatchMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PlantTypeConversionQuery.cs:" +
      "IsBadTypeMatch"
    }) ||
    canPoundTileMethod?.Status != "Partial" || canPoundTileMethod.Mapping is null ||
    !canPoundTileMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TilePoundingEligibilityQuery.cs:" +
      "CanPound"
    }) ||
    forbidsSlopingMethod?.Status != "Partial" || forbidsSlopingMethod.Mapping is null ||
    !forbidsSlopingMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileSlopingQuery.cs:ForbidsSloping"
    }) ||
    slopeTileMethod?.Status != "Partial" || slopeTileMethod.Mapping is null ||
    !slopeTileMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/TilePoundingCommandSystem.cs:" +
      "TryCreateSlopeCommand",
      "src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs:TryCommit"
    }) ||
    poundTileMethod?.Status != "Partial" || poundTileMethod.Mapping is null ||
    !poundTileMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/TilePoundingCommandSystem.cs:" +
      "TryCreatePoundCommand",
      "src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs:TryCommit"
    }) ||
    tileMergeFrametestSingleMethod?.Status != "Partial" ||
    tileMergeFrametestSingleMethod.Mapping is null ||
    !tileMergeFrametestSingleMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ApplyFrametest"
    }) ||
    tileMergeFrametestSetMethod?.Status != "Partial" ||
    tileMergeFrametestSetMethod.Mapping is null ||
    !tileMergeFrametestSetMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ApplyFrametest"
    }) ||
    tileMergeCardinalSingleMethod?.Status != "Partial" ||
    tileMergeCardinalSingleMethod.Mapping is null ||
    !tileMergeCardinalSingleMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceCardinal"
    }) ||
    tileMergeCardinalSetMethod?.Status != "Partial" ||
    tileMergeCardinalSetMethod.Mapping is null ||
    !tileMergeCardinalSetMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceCardinal"
    }) ||
    tileMergeAllSingleMethod?.Status != "Partial" || tileMergeAllSingleMethod.Mapping is null ||
    !tileMergeAllSingleMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceAll"
    }) ||
    tileMergeAllSetMethod?.Status != "Partial" || tileMergeAllSetMethod.Mapping is null ||
    !tileMergeAllSetMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceAll"
    }) ||
    tileMergeAllSingleExcludeMethod?.Status != "Partial" ||
    tileMergeAllSingleExcludeMethod.Mapping is null ||
    !tileMergeAllSingleExcludeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceAllExcept"
    }) ||
    tileMergeAllSetExcludeMethod?.Status != "Partial" ||
    tileMergeAllSetExcludeMethod.Mapping is null ||
    !tileMergeAllSetExcludeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceAllExcept"
    }) ||
    tileMergeWeirdMethod?.Status != "Partial" || tileMergeWeirdMethod.Mapping is null ||
    !tileMergeWeirdMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceDifferentExcept"
    }) ||
    tileMossColorMethod?.Status != "Partial" || tileMossColorMethod.Mapping is null ||
    !tileMossColorMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/MossColorQuery.cs:GetColor"
    }) ||
    countTilesMethod?.Status != "Partial" || countTilesMethod.Mapping is null ||
    !countTilesMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileRegionProbe.cs:CountOpenTiles"
    }) ||
    nextCountMethod?.Status != "Partial" || nextCountMethod.Mapping is null ||
    !nextCountMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileRegionProbe.cs:CountOpenTiles"
    }) ||
    countDirtTilesMethod?.Status != "Partial" || countDirtTilesMethod.Mapping is null ||
    !countDirtTilesMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileDirtRegionProbe.cs:CountTiles"
    }) ||
    nextDirtCountMethod?.Status != "Partial" || nextDirtCountMethod.Mapping is null ||
    !nextDirtCountMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileDirtRegionProbe.cs:CountTiles"
    }) ||
    solidTileValueMethod?.Status != "Partial" || solidTileValueMethod.Mapping is null ||
    tileEmptyMethod?.Status != "Partial" || tileEmptyMethod.Mapping is null ||
    solidOrSlopedTileValueMethod?.Status != "Partial" ||
    solidOrSlopedTileValueMethod.Mapping is null ||
    tileTypeMethod?.Status != "Partial" || tileTypeMethod.Mapping is null ||
    solidOrSlopedTileCoordinateMethod?.Status != "Partial" ||
    solidOrSlopedTileCoordinateMethod.Mapping is null ||
    solidTileCoordinateMethod?.Status != "Partial" || solidTileCoordinateMethod.Mapping is null ||
    !solidTileValueMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolid"
    }) ||
    !tileEmptyMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsEmpty"
    }) ||
    !solidOrSlopedTileValueMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidOrSloped"
    }) ||
    !tileTypeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:GetActiveTileType"
    }) ||
    !solidOrSlopedTileCoordinateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidOrSloped"
    }) ||
    !solidTileCoordinateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolid"
    }) ||
    solidTile2ValueMethod?.Status != "Partial" || solidTile2ValueMethod.Mapping is null ||
    platformProperTopFrameMethod?.Status != "Partial" ||
    platformProperTopFrameMethod.Mapping is null ||
    solidTileAllowBottomSlopeMethod?.Status != "Partial" ||
    solidTileAllowBottomSlopeMethod.Mapping is null ||
    solidTile2CoordinateMethod?.Status != "Partial" ||
    solidTile2CoordinateMethod.Mapping is null ||
    !solidTile2ValueMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidWithoutPlatformTop"
    }) ||
    !platformProperTopFrameMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsPlatformProperTopFrame"
    }) ||
    !solidTileAllowBottomSlopeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingBottomSlope"
    }) ||
    !solidTile2CoordinateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidWithoutPlatformTop"
    }) ||
    solidTileNoPlatformsMethod?.Status != "Partial" ||
    solidTileNoPlatformsMethod.Mapping is null ||
    solidTileAllowTopSlopeMethod?.Status != "Partial" ||
    solidTileAllowTopSlopeMethod.Mapping is null ||
    solidTileAllowLeftSlopeMethod?.Status != "Partial" ||
    solidTileAllowLeftSlopeMethod.Mapping is null ||
    solidTileAllowRightSlopeMethod?.Status != "Partial" ||
    solidTileAllowRightSlopeMethod.Mapping is null ||
    !solidTileNoPlatformsMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidWithoutPlatforms"
    }) ||
    !solidTileAllowTopSlopeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingTopSlope"
    }) ||
    !solidTileAllowLeftSlopeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingLeftSlope"
    }) ||
    !solidTileAllowRightSlopeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingRightSlope"
    }) ||
    solidTile3CoordinateMethod?.Status != "Partial" ||
    solidTile3CoordinateMethod.Mapping is null ||
    solidTile3ValueMethod?.Status != "Partial" || solidTile3ValueMethod.Mapping is null ||
    !solidTile3CoordinateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:" +
      "IsSolidWithLegacyTile3Semantics"
    }) ||
    !solidTile3ValueMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:" +
      "IsSolidWithLegacyTile3Semantics"
    }))
{
  throw new InvalidOperationException(
    "Bounded legacy method mappings did not retain their ECS targets.");
}

WorldBoundsComponent boundsContract = new(replayWidth, replayHeight);
if (!boundsContract.Contains(0, 0) ||
    boundsContract.Contains(-1, 0) ||
    boundsContract.Contains(0, 0, fluff: 1) ||
    !boundsContract.Contains(1, 1, fluff: 1) ||
    boundsContract.Contains(replayWidth - 1, replayHeight - 1, fluff: 1) ||
    !boundsContract.ContainsRectangle(10, 10, width: 20, height: 20, fluff: 1) ||
    boundsContract.ContainsRectangle(10, 10, width: 390, height: 290, fluff: 1))
{
  throw new InvalidOperationException(
    "World bounds queries did not preserve coordinate, fluff, and rectangle semantics.");
}

WorldGrid neighborhoodWorld = new(replayWidth, replayHeight);
WorldMetadata neighborhoodMetadata = new(
  "worldgen-neighborhood",
  new WorldSeed(1456),
  replayWidth,
  replayHeight);
neighborhoodWorld.TrySetTile(11, 10, new WorldTile(IsActive: true, Type: 5));
neighborhoodWorld.TrySetTile(12, 10, new WorldTile(IsActive: true, Type: 235));
neighborhoodWorld.TrySetTile(10, 11, new WorldTile(IsActive: false, Type: 5));
WorldGridSnapshot neighborhoodSnapshot = neighborhoodWorld.CreateSnapshot(neighborhoodMetadata);
bool[] neighborhoodTileSet = new bool[6];
neighborhoodTileSet[5] = true;
if (!TileNeighborhoodQuery.AreAnyTilesInSetNearby(
      neighborhoodSnapshot,
      10,
      10,
      neighborhoodTileSet,
      distance: 1) ||
    TileNeighborhoodQuery.AreAnyTilesInSetNearby(
      neighborhoodSnapshot,
      10,
      10,
      neighborhoodTileSet,
      distance: 0) ||
    TileNeighborhoodQuery.IsTileNearby(neighborhoodSnapshot, 10, 10, 235, distance: 2))
{
  throw new InvalidOperationException(
    "Tile neighborhood set and special-stride queries did not preserve snapshot semantics.");
}

neighborhoodWorld.TrySetTile(11, 10, new WorldTile(IsActive: false, Type: 5));
neighborhoodWorld.TrySetTile(12, 10, new WorldTile(IsActive: false, Type: 235));
neighborhoodWorld.TrySetTile(11, 11, new WorldTile(IsActive: true, Type: 5));
WorldGridSnapshot boundaryNeighborhoodSnapshot =
  neighborhoodWorld.CreateSnapshot(neighborhoodMetadata);
if (!TileNeighborhoodQuery.IsTileNearby(
      boundaryNeighborhoodSnapshot,
      10,
      10,
      5,
      distance: 1) ||
    TileNeighborhoodQuery.IsTileNearby(
      boundaryNeighborhoodSnapshot,
      10,
      10,
      235,
      distance: 2))
{
  throw new InvalidOperationException(
    "Tile neighborhood queries did not require active tiles or preserve the tile-235 stride.");
}

neighborhoodWorld.TrySetTile(11, 10, new WorldTile(IsActive: true, Type: 235));
WorldGridSnapshot strideNeighborhoodSnapshot = neighborhoodWorld.CreateSnapshot(neighborhoodMetadata);
if (!TileNeighborhoodQuery.IsTileNearby(
      strideNeighborhoodSnapshot,
      10,
      10,
      235,
      distance: 2))
{
  throw new InvalidOperationException(
    "Tile neighborhood queries did not find a tile-235 match on a stride column.");
}

if (!TileNeighborhoodQuery.AreAnyTilesInSetNearby(
      neighborhoodSnapshot,
      0,
      0,
      neighborhoodTileSet,
      distance: 11) ||
    TileNeighborhoodQuery.AreAnyTilesInSetNearby(
      neighborhoodSnapshot,
      10,
      10,
      new[] { true },
      distance: 1))
{
  throw new InvalidOperationException(
    "Tile neighborhood queries did not handle world edges and short tile sets deterministically.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  TileNeighborhoodQuery.AreAnyTilesInSetNearby(
    neighborhoodSnapshot,
    10,
    10,
    neighborhoodTileSet,
    distance: -1));
AssertThrows<ArgumentOutOfRangeException>(() =>
  TileNeighborhoodQuery.IsTileNearby(neighborhoodSnapshot, 10, 10, -1, distance: 1));
Console.WriteLine("PASS: tile neighborhood queries preserve snapshot and legacy stride semantics");

WorldGrid regionProbeWorld = new(replayWidth, replayHeight);
WorldMetadata regionProbeMetadata = new(
  "worldgen-region-probe",
  new WorldSeed(1456),
  replayWidth,
  replayHeight);
_ = regionProbeWorld.TrySetTile(9, 10, new WorldTile(IsActive: true, Type: 70));
_ = regionProbeWorld.TrySetTile(11, 10, new WorldTile(IsActive: true, Type: 1));
_ = regionProbeWorld.TrySetTile(10, 9, new WorldTile(IsActive: true, Type: 147));
_ = regionProbeWorld.TrySetTile(10, 11, new WorldTile(IsActive: true, Type: 53));
WorldGridSnapshot regionProbeSnapshot = regionProbeWorld.CreateSnapshot(regionProbeMetadata);
TileRegionProbeResult categorizedRegion = TileRegionProbe.CountOpenTiles(
  regionProbeSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(maximumTiles: 20));
if (categorizedRegion.TileCount != 1 || categorizedRegion.ReachedLimit ||
    categorizedRegion.ShroomTileCount != 1 || categorizedRegion.RockTileCount != 1 ||
    categorizedRegion.IceTileCount != 1 || categorizedRegion.SandTileCount != 1 ||
    categorizedRegion.LavaTileCount != 0)
{
  throw new InvalidOperationException(
    "Tile region probing did not preserve legacy tile categories and solid boundaries.");
}

_ = regionProbeWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: false, Type: 0, LiquidAmount: 1, LiquidType: 1));
WorldGridSnapshot lavaRegionSnapshot = regionProbeWorld.CreateSnapshot(regionProbeMetadata);
TileRegionProbeResult blockedLavaRegion = TileRegionProbe.CountOpenTiles(
  lavaRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(maximumTiles: 20));
TileRegionProbeResult permittedLavaRegion = TileRegionProbe.CountOpenTiles(
  lavaRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(lavaOk: true, maximumTiles: 20));
if (!blockedLavaRegion.ReachedLimit || blockedLavaRegion.LavaTileCount != 1 ||
    permittedLavaRegion.ReachedLimit || permittedLavaRegion.TileCount != 1 ||
    permittedLavaRegion.LavaTileCount != 1)
{
  throw new InvalidOperationException(
    "Tile region probing did not preserve lava acceptance and termination semantics.");
}

_ = regionProbeWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: false, Type: 0, LiquidAmount: 1, LiquidType: 3));
WorldGridSnapshot shimmerRegionSnapshot = regionProbeWorld.CreateSnapshot(regionProbeMetadata);
TileRegionProbeResult shimmerRegion = TileRegionProbe.CountOpenTiles(
  shimmerRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(jungle: true, lavaOk: true, maximumTiles: 20));
if (!shimmerRegion.ReachedLimit || shimmerRegion.TileCount != 20)
{
  throw new InvalidOperationException(
    "Tile region probing did not preserve shimmer termination semantics.");
}

_ = regionProbeWorld.TrySetTile(10, 10, new WorldTile(IsActive: false, Type: 0, WallType: 1));
WorldGridSnapshot walledRegionSnapshot = regionProbeWorld.CreateSnapshot(regionProbeMetadata);
TileRegionProbeResult walledRegion = TileRegionProbe.CountOpenTiles(
  walledRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(maximumTiles: 20));
TileRegionProbeResult jungleRegion = TileRegionProbe.CountOpenTiles(
  walledRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(jungle: true, maximumTiles: 20));
if (!walledRegion.ReachedLimit || jungleRegion.ReachedLimit || jungleRegion.TileCount != 1)
{
  throw new InvalidOperationException(
    "Tile region probing did not preserve jungle wall handling.");
}

WorldGrid openRegionWorld = new(replayWidth, replayHeight);
WorldGridSnapshot openRegionSnapshot = openRegionWorld.CreateSnapshot(regionProbeMetadata);
TileRegionProbeResult limitedRegion = TileRegionProbe.CountOpenTiles(
  openRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(maximumTiles: 3));
TileRegionProbeResult edgeRegion = TileRegionProbe.CountOpenTiles(
  openRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 1,
  y: 10,
  new TileRegionProbeOptions(maximumTiles: 20));
if (!limitedRegion.ReachedLimit || limitedRegion.TileCount != 3 ||
    !edgeRegion.ReachedLimit || edgeRegion.TileCount != 20)
{
  throw new InvalidOperationException(
    "Tile region probing did not preserve bounded traversal and edge termination.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  new TileRegionProbeOptions(maximumTiles: 0));
Console.WriteLine("PASS: tile region probing preserves bounded legacy traversal semantics");

WorldGrid dirtRegionWorld = new(replayWidth, replayHeight);
WorldMetadata dirtRegionMetadata = new(
  "worldgen-dirt-region-probe",
  new WorldSeed(1456),
  replayWidth,
  replayHeight);
_ = dirtRegionWorld.TrySetTile(10, 10, new WorldTile(IsActive: false, Type: 0, WallType: 2));
_ = dirtRegionWorld.TrySetTile(11, 11, new WorldTile(IsActive: false, Type: 0, WallType: 59));
_ = dirtRegionWorld.TrySetTile(13, 11, new WorldTile(IsActive: false, Type: 0, WallType: 2));
_ = dirtRegionWorld.TrySetTile(9, 10, new WorldTile(IsActive: true, Type: 1, WallType: 2));
WorldGridSnapshot dirtRegionSnapshot = dirtRegionWorld.CreateSnapshot(dirtRegionMetadata);
TileDirtRegionProbeResult dirtRegion = TileDirtRegionProbe.CountTiles(
  dirtRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileDirtRegionProbeOptions(maximumTiles: 20));
if (dirtRegion.TileCount != 3 || dirtRegion.ReachedLimit)
{
  throw new InvalidOperationException(
    "Dirt region probing did not preserve wall eligibility and diagonal/two-column traversal.");
}

TileDirtRegionProbeResult limitedDirtRegion = TileDirtRegionProbe.CountTiles(
  dirtRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileDirtRegionProbeOptions(maximumTiles: 2));
if (!limitedDirtRegion.ReachedLimit || limitedDirtRegion.TileCount != 2)
{
  throw new InvalidOperationException(
    "Dirt region probing did not preserve its maximum-count termination.");
}

_ = dirtRegionWorld.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 147, WallType: 2));
WorldGridSnapshot icyDirtRegionSnapshot = dirtRegionWorld.CreateSnapshot(dirtRegionMetadata);
TileDirtRegionProbeResult icyDirtRegion = TileDirtRegionProbe.CountTiles(
  icyDirtRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileDirtRegionProbeOptions(maximumTiles: 20));
_ = dirtRegionWorld.TrySetTile(10, 10, new WorldTile(IsActive: false, Type: 0, WallType: 83));
WorldGridSnapshot blockedDirtRegionSnapshot = dirtRegionWorld.CreateSnapshot(dirtRegionMetadata);
TileDirtRegionProbeResult blockedDirtRegion = TileDirtRegionProbe.CountTiles(
  blockedDirtRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileDirtRegionProbeOptions(maximumTiles: 20));
if (!icyDirtRegion.ReachedLimit || icyDirtRegion.TileCount != 20 ||
    !blockedDirtRegion.ReachedLimit || blockedDirtRegion.TileCount != 20)
{
  throw new InvalidOperationException(
    "Dirt region probing did not preserve ice and wall termination semantics.");
}

TileDirtRegionProbeResult edgeDirtRegion = TileDirtRegionProbe.CountTiles(
  dirtRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 1,
  y: 10,
  new TileDirtRegionProbeOptions(maximumTiles: 20));
if (!edgeDirtRegion.ReachedLimit || edgeDirtRegion.TileCount != 20)
{
  throw new InvalidOperationException(
    "Dirt region probing did not preserve world-edge termination.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  new TileDirtRegionProbeOptions(maximumTiles: 0));
Console.WriteLine("PASS: dirt region probing preserves bounded legacy traversal semantics");

WorldGrid tileStateWorld = new(replayWidth, replayHeight);
WorldMetadata tileStateMetadata = new(
  "worldgen-tile-state-query",
  new WorldSeed(1456),
  replayWidth,
  replayHeight);
WorldGridSnapshot emptyTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
TileDefinitionRegistry tileDefinitions = TileDefinitionRegistry.CreateVersion4Base();
if (!TileStateQuery.IsEmpty(emptyTileStateSnapshot, 10, 10) ||
    TileStateQuery.GetActiveTileType(emptyTileStateSnapshot, 10, 10) != -1 ||
    TileStateQuery.IsSolid(emptyTileStateSnapshot, tileDefinitions, 10, 10) ||
    TileStateQuery.IsSolidOrSloped(emptyTileStateSnapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve the empty inactive tile semantics.");
}

_ = tileStateWorld.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 1));
WorldGridSnapshot solidTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsEmpty(solidTileStateSnapshot, 10, 10) ||
    TileStateQuery.GetActiveTileType(solidTileStateSnapshot, 10, 10) != 1 ||
    !TileStateQuery.IsSolid(solidTileStateSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidOrSloped(solidTileStateSnapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve active solid tile semantics.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 1, IsInactive: true));
WorldGridSnapshot inactiveTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (!TileStateQuery.IsEmpty(inactiveTileStateSnapshot, 10, 10) ||
    TileStateQuery.GetActiveTileType(inactiveTileStateSnapshot, 10, 10) != 1 ||
    TileStateQuery.IsSolid(inactiveTileStateSnapshot, tileDefinitions, 10, 10) ||
    TileStateQuery.IsSolidOrSloped(inactiveTileStateSnapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve inactive active-tile semantics.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 1, IsHalfBrick: true));
WorldGridSnapshot halfBrickTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolid(halfBrickTileStateSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidOrSloped(halfBrickTileStateSnapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not distinguish solid tiles from half-brick tiles.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 1, Slope: 1));
WorldGridSnapshot slopedTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolid(slopedTileStateSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidOrSloped(slopedTileStateSnapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not distinguish solid tiles from sloped tiles.");
}

_ = tileStateWorld.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 19));
WorldGridSnapshot platformTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolid(platformTileStateSnapshot, tileDefinitions, 10, 10) ||
    TileStateQuery.IsSolidOrSloped(platformTileStateSnapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not exclude solid-top platform tiles.");
}

_ = tileStateWorld.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 10));
WorldGridSnapshot doorTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (!TileStateQuery.IsSolid(doorTileStateSnapshot, tileDefinitions, 10, 10) ||
    TileStateQuery.IsSolid(doorTileStateSnapshot, tileDefinitions, 10, 10, noDoors: true))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve the no-doors solid tile option.");
}

_ = tileStateWorld.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 19));
WorldGridSnapshot platformSolid2Snapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (!TileStateQuery.IsSolidWithoutPlatformTop(platformSolid2Snapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidAllowingBottomSlope(platformSolid2Snapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve platform solid variants.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 19, Slope: 1, FrameX: 0));
WorldGridSnapshot properPlatformSlopeSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
_ = tileStateWorld.TrySetTile(
  11,
  10,
  new WorldTile(IsActive: true, Type: 19, Slope: 1, FrameX: 144));
WorldGridSnapshot improperPlatformSlopeSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (!TileStateQuery.IsSolidWithoutPlatformTop(properPlatformSlopeSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidAllowingBottomSlope(properPlatformSlopeSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidWithoutPlatformTop(improperPlatformSlopeSnapshot, tileDefinitions, 11, 10) ||
    TileStateQuery.IsSolidAllowingBottomSlope(improperPlatformSlopeSnapshot, tileDefinitions, 11, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve platform top-slope frame rules.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 1, Slope: 3));
WorldGridSnapshot bottomSlopeSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolidWithoutPlatformTop(bottomSlopeSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidAllowingBottomSlope(bottomSlopeSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidAllowingBottomSlope(
      bottomSlopeSnapshot,
      tileDefinitions,
      -1,
      10) ||
    !TileStateQuery.IsPlatformProperTopFrame(0) ||
    TileStateQuery.IsPlatformProperTopFrame(144) ||
    !TileStateQuery.IsPlatformProperTopFrame(468))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve bottom-slope and platform-frame semantics.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 1, Slope: 1));
WorldGridSnapshot rightSlopeTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
_ = tileStateWorld.TrySetTile(
  11,
  10,
  new WorldTile(IsActive: true, Type: 1, Slope: 2));
WorldGridSnapshot leftSlopeTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
_ = tileStateWorld.TrySetTile(
  12,
  10,
  new WorldTile(IsActive: true, Type: 1, Slope: 3));
WorldGridSnapshot bottomRightSlopeTileStateSnapshot =
  tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolidAllowingLeftSlope(
      rightSlopeTileStateSnapshot,
      tileDefinitions,
      10,
      10) ||
    !TileStateQuery.IsSolidAllowingRightSlope(
      rightSlopeTileStateSnapshot,
      tileDefinitions,
      10,
      10) ||
    !TileStateQuery.IsSolidAllowingLeftSlope(
      leftSlopeTileStateSnapshot,
      tileDefinitions,
      11,
      10) ||
    TileStateQuery.IsSolidAllowingRightSlope(
      leftSlopeTileStateSnapshot,
      tileDefinitions,
      11,
      10) ||
    TileStateQuery.IsSolidAllowingTopSlope(
      bottomRightSlopeTileStateSnapshot,
      tileDefinitions,
      12,
      10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve directional slope predicates.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 19, IsHalfBrick: true));
WorldGridSnapshot platformTopSlopeSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
_ = tileStateWorld.TrySetTile(
  11,
  10,
  new WorldTile(IsActive: true, Type: 1, IsHalfBrick: true));
WorldGridSnapshot halfBrickTopSlopeSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolidWithoutPlatforms(
      platformTopSlopeSnapshot,
      tileDefinitions,
      10,
      10) ||
    !TileStateQuery.IsSolidAllowingTopSlope(
      platformTopSlopeSnapshot,
      tileDefinitions,
      10,
      10) ||
    !TileStateQuery.IsSolidAllowingTopSlope(
      halfBrickTopSlopeSnapshot,
      tileDefinitions,
      11,
      10) ||
    TileStateQuery.IsSolidAllowingLeftSlope(
      halfBrickTopSlopeSnapshot,
      tileDefinitions,
      11,
      10) ||
    !TileStateQuery.IsSolidWithoutPlatforms(
      halfBrickTopSlopeSnapshot,
      tileDefinitions,
      11,
      10) ||
    !TileStateQuery.IsSolidWithoutPlatforms(
      halfBrickTopSlopeSnapshot,
      tileDefinitions,
      -1,
      10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve platform and half-brick slope predicates.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 1, IsHalfBrick: true, Slope: 2));
WorldGridSnapshot solidTile3Snapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (!TileStateQuery.IsSolidWithLegacyTile3Semantics(
      solidTile3Snapshot,
      tileDefinitions,
      10,
      10) ||
    TileStateQuery.IsSolidWithLegacyTile3Semantics(
      solidTile3Snapshot,
      tileDefinitions,
      1,
      10) ||
    TileStateQuery.IsSolidWithLegacyTile3Semantics(
      solidTile3Snapshot,
      tileDefinitions,
      replayWidth - 1,
      10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve SolidTile3 value and one-tile boundary semantics.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 19, Slope: 0));
WorldGridSnapshot platformTile3Snapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolidWithLegacyTile3Semantics(
      platformTile3Snapshot,
      tileDefinitions,
      10,
      10))
{
  throw new InvalidOperationException(
    "Tile state queries did not exclude platforms from SolidTile3 semantics.");
}

Console.WriteLine("PASS: tile state queries preserve bounded legacy solid semantics");

WorldGrid wireQueryWorld = new(replayWidth, replayHeight);
WorldMetadata wireQueryMetadata = new(
  "wire-query",
  new WorldSeed(1456),
  replayWidth,
  replayHeight,
  spawnX: replaySpawnX,
  spawnY: replaySurfaceY);
_ = wireQueryWorld.TrySetTile(0, 0, new WorldTile(false, 0, HasWire: true));
_ = wireQueryWorld.TrySetTile(8, 8, new WorldTile(false, 0, HasWire2: true));
_ = wireQueryWorld.TrySetTile(12, 8, new WorldTile(false, 0, HasWire3: true));
_ = wireQueryWorld.TrySetTile(8, 12, new WorldTile(false, 0, HasWire4: true));
WorldGridSnapshot wireQuerySnapshot = wireQueryWorld.CreateSnapshot(wireQueryMetadata);
if (!TileWireQuery.HasAnyWireNearby(wireQuerySnapshot, 0, 0, boxSpread: 0) ||
    !TileWireQuery.HasAnyWireNearby(wireQuerySnapshot, 10, 10, boxSpread: 2) ||
    !TileWireQuery.HasAnyWireNearby(wireQuerySnapshot, 10, 10, boxSpread: 3) ||
    !TileWireQuery.HasAnyWireNearby(wireQuerySnapshot, -10, -10, boxSpread: 0) ||
    TileWireQuery.HasAnyWireNearby(wireQuerySnapshot, 10, 10, boxSpread: 1) ||
    TileWireQuery.HasAnyWireNearby(wireQuerySnapshot, 20, 20, boxSpread: 2))
{
  throw new InvalidOperationException(
    "Tile wire queries did not preserve bounded legacy rectangle and wire-channel semantics.");
}

Console.WriteLine("PASS: tile wire query preserves bounded legacy wire semantics");

WorldGrid ropeQueryWorld = new(replayWidth, replayHeight);
WorldMetadata ropeQueryMetadata = new(
  "rope-query",
  new WorldSeed(1456),
  replayWidth,
  replayHeight,
  spawnX: replaySpawnX,
  spawnY: replaySurfaceY);
_ = ropeQueryWorld.TrySetTile(20, 19, new WorldTile(true, 213));
_ = ropeQueryWorld.TrySetTile(20, 21, new WorldTile(true, 449));
_ = ropeQueryWorld.TrySetTile(30, 29, new WorldTile(true, 213));
_ = ropeQueryWorld.TrySetTile(30, 31, new WorldTile(true, 353));
_ = ropeQueryWorld.TrySetTile(30, 30, new WorldTile(true, 19));
_ = ropeQueryWorld.TrySetTile(40, 39, new WorldTile(true, 213));
_ = ropeQueryWorld.TrySetTile(40, 41, new WorldTile(true, 1));
WorldGridSnapshot ropeQuerySnapshot = ropeQueryWorld.CreateSnapshot(ropeQueryMetadata);
TileRopeEnds directRopeEnds = TileRopeQuery.FindEnds(ropeQuerySnapshot, 20, 20);
TileRopeEnds emptyRopeEnds = TileRopeQuery.FindEnds(
  ropeQuerySnapshot,
  10,
  10,
  treatEmptyAsRopeEnd: true);
if (directRopeEnds.TopY != 19 || directRopeEnds.BottomY != 21 ||
    emptyRopeEnds.TopY != 9 || emptyRopeEnds.BottomY != 11 ||
    !TileRopeQuery.IsRope(ropeQuerySnapshot, tileDefinitions, 20, 19) ||
    !TileRopeQuery.IsRope(ropeQuerySnapshot, tileDefinitions, 30, 30) ||
    TileRopeQuery.IsRope(ropeQuerySnapshot, tileDefinitions, 40, 40) ||
    TileRopeQuery.IsRope(ropeQuerySnapshot, tileDefinitions, 50, 50))
{
  throw new InvalidOperationException(
    "Tile rope queries did not preserve bounded rope and platform-bridge semantics.");
}

Console.WriteLine("PASS: tile rope queries preserve bounded legacy rope semantics");

IReadOnlyList<TileFrameRequest> ropeFrameRequests = TileRopeEndFramingQuery.CreateRequests(
  ropeQuerySnapshot,
  tileDefinitions,
  30,
  30,
  Array.Empty<TileChangeCommand>());
if (ropeFrameRequests.Count != 2 || ropeFrameRequests[0].Y != 29 ||
    ropeFrameRequests[1].Y != 31 ||
    ropeFrameRequests.Any(request => request.MutationKind != TileFrameMutationKind.RopeEnd))
{
  throw new InvalidOperationException("Rope endpoints did not become deterministic frame requests.");
}

WorldGrid countNearbyWorld = new(replayWidth, replayHeight);
WorldMetadata countNearbyMetadata = new(
  "count-nearby",
  new WorldSeed(1456),
  replayWidth,
  replayHeight,
  spawnX: replaySpawnX,
  spawnY: replaySurfaceY);
_ = countNearbyWorld.TrySetTile(0, 0, new WorldTile(true, 7));
_ = countNearbyWorld.TrySetTile(1, 0, new WorldTile(true, 7));
_ = countNearbyWorld.TrySetTile(1, 1, new WorldTile(true, 8));
_ = countNearbyWorld.TrySetTile(2, 2, new WorldTile(false, 7));
WorldGridSnapshot countNearbySnapshot = countNearbyWorld.CreateSnapshot(countNearbyMetadata);
if (TileNeighborhoodQuery.CountNearbyTileTypes(
      countNearbySnapshot,
      0,
      0,
      1,
      Array.Empty<int>()) != 0 ||
    TileNeighborhoodQuery.CountNearbyTileTypes(
      countNearbySnapshot,
      0,
      0,
      1,
      new[] { 7, 7 }) != 2 ||
    TileNeighborhoodQuery.CountNearbyTileTypes(
      countNearbySnapshot,
      0,
      0,
      1,
      new[] { 7, 8 }, cap: 2) != 2 ||
    TileNeighborhoodQuery.CountNearbyTileTypes(
      countNearbySnapshot,
      0,
      0,
      1,
      new[] { 7, 8 }) != 3)
{
  throw new InvalidOperationException(
    "Tile neighborhood counting did not preserve clamping, duplicate-type, and cap semantics.");
}

Console.WriteLine("PASS: tile neighborhood counting preserves bounded legacy semantics");

if (WorldUpdateRatePolicy.GetRate(7, isTimeFrozen: false) != 7 ||
    WorldUpdateRatePolicy.GetRate(30, isTimeFrozen: false) != 24 ||
    WorldUpdateRatePolicy.GetRate(7, isTimeFrozen: true) != 0)
{
  throw new InvalidOperationException("World update-rate policy did not preserve legacy caps.");
}

Console.WriteLine("PASS: world update-rate policy preserves bounded legacy semantics");

WorldGrid attachmentWorld = new(replayWidth, replayHeight);
WorldMetadata attachmentMetadata = new("attachment", new WorldSeed(1456), replayWidth, replayHeight);
_ = attachmentWorld.TrySetTile(10, 10, new WorldTile(true, 1));
_ = attachmentWorld.TrySetTile(11, 10, new WorldTile(true, 387));
_ = attachmentWorld.TrySetTile(12, 10, new WorldTile(true, 1, Slope: 3));
WorldGridSnapshot attachmentSnapshot = attachmentWorld.CreateSnapshot(attachmentMetadata);
if (!TileStateQuery.CanAttachToTop(attachmentSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.CanAttachToRight(attachmentSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.CanAttachToLeft(attachmentSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.CanAttachToBottom(attachmentSnapshot, tileDefinitions, 10, 10) ||
    TileStateQuery.CanAttachToRight(attachmentSnapshot, tileDefinitions, 11, 10) ||
    TileStateQuery.CanAttachToBottom(attachmentSnapshot, tileDefinitions, 12, 10))
{
  throw new InvalidOperationException("Tile attachment queries did not preserve legacy edge semantics.");
}

Console.WriteLine("PASS: tile attachment queries preserve bounded legacy edge semantics");

WorldGrid rainTraceWorld = new(replayWidth, replayHeight);
WorldMetadata rainTraceMetadata = new("rain-trace", new WorldSeed(1456), replayWidth, replayHeight);
WorldGridSnapshot clearRainSnapshot = rainTraceWorld.CreateSnapshot(rainTraceMetadata);
if (!TileLineTraceQuery.IsSafeFromRainPath(
      clearRainSnapshot,
      tileDefinitions,
      100,
      100,
      windSpeedCurrent: 0.0f))
{
  throw new InvalidOperationException("Clear rain trace was incorrectly blocked.");
}

_ = rainTraceWorld.TrySetTile(100, 95, new WorldTile(true, 1));
WorldGridSnapshot blockedRainSnapshot = rainTraceWorld.CreateSnapshot(rainTraceMetadata);
if (TileLineTraceQuery.IsSafeFromRainPath(
      blockedRainSnapshot,
      tileDefinitions,
      100,
      100,
      windSpeedCurrent: 0.0f))
{
  throw new InvalidOperationException("Solid rain trace blocker was not detected.");
}

_ = rainTraceWorld.TrySetTile(100, 95, new WorldTile(true, 1, IsInactive: true));
_ = rainTraceWorld.TrySetTile(100, 94, new WorldTile(true, 19));
WorldGridSnapshot ignoredRainSnapshot = rainTraceWorld.CreateSnapshot(rainTraceMetadata);
if (!TileLineTraceQuery.IsSafeFromRainPath(
      ignoredRainSnapshot,
      tileDefinitions,
      100,
      100,
      windSpeedCurrent: 0.0f))
{
  throw new InvalidOperationException("Inactive or platform rain trace tile was treated as solid.");
}

Console.WriteLine("PASS: snapshot rain trace preserves legacy solid-stop semantics");

if (SecretSeedAdjustmentPolicy.Adjust(10.0, 0) != 4 ||
    SecretSeedAdjustmentPolicy.Adjust(10.0, 1) != 10 ||
    SecretSeedAdjustmentPolicy.Adjust(10.0, 4) != 10 ||
    SecretSeedAdjustmentPolicy.Adjust(10.0, 5) != 20 ||
    SecretSeedAdjustmentPolicy.Adjust(2.5, 5) != 5)
{
  throw new InvalidOperationException("Secret-seed adjustment policy diverged from legacy scaling.");
}

Console.WriteLine("PASS: secret-seed adjustment policy preserves bounded legacy scaling");

RandomRectanglePointResult firstRandomPoint = RandomRectanglePointPolicy.Next(
  new GenerationRandomState(1456),
  10,
  20,
  5,
  7);
RandomRectanglePointResult secondRandomPoint = RandomRectanglePointPolicy.Next(
  new GenerationRandomState(1456),
  10,
  20,
  5,
  7);
if (firstRandomPoint != secondRandomPoint ||
    firstRandomPoint.X < 10 || firstRandomPoint.X >= 15 ||
    firstRandomPoint.Y < 20 || firstRandomPoint.Y >= 27 ||
    firstRandomPoint.State == new GenerationRandomState(1456))
{
  throw new InvalidOperationException(
    "Deterministic rectangle point generation did not preserve bounds and replay state.");
}

Console.WriteLine("PASS: deterministic rectangle point generation preserves bounded replay state");

RandomRectanglePointResult randomWorldPoint = RandomWorldPointPolicy.Next(
  new GenerationRandomState(1456),
  replayRequest.Metadata,
  top: 10,
  right: 20,
  bottom: 30,
  left: 40);
if (randomWorldPoint.X < 40 || randomWorldPoint.X >= replayWidth - 20 ||
    randomWorldPoint.Y < 10 || randomWorldPoint.Y >= replayHeight - 30)
{
  throw new InvalidOperationException("Deterministic world point generation did not preserve insets.");
}

Console.WriteLine("PASS: deterministic world point generation preserves bounded replay state");

bool[] gemFlags = [false, false, true, false, false, false];
(GenerationRandomState gemState, int gemIndex) = GemTileRandomPolicy.NextGemIndex(
  new GenerationRandomState(1456),
  gemFlags);
GemTileRandomResult firstGemTile = GemTileRandomPolicy.NextTile(
  new GenerationRandomState(1456),
  gemFlags);
GemTileRandomResult secondGemTile = GemTileRandomPolicy.NextTile(
  new GenerationRandomState(1456),
  gemFlags);
if (gemIndex != 2 || gemState == new GenerationRandomState(1456) ||
    firstGemTile != secondGemTile ||
    firstGemTile.TileType is not (1 or 63) ||
    firstGemTile.TileType == 63 && firstGemTile.GemIndex != 2)
{
  throw new InvalidOperationException("Deterministic gem selection did not preserve legacy retry rules.");
}

Console.WriteLine("PASS: deterministic gem selection preserves bounded legacy retry semantics");

MossSelectionResult fullMossSelection = MossSelectionPolicy.Next(new GenerationRandomState(1456));
MossSelectionResult neonMossSelection = MossSelectionPolicy.Next(
  new GenerationRandomState(1456),
  justNeon: true);
if (fullMossSelection.NeonMossTileType is not (534 or 536 or 539 or 625) ||
    fullMossSelection.FirstMossType == fullMossSelection.SecondMossType ||
    fullMossSelection.FirstMossType == fullMossSelection.ThirdMossType ||
    fullMossSelection.SecondMossType == fullMossSelection.ThirdMossType ||
    neonMossSelection.FirstMossType != -1 || neonMossSelection.SecondMossType != -1 ||
    neonMossSelection.ThirdMossType != -1 ||
    neonMossSelection.State == fullMossSelection.State)
{
  throw new InvalidOperationException("Deterministic moss selection did not preserve legacy draws.");
}

Console.WriteLine("PASS: deterministic moss selection preserves bounded legacy retry semantics");

Type treeProfileRegistryType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.LegacyTreeProfileRegistry") ??
  throw new InvalidOperationException("The immutable legacy tree profile registry was not found.");
Type treeProfileType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.LegacyTreeProfileDefinition") ??
  throw new InvalidOperationException("The immutable legacy tree profile definition was not found.");
System.Reflection.MethodInfo tryGetTreeProfile = treeProfileRegistryType.GetMethod(
  "TryGet",
  new[] { typeof(ushort), treeProfileType.MakeByRefType() }) ??
  throw new InvalidOperationException("The legacy tree profile lookup contract was not found.");
(ushort TreeTileType, ushort SaplingTileType, string Kind)[] expectedTreeProfiles =
[
  (583, 590, "GemTreeTopaz"),
  (584, 590, "GemTreeAmethyst"),
  (585, 590, "GemTreeSapphire"),
  (586, 590, "GemTreeEmerald"),
  (587, 590, "GemTreeRuby"),
  (588, 590, "GemTreeDiamond"),
  (589, 590, "GemTreeAmber"),
  (596, 595, "VanityTreeSakura"),
  (616, 615, "VanityTreeWillow"),
  (634, 20, "TreeAsh")
];
foreach ((ushort treeTileType, ushort saplingTileType, string kind) in expectedTreeProfiles)
{
  object?[] arguments = [treeTileType, null];
  bool found = (bool)(tryGetTreeProfile.Invoke(null, arguments) ?? false);
  object profile = arguments[1] ??
    throw new InvalidOperationException("A successful tree profile lookup returned no profile.");
  if (!found ||
      (ushort)(treeProfileType.GetProperty("TreeTileType")?.GetValue(profile) ?? -1) !=
        treeTileType ||
      (ushort)(treeProfileType.GetProperty("SaplingTileType")?.GetValue(profile) ?? -1) !=
        saplingTileType ||
      (int)(treeProfileType.GetProperty("MinimumHeight")?.GetValue(profile) ?? -1) != 7 ||
      (int)(treeProfileType.GetProperty("MaximumHeight")?.GetValue(profile) ?? -1) != 12 ||
      (int)(treeProfileType.GetProperty("TopPaddingNeeded")?.GetValue(profile) ?? -1) != 4 ||
      !StringComparer.Ordinal.Equals(
        treeProfileType.GetProperty("Kind")?.GetValue(profile)?.ToString(),
        kind))
  {
    throw new InvalidOperationException("Legacy tree profile lookup diverged from the source profiles.");
  }
}

object?[] unknownTreeProfileArguments = [ushort.MaxValue, null];
if ((bool)(tryGetTreeProfile.Invoke(null, unknownTreeProfileArguments) ?? true) ||
    (ushort)(treeProfileType.GetProperty("TreeTileType")?.GetValue(
      unknownTreeProfileArguments[1]) ?? -1) != 0)
{
  throw new InvalidOperationException("Unknown tree profile IDs must not resolve.");
}

Console.WriteLine("PASS: immutable tree profile lookup preserves bounded legacy profile data");

Type treeGroundDefinitionType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeGroundTileDefinition") ??
  throw new InvalidOperationException("The tree ground definition contract was not found.");
Type treeGroundQueryType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeGroundSuitabilityQuery") ??
  throw new InvalidOperationException("The tree ground suitability query was not found.");
System.Reflection.MethodInfo isTreeGroundSuitable = treeGroundQueryType.GetMethod(
  "IsSuitable",
  new[] { typeof(LegacyTreeProfileKind), treeGroundDefinitionType }) ??
  throw new InvalidOperationException("The tree ground suitability contract was not found.");
System.Reflection.ConstructorInfo treeGroundDefinitionConstructor =
  treeGroundDefinitionType.GetConstructor(
    new[] { typeof(ushort), typeof(bool), typeof(bool), typeof(bool) }) ??
  throw new InvalidOperationException("The tree ground definition constructor was not found.");
object CreateTreeGroundDefinition(ushort tileType, bool isStone, bool isMoss, bool isGrass)
{
  return treeGroundDefinitionConstructor.Invoke([tileType, isStone, isMoss, isGrass]);
}

bool IsTreeGroundSuitable(
  LegacyTreeProfileKind kind,
  ushort tileType,
  bool isStone,
  bool isMoss,
  bool isGrass)
{
  object definition = CreateTreeGroundDefinition(tileType, isStone, isMoss, isGrass);
  return (bool)(isTreeGroundSuitable.Invoke(null, [kind, definition]) ?? false);
}

if (!IsTreeGroundSuitable(LegacyTreeProfileKind.GemTreeTopaz, 1, true, false, false) ||
    !IsTreeGroundSuitable(LegacyTreeProfileKind.GemTreeAmber, 182, false, true, false) ||
    IsTreeGroundSuitable(LegacyTreeProfileKind.GemTreeRuby, 2, false, false, true) ||
    !IsTreeGroundSuitable(LegacyTreeProfileKind.VanityTreeSakura, 2, false, false, true) ||
    IsTreeGroundSuitable(LegacyTreeProfileKind.VanityTreeWillow, 23, false, false, true) ||
    IsTreeGroundSuitable(LegacyTreeProfileKind.VanityTreeWillow, 199, false, false, true) ||
    !IsTreeGroundSuitable(LegacyTreeProfileKind.TreeAsh, 633, false, false, false) ||
    IsTreeGroundSuitable(LegacyTreeProfileKind.TreeAsh, 634, true, true, true))
{
  throw new InvalidOperationException("Tree ground suitability diverged from legacy profile rules.");
}

Console.WriteLine("PASS: tree ground suitability preserves bounded legacy profile rules");

Type treeWallDefinitionType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeWallDefinition") ??
  throw new InvalidOperationException("The tree wall definition contract was not found.");
Type treeWallQueryType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeWallSuitabilityQuery") ??
  throw new InvalidOperationException("The tree wall suitability query was not found.");
System.Reflection.MethodInfo isTreeWallSuitable = treeWallQueryType.GetMethod(
  "IsSuitable",
  new[] { typeof(LegacyTreeProfileKind), treeWallDefinitionType }) ??
  throw new InvalidOperationException("The tree wall suitability contract was not found.");
System.Reflection.ConstructorInfo treeWallDefinitionConstructor =
  treeWallDefinitionType.GetConstructor(new[] { typeof(ushort), typeof(bool) }) ??
  throw new InvalidOperationException("The tree wall definition constructor was not found.");
bool IsTreeWallSuitable(
  LegacyTreeProfileKind kind,
  ushort wallType,
  bool allowsPlantsToGrow)
{
  object wall = treeWallDefinitionConstructor.Invoke([wallType, allowsPlantsToGrow]);
  return (bool)(isTreeWallSuitable.Invoke(null, [kind, wall]) ?? false);
}

if (!IsTreeWallSuitable(LegacyTreeProfileKind.VanityTreeSakura, 700, true) ||
    IsTreeWallSuitable(LegacyTreeProfileKind.VanityTreeWillow, 2, false) ||
    !IsTreeWallSuitable(LegacyTreeProfileKind.GemTreeTopaz, 2, false) ||
    !IsTreeWallSuitable(LegacyTreeProfileKind.GemTreeRuby, 215, false) ||
    IsTreeWallSuitable(LegacyTreeProfileKind.GemTreeDiamond, 216, false) ||
    !IsTreeWallSuitable(LegacyTreeProfileKind.GemTreeAmber, 700, true) ||
    !IsTreeWallSuitable(LegacyTreeProfileKind.TreeAsh, 701, true))
{
  throw new InvalidOperationException("Tree wall suitability diverged from legacy profile rules.");
}

Console.WriteLine("PASS: tree wall suitability preserves bounded legacy profile rules");

Type treeGrowthDispatchType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeGrowthDispatch") ??
  throw new InvalidOperationException("The tree growth dispatch contract was not found.");
Type treeGrowthDispatchKindType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeGrowthDispatchKind") ??
  throw new InvalidOperationException("The tree growth dispatch kind was not found.");
Type treeGrowthDispatchQueryType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeGrowthDispatchQuery") ??
  throw new InvalidOperationException("The tree growth dispatch query was not found.");
System.Reflection.MethodInfo tryGetTreeGrowthDispatch = treeGrowthDispatchQueryType.GetMethod(
  "TryGet",
  new[] { typeof(int), treeGrowthDispatchType.MakeByRefType() }) ??
  throw new InvalidOperationException("The tree growth dispatch lookup contract was not found.");
System.Reflection.PropertyInfo dispatchKindProperty = treeGrowthDispatchType.GetProperty("Kind") ??
  throw new InvalidOperationException("The tree growth dispatch kind property was not found.");
System.Reflection.PropertyInfo dispatchProfileProperty =
  treeGrowthDispatchType.GetProperty("ProfileKind") ??
  throw new InvalidOperationException("The tree growth dispatch profile property was not found.");
object?[] dispatchArguments = [5, null];
if (!(bool)(tryGetTreeGrowthDispatch.Invoke(null, dispatchArguments) ?? false) ||
    !StringComparer.Ordinal.Equals(dispatchKindProperty.GetValue(dispatchArguments[1])?.ToString(), "Ordinary"))
{
  throw new InvalidOperationException("Ordinary tree dispatch did not preserve the legacy handler.");
}

dispatchArguments = [323, null];
if (!(bool)(tryGetTreeGrowthDispatch.Invoke(null, dispatchArguments) ?? false) ||
    !StringComparer.Ordinal.Equals(dispatchKindProperty.GetValue(dispatchArguments[1])?.ToString(), "Palm"))
{
  throw new InvalidOperationException("Palm tree dispatch did not preserve the legacy handler.");
}

dispatchArguments = [634, null];
if (!(bool)(tryGetTreeGrowthDispatch.Invoke(null, dispatchArguments) ?? false) ||
    !StringComparer.Ordinal.Equals(dispatchKindProperty.GetValue(dispatchArguments[1])?.ToString(), "Profile") ||
    !StringComparer.Ordinal.Equals(
      dispatchProfileProperty.GetValue(dispatchArguments[1])?.ToString(),
      "TreeAsh"))
{
  throw new InvalidOperationException("Ash tree dispatch did not preserve the legacy profile.");
}

dispatchArguments = [631, null];
if ((bool)(tryGetTreeGrowthDispatch.Invoke(null, dispatchArguments) ?? true))
{
  throw new InvalidOperationException("Unsupported tree dispatch IDs must not resolve.");
}

Console.WriteLine("PASS: tree growth dispatch preserves bounded legacy handler selection");

Type treeEligibilityReasonType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeProfileGrowthEligibilityReason") ??
  throw new InvalidOperationException("The tree profile eligibility reason was not found.");
Type treeEligibilityResultType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeProfileGrowthEligibilityResult") ??
  throw new InvalidOperationException("The tree profile eligibility result was not found.");
Type treeEligibilityQueryType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeProfileGrowthEligibilityQuery") ??
  throw new InvalidOperationException("The tree profile eligibility query was not found.");
System.Reflection.MethodInfo evaluateTreeEligibility = treeEligibilityQueryType.GetMethod(
  "Evaluate",
  new[]
  {
    typeof(WorldGridSnapshot),
    typeof(LegacyTreeProfileKind),
    typeof(int),
    typeof(int),
    typeof(IReadOnlyDictionary<ushort, TreeGroundTileDefinition>),
    typeof(IReadOnlyDictionary<ushort, TreeWallDefinition>),
    typeof(bool)
  }) ?? throw new InvalidOperationException("The tree profile eligibility contract was not found.");
System.Reflection.PropertyInfo eligibilityReasonProperty =
  treeEligibilityResultType.GetProperty("Reason") ??
  throw new InvalidOperationException("The tree profile eligibility reason property was not found.");
System.Reflection.PropertyInfo eligibilityGroundYProperty =
  treeEligibilityResultType.GetProperty("GroundY") ??
  throw new InvalidOperationException("The tree profile eligibility ground Y property was not found.");
Dictionary<ushort, TreeGroundTileDefinition> treeGroundDefinitions = new()
{
  [1] = new TreeGroundTileDefinition(1, IsStone: true, IsMoss: false, IsGrass: false)
};
Dictionary<ushort, TreeWallDefinition> treeWallDefinitions = new()
{
  [2] = new TreeWallDefinition(2, AllowsPlantsToGrow: false)
};
WorldGrid treeEligibilityWorld = new(replayWidth, replayHeight);
_ = treeEligibilityWorld.TrySetTile(100, 100, new WorldTile(true, 590));
_ = treeEligibilityWorld.TrySetTile(100, 101, new WorldTile(true, 590));
_ = treeEligibilityWorld.TrySetTile(100, 102, new WorldTile(true, 1));
_ = treeEligibilityWorld.TrySetTile(99, 102, new WorldTile(true, 1));
_ = treeEligibilityWorld.TrySetTile(100, 101, new WorldTile(true, 590, WallType: 2));
object EvaluateTreeEligibility(WorldGridSnapshot snapshot, bool ignoreWalls = false)
{
  return evaluateTreeEligibility.Invoke(
    null,
    [
      snapshot,
      LegacyTreeProfileKind.GemTreeTopaz,
      100,
      100,
      treeGroundDefinitions,
      treeWallDefinitions,
      ignoreWalls
    ]) ?? throw new InvalidOperationException("Tree profile eligibility returned no result.");
}

object eligibleTreeResult = EvaluateTreeEligibility(
  treeEligibilityWorld.CreateSnapshot(replayRequest.Metadata));
if (!StringComparer.Ordinal.Equals(eligibilityReasonProperty.GetValue(eligibleTreeResult)?.ToString(), "Eligible") ||
    (int)(eligibilityGroundYProperty.GetValue(eligibleTreeResult) ?? -1) != 102)
{
  throw new InvalidOperationException("Tree profile eligibility did not follow the sapling chain.");
}

_ = treeEligibilityWorld.TrySetLiquid(99, 101, 1, 0);
object liquidBlockedTreeResult = EvaluateTreeEligibility(
  treeEligibilityWorld.CreateSnapshot(replayRequest.Metadata));
if (!StringComparer.Ordinal.Equals(
      eligibilityReasonProperty.GetValue(liquidBlockedTreeResult)?.ToString(),
      "LiquidAboveGround"))
{
  throw new InvalidOperationException("Tree profile eligibility did not reject liquid above ground.");
}

_ = treeEligibilityWorld.TrySetLiquid(99, 101, 0, 0);
_ = treeEligibilityWorld.TrySetTile(100, 102, new WorldTile(true, 1, Slope: 1));
object slopeBlockedTreeResult = EvaluateTreeEligibility(
  treeEligibilityWorld.CreateSnapshot(replayRequest.Metadata));
if (!StringComparer.Ordinal.Equals(
      eligibilityReasonProperty.GetValue(slopeBlockedTreeResult)?.ToString(),
      "GroundShape"))
{
  throw new InvalidOperationException("Tree profile eligibility did not reject sloped ground.");
}

_ = treeEligibilityWorld.TrySetTile(100, 102, new WorldTile(true, 1));
_ = treeEligibilityWorld.TrySetTile(99, 102, new WorldTile());
object neighborBlockedTreeResult = EvaluateTreeEligibility(
  treeEligibilityWorld.CreateSnapshot(replayRequest.Metadata));
if (!StringComparer.Ordinal.Equals(
      eligibilityReasonProperty.GetValue(neighborBlockedTreeResult)?.ToString(),
      "NoSuitableNeighbor"))
{
  throw new InvalidOperationException("Tree profile eligibility did not require a suitable neighbor.");
}

Console.WriteLine("PASS: profile tree eligibility preserves bounded legacy root checks");

Type treeCanopyQueryType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeCanopyClearanceQuery") ??
  throw new InvalidOperationException("The tree canopy clearance query was not found.");
System.Reflection.MethodInfo isTreeCanopyClear = treeCanopyQueryType.GetMethod(
  "IsClear",
  new[]
  {
    typeof(WorldGridSnapshot),
    typeof(int),
    typeof(int),
    typeof(int),
    typeof(int),
    typeof(int),
    typeof(IReadOnlySet<ushort>)
  }) ?? throw new InvalidOperationException("The tree canopy clearance contract was not found.");
HashSet<ushort> commonSaplingTypes = [20, 590, 595, 615];
WorldGrid canopyWorld = new(replayWidth, replayHeight);
WorldGridSnapshot clearCanopySnapshot = canopyWorld.CreateSnapshot(replayRequest.Metadata);
bool InvokeTreeCanopyClear(
  WorldGridSnapshot snapshot,
  int startX,
  int endX,
  int startY,
  int endY,
  int ignoreId)
{
  return (bool)(isTreeCanopyClear.Invoke(
    null,
    [snapshot, startX, endX, startY, endY, ignoreId, commonSaplingTypes]) ?? false);
}

if (!InvokeTreeCanopyClear(clearCanopySnapshot, 10, 12, 10, 12, 20))
{
  throw new InvalidOperationException("Empty tree canopy was incorrectly rejected.");
}

_ = canopyWorld.TrySetTile(11, 11, new WorldTile(true, 3));
if (!InvokeTreeCanopyClear(
      canopyWorld.CreateSnapshot(replayRequest.Metadata),
      10,
      12,
      10,
      12,
      20))
{
  throw new InvalidOperationException("A legacy plant exception was incorrectly rejected.");
}

_ = canopyWorld.TrySetTile(11, 11, new WorldTile(true, 1));
if (InvokeTreeCanopyClear(
      canopyWorld.CreateSnapshot(replayRequest.Metadata),
      10,
      12,
      10,
      12,
      20))
{
  throw new InvalidOperationException("A non-sapling active tile was incorrectly accepted.");
}

if (InvokeTreeCanopyClear(clearCanopySnapshot, -1, 12, 10, 12, 20) ||
    InvokeTreeCanopyClear(clearCanopySnapshot, 10, 12, 10, replayHeight, 20))
{
  throw new InvalidOperationException("Out-of-bounds tree canopy was incorrectly accepted.");
}

Console.WriteLine("PASS: tree canopy clearance preserves bounded legacy empty-tile rules");

WorldGrid profileTrunkWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent profileTrunkState = new(44);
List<TileChangeCommand> profileTrunkCommands = new();
if (!new TreeProfileTrunkCommandSystem().TryAppendCommands(
      profileTrunkWorld.CreateSnapshot(replayRequest.Metadata),
      LegacyTreeProfileKind.GemTreeTopaz,
      originX: 120,
      groundY: 120,
      height: 7,
      new TileProtectionComponent(120, 120, 0, 0),
      ref profileTrunkState,
      profileTrunkCommands) ||
    profileTrunkCommands.Count != 7 ||
    profileTrunkCommands[0].TileType != 583 ||
    profileTrunkCommands[^1].Y != 113)
{
  throw new InvalidOperationException("Profile tree trunk commands were not generated deterministically.");
}

WorldGrid protectedTrunkWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent protectedTrunkState = new(45);
List<TileChangeCommand> protectedTrunkCommands = new();
if (new TreeProfileTrunkCommandSystem().TryAppendCommands(
      protectedTrunkWorld.CreateSnapshot(replayRequest.Metadata),
      LegacyTreeProfileKind.TreeAsh,
      originX: 120,
      groundY: 120,
      height: 7,
      new TileProtectionComponent(120, 113, 2, 7),
      ref protectedTrunkState,
      protectedTrunkCommands) ||
    protectedTrunkCommands.Count != 0)
{
  throw new InvalidOperationException("Protected profile tree trunk cells were not rejected atomically.");
}

Console.WriteLine("PASS: profile tree trunk commands preserve bounded command-only placement");

if (!OrdinaryTreeGroundQuery.IsSuitable(2) ||
    !OrdinaryTreeGroundQuery.IsSuitable(70) ||
    !OrdinaryTreeGroundQuery.IsSuitable(662) ||
    OrdinaryTreeGroundQuery.IsSuitable(1) ||
    OrdinaryTreeGroundQuery.IsSuitable(583))
{
  throw new InvalidOperationException("Ordinary tree ground tile classification diverged from legacy.");
}

Console.WriteLine("PASS: ordinary tree ground classification preserves bounded legacy tile rules");

if (!OrdinaryTreeGrowthEligibilityQuery.IsSuitableWall(
      new TreeWallDefinition(0, true)) ||
    OrdinaryTreeGrowthEligibilityQuery.IsSuitableWall(
      new TreeWallDefinition(1, false)))
{
  throw new InvalidOperationException("Ordinary tree wall classification diverged from legacy.");
}

Console.WriteLine("PASS: ordinary tree wall classification preserves bounded legacy rules");

WorldGrid ordinaryTreeWorld = new(replayWidth, replayHeight);
_ = ordinaryTreeWorld.TrySetTile(160, 100, new WorldTile(true, 20));
_ = ordinaryTreeWorld.TrySetTile(160, 101, new WorldTile(true, 2));
_ = ordinaryTreeWorld.TrySetTile(159, 101, new WorldTile(true, 2));
OrdinaryTreeGrowthEligibilityResult ordinaryTreeEligibility =
  OrdinaryTreeGrowthEligibilityQuery.Evaluate(
    ordinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
    160,
    100,
    new Dictionary<ushort, TreeWallDefinition>(),
    ignoreWalls: false);
if (!ordinaryTreeEligibility.IsEligible || ordinaryTreeEligibility.GroundY != 101)
{
  throw new InvalidOperationException("Ordinary tree eligibility did not preserve legacy root checks.");
}

_ = ordinaryTreeWorld.TrySetLiquid(159, 100, 1, 0);
ordinaryTreeEligibility = OrdinaryTreeGrowthEligibilityQuery.Evaluate(
  ordinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
  160,
  100,
  new Dictionary<ushort, TreeWallDefinition>(),
  ignoreWalls: false);
if (ordinaryTreeEligibility.Reason != OrdinaryTreeGrowthEligibilityReason.LiquidAboveGround)
{
  throw new InvalidOperationException("Ordinary tree eligibility did not reject liquid above ground.");
}

_ = ordinaryTreeWorld.TrySetLiquid(159, 100, 0, 0);
_ = ordinaryTreeWorld.TrySetTile(160, 100, new WorldTile(true, 20, WallType: 1));
ordinaryTreeEligibility = OrdinaryTreeGrowthEligibilityQuery.Evaluate(
  ordinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
  160,
  100,
  new Dictionary<ushort, TreeWallDefinition>(),
  ignoreWalls: false);
if (ordinaryTreeEligibility.Reason != OrdinaryTreeGrowthEligibilityReason.WallUnsuitable)
{
  throw new InvalidOperationException("Ordinary tree eligibility did not reject unsuitable walls.");
}

ordinaryTreeEligibility = OrdinaryTreeGrowthEligibilityQuery.Evaluate(
  ordinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
  160,
  100,
  new Dictionary<ushort, TreeWallDefinition>(),
  ignoreWalls: true);
if (!ordinaryTreeEligibility.IsEligible)
{
  throw new InvalidOperationException("Ordinary tree eligibility did not honor ignoreWalls.");
}

Console.WriteLine("PASS: ordinary tree eligibility preserves bounded legacy root checks");

WorldGrid ordinaryTreeTrunkWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent ordinaryTreeTrunkState = new(46);
List<TileChangeCommand> ordinaryTreeTrunkCommands = new();
if (!new OrdinaryTreeTrunkCommandSystem().TryAppendCommands(
      ordinaryTreeTrunkWorld.CreateSnapshot(replayRequest.Metadata),
      originX: 180,
      groundY: 120,
      height: 5,
      new TileProtectionComponent(180, 120, 0, 0),
      ref ordinaryTreeTrunkState,
      ordinaryTreeTrunkCommands) ||
    ordinaryTreeTrunkCommands.Count != 5 ||
    ordinaryTreeTrunkCommands[0].TileType != 5 ||
    ordinaryTreeTrunkCommands[^1].Y != 115 ||
    ordinaryTreeTrunkWorld.GetTile(180, 119).IsActive)
{
  throw new InvalidOperationException("Ordinary tree trunk commands were not isolated or stable.");
}

WorldGrid blockedOrdinaryTreeWorld = new(replayWidth, replayHeight);
_ = blockedOrdinaryTreeWorld.TrySetTile(180, 114, new WorldTile(true, 1));
WorldGenerationStateComponent blockedOrdinaryTreeState = new(47);
List<TileChangeCommand> blockedOrdinaryTreeCommands = new();
if (new OrdinaryTreeTrunkCommandSystem().TryAppendCommands(
      blockedOrdinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
      originX: 180,
      groundY: 120,
      height: 5,
      new TileProtectionComponent(180, 120, 0, 0),
      ref blockedOrdinaryTreeState,
      blockedOrdinaryTreeCommands) ||
    blockedOrdinaryTreeCommands.Count != 0)
{
  throw new InvalidOperationException("Blocked ordinary tree trunk was not rejected atomically.");
}

Console.WriteLine("PASS: ordinary tree trunk commands preserve bounded command-only placement");

OrdinaryTreePlacementSystem ordinaryTreePlacementSystem = new();
WorldGrid preparedOrdinaryTreeWorld = new(replayWidth, replayHeight);
_ = preparedOrdinaryTreeWorld.TrySetTile(160, 100, new WorldTile(true, 20));
_ = preparedOrdinaryTreeWorld.TrySetTile(160, 101, new WorldTile(true, 2));
_ = preparedOrdinaryTreeWorld.TrySetTile(159, 101, new WorldTile(true, 2));
if (!ordinaryTreePlacementSystem.TryPrepare(
      preparedOrdinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
      160,
      101,
      height: 5,
      new Dictionary<ushort, TreeWallDefinition>(),
      ignoreWalls: false,
      out OrdinaryTreePlacementPreparation ordinaryPreparation,
      out string? ordinaryPreparationFailure) ||
    ordinaryPreparation.GroundY != 101 ||
    ordinaryPreparation.Height != 5 ||
    ordinaryPreparationFailure is not null)
{
  throw new InvalidOperationException(
    $"Ordinary tree placement did not prepare atomically: {ordinaryPreparationFailure}");
}

WorldGenerationStateComponent preparedOrdinaryTreeState = new(48);
List<TileChangeCommand> preparedOrdinaryTreeCommands = new();
if (!ordinaryTreePlacementSystem.TryAppendCommands(
      preparedOrdinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
      ordinaryPreparation,
      new TileProtectionComponent(160, 101, 0, 0),
      ref preparedOrdinaryTreeState,
      preparedOrdinaryTreeCommands) ||
    preparedOrdinaryTreeCommands.Count != 5)
{
  throw new InvalidOperationException("Prepared ordinary tree did not append trunk commands.");
}

Console.WriteLine("PASS: ordinary tree prepare and command phases remain separate");

GenerationRandomState ordinaryTreeHeightState = new(1456);
OrdinaryTreeHeightResult ordinaryTreeHeight = OrdinaryTreeHeightPolicy.Next(
  ordinaryTreeHeightState,
  treeHeightAddon: 3);
OrdinaryTreeHeightResult repeatedOrdinaryTreeHeight = OrdinaryTreeHeightPolicy.Next(
  ordinaryTreeHeightState,
  treeHeightAddon: 3);
if (ordinaryTreeHeight.Height < 8 || ordinaryTreeHeight.Height > 19 ||
    ordinaryTreeHeight.State == ordinaryTreeHeightState ||
    ordinaryTreeHeight != repeatedOrdinaryTreeHeight)
{
  throw new InvalidOperationException("Ordinary tree height was not deterministic or range bounded.");
}

Console.WriteLine("PASS: ordinary tree height preserves bounded deterministic selection");

GenerationRandomState ordinaryTreePrepareRandom = new(73);
if (!ordinaryTreePlacementSystem.TryPrepareWithHeightSelection(
      preparedOrdinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
      160,
      101,
      treeHeightAddon: 0,
      new Dictionary<ushort, TreeWallDefinition>(),
      ignoreWalls: false,
      ref ordinaryTreePrepareRandom,
      out OrdinaryTreePlacementPreparation randomizedOrdinaryPreparation,
      out string? randomizedOrdinaryPreparationFailure) ||
    randomizedOrdinaryPreparation.Height < 5 ||
    randomizedOrdinaryPreparation.Height > 16 ||
    randomizedOrdinaryPreparationFailure is not null ||
    ordinaryTreePrepareRandom == new GenerationRandomState(73))
{
  throw new InvalidOperationException("Ordinary tree preparation did not consume bounded height state.");
}

Console.WriteLine("PASS: ordinary tree preparation consumes bounded height state");

WorldGrid treeFrameWorld = new(replayWidth, replayHeight);
HashSet<ushort> treeTrunkTypes = [5];
_ = treeFrameWorld.TrySetTile(180, 180, new WorldTile(true, 5, FrameX: 44, FrameY: 220));
if (!TreeTrunkFrameQuery.TryGetBranchOffset(
      treeFrameWorld.CreateSnapshot(replayRequest.Metadata),
      180,
      180,
      treeTrunkTypes,
      out int branchOffset) ||
    branchOffset != 1)
{
  throw new InvalidOperationException("Tree branch frame classification diverged from legacy.");
}

_ = treeFrameWorld.TrySetTile(181, 180, new WorldTile(true, 5, FrameX: 22, FrameY: 154));
if (!TreeTrunkFrameQuery.TryGetRootOffset(
      treeFrameWorld.CreateSnapshot(replayRequest.Metadata),
      181,
      180,
      treeTrunkTypes,
      out int rootOffset) ||
    rootOffset != -1 ||
    TreeTrunkFrameQuery.TryGetBranchOffset(
      treeFrameWorld.CreateSnapshot(replayRequest.Metadata),
      1,
      180,
      treeTrunkTypes,
      out _))
{
  throw new InvalidOperationException("Tree root or margin classification diverged from legacy.");
}

Console.WriteLine("PASS: tree branch and root frames preserve bounded legacy offsets");

HashSet<ushort> leafCheckedTypes = [5, 323];
if (!TreeLeafFrameQuery.IsLeafyTreeTop(
      new WorldTile(true, 5, FrameX: 22, FrameY: 220),
      leafCheckedTypes) ||
    !TreeLeafFrameQuery.IsLeafyTreeTop(
      new WorldTile(true, 323, FrameX: 88, FrameY: 0),
      leafCheckedTypes) ||
    TreeLeafFrameQuery.IsLeafyTreeTop(
      new WorldTile(true, 5, FrameX: 66, FrameY: 100),
      leafCheckedTypes) ||
    TreeLeafFrameQuery.IsLeafyTreeTop(
      new WorldTile(false, 5, FrameX: 22, FrameY: 220),
      leafCheckedTypes))
{
  throw new InvalidOperationException("Leafy tree top classification diverged from legacy.");
}

Console.WriteLine("PASS: leafy tree top frames preserve bounded legacy rules");

WorldGrid undergroundTreeWorld = new(replayWidth, replayHeight);
_ = undergroundTreeWorld.TrySetTile(190, 180, new WorldTile(true, 60));
_ = undergroundTreeWorld.TrySetTile(189, 180, new WorldTile(true, 60));
UndergroundTreeGrowthEligibilityResult undergroundTreeEligibility =
  UndergroundTreeGrowthEligibilityQuery.Evaluate(
    undergroundTreeWorld.CreateSnapshot(replayRequest.Metadata),
    190,
    180,
    height: 5);
if (!undergroundTreeEligibility.IsEligible ||
    undergroundTreeEligibility.CanopyTopY != 168)
{
  throw new InvalidOperationException("Underground tree eligibility diverged from legacy roots.");
}

_ = undergroundTreeWorld.TrySetTile(190, 175, new WorldTile(true, 1));
undergroundTreeEligibility = UndergroundTreeGrowthEligibilityQuery.Evaluate(
  undergroundTreeWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  180,
  height: 5);
if (undergroundTreeEligibility.Reason != UndergroundTreeGrowthEligibilityReason.CanopyBlocked)
{
  throw new InvalidOperationException("Underground tree canopy did not reject active tiles.");
}

Console.WriteLine("PASS: underground tree eligibility preserves bounded legacy roots");

if (reducedVerification)
{
  Console.WriteLine(
    "SUMMARY: reduced world-generation verification completed 32 of 81 test sections " +
    "(39.5%); full verification remains available without --reduced");
  Environment.Exit(0);
}

HashSet<int> classifiedTreeTrunkTypes = [5, 323, 583];
if (!TreeTypeClassificationQuery.IsTreeType(5, classifiedTreeTrunkTypes) ||
    TreeTypeClassificationQuery.IsTreeType(-1, classifiedTreeTrunkTypes) ||
    TreeTypeClassificationQuery.IsTreeType(6, classifiedTreeTrunkTypes))
{
  throw new InvalidOperationException(
    "Tree type classification did not preserve the explicit trunk registry semantics.");
}

Console.WriteLine("CHECK: tree type classification preserves bounded registry semantics");

PaintColorValue redPaint = PaintColorQuery.GetColor(1);
PaintColorValue transparentPaint = PaintColorQuery.GetColor(30);
PaintColorValue defaultPaint = PaintColorQuery.GetColor(0);
if (redPaint != new PaintColorValue(byte.MaxValue, 0, 0, byte.MaxValue) ||
    transparentPaint != new PaintColorValue(200, 200, 200, 150) ||
    defaultPaint != new PaintColorValue(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue))
{
  throw new InvalidOperationException("Paint color mapping diverged from legacy RGBA values.");
}

Console.WriteLine("CHECK: paint color mapping preserves bounded legacy RGBA values");

CoatingColorValue illuminantCoating = CoatingColorQuery.GetColor(1);
CoatingColorValue invisibleCoating = CoatingColorQuery.GetColor(2);
CoatingColorValue clearCoating = CoatingColorQuery.GetColor(0);
if (illuminantCoating != new CoatingColorValue(235, 170, byte.MaxValue, byte.MaxValue) ||
    invisibleCoating != new CoatingColorValue(180, 245, byte.MaxValue, byte.MaxValue) ||
    clearCoating != default)
{
  throw new InvalidOperationException("Coating color mapping diverged from legacy RGBA values.");
}

Console.WriteLine("CHECK: coating color mapping preserves bounded legacy RGBA values");

WorldTile coatedTile = new(
  IsActive: true,
  Type: 1,
  IsInvisibleBlock: true,
  IsFullbrightBlock: true,
  IsInvisibleWall: true,
  IsFullbrightWall: false);
CoatingColorSelection blockCoatings = CoatingColorSelectionQuery.Evaluate(coatedTile, block: true);
CoatingColorSelection wallCoatings = CoatingColorSelectionQuery.Evaluate(coatedTile, block: false);
if (blockCoatings != new CoatingColorSelection(true, true) ||
    wallCoatings != new CoatingColorSelection(false, true) ||
    CoatingColorSelectionQuery.Evaluate(null, block: true) != default)
{
  throw new InvalidOperationException("Coating color selection diverged from legacy tile flags.");
}

Console.WriteLine("CHECK: coating color selection preserves bounded tile flag semantics");

ForestBackgroundSet forestStyle = ForestBackgroundSetQuery.Evaluate(72);
ForestBackgroundSet forestDefault = ForestBackgroundSetQuery.Evaluate(0);
ForestBackgroundSet forestSpecial = ForestBackgroundSetQuery.Evaluate(13);
if (forestStyle != new ForestBackgroundSet(176, 177, 178, -1, 52) ||
    forestDefault != new ForestBackgroundSet(7, 8, 9, 10, 11) ||
    forestSpecial != new ForestBackgroundSet(7, -1, 343, 342, 341))
{
  throw new InvalidOperationException("Forest background style mapping diverged from legacy.");
}

Console.WriteLine("CHECK: forest background style mapping preserves bounded legacy sets");

if (HollowTreeFoliageStyleQuery.GetStyle(4) != 19 ||
    HollowTreeFoliageStyleQuery.GetStyle(2) != 20 ||
    HollowTreeFoliageStyleQuery.GetStyle(3) != 20 ||
    HollowTreeFoliageStyleQuery.GetStyle(0) != 3)
{
  throw new InvalidOperationException("Hollow-tree foliage style diverged from legacy mapping.");
}

Console.WriteLine("CHECK: hollow-tree foliage style preserves bounded background mapping");

WorldGrid pileInvalidityWorld = new(replayWidth, replayHeight);
_ = pileInvalidityWorld.TrySetTile(100, 100, new WorldTile(IsActive: true, Type: 26));
HashSet<ushort> boulderTypes = [26, 665];
if (!PilesOrSpeleothemsInvalidityQuery.Evaluate(
      pileInvalidityWorld.CreateSnapshot(replayRequest.Metadata),
      100,
      100,
      boulderTypes) ||
    PilesOrSpeleothemsInvalidityQuery.Evaluate(
      pileInvalidityWorld.CreateSnapshot(replayRequest.Metadata),
      100,
      101,
      boulderTypes) ||
    PilesOrSpeleothemsInvalidityQuery.Evaluate(
      pileInvalidityWorld.CreateSnapshot(replayRequest.Metadata),
      1,
      100,
      boulderTypes))
{
  throw new InvalidOperationException("Pile or speleothem invalidity diverged from legacy.");
}

Console.WriteLine("CHECK: pile or speleothem invalidity preserves bounded boulder rules");

IReadOnlyList<TileFrameRequest> squareFrameRequests = SquareTileFrameRequestQuery.CreateRequests(
  pileInvalidityWorld.CreateSnapshot(replayRequest.Metadata),
  100,
  100,
  TileFrameMutationKind.TileMergeFrametest,
  Array.Empty<TileChangeCommand>());
if (squareFrameRequests.Count != 9 ||
    squareFrameRequests[0].X != 99 || squareFrameRequests[0].Y != 99 ||
    squareFrameRequests[8].X != 101 || squareFrameRequests[8].Y != 101)
{
  throw new InvalidOperationException("Square tile framing request topology diverged from legacy.");
}

Console.WriteLine("CHECK: square tile framing preserves bounded nine-point request order");

IReadOnlyList<WallFrameCoordinate> squareWallCoordinates =
  SquareWallFrameRequestQuery.CreateCoordinates(
    pileInvalidityWorld.CreateSnapshot(replayRequest.Metadata),
    100,
    100);
if (squareWallCoordinates.Count != 9 ||
    squareWallCoordinates[0] != new WallFrameCoordinate(99, 99) ||
    squareWallCoordinates[8] != new WallFrameCoordinate(101, 101))
{
  throw new InvalidOperationException("Square wall framing topology diverged from legacy.");
}

Console.WriteLine("CHECK: square wall framing preserves bounded nine-point topology");

IReadOnlyList<WallFrameCoordinate> rangeFrameCoordinates = RangeFrameCoordinateQuery.CreateCoordinates(
  pileInvalidityWorld.CreateSnapshot(replayRequest.Metadata),
  100,
  100,
  101,
  102);
if (rangeFrameCoordinates.Count != 20 ||
    rangeFrameCoordinates[0] != new WallFrameCoordinate(99, 99) ||
    rangeFrameCoordinates[19] != new WallFrameCoordinate(102, 103))
{
  throw new InvalidOperationException("Range frame coordinate topology diverged from legacy.");
}

Console.WriteLine("CHECK: range framing preserves bounded expanded-rectangle order");

TileMergeNeighbors culledNeighbors = TileMergeCullApplyQuery.Apply(
  new TileMergeCullMask(
    CullUp: true,
    CullDown: false,
    CullLeft: true,
    CullRight: false,
    CullUpLeft: false,
    CullUpRight: true,
    CullDownLeft: false,
    CullDownRight: true),
  new TileMergeNeighbors(1, 2, 3, 4, 5, 6, 7, 8));
if (culledNeighbors != new TileMergeNeighbors(-1, 2, -1, 4, 5, -1, 7, -1))
{
  throw new InvalidOperationException("Tile merge culling application diverged from legacy.");
}

Console.WriteLine("CHECK: tile merge culling application preserves bounded mask semantics");

if (!SpawnAreaClassificationQuery.IsConsidered(
      y: 50,
      isRemixWorld: false,
      remixSurfaceLayerLow: 40,
      remixSurfaceLayerHigh: 80,
      worldSpawnHasBeenRandomized: false,
      hasWorldSurface: true,
      worldSurface: 100,
      underworldLayer: 250) ||
    SpawnAreaClassificationQuery.IsConsidered(
      y: 150,
      isRemixWorld: false,
      remixSurfaceLayerLow: 40,
      remixSurfaceLayerHigh: 80,
      worldSpawnHasBeenRandomized: false,
      hasWorldSurface: true,
      worldSurface: 100,
      underworldLayer: 250) ||
    !SpawnAreaClassificationQuery.IsConsidered(
      y: 150,
      isRemixWorld: false,
      remixSurfaceLayerLow: 40,
      remixSurfaceLayerHigh: 80,
      worldSpawnHasBeenRandomized: true,
      hasWorldSurface: false,
      worldSurface: 100,
      underworldLayer: 250) ||
    !SpawnAreaClassificationQuery.IsConsidered(
      y: 60,
      isRemixWorld: true,
      remixSurfaceLayerLow: 40,
      remixSurfaceLayerHigh: 80,
      worldSpawnHasBeenRandomized: false,
      hasWorldSurface: true,
      worldSurface: 100,
      underworldLayer: 250))
{
  throw new InvalidOperationException("Spawn area classification diverged from legacy rules.");
}

Console.WriteLine("CHECK: spawn area classification preserves bounded world rules");

int[] tileTypeCounts = new int[404];
tileTypeCounts[23] = 3;
tileTypeCounts[27] = 1;
tileTypeCounts[109] = 8;
tileTypeCounts[110] = 2;
if (TileTypeCategoryCountQuery.Evaluate(tileTypeCounts, TileScanGroupKind.Corruption) != 3 - 5 ||
    TileTypeCategoryCountQuery.Evaluate(tileTypeCounts, TileScanGroupKind.Hallow) != 10 ||
    TileTypeCategoryCountQuery.Evaluate(tileTypeCounts, TileScanGroupKind.None) != 0)
{
  throw new InvalidOperationException("Tile category count mapping diverged from legacy formulas.");
}

Console.WriteLine("CHECK: tile category counts preserve bounded legacy formulas");

WorldGrid countWorld = new(replayWidth, replayHeight);
_ = countWorld.TrySetTile(20, 20, new WorldTile(IsActive: true, Type: 23));
_ = countWorld.TrySetTile(21, 20, new WorldTile(IsActive: true, Type: 23));
_ = countWorld.TrySetTile(20, 21, new WorldTile(IsActive: false, Type: 23));
IReadOnlyList<int> countedArea = TileTypeCountAreaQuery.Count(
  countWorld.CreateSnapshot(replayRequest.Metadata),
  20,
  21,
  20,
  21);
if (countedArea[23] != 2 || countedArea[1] != 0)
{
  throw new InvalidOperationException("Tile type area counting diverged from legacy active rules.");
}

Console.WriteLine("CHECK: tile type area counting preserves bounded active-tile semantics");

HousingTestBounds housingBounds = HousingTestBoundsQuery.Calculate(
  roomStartX: 100,
  roomEndX: 120,
  roomStartY: 80,
  roomEndY: 90,
  worldWidth: replayWidth,
  worldHeight: replayHeight);
HousingTestBounds clampedHousingBounds = HousingTestBoundsQuery.Calculate(
  roomStartX: 0,
  roomEndX: 390,
  roomStartY: 0,
  roomEndY: 290,
  worldWidth: replayWidth,
  worldHeight: replayHeight);
if (housingBounds != new HousingTestBounds(54, 166, 36, 134) ||
    clampedHousingBounds != new HousingTestBounds(5, 394, 5, 294))
{
  throw new InvalidOperationException("Housing tested-room bounds diverged from legacy.");
}

Console.WriteLine("CHECK: housing tested-room bounds preserve bounded expansion and clamps");

if (!HousingHomeSpotQuery.IsEligible(new WorldTile(IsActive: false, Type: 379)) ||
    !HousingHomeSpotQuery.IsEligible(new WorldTile(IsActive: true, Type: 1)) ||
    HousingHomeSpotQuery.IsEligible(new WorldTile(IsActive: true, Type: 379)))
{
  throw new InvalidOperationException("Housing home-spot eligibility diverged from legacy.");
}

Console.WriteLine("CHECK: housing home-spot eligibility preserves bounded tile rule");

RoomNeedsResult roomNeeds = RoomNeedsQuery.Evaluate(
  new HashSet<int> { 15, 18, 19, 33 },
  new HashSet<int> { 15 },
  new HashSet<int> { 18 },
  new HashSet<int> { 19 },
  new HashSet<int> { 33 });
RoomNeedsResult missingRoomNeed = RoomNeedsQuery.Evaluate(
  new HashSet<int> { 15, 18, 19 },
  new HashSet<int> { 15 },
  new HashSet<int> { 18 },
  new HashSet<int> { 19 },
  new HashSet<int> { 33 });
if (!roomNeeds.CanSpawn || !roomNeeds.HasChair || !roomNeeds.HasTable ||
    !roomNeeds.HasDoor || !roomNeeds.HasTorch || missingRoomNeed.CanSpawn)
{
  throw new InvalidOperationException("Room-needs classification diverged from legacy.");
}

Console.WriteLine("CHECK: room-needs classification preserves bounded registry semantics");

HashSet<(int X, int Y)> roomTileCoordinates = [(101, 81), (102, 81)];
if (!HousingRoomOccupancyQuery.Contains(roomTileCoordinates, 101, 81) ||
    HousingRoomOccupancyQuery.Contains(roomTileCoordinates, 100, 81))
{
  throw new InvalidOperationException("Housing room occupancy diverged from legacy membership.");
}

Console.WriteLine("CHECK: housing room occupancy preserves bounded coordinate membership");

WorldGrid onTableWorld = new(replayWidth, replayHeight);
_ = onTableWorld.TrySetTile(190, 181, new WorldTile(true, 2));
WorldGridSnapshot onTableSnapshot = onTableWorld.CreateSnapshot(replayRequest.Metadata);
TileDefinitionRegistry onTableDefinitions = TileDefinitionRegistry.CreateVersion4Base();
OnTable1x1ValidationResult onTableSupport = OnTable1x1ValidationQuery.Evaluate(
  onTableSnapshot,
  onTableDefinitions,
  190,
  180,
  type: 49,
  hasTableAnchor: false,
  hasPlatformSideJoin: false);
if (!onTableSupport.IsSupported || onTableSupport.ShouldKill ||
    onTableSupport.UsedTableSupport)
{
  throw new InvalidOperationException("On-table 1x1 solid support contract diverged.");
}

OnTable1x1ValidationResult type78Support = OnTable1x1ValidationQuery.Evaluate(
  onTableSnapshot,
  onTableDefinitions,
  190,
  180,
  type: 78,
  hasTableAnchor: false,
  hasPlatformSideJoin: false);
if (!type78Support.IsSupported || !type78Support.UsedType78BottomSlope)
{
  throw new InvalidOperationException("On-table type-78 bottom-slope contract diverged.");
}

WorldGrid slopedSupportWorld = new(replayWidth, replayHeight);
_ = slopedSupportWorld.TrySetTile(190, 181, new WorldTile(true, 2, Slope: 1));
OnTable1x1ValidationResult slopedSupport = OnTable1x1ValidationQuery.Evaluate(
  slopedSupportWorld.CreateSnapshot(replayRequest.Metadata),
  onTableDefinitions,
  190,
  180,
  type: 49,
  hasTableAnchor: false,
  hasPlatformSideJoin: false);
if (!slopedSupport.ShouldKill || !slopedSupport.HasUnsupportedShape)
{
  throw new InvalidOperationException("On-table unsupported-shape contract diverged.");
}

OnTable1x1ValidationResult repeatedOnTableSupport = OnTable1x1ValidationQuery.Evaluate(
  onTableSnapshot,
  onTableDefinitions,
  190,
  180,
  type: 49,
  hasTableAnchor: false,
  hasPlatformSideJoin: false);
if (repeatedOnTableSupport != onTableSupport)
{
  throw new InvalidOperationException("On-table support evaluation was not deterministic.");
}

Console.WriteLine("CHECK: on-table 1x1 support contract remains deterministic");

WorldGrid sunflowerWorld = new(replayWidth, replayHeight);
TileDefinitionRegistry sunflowerDefinitions = TileDefinitionRegistry.CreateVersion4Base();
for (int offsetX = 0; offsetX < 2; offsetX++)
{
  for (int offsetY = 0; offsetY < 4; offsetY++)
  {
    _ = sunflowerWorld.TrySetTile(
      190 + offsetX,
      170 + offsetY,
      new WorldTile(true, 27, FrameX: (short)(offsetX * 18), FrameY: (short)(offsetY * 18)));
  }

  _ = sunflowerWorld.TrySetTile(190 + offsetX, 174, new WorldTile(true, 2));
}

SunflowerValidationResult sunflower = SunflowerValidationQuery.Evaluate(
  sunflowerWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170);
if (!sunflower.IsValid || sunflower.ShouldKill || !sunflower.HasAllowedGround ||
    sunflower.InvalidTiles != 0)
{
  throw new InvalidOperationException("Sunflower footprint contract diverged.");
}

_ = sunflowerWorld.TrySetTile(191, 173, new WorldTile(true, 27, FrameX: 0, FrameY: 54));
SunflowerValidationResult invalidSunflower = SunflowerValidationQuery.Evaluate(
  sunflowerWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170);
if (invalidSunflower.IsValid || !invalidSunflower.ShouldKill ||
    invalidSunflower.InvalidTiles == 0)
{
  throw new InvalidOperationException("Sunflower invalid-frame contract diverged.");
}

Console.WriteLine("CHECK: sunflower footprint and ground contract remains deterministic");

WorldGrid gnomeWorld = new(replayWidth, replayHeight);
_ = gnomeWorld.TrySetTile(190, 170, new WorldTile(true, 567, FrameY: 0));
_ = gnomeWorld.TrySetTile(190, 171, new WorldTile(true, 567, FrameY: 20));
_ = gnomeWorld.TrySetTile(190, 172, new WorldTile(true, 2));
GnomeValidationResult gnome = GnomeValidationQuery.Evaluate(
  gnomeWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170);
if (!gnome.IsValid || gnome.ShouldKill || !gnome.HasExpectedFootprint ||
    !gnome.HasSupportedGround)
{
  throw new InvalidOperationException("Gnome footprint and ground contract diverged.");
}

_ = gnomeWorld.TrySetTile(190, 171, new WorldTile(true, 567, FrameY: 0));
GnomeValidationResult invalidGnome = GnomeValidationQuery.Evaluate(
  gnomeWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170);
if (invalidGnome.IsValid || !invalidGnome.ShouldKill)
{
  throw new InvalidOperationException("Gnome invalid-frame contract diverged.");
}

Console.WriteLine("CHECK: gnome footprint and ground contract remains deterministic");

WorldGrid anchorWorld = new(replayWidth, replayHeight);
_ = anchorWorld.TrySetTile(190, 171, new WorldTile(true, 2));
_ = anchorWorld.TrySetTile(189, 170, new WorldTile(true, 2));
AnchorOrientationValidationResult anchor = AnchorOrientationValidationQuery.Evaluate(
  anchorWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  style: 0,
  wallType: 0,
  switchToWallIfInvalid: false);
if (!anchor.IsValid || anchor.ShouldKill || anchor.SuggestedStyle != 0)
{
  throw new InvalidOperationException("Anchor bottom orientation contract diverged.");
}

AnchorOrientationValidationResult wallAnchor = AnchorOrientationValidationQuery.Evaluate(
  new WorldGrid(replayWidth, replayHeight).CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  style: 9,
  wallType: 1,
  switchToWallIfInvalid: true);
if (!wallAnchor.IsValid || !wallAnchor.UsedWallFallback || wallAnchor.SuggestedStyle != 4)
{
  throw new InvalidOperationException("Anchor wall fallback contract diverged.");
}

AnchorOrientationValidationResult repeatedAnchor = AnchorOrientationValidationQuery.Evaluate(
  anchorWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  style: 0,
  wallType: 0,
  switchToWallIfInvalid: false);
if (repeatedAnchor != anchor)
{
  throw new InvalidOperationException("Anchor orientation evaluation was not deterministic.");
}

Console.WriteLine("CHECK: anchor orientation contract remains deterministic");

StinkbugBlockerValidationResult stinkbug = StinkbugBlockerValidationQuery.Evaluate(
  anchorWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  style: 2,
  wallType: 0);
if (!stinkbug.IsValid || stinkbug.ShouldKill || stinkbug.SuggestedStyle != 2 ||
    !stinkbug.SwappedHorizontalStyle)
{
  throw new InvalidOperationException("Stinkbug blocker horizontal style contract diverged.");
}

StinkbugBlockerValidationResult invalidStinkbug = StinkbugBlockerValidationQuery.Evaluate(
  new WorldGrid(replayWidth, replayHeight).CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  style: 0,
  wallType: 0);
if (invalidStinkbug.IsValid || !invalidStinkbug.ShouldKill)
{
  throw new InvalidOperationException("Stinkbug blocker invalid-anchor contract diverged.");
}

Console.WriteLine("CHECK: stinkbug blocker orientation contract remains deterministic");

WorldGrid chandelierWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 3; offsetX++)
{
  for (int offsetY = 0; offsetY < 3; offsetY++)
  {
    _ = chandelierWorld.TrySetTile(
      190 + offsetX,
      170 + offsetY,
      new WorldTile(true, 15));
  }
}

_ = chandelierWorld.TrySetTile(191, 169, new WorldTile(true, 2));
ChandelierValidationResult chandelier = ChandelierValidationQuery.Evaluate(
  chandelierWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  type: 15);
if (!chandelier.IsValid || chandelier.ShouldKill || chandelier.Width != 3 ||
    !chandelier.HasSolidSupport)
{
  throw new InvalidOperationException("Chandelier footprint and support contract diverged.");
}

_ = chandelierWorld.TrySetTile(192, 172, new WorldTile(false, 0));
ChandelierValidationResult invalidChandelier = ChandelierValidationQuery.Evaluate(
  chandelierWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  type: 15);
if (invalidChandelier.IsValid || !invalidChandelier.ShouldKill)
{
  throw new InvalidOperationException("Chandelier invalid-footprint contract diverged.");
}

Console.WriteLine("CHECK: chandelier footprint and support contract remains deterministic");

WorldGrid potWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 2; offsetX++)
{
  for (int offsetY = 0; offsetY < 2; offsetY++)
  {
    _ = potWorld.TrySetTile(
      190 + offsetX,
      170 + offsetY,
      new WorldTile(
        true,
        28,
        FrameX: (short)(offsetX * 18),
        FrameY: (short)(offsetY * 18)));
  }

  _ = potWorld.TrySetTile(190 + offsetX, 172, new WorldTile(true, 2));
}

PotValidationResult pot = PotValidationQuery.Evaluate(
  potWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170);
if (!pot.IsValid || pot.ShouldKill || pot.StyleBand != 0)
{
  throw new InvalidOperationException("Pot footprint and support contract diverged.");
}

PotValidationResult type653Pot = PotValidationQuery.Evaluate(
  potWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  type: 653);
if (type653Pot.IsValid || !type653Pot.ShouldKill || !type653Pot.UsedType653BottomSlope)
{
  throw new InvalidOperationException("Pot type-653 support contract diverged.");
}

Console.WriteLine("CHECK: pot footprint and support contract remains deterministic");

WorldGrid palmWorld = new(replayWidth, replayHeight);
_ = palmWorld.TrySetTile(190, 170, new WorldTile(true, 53, FrameX: 66));
_ = palmWorld.TrySetTile(190, 169, new WorldTile(true, 234));
PalmTreeValidationResult palm = PalmTreeValidationQuery.Evaluate(
  palmWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  170);
if (!palm.IsSupported || palm.ShouldKill || palm.NormalizedAboveType != 53 ||
    palm.SuggestedFrameX != 220)
{
  throw new InvalidOperationException("Palm tree support and frame contract diverged.");
}

_ = palmWorld.TrySetTile(190, 169, new WorldTile(true, 1));
PalmTreeValidationResult invalidPalm = PalmTreeValidationQuery.Evaluate(
  palmWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  170);
if (invalidPalm.IsSupported || !invalidPalm.ShouldKill)
{
  throw new InvalidOperationException("Palm tree invalid-ground contract diverged.");
}

Console.WriteLine("CHECK: palm tree support and frame contract remains deterministic");

WorldGrid configuredTreeFrameWorld = new(replayWidth, replayHeight);
_ = configuredTreeFrameWorld.TrySetTile(
  190,
  170,
  new WorldTile(true, 5, FrameX: 66, FrameY: 70));
_ = configuredTreeFrameWorld.TrySetTile(189, 170, new WorldTile(true, 5));
_ = configuredTreeFrameWorld.TrySetTile(191, 170, new WorldTile(true, 5));
_ = configuredTreeFrameWorld.TrySetTile(190, 171, new WorldTile(true, 2));
TreeFrameValidationResult treeFrame = TreeFrameValidationQuery.Evaluate(
  configuredTreeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  170,
  treeType: 5);
if (!treeFrame.IsSupported || treeFrame.ShouldKill || !treeFrame.HasLeftTree ||
    !treeFrame.HasRightTree || treeFrame.SuggestedFrameX != 110)
{
  throw new InvalidOperationException("Tree frame support contract diverged.");
}

_ = configuredTreeFrameWorld.TrySetTile(190, 171, new WorldTile(true, 1));
TreeFrameValidationResult invalidTreeFrame = TreeFrameValidationQuery.Evaluate(
  configuredTreeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  170,
  treeType: 5);
if (invalidTreeFrame.IsSupported || !invalidTreeFrame.ShouldKill)
{
  throw new InvalidOperationException("Tree frame invalid-ground contract diverged.");
}

Console.WriteLine("CHECK: tree frame support and branch contract remains deterministic");

_ = configuredTreeFrameWorld.TrySetTile(190, 171, new WorldTile(true, 2));
TreeSettingsValidationResult configuredTree = TreeSettingsValidationQuery.Evaluate(
  configuredTreeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  170,
  treeType: 5,
  isGroundValid: groundType => groundType == 2);
if (!configuredTree.IsSupported || configuredTree.ShouldKill ||
    !configuredTree.GroundValid || !configuredTree.HasLeftTree ||
    !configuredTree.HasRightTree)
{
  throw new InvalidOperationException("Configured tree ground contract diverged.");
}

TreeSettingsValidationResult invalidConfiguredTree = TreeSettingsValidationQuery.Evaluate(
  configuredTreeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  170,
  treeType: 5,
  isGroundValid: groundType => groundType == 53);
if (invalidConfiguredTree.IsSupported || !invalidConfiguredTree.ShouldKill)
{
  throw new InvalidOperationException("Configured tree invalid-ground contract diverged.");
}

Console.WriteLine("CHECK: configured tree ground contract remains deterministic");

SpecialTownNpcSpawningResult ordinaryTownNpc = SpecialTownNpcSpawningQuery.Evaluate(
  npcType: 17,
  truffleUnlocked: false,
  roomAboveWorldSurface: true,
  noFunctionalSurface: false,
  mushroomTileCount: 0,
  mushroomTileThreshold: 100);
if (!ordinaryTownNpc.IsAllowed || ordinaryTownNpc.UsedTruffleRule)
{
  throw new InvalidOperationException("Ordinary special-town NPC rule diverged.");
}

SpecialTownNpcSpawningResult truffleAllowed = SpecialTownNpcSpawningQuery.Evaluate(
  npcType: 160,
  truffleUnlocked: true,
  roomAboveWorldSurface: true,
  noFunctionalSurface: false,
  mushroomTileCount: 100,
  mushroomTileThreshold: 100);
if (!truffleAllowed.IsAllowed || !truffleAllowed.UsedTruffleRule)
{
  throw new InvalidOperationException("Truffle spawning rule did not allow a valid room.");
}

SpecialTownNpcSpawningResult truffleRejected = SpecialTownNpcSpawningQuery.Evaluate(
  npcType: 160,
  truffleUnlocked: false,
  roomAboveWorldSurface: true,
  noFunctionalSurface: false,
  mushroomTileCount: 99,
  mushroomTileThreshold: 100);
if (truffleRejected.IsAllowed)
{
  throw new InvalidOperationException("Truffle spawning rule accepted insufficient inputs.");
}

Console.WriteLine("CHECK: special-town NPC spawning predicate remains deterministic");

List<int> achievementNpcTypes = new()
{
  38, 17, 107, 19, 22, 124, 228, 178, 18, 229, 209, 54, 108, 160, 20, 369, 207, 227,
  208, 441, 353, 550, 588, 633, 663, 670, 678, 679, 680, 681, 682, 683, 684
};
TownAchievementEligibilityResult completeAchievements = TownAchievementEligibilityQuery.Evaluate(
  achievementNpcTypes);
if (!completeAchievements.RealEstateComplete || !completeAchievements.TownSlimesComplete ||
    completeAchievements.RealEstateMissingCount != 0 ||
    completeAchievements.TownSlimesMissingCount != 0)
{
  throw new InvalidOperationException("Town achievement eligibility contract diverged.");
}

achievementNpcTypes.Remove(670);
TownAchievementEligibilityResult incompleteAchievements = TownAchievementEligibilityQuery.Evaluate(
  achievementNpcTypes);
if (!incompleteAchievements.RealEstateComplete || incompleteAchievements.TownSlimesComplete ||
    incompleteAchievements.TownSlimesMissingCount != 1)
{
  throw new InvalidOperationException("Town slime achievement eligibility contract diverged.");
}

Console.WriteLine("CHECK: town achievement eligibility predicate remains deterministic");

WorldGrid undergroundWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 120; offsetX++)
{
  for (int offsetY = 0; offsetY < 3; offsetY++)
  {
    _ = undergroundWorld.TrySetTile(140 + offsetX, 80 + offsetY, new WorldTile(true, 2));
  }
}

UndergroundClassificationResult denseUnderground = UndergroundClassificationQuery.Evaluate(
  undergroundWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  200,
  160,
  worldSurface: 100,
  currentTileHasWall: false);
if (!denseUnderground.IsUnderground || denseUnderground.SolidTileCount == 0)
{
  throw new InvalidOperationException("Underground dense-window contract diverged.");
}

UndergroundClassificationResult deepUnderground = UndergroundClassificationQuery.Evaluate(
  undergroundWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  200,
  200,
  worldSurface: 100,
  currentTileHasWall: false);
if (!deepUnderground.IsUnderground || !deepUnderground.UsedDeepShortcut ||
    deepUnderground.SolidTileCount != 0 || deepUnderground.ScannedTileCount != 0)
{
  throw new InvalidOperationException("Underground deep shortcut contract diverged.");
}

UndergroundClassificationResult shallowSurface = UndergroundClassificationQuery.Evaluate(
  undergroundWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  200,
  40,
  worldSurface: 100,
  currentTileHasWall: false);
if (shallowSurface.IsUnderground || !shallowSurface.UsedShallowShortcut)
{
  throw new InvalidOperationException("Underground shallow shortcut contract diverged.");
}

Console.WriteLine("CHECK: underground classification contract remains deterministic");

RoomBoundaryValidationResult validRoomBoundary = RoomBoundaryValidationQuery.Evaluate(
  x: 100,
  y: 100,
  worldWidth: 400,
  worldHeight: 300,
  roomTileCount: 20,
  roomMinX: 90,
  roomMaxX: 110,
  roomMinY: 90,
  roomMaxY: 110,
  maxRoomTiles: 200,
  maxRoomSize: 100,
  stopOnFail: true,
  roomTilesContainsPoint: true);
if (!validRoomBoundary.IsAllowed || validRoomBoundary.ShouldStop)
{
  throw new InvalidOperationException("Room boundary contract diverged for valid input.");
}

RoomBoundaryValidationResult edgeRoomBoundary = RoomBoundaryValidationQuery.Evaluate(
  x: 5,
  y: 100,
  worldWidth: 400,
  worldHeight: 300,
  roomTileCount: 20,
  roomMinX: 0,
  roomMaxX: 10,
  roomMinY: 90,
  roomMaxY: 110,
  maxRoomTiles: 200,
  maxRoomSize: 100,
  stopOnFail: true,
  roomTilesContainsPoint: true);
if (edgeRoomBoundary.IsAllowed || !edgeRoomBoundary.TooCloseToWorldEdge)
{
  throw new InvalidOperationException("Room boundary edge contract diverged.");
}

Console.WriteLine("CHECK: room boundary contract remains deterministic");

SecretSeedInputResult normalizedSecretSeed = SecretSeedInputQuery.Evaluate(
  "  My-Seed!! ",
  new List<(string Plaintext, string Code)> { ("myseed", "unused-code") });
if (!normalizedSecretSeed.HasNormalizedInput || !normalizedSecretSeed.IsMatch ||
    normalizedSecretSeed.NormalizedInput != "myseed" ||
    normalizedSecretSeed.DisplayInput != "  MySeed ")
{
  throw new InvalidOperationException("Secret-seed input normalization contract diverged.");
}

SecretSeedInputResult invalidSecretSeed = SecretSeedInputQuery.Evaluate(
  "---",
  Array.Empty<(string Plaintext, string Code)>());
if (invalidSecretSeed.HasNormalizedInput || invalidSecretSeed.IsMatch)
{
  throw new InvalidOperationException("Secret-seed empty-normalization contract diverged.");
}

Console.WriteLine("CHECK: secret-seed input normalization contract remains deterministic");

if (!BackgroundEquivalenceQuery.AreEquivalent(3, 31) ||
    !BackgroundEquivalenceQuery.AreEquivalent(7, 73) ||
    BackgroundEquivalenceQuery.AreEquivalent(3, 5) ||
    !BackgroundEquivalenceQuery.AreEquivalent(12, 12) ||
    BackgroundEquivalenceQuery.AreEquivalent(12, 13))
{
  throw new InvalidOperationException("Background equivalence contract diverged.");
}

Console.WriteLine("CHECK: background equivalence contract remains deterministic");

JungleChestItemSelectionResult jungleItem = JungleChestItemSelectionQuery.Evaluate(6);
if (jungleItem.BaseItemType != 213 || jungleItem.NextJungleItemCount != 7 ||
    !jungleItem.RandomOverrideDeferred || !jungleItem.CounterMutationDeferred)
{
  throw new InvalidOperationException("Jungle chest item rotation contract diverged.");
}

Console.WriteLine("CHECK: jungle chest item rotation contract remains deterministic");

if (!SecretSeedCodeCheckQuery.Matches("  My-Code! ", "mycode") ||
    SecretSeedCodeCheckQuery.Matches("wrong", "mycode") ||
    SecretSeedCodeCheckQuery.Matches("---", "mycode"))
{
  throw new InvalidOperationException("Secret-seed code check contract diverged.");
}

Console.WriteLine("CHECK: secret-seed code check contract remains deterministic");

TileSolidityOverrideProjection solidityOverrides = TileSolidityOverrideQuery.Evaluate(solid: true);
if (!solidityOverrides.Solid || solidityOverrides.BoulderTileTypes.Count != 9 ||
    solidityOverrides.CrackedBrickTileTypes.Count != 3 ||
    !solidityOverrides.BoulderTileTypes.Contains((ushort)138) ||
    !solidityOverrides.CrackedBrickTileTypes.Contains((ushort)483))
{
  throw new InvalidOperationException("Tile solidity override contract diverged.");
}

Console.WriteLine("CHECK: tile solidity override projection remains deterministic");

if (!AlchemyPlantHarvestabilityQuery.IsHarvestable(
      style: 0,
      y: 100,
      dayTime: true,
      bloodMoon: false,
      raining: false,
      cloudAlpha: 0,
      time: 0,
      worldSurface: 200,
      remixWorld: false,
      maxTilesY: 300,
      moonPhase: 2) ||
    AlchemyPlantHarvestabilityQuery.IsHarvestable(
      style: 1,
      y: 100,
      dayTime: true,
      bloodMoon: false,
      raining: false,
      cloudAlpha: 0,
      time: 0,
      worldSurface: 200,
      remixWorld: false,
      maxTilesY: 300,
      moonPhase: 2) ||
    !HarvestableHerbQuery.IsHarvestableWithSeed(84, 0, 100, alchemyPlantHarvestable: false) ||
    HarvestableHerbQuery.IsHarvestableWithSeed(82, 0, 100, alchemyPlantHarvestable: true))
{
  throw new InvalidOperationException("Alchemy herb harvestability contract diverged.");
}

Console.WriteLine("CHECK: alchemy herb harvestability contract remains deterministic");

if (!ChestRiggingQuery.IsRigged(new WorldTile(true, 467, FrameX: 144)) ||
    ChestRiggingQuery.IsRigged(new WorldTile(true, 467, FrameX: 108)) ||
    ChestRiggingQuery.IsRigged(new WorldTile(true, 21, FrameX: 144)))
{
  throw new InvalidOperationException("Chest rigging contract diverged.");
}

Console.WriteLine("CHECK: chest rigging contract remains deterministic");

List<TownNpcSpawnCandidate> townNpcCandidates = new()
{
  new TownNpcSpawnCandidate(17, true, false, true, false, false, true),
  new TownNpcSpawnCandidate(18, true, false, true, true, false, false),
  new TownNpcSpawnCandidate(19, true, true, true, true, false, false)
};
int selectedOccupant = TownNpcSpawnSelectorQuery.Select([17, 18], townNpcCandidates);
if (selectedOccupant != 17)
{
  throw new InvalidOperationException("Town NPC occupant priority contract diverged.");
}

townNpcCandidates[0] = townNpcCandidates[0] with { AlreadyPresent = true };
int selectedRoom = TownNpcSpawnSelectorQuery.Select([17], townNpcCandidates);
if (selectedRoom != 18)
{
  throw new InvalidOperationException("Town NPC room fallback contract diverged.");
}

Console.WriteLine("CHECK: town NPC spawn selector contract remains deterministic");

UndergroundTreeHeightResult undergroundTreeHeight = UndergroundTreeHeightPolicy.Next(
  new GenerationRandomState(1456),
  treeHeightAddon: 2);
if (undergroundTreeHeight.Height < 7 || undergroundTreeHeight.Height > 16)
{
  throw new InvalidOperationException("Underground tree height was not range bounded.");
}

WorldGenerationStateComponent undergroundTreeTrunkState = new(49);
List<TileChangeCommand> undergroundTreeTrunkCommands = new();
WorldGrid undergroundTreeTrunkWorld = new(replayWidth, replayHeight);
_ = undergroundTreeTrunkWorld.TrySetTile(190, 180, new WorldTile(true, 60));
_ = undergroundTreeTrunkWorld.TrySetTile(189, 180, new WorldTile(true, 60));
if (!new UndergroundTreeTrunkCommandSystem().TryAppendCommands(
      undergroundTreeTrunkWorld.CreateSnapshot(replayRequest.Metadata),
      originX: 190,
      groundY: 180,
      height: 5,
      new TileProtectionComponent(190, 180, 0, 0),
      ref undergroundTreeTrunkState,
      undergroundTreeTrunkCommands) ||
    undergroundTreeTrunkCommands.Count != 5 ||
    undergroundTreeTrunkCommands[0].TileType != 5 ||
    undergroundTreeTrunkWorld.GetTile(190, 179).IsActive)
{
  throw new InvalidOperationException("Underground tree trunk commands were not command-only.");
}

Console.WriteLine("PASS: underground tree height and trunk commands remain bounded");

TreeLeafPassStyleResult gemLeafPassStyle = TreeLeafPassStyleQuery.Evaluate(
  x: 14,
  new WorldTile(true, 583, FrameX: 22, FrameY: 242),
  new WorldTile(true, 2),
  treeHeight: 7,
  hollowTreeFoliageStyle: 20);
if (gemLeafPassStyle.TreeFrame != 2 || gemLeafPassStyle.PassStyle != 1249 ||
    gemLeafPassStyle.TreeHeight != 7)
{
  throw new InvalidOperationException("Gem tree leaf pass style diverged from legacy.");
}

TreeLeafPassStyleResult hollowLeafPassStyle = TreeLeafPassStyleQuery.Evaluate(
  x: 1,
  new WorldTile(true, 5, FrameX: 22, FrameY: 220),
  new WorldTile(true, 109),
  treeHeight: 7,
  hollowTreeFoliageStyle: 20);
if (hollowLeafPassStyle.TreeFrame != 4 || hollowLeafPassStyle.PassStyle != 1115 ||
    hollowLeafPassStyle.TreeHeight != 12)
{
  throw new InvalidOperationException("Hollow tree leaf pass style diverged from legacy.");
}

Console.WriteLine("PASS: tree leaf pass styles preserve bounded legacy mapping");

WorldGrid treeLeafScanWorld = new(replayWidth, replayHeight);
HashSet<ushort> leafScanTypes = [5];
_ = treeLeafScanWorld.TrySetTile(200, 199, new WorldTile(true, 5, FrameX: 22, FrameY: 220));
_ = treeLeafScanWorld.TrySetTile(200, 201, new WorldTile(true, 2));
TreeLeafScanResult treeLeafScan = TreeLeafScanQuery.Scan(
  treeLeafScanWorld.CreateSnapshot(replayRequest.Metadata),
  200,
  200,
  leafScanTypes,
  hollowTreeFoliageStyle: 20);
if (!treeLeafScan.FoundTopTile || treeLeafScan.TreeHeight != 2 ||
    treeLeafScan.PassStyle != 910 || treeLeafScan.TreeFrame != 1)
{
  throw new InvalidOperationException("Tree leaf snapshot scan diverged from legacy traversal.");
}

Console.WriteLine("PASS: tree leaf snapshot scan preserves bounded legacy traversal");

Dictionary<ushort, TileWiringClassificationDefinition> wiringDefinitions = new()
{
  [500] = new TileWiringClassificationDefinition(
    500,
    IsMechanism: true,
    IgnoreWhenValidatingTraps: false,
    IsTrigger: false),
  [501] = new TileWiringClassificationDefinition(
    501,
    IsMechanism: true,
    IgnoreWhenValidatingTraps: true,
    IsTrigger: true)
};
WorldTile actuatorTrapTile = new(
  IsActive: true,
  Type: 500,
  IsActuated: true);
WorldTile ignoredMechanismTile = new(IsActive: true, Type: 501);
if (!TileWiringQuery.IsItATrap(actuatorTrapTile, wiringDefinitions) ||
    TileWiringQuery.IsItATrap(ignoredMechanismTile, wiringDefinitions) ||
    !TileWiringQuery.IsItATrigger(ignoredMechanismTile, wiringDefinitions) ||
    !TileWiringQuery.IsItATrigger(
      new WorldTile(IsActive: true, Type: 467, FrameX: 144),
      wiringDefinitions) ||
    !TileWiringQuery.IsItATrigger(
      new WorldTile(IsActive: true, Type: 314),
      wiringDefinitions,
      isPressurePlate: true))
{
  throw new InvalidOperationException(
    "Explicit tile wiring classifications diverged from bounded legacy predicates.");
}

Console.WriteLine("PASS: tile trap and trigger predicates preserve bounded legacy classifications");

if (!DungeonPlatformQuery.IsPlatformOrShelf(
      new WorldTile(IsActive: true, Type: 19, FrameY: 18 * 6)) ||
    !DungeonPlatformQuery.IsPlatformOrShelf(
      new WorldTile(IsActive: true, Type: 19, FrameY: 18 * 12)) ||
    DungeonPlatformQuery.IsPlatformOrShelf(
      new WorldTile(IsActive: true, Type: 19, FrameY: 18 * 13)) ||
    DungeonPlatformQuery.IsPlatformOrShelf(
      new WorldTile(IsActive: true, Type: 20, FrameY: 18 * 6)) ||
    DungeonPlatformQuery.IsPlatformOrShelf(default))
{
  throw new InvalidOperationException(
    "Dungeon platform and shelf classification diverged from legacy frame rules.");
}

Console.WriteLine("PASS: dungeon platform and shelf query preserves bounded frame rules");

AtmosphericSurfaceProfile normalAtmosphericProfile = new(
  replayHeight,
  replaySurfaceY,
  replayRequest.RockLayerY,
  IsRemixWorld: false);
AtmosphericSurfaceProfile remixAtmosphericProfile = normalAtmosphericProfile with
{
  WorldHeight = 1200,
  IsRemixWorld = true
};
if (!AtmosphericSurfaceQuery.IsSurfaceForAtmospherics(
      y: replaySurfaceY,
      profile: normalAtmosphericProfile) ||
    AtmosphericSurfaceQuery.IsSurfaceForAtmospherics(
      y: replaySurfaceY + 1,
      profile: normalAtmosphericProfile) ||
    !AtmosphericSurfaceQuery.IsSurfaceForAtmospherics(
      y: replayRequest.RockLayerY + 1,
      profile: remixAtmosphericProfile) ||
    AtmosphericSurfaceQuery.IsSurfaceForAtmospherics(
      y: replayRequest.RockLayerY,
      profile: remixAtmosphericProfile) ||
    AtmosphericSurfaceQuery.IsSurfaceForAtmospherics(
      y: replayHeight - 350,
      profile: remixAtmosphericProfile))
{
  throw new InvalidOperationException(
    "Atmospheric surface query diverged from bounded legacy remix rules.");
}

Console.WriteLine("PASS: atmospheric surface query preserves bounded legacy surface rules");

WorldGrid pressurePlateWorld = new(replayWidth, replayHeight);
_ = pressurePlateWorld.TrySetTile(25, 26, new WorldTile(IsActive: true, Type: 1));
PressurePlatePlacementDefinition pressurePlateDefinition = new(
  ForbiddenWallType: 350,
  IsBoulder: false);
if (!PressurePlatePlacementQuery.CanGenerateAt(
      pressurePlateWorld.CreateSnapshot(replayRequest.Metadata),
      tileDefinitions,
      x: 25,
      y: 25,
      pressurePlateDefinition))
{
  throw new InvalidOperationException(
    "Pressure plate placement did not accept a supported legacy tile configuration.");
}

_ = pressurePlateWorld.TrySetTile(25, 26, new WorldTile(IsActive: true, Type: 1, WallType: 350));
if (PressurePlatePlacementQuery.CanGenerateAt(
      pressurePlateWorld.CreateSnapshot(replayRequest.Metadata),
      tileDefinitions,
      x: 25,
      y: 25,
      pressurePlateDefinition))
{
  throw new InvalidOperationException("Pressure plate placement did not reject the forbidden wall.");
}

Console.WriteLine("PASS: pressure plate placement preserves bounded legacy support rules");

if (StatueStyleItemQuery.ToItem(0) != 360 ||
    StatueStyleItemQuery.ToItem(1) != 52 ||
    StatueStyleItemQuery.ToItem(43) != 1152 ||
    StatueStyleItemQuery.ToItem(51) != 3651 ||
    StatueStyleItemQuery.ToItem(63) != 3708 ||
    StatueStyleItemQuery.ToItem(76) != 4397 ||
    StatueStyleItemQuery.ToItem(82) != 5319 ||
    StatueStyleItemQuery.ToItem(10) != 446)
{
  throw new InvalidOperationException("Statue style item mapping diverged from legacy switch rules.");
}

Console.WriteLine("PASS: statue style item mapping preserves bounded legacy switch rules");

if (!PlatformSupportQuery.IsBelowANonHammeredPlatform(
      new WorldTile(IsActive: true, Type: 19, IsHalfBrick: false, Slope: 0),
      platformTypes: new ushort[] { 19 }) ||
    PlatformSupportQuery.IsBelowANonHammeredPlatform(
      new WorldTile(IsActive: true, Type: 19, IsHalfBrick: true, Slope: 0),
      platformTypes: new ushort[] { 19 }) ||
    PlatformSupportQuery.IsBelowANonHammeredPlatform(
      new WorldTile(IsActive: true, Type: 19, IsHalfBrick: false, Slope: 1),
      platformTypes: new ushort[] { 19 }) ||
    PlatformSupportQuery.IsBelowANonHammeredPlatform(
      new WorldTile(IsActive: true, Type: 20),
      platformTypes: new ushort[] { 19 }))
{
  throw new InvalidOperationException(
    "Non-hammered platform query diverged from legacy active/platform/shape rules.");
}

Console.WriteLine("PASS: non-hammered platform query preserves bounded legacy shape rules");

if (CandleItemDropQuery.ToItem(-1) != 105 ||
    CandleItemDropQuery.ToItem(0) != 105 ||
    CandleItemDropQuery.ToItem(1) != 1405 ||
    CandleItemDropQuery.ToItem(4) != 2045 ||
    CandleItemDropQuery.ToItem(13) != 2054 ||
    CandleItemDropQuery.ToItem(14) != 2153 ||
    CandleItemDropQuery.ToItem(16) != 2155 ||
    CandleItemDropQuery.ToItem(17) != 2236 ||
    CandleItemDropQuery.ToItem(30) != 3890 ||
    CandleItemDropQuery.ToItem(43) != 5606 ||
    CandleItemDropQuery.ToItem(63) != 6115 ||
    CandleItemDropQuery.ToItem(64) != 105)
{
  throw new InvalidOperationException(
    "Candle item drop mapping diverged from legacy style rules.");
}

Console.WriteLine("PASS: candle item drop mapping preserves bounded legacy switch rules");

if (PicnicTableItemDropQuery.ToItem(-1) != 4064 ||
    PicnicTableItemDropQuery.ToItem(0) != 4064 ||
    PicnicTableItemDropQuery.ToItem(1) != 4065 ||
    PicnicTableItemDropQuery.ToItem(2) != 4064)
{
  throw new InvalidOperationException(
    "Picnic table item drop mapping diverged from legacy style rules.");
}

Console.WriteLine("PASS: picnic table item drop mapping preserves bounded legacy style rules");

if (BottleItemDropQuery.ToItem(-1) != 31 ||
    BottleItemDropQuery.ToItem(0) != 31 ||
    BottleItemDropQuery.ToItem(1) != 28 ||
    BottleItemDropQuery.ToItem(2) != 110 ||
    BottleItemDropQuery.ToItem(3) != 350 ||
    BottleItemDropQuery.ToItem(4) != 351 ||
    BottleItemDropQuery.ToItem(5) != 2234 ||
    BottleItemDropQuery.ToItem(6) != 2244 ||
    BottleItemDropQuery.ToItem(7) != 2257 ||
    BottleItemDropQuery.ToItem(8) != 2258 ||
    BottleItemDropQuery.ToItem(9) != 31)
{
  throw new InvalidOperationException(
    "Bottle item drop mapping diverged from legacy style rules.");
}

Console.WriteLine("PASS: bottle item drop mapping preserves bounded legacy switch rules");

int[] benchItems =
{
  335, 2397, 2398, 2399, 2400, 2401, 2402, 2403, 2404, 2405,
  2406, 2407, 2408, 2409, 2410, 2411, 2412, 2413, 2414, 2415,
  2416, 2521, 2527, 2539, 858, 2582, 2634, 2635, 2636, 2823,
  3150, 3152, 3151, 3918, 3919, 3947, 3973, 4161, 4182, 4203,
  4224, 4313, 4582, 4993, 5164, 5185, 5206, 5564, 5617, 5705,
  5728, 5753, 5772, 5793, 5814, 5835, 5854, 5874, 5893, 5914,
  5948, 5970, 5991, 6014, 6037, 6060, 6083, 6105, 6127
};
if (BenchItemDropQuery.ToItem(-1) != 335 || BenchItemDropQuery.ToItem(0) != 335)
{
  throw new InvalidOperationException("Bench default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 68; style++)
{
  if (BenchItemDropQuery.ToItem(style) != benchItems[style])
  {
    throw new InvalidOperationException(
      $"Bench item mapping diverged for style {style}.");
  }
}

if (BenchItemDropQuery.ToItem(69) != 335)
{
  throw new InvalidOperationException("Bench out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: bench item drop mapping preserves bounded legacy switch rules");

int[] clockStyleItems =
{
  2809, 3126, 3128, 3127, 3898, 3899, 3900, 3901, 3902, 3940,
  3966, 4154, 4175, 4196, 4217, 4306, 4575, 5157, 5178, 5199,
  5557, 5610, 5698, 5721, 5746, 5764, 5785, 5806, 5827, 5847,
  5866, 5887, 5906, 5940, 5963, 5983, 6006, 6029, 6052, 6075,
  6097, 6119
};
if (ClockItemDropQuery.ToItem(-1) != 359 || ClockItemDropQuery.ToItem(0) != 359)
{
  throw new InvalidOperationException("Clock default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 5; style++)
{
  if (ClockItemDropQuery.ToItem(style) != 2237 + style - 1)
  {
    throw new InvalidOperationException($"Clock first range diverged for style {style}.");
  }
}

if (ClockItemDropQuery.ToItem(6) != 2560 || ClockItemDropQuery.ToItem(7) != 2575)
{
  throw new InvalidOperationException("Clock special style mappings diverged from legacy rules.");
}

for (int style = 8; style <= 23; style++)
{
  if (ClockItemDropQuery.ToItem(style) != 2591 + style - 8)
  {
    throw new InvalidOperationException($"Clock second range diverged for style {style}.");
  }
}

for (int style = 24; style <= 65; style++)
{
  if (ClockItemDropQuery.ToItem(style) != clockStyleItems[style - 24])
  {
    throw new InvalidOperationException($"Clock item mapping diverged for style {style}.");
  }
}

if (ClockItemDropQuery.ToItem(66) != 359)
{
  throw new InvalidOperationException("Clock out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: clock item drop mapping preserves bounded legacy switch rules");

int[] bedStyleItems =
{
  2139, 2140, 2231, 2520, 2538, 2553, 2568, 2669, 2811, 3162,
  3164, 3163, 3897, 3932, 3959, 4146, 4167, 4188, 4209, 4299,
  4567, 5149, 5170, 5191, 5549, 5602, 5690, 5713, 5740, 5757,
  5778, 5799, 5820, 5841, 5859, 5880, 5899, 5933, 5956, 5976,
  5999, 6022, 6045, 6068, 6091, 6112
};
if (BedItemDropQuery.ToItem(-1) != 224 || BedItemDropQuery.ToItem(0) != 224)
{
  throw new InvalidOperationException("Bed default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 3; style++)
{
  if (BedItemDropQuery.ToItem(style) != style + 643)
  {
    throw new InvalidOperationException($"Bed first range diverged for style {style}.");
  }
}

if (BedItemDropQuery.ToItem(4) != 920)
{
  throw new InvalidOperationException("Bed special style mapping diverged from legacy rules.");
}

for (int style = 5; style <= 8; style++)
{
  if (BedItemDropQuery.ToItem(style) != 1465 + style)
  {
    throw new InvalidOperationException($"Bed second range diverged for style {style}.");
  }
}

for (int style = 9; style <= 12; style++)
{
  if (BedItemDropQuery.ToItem(style) != 1710 + style)
  {
    throw new InvalidOperationException($"Bed third range diverged for style {style}.");
  }
}

for (int style = 13; style <= 18; style++)
{
  if (BedItemDropQuery.ToItem(style) != 2066 + style - 13)
  {
    throw new InvalidOperationException($"Bed fourth range diverged for style {style}.");
  }
}

for (int style = 19; style <= 64; style++)
{
  if (BedItemDropQuery.ToItem(style) != bedStyleItems[style - 19])
  {
    throw new InvalidOperationException($"Bed item mapping diverged for style {style}.");
  }
}

if (BedItemDropQuery.ToItem(65) != 224)
{
  throw new InvalidOperationException("Bed out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: bed item drop mapping preserves bounded legacy switch rules");

int[] candelabraStyleItems =
{
  2227, 2522, 2541, 2555, 2570, 2664, 2665, 2666, 2667, 2668,
  2825, 3168, 3170, 3169, 3893, 3935, 3961, 4149, 4170, 4191,
  4212, 4302, 4570, 5152, 5173, 5194, 5552, 5605, 5693, 5716,
  5742, 5759, 5780, 5801, 5822, 5843, 5861, 5882, 5901, 5935,
  5958, 5978, 6001, 6024, 6047, 6070, 6093, 6114
};
if (CandelabraItemDropQuery.ToItem(-1) != 349 ||
    CandelabraItemDropQuery.ToItem(0) != 349)
{
  throw new InvalidOperationException(
    "Candelabra default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 12; style++)
{
  if (CandelabraItemDropQuery.ToItem(style) != 2092 + style - 1)
  {
    throw new InvalidOperationException($"Candelabra first range diverged for style {style}.");
  }
}

for (int style = 13; style <= 16; style++)
{
  if (CandelabraItemDropQuery.ToItem(style) != 2149 + style - 13)
  {
    throw new InvalidOperationException($"Candelabra second range diverged for style {style}.");
  }
}

for (int style = 17; style <= 64; style++)
{
  if (CandelabraItemDropQuery.ToItem(style) != candelabraStyleItems[style - 17])
  {
    throw new InvalidOperationException($"Candelabra item mapping diverged for style {style}.");
  }
}

if (CandelabraItemDropQuery.ToItem(65) != 349)
{
  throw new InvalidOperationException(
    "Candelabra out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: candelabra item drop mapping preserves bounded legacy switch rules");

int[] bookcaseInitialItems =
{
  1414, 1415, 1416, 1463, 1512, 2020, 2021, 2022, 2023,
  2024, 2025, 2026, 2027, 2028, 2029, 2030, 2031
};
int[] bookcaseStyleItems =
{
  2233, 2536, 2540, 2554, 2569, 2670, 2817, 3165, 3167, 3166,
  3917, 3933, 3960, 4147, 4168, 4189, 4210, 4300, 4568, 5150,
  5171, 5192, 5550, 5603, 5691, 5714, 5758, 5779, 5800, 5821,
  5842, 5860, 5881, 5900, 5934, 5957, 5977, 6000, 6023, 6046,
  6069, 6092, 6113
};
if (BookcaseItemDropQuery.ToItem(-1) != 354 || BookcaseItemDropQuery.ToItem(0) != 354)
{
  throw new InvalidOperationException("Bookcase default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 17; style++)
{
  if (BookcaseItemDropQuery.ToItem(style) != bookcaseInitialItems[style - 1])
  {
    throw new InvalidOperationException($"Bookcase initial mapping diverged for style {style}.");
  }
}

for (int style = 18; style <= 21; style++)
{
  if (BookcaseItemDropQuery.ToItem(style) != 2135 + style - 18)
  {
    throw new InvalidOperationException($"Bookcase range diverged for style {style}.");
  }
}

for (int style = 22; style <= 64; style++)
{
  if (BookcaseItemDropQuery.ToItem(style) != bookcaseStyleItems[style - 22])
  {
    throw new InvalidOperationException($"Bookcase item mapping diverged for style {style}.");
  }
}

if (BookcaseItemDropQuery.ToItem(65) != 354)
{
  throw new InvalidOperationException("Bookcase out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: bookcase item drop mapping preserves bounded legacy switch rules");

int[] chandelierInitialItems = { 107, 108, 710, 711, 712, 1812 };
int[] chandelierStyleItems =
{
  2224, 2525, 2543, 2558, 2573, 2652, 2653, 2654, 2655, 2656,
  2657, 2813, 3177, 3179, 3178, 3894, 3938, 3964, 4152, 4173,
  4194, 4215, 4305, 4573, 5155, 5176, 5197, 5555, 5608, 5696,
  5719, 5744, 5762, 5783, 5804, 5825, 5845, 5864, 5885, 5904,
  5938, 5961, 5981, 6004, 6027, 6050, 6073, 6096, 6117
};
if (ChandelierItemDropQuery.ToItem(-1) != 106 ||
    ChandelierItemDropQuery.ToItem(0) != 106)
{
  throw new InvalidOperationException(
    "Chandelier default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 6; style++)
{
  if (ChandelierItemDropQuery.ToItem(style) != chandelierInitialItems[style - 1])
  {
    throw new InvalidOperationException($"Chandelier initial mapping diverged for style {style}.");
  }
}

for (int style = 7; style <= 17; style++)
{
  if (ChandelierItemDropQuery.ToItem(style) != 2055 + style - 7)
  {
    throw new InvalidOperationException($"Chandelier first range diverged for style {style}.");
  }
}

for (int style = 18; style <= 21; style++)
{
  if (ChandelierItemDropQuery.ToItem(style) != 2141 + style - 18)
  {
    throw new InvalidOperationException($"Chandelier second range diverged for style {style}.");
  }
}

for (int style = 22; style <= 70; style++)
{
  if (ChandelierItemDropQuery.ToItem(style) != chandelierStyleItems[style - 22])
  {
    throw new InvalidOperationException($"Chandelier item mapping diverged for style {style}.");
  }
}

if (ChandelierItemDropQuery.ToItem(71) != 106)
{
  throw new InvalidOperationException(
    "Chandelier out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: chandelier item drop mapping preserves bounded legacy switch rules");

int[] lanternStyleItems =
{
  2226, 2530, 2546, 2564, 2579, 2641, 2642, 2820, 3138, 3140,
  3139, 3891, 3943, 3970, 4157, 4178, 4199, 4220, 4309, 4578,
  5160, 5181, 5202, 5560, 5613, 5701, 5724, 5749, 5768, 5789,
  5810, 5831, 5850, 5870, 5890, 5910, 5944, 5967, 5987, 6010,
  6033, 6056, 6079, 6101, 6123
};
if (LanternItemDropQuery.ToItem(-1) != 1388 || LanternItemDropQuery.ToItem(0) != 136)
{
  throw new InvalidOperationException("Lantern low-style mapping diverged from legacy rules.");
}

for (int style = 1; style <= 6; style++)
{
  if (LanternItemDropQuery.ToItem(style) != 1389 + style)
  {
    throw new InvalidOperationException($"Lantern first range diverged for style {style}.");
  }
}

if (LanternItemDropQuery.ToItem(7) != 1431 || LanternItemDropQuery.ToItem(8) != 1808 ||
    LanternItemDropQuery.ToItem(9) != 1859)
{
  throw new InvalidOperationException("Lantern special style mappings diverged from legacy rules.");
}

for (int style = 10; style <= 21; style++)
{
  if (LanternItemDropQuery.ToItem(style) != 2032 + style - 10)
  {
    throw new InvalidOperationException($"Lantern second range diverged for style {style}.");
  }
}

for (int style = 22; style <= 25; style++)
{
  if (LanternItemDropQuery.ToItem(style) != 2145 + style - 22)
  {
    throw new InvalidOperationException($"Lantern third range diverged for style {style}.");
  }
}

for (int style = 26; style <= 70; style++)
{
  if (LanternItemDropQuery.ToItem(style) != lanternStyleItems[style - 26])
  {
    throw new InvalidOperationException($"Lantern item mapping diverged for style {style}.");
  }
}

if (LanternItemDropQuery.ToItem(71) != 136)
{
  throw new InvalidOperationException("Lantern out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: lantern item drop mapping preserves bounded legacy switch rules");

int[] lampStyleItems =
{
  2225, 2533, 2547, 2563, 2578, 2643, 2644, 2645, 2646, 2647,
  2819, 3135, 3137, 3136, 3892, 3942, 3969, 4156, 4177, 4198,
  4219, 4308, 4577, 5159, 5180, 5201, 5559, 5612, 5700, 5723,
  5748, 5767, 5788, 5809, 5830, 5849, 5869, 5889, 5909, 5943,
  5966, 5986, 6009, 6032, 6055, 6078, 6100, 6122
};
if (LampItemDropQuery.ToItem(-1) != 342 || LampItemDropQuery.ToItem(0) != 342)
{
  throw new InvalidOperationException("Lamp default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 10; style++)
{
  if (LampItemDropQuery.ToItem(style) != 2082 + style - 1)
  {
    throw new InvalidOperationException($"Lamp first range diverged for style {style}.");
  }
}

for (int style = 11; style <= 16; style++)
{
  if (LampItemDropQuery.ToItem(style) != 2129 + style - 11)
  {
    throw new InvalidOperationException($"Lamp second range diverged for style {style}.");
  }
}

for (int style = 17; style <= 64; style++)
{
  if (LampItemDropQuery.ToItem(style) != lampStyleItems[style - 17])
  {
    throw new InvalidOperationException($"Lamp item mapping diverged for style {style}.");
  }
}

if (LampItemDropQuery.ToItem(65) != 342)
{
  throw new InvalidOperationException("Lamp out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: lamp item drop mapping preserves bounded legacy switch rules");

int[] pianoStyleItems =
{
  2531, 2548, 2565, 2580, 2671, 2821, 3141, 3143, 3142, 3915,
  3916, 3944, 3971, 4158, 4179, 4200, 4221, 4310, 4579, 5161,
  5182, 5203, 5561, 5614, 5702, 5725, 5750, 5769, 5790, 5811,
  5832, 5851, 5871, 5891, 5911, 5945, 5968, 5988, 6011, 6034,
  6057, 6080, 6102, 6124
};
if (PianoItemDropQuery.ToItem(-1) != 333 || PianoItemDropQuery.ToItem(0) != 333)
{
  throw new InvalidOperationException("Piano default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 3; style++)
{
  if (PianoItemDropQuery.ToItem(style) != 640 + style)
  {
    throw new InvalidOperationException($"Piano first range diverged for style {style}.");
  }
}

if (PianoItemDropQuery.ToItem(4) != 919)
{
  throw new InvalidOperationException("Piano special style mapping diverged from legacy rules.");
}

for (int style = 5; style <= 7; style++)
{
  if (PianoItemDropQuery.ToItem(style) != 2245 + style - 5)
  {
    throw new InvalidOperationException($"Piano second range diverged for style {style}.");
  }
}

for (int style = 8; style <= 10; style++)
{
  if (PianoItemDropQuery.ToItem(style) != 2254 + style - 8)
  {
    throw new InvalidOperationException($"Piano third range diverged for style {style}.");
  }
}

for (int style = 11; style <= 20; style++)
{
  if (PianoItemDropQuery.ToItem(style) != 2376 + style - 11)
  {
    throw new InvalidOperationException($"Piano fourth range diverged for style {style}.");
  }
}

for (int style = 21; style <= 64; style++)
{
  if (PianoItemDropQuery.ToItem(style) != pianoStyleItems[style - 21])
  {
    throw new InvalidOperationException($"Piano item mapping diverged for style {style}.");
  }
}

if (PianoItemDropQuery.ToItem(65) != 333)
{
  throw new InvalidOperationException("Piano out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: piano item drop mapping preserves bounded legacy switch rules");

int[] sinkStyleItems =
{
  3147, 3149, 3148, 3896, 3946, 3972, 4160, 4181, 4202, 4223,
  4312, 4581, 5163, 5184, 5205, 5563, 5616, 5704, 5727, 5752,
  5771, 5792, 5813, 5834, 5853, 5873, 5892, 5913, 5947, 5969,
  5990, 6013, 6036, 6059, 6082, 6104, 6126
};
if (SinkItemDropQuery.ToItem(-1) != 2827)
{
  throw new InvalidOperationException("Sink negative-style mapping diverged from legacy rules.");
}

for (int style = 0; style <= 28; style++)
{
  if (SinkItemDropQuery.ToItem(style) != 2827 + style)
  {
    throw new InvalidOperationException($"Sink range diverged for style {style}.");
  }
}

for (int style = 29; style <= 65; style++)
{
  if (SinkItemDropQuery.ToItem(style) != sinkStyleItems[style - 29])
  {
    throw new InvalidOperationException($"Sink item mapping diverged for style {style}.");
  }
}

if (SinkItemDropQuery.ToItem(66) != 2827)
{
  throw new InvalidOperationException("Sink out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: sink item drop mapping preserves bounded legacy switch rules");

int[] secondTableItems =
{
  3920, 3948, 3974, 4162, 4183, 4204, 4225, 4314, 4583, 5165,
  5186, 5207, 5565, 5618, 5706, 5729, 5773, 5794, 5815, 5836,
  5875, 5894, 5915, 5949, 5971, 5992, 6015, 6038, 6061, 6084,
  6106, 6128
};
if (TableItemDropQuery.ToItem(-1, false) != 32 ||
    TableItemDropQuery.ToItem(0, true) != 3920)
{
  throw new InvalidOperationException("Table default mappings diverged from legacy rules.");
}

for (int style = 1; style <= 31; style++)
{
  if (TableItemDropQuery.ToItem(style, true) != secondTableItems[style])
  {
    throw new InvalidOperationException($"Second table mapping diverged for style {style}.");
  }
}

for (int style = 1; style <= 3; style++)
{
  if (TableItemDropQuery.ToItem(style, false) != 637 + style)
  {
    throw new InvalidOperationException($"Table first range diverged for style {style}.");
  }
}

for (int style = 4; style <= 7; style++)
{
  if (TableItemDropQuery.ToItem(style, false) != 823 + style)
  {
    throw new InvalidOperationException($"Table second range diverged for style {style}.");
  }
}

for (int style = 15; style <= 20; style++)
{
  if (TableItemDropQuery.ToItem(style, false) != 1698 + style)
  {
    throw new InvalidOperationException($"Table third range diverged for style {style}.");
  }
}

foreach ((int style, int expected) in new[]
{
  (8, 917), (9, 1144), (10, 1397), (11, 1400), (12, 1403), (13, 1460),
  (14, 1510), (21, 1794), (22, 1816), (23, 1926), (24, 2248), (25, 2259),
  (26, 2532), (27, 2550), (28, 677), (29, 2583), (30, 2743), (31, 2824),
  (32, 3153), (33, 3155), (34, 3154)
})
{
  if (TableItemDropQuery.ToItem(style, false) != expected)
  {
    throw new InvalidOperationException($"Table item mapping diverged for style {style}.");
  }
}

if (TableItemDropQuery.ToItem(35, false) != 32 ||
    TableItemDropQuery.ToItem(32, true) != 3920)
{
  throw new InvalidOperationException("Table out-of-range mappings diverged from legacy rules.");
}

Console.WriteLine("PASS: table item drop mapping preserves bounded legacy switch rules");

int[] bathtubItems =
{
  336, 2072, 2073, 2074, 2075, 2076, 2077, 2078, 2079, 2080,
  2081, 2124, 2125, 2126, 2127, 2128, 2232, 2519, 2537, 2552,
  2567, 2658, 2659, 2660, 2661, 2662, 2663, 2810, 3159, 3161,
  3160, 3895, 3931, 3958, 4145, 4166, 4187, 4208, 4298, 4566,
  5148, 5169, 5190, 5548, 5601, 5689, 5712, 5739, 5756, 5777,
  5798, 5819, 5840, 5858, 5879, 5898, 5932, 5955, 5975, 5998,
  6021, 6044, 6067, 6090, 6111
};
if (BathtubItemDropQuery.ToItem(-1) != 336 ||
    BathtubItemDropQuery.ToItem(65) != 336)
{
  throw new InvalidOperationException("Bathtub default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 64; style++)
{
  if (BathtubItemDropQuery.ToItem(style) != bathtubItems[style])
  {
    throw new InvalidOperationException($"Bathtub mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: bathtub item drop mapping preserves bounded legacy switch rules");

int[] workbenchItems =
{
  36, 635, 636, 637, 811, 812, 813, 814, 815, 916,
  1145, 1398, 1401, 1404, 1461, 1511, 1795, 1817, 2229, 2251,
  2252, 2253, 2534, 673, 2631, 2632, 2633, 2826, 3156, 3158,
  3157, 3909, 3910, 3949, 3975, 4163, 4184, 4205, 4226, 4315,
  4584, 5166, 5187, 5208, 5566, 5619, 5707, 5730, 5775, 5796,
  5817, 5838, 5856, 5877, 5896, 5917, 5951, 5973, 5994, 6017,
  6040, 6063, 6086, 6108, 6130
};
if (WorkbenchItemDropQuery.ToItem(-1) != 36 ||
    WorkbenchItemDropQuery.ToItem(65) != 36)
{
  throw new InvalidOperationException("Workbench default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 64; style++)
{
  if (WorkbenchItemDropQuery.ToItem(style) != workbenchItems[style])
  {
    throw new InvalidOperationException($"Workbench mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: workbench item drop mapping preserves bounded legacy switch rules");

int[] chairItems =
{
  34, 358, 628, 629, 630, 806, 807, 808, 809, 810,
  826, 915, 1143, 1396, 1399, 1402, 1459, 1509, 1703, 1704,
  1705, 1706, 1707, 1708, 1792, 1814, 1925, 2228, 2288, 2524,
  2557, 2572, 2812, 3174, 3176, 3175, 3889, 3937, 3963, 4151,
  4172, 4193, 4214, 4304, 4572, 5154, 5175, 5196, 5554, 5607,
  5695, 5718, 5761, 5782, 5803, 5824, 5863, 5884, 5903, 5937,
  5960, 5980, 6003, 6026, 6049, 6072, 6095, 6116
};
if (ChairItemDropQuery.ToItem(-1) != 34 ||
    ChairItemDropQuery.ToItem(68) != 34)
{
  throw new InvalidOperationException("Chair default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 67; style++)
{
  if (ChairItemDropQuery.ToItem(style) != chairItems[style])
  {
    throw new InvalidOperationException($"Chair mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: chair item drop mapping preserves bounded legacy switch rules");

if (ToiletItemDropQuery.ToItem(-1) != 4096 ||
    ToiletItemDropQuery.ToItem(65) != 4096)
{
  throw new InvalidOperationException("Toilet default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 31; style++)
{
  if (ToiletItemDropQuery.ToItem(style) != 4096 + style)
  {
    throw new InvalidOperationException($"Toilet range mapping diverged for style {style}.");
  }
}

foreach ((int style, int expected) in new[]
{
  (32, 4141), (33, 4165), (34, 4186), (35, 4207), (36, 4228), (37, 4316),
  (38, 4586), (39, 4731), (40, 5168), (41, 5189), (42, 5210), (43, 5568),
  (44, 5621), (45, 5709), (46, 5732), (47, 5755), (48, 5774), (49, 5795),
  (50, 5816), (51, 5837), (52, 5855), (53, 5876), (54, 5895), (55, 5916),
  (56, 5950), (57, 5972), (58, 5993), (59, 6016), (60, 6039), (61, 6062),
  (62, 6085), (63, 6107), (64, 6129)
})
{
  if (ToiletItemDropQuery.ToItem(style) != expected)
  {
    throw new InvalidOperationException($"Toilet mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: toilet item drop mapping preserves bounded legacy switch rules");

int[] platformItems =
{
  94, 631, 632, 633, 634, 913, 1384, 1385, 1386, 1387,
  1388, 1389, 1418, 1457, 1702, 1796, 1818, 2518, 2549, 2566,
  2581, 2627, 2628, 2629, 2630, 2744, 2822, 3144, 3146, 3145,
  3903, 3904, 3905, 3906, 3907, 3908, 3945, 3957, 4159, 4180,
  4201, 4222, 4311, 4416, 4580, 5162, 5183, 5204, 5292, 5544,
  5562, 5615, 5703, 5726, 5751, 5770, 5791, 5812, 5833, 5852,
  5872, 5912, 5946, 5989, 6012, 6035, 6058, 6081, 6103, 6125
};
if (PlatformItemDropQuery.ToItem(-1) != 94 ||
    PlatformItemDropQuery.ToItem(70) != 94)
{
  throw new InvalidOperationException("Platform default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 69; style++)
{
  if (PlatformItemDropQuery.ToItem(style) != platformItems[style])
  {
    throw new InvalidOperationException($"Platform mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: platform item drop mapping preserves bounded legacy switch rules");

if (MusicBoxItemDropQuery.ToItem(-100) != 462 ||
    MusicBoxItemDropQuery.ToItem(-1) != 561 ||
    MusicBoxItemDropQuery.ToItem(101) != 576)
{
  throw new InvalidOperationException("Music box default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 12; style++)
{
  if (MusicBoxItemDropQuery.ToItem(style) != 562 + style)
  {
    throw new InvalidOperationException($"Music box first range diverged for style {style}.");
  }
}

for (int style = 13; style <= 27; style++)
{
  if (MusicBoxItemDropQuery.ToItem(style) != 1583 + style)
  {
    throw new InvalidOperationException($"Music box second range diverged for style {style}.");
  }
}

foreach ((int style, int expected) in new[]
{
  (28, 1963), (29, 1964), (30, 1965), (31, 2742), (32, 3044), (33, 3235),
  (34, 3236), (35, 3237), (36, 3370), (37, 3371), (38, 3796), (39, 3869),
  (40, 4082), (41, 4078), (42, 4079), (43, 4077), (44, 4080), (45, 4081),
  (46, 4237), (47, 4356), (48, 4357), (49, 4358), (50, 4421), (51, 4606),
  (52, 4979), (53, 4985), (54, 4990), (55, 4991), (56, 4992), (57, 5006),
  (58, 5014), (59, 5015), (60, 5016), (61, 5017), (62, 5018), (63, 5019),
  (64, 5020), (65, 5021), (66, 5022), (67, 5023), (68, 5024), (69, 5025),
  (70, 5026), (71, 5027), (72, 5028), (73, 5029), (74, 5030), (75, 5031),
  (76, 5032), (77, 5033), (78, 5034), (79, 5035), (80, 5036), (81, 5037),
  (82, 5038), (83, 5039), (84, 5040), (85, 5044), (86, 5112), (87, 5362),
  (88, 5578), (89, 5538), (90, 5579), (91, 5580), (92, 5539), (93, 5581),
  (94, 5582), (95, 5637), (96, 5638), (97, 5639), (98, 6144), (99, 6145),
  (100, 6146)
})
{
  if (MusicBoxItemDropQuery.ToItem(style) != expected)
  {
    throw new InvalidOperationException($"Music box mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: music box item drop mapping preserves bounded legacy switch rules");

int[] dresserItems =
{
  334, 647, 648, 649, 918, 2386, 2387, 2388, 2389, 2390,
  2391, 2392, 2393, 2394, 2395, 2396, 2529, 2545, 2562, 2577,
  2637, 2638, 2639, 2640, 2816, 3132, 3134, 3133, 3911, 3912,
  3913, 3914, 3934, 3968, 4148, 4169, 4190, 4211, 4301, 4569,
  5151, 5172, 5193, 5551, 5604, 5692, 5715, 5741, 5766, 5787,
  5808, 5829, 5848, 5868, 5888, 5908, 5942, 5965, 5985, 6008,
  6031, 6054, 6077, 6099, 6121
};
if (DresserItemDropQuery.ToItem(-1) != 334 ||
    DresserItemDropQuery.ToItem(65) != 334)
{
  throw new InvalidOperationException("Dresser default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 64; style++)
{
  if (DresserItemDropQuery.ToItem(style) != dresserItems[style])
  {
    throw new InvalidOperationException($"Dresser mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: dresser item drop mapping preserves bounded legacy switch rules");

int[] secondChestItems =
{
  3884, 3885, 3939, 3965, 3988, 4153, 4174, 4195, 4216, 4265,
  4267, 4574, 4712, 4712, 5156, 5177, 5198, 5556, 5609, 5697,
  5720, 5745, 5763, 5784, 5805, 5826, 5846, 5865, 5886, 5905,
  5939, 5962, 5982, 6005, 6028, 6051, 6074, 6118
};
int[] firstChestItems =
{
  48, 306, 306, 328, 328, 343, 348, 625, 626, 627,
  680, 681, 831, 838, 914, 952, 1142, 1298, 1528, 1529,
  1530, 1531, 1532, 1528, 1529, 1530, 1531, 1532, 2230, 2249,
  2250, 2526, 2544, 2559, 2574, 2612, 2612, 2613, 2613, 2614,
  2614, 2615, 2616, 2617, 2618, 2619, 2620, 2748, 2814, 3180,
  3125, 3181
};
for (int style = 0; style <= 37; style++)
{
  if (ChestItemDropQuery.ToItem(style, true) != secondChestItems[style])
  {
    throw new InvalidOperationException($"Second chest mapping diverged for style {style}.");
  }
}

for (int style = 0; style <= 51; style++)
{
  if (ChestItemDropQuery.ToItem(style, false) != firstChestItems[style])
  {
    throw new InvalidOperationException($"First chest mapping diverged for style {style}.");
  }
}

if (ChestItemDropQuery.ToItem(-1, true) != 3884 ||
    ChestItemDropQuery.ToItem(38, true) != 3884 ||
    ChestItemDropQuery.ToItem(-1, false) != 48 ||
    ChestItemDropQuery.ToItem(52, false) != 48)
{
  throw new InvalidOperationException("Chest default mappings diverged from legacy rules.");
}

Console.WriteLine("PASS: chest item drop mapping preserves dual legacy style tables");

foreach ((int style, int expected) in new[]
{
  (0, 3886), (1, 3887), (2, 3950), (3, 3976), (4, -1), (5, 4164),
  (6, 4185), (7, 4206), (8, 4227), (9, 4266), (10, 4268), (11, 4585),
  (12, 4713), (13, -1), (14, 5167), (15, 5188), (16, 5209), (17, 5567),
  (18, 5620), (19, 5708), (20, 5731), (21, 5754), (22, 5776), (23, 5797),
  (24, 5818), (25, 5839), (26, 5857), (27, 5878), (28, 5897), (29, 5918),
  (30, 5952), (31, 5974), (32, 5995), (33, 6018), (34, 6041), (35, 6064),
  (36, 6087), (37, 6131)
})
{
  if (FakeChestItemDropQuery.ToItem(style, true) != expected)
  {
    throw new InvalidOperationException($"Second fake chest mapping diverged for style {style}.");
  }
}

foreach ((int style, int expected) in new[]
{
  (0, 3665), (1, 3666), (2, 3665), (3, 3667), (4, 3665), (5, 3665),
  (6, 3665), (7, 3668), (8, 3669), (9, 3670), (10, 3671), (11, 3672),
  (12, 3673), (13, 3674), (14, 3675), (15, 3676), (16, 3677), (17, 3678),
  (18, 3679), (19, 3680), (20, 3681), (21, 3682), (22, 3683), (23, 3665),
  (24, 3665), (25, 3665), (26, 3665), (27, 3665), (28, 3684), (29, 3685),
  (30, 3686), (31, 3687), (32, 3688), (33, 3689), (34, 3690), (35, 3691),
  (36, 3665), (37, 3692), (38, 3665), (39, 3693), (40, 3665), (41, 3694),
  (42, 3695), (43, 3696), (44, 3697), (45, 3698), (46, 3699), (47, 3700),
  (48, 3701), (49, 3702), (50, 3703), (51, 3704)
})
{
  if (FakeChestItemDropQuery.ToItem(style, false) != expected)
  {
    throw new InvalidOperationException($"First fake chest mapping diverged for style {style}.");
  }
}

if (FakeChestItemDropQuery.ToItem(-1, true) != 3886 ||
    FakeChestItemDropQuery.ToItem(38, true) != 3886 ||
    FakeChestItemDropQuery.ToItem(-1, false) != 3665 ||
    FakeChestItemDropQuery.ToItem(52, false) != 3665)
{
  throw new InvalidOperationException("Fake chest default mappings diverged from legacy rules.");
}

Console.WriteLine("PASS: fake chest item drop mapping preserves dual legacy style tables");

int[] campfireItems =
{
  966, 3046, 3047, 3048, 3049, 3050, 3723, 3724,
  4689, 4690, 4691, 4692, 4693, 4694, 5299, 5357
};
for (int style = 0; style <= 15; style++)
{
  if (CampfireItemDropQuery.ToItem(style) != campfireItems[style])
  {
    throw new InvalidOperationException($"Campfire mapping diverged for style {style}.");
  }
}

if (CampfireItemDropQuery.ToItem(-1) != 966 ||
    CampfireItemDropQuery.ToItem(16) != 966)
{
  throw new InvalidOperationException("Campfire default mappings diverged from legacy rules.");
}

Console.WriteLine("PASS: campfire item drop mapping preserves bounded legacy switch rules");

if (RainbowPaintQuery.ToPaintId(0, 0, false) != 13 ||
    RainbowPaintQuery.ToPaintId(43, 43, false) != 22 ||
    RainbowPaintQuery.ToPaintId(44, 0, false) != 13 ||
    RainbowPaintQuery.ToPaintId(10, 10, false) != 22)
{
  throw new InvalidOperationException("Rainbow paint mapping diverged for direct coordinates.");
}

if (RainbowPaintQuery.ToPaintId(0, 0, true) != 8 ||
    RainbowPaintQuery.ToPaintId(25, 25, true) != 22 ||
    RainbowPaintQuery.ToPaintId(49, 0, true) != 13)
{
  throw new InvalidOperationException("Rainbow paint mapping diverged for wiggly coordinates.");
}

if (RainbowPaintQuery.ToPaintId(-1, 0, false) != 12 ||
    RainbowPaintQuery.ToPaintId(-50, -10, true) != 4)
{
  throw new InvalidOperationException("Rainbow paint mapping normalized legacy negative coordinates.");
}

Console.WriteLine("PASS: rainbow paint mapping preserves direct, wiggly, and negative coordinates");

if (!DungeonChestQuery.IsLockedBiomeChest(21, 23) ||
    !DungeonChestQuery.IsLockedBiomeChest(21, 27) ||
    !DungeonChestQuery.IsLockedBiomeChest(467, 13) ||
    DungeonChestQuery.IsLockedBiomeChest(21, 22) ||
    DungeonChestQuery.IsLockedBiomeChest(21, 28) ||
    DungeonChestQuery.IsLockedBiomeChest(467, 12) ||
    DungeonChestQuery.IsLockedBiomeChest(467, 14) ||
    DungeonChestQuery.IsLockedBiomeChest(0, 23))
{
  throw new InvalidOperationException(
    "Locked dungeon biome chest mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: locked dungeon biome chest mapping preserves bounded legacy styles");

if (PileGenerationAttemptPolicy.GetAttempts(4200, false) != 2100 ||
    PileGenerationAttemptPolicy.GetAttempts(4200, true) != 210 ||
    PileGenerationAttemptPolicy.GetAttempts(101, false) != 50 ||
    PileGenerationAttemptPolicy.GetAttempts(101, true) != 5)
{
  throw new InvalidOperationException(
    "Pile generation attempt policy diverged from legacy division rules.");
}

Console.WriteLine("PASS: pile generation attempt policy preserves explicit world-rule inputs");

if (!PlantTypeConversionQuery.IsBadTypeMatch(23, 3))
{
  throw new InvalidOperationException("Plant grass mismatch was accepted.");
}

if (PlantTypeConversionQuery.IsBadTypeMatch(2, 3) ||
    PlantTypeConversionQuery.IsBadTypeMatch(477, 73) ||
    PlantTypeConversionQuery.IsBadTypeMatch(23, 24) ||
    PlantTypeConversionQuery.IsBadTypeMatch(60, 61) ||
    PlantTypeConversionQuery.IsBadTypeMatch(70, 71) ||
    PlantTypeConversionQuery.IsBadTypeMatch(109, 110) ||
    PlantTypeConversionQuery.IsBadTypeMatch(199, 201) ||
    PlantTypeConversionQuery.IsBadTypeMatch(633, 637))
{
  throw new InvalidOperationException("Plant compatible support was rejected.");
}

if (!PlantTypeConversionQuery.IsBadTypeMatch(2, 637))
{
  throw new InvalidOperationException("Plant ash mismatch was accepted for non-ash grass.");
}

PlantTypeConversionResult grassConversion = PlantTypeConversionQuery.Evaluate(113, 144, 2);
if (grassConversion.TileType != 73 || grassConversion.FrameX != 144 ||
    grassConversion.IsMushroom)
{
  throw new InvalidOperationException("Plant grass conversion diverged from legacy rules.");
}

PlantTypeConversionResult corruptConversion = PlantTypeConversionQuery.Evaluate(3, 180, 23);
if (corruptConversion.TileType != 24 || corruptConversion.FrameX != 126 ||
    corruptConversion.IsMushroom)
{
  throw new InvalidOperationException("Plant corrupt-grass conversion diverged from legacy rules.");
}

PlantTypeConversionResult mushroomConversion = PlantTypeConversionQuery.Evaluate(3, 144, 60);
if (mushroomConversion.TileType != 61 || mushroomConversion.FrameX != 18 ||
    !mushroomConversion.IsMushroom)
{
  throw new InvalidOperationException("Plant mushroom conversion diverged from legacy rules.");
}

PlantTypeConversionResult crimsonMushroom = PlantTypeConversionQuery.Evaluate(201, 270, 199);
if (crimsonMushroom.TileType != 201 || crimsonMushroom.FrameX != 270 ||
    !crimsonMushroom.IsMushroom)
{
  throw new InvalidOperationException(
    "Plant crimson mushroom handling diverged from legacy rules.");
}

Console.WriteLine("PASS: plant type conversion preserves bounded legacy compatibility and frames");

WorldGrid plantWorld = new(replayWidth, replayHeight);
WorldMetadata plantMetadata = new(
  "worldgen-plant-check-query",
  new WorldSeed(1456),
  replayWidth,
  replayHeight);
_ = plantWorld.TrySetTile(
  40,
  40,
  new WorldTile(
    IsActive: true,
    Type: 3,
    LiquidAmount: 90,
    LiquidType: 1,
    FrameX: 144,
    FrameY: 36,
    WallType: 6,
    HasWire: true));
_ = plantWorld.TrySetTile(40, 41, new WorldTile(IsActive: true, Type: 23));
WorldGridSnapshot corruptPlantSnapshot = plantWorld.CreateSnapshot(plantMetadata);
if (!PlantPlacementQuery.CanPlace(
      corruptPlantSnapshot,
      tileDefinitions,
      40,
      40,
      24) ||
    PlantPlacementQuery.CanPlace(
      corruptPlantSnapshot,
      tileDefinitions,
      40,
      40,
      3))
{
  throw new InvalidOperationException(
    "Plant placement support classification diverged from legacy rules.");
}

PlantCheckResult corruptPlantCheck = PlantCheckQuery.Evaluate(
  corruptPlantSnapshot,
  tileDefinitions,
  40,
  40);
if (corruptPlantCheck.ShouldDestroy || !corruptPlantCheck.ShouldConvert ||
    corruptPlantCheck.Conversion.TileType != 24 ||
    corruptPlantCheck.Conversion.FrameX != 144 || !corruptPlantCheck.Conversion.IsMushroom)
{
  throw new InvalidOperationException("Plant check conversion intent diverged from legacy rules.");
}

if (!PlantCheckCommandSystem.TryCreateCommand(
      corruptPlantSnapshot,
      tileDefinitions,
      40,
      40,
      18,
      out TileChangeCommand plantConversionCommand) ||
    plantConversionCommand.Kind != TileChangeKind.UpdateTileType ||
    plantConversionCommand.TileType != 24 || plantConversionCommand.FrameX != 144 ||
    plantConversionCommand.FrameY != 36)
{
  throw new InvalidOperationException(
    "Plant check conversion command did not preserve legacy type and frame semantics.");
}

WorldGrid convertedPlantWorld = WorldGrid.FromSnapshot(corruptPlantSnapshot);
if (!new TileChangeCommitSystem().TryCommit(
      convertedPlantWorld,
      new[] { plantConversionCommand },
      out TileChangeCommitResult plantConversionCommit) ||
    plantConversionCommit.AppliedCount != 1)
{
  throw new InvalidOperationException("Plant conversion command did not commit.");
}

WorldTile convertedPlantTile = convertedPlantWorld.GetTile(40, 40);
if (convertedPlantTile.Type != 24 || convertedPlantTile.FrameX != 144 ||
    convertedPlantTile.FrameY != 36 || convertedPlantTile.LiquidAmount != 90 ||
    convertedPlantTile.LiquidType != 1 || convertedPlantTile.WallType != 6 ||
    !convertedPlantTile.HasWire)
{
  throw new InvalidOperationException(
    "Plant conversion command did not preserve unrelated tile state.");
}

_ = plantWorld.TrySetTile(40, 41, new WorldTile(IsActive: true, Type: 1));
WorldGridSnapshot unsupportedPlantSnapshot = plantWorld.CreateSnapshot(plantMetadata);
if (!PlantCheckQuery.Evaluate(unsupportedPlantSnapshot, tileDefinitions, 40, 40).ShouldDestroy)
{
  throw new InvalidOperationException("Unsupported plants were not marked for destruction.");
}

if (!PlantCheckCommandSystem.TryCreateCommand(
      unsupportedPlantSnapshot,
      tileDefinitions,
      40,
      40,
      19,
      out TileChangeCommand plantDestroyCommand) ||
    plantDestroyCommand.Kind != TileChangeKind.Kill || plantDestroyCommand.TileType != 0)
{
  throw new InvalidOperationException("Plant check destruction did not produce a kill command.");
}

_ = plantWorld.TrySetTile(40, 40, new WorldTile(IsActive: true, Type: 703));
_ = plantWorld.TrySetTile(40, 41, new WorldTile(IsActive: true, Type: 1, Slope: 3));
WorldGridSnapshot slopedSaplingSnapshot = plantWorld.CreateSnapshot(plantMetadata);
if (!PlantPlacementQuery.CanPlace(
      slopedSaplingSnapshot,
      tileDefinitions,
      40,
      40,
      703) ||
    PlantCheckQuery.Evaluate(slopedSaplingSnapshot, tileDefinitions, 40, 40).ShouldDestroy)
{
  throw new InvalidOperationException("Plant 703 bottom-slope support diverged from legacy rules.");
}

Console.WriteLine("PASS: plant placement and check queries preserve bounded support decisions");

WorldGrid foodPlatterWorld = new(replayWidth, replayHeight);
_ = foodPlatterWorld.TrySetTile(60, 60, new WorldTile(IsActive: true, Type: 520));
_ = foodPlatterWorld.TrySetTile(60, 61, new WorldTile(IsActive: true, Type: 1));
FoodPlatterSnapshot foodPlatter = new(
  EntityId: 700,
  TileX: 60,
  TileY: 60,
  Exists: true,
  StoredItem: new ItemStack(123, 2));
WorldGridSnapshot supportedFoodPlatterSnapshot = foodPlatterWorld.CreateSnapshot(plantMetadata);
if (FoodPlatterDestructionQuery.Evaluate(
      supportedFoodPlatterSnapshot,
      tileDefinitions,
      foodPlatter).ShouldDestroy)
{
  throw new InvalidOperationException("Supported Food Platter was incorrectly marked for destruction.");
}

_ = foodPlatterWorld.TrySetTile(60, 61, default);
WorldGridSnapshot unsupportedFoodPlatterSnapshot = foodPlatterWorld.CreateSnapshot(plantMetadata);
if (!FoodPlatterDestructionCommandSystem.TryCreateBatch(
      unsupportedFoodPlatterSnapshot,
      tileDefinitions,
      foodPlatter,
      sequence: 94,
      out FoodPlatterDestructionBatch foodPlatterBatch) ||
    !foodPlatterBatch.RemoveTileEntity || foodPlatterBatch.DroppedItem != new ItemStack(123, 2))
{
  throw new InvalidOperationException("Food Platter destruction batch did not preserve entity and drop intent.");
}

Dictionary<int, TileEntityPersistentState> foodPlatterEntities = new()
{
  [foodPlatter.EntityId] = new TileEntityPersistentState(
    id: foodPlatter.EntityId,
    type: 7,
    tileX: foodPlatter.TileX,
    tileY: foodPlatter.TileY,
    payload: Array.Empty<byte>(),
    isOpaque: true)
};
List<ItemStack> foodPlatterDrops = new();
if (!new FoodPlatterDestructionCommitSystem().TryCommit(
      foodPlatterWorld,
      foodPlatterEntities,
      foodPlatterBatch,
      foodPlatterDrops,
      out FoodPlatterDestructionCommitResult foodPlatterCommit) ||
    !foodPlatterCommit.TileEntityRemoved || !foodPlatterCommit.ItemDropped ||
    foodPlatterWorld.GetTile(60, 60).IsActive || foodPlatterEntities.Count != 0 ||
    !foodPlatterDrops.SequenceEqual(new[] { new ItemStack(123, 2) }))
{
  throw new InvalidOperationException("Food Platter destruction did not commit atomically.");
}

WorldGrid vineWorld = new(replayWidth, replayHeight);
_ = vineWorld.TrySetTile(40, 40, new WorldTile(IsActive: true, Type: 52, FrameX: 18));
_ = vineWorld.TrySetTile(40, 39, new WorldTile(IsActive: true, Type: 60));
WorldGridSnapshot vineSnapshot = vineWorld.CreateSnapshot(plantMetadata);
VineFrameResult vineConversion = VineFrameQuery.Evaluate(vineSnapshot, 40, 40);
if (vineConversion.ShouldKeep || vineConversion.ShouldKill ||
    vineConversion.ReplacementTileType != 62 ||
    !VineFrameCommandSystem.TryCreateCommand(
      vineSnapshot,
      40,
      40,
      sequence: 90,
      out TileChangeCommand vineCommand) ||
    vineCommand.Kind != TileChangeKind.UpdateTileType || vineCommand.TileType != 62 ||
    vineCommand.FrameX != 18)
{
  throw new InvalidOperationException("Vine conversion did not preserve the legacy mutation intent.");
}

_ = vineWorld.TrySetTile(40, 39, new WorldTile(IsActive: true, Type: 2));
VineFrameResult supportedVine = VineFrameQuery.Evaluate(
  vineWorld.CreateSnapshot(plantMetadata),
  40,
  40);
if (!supportedVine.ShouldKeep || supportedVine.ShouldKill ||
    supportedVine.ReplacementTileType is not null)
{
  throw new InvalidOperationException("Supported vines were not retained.");
}

_ = vineWorld.TrySetTile(40, 39, new WorldTile(IsActive: true, Type: 2, Slope: 3));
WorldGridSnapshot unsupportedVineSnapshot = vineWorld.CreateSnapshot(plantMetadata);
VineFrameResult unsupportedVine = VineFrameQuery.Evaluate(unsupportedVineSnapshot, 40, 40);
if (!unsupportedVine.ShouldKill ||
    !VineFrameCommandSystem.TryCreateCommand(
      unsupportedVineSnapshot,
      40,
      40,
      sequence: 91,
      out TileChangeCommand killVineCommand) ||
    killVineCommand.Kind != TileChangeKind.Kill)
{
  throw new InvalidOperationException("Unsupported vines were not converted to kill commands.");
}

WorldGrid cactusWorld = new(replayWidth, replayHeight);
_ = cactusWorld.TrySetTile(50, 50, new WorldTile(IsActive: true, Type: 80));
_ = cactusWorld.TrySetTile(50, 51, new WorldTile(IsActive: true, Type: 53));
WorldGridSnapshot cactusSnapshot = cactusWorld.CreateSnapshot(plantMetadata);
CactusFrameResult groundedCactus = CactusFrameQuery.Evaluate(cactusSnapshot, 50, 50);
if (groundedCactus.ShouldKill || groundedCactus.SupportX != 50 ||
    groundedCactus.SupportY != 51 ||
    CactusFrameCommandSystem.TryCreateCommand(
      cactusSnapshot,
      50,
      50,
      sequence: 92,
      out TileChangeCommand groundedCactusCommand) ||
    groundedCactusCommand != default)
{
  throw new InvalidOperationException("Grounded cactus was not preserved without a command.");
}

_ = cactusWorld.TrySetTile(50, 51, new WorldTile(IsActive: true, Type: 1));
WorldGridSnapshot unsupportedCactusSnapshot = cactusWorld.CreateSnapshot(plantMetadata);
CactusFrameResult unsupportedCactus = CactusFrameQuery.Evaluate(
  unsupportedCactusSnapshot,
  50,
  50);
if (!unsupportedCactus.ShouldKill ||
    !CactusFrameCommandSystem.TryCreateCommand(
      unsupportedCactusSnapshot,
      50,
      50,
      sequence: 93,
      out TileChangeCommand killCactusCommand) ||
    killCactusCommand.Kind != TileChangeKind.Kill)
{
  throw new InvalidOperationException("Unsupported cactus was not converted to a kill command.");
}

if (!TileSlopingQuery.ForbidsSloping(21) || !TileSlopingQuery.ForbidsSloping(597) ||
    TileSlopingQuery.ForbidsSloping(1))
{
  throw new InvalidOperationException("Tile sloping protection table diverged from legacy rules.");
}

WorldGrid poundingWorld = new(replayWidth, replayHeight);
WorldMetadata poundingMetadata = new(
  "worldgen-tile-pounding-query",
  new WorldSeed(1456),
  replayWidth,
  replayHeight);
_ = poundingWorld.TrySetTile(50, 50, new WorldTile(IsActive: true, Type: 1));
WorldGridSnapshot poundableSnapshot = poundingWorld.CreateSnapshot(poundingMetadata);
HashSet<ushort> boulderTileTypes = [138, 484, 664, 665, 711, 712, 713, 714, 715, 716];
if (!TilePoundingEligibilityQuery.CanPound(
      poundableSnapshot,
      50,
      50,
      boulderTileTypes,
      isGeneratingOrLoadingWorld: false,
      canKillTile: static (_, _) => true) ||
    TilePoundingEligibilityQuery.CanPound(
      poundableSnapshot,
      50,
      50,
      boulderTileTypes,
      isGeneratingOrLoadingWorld: false,
      canKillTile: static (_, _) => false))
{
  throw new InvalidOperationException(
    "Tile pounding did not preserve the final CanKillTile guard.");
}

_ = poundingWorld.TrySetTile(50, 50, new WorldTile(IsActive: true, Type: 138));
WorldGridSnapshot boulderPoundingSnapshot = poundingWorld.CreateSnapshot(poundingMetadata);
if (TilePoundingEligibilityQuery.CanPound(
      boulderPoundingSnapshot,
      50,
      50,
      boulderTileTypes,
      isGeneratingOrLoadingWorld: false,
      canKillTile: static (_, _) => true))
{
  throw new InvalidOperationException("Boulder tiles were accepted for pounding.");
}

_ = poundingWorld.TrySetTile(50, 50, new WorldTile(IsActive: true, Type: 190));
WorldGridSnapshot generatingPoundingSnapshot = poundingWorld.CreateSnapshot(poundingMetadata);
if (TilePoundingEligibilityQuery.CanPound(
      generatingPoundingSnapshot,
      50,
      50,
      boulderTileTypes,
      isGeneratingOrLoadingWorld: true,
      canKillTile: static (_, _) => true))
{
  throw new InvalidOperationException("Generating-world pounding exception was not preserved.");
}

_ = poundingWorld.TrySetTile(50, 50, new WorldTile(IsActive: true, Type: 1));
_ = poundingWorld.TrySetTile(50, 49, new WorldTile(IsActive: true, Type: 21));
WorldGridSnapshot protectedAbovePoundingSnapshot = poundingWorld.CreateSnapshot(poundingMetadata);
if (TilePoundingEligibilityQuery.CanPound(
      protectedAbovePoundingSnapshot,
      50,
      50,
      boulderTileTypes,
      isGeneratingOrLoadingWorld: false,
      canKillTile: static (_, _) => true))
{
  throw new InvalidOperationException("Protected above-tile relation was accepted for pounding.");
}

Console.WriteLine("PASS: tile pounding queries preserve bounded legacy eligibility");

_ = poundingWorld.TrySetTile(
  50,
  50,
  new WorldTile(
    IsActive: true,
    Type: 1,
    LiquidAmount: 120,
    LiquidType: 2,
    FrameX: 36,
    FrameY: 54,
    WallType: 7,
    HasWire: true,
    IsHalfBrick: true));
_ = poundingWorld.TrySetTile(50, 49, default);
WorldGridSnapshot shapedPoundingSnapshot = poundingWorld.CreateSnapshot(poundingMetadata);
if (!TilePoundingCommandSystem.TryCreateSlopeCommand(
      shapedPoundingSnapshot,
      50,
      50,
      slope: 3,
      sequence: 20,
      boulderTileTypes: boulderTileTypes,
      isGeneratingOrLoadingWorld: false,
      canKillTile: static (_, _) => true,
      out TileChangeCommand slopeCommand) ||
    slopeCommand.Kind != TileChangeKind.UpdateTileShape || slopeCommand.IsHalfBrick != false ||
    slopeCommand.Slope != 3)
{
  throw new InvalidOperationException(
    "Tile slope command did not preserve the legacy shape intent.");
}

WorldGrid shapedPoundingWorld = WorldGrid.FromSnapshot(shapedPoundingSnapshot);
if (!new TileChangeCommitSystem().TryCommit(
      shapedPoundingWorld,
      new[] { slopeCommand },
      out TileChangeCommitResult slopeCommit) || slopeCommit.AppliedCount != 1)
{
  throw new InvalidOperationException("Tile slope command did not commit.");
}

WorldTile slopedTile = shapedPoundingWorld.GetTile(50, 50);
if (slopedTile.IsHalfBrick || slopedTile.Slope != 3 || slopedTile.Type != 1 ||
    slopedTile.LiquidAmount != 120 || slopedTile.LiquidType != 2 || slopedTile.FrameX != 36 ||
    slopedTile.FrameY != 54 || slopedTile.WallType != 7 || !slopedTile.HasWire)
{
  throw new InvalidOperationException("Tile slope command did not preserve unrelated tile state.");
}

WorldGridSnapshot poundableShapeSnapshot = shapedPoundingWorld.CreateSnapshot(poundingMetadata);
if (!TilePoundingCommandSystem.TryCreatePoundCommand(
      poundableShapeSnapshot,
      50,
      50,
      sequence: 21,
      boulderTileTypes: boulderTileTypes,
      isGeneratingOrLoadingWorld: false,
      canKillTile: static (_, _) => true,
      out TileChangeCommand poundCommand) || poundCommand.IsHalfBrick != true ||
    poundCommand.Slope is not null)
{
  throw new InvalidOperationException("Tile pound command did not toggle half-brick state.");
}

Console.WriteLine("PASS: tile slope and pound commands preserve bounded shape updates");

TileMergeNeighbors mergeNeighbors = new(
  Up: 2,
  Down: 8,
  Left: 2,
  Right: 7,
  UpLeft: 2,
  UpRight: 9,
  DownLeft: 2,
  DownRight: 10);
TileMergeFrametestResult frametest = TileMergeQuery.ApplyFrametest(
  tileType: 1,
  lookForTileType: 2,
  neighbors: mergeNeighbors,
  mergeUp: false,
  mergeDown: true,
  mergeLeft: false,
  mergeRight: false);
if (frametest.Neighbors.Up != 1 || frametest.Neighbors.Down != 8 ||
    frametest.Neighbors.Left != 2 || frametest.Neighbors.UpLeft != 1 ||
    frametest.Neighbors.DownLeft != 1 || !frametest.FrameUp || frametest.FrameDown ||
    !frametest.FrameLeft || frametest.FrameRight)
{
  throw new InvalidOperationException("Tile merge frametest semantics diverged from legacy rules.");
}

WorldGrid mergeFrameWorld = new(replayWidth, replayHeight);
_ = mergeFrameWorld.TrySetTile(10, 9, new WorldTile(IsActive: true, Type: 1));
IReadOnlyList<TileFrameRequest> mergeFrameRequests = frametest.CreateFrameRequests(
  10,
  10,
  mergeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  Array.Empty<TileChangeCommand>());
if (mergeFrameRequests.Count != 2 || mergeFrameRequests[0].X != 10 ||
    mergeFrameRequests[0].Y != 9 ||
    mergeFrameRequests[0].MutationKind != TileFrameMutationKind.TileMergeFrametest ||
    mergeFrameRequests[1].X != 9 || mergeFrameRequests[1].Y != 10 ||
    mergeFrameRequests[1].MutationKind != TileFrameMutationKind.TileMergeFrametest)
{
  throw new InvalidOperationException("Tile merge frametest did not create the requested frame work.");
}

HashSet<int> mergeLookForTileTypes = [2, 7];
TileMergeNeighbors cardinalMerge = TileMergeQuery.ReplaceCardinal(
  1,
  mergeLookForTileTypes,
  mergeNeighbors);
if (cardinalMerge.Up != 1 || cardinalMerge.Down != 8 || cardinalMerge.Left != 1 ||
    cardinalMerge.Right != 1 || cardinalMerge.UpLeft != 2 || cardinalMerge.DownLeft != 2)
{
  throw new InvalidOperationException("Cardinal tile merge altered the wrong neighbor set.");
}

TileMergeNeighbors allMerge = TileMergeQuery.ReplaceAllExcept(
  1,
  mergeLookForTileTypes,
  excludedTileType: 7,
  neighbors: mergeNeighbors);
if (allMerge.Up != 1 || allMerge.Left != 1 || allMerge.UpLeft != 1 ||
    allMerge.DownLeft != 1 || allMerge.Right != 7)
{
  throw new InvalidOperationException("Excluded tile merge semantics diverged from legacy rules.");
}

HashSet<int> excludedTileTypes = [2, 10];
TileMergeNeighbors weirdMerge = TileMergeQuery.ReplaceDifferentExcept(
  tileType: 1,
  replacementTileType: 99,
  excludedTileTypes: excludedTileTypes,
  neighbors: mergeNeighbors);
if (weirdMerge.Up != 2 || weirdMerge.Down != 99 || weirdMerge.Left != 2 ||
    weirdMerge.Right != 99 || weirdMerge.UpRight != 99 || weirdMerge.DownRight != 10)
{
  throw new InvalidOperationException("Weird tile merge semantics diverged from legacy rules.");
}

Console.WriteLine("PASS: tile merge queries preserve bounded neighbor rewrite semantics");

WorldTile visibleTile = new(IsActive: true, Type: 1);
WorldTile invisibleTile = visibleTile with { IsInvisibleBlock = true };
TileMergeCullMask mergeCullMask = TileMergeCullingQuery.Evaluate(
  invisibleTile,
  visibleTile,
  invisibleTile,
  null,
  visibleTile,
  visibleTile,
  invisibleTile,
  invisibleTile,
  visibleTile,
  showInvisibleBlocks: false);
TileMergeCullMask visibleMergeCullMask = TileMergeCullingQuery.Evaluate(
  invisibleTile,
  visibleTile,
  invisibleTile,
  null,
  visibleTile,
  visibleTile,
  invisibleTile,
  invisibleTile,
  visibleTile,
  showInvisibleBlocks: true);
if (!mergeCullMask.CullUp || mergeCullMask.CullDown || mergeCullMask.CullLeft ||
    !mergeCullMask.CullRight || !mergeCullMask.CullUpLeft ||
    mergeCullMask.CullUpRight || mergeCullMask.CullDownLeft ||
    !mergeCullMask.CullDownRight || visibleMergeCullMask != default)
{
  throw new InvalidOperationException(
    "Tile merge culling did not preserve the explicit invisible-block visibility policy.");
}

if (MossColorQuery.GetColor(179) != 0 || MossColorQuery.GetColor(512) != 0 ||
    MossColorQuery.GetColor(381) != 5 || MossColorQuery.GetColor(540) != 8 ||
    MossColorQuery.GetColor(625) != 9 || MossColorQuery.GetColor(628) != 10 ||
    MossColorQuery.GetColor(1) != -1)
{
  throw new InvalidOperationException("Moss color mapping diverged from legacy tile IDs.");
}

Console.WriteLine("PASS: moss color query preserves bounded legacy tile mappings");

Dictionary<ushort, OrePatchTileDefinition> orePatchDefinitions = new()
{
  [1] = new OrePatchTileDefinition(1, IsSolid: true, IsGrass: true, false, false, false)
};
WorldGrid orePatchWorld = new(replayWidth, replayHeight);
for (int x = 210; x <= 230; x++)
{
  for (int y = 217; y <= 240; y++)
  {
    _ = orePatchWorld.TrySetTile(x, y, new WorldTile(true, 1, WallType: 1));
  }
}

_ = orePatchWorld.TrySetTile(220, 210, new WorldTile(true, 1));
_ = orePatchWorld.TrySetTile(219, 210, new WorldTile(true, 1));
_ = orePatchWorld.TrySetTile(221, 210, new WorldTile(true, 1));
OrePatchEligibilityResult orePatchEligibility = OrePatchEligibilityQuery.Evaluate(
  orePatchWorld.CreateSnapshot(replayRequest.Metadata),
  220,
  205,
  worldSurfaceY: 220,
  orePatchDefinitions);
if (!orePatchEligibility.IsEligible || orePatchEligibility.GroundY != 210)
{
  throw new InvalidOperationException("Ore patch eligibility diverged from legacy support checks.");
}

_ = orePatchWorld.TrySetTile(220, 220, new WorldTile(true, 1));
orePatchEligibility = OrePatchEligibilityQuery.Evaluate(
  orePatchWorld.CreateSnapshot(replayRequest.Metadata),
  220,
  205,
  worldSurfaceY: 220,
  orePatchDefinitions);
if (orePatchEligibility.Reason != OrePatchEligibilityReason.SupportWallMissing)
{
  throw new InvalidOperationException("Ore patch eligibility did not reject missing support walls.");
}

_ = orePatchWorld.TrySetTile(220, 220, new WorldTile(true, 1, WallType: 1));
Console.WriteLine("PASS: ore patch eligibility preserves bounded legacy support checks");

WorldSizeProfile smallWorldSize = WorldSizeProfile.FromLegacyIndex(0);
WorldSizeProfile mediumWorldSize = WorldSizeProfile.FromLegacyIndex(1);
WorldSizeProfile largeWorldSize = WorldSizeProfile.FromLegacyIndex(-1);
if (smallWorldSize.Width != 4200 || smallWorldSize.Height != 1200 ||
    mediumWorldSize.Width != 6400 || mediumWorldSize.Height != 1800 ||
    largeWorldSize.Width != 8400 || largeWorldSize.Height != 2400 ||
    WorldSizeProfile.GetLegacyIndexForWidth(4200) != 0 ||
    WorldSizeProfile.GetLegacyIndexForWidth(4201) != 1 ||
    WorldSizeProfile.GetLegacyIndexForWidth(6400) != 1 ||
    WorldSizeProfile.GetLegacyIndexForWidth(6401) != 2)
{
  throw new InvalidOperationException(
    "World size profiles did not preserve legacy dimensions and width classification.");
}

WorldBoundsComponent largeWorldBounds = new(largeWorldSize.Width, largeWorldSize.Height);
if (largeWorldBounds.PixelWidth != 134400 ||
    largeWorldBounds.PixelHeight != 38400 ||
    largeWorldBounds.SectionColumnCount != 42 ||
    largeWorldBounds.SectionRowCount != 16)
{
  throw new InvalidOperationException(
    "World bounds did not derive legacy pixel and section dimensions.");
}

string evidenceDirectory = Path.Combine(repositoryRoot, "docs", "worldgen");
Directory.CreateDirectory(evidenceDirectory);
WorldgenInventory inventory = new(
  "WorldGen.cs",
  sourcePath,
  new FileEvidence(
    new FileInfo(sourcePath).Length,
    File.ReadLines(sourcePath).Count(),
    Convert.ToHexString(SHA256.HashData(sourceBytes)),
    typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown"),
  methods,
  fields,
  references,
  new ReplayEvidence(
    replayRequest.Metadata.Width,
    replayRequest.Metadata.Height,
    replayRequest.Metadata.Seed.Value,
    replayRequest.SpawnX,
    replayRequest.SurfaceY,
    firstFingerprint,
    GetSectionVersions(firstSnapshot)));
JsonSerializerOptions options = new() { WriteIndented = true };
File.WriteAllText(
  Path.Combine(evidenceDirectory, "worldgen-source-inventory.json"),
  JsonSerializer.Serialize(inventory, options));
File.WriteAllText(
  Path.Combine(evidenceDirectory, "worldgen-method-map.md"),
  CreateMethodMap(inventory));
Console.WriteLine(
  $"PASS: stage 0 inventory contains {methods.Count} methods and {fields.Count} fields");
Console.WriteLine("PASS: legacy GenerateWorld has a bounded partial mapping with exclusions");
Console.WriteLine($"PASS: deterministic replay fingerprint {firstFingerprint}");

if (typeof(WorldGenerationRequest).GetProperty("RockLayerY") is null)
{
  throw new InvalidOperationException(
    "WorldGenerationRequest must expose the frozen rock-layer input.");
}

WorldGenerationRequest enrichedRequest = new(
  replayRequest.Metadata,
  replayRequest.SpawnX,
  replayRequest.SurfaceY,
  seedVariant: "default",
    randomStreamVersion: 1,
    generationId: 42);
if (enrichedRequest.GenerationId != 42 ||
    enrichedRequest.SeedVariant != "default" ||
    enrichedRequest.RandomStreamVersion != 1 ||
    enrichedRequest.RockLayerY != 153)
{
  throw new InvalidOperationException("WorldGenerationRequest did not freeze generation inputs.");
}

WorldGenerationRequest unsupportedRuleRequest = new(
  replayRequest.Metadata,
  replayRequest.SpawnX,
  replayRequest.SurfaceY,
  seedVariant: "for-the-worthy",
  rules: new WorldRuleSnapshotComponent(0, "for-the-worthy", false));
try
{
  _ = new WorldGenerationPipeline().Generate(unsupportedRuleRequest);
  throw new InvalidOperationException(
    "Unsupported secret-seed generation did not fail before world generation.");
}
catch (NotSupportedException)
{
}

Console.WriteLine("PASS: unsupported legacy world rules fail before generation");

WorldGenerationBootstrap bootstrap = new WorldGenerationStageSystem().Initialize(enrichedRequest);
if (bootstrap.State.GenerationId != enrichedRequest.GenerationId ||
    bootstrap.Seed.Seed != enrichedRequest.Metadata.Seed.Value ||
    bootstrap.Bounds.Width != replayWidth ||
    bootstrap.Rules.SecretSeedVariant != "default" ||
    bootstrap.Cursor.Stage != WorldGenerationStage.Created)
{
  throw new InvalidOperationException(
    "World generation stage bootstrap did not freeze components.");
}

WorldGenerationStateComponent state = new(42);
if (!state.TryAdvance(WorldGenerationStage.Terrain) ||
    state.TryAdvance(WorldGenerationStage.Created) ||
    state.Stage != WorldGenerationStage.Terrain)
{
  throw new InvalidOperationException("World generation stages did not advance monotonically.");
}

WorldGrid commitWorld = new(replayWidth, replayHeight);
WorldSectionCoordinates committedSection = new(0, 0);
IReadOnlyList<TileChangeCommand> commands = new[]
{
  new TileChangeCommand(2, 11, 10, TileChangeKind.Place, 2),
  new TileChangeCommand(1, 10, 10, TileChangeKind.Place, 1)
};
TileChangeCommitSystem commitSystem = new();
if (!Enum.TryParse("SetWall", out TileChangeKind _) ||
    typeof(TileChangeCommand).GetProperty("WallType") is null)
{
  throw new InvalidOperationException(
    "Tile changes must represent a wall-only mutation without replacing the block.");
}

if (!commitSystem.TryCommit(commitWorld, commands, out TileChangeCommitResult commitResult) ||
    commitResult.AppliedCount != 2 ||
    commitWorld.GetTile(10, 10).Type != 1 ||
    commitWorld.GetTile(11, 10).Type != 2 ||
    commitWorld.GetSectionVersion(committedSection) != 2)
{
  throw new InvalidOperationException("Tile changes did not commit in stable sequence order.");
}

WorldTile wallSourceTile = new(
  IsActive: true,
  Type: 12,
  LiquidAmount: 91,
  LiquidType: 3,
  FrameX: 144,
  FrameY: 216,
  WallType: 2,
  HasWire: true,
  Slope: 3);
if (!commitWorld.TrySetTile(20, 20, wallSourceTile) ||
    !commitSystem.TryCommit(
      commitWorld,
      new[] { new TileChangeCommand(3, 20, 20, TileChangeKind.SetWall, 0, 9) },
      out TileChangeCommitResult wallCommitResult) ||
    wallCommitResult.AppliedCount != 1)
{
  throw new InvalidOperationException("Wall-only tile commands could not be committed.");
}

WorldTile wallUpdatedTile = commitWorld.GetTile(20, 20);
if (wallUpdatedTile != wallSourceTile with { WallType = 9 })
{
  throw new InvalidOperationException(
    "Wall-only tile commands did not preserve the existing tile state.");
}

WorldTile killedTile = TileMutationProjection.Apply(
  wallSourceTile,
  new TileChangeCommand(4, 20, 20, TileChangeKind.Kill, 0));
WorldTile preservedLiquidTile = TileMutationProjection.Apply(
  wallSourceTile,
  new TileChangeCommand(5, 20, 20, TileChangeKind.Kill, 0, PreserveLiquid: true));
if (killedTile != default ||
    preservedLiquidTile != new WorldTile(
      IsActive: false,
      Type: 0,
      LiquidAmount: wallSourceTile.LiquidAmount,
      LiquidType: wallSourceTile.LiquidType))
{
  throw new InvalidOperationException(
    "Tile kill projection did not preserve the explicit liquid contract.");
}

long versionBeforeRejectedBatch = commitWorld.GetSectionVersion(committedSection);
IReadOnlyList<TileChangeCommand> rejectedCommands = new[]
{
  new TileChangeCommand(3, 12, 10, TileChangeKind.Place, 3),
  new TileChangeCommand(3, 13, 10, TileChangeKind.Place, 4)
};
if (commitSystem.TryCommit(
      commitWorld,
      rejectedCommands,
      out TileChangeCommitResult rejectedResult) ||
    rejectedResult.FailureReason is null ||
    commitWorld.GetSectionVersion(committedSection) != versionBeforeRejectedBatch ||
    commitWorld.GetTile(12, 10) != default)
{
  throw new InvalidOperationException("Invalid tile batches were not rejected atomically.");
}

if (commitSystem.TryCommit(
      commitWorld,
      new[] { new TileChangeCommand(long.MaxValue, 10, 10, TileChangeKind.Place, 3) },
      out _))
{
  throw new InvalidOperationException("Tile commit accepted a sequence that would overflow next sequence.");
}

Console.WriteLine("PASS: frozen generation inputs, monotonic stages, and atomic tile commit");

if (Type.GetType(
      "Terraria.Dome.Simulation.WorldGeneration.Systems.DirtWallBackgroundSystem, " +
      "Terraria.Dome.Simulation") is null)
{
  throw new InvalidOperationException(
    "The legacy DirtWallBackgrounds source slice must have an ECS system.");
}

WorldGrid wallBackgroundWorld = new(replayWidth, replayHeight);
foreach ((int x, int y) in new[]
{
  (9, 5), (10, 5), (11, 5), (9, 6), (10, 6), (11, 6)
})
{
  _ = wallBackgroundWorld.TrySetTile(x, y, new WorldTile(true, 1));
}

WorldGenerationStateComponent wallBackgroundState = new(42);
_ = wallBackgroundState.TryAdvance(WorldGenerationStage.Terrain);
_ = wallBackgroundState.TryAdvance(WorldGenerationStage.Cave);
_ = wallBackgroundState.TryAdvance(WorldGenerationStage.Biome);
List<TileChangeCommand> wallBackgroundCommands = new();
new DirtWallBackgroundSystem().AppendCommands(
  wallBackgroundWorld.CreateSnapshot(replayRequest.Metadata),
  worldSurfaceY: 6,
  Enumerable.Repeat(0, replayWidth - 2).ToArray(),
  ref wallBackgroundState,
  wallBackgroundCommands);
if (!wallBackgroundCommands.Any(command =>
      command.X == 10 && command.Y == 6 && command.Kind == TileChangeKind.SetWall) ||
    !commitSystem.TryCommit(
      wallBackgroundWorld,
      wallBackgroundCommands,
      out TileChangeCommitResult wallBackgroundResult) ||
    !wallBackgroundResult.Succeeded ||
    wallBackgroundWorld.GetTile(10, 6) != new WorldTile(true, 1, WallType: 2))
{
  throw new InvalidOperationException(
    "Dirt wall backgrounds did not preserve the enclosed tile while writing a dirt wall.");
}

WorldGenerationRequest capturedWallRequest = new(
  replayRequest.Metadata,
  replayRequest.SpawnX,
  replayRequest.SurfaceY,
  seedVariant: replayRequest.SeedVariant,
  randomStreamVersion: replayRequest.RandomStreamVersion,
  generationId: replayRequest.GenerationId,
  rules: replayRequest.Rules,
  rockLayerY: replayRequest.RockLayerY,
  dirtWallSurfaceOffsetChanges: Enumerable.Repeat(0, replayWidth - 2).ToArray());
WorldGrid defaultWallPipelineWorld = new WorldGenerationPipeline().Generate(replayRequest);
WorldGrid capturedWallPipelineWorld = new WorldGenerationPipeline().Generate(capturedWallRequest);
int defaultDirtWallCount = CountWalls(defaultWallPipelineWorld, wallType: 2);
int capturedDirtWallCount = CountWalls(capturedWallPipelineWorld, wallType: 2);
if (capturedDirtWallCount <= defaultDirtWallCount)
{
  throw new InvalidOperationException(
    "An explicit DirtWallBackgrounds oracle did not add wall-only pipeline output.");
}

IReadOnlyList<int> parsedDirtWallOffsets = ParseDirtWallOffsetChanges(
  new[] { "1,-1", "2,0", "3,1" },
  expectedWorldWidth: 5);
if (!parsedDirtWallOffsets.SequenceEqual(new[] { -1, 0, 1 }))
{
  throw new InvalidOperationException(
    "Dirt wall offset artifacts were not parsed in their recorded column order.");
}

static int CountWalls(WorldGrid world, ushort wallType)
{
  int count = 0;
  for (int y = 0; y < world.Height; y++)
  {
    for (int x = 0; x < world.Width; x++)
    {
      if (world.GetTile(x, y).WallType == wallType)
      {
        count++;
      }
    }
  }

  return count;
}

static IReadOnlyList<int> ParseDirtWallOffsetChanges(
  IReadOnlyList<string> rows,
  int expectedWorldWidth)
{
  ArgumentNullException.ThrowIfNull(rows);
  if (expectedWorldWidth < 3)
  {
    throw new ArgumentOutOfRangeException(nameof(expectedWorldWidth));
  }

  int expectedCount = expectedWorldWidth - 2;
  if (rows.Count != expectedCount)
  {
    throw new ArgumentException(
      "A dirt wall offset row is required for every non-edge column.",
      nameof(rows));
  }

  int[] changes = new int[expectedCount];
  for (int index = 0; index < rows.Count; index++)
  {
    string[] values = rows[index].Split(',', StringSplitOptions.TrimEntries);
    if (values.Length != 2 ||
        !int.TryParse(values[0], out int column) ||
        !int.TryParse(values[1], out int delta))
    {
      throw new ArgumentException("A dirt wall offset row is malformed.", nameof(rows));
    }

    if (column != index + 1 || delta < -1 || delta > 1)
    {
      throw new ArgumentException("A dirt wall offset row is outside the legacy range.", nameof(rows));
    }

    changes[index] = delta;
  }

  return Array.AsReadOnly(changes);
}

TerrainProfileComponent terrainProfile = new(
  surfaceY: replaySurfaceY,
  rockLayerY: 150,
  underworldY: 270);
if (terrainProfile.SurfaceY >= terrainProfile.RockLayerY ||
    terrainProfile.RockLayerY >= terrainProfile.UnderworldY ||
    terrainProfile.UnderworldY >= replayHeight)
{
  throw new InvalidOperationException("Terrain profile height bands were not ordered.");
}

WorldGrid caveWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent caveState = new(42);
List<TileChangeCommand> terrainCommands = new();
GenerationRandomState terrainRandomState = new(unchecked((uint)replayRequest.Metadata.Seed.Value));
new TerrainBaseSystem().AppendCommands(
  caveWorld,
  replayRequest,
  terrainProfile,
  ref caveState,
  ref terrainRandomState,
  terrainCommands);
if (!new TileChangeCommitSystem().TryCommit(caveWorld, terrainCommands, out _))
{
  throw new InvalidOperationException("Terrain base commands could not be committed.");
}

WorldGridSnapshot caveSnapshot = caveWorld.CreateSnapshot(replayRequest.Metadata);
GenerationCursorComponent terrainCursor = new GenerationCursorComponent(
  WorldGenerationStage.Created,
  0,
  0,
  0).Advance(WorldGenerationStage.Terrain, 0, 1, terrainRandomState.Value);
WorldGenerationCheckpoint terrainCheckpoint = new(caveSnapshot, caveState, terrainCursor);
WorldGrid uninterruptedCaveWorld = terrainCheckpoint.RestoreWorld();
WorldGrid restartedCaveWorld = terrainCheckpoint.RestoreWorld();
WorldGenerationStateComponent uninterruptedCaveState = terrainCheckpoint.State;
WorldGenerationStateComponent restartedCaveState = terrainCheckpoint.State;
List<TileChangeCommand> firstCaveCommands = new();
List<TileChangeCommand> secondCaveCommands = new();
CaveCarvingComponent caveProfile = new("single-tunnel", radius: 1, density: 4);
new CaveCarvingSystem().AppendCommands(
  caveSnapshot,
  replayRequest,
  caveProfile,
  ref caveState,
  firstCaveCommands);
WorldGenerationStateComponent secondCaveState = new(42);
List<TileChangeCommand> ignoredTerrainCommands = new();
new TerrainBaseSystem().AppendCommands(
  caveWorld,
  replayRequest,
  terrainProfile,
  ref secondCaveState,
  ignoredTerrainCommands);
new CaveCarvingSystem().AppendCommands(
  caveSnapshot,
  replayRequest,
  caveProfile,
  ref secondCaveState,
  secondCaveCommands);
List<TileChangeCommand> uninterruptedCommands = new();
List<TileChangeCommand> restartedCommands = new();
new CaveCarvingSystem().AppendCommands(
  uninterruptedCaveWorld.CreateSnapshot(replayRequest.Metadata),
  replayRequest,
  caveProfile,
  ref uninterruptedCaveState,
  uninterruptedCommands);
new CaveCarvingSystem().AppendCommands(
  restartedCaveWorld.CreateSnapshot(replayRequest.Metadata),
  replayRequest,
  caveProfile,
  ref restartedCaveState,
  restartedCommands);
bool uninterruptedCommitted = new TileChangeCommitSystem().TryCommit(
  uninterruptedCaveWorld,
  uninterruptedCommands,
  out TileChangeCommitResult uninterruptedCommit);
bool restartedCommitted = new TileChangeCommitSystem().TryCommit(
  restartedCaveWorld,
  restartedCommands,
  out TileChangeCommitResult restartedCommit);
string uninterruptedCaveFingerprint = CreateSnapshotFingerprint(
  uninterruptedCaveWorld.CreateSnapshot(replayRequest.Metadata));
string restartedCaveFingerprint = CreateSnapshotFingerprint(
  restartedCaveWorld.CreateSnapshot(replayRequest.Metadata));
if (!uninterruptedCommitted ||
    !restartedCommitted ||
     uninterruptedCommit.AppliedCount != restartedCommit.AppliedCount ||
    !StringComparer.Ordinal.Equals(uninterruptedCaveFingerprint, restartedCaveFingerprint) ||
    uninterruptedCaveState.Stage != restartedCaveState.Stage ||
    uninterruptedCaveState.NextSequence != restartedCaveState.NextSequence ||
    terrainCheckpoint.Cursor != new GenerationCursorComponent(
      WorldGenerationStage.Terrain,
      0,
      1,
      terrainRandomState.Value))
{
  throw new InvalidOperationException("Cursor checkpoint restart replay was not deterministic.");
}
if (!firstCaveCommands.SequenceEqual(secondCaveCommands) ||
    firstCaveCommands.Any(command =>
      Math.Abs(command.X - replayRequest.SpawnX) <= 4 &&
      command.Y >= replayRequest.SurfaceY &&
      command.Y <= replayRequest.SurfaceY + 7))
{
  throw new InvalidOperationException(
    "Cave carving was not deterministic or crossed spawn protection.");
}

CursorRestartEvidence cursorEvidence = new(
  "Terrain",
  "Cave",
  terrainCheckpoint.Cursor.SectionX,
  terrainCheckpoint.Cursor.SectionY,
  terrainCheckpoint.Cursor.RandomState,
  restartedCaveFingerprint,
  uninterruptedCaveState.NextSequence,
  restartedCaveState.NextSequence);
File.WriteAllText(
  Path.Combine(evidenceDirectory, "cursor-restart-replay.json"),
  JsonSerializer.Serialize(cursorEvidence, options));
Console.WriteLine(
  $"PASS: cursor checkpoint restart replay resumes deterministically at cave stage " +
  $"fingerprint {restartedCaveFingerprint}");

BiomeSurfaceResult unsupportedBiome = new BiomeSurfaceSystem().AppendCommands(
  caveSnapshot,
  new BiomeSurfaceComponent("unsupported-biome"),
  ref caveState,
  new List<TileChangeCommand>());
if (unsupportedBiome.Supported || unsupportedBiome.FailureReason is null)
{
  throw new InvalidOperationException("Unsupported biome rules were not explicit.");
}

if (!WorldGenerationSystemOrder.Systems.SequenceEqual(new[]
    {
      WorldGenerationSystemId.TerrainBase,
      WorldGenerationSystemId.CaveCarving,
      WorldGenerationSystemId.BiomeSurface,
      WorldGenerationSystemId.OrePlacement,
      WorldGenerationSystemId.StructurePlacement,
      WorldGenerationSystemId.TreePlacement,
      WorldGenerationSystemId.LiquidSource,
      WorldGenerationSystemId.LiquidPropagation,
      WorldGenerationSystemId.TileFrame,
      WorldGenerationSystemId.TileChangeCommit,
      WorldGenerationSystemId.Validation
    }))
{
  throw new InvalidOperationException("World generation system order was not explicit and stable.");
}

Console.WriteLine(
  "PASS: terrain profile, protected deterministic cave, biome failure, and system order");

WorldGrid objectWorld = new(replayWidth, replayHeight);
WorldGridSnapshot objectSnapshot = objectWorld.CreateSnapshot(replayRequest.Metadata);
TileProtectionComponent protection = new(
  replayRequest.SpawnX,
  replayRequest.SurfaceY,
  HalfWidth: 4,
  Height: 7);
OreDefinition copper = new(
  "copper",
  tileType: 7,
  minDepth: replaySurfaceY + 10,
  maxDepth: replaySurfaceY + 80,
  veinRadius: 1,
  priority: 10);
WorldGenerationStateComponent oreState = new(42);
List<TileChangeCommand> oreCommands = new();
new OrePlacementSystem().AppendCommands(
  objectSnapshot,
  replayRequest,
  copper,
  protection,
  ref oreState,
  oreCommands);
if (oreCommands.Count == 0 ||
    oreCommands.Any(command => protection.IsProtected(command.X, command.Y)))
{
  throw new InvalidOperationException(
    "Ore placement did not produce a protected deterministic vein.");
}

OrePlacementTransactionSystem oreTransactionSystem = new();
WorldGrid oreTransactionWorld = new(replayWidth, replayHeight);
OrePlacementPreparationResult orePreparation = oreTransactionSystem.TryPrepare(
  oreTransactionWorld.CreateSnapshot(replayRequest.Metadata),
  replayRequest,
  copper,
  protection,
  out OrePlacementPreparationResult orePreparationResult)
  ? orePreparationResult
  : throw new InvalidOperationException("Ore transaction did not prepare a free footprint.");
WorldGenerationStateComponent oreTransactionState = new(43);
List<TileChangeCommand> oreTransactionCommands = new();
if (!oreTransactionSystem.TryAppendCommands(
      oreTransactionWorld.CreateSnapshot(replayRequest.Metadata),
      orePreparation,
      protection,
      ref oreTransactionState,
      oreTransactionCommands) ||
    oreTransactionCommands.Count == 0 ||
    oreTransactionCommands.Any(command => protection.IsProtected(command.X, command.Y)))
{
  throw new InvalidOperationException("Ore transaction did not append prepared commands.");
}

WorldGrid blockedOreTransactionWorld = new(replayWidth, replayHeight);
_ = blockedOreTransactionWorld.TrySetTile(
  orePreparation.Cells[0].X,
  orePreparation.Cells[0].Y,
  new WorldTile(true, 1));
if (oreTransactionSystem.TryPrepare(
      blockedOreTransactionWorld.CreateSnapshot(replayRequest.Metadata),
      replayRequest,
      copper,
      protection,
      out _) )
{
  throw new InvalidOperationException("Ore transaction did not reject an occupied footprint.");
}

OreDefinition orePatchDefinition = new(
  "ore-patch",
  tileType: 7,
  minDepth: replaySurfaceY + 10,
  maxDepth: replaySurfaceY + 80,
  veinRadius: 0,
  priority: 10);
OrePatchPlacementSystem orePatchPlacementSystem = new();
OrePatchPlacementPreparation orePatchPreparation = orePatchPlacementSystem.TryPrepare(
  orePatchWorld.CreateSnapshot(replayRequest.Metadata),
  originX: 220,
  originY: 205,
  worldSurfaceY: 220,
  orePatchDefinitions,
  orePatchDefinition,
  protection,
  out OrePatchPlacementPreparation orePatchPreparationResult)
  ? orePatchPreparationResult
  : throw new InvalidOperationException("Eligible ore patch did not prepare a transaction.");
WorldGenerationStateComponent orePatchState = new(44);
List<TileChangeCommand> orePatchCommands = new();
_ = orePatchWorld.TrySetTile(
  orePatchPreparation.Transaction.Cells[0].X,
  orePatchPreparation.Transaction.Cells[0].Y,
  new WorldTile(true, 1));
if (orePatchPlacementSystem.TryAppendCommands(
      orePatchWorld.CreateSnapshot(replayRequest.Metadata),
      orePatchPreparation,
      protection,
      ref orePatchState,
      orePatchCommands) ||
    orePatchCommands.Count != 0 ||
    orePatchState.Stage >= WorldGenerationStage.Ore)
{
  throw new InvalidOperationException(
    "Ore patch placement did not reject a post-prepare footprint conflict atomically.");
}

_ = orePatchWorld.TrySetTile(
  orePatchPreparation.Transaction.Cells[0].X,
  orePatchPreparation.Transaction.Cells[0].Y,
  default);
if (orePatchPreparation.GroundY != 210 ||
    !orePatchPlacementSystem.TryAppendCommands(
      orePatchWorld.CreateSnapshot(replayRequest.Metadata),
      orePatchPreparation,
      protection,
      ref orePatchState,
      orePatchCommands) ||
    orePatchCommands.Count == 0)
{
  throw new InvalidOperationException(
    "Ore patch placement did not compose eligibility and a prepared transaction.");
}

Console.WriteLine("PASS: ore placement prepare and command phases reject conflicts atomically");

TreeDefinition ordinaryTree = new(
  "ordinary",
  trunkTileType: 3,
  leafTileType: 4,
  minimumHeight: 3,
  maximumHeight: 5,
  canopyRadius: 2);
TreePlacementComponent treePlacement = new(ordinaryTree.Id, 100, 100);
WorldGenerationStateComponent treeState = new(42);
List<TileChangeCommand> treeCommands = new();
if (!new TreePlacementSystem().TryAppendCommands(
      objectSnapshot,
      replayRequest,
      ordinaryTree,
      treePlacement,
      protection,
      ref treeState,
      treeCommands) ||
    treeCommands.Count == 0 ||
    treeCommands.Any(command => protection.IsProtected(command.X, command.Y)))
{
  throw new InvalidOperationException("Tree placement did not respect definition or protection.");
}

StructureDefinition house = new(
  "starter-house",
  width: 4,
  height: 3,
  tileType: 5,
  wallType: 1,
  allowReplaceExisting: false);
StructurePlacementSystem structureSystem = new();
if (!structureSystem.TryPrepare(
      objectSnapshot,
      house,
      originX: 20,
      originY: 20,
      protection,
      out StructurePlacementComponent placement,
      out string? structureFailure))
{
  throw new InvalidOperationException($"Starter structure did not prepare: {structureFailure}");
}

_ = objectWorld.TrySetTile(20, 20, new WorldTile(true, 1));
WorldGenerationStateComponent structureState = new(42);
List<TileChangeCommand> structureCommands = new();
if (structureSystem.AppendCommands(
      objectWorld.CreateSnapshot(replayRequest.Metadata),
      house,
      placement,
      protection,
      ref structureState,
      structureCommands) ||
    structureCommands.Count != 0 ||
    structureState.Stage >= WorldGenerationStage.Structure)
{
  throw new InvalidOperationException(
    "Structure commit did not reject a post-prepare footprint conflict atomically.");
}

_ = objectWorld.TrySetTile(20, 20, default);
if (!structureSystem.AppendCommands(
      objectSnapshot,
      house,
      placement,
      protection,
    ref structureState,
    structureCommands) ||
    structureCommands.Count != house.Width * house.Height ||
    !new TileChangeCommitSystem().TryCommit(objectWorld, structureCommands, out _))
{
  throw new InvalidOperationException("Starter structure did not commit transactionally.");
}

WorldGridSnapshot occupiedSnapshot = objectWorld.CreateSnapshot(replayRequest.Metadata);
if (structureSystem.TryPrepare(
      occupiedSnapshot,
      house,
      originX: 20,
      originY: 20,
      protection,
      out _,
      out _))
{
  throw new InvalidOperationException(
    "Structure conflict was not rejected before command creation.");
}

Console.WriteLine(
  "PASS: ore, tree, and transactional structure placement honor definitions and protection");

StructurePlacementTransactionSystem structureTransactionSystem = new();
WorldGrid transactionStructureWorld = new(replayWidth, replayHeight);
WorldGridSnapshot transactionStructureSnapshot =
  transactionStructureWorld.CreateSnapshot(replayRequest.Metadata);
if (!structureTransactionSystem.TryPrepare(
      transactionStructureSnapshot,
      house,
      originX: 40,
      originY: 40,
      protection,
      out StructurePlacementComponent transactionPlacement,
      out string? transactionFailure))
{
  throw new InvalidOperationException(
    $"Independent structure transaction did not prepare: {transactionFailure}");
}

_ = transactionStructureWorld.TrySetTile(
  transactionPlacement.OriginX,
  transactionPlacement.OriginY,
  new WorldTile(true, 1));
WorldGenerationStateComponent blockedTransactionState = new(45);
List<TileChangeCommand> blockedTransactionCommands = new();
if (structureTransactionSystem.TryAppendCommands(
      transactionStructureWorld.CreateSnapshot(replayRequest.Metadata),
      house,
      transactionPlacement,
      protection,
      ref blockedTransactionState,
      blockedTransactionCommands) ||
    blockedTransactionCommands.Count != 0 ||
    blockedTransactionState.Stage >= WorldGenerationStage.Structure)
{
  throw new InvalidOperationException(
    "Independent structure transaction did not reject a post-prepare conflict atomically.");
}

_ = transactionStructureWorld.TrySetTile(
  transactionPlacement.OriginX,
  transactionPlacement.OriginY,
  default);
WorldGenerationStateComponent transactionStructureState = new(45);
List<TileChangeCommand> transactionStructureCommands = new();
if (!structureTransactionSystem.TryAppendCommands(
      transactionStructureSnapshot,
      house,
      transactionPlacement,
      protection,
      ref transactionStructureState,
      transactionStructureCommands) ||
    transactionStructureCommands.Count != house.Width * house.Height * 2 ||
    !new TileChangeCommitSystem().TryCommit(
      transactionStructureWorld,
      transactionStructureCommands,
      out _)
    || transactionStructureWorld.GetTile(40, 40).WallType != house.WallType)
{
  throw new InvalidOperationException(
    "Independent structure transaction did not commit tile and wall commands.");
}

WorldGrid replayStructureWorld = new(replayWidth, replayHeight);
StructurePlacementComponent replayPlacement = structureTransactionSystem.TryPrepare(
  replayStructureWorld.CreateSnapshot(replayRequest.Metadata),
  house,
  originX: 40,
  originY: 40,
  protection,
  out StructurePlacementComponent replayPlacementResult,
  out string? replayStructureFailure)
  ? replayPlacementResult
  : throw new InvalidOperationException(
    $"Structure transaction replay did not prepare: {replayStructureFailure}");
WorldGenerationStateComponent replayStructureState = new(45);
List<TileChangeCommand> replayStructureCommands = new();
if (!structureTransactionSystem.TryAppendCommands(
      replayStructureWorld.CreateSnapshot(replayRequest.Metadata),
      house,
      replayPlacement,
      protection,
      ref replayStructureState,
      replayStructureCommands) ||
    !transactionStructureCommands.SequenceEqual(replayStructureCommands))
{
  throw new InvalidOperationException(
    "Independent structure transaction command replay was not deterministic.");
}

Console.WriteLine(
  "PASS: independent structure transaction commits tile and wall commands atomically");

WorldGrid liquidWorld = new(replayWidth, replayHeight);
WorldMetadata liquidMetadata = replayRequest.Metadata;
WorldGridSnapshot liquidSnapshot = liquidWorld.CreateSnapshot(liquidMetadata);
WorldBoundsComponent liquidBounds = new(replayWidth, replayHeight);
LiquidDefinition water = new("water", type: 0, maxAmount: byte.MaxValue);
LiquidDefinition lava = new("lava", type: 1, maxAmount: byte.MaxValue);
LiquidDefinition obsidian = new("obsidian", type: 2, maxAmount: byte.MaxValue);
LiquidSourceComponent liquidSource = new(50, 100, water.Type, byte.MaxValue, "worldgen");
WorldGenerationStateComponent liquidState = new(42);
List<LiquidWorkItemComponent> workItems = new();
new LiquidSourceSystem().AppendWorkItems(
  new[] { liquidSource },
  liquidBounds,
  ref liquidState,
  workItems);
if (workItems.Count != 1 || workItems[0].LiquidType != water.Type)
{
  throw new InvalidOperationException("Liquid sources did not become bounded work items.");
}

List<LiquidChangeCommand> firstLiquidCommands = new();
LiquidPropagationSystem propagationSystem = new();
LiquidPropagationResult firstPropagation = propagationSystem.TryAppendCommands(
  liquidSnapshot,
  new[] { water, lava, obsidian },
  new[] { new LiquidMergeComponent(water.Type, lava.Type, obsidian.Type) },
  workItems,
  budget: 4,
  ref liquidState,
  firstLiquidCommands);
if (!firstPropagation.Succeeded ||
    firstPropagation.ConsumedWorkItems > 4 ||
    firstPropagation.RemainingWorkItems < 0 ||
    firstLiquidCommands.Count > 4)
{
  throw new InvalidOperationException("Liquid propagation exceeded its deterministic budget.");
}

WorldGrid mergeWorld = new(replayWidth, replayHeight);
if (!mergeWorld.TrySetLiquid(liquidSource.X, liquidSource.Y, amount: 100, lava.Type))
{
  throw new InvalidOperationException("Liquid merge fixture could not seed lava.");
}
WorldGridSnapshot mergeSnapshot = mergeWorld.CreateSnapshot(liquidMetadata);
WorldGenerationStateComponent mergeState = new(42);
List<LiquidWorkItemComponent> mergeWorkItems = new();
new LiquidSourceSystem().AppendWorkItems(
  new[] { liquidSource },
  liquidBounds,
  ref mergeState,
  mergeWorkItems);
List<LiquidChangeCommand> mergeCommands = new();
LiquidPropagationResult mergeResult = propagationSystem.TryAppendCommands(
  mergeSnapshot,
  new[] { water, lava, obsidian },
  new[] { new LiquidMergeComponent(water.Type, lava.Type, obsidian.Type) },
  mergeWorkItems,
  budget: 1,
  ref mergeState,
  mergeCommands);
if (!mergeResult.Succeeded || mergeCommands.Count != 1 ||
    mergeCommands[0].Type != obsidian.Type)
{
  throw new InvalidOperationException("Liquid merge did not use the explicit merge rule.");
}

long liquidVersion = mergeWorld.GetSectionVersion(
  mergeWorld.GetSectionCoordinates(liquidSource.X, liquidSource.Y));
if (!new LiquidChangeCommitSystem().TryCommit(
      mergeWorld,
      mergeCommands,
      new[] { water, lava, obsidian },
      out LiquidChangeCommitResult liquidCommit) ||
    liquidCommit.AppliedCount != 1 ||
    mergeWorld.GetTile(liquidSource.X, liquidSource.Y).LiquidType != obsidian.Type ||
    mergeWorld.GetSectionVersion(
      mergeWorld.GetSectionCoordinates(liquidSource.X, liquidSource.Y)) <= liquidVersion)
{
  throw new InvalidOperationException(
    "Liquid commands did not commit with a section version change.");
}

if (new LiquidChangeCommitSystem().TryCommit(
      mergeWorld,
      new[] { new LiquidChangeCommand(99, 1, 1, byte.MaxValue, 9) },
      new[] { water },
      out LiquidChangeCommitResult invalidLiquidCommit) ||
    invalidLiquidCommit.FailureReason is null)
{
  throw new InvalidOperationException("Invalid liquid commands were not rejected.");
}

if (LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Water,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Lava) !=
      LiquidInteractionKind.LavaWater ||
    LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Honey,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Water) !=
      LiquidInteractionKind.HoneyWater ||
    LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Lava,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Honey) !=
      LiquidInteractionKind.HoneyLava ||
    LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Shimmer,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Water) !=
      LiquidInteractionKind.ShimmerWater ||
    LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Lava,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Shimmer) !=
      LiquidInteractionKind.ShimmerLava ||
    LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Honey,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Shimmer) !=
      LiquidInteractionKind.ShimmerHoney ||
    LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Water,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Water) !=
      LiquidInteractionKind.None ||
    LiquidInteractionClassifier.GetKind(
      (Terraria.Dome.Simulation.Liquid.Components.LiquidType)9,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Water) !=
      LiquidInteractionKind.None)
{
  throw new InvalidOperationException(
    "Liquid interaction classification did not preserve the legacy pair table.");
}

WorldGrid sessionWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent sessionState = new(44);
List<LiquidWorkItemComponent> sessionWorkItems = new();
new LiquidSourceSystem().AppendWorkItems(
  new[] { liquidSource },
  liquidBounds,
  ref sessionState,
  sessionWorkItems);
LiquidPropagationSession session = new(
  sessionWorld,
  liquidMetadata,
  new[] { water, lava, obsidian },
  new[] { new LiquidMergeComponent(water.Type, lava.Type, obsidian.Type) },
  sessionState,
  sessionWorkItems);
LiquidPropagationAdvanceResult firstSessionAdvance = session.Advance(budget: 1);
if (!firstSessionAdvance.Succeeded ||
    firstSessionAdvance.ConsumedWorkItems != 1 ||
    firstSessionAdvance.PendingWorkItems == 0 ||
    session.PendingWorkItemCount != firstSessionAdvance.PendingWorkItems)
{
  throw new InvalidOperationException(
    "Liquid propagation session did not retain bounded pending work.");
}

LiquidPropagationCheckpoint sessionCheckpoint = session.CreateCheckpoint();
LiquidPropagationAdvanceResult uninterruptedSessionAdvance = session.Advance(budget: 3);
LiquidPropagationSession restartedSession = LiquidPropagationSession.Restore(
  sessionCheckpoint,
  new[] { water, lava, obsidian },
  new[] { new LiquidMergeComponent(water.Type, lava.Type, obsidian.Type) });
LiquidPropagationAdvanceResult restartedSessionAdvance = restartedSession.Advance(budget: 3);
if (!uninterruptedSessionAdvance.Succeeded ||
    !restartedSessionAdvance.Succeeded ||
    uninterruptedSessionAdvance.ConsumedWorkItems != restartedSessionAdvance.ConsumedWorkItems ||
    uninterruptedSessionAdvance.PendingWorkItems != restartedSessionAdvance.PendingWorkItems ||
    CreateSnapshotFingerprint(session.CreateSnapshot()) !=
      CreateSnapshotFingerprint(restartedSession.CreateSnapshot()))
{
  throw new InvalidOperationException(
    "Liquid propagation checkpoint restart was not deterministic.");
}

WorldGrid deduplicatedSessionWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent deduplicatedSessionState = new(46);
List<LiquidWorkItemComponent> deduplicatedWorkItems = new();
new LiquidSourceSystem().AppendWorkItems(
  new[] { liquidSource },
  liquidBounds,
  ref deduplicatedSessionState,
  deduplicatedWorkItems);
LiquidPropagationSession deduplicatedSession = new(
  deduplicatedSessionWorld,
  liquidMetadata,
  new[] { water },
  Array.Empty<LiquidMergeComponent>(),
  deduplicatedSessionState,
  deduplicatedWorkItems);
_ = deduplicatedSession.Advance(budget: 1);
_ = deduplicatedSession.Advance(budget: 8);
if (deduplicatedSession.CreateSnapshot().GetTile(liquidSource.X, liquidSource.Y).LiquidAmount !=
    byte.MaxValue)
{
  throw new InvalidOperationException(
    "Liquid propagation session revisited a committed source across budget ticks.");
}

WorldGrid rollbackWorld = new(replayWidth, replayHeight);
_ = rollbackWorld.TrySetLiquid(liquidSource.X, liquidSource.Y, amount: 100, lava.Type);
WorldGenerationStateComponent rollbackState = new(45);
List<LiquidWorkItemComponent> rollbackWorkItems = new();
new LiquidSourceSystem().AppendWorkItems(
  new[] { liquidSource },
  liquidBounds,
  ref rollbackState,
  rollbackWorkItems);
LiquidPropagationSession rollbackSession = new(
  rollbackWorld,
  liquidMetadata,
  new[] { water, lava },
  Array.Empty<LiquidMergeComponent>(),
  rollbackState,
  rollbackWorkItems);
string rollbackFingerprint = CreateSnapshotFingerprint(rollbackSession.CreateSnapshot());
long rollbackSequence = rollbackSession.State.NextSequence;
LiquidPropagationAdvanceResult rollbackResult = rollbackSession.Advance(budget: 1);
if (rollbackResult.Succeeded ||
    rollbackSession.PendingWorkItemCount != rollbackWorkItems.Count ||
    rollbackSession.State.NextSequence != rollbackSequence ||
    CreateSnapshotFingerprint(rollbackSession.CreateSnapshot()) != rollbackFingerprint)
{
  throw new InvalidOperationException(
    "Liquid propagation failure did not restore its checkpoint state.");
}

Console.WriteLine("PASS: bounded liquid propagation, explicit merge, and versioned liquid commit");

WorldGrid frameWorld = new(replayWidth, replayHeight);
_ = frameWorld.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 1));
WorldGridSnapshot frameSnapshot = frameWorld.CreateSnapshot(replayRequest.Metadata);
List<TileChangeCommand> pendingFrameChanges = new()
{
  new TileChangeCommand(0, 11, 10, TileChangeKind.Place, 1)
};
WorldGenerationStateComponent frameState = new(42);
List<TileFrameCommand> frameCommands = new();
IReadOnlyList<TileFrameRequest> frameRequests = TileFrameEvaluationQuery.CreateRequests(
  frameSnapshot,
  pendingFrameChanges,
  TileFrameMutationKind.TileChange);
TileFrameEvaluationResult frameEvaluation = TileFrameEvaluationQuery.Evaluate(
  frameRequests.Single(request => request.X == 10 && request.Y == 10));
TileFrameEvaluationResult reverseOrderFrameEvaluation = TileFrameEvaluationQuery.Evaluate(
  new TileFrameRequest(
    10,
    10,
    TileFrameMutationKind.TileChange,
    frameSnapshot,
    new[]
    {
      new TileChangeCommand(1, 11, 10, TileChangeKind.Kill, 0),
      new TileChangeCommand(0, 11, 10, TileChangeKind.Place, 1)
    }));
TileFrameEvaluationResult forwardOrderFrameEvaluation = TileFrameEvaluationQuery.Evaluate(
  new TileFrameRequest(
    10,
    10,
    TileFrameMutationKind.TileChange,
    frameSnapshot,
    new[]
    {
      new TileChangeCommand(0, 11, 10, TileChangeKind.Place, 1),
      new TileChangeCommand(1, 11, 10, TileChangeKind.Kill, 0)
    }));
if (reverseOrderFrameEvaluation.IsSupported != forwardOrderFrameEvaluation.IsSupported ||
    reverseOrderFrameEvaluation.FrameX != forwardOrderFrameEvaluation.FrameX ||
    reverseOrderFrameEvaluation.FrameY != forwardOrderFrameEvaluation.FrameY ||
    reverseOrderFrameEvaluation.IsHalfBrick != forwardOrderFrameEvaluation.IsHalfBrick ||
    reverseOrderFrameEvaluation.Slope != forwardOrderFrameEvaluation.Slope ||
    reverseOrderFrameEvaluation.ShouldKill != forwardOrderFrameEvaluation.ShouldKill ||
    reverseOrderFrameEvaluation.Classification != forwardOrderFrameEvaluation.Classification ||
    !reverseOrderFrameEvaluation.AffectedCoordinates.SequenceEqual(
      forwardOrderFrameEvaluation.AffectedCoordinates))
{
  throw new InvalidOperationException(
    "Pending tile framing changed when mutation enumeration order changed.");
}
if (TileFrameClassificationQuery.Classify(new WorldTile(IsActive: true, Type: 1)) !=
      TileFrameClassificationKind.OrdinarySolid ||
    TileFrameClassificationQuery.Classify(new WorldTile(IsActive: true, Type: 520)) !=
      TileFrameClassificationKind.FoodPlatter ||
    TileFrameClassificationQuery.Classify(new WorldTile(IsActive: true, Type: 80)) !=
      TileFrameClassificationKind.Cactus ||
    TileFrameClassificationQuery.Classify(new WorldTile(IsActive: true, Type: 21)) !=
      TileFrameClassificationKind.MultiTileUnsupported ||
    TileFrameClassificationQuery.Classify(new WorldTile(IsActive: true, Type: 385)) !=
      TileFrameClassificationKind.CosmeticNoFrameChange ||
    TileFrameClassificationQuery.Classify(new WorldTile(IsActive: true, Type: 3)) !=
      TileFrameClassificationKind.FrameImportantUnsupported)
{
  throw new InvalidOperationException(
    "Tile frame classification did not preserve the migrated special-tile boundary.");
}

TileFrameEvaluationResult unsupportedSpecialFrame = TileFrameEvaluationQuery.Evaluate(
  new TileFrameRequest(
    10,
    10,
    TileFrameMutationKind.TileChange,
    frameSnapshot,
    new[] { new TileChangeCommand(0, 10, 10, TileChangeKind.UpdateTileType, 520) }));
if (unsupportedSpecialFrame.IsSupported)
{
  throw new InvalidOperationException(
    "Frame-important special tiles received generic frame evaluation.");
}

if (unsupportedSpecialFrame.Classification != TileFrameClassificationKind.FoodPlatter)
{
  throw new InvalidOperationException(
    "Tile frame evaluation did not preserve the special-tile classification.");
}

TileFrameEvaluationResult noOpCosmeticFrame = TileFrameEvaluationQuery.Evaluate(
  new TileFrameRequest(
    10,
    10,
    TileFrameMutationKind.TileChange,
    frameSnapshot,
    new[] { new TileChangeCommand(0, 10, 10, TileChangeKind.UpdateTileType, 385) }));
if (noOpCosmeticFrame.IsSupported ||
    noOpCosmeticFrame.Classification != TileFrameClassificationKind.CosmeticNoFrameChange)
{
  throw new InvalidOperationException(
    "Cosmetic no-op tile was incorrectly routed to generic framing.");
}

WorldGrid cosmeticNeighborWorld = new(replayWidth, replayHeight);
_ = cosmeticNeighborWorld.TrySetTile(30, 30, new WorldTile(IsActive: true, Type: 1));
_ = cosmeticNeighborWorld.TrySetTile(29, 30, new WorldTile(IsActive: true, Type: 5));
_ = cosmeticNeighborWorld.TrySetTile(31, 30, new WorldTile(IsActive: true, Type: 6, Slope: 2));
_ = cosmeticNeighborWorld.TrySetTile(30, 29, new WorldTile(IsActive: true, Type: 7));
TileCosmeticNeighborResult cosmeticNeighbors = TileCosmeticNeighborQuery.Evaluate(
  cosmeticNeighborWorld.CreateSnapshot(replayRequest.Metadata),
  30,
  30,
  new HashSet<ushort> { 5 });
if (cosmeticNeighbors.CanonicalTileType != 1 || cosmeticNeighbors.Left != 1 ||
    cosmeticNeighbors.Right != -1 ||
    cosmeticNeighbors.Up != 7 || cosmeticNeighbors.Down != -1)
{
  throw new InvalidOperationException(
    "Cosmetic neighbor projection did not preserve canonical types and slope boundaries.");
}

_ = cosmeticNeighborWorld.TrySetTile(30, 30, new WorldTile(IsActive: true, Type: 668));
TileCosmeticNeighborResult dirtAliasNeighbors = TileCosmeticNeighborQuery.Evaluate(
  cosmeticNeighborWorld.CreateSnapshot(replayRequest.Metadata),
  30,
  30,
  new HashSet<ushort>());
if (dirtAliasNeighbors.CanonicalTileType != 0)
{
  throw new InvalidOperationException("Tile 668 cosmetic alias did not normalize to dirt.");
}

_ = cosmeticNeighborWorld.TrySetTile(30, 30, new WorldTile(IsActive: true, Type: 697));
TileCosmeticNeighborResult snowAliasNeighbors = TileCosmeticNeighborQuery.Evaluate(
  cosmeticNeighborWorld.CreateSnapshot(replayRequest.Metadata),
  30,
  30,
  new HashSet<ushort>());
if (snowAliasNeighbors.CanonicalTileType != 51)
{
  throw new InvalidOperationException("Tile 697 cosmetic alias did not normalize to tile 51.");
}

_ = cosmeticNeighborWorld.TrySetTile(30, 30, new WorldTile(IsActive: true, Type: 5));
TileCosmeticNeighborResult stoneSourceNeighbors = TileCosmeticNeighborQuery.Evaluate(
  cosmeticNeighborWorld.CreateSnapshot(replayRequest.Metadata),
  30,
  30,
  new HashSet<ushort> { 5 });
if (stoneSourceNeighbors.CanonicalTileType != 1)
{
  throw new InvalidOperationException("Cosmetic stone source type did not normalize to tile 1.");
}

IReadOnlySet<ushort> cosmeticStoneTypes = new HashSet<ushort> { 5 };
TileCosmeticNeighborResult pendingCosmeticNeighbors = TileCosmeticNeighborQuery.Evaluate(
  cosmeticNeighborWorld.CreateSnapshot(replayRequest.Metadata),
  30,
  30,
  new[] { new TileChangeCommand(0, 31, 30, TileChangeKind.Place, 5) },
  cosmeticStoneTypes);
if (pendingCosmeticNeighbors.Right != 1)
{
  throw new InvalidOperationException(
    "Cosmetic neighbor projection did not apply pending tile mutations.");
}

TileFrameEvaluationResult wallOverlayFrame = TileFrameEvaluationQuery.Evaluate(
  new TileFrameRequest(
    10,
    10,
    TileFrameMutationKind.TileChange,
    frameSnapshot,
    new[] { new TileChangeCommand(0, 10, 10, TileChangeKind.SetWall, 0, WallType: 7) }));
if (wallOverlayFrame.Classification != TileFrameClassificationKind.OrdinarySolid)
{
  throw new InvalidOperationException("SetWall pending overlay changed tile classification unexpectedly.");
}

TileFrameEvaluationResult inactiveOverlayFrame = TileFrameEvaluationQuery.Evaluate(
  new TileFrameRequest(
    10,
    10,
    TileFrameMutationKind.TileChange,
    frameSnapshot,
    new[] { new TileChangeCommand(0, 10, 10, TileChangeKind.SetInactive, 0, IsInactive: true) }));
if (inactiveOverlayFrame.Classification != TileFrameClassificationKind.InactiveOrUnsupported)
{
  throw new InvalidOperationException(
    "SetInactive pending overlay changed tile classification unexpectedly.");
}

WorldGrid frameImportantWorld = new(replayWidth, replayHeight);
_ = frameImportantWorld.TrySetTile(90, 90, new WorldTile(IsActive: true, Type: 136));
_ = frameImportantWorld.TrySetTile(90, 91, new WorldTile(IsActive: true, Type: 1));
TileFrameImportant136Result frameImportantDown = TileFrameImportant136Query.Evaluate(
  frameImportantWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  90,
  90,
  new HashSet<ushort>(),
  new HashSet<ushort>());
if (!frameImportantDown.IsSupported || frameImportantDown.FrameX != 0 ||
    frameImportantDown.ShouldKill)
{
  throw new InvalidOperationException("Tile 136 down-support framing diverged from legacy rules.");
}

_ = frameImportantWorld.TrySetTile(90, 91, default);
_ = frameImportantWorld.TrySetTile(89, 90, new WorldTile(IsActive: true, Type: 1));
TileFrameImportant136Result frameImportantLeft = TileFrameImportant136Query.Evaluate(
  frameImportantWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  90,
  90,
  new HashSet<ushort>(),
  new HashSet<ushort>());
if (!frameImportantLeft.IsSupported || frameImportantLeft.FrameX != 18)
{
  throw new InvalidOperationException("Tile 136 left-support framing diverged from legacy rules.");
}

_ = frameImportantWorld.TrySetTile(89, 90, default);
TileFrameImportant136Result frameImportantKilled = TileFrameImportant136Query.Evaluate(
  frameImportantWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  90,
  90,
  new HashSet<ushort>(),
  new HashSet<ushort>());
if (!frameImportantKilled.ShouldKill || frameImportantKilled.IsSupported)
{
  throw new InvalidOperationException("Tile 136 unsupported framing did not preserve kill intent.");
}

WorldGrid mossFrameWorld = new(replayWidth, replayHeight);
_ = mossFrameWorld.TrySetTile(100, 100, new WorldTile(IsActive: true, Type: 184));
_ = mossFrameWorld.TrySetTile(100, 101, new WorldTile(IsActive: true, Type: 179));
TileFrameImportant184Result mossFrameDown = TileFrameImportant184Query.Evaluate(
  mossFrameWorld.CreateSnapshot(replayRequest.Metadata),
  100,
  100,
  currentFrameY: -1,
  randomState: new GenerationRandomState(11));
if (!mossFrameDown.IsSupported || mossFrameDown.FrameX != 0 ||
    mossFrameDown.FrameY is < 0 or > 36 || mossFrameDown.ShouldKill)
{
  throw new InvalidOperationException("Tile 184 down moss framing diverged from legacy rules.");
}

_ = mossFrameWorld.TrySetTile(100, 101, default);
_ = mossFrameWorld.TrySetTile(99, 100, new WorldTile(IsActive: true, Type: 180));
TileFrameImportant184Result mossFrameLeft = TileFrameImportant184Query.Evaluate(
  mossFrameWorld.CreateSnapshot(replayRequest.Metadata),
  100,
  100,
  currentFrameY: 120,
  randomState: new GenerationRandomState(11));
if (!mossFrameLeft.IsSupported || mossFrameLeft.FrameX != 22 ||
    mossFrameLeft.FrameY != 120)
{
  throw new InvalidOperationException("Tile 184 left moss framing did not preserve frame Y.");
}

_ = mossFrameWorld.TrySetTile(99, 100, default);
TileFrameImportant184Result mossFrameKilled = TileFrameImportant184Query.Evaluate(
  mossFrameWorld.CreateSnapshot(replayRequest.Metadata),
  100,
  100,
  currentFrameY: -1,
  randomState: new GenerationRandomState(11));
if (!mossFrameKilled.ShouldKill || mossFrameKilled.IsSupported)
{
  throw new InvalidOperationException("Tile 184 unsupported framing did not preserve kill intent.");
}

WorldGrid sandFrameWorld = new(replayWidth, replayHeight);
_ = sandFrameWorld.TrySetTile(110, 110, new WorldTile(IsActive: true, Type: 529));
_ = sandFrameWorld.TrySetTile(110, 111, new WorldTile(IsActive: true, Type: 53));
TileFrameImportant529Result sandFrameSupported = TileFrameImportant529Query.Evaluate(
  sandFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  110,
  110,
  new HashSet<ushort> { 53 });
if (!sandFrameSupported.IsSupported || sandFrameSupported.ShouldKill)
{
  throw new InvalidOperationException("Tile 529 conversion-sand support diverged from legacy rules.");
}

_ = sandFrameWorld.TrySetTile(110, 111, new WorldTile(IsActive: true, Type: 1));
TileFrameImportant529Result sandFrameKilled = TileFrameImportant529Query.Evaluate(
  sandFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  110,
  110,
  new HashSet<ushort> { 53 });
if (!sandFrameKilled.ShouldKill || sandFrameKilled.IsSupported)
{
  throw new InvalidOperationException("Tile 529 non-sand support did not preserve kill intent.");
}

WorldGrid pileFrameWorld = new(replayWidth, replayHeight);
_ = pileFrameWorld.TrySetTile(120, 120, new WorldTile(IsActive: true, Type: 324));
_ = pileFrameWorld.TrySetTile(120, 121, new WorldTile(IsActive: true, Type: 1));
TileFrameImportant324Result pileFrameSupported = TileFrameImportant324Query.Evaluate(
  pileFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  120,
  120,
  new HashSet<ushort> { 666 });
if (!pileFrameSupported.IsSupported || pileFrameSupported.ShouldKill)
{
  throw new InvalidOperationException("Tile 324 support framing diverged from legacy rules.");
}

_ = pileFrameWorld.TrySetTile(120, 121, new WorldTile(IsActive: true, Type: 666));
TileFrameImportant324Result pileFrameKilled = TileFrameImportant324Query.Evaluate(
  pileFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  120,
  120,
  new HashSet<ushort> { 666 });
if (!pileFrameKilled.ShouldKill || pileFrameKilled.IsSupported)
{
  throw new InvalidOperationException("Tile 324 boulder support did not preserve kill intent.");
}

WorldGrid threeByOneWorld = new(replayWidth, replayHeight);
for (int offset = 0; offset < 3; offset++)
{
  _ = threeByOneWorld.TrySetTile(
    130 + offset,
    130,
    new WorldTile(
      IsActive: true,
      Type: 235,
      FrameX: checked((short)(offset * 18)),
      FrameY: 0));
  _ = threeByOneWorld.TrySetTile(130 + offset, 131, new WorldTile(IsActive: true, Type: 1));
}

Tile3x1ValidationResult threeByOneValid = Tile3x1ValidationQuery.Evaluate(
  threeByOneWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  131,
  130,
  235);
if (!threeByOneValid.IsValid || threeByOneValid.RequiresBreakabilityCheck ||
    threeByOneValid.OriginX != 130 || threeByOneValid.OriginY != 130)
{
  throw new InvalidOperationException("3x1 footprint validation did not preserve legacy origin rules.");
}

_ = threeByOneWorld.TrySetTile(
  131,
  130,
  new WorldTile(IsActive: true, Type: 235, FrameX: 18, FrameY: 18));
Tile3x1ValidationResult threeByOneInvalid = Tile3x1ValidationQuery.Evaluate(
  threeByOneWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  131,
  130,
  235);
if (threeByOneInvalid.IsValid || !threeByOneInvalid.RequiresBreakabilityCheck)
{
  throw new InvalidOperationException("3x1 invalid frame did not preserve deferred breakability.");
}

WorldGrid pileValidationWorld = new(replayWidth, replayHeight);
_ = pileValidationWorld.TrySetTile(
  140,
  140,
  new WorldTile(IsActive: true, Type: 185, FrameX: 36 * 18));
_ = pileValidationWorld.TrySetTile(140, 141, new WorldTile(IsActive: true, Type: 147));
TilePileValidationResult snowPile = TilePileValidationQuery.Evaluate(
  pileValidationWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  140,
  140,
  new HashSet<ushort> { 147 },
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>());
if (!snowPile.IsSupported || snowPile.ShouldKill || snowPile.RequiresTwoByOneCheck)
{
  throw new InvalidOperationException("Snow pile support validation diverged from legacy rules.");
}

_ = pileValidationWorld.TrySetTile(140, 141, new WorldTile(IsActive: true, Type: 1));
TilePileValidationResult invalidPile = TilePileValidationQuery.Evaluate(
  pileValidationWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  140,
  140,
  new HashSet<ushort> { 147 },
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>());
if (!invalidPile.ShouldKill || invalidPile.IsSupported)
{
  throw new InvalidOperationException("Invalid snow pile support did not preserve kill intent.");
}

_ = pileValidationWorld.TrySetTile(
  140,
  140,
  new WorldTile(IsActive: true, Type: 185, FrameX: 0, FrameY: 18));
TilePileValidationResult deferredPile = TilePileValidationQuery.Evaluate(
  pileValidationWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  140,
  140,
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>());
if (!deferredPile.RequiresTwoByOneCheck || deferredPile.ShouldKill)
{
  throw new InvalidOperationException("2x1 pile branch did not remain explicitly deferred.");
}

WorldGrid orbValidationWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 2; offsetX++)
{
  for (int offsetY = 0; offsetY < 2; offsetY++)
  {
    _ = orbValidationWorld.TrySetTile(
      170 + offsetX,
      170 + offsetY,
      new WorldTile(
        IsActive: true,
        Type: 12,
        FrameX: checked((short)(offsetX == 0 ? 0 : 36)),
        FrameY: checked((short)(offsetY == 0 ? 0 : 18))));
  }

  _ = orbValidationWorld.TrySetTile(170 + offsetX, 172, new WorldTile(IsActive: true, Type: 1));
}

TileOrbValidationResult orbValid = TileOrbValidationQuery.Evaluate(
  orbValidationWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  171,
  171,
  12);
if (!orbValid.IsValid || orbValid.ShouldKill || orbValid.OriginX != 170 || orbValid.OriginY != 170)
{
  throw new InvalidOperationException("2x2 orb footprint validation diverged from legacy rules.");
}

_ = orbValidationWorld.TrySetTile(171, 170, default);
TileOrbValidationResult orbInvalid = TileOrbValidationQuery.Evaluate(
  orbValidationWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  171,
  171,
  12);
if (orbInvalid.IsValid || !orbInvalid.ShouldKill)
{
  throw new InvalidOperationException("Invalid 2x2 orb footprint did not preserve kill intent.");
}

WorldGrid styledObjectWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 2; offsetX++)
{
  for (int offsetY = 0; offsetY < 2; offsetY++)
  {
    _ = styledObjectWorld.TrySetTile(
      180 + offsetX,
      180 + offsetY,
      new WorldTile(
        IsActive: true,
        Type: 254,
        FrameX: checked((short)(offsetX * 18 + 72)),
        FrameY: checked((short)(offsetY * 18))));
  }

  _ = styledObjectWorld.TrySetTile(180 + offsetX, 182, new WorldTile(IsActive: true, Type: 2));
}

Tile2x2StyleValidationResult styledObjectValid = Tile2x2StyleValidationQuery.Evaluate(
  styledObjectWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  181,
  181,
  254);
if (!styledObjectValid.IsValid || styledObjectValid.ShouldKill ||
    styledObjectValid.OriginX != 180 || styledObjectValid.OriginY != 180 ||
    styledObjectValid.StyleBand != 2)
{
  throw new InvalidOperationException(
    "2x2 style footprint validation diverged from legacy rules.");
}

_ = styledObjectWorld.TrySetTile(181, 182, new WorldTile(IsActive: true, Type: 1));
Tile2x2StyleValidationResult styledObjectInvalid = Tile2x2StyleValidationQuery.Evaluate(
  styledObjectWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  181,
  181,
  254);
if (styledObjectInvalid.IsValid || !styledObjectInvalid.ShouldKill)
{
  throw new InvalidOperationException(
    "Invalid 2x2 style support did not preserve kill intent.");
}

WorldGrid twoByOneWorld = new(replayWidth, replayHeight);
_ = twoByOneWorld.TrySetTile(
  190,
  190,
  new WorldTile(IsActive: true, Type: 16, FrameX: 36, FrameY: 18));
_ = twoByOneWorld.TrySetTile(
  191,
  190,
  new WorldTile(IsActive: true, Type: 16, FrameX: 54, FrameY: 18));
_ = twoByOneWorld.TrySetTile(190, 191, new WorldTile(IsActive: true, Type: 1));
_ = twoByOneWorld.TrySetTile(191, 191, new WorldTile(IsActive: true, Type: 1));
Tile2x1ValidationResult twoByOneValid = Tile2x1ValidationQuery.Evaluate(
  twoByOneWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  191,
  190,
  16,
  new HashSet<ushort>());
if (!twoByOneValid.IsValid || twoByOneValid.ShouldKill ||
    twoByOneValid.OriginX != 190 || twoByOneValid.OriginY != 190 ||
    twoByOneValid.StyleBand != 1)
{
  throw new InvalidOperationException("2x1 footprint validation diverged from legacy rules.");
}

Tile2x1ValidationResult twoByOnePile = Tile2x1ValidationQuery.Evaluate(
  twoByOneWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  191,
  190,
  185,
  new HashSet<ushort>());
if (twoByOnePile.IsValid || twoByOnePile.ShouldKill ||
    !twoByOnePile.RequiresPileValidation)
{
  throw new InvalidOperationException(
    "2x1 pile validation did not preserve the deferred legacy branch.");
}

WorldGrid dyeFrameWorld = new(replayWidth, replayHeight);
_ = dyeFrameWorld.TrySetTile(150, 150, new WorldTile(IsActive: true, Type: 227));
_ = dyeFrameWorld.TrySetTile(150, 151, new WorldTile(IsActive: true, Type: 1));
TileDyeFrameResult dyeFrameDefault = TileDyeFrameQuery.Evaluate(
  dyeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  150,
  150,
  frameX: 0);
if (!dyeFrameDefault.IsSupported || dyeFrameDefault.ShouldKill)
{
  throw new InvalidOperationException("Default dye frame support diverged from legacy rules.");
}

_ = dyeFrameWorld.TrySetTile(150, 151, new WorldTile(IsActive: true, Type: 80));
TileDyeFrameResult dyeFrameCactus = TileDyeFrameQuery.Evaluate(
  dyeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  150,
  150,
  frameX: 204);
if (!dyeFrameCactus.IsSupported || dyeFrameCactus.ShouldKill)
{
  throw new InvalidOperationException("Cactus dye frame support diverged from legacy rules.");
}

_ = dyeFrameWorld.TrySetTile(150, 151, new WorldTile(IsActive: true, Type: 1));
TileDyeFrameResult dyeFrameKilled = TileDyeFrameQuery.Evaluate(
  dyeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  150,
  150,
  frameX: 204);
if (!dyeFrameKilled.ShouldKill || dyeFrameKilled.IsSupported)
{
  throw new InvalidOperationException("Invalid cactus dye frame did not preserve kill intent.");
}

WorldGrid rockGolemWorld = new(replayWidth, replayHeight);
_ = rockGolemWorld.TrySetTile(160, 160, new WorldTile(IsActive: true, Type: 579));
_ = rockGolemWorld.TrySetTile(160, 161, new WorldTile(IsActive: true, Type: 1));
RockGolemHeadFrameResult rockGolemSupported = RockGolemHeadFrameQuery.Evaluate(
  rockGolemWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  160,
  160);
if (!rockGolemSupported.IsSupported || rockGolemSupported.ShouldKill)
{
  throw new InvalidOperationException("Rock Golem head support diverged from legacy rules.");
}

_ = rockGolemWorld.TrySetTile(160, 161, default);
RockGolemHeadFrameResult rockGolemKilled = RockGolemHeadFrameQuery.Evaluate(
  rockGolemWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  160,
  160);
if (!rockGolemKilled.ShouldKill || rockGolemKilled.IsSupported)
{
  throw new InvalidOperationException("Unsupported Rock Golem head did not preserve kill intent.");
}

if (!frameEvaluation.IsSupported || frameEvaluation.FrameX != 18 ||
    frameEvaluation.FrameY != 0 || frameEvaluation.AffectedCoordinates.Count != 5)
{
  throw new InvalidOperationException(
    "Tile frame evaluation did not use the pending mutation overlay.");
}

if (!new TileFrameSystem().TryAppendCommands(
      frameSnapshot,
      pendingFrameChanges,
      ref frameState,
      frameCommands) ||
    frameCommands.Count == 0 ||
    frameSnapshot.GetTile(10, 10) != new WorldTile(IsActive: true, Type: 1) ||
    frameWorld.GetTile(11, 10) != default)
{
  throw new InvalidOperationException(
    "Tile framing changed state instead of producing frame commands.");
}

long frameSectionVersion = frameWorld.GetSectionVersion(
  frameWorld.GetSectionCoordinates(10, 10));
if (!new TileChangeCommitSystem().TryCommit(
      frameWorld,
      frameCommands,
      out TileFrameCommitResult frameCommit) ||
    frameCommit.AppliedCount != frameCommands.Count ||
    frameWorld.GetTile(10, 10).FrameX != 18 ||
    frameWorld.GetSectionVersion(frameWorld.GetSectionCoordinates(10, 10)) <=
      frameSectionVersion)
{
  throw new InvalidOperationException(
    "Tile frame commands did not commit through the tile change boundary.");
}

WorldGrid boundaryFrameWorld = new(replayWidth, replayHeight);
_ = boundaryFrameWorld.TrySetTile(0, 0, new WorldTile(IsActive: true, Type: 1));
List<TileFrameCommand> boundaryFrameCommands = new();
WorldGenerationStateComponent boundaryFrameState = new(43);
if (!new TileFrameSystem().TryAppendCommands(
      boundaryFrameWorld.CreateSnapshot(replayRequest.Metadata),
      new[] { new TileChangeCommand(0, 0, 1, TileChangeKind.Place, 1) },
      ref boundaryFrameState,
      boundaryFrameCommands) ||
    boundaryFrameCommands.Any(command => !boundaryFrameWorld.Contains(command.X, command.Y)) ||
    !new TileChangeCommitSystem().TryCommit(
      boundaryFrameWorld,
      boundaryFrameCommands,
      out TileFrameCommitResult boundaryFrameCommit) ||
    boundaryFrameCommit.AppliedCount != boundaryFrameCommands.Count)
{
  throw new InvalidOperationException(
    "Tile framing did not discard out-of-world neighbor commands.");
}

WorldGrid fourByTwoWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 4; offsetX++)
{
  for (int offsetY = 0; offsetY < 2; offsetY++)
  {
    _ = fourByTwoWorld.TrySetTile(
      30 + offsetX,
      30 + offsetY,
      new WorldTile(
        IsActive: true,
        Type: 79,
        FrameX: checked((short)(offsetX * 18)),
        FrameY: checked((short)(offsetY * 18))));
  }

  _ = fourByTwoWorld.TrySetTile(30 + offsetX, 32, new WorldTile(IsActive: true, Type: 1));
}

Tile4x2ValidationResult fourByTwoValid = Tile4x2ValidationQuery.Evaluate(
  fourByTwoWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  30,
  30,
  79);
if (!fourByTwoValid.IsValid || fourByTwoValid.ShouldKill ||
    fourByTwoValid.OriginX != 30 || fourByTwoValid.OriginY != 30)
{
  throw new InvalidOperationException("Valid 4x2 tile footprint was rejected.");
}

_ = fourByTwoWorld.TrySetTile(32, 32, default);
Tile4x2ValidationResult fourByTwoInvalid = Tile4x2ValidationQuery.Evaluate(
  fourByTwoWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  30,
  30,
  79);
if (fourByTwoInvalid.IsValid || !fourByTwoInvalid.ShouldKill)
{
  throw new InvalidOperationException("Invalid 4x2 tile support did not preserve kill intent.");
}

WorldGrid threeByFourWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 3; offsetX++)
{
  for (int offsetY = 0; offsetY < 4; offsetY++)
  {
    _ = threeByFourWorld.TrySetTile(
      40 + offsetX,
      30 + offsetY,
      new WorldTile(
        IsActive: true,
        Type: 101,
        FrameX: checked((short)(offsetX * 18)),
        FrameY: checked((short)(offsetY * 18))));
  }

  _ = threeByFourWorld.TrySetTile(40 + offsetX, 34, new WorldTile(IsActive: true, Type: 1));
}

Tile3x4ValidationResult threeByFourValid = Tile3x4ValidationQuery.Evaluate(
  threeByFourWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  40,
  30,
  101);
if (!threeByFourValid.IsValid || threeByFourValid.ShouldKill ||
    threeByFourValid.OriginX != 40 || threeByFourValid.OriginY != 30)
{
  throw new InvalidOperationException("Valid 3x4 tile footprint was rejected.");
}

_ = threeByFourWorld.TrySetTile(41, 34, default);
Tile3x4ValidationResult threeByFourInvalid = Tile3x4ValidationQuery.Evaluate(
  threeByFourWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  40,
  30,
  101);
if (threeByFourInvalid.IsValid || !threeByFourInvalid.ShouldKill)
{
  throw new InvalidOperationException("Invalid 3x4 tile support did not preserve kill intent.");
}

WorldGenerationStateComponent validationState = new(42);
_ = validationState.TryAdvance(WorldGenerationStage.Committed);
WorldGenerationValidationResult validationResult = new WorldGenerationValidationSystem().Validate(
  frameSnapshot,
  ref validationState,
  Array.Empty<LiquidWorkItemComponent>(),
  new[]
  {
    new StructurePlacementComponent(
      "starter-house",
      20,
      20,
      new StructureFootprintComponent(4, 3))
  });
if (!validationResult.Succeeded || !validationState.IsComplete)
{
  throw new InvalidOperationException("Valid world generation state did not reach validation.");
}

WorldGenerationStateComponent invalidValidationState = new(42);
WorldGenerationValidationResult invalidValidation = new WorldGenerationValidationSystem().Validate(
  frameSnapshot,
  ref invalidValidationState,
  new[] { new LiquidWorkItemComponent(-1, 0, 0, 1, 0) },
  Array.Empty<StructurePlacementComponent>());
if (invalidValidation.Succeeded || invalidValidation.FailureReason is null)
{
  throw new InvalidOperationException("Invalid world generation work was not rejected.");
}

Console.WriteLine(
  "PASS: frame commands, immutable snapshots, and generation validation boundaries");

WorldGrid runtimeWorld = new(4200, 1200);
DefaultWorldEnvironmentConvergence convergence =
  DefaultWorldEnvironmentConvergence.Create(runtimeWorld);
WorldEnvironmentTickSystem environmentTickSystem = new();
WorldRuleSnapshot ruleSnapshot = new(0, 0, true);
IReadOnlyList<WorldEnvironmentChange> runtimeChanges = Array.Empty<WorldEnvironmentChange>();
for (int tick = 0; tick < 5; tick++)
{
  runtimeChanges = environmentTickSystem.Advance(runtimeWorld, ruleSnapshot, convergence);
}

if (runtimeChanges.Count == 0 || runtimeWorld.GetTile(100, 400).LiquidAmount == 0)
{
  throw new InvalidOperationException(
    "Runtime environment systems did not continue from a world snapshot.");
}

Console.WriteLine("PASS: runtime environment tick consumes rule snapshot and bounded changes");

static void VerifyTorchDefinitions()
{
  if (TorchDefinitionRegistry.Definitions.Count != 24)
  {
    throw new InvalidOperationException("Torch definition count does not match the source registry.");
  }

  if (!TorchDefinitionRegistry.TryGet(0, out TorchDefinition ordinary) ||
      ordinary.DustType != 6 || !ordinary.IsBiomeTorch)
  {
    throw new InvalidOperationException("The ordinary torch definition is incorrect.");
  }

  if (!TorchDefinitionRegistry.TryGet(23, out TorchDefinition shimmer) ||
      shimmer.DustType != 310 || !shimmer.IsBiomeTorch)
  {
    throw new InvalidOperationException("The shimmer torch definition is incorrect.");
  }

  if (TorchDefinitionRegistry.TryGet(-1, out _) ||
      TorchDefinitionRegistry.TryGet(24, out _))
  {
    throw new InvalidOperationException("Out-of-range torch IDs were accepted.");
  }

  Console.WriteLine("PASS: fixed TorchID definitions preserve source dust and biome facts");
}

static void VerifyTileEntityDefinitions()
{
  string[] expectedNames =
  [
    "TrainingDummy",
    "ItemFrame",
    "LogicSensor",
    "DisplayDoll",
    "WeaponsRack",
    "HatRack",
    "FoodPlatter",
    "TeleportationPylon",
    "DeadCellsDisplayJar",
    "KiteAnchor",
    "CritterAnchor"
  ];
  if (TileEntityDefinitionRegistry.Definitions.Count != expectedNames.Length)
  {
    throw new InvalidOperationException("Tile-entity definition count does not match the source.");
  }

  for (byte type = 0; type < expectedNames.Length; type++)
  {
    if (!TileEntityDefinitionRegistry.TryGet(type, out TileEntityDefinition definition) ||
        definition.Type != type || definition.Name != expectedNames[type])
    {
      throw new InvalidOperationException($"Tile-entity type {type} does not match source order.");
    }
  }

  if (TileEntityDefinitionRegistry.TryGet(11, out _))
  {
    throw new InvalidOperationException("An unregistered tile-entity type was accepted.");
  }

  Console.WriteLine("PASS: TileEntity.InitializeAll definitions preserve source registration order");
}

static MethodInventory CreateMethodInventory(MethodDeclarationSyntax method)
{
  string body = method.ToFullString();
  int line = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
  SourceMapping? mapping = CreateMethodMapping(method.Identifier.ValueText, line);
  return new MethodInventory(
    method.Identifier.ValueText,
    method.Modifiers.Any(SyntaxKind.PublicKeyword)
      ? "Public"
      : method.Modifiers.Any(SyntaxKind.InternalKeyword) ? "Internal" : "Private",
    line,
    Classify(body),
    mapping?.Status ?? "Unmapped",
    CreateReferences(body),
    mapping);
}

static SourceMapping? CreateMethodMapping(string name, int line)
{
  if (name == "IsItATrap" && line == 22015)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileWiringQuery.cs:IsItATrap"
      },
      "Classifies active actuated and mechanism tiles with explicit wiring definitions.",
      new[]
      {
        "Legacy TileID.Sets wiring ownership is supplied by the caller."
      },
      "Terraria.Dome.WorldGeneration.Verification tile wiring predicate checks.");
  }

  if (name == "IsItATrigger" && line == 22034)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileWiringQuery.cs:IsItATrigger"
      },
      "Classifies explicit wiring triggers and legacy pressure-plate frame rules.",
      new[]
      {
        "Legacy Minecart.IsPressurePlate behavior is supplied as explicit input."
      },
      "Terraria.Dome.WorldGeneration.Verification tile wiring predicate checks.");
  }

  if (name == "IsDungeonPlatformOrShelf" && line == 10533)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/DungeonPlatformQuery.cs:" +
        "IsPlatformOrShelf"
      },
      "Classifies dungeon platform and shelf frame columns from an immutable tile value.",
      new[]
      {
        "Legacy Tile reference ownership is represented by WorldTile value input."
      },
      "Terraria.Dome.WorldGeneration.Verification dungeon platform frame checks.");
  }

  if (name == "IsSurfaceForAtmospherics" && line == 10033)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/AtmosphericSurfaceQuery.cs:" +
        "IsSurfaceForAtmospherics"
      },
      "Classifies normal and remix atmospheric surface heights from an explicit profile.",
      new[]
      {
        "Legacy Main remixWorld, worldSurface, rockLayer and maxTilesY fields become " +
        "explicit profile input."
      },
      "Terraria.Dome.WorldGeneration.Verification atmospheric surface checks.");
  }

  if (name == "CanGeneratePressurePlateAt" && line == 10072)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PressurePlatePlacementQuery.cs:" +
        "CanGenerateAt"
      },
      "Checks bounded placement support, boulder classification, and forbidden-wall rules.",
      new[]
      {
        "Legacy TileID.Sets.Boulders ownership is supplied as an explicit definition input."
      },
      "Terraria.Dome.WorldGeneration.Verification pressure plate placement checks.");
  }

  if (name == "IsBelowANonHammeredPlatform" && line == 31812)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PlatformSupportQuery.cs:" +
        "IsBelowANonHammeredPlatform"
      },
      "Classifies active platform support without half-brick or slope state.",
      new[]
      {
        "Legacy TileID.Sets.Platforms ownership is supplied as an explicit collection."
      },
      "Terraria.Dome.WorldGeneration.Verification non-hammered platform checks.");
  }

  if (name == "GetItemDrop_Candles" && line == 32845)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/CandleItemDropQuery.cs:ToItem"
      },
      "Maps candle tile styles to item identifiers using the legacy deterministic switch.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification candle item-drop mapping checks.");
  }

  if (name == "GetItemDrop_PicnicTables" && line == 33374)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PicnicTableItemDropQuery.cs:ToItem"
      },
      "Maps picnic-table style one to its item identifier and all other styles to default.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification picnic-table item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Bottles" && line == 34364)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/BottleItemDropQuery.cs:ToItem"
      },
      "Maps bottle tile styles to item identifiers using the legacy deterministic switch.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification bottle item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Benches" && line == 33296)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/BenchItemDropQuery.cs:ToItem"
      },
      "Maps bench tile styles to item identifiers using the legacy deterministic switch.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification bench item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Clocks" && line == 33214)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/ClockItemDropQuery.cs:ToItem"
      },
      "Maps clock tile styles to item identifiers using legacy ranges and switch branches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification clock item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Beds" && line == 33028)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/BedItemDropQuery.cs:ToItem"
      },
      "Maps bed tile styles to item identifiers using legacy ranges and switch branches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification bed item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Candelabras" && line == 33385)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/CandelabraItemDropQuery.cs:ToItem"
      },
      "Maps candelabra tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification candelabra item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Bookcases" && line == 33567)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/BookcaseItemDropQuery.cs:ToItem"
      },
      "Maps bookcase tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification bookcase item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Chandeliers" && line == 33769)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/ChandelierItemDropQuery.cs:ToItem"
      },
      "Maps chandelier tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification chandelier item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Lanterns" && line == 33968)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/LanternItemDropQuery.cs:ToItem"
      },
      "Maps lantern tile styles to item identifiers, including legacy negative-style behavior.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification lantern item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Lamps" && line == 34184)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/LampItemDropQuery.cs:ToItem"
      },
      "Maps lamp tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification lamp item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Pianos" && line == 34399)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PianoItemDropQuery.cs:ToItem"
      },
      "Maps piano tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification piano item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Sinks" && line == 34572)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/SinkItemDropQuery.cs:ToItem"
      },
      "Maps sink tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification sink item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Tables" && line == 35240)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TableItemDropQuery.cs:ToItem"
      },
      "Maps first and second table tile styles using explicit legacy input branches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification table item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Bathtubs" && line == 35433)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/BathtubItemDropQuery.cs:ToItem"
      },
      "Maps bathtub tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification bathtub item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Workbenches" && line == 35602)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/WorkbenchItemDropQuery.cs:ToItem"
      },
      "Maps workbench tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification workbench item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Chair" && line == 35792)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/ChairItemDropQuery.cs:ToItem"
      },
      "Maps chair tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification chair item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Toilet" && line == 35932)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/ToiletItemDropQuery.cs:ToItem"
      },
      "Maps toilet tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification toilet item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Platforms" && line == 36046)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PlatformItemDropQuery.cs:ToItem"
      },
      "Maps platform tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification platform item-drop mapping checks.");
  }

  if (name == "GetItemDrop_MusicBoxes" && line == 36259)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/MusicBoxItemDropQuery.cs:ToItem"
      },
      "Maps music-box tile styles to item identifiers, including negative-style behavior.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification music-box item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Dressers" && line == 42757)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/DresserItemDropQuery.cs:ToItem"
      },
      "Maps dresser tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification dresser item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Chests" && line == 34701)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/ChestItemDropQuery.cs:ToItem"
      },
      "Maps first and second chest tile styles using explicit legacy input branches.",
      new[]
      {
        "The private coordinate wrapper reads Main.tile and remains unmapped.",
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification chest item-drop mapping checks.");
  }

  if (name == "GetItemDrop_FakeChests" && line == 34990)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/FakeChestItemDropQuery.cs:ToItem"
      },
      "Maps first and second fake-chest tile styles, including explicit no-drop values.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification fake-chest item-drop mapping checks.");
  }

  if (name == "GetCampfireItemDrop" && line == 42943)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/CampfireItemDropQuery.cs:ToItem"
      },
      "Maps campfire tile styles to item identifiers using the bounded legacy switch rules.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification campfire item-drop mapping checks.");
  }

  if (name == "GetRainbowPaintIDForPosition" && line == 21860)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/RainbowPaintQuery.cs:ToPaintId"
      },
      "Maps direct and wiggly coordinate paint identifiers using the legacy arithmetic.",
      new[]
      {
        "Legacy tile and wall mutation remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification rainbow paint mapping checks.");
  }

  if (name == "IsLockedDungeonBiomeChest" && line == 29381)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/DungeonChestQuery.cs:" +
        "IsLockedBiomeChest"
      },
      "Maps locked dungeon biome chest type and style classification.",
      new[]
      {
        "Legacy chest storage and tile ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification locked dungeon chest mapping checks.");
  }

  if (name == "GetPileGenerationAttempts" && line == 21715)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PileGenerationAttemptPolicy.cs:" +
        "GetAttempts"
      },
      "Maps pile attempts from explicit world width and skyblock rule inputs.",
      new[]
      {
        "Legacy Main.maxTilesX and skyblockWorldGen reads remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification pile generation attempt policy checks.");
  }

  if (name == "PlantCheck_TryGetNewType" && line == 67430)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PlantTypeConversionQuery.cs:Evaluate"
      },
      "Maps plant tile type, frame, and mushroom conversion from explicit tile inputs.",
      new[]
      {
        "Legacy PlantCheck neighborhood scans, slope rules, tile mutation, and destruction remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification plant type conversion checks.");
  }

  if (name == "PlantCheck_CanPlaceHook" && line == 67332)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PlantPlacementQuery.cs:CanPlace"
      },
      "Maps plant support eligibility from an immutable snapshot and tile definitions.",
      new[]
      {
        "Legacy mutable Tile access and placement-hook integration remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification plant placement checks.");
  }

  if (name == "PlantCheck" && line == 67360)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PlantCheckQuery.cs:Evaluate",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/PlantCheckCommandSystem.cs:" +
        "TryCreateCommand",
        "src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs:TryCommit"
      },
      "Maps clamped plant support, conversion, destruction, and atomic type/frame command intent.",
      new[]
      {
        "Legacy nullable 3x3 tile scan and KillTile side effects remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification plant check command checks.");
  }

  if (name == "PlantCheck_IsBadTypeMatch" && line == 67434)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PlantTypeConversionQuery.cs:" +
        "IsBadTypeMatch"
      },
      "Maps plant and support tile compatibility classification.",
      new[]
      {
        "Legacy PlantCheck neighborhood scans, slope rules, tile mutation, and destruction remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification plant type conversion checks.");
  }

  if (name == "CanPoundTile" && line == 67449)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TilePoundingEligibilityQuery.cs:" +
        "CanPound"
      },
      "Maps local pound eligibility from a snapshot, boulder definitions, and CanKillTile input.",
      new[]
      {
        "Legacy mutable Tile initialization and full CanKillTile ownership remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification tile pounding eligibility checks.");
  }

  if (name == "ForbidsSloping" && line == 67501)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileSlopingQuery.cs:ForbidsSloping"
      },
      "Maps the bounded legacy tile-type table that forbids sloping below active tiles.",
      new[]
      {
        "Legacy mutable Tile coordinate access is represented by an explicit tile-type input."
      },
      "Terraria.Dome.WorldGeneration.Verification tile sloping checks.");
  }

  if (name == "SlopeTile" && line == 67526)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/TilePoundingCommandSystem.cs:" +
        "TryCreateSlopeCommand",
        "src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs:TryCommit"
      },
      "Maps accepted slope changes to atomic shape commands.",
      new[]
      {
        "Legacy effects, player movement, framing, and network broadcast remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification tile slope command checks.");
  }

  if (name == "PoundTile" && line == 67565)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/TilePoundingCommandSystem.cs:" +
        "TryCreatePoundCommand",
        "src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs:TryCommit"
      },
      "Maps accepted half-brick toggles to atomic shape commands.",
      new[]
      {
        "Legacy effects, player movement, framing, and network broadcast remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification tile pound command checks.");
  }

  if (name == "TileMergeAttemptFrametest" && line is 67602 or 67656)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ApplyFrametest"
      },
      "Maps neighbor rewrite and cardinal frame-work decisions from explicit values.",
      new[]
      {
        "Legacy TileFrame side effects and raw bool-array ownership remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification tile merge query checks.");
  }

  if (name == "TileMergeAttempt" && line is 67710 or 67732)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceCardinal"
      },
      "Maps four-neighbor replacement from explicit type or type-set inputs.",
      new[]
      {
        "Legacy raw bool-array ownership becomes an explicit set."
      },
      "Terraria.Dome.WorldGeneration.Verification tile merge query checks.");
  }

  if (name == "TileMergeAttempt" && line is 67754 or 67792)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceAll"
      },
      "Maps eight-neighbor replacement from explicit type or type-set inputs.",
      new[]
      {
        "Legacy raw bool-array ownership becomes an explicit set."
      },
      "Terraria.Dome.WorldGeneration.Verification tile merge query checks.");
  }

  if (name == "TileMergeAttempt" && line is 67830 or 67868)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceAllExcept"
      },
      "Maps eight-neighbor replacement with explicit excluded type inputs.",
      new[]
      {
        "Legacy raw bool-array ownership becomes explicit sets."
      },
      "Terraria.Dome.WorldGeneration.Verification tile merge query checks.");
  }

  if (name == "TileMergeAttemptWeird" && line == 67906)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:" +
        "ReplaceDifferentExcept"
      },
      "Maps eight-neighbor replacement for non-excluded types distinct from the source type.",
      new[]
      {
        "Legacy raw bool-array ownership becomes an explicit set."
      },
      "Terraria.Dome.WorldGeneration.Verification tile merge query checks.");
  }

  if (name == "GetTileMossColor" && line == 67944)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/MossColorQuery.cs:GetColor"
      },
      "Maps the bounded legacy moss tile-type table to moss color identifiers.",
      Array.Empty<string>(),
      "Terraria.Dome.WorldGeneration.Verification moss color checks.");
  }

  if (name == "StatueStyleToItem" && line == 31437)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/StatueStyleItemQuery.cs:ToItem"
      },
      "Maps statue styles to item IDs with the bounded legacy switch table.",
      new[]
      {
        "Legacy item registry ownership and statue placement side effects remain excluded."
      },
      "Terraria.Dome.WorldGeneration.Verification statue style mapping checks.");
  }

  if (name == "OrePatch" && line == 9624)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "OrePatchEligibilityQuery.cs:Evaluate",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrePatchPlacementSystem.cs:TryPrepare",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrePatchPlacementSystem.cs:TryAppendCommands"
      },
      "Composes bounded OrePatch eligibility with an explicit ore definition and a " +
      "conflict-safe prepare/command transaction from immutable snapshots.",
      new[]
      {
        "SavedOreTiers selection and legacy genRand stream parity.",
        "OreHelper, random ore-walk mutation, SquareTileFrame, and mutable Tile ownership."
      },
      "Terraria.Dome.WorldGeneration.Verification ore patch preparation and command checks.");
  }

  if (name == "TreeGrowFXCheck" && line == 23974)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeLeafScanQuery.cs:Scan"
      },
      "Scans immutable snapshot tree tiles and computes leaf pass-style inputs without " +
      "network dispatch.",
      new[]
      {
        "Legacy Main.tile ownership and TileID.Sets.GetsCheckedForLeaves lookup.",
        "NetMessage.SendData tree-growth FX dispatch."
      },
      "Terraria.Dome.WorldGeneration.Verification tree leaf snapshot scan.");
  }

  if (name == "GetTreeLeaf" && line == 24008)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeLeafPassStyleQuery.cs:Evaluate"
      },
      "Computes tree frame, foliage pass style, and hollow-tree height increment from " +
      "explicit Tile values and foliage-style input.",
      new[]
      {
        "Legacy GetHollowTreeFoliageStyle global lookup.",
        "TreeGrowFXCheck network dispatch and mutable Tile ownership."
      },
      "Terraria.Dome.WorldGeneration.Verification tree leaf pass-style checks.");
  }

  if (name == "GrowUndergroundTree" && line == 25416)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "UndergroundTreeGrowthEligibilityQuery.cs:Evaluate",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "UndergroundTreeHeightPolicy.cs:Next",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "UndergroundTreeTrunkCommandSystem.cs:TryAppendCommands"
      },
      "Checks bounded underground-tree eligibility, selects deterministic height, and emits " +
      "command-only trunk placement from an immutable snapshot.",
      new[]
      {
        "Legacy genRand stream parity and random branch selection.",
        "Branch mutation, frame updates, RangeFrame, and network side effects."
      },
      "Terraria.Dome.WorldGeneration.Verification underground tree eligibility checks.");
  }

  if (name == "IsTileALeafyTreeTop" && line == 24227)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeLeafFrameQuery.cs:IsLeafyTreeTop"
      },
      "Classifies active leafy-tree top frames against an explicit immutable type set.",
      new[]
      {
        "Legacy TileID.Sets.GetsCheckedForLeaves ownership and CallTracker instrumentation."
      },
      "Terraria.Dome.WorldGeneration.Verification leafy tree top frame checks.");
  }

  if (name == "IsTileATreeBranch" && line == 24269)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeTrunkFrameQuery.cs:TryGetBranchOffset"
      },
      "Classifies legacy tree branch frames against an explicit immutable trunk-type set.",
      new[]
      {
        "Legacy TileID.Sets.IsATreeTrunk ownership and CallTracker instrumentation."
      },
      "Terraria.Dome.WorldGeneration.Verification tree branch frame checks.");
  }

  if (name == "IsTileATreeRoot" && line == 24296)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeTrunkFrameQuery.cs:TryGetRootOffset"
      },
      "Classifies legacy tree root frames against an explicit immutable trunk-type set.",
      new[]
      {
        "Legacy TileID.Sets.IsATreeTrunk ownership and CallTracker instrumentation."
      },
      "Terraria.Dome.WorldGeneration.Verification tree root frame checks.");
  }

  if (name == "GrowTree" && line == 24323)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "OrdinaryTreeGrowthEligibilityQuery.cs:Evaluate",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "OrdinaryTreeHeightPolicy.cs:Next",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrdinaryTreeTrunkCommandSystem.cs:TryAppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrdinaryTreePlacementSystem.cs:TryPrepare",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrdinaryTreePlacementSystem.cs:TryPrepareWithHeightSelection",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrdinaryTreePlacementSystem.cs:TryAppendCommands"
      },
      "Separates immutable-snapshot ordinary-tree preparation, bounded deterministic height " +
      "selection, and command-only trunk placement.",
      new[]
      {
        "Legacy genRand stream parity and errorWorld/extraLivingTrees height branches.",
        "World-rule branches for remix and notTheBees.",
        "Branch/foliage mutation, framing, colors, and network side effects."
      },
      "Terraria.Dome.WorldGeneration.Verification ordinary tree eligibility and trunk checks.");
  }

  if (name == "IsTileTypeFitForTree" && line == 24245)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "OrdinaryTreeGroundQuery.cs:IsSuitable"
      },
      "Classifies explicit ordinary-tree ground Tile IDs without reading legacy global sets.",
      new[]
      {
        "Legacy TileID sets and CallTracker instrumentation.",
        "GrowTree eligibility, random height, tree mutation, framing, and network side effects."
      },
      "Terraria.Dome.WorldGeneration.Verification ordinary tree ground checks.");
  }

  if (name == "EmptyTileCheck" && line == 25960)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeCanopyClearanceQuery.cs:IsClear"
      },
      "Checks an explicit immutable snapshot rectangle for legacy tree canopy clearance.",
      new[]
      {
        "Legacy TileID.Sets.CommonSapling ownership and mutable Main.tile state.",
        "CallTracker instrumentation and callers outside tree canopy preparation."
      },
      "Terraria.Dome.WorldGeneration.Verification tree canopy clearance checks.");
  }

  if (name == "GrowTreeWithSettings" && line == 24955)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeProfileGrowthEligibilityQuery.cs:Evaluate",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "TreeProfileTrunkCommandSystem.cs:TryAppendCommands"
      },
      "Validates an explicit profile tree root and emits protected trunk commands from an immutable snapshot.",
      new[]
      {
        "Legacy genRand height selection, error-world variations, and empty-canopy clearance.",
        "Branch and foliage layout, framing, color changes, and NetMessage effects."
      },
      "Terraria.Dome.WorldGeneration.Verification profile tree eligibility checks.");
  }

  if (name == "TryGrowingTreeByType" && line == 24908)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeGrowthDispatchQuery.cs:TryGet"
      },
      "Selects an explicit ordinary, palm, or profile tree handler without executing growth.",
      new[]
      {
        "Legacy grow handler execution, Main.tile mutation, and genRand stream ownership.",
        "Tree framing, color changes, NetMessage side effects, and unsupported handler behavior."
      },
      "Terraria.Dome.WorldGeneration.Verification bounded tree-dispatch checks.");
  }

  if (name is "DefaultTreeWallTest" or "GemTreeWallTest" && line is 24815 or 24826)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeWallSuitabilityQuery.cs:IsSuitable"
      },
      "Classifies an explicit wall definition as suitable for a legacy tree profile.",
      new[]
      {
        "Legacy WallID.Sets ownership and wall-definition initialization.",
        "Tree placement, CallTracker instrumentation, and mutable timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tree wall suitability checks.");
  }

  if (name is "GemTreeGroundTest" or "VanityTreeGroundTest" or "AshTreeGroundTest" &&
      line is 24863 or 24878 or 24893)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeGroundSuitabilityQuery.cs:IsSuitable"
      },
      "Classifies an explicit tile definition as suitable ground for a legacy tree profile.",
      new[]
      {
        "Legacy TileID.Sets ownership and tile-definition initialization.",
        "Tree placement, wall suitability, CallTracker instrumentation, and mutable timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tree ground suitability checks.");
  }

  if (name == "TryGetFromTreeId" && line == 3951)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Definitions/" +
        "LegacyTreeProfileRegistry.cs:TryGet"
      },
      "Looks up immutable tree profile metadata by legacy tree tile ID.",
      new[]
      {
        "Legacy ground and wall suitability delegates.",
        "Tree placement, foliage, CallTracker instrumentation, and mutable generator timing."
      },
      "Terraria.Dome.WorldGeneration.Verification immutable tree-profile lookup checks.");
  }

  if (name == "randMoss" && line == 9028)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/MossSelectionPolicy.cs:Next" },
      "Selects legacy neon moss and distinct ordinary moss types through explicit state.",
      new[]
      {
        "Legacy neonMossType and mossType global ownership and genRand stream ownership.",
        "CallTracker instrumentation and mutable generator timing."
      },
      "Terraria.Dome.WorldGeneration.Verification deterministic moss-selection checks.");
  }

  if (name == "randGem" && line == 8997)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/GemTileRandomPolicy.cs:NextGemIndex" },
      "Draws legacy gem indexes through explicit deterministic state and enabled-gem flags.",
      new[]
      {
        "Legacy gem global array ownership and genRand stream ownership.",
        "CallTracker instrumentation and mutable generator timing."
      },
      "Terraria.Dome.WorldGeneration.Verification deterministic gem-selection checks.");
  }

  if (name == "randGemTile" && line == 9009)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/GemTileRandomPolicy.cs:NextTile" },
      "Applies the legacy one-in-twenty gem tile draw and tile ID mapping through explicit state.",
      new[]
      {
        "Legacy gem global array ownership and genRand stream ownership.",
        "CallTracker instrumentation and mutable generator timing."
      },
      "Terraria.Dome.WorldGeneration.Verification deterministic gem-selection checks.");
  }

  if (name == "RandomWorldPoint" && line == 22195)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/RandomWorldPointPolicy.cs:Next"
      },
      "Generates an inset world point through explicit deterministic generation state.",
      new[]
      {
        "Legacy genRand global ownership and shared stream ordering outside the explicit state.",
        "Legacy Main world dimensions, Point type, CallTracker instrumentation, and generator timing."
      },
      "Terraria.Dome.WorldGeneration.Verification deterministic world-point checks.");
  }

  if (name == "RandomRectanglePoint" && line is 22181 or 22188)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "RandomRectanglePointPolicy.cs:Next"
      },
      "Generates a bounded rectangle point through explicit deterministic generation state.",
      new[]
      {
        "Legacy genRand global ownership and shared stream ordering outside the explicit state.",
        "Legacy Point/Rectangle types, CallTracker instrumentation, and mutable generator timing.",
        "Invalid or empty rectangles are rejected by the explicit simulation contract."
      },
      "Terraria.Dome.WorldGeneration.Verification deterministic rectangle-point checks.");
  }

  if (name == "errorWorldAdjustment" && line == 328)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "SecretSeedAdjustmentPolicy.cs:Adjust"
      },
      "Applies the legacy active-secret-seed default and integer scaling to an explicit value.",
      new[]
      {
        "Legacy activeSecretSeedCount global ownership and secret-seed registration lifecycle.",
        "Legacy CallTracker instrumentation and mutable secret-seed state."
      },
      "Terraria.Dome.WorldGeneration.Verification secret-seed adjustment checks.");
  }

  if (name == "IsSafeFromRain" && line == 60739)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileLineTraceQuery.cs:IsSafeFromRainPath" },
      "Traces an explicit rain trajectory through an immutable snapshot and stops at solid tiles.",
      new[]
      {
        "Legacy Rain static velocity provider and Main.windSpeedCurrent ownership.",
        "Legacy CallTracker instrumentation and DelegateMethods mutable CheckResultOut state.",
        "The compatibility wrapper's implicit pixel/vector inputs are replaced by explicit parameters."
      },
      "Terraria.Dome.WorldGeneration.Verification snapshot rain-trace checks.");
  }

  if (name is "TopEdgeCanBeAttachedTo" or "RightEdgeCanBeAttachedTo" or
      "LeftEdgeCanBeAttachedTo" or "BottomEdgeCanBeAttachedTo" &&
      line is 58950 or 58972 or 58994 or 59016)
  {
    string target = line switch
    {
      58950 => "CanAttachToTop",
      58972 => "CanAttachToRight",
      58994 => "CanAttachToLeft",
      _ => "CanAttachToBottom"
    };
    return new SourceMapping("Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:" + target },
      "Classifies a snapshot tile as an attachment edge using immutable Version4 tile definitions.",
      new[] { "Legacy nullable Tile slots and exception swallowing.", "CallTracker and mutable Main.tile timing." },
      "Terraria.Dome.WorldGeneration.Verification tile-attachment query checks.");
  }

  if (name == "GetWorldUpdateRate" && line == 59850)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/WorldUpdateRatePolicy.cs:GetRate" },
      "Caps an explicit desired tile-update rate at 24 and returns zero while time is frozen.",
      new[]
      {
        "Legacy Main.desiredWorldTilesUpdateRate global ownership and scheduler integration.",
        "CreativePowerManager state lookup and CallTracker instrumentation.",
        "Negative desired rates are rejected by the explicit simulation contract."
      },
      "Terraria.Dome.WorldGeneration.Verification world update-rate policy checks.");
  }

  if (name == "CountNearBlocksTypes" && line == 58214)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileNeighborhoodQuery.cs:CountNearbyTileTypes" },
      "Counts active matching snapshot tiles in a clamped square with the legacy cap cutoff.",
      new[]
      {
        "Legacy Main dimensions, direct mutable Main.tile access, and nullable Tile behavior.",
        "Legacy CallTracker instrumentation and mutable Tile timing.",
        "Negative radius and negative tile types are rejected by the explicit snapshot contract."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-neighborhood counting checks.");
  }

  if (name == "setWorldSize" && line == 6221)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:PixelWidth",
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:PixelHeight",
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:SectionColumnCount",
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:SectionRowCount"
      },
      "Derives immutable pixel and section dimensions from world bounds.",
      new[]
      {
        "Legacy Main.bottomWorld, rightWorld, maxSectionsX, and maxSectionsY mutation.",
        "Global world initialization order and any caller-visible static side effects."
      },
      "Terraria.Dome.WorldGeneration.Verification world-size derived bounds checks.");
  }

  if (name == "GetWorldSize" && line == 6231)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/WorldSizeProfile.cs:" +
        "GetLegacyIndexForWidth"
      },
      "Maps the legacy width thresholds to a pure world size index classifier.",
      new[]
      {
        "Legacy Main.maxTilesX static read and CallTracker instrumentation.",
        "World creation, persistence, protocol, and global state interactions."
      },
      "Terraria.Dome.WorldGeneration.Verification world-size width classification checks.");
  }

  if (name == "SetWorldSize" && line == 6246)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/WorldSizeProfile.cs:FromLegacyIndex"
      },
      "Maps legacy size indices to immutable fixed tile dimension profiles.",
      new[]
      {
        "Legacy Main.maxTilesX/maxTilesY mutation and global lifecycle ordering.",
        "World allocation, save state, network state, and protocol reconfiguration."
      },
      "Terraria.Dome.WorldGeneration.Verification fixed world-size profile checks.");
  }

  if (name == "InWorld" && line == 8944)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:Contains"
      },
      "Maps the Point overload to the immutable coordinate bounds contract.",
      new[]
      {
        "Legacy Terraria.Point adapter and Main.maxTilesX/maxTilesY static state.",
        "Legacy CallTracker instrumentation and caller-specific Tile assumptions."
      },
      "Terraria.Dome.WorldGeneration.Verification bounds overload checks.");
  }

  if (name == "InWorld" && line == 8951)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:Contains"
      },
      "Maps coordinate bounds with a non-negative symmetric fluff margin.",
      new[]
      {
        "Legacy Main.maxTilesX/maxTilesY static dimensions and CallTracker behavior.",
        "Any Tile existence, Tile initialization, or caller-side world state assumptions."
      },
      "Terraria.Dome.WorldGeneration.Verification coordinate and fluff checks.");
  }

  if (name == "InWorld" && line == 8962)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:ContainsRectangle"
      },
      "Maps rectangular bounds with explicit width, height, and fluff validation.",
      new[]
      {
        "Legacy Terraria.Rectangle adapter and Main.maxTilesX/maxTilesY static state.",
        "Legacy integer overflow behavior, CallTracker instrumentation, and Tile side effects."
      },
      "Terraria.Dome.WorldGeneration.Verification rectangle bounds checks.");
  }

  if (name == "AreAnyTilesInSetNearby" && line == 8153)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileNeighborhoodQuery.cs:" +
        "AreAnyTilesInSetNearby"
      },
      "Queries an immutable snapshot over the inclusive square neighborhood for active tiles.",
      new[]
      {
        "Legacy Main.tile reads, nullable Tile slots, and Main-based InWorld checks.",
        "Legacy CallTracker instrumentation and any mutable-world timing assumptions.",
        "Legacy null or short bool[] exception behavior; short read-only sets are non-matches."
      },
      "Terraria.Dome.WorldGeneration.Verification snapshot neighborhood query checks.");
  }

  if (name == "IsTileNearby" && line == 8183)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileNeighborhoodQuery.cs:IsTileNearby"
      },
      "Queries an immutable snapshot over the inclusive neighborhood with tile-235 X stride.",
      new[]
      {
        "Legacy Main.tile reads, nullable Tile slots, and Main-based InWorld checks.",
        "Legacy CallTracker instrumentation and any mutable-world timing assumptions.",
        "Legacy negative tile-type behavior; the snapshot query rejects negative type values."
      },
      "Terraria.Dome.WorldGeneration.Verification snapshot neighborhood query checks.");
  }

  if (name == "countTiles" && line == 8799)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileRegionProbe.cs:CountOpenTiles"
      },
      "Returns a bounded snapshot flood-fill result with legacy category counts.",
      new[]
      {
        "Legacy static counter fields, CountedTiles dictionary, and CallTracker instrumentation.",
        "Legacy nullable Tile access and direct Main dimensions/global Tile state.",
        "Unsupported definition registries and callers that read legacy counter fields after return."
      },
      "Terraria.Dome.WorldGeneration.Verification bounded region-probe checks.");
  }

  if (name == "nextCount" && line == 8814)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileRegionProbe.cs:CountOpenTiles"
      },
      "Performs the bounded left-right-up-down snapshot traversal for CountOpenTiles.",
      new[]
      {
        "Legacy recursive call stack, static mutable counters, and CountedTiles dictionary identity.",
        "Legacy nullable Tile access, Main dimensions, CallTracker instrumentation, and global Tile state.",
        "The separate countDirtTiles and nextDirtCount traversal rules."
      },
      "Terraria.Dome.WorldGeneration.Verification bounded region-probe checks.");
  }

  if (name == "countDirtTiles" && line == 8894)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileDirtRegionProbe.cs:CountTiles"
      },
      "Returns a bounded snapshot dirt-wall region count without shared mutable counters.",
      new[]
      {
        "Legacy static numTileCount and CountedTiles state, plus CallTracker instrumentation.",
        "Legacy nullable Tile access and direct Main dimensions/global Tile state.",
        "Legacy callers that read shared counters or use the region result to run placement logic."
      },
      "Terraria.Dome.WorldGeneration.Verification bounded dirt-region probe checks.");
  }

  if (name == "nextDirtCount" && line == 8904)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileDirtRegionProbe.cs:CountTiles"
      },
      "Performs bounded snapshot traversal through dirt or jungle walls with legacy neighbors.",
      new[]
      {
        "Legacy recursive call stack, static mutable counters, and CountedTiles dictionary identity.",
        "Legacy nullable Tile access, Main dimensions, CallTracker instrumentation, and global Tile state.",
        "The separate countTiles and nextCount lava, shimmer, and category-count rules."
      },
      "Terraria.Dome.WorldGeneration.Verification bounded dirt-region probe checks.");
  }

  if (name == "SolidTile" && line == 58615)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolid" },
      "Classifies a supplied immutable tile from explicit solid and platform definitions.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid and Main.tileSolidTop static arrays and CallTracker instrumentation.",
        "Legacy Tile reference identity and mutation timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "TileEmpty" && line == 58636)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsEmpty" },
      "Classifies snapshot cells as empty when inactive or marked inactive.",
      new[]
      {
        "Legacy nullable Tile slots and direct Main.tile array access.",
        "Legacy CallTracker instrumentation and out-of-bounds exception behavior."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidOrSlopedTile" && line == 58647)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidOrSloped" },
      "Classifies supplied active non-platform solid tiles without excluding slopes or half bricks.",
      new[]
      {
        "Legacy nullable Tile behavior and Main.tileSolid/Main.tileSolidTop static arrays.",
        "Legacy CallTracker instrumentation and Tile reference identity."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "TileType" && line == 58658)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:GetActiveTileType" },
      "Returns the active snapshot tile type or negative one when inactive.",
      new[]
      {
        "Legacy nullable Tile slots, direct Main.tile array access, and CallTracker instrumentation.",
        "Legacy out-of-bounds exception behavior and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidOrSlopedTile" && line == 58669)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidOrSloped" },
      "Classifies a snapshot coordinate through the immutable solid-or-sloped predicate.",
      new[]
      {
        "Legacy nullable Tile slots, direct Main.tile array access, and CallTracker instrumentation.",
        "Legacy out-of-bounds exception behavior and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTile" && line == 58770)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolid" },
      "Classifies a snapshot coordinate with the legacy no-doors solid-tile option.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid/Main.tileSolidTop static arrays and CallTracker instrumentation.",
        "The old Point overload and direct mutable Main.tile access."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTile2" && line == 58795)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidWithoutPlatformTop" },
      "Classifies supplied immutable tiles with the SolidTile2 platform top-slope rule.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid static array and CallTracker instrumentation.",
        "Legacy Tile reference identity and mutation timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "PlatformProperTopFrame" && line == 58816)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsPlatformProperTopFrame" },
      "Classifies platform frame columns using the fixed legacy 18-pixel frame width.",
      new[]
      {
        "Legacy TileObjectData runtime lookup for platform coordinate width.",
        "Legacy CallTracker instrumentation and any modded platform frame contract."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTileAllowBottomSlope" && line == 58832)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingBottomSlope" },
      "Classifies snapshot cells with bottom slopes and valid platform top-frame exceptions.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid/Main.tileSolidTop and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTile2" && line == 59064)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidWithoutPlatformTop" },
      "Classifies snapshot coordinates with the SolidTile2 platform top-slope rule.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tile array access, Main.tileSolid static array, and CallTracker instrumentation.",
        "Legacy out-of-bounds exception behavior and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTileNoPlatforms" && line == 58858)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidWithoutPlatforms" },
      "Classifies snapshot cells as solid while excluding platform definitions.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid/Main.tileSolidTop and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTileAllowTopSlope" && line == 58884)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingTopSlope" },
      "Classifies snapshot cells that allow top-facing slope support and platform half bricks.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTileAllowLeftSlope" && line == 58906)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingLeftSlope" },
      "Classifies snapshot cells that allow left-facing slope support.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTileAllowRightSlope" && line == 58928)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingRightSlope" },
      "Classifies snapshot cells that allow right-facing slope support.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTile3" && line == 59038)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:" +
        "IsSolidWithLegacyTile3Semantics"
      },
      "Classifies snapshot coordinates through the legacy one-tile-fluff SolidTile3 boundary.",
      new[]
      {
        "Legacy Main.tile array and Main.tileSolid/Main.tileSolidTop static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing.",
        "Legacy nullable Tile behavior is excluded because snapshot cells are non-null values."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTile3" && line == 59049)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:" +
        "IsSolidWithLegacyTile3Semantics"
      },
      "Classifies supplied immutable tiles as active non-platform solid tiles.",
      new[]
      {
        "Legacy nullable Tile behavior and Main.tileSolid/Main.tileSolidTop static arrays.",
        "Legacy CallTracker instrumentation and Tile reference identity."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "HasAnyWireNearby" && line == 60717)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileWireQuery.cs:HasAnyWireNearby" },
      "Scans a clamped immutable snapshot rectangle for any of the four wire channels.",
      new[]
      {
        "Legacy nullable Tile slots, Main dimensions, and direct mutable Main.tile access.",
        "Legacy CallTracker instrumentation and mutable Tile timing.",
        "Negative boxSpread is rejected by the explicit snapshot contract."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-wire query checks.");
  }

  if (name == "GetRopeEnds" && line == 58676)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileRopeQuery.cs:FindEnds" },
      "Finds bounded rope endpoints in an immutable snapshot using the Version4 rope set.",
      new[]
      {
        "Legacy nullable Tile slots, Main dimensions, and direct mutable Main.tile access.",
        "Legacy CallTracker instrumentation and mutable Tile timing.",
        "Negative rangeToCheck is rejected by the explicit snapshot contract."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-rope query checks.");
  }

  if (name == "IsRope" && line == 58736)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileRopeQuery.cs:IsRope" },
      "Classifies active Version4 rope tiles and supported platform bridges from a snapshot.",
      new[]
      {
        "Legacy nullable Tile slots, Main.tileRope and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing.",
        "The overload that exposes legacy out parameters remains unmapped."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-rope query checks.");
  }

  if (name == "IsRope" && line == 58727)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileRopeQuery.cs:IsRope" },
      "Classifies a snapshot coordinate without exposing the intermediate rope endpoints.",
      new[]
      {
        "Legacy nullable Tile slots, Main.tileRope and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing.",
        "Legacy endpoint out parameters are intentionally not exposed by this convenience mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-rope query checks.");
  }

  if (name == "GetLiquidChangeType" && line == 4527)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/Liquid/Definitions/" +
        "LiquidInteractionClassifier.cs:GetKind"
      },
      "Maps the four liquid pair table to a protocol-independent interaction fact.",
      new[]
      {
        "Legacy TileChangeType protocol enum and NetMessage.SendTileSquare projection.",
        "Tile placement, liquid mutation, framing, and any runtime side effects.",
        "Compatibility packet encoding and caller-specific notification routing."
      },
      "Terraria.Dome.WorldGeneration.Verification liquid interaction classification checks.");
  }

  if (name == "EmptyLiquid" && line == 4454)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/Systems/" +
        "LiquidChangeCommitSystem.cs:TryCommit"
      },
      "Maps validated liquid state clearing to the immutable command commit boundary.",
      new[]
      {
        "Legacy Main.tile access, solid-tile predicates, and direct liquid clearing.",
        "SquareTileFrame, NetMessage.sendWater, and runtime notification side effects.",
        "Legacy global Liquid queue and all liquid-type-specific compatibility behavior."
      },
      "Terraria.Dome.WorldGeneration.Verification liquid command validation and commit checks.");
  }

  if (name == "PlaceLiquid" && line == 4478)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LiquidPropagationSession.cs:Advance",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "LiquidPropagationSystem.cs:TryAppendCommands",
        "src/Terraria.Dome.Simulation/World/Systems/" +
        "LiquidChangeCommitSystem.cs:TryCommit"
      },
      "Maps bounded liquid work-item propagation, amount validation, and typed command commit.",
      new[]
      {
        "Legacy Main.tile solid predicates and direct liquid mutation/clamping.",
        "SquareTileFrame, NetMessage, and runtime notification side effects.",
        "Legacy Liquid.GetLiquidMergeTypes tile placement and global queue semantics.",
        "Uncaptured liquid definitions, secret seeds, and unsupported world-generation rules."
      },
      "Terraria.Dome.WorldGeneration.Verification bounded propagation, merge, and commit checks.");
  }

  if (name == "AddPasses" && line == 10553)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "DirtWallBackgroundSystem.cs:AppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationPipeline.cs:Generate",
        "src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationRequest.cs:" +
        "WorldGenerationRequest"
      },
      "Models the DirtWallBackgrounds predicate and conditionally commits oracle wall commands.",
      new[]
      {
        "Legacy pass registration order, GenVars, and all other generation passes.",
        "UnifiedRandom state except for the archived default seed-1456 offset artifact.",
        "Secret-seed, difficulty, hardmode, and all unmatched world sizes/options."
      },
      "Fixture plus source-built default seed-1456 oracle differential.");
  }

  if (name != "GenerateWorld" || line != 10108)
  {
    return null;
  }

  return new SourceMapping(
    "Partial",
    new[] { "src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationPipeline.cs:Generate" },
    "Creates a deterministic request-to-snapshot stage pipeline through command commits.",
    new[]
    {
      "Legacy Main state, configuration hooks, and complete generator pass registration.",
      "Secret-seed, difficulty, evil, and world-option input capture.",
      "clearWorld, Reset, Finish, save, audio, callback, and legacy temporary-state behavior."
    },
    "Terraria.Dome.WorldGeneration.Verification deterministic replay and stage assertions.");
}

static IEnumerable<FieldInventory> CreateFieldInventory(FieldDeclarationSyntax field)
{
  string body = field.ToFullString();
  string visibility = field.Modifiers.Any(SyntaxKind.PublicKeyword)
    ? "Public"
    : field.Modifiers.Any(SyntaxKind.InternalKeyword) ? "Internal" : "Private";
  foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
  {
    yield return new FieldInventory(
      variable.Identifier.ValueText,
      visibility,
      field.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
      field.Modifiers.Any(SyntaxKind.StaticKeyword),
      Classify(body),
      "Unmapped",
      CreateReferences(body));
  }
}

static List<ReferenceInventory> CreateReferenceInventory(CompilationUnitSyntax root)
{
  string source = root.ToFullString();
  return new[] { "Main", "Tile", "Liquid", "NetMessage" }
    .Select(reference => new ReferenceInventory(
      reference,
      CountToken(source, reference),
      source.Contains(reference, StringComparison.Ordinal) ? "Observed" : "Absent"))
    .ToList();
}

static List<string> CreateReferences(string body)
{
  return new[] { "Main", "Tile", "Liquid", "NetMessage" }
    .Where(reference => body.Contains(reference, StringComparison.Ordinal))
    .ToList();
}

static string Classify(string body)
{
  if (body.Contains("Liquid", StringComparison.Ordinal))
  {
    return "Liquid";
  }

  if (body.Contains("Tile", StringComparison.Ordinal))
  {
    return "Tile";
  }

  if (body.Contains("Structure", StringComparison.Ordinal) ||
      body.Contains("Dungeon", StringComparison.Ordinal))
  {
    return "Structure";
  }

  if (body.Contains("Tree", StringComparison.Ordinal) ||
      body.Contains("Grow", StringComparison.Ordinal))
  {
    return "Tree";
  }

  if (body.Contains("Ore", StringComparison.Ordinal) ||
      body.Contains("Gem", StringComparison.Ordinal))
  {
    return "Ore";
  }

  if (body.Contains("Biome", StringComparison.Ordinal) ||
      body.Contains("Surface", StringComparison.Ordinal))
  {
    return "Biome";
  }

  return "WorldRuntime";
}

static int CountToken(string source, string token)
{
  return source.Split(token, StringSplitOptions.None).Length - 1;
}

static string CreateSnapshotFingerprint(WorldGridSnapshot snapshot)
{
  StringBuilder builder = new();
  for (int y = 0; y < snapshot.Metadata.Height; y++)
  {
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      builder.Append(snapshot.GetTile(x, y));
    }
  }

  for (int y = 0; y < snapshot.Metadata.Height / WorldGrid.SectionHeight; y++)
  {
    for (int x = 0; x < snapshot.Metadata.Width / WorldGrid.SectionWidth; x++)
    {
      builder.Append(snapshot.GetSectionVersion(new WorldSectionCoordinates(x, y)));
    }
  }

  return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())));
}

static string CreateLegacyOracleFingerprint(LegacyWorldDocument document)
{
  using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
  AppendText(hash, document.Version.ToString());
  AppendText(hash, document.Metadata.Name);
  AppendInt32(hash, document.Metadata.WorldId);
  AppendInt32(hash, document.Metadata.Width);
  AppendInt32(hash, document.Metadata.Height);
  AppendInt32(hash, document.Metadata.SpawnX);
  AppendInt32(hash, document.Metadata.SpawnY);
  Span<byte> tileBuffer = stackalloc byte[32];
  foreach (LegacyTile tile in document.Tiles)
  {
    int offset = 0;
    tileBuffer[offset++] = tile.IsActive ? (byte)1 : (byte)0;
    BinaryPrimitives.WriteUInt16LittleEndian(tileBuffer[offset..], tile.TileType);
    offset += sizeof(ushort);
    BinaryPrimitives.WriteUInt16LittleEndian(tileBuffer[offset..], tile.WallType);
    offset += sizeof(ushort);
    tileBuffer[offset++] = tile.LiquidAmount;
    tileBuffer[offset++] = tile.LiquidKind;
    tileBuffer[offset++] = tile.HasWire ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.HasWire2 ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.HasWire3 ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.HasWire4 ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.IsHalfBrick ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.Slope;
    tileBuffer[offset++] = tile.IsActuated ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.IsInactive ? (byte)1 : (byte)0;
    BinaryPrimitives.WriteInt16LittleEndian(tileBuffer[offset..], tile.FrameX);
    offset += sizeof(short);
    BinaryPrimitives.WriteInt16LittleEndian(tileBuffer[offset..], tile.FrameY);
    offset += sizeof(short);
    tileBuffer[offset++] = tile.TileColor;
    tileBuffer[offset++] = tile.WallColor;
    tileBuffer[offset++] = tile.IsInvisibleBlock ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.IsInvisibleWall ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.IsFullbrightBlock ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.IsFullbrightWall ? (byte)1 : (byte)0;
    hash.AppendData(tileBuffer[..offset]);
  }

  foreach (LegacyChest chest in document.Chests)
  {
    AppendText(hash, chest);
  }

  foreach (LegacySign sign in document.Signs)
  {
    AppendText(hash, sign);
  }

  foreach (LegacyNpc npc in document.Npcs)
  {
    AppendText(hash, npc);
  }

  foreach (LegacyTileEntity entity in document.TileEntities)
  {
    AppendText(hash, entity);
  }

  return Convert.ToHexString(hash.GetHashAndReset());
}

static void RunLegacyDifferential(
  string legacyPath,
  string repositoryRoot,
  bool useLegacyMetadataSpawn,
  bool useLegacyMetadataSurfaceY,
  int? requestedSurfaceY,
  string? dirtWallOffsetsPath)
{
  using FileStream oracleStream = new(
    legacyPath,
    FileMode.Open,
    FileAccess.Read,
    FileShare.Read);
  LegacyWorldDocument legacy = WldWorldReader.Read(oracleStream);
  int spawnX = useLegacyMetadataSpawn
    ? legacy.Metadata.SpawnX
    : legacy.Metadata.Width / 2;
  if (useLegacyMetadataSurfaceY && !double.IsFinite(legacy.Metadata.WorldSurface))
  {
    throw new ArgumentOutOfRangeException(
      nameof(legacy.Metadata.WorldSurface),
      "The legacy WLD world surface must be finite.");
  }

  int surfaceY = useLegacyMetadataSurfaceY
    ? (int)legacy.Metadata.WorldSurface
    : requestedSurfaceY ?? legacy.Metadata.Height / 4;
  int legacyWorldSurfaceY = GetLegacyLayerY(
    legacy.Metadata.WorldSurface,
    legacy.Metadata.Height,
    nameof(legacy.Metadata.WorldSurface));
  int legacyRockLayerY = GetLegacyLayerY(
    legacy.Metadata.RockLayer,
    legacy.Metadata.Height,
    nameof(legacy.Metadata.RockLayer));
  if (spawnX < 0 || spawnX >= legacy.Metadata.Width)
  {
    throw new ArgumentOutOfRangeException(
      nameof(spawnX),
      "The selected legacy differential spawn X is outside the world bounds.");
  }

  if (surfaceY < 0 || surfaceY >= legacy.Metadata.Height)
  {
    throw new ArgumentOutOfRangeException(
      nameof(requestedSurfaceY),
      "The selected legacy differential surface Y is outside the world bounds.");
  }

  if (legacyWorldSurfaceY > legacyRockLayerY)
  {
    throw new ArgumentOutOfRangeException(
      nameof(legacy.Metadata.RockLayer),
      "The legacy WLD rock layer must not be above the world surface.");
  }

  string spawnSource = useLegacyMetadataSpawn ? "legacy-metadata" : "world-center";
  string surfaceYSource = useLegacyMetadataSurfaceY
    ? "legacy-world-surface"
    : requestedSurfaceY.HasValue ? "explicit-argument" : "world-quarter";
  WorldMetadata metadata = new(
    legacy.Metadata.Name,
    new WorldSeed(1456),
    legacy.Metadata.Width,
    legacy.Metadata.Height);
  IReadOnlyList<int>? dirtWallSurfaceOffsetChanges = dirtWallOffsetsPath is null
    ? null
    : ParseDirtWallOffsetChanges(
      File.ReadAllLines(dirtWallOffsetsPath),
      metadata.Width);
  WorldGenerationRequest generationRequest = new(
    metadata,
    spawnX,
    surfaceY,
    rockLayerY: legacyRockLayerY,
    dirtWallSurfaceOffsetChanges: dirtWallSurfaceOffsetChanges);
  WorldGrid generated = new WorldGenerationPipeline().Generate(generationRequest);
  int sectionColumns = metadata.Width / WorldGrid.SectionWidth;
  int sectionRows = metadata.Height / WorldGrid.SectionHeight;
  int[,] mismatchesBySection = new int[sectionColumns, sectionRows];
  int[,] extendedStateMismatchesBySection = new int[sectionColumns, sectionRows];
  Dictionary<string, int>[,] mismatchFieldCountsBySection =
    new Dictionary<string, int>[sectionColumns, sectionRows];
  Dictionary<string, int> mismatchFieldCounts = CreateMismatchFieldCounts();
  Dictionary<string, int> extendedStateMismatchFieldCounts =
    CreateExtendedStateMismatchFieldCounts();
  List<LegacySpatialRegionAccumulator> spatialRegions = CreateSpatialRegions(
    legacyWorldSurfaceY,
    legacyRockLayerY,
    metadata.Height);
  int comparedTiles = 0;
  int mismatchTiles = 0;
  int extendedStateMismatchTiles = 0;
  int activeLegacyTiles = 0;
  int activeGeneratedTiles = 0;
  for (int index = 0; index < legacy.Tiles.Count; index++)
  {
    int x = index / metadata.Height;
    int y = index % metadata.Height;
    LegacyTile expected = legacy.Tiles[index];
    WorldTile actual = generated.GetTile(x, y);
    WorldSectionCoordinates section = new(
      x / WorldGrid.SectionWidth,
      y / WorldGrid.SectionHeight);
    Dictionary<string, int>? sectionMismatchFieldCounts =
      mismatchFieldCountsBySection[section.X, section.Y];
    if (sectionMismatchFieldCounts is null)
    {
      sectionMismatchFieldCounts = CreateMismatchFieldCounts();
      mismatchFieldCountsBySection[section.X, section.Y] = sectionMismatchFieldCounts;
    }

    LegacySpatialRegionAccumulator spatialRegion = spatialRegions[
      GetSpatialRegionIndex(y, legacyWorldSurfaceY, legacyRockLayerY)];
    bool hasMismatch = CountTileStateMismatches(
      expected,
      actual,
      mismatchFieldCounts,
      sectionMismatchFieldCounts,
      spatialRegion.MismatchFieldCounts);
    if (expected.IsActive)
    {
      activeLegacyTiles++;
    }

    if (actual.IsActive)
    {
      activeGeneratedTiles++;
    }

    comparedTiles++;
    spatialRegion.ComparedTiles++;
    if (hasMismatch)
    {
      mismatchTiles++;
      mismatchesBySection[section.X, section.Y]++;
      spatialRegion.MismatchTiles++;
    }

    if (CountExtendedStateMismatches(
          expected,
          actual,
          extendedStateMismatchFieldCounts))
    {
      extendedStateMismatchTiles++;
      extendedStateMismatchesBySection[section.X, section.Y]++;
    }
  }

  List<LegacySpatialRegionDifference> regions = spatialRegions
    .Select(region => region.ToDifference())
    .ToList();
  List<LegacySectionDifference> sections = new(sectionColumns * sectionRows);
  for (int y = 0; y < sectionRows; y++)
  {
    for (int x = 0; x < sectionColumns; x++)
    {
      sections.Add(new LegacySectionDifference(
        x,
        y,
        mismatchesBySection[x, y],
        extendedStateMismatchesBySection[x, y],
        mismatchFieldCountsBySection[x, y] ?? CreateMismatchFieldCounts()));
    }
  }

  LegacyDifferentialEvidence evidence = new(
    legacyPath,
    metadata.Width,
    metadata.Height,
    spawnX,
    surfaceY,
    spawnSource,
    surfaceYSource,
    legacy.Metadata.SpawnX,
    legacy.Metadata.SpawnY,
    legacyWorldSurfaceY,
    legacyRockLayerY,
    dirtWallOffsetsPath,
    dirtWallOffsetsPath is null
      ? null
      : Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(dirtWallOffsetsPath))),
    dirtWallSurfaceOffsetChanges?.Count,
    generationRequest.RockLayerY,
    comparedTiles,
    mismatchTiles,
    mismatchFieldCounts,
    extendedStateMismatchTiles,
    extendedStateMismatchFieldCounts,
    activeLegacyTiles,
    activeGeneratedTiles,
    CreateLegacyOracleFingerprint(legacy),
    CreateWorldGridFingerprint(generated),
    sections,
    regions);
  string evidenceDirectory = Path.Combine(repositoryRoot, "docs", "worldgen");
  Directory.CreateDirectory(evidenceDirectory);
  File.WriteAllText(
    Path.Combine(evidenceDirectory, "legacy-worldgen-differential.json"),
    JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
  Console.WriteLine(
    $"DIFF: legacy oracle compared {comparedTiles} tiles, mismatches {mismatchTiles}, " +
    $"extended-state mismatches {extendedStateMismatchTiles}");
}

static int GetLegacyLayerY(double layer, int worldHeight, string parameterName)
{
  if (!double.IsFinite(layer))
  {
    throw new ArgumentOutOfRangeException(
      parameterName,
      "The legacy WLD layer value must be finite.");
  }

  int layerY = (int)layer;
  if (layerY < 0 || layerY > worldHeight)
  {
    throw new ArgumentOutOfRangeException(
      parameterName,
      "The legacy WLD layer value is outside the world bounds.");
  }

  return layerY;
}

static List<LegacySpatialRegionAccumulator> CreateSpatialRegions(
  int worldSurfaceY,
  int rockLayerY,
  int worldHeight)
{
  return new List<LegacySpatialRegionAccumulator>
  {
    new("AboveWorldSurface", 0, worldSurfaceY, CreateMismatchFieldCounts()),
    new("SurfaceToRockLayer", worldSurfaceY, rockLayerY, CreateMismatchFieldCounts()),
    new("BelowRockLayer", rockLayerY, worldHeight, CreateMismatchFieldCounts())
  };
}

static int GetSpatialRegionIndex(int y, int worldSurfaceY, int rockLayerY)
{
  if (y < worldSurfaceY)
  {
    return 0;
  }

  if (y < rockLayerY)
  {
    return 1;
  }

  return 2;
}

static bool CountTileStateMismatches(
  LegacyTile expected,
  WorldTile actual,
  Dictionary<string, int> mismatchFieldCounts,
  Dictionary<string, int> sectionMismatchFieldCounts,
  Dictionary<string, int> regionMismatchFieldCounts)
{
  bool hasMismatch = false;
  IncrementMismatch(expected.IsActive != actual.IsActive, "IsActive", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.TileType != actual.Type, "TileType", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.LiquidAmount != actual.LiquidAmount, "LiquidAmount", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.LiquidKind != actual.LiquidType, "LiquidKind", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.FrameX != actual.FrameX, "FrameX", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.FrameY != actual.FrameY, "FrameY", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.WallType != actual.WallType, "WallType", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.HasWire != actual.HasWire, "Wire", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.HasWire2 != actual.HasWire2, "Wire2", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.HasWire3 != actual.HasWire3, "Wire3", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.HasWire4 != actual.HasWire4, "Wire4", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsHalfBrick != actual.IsHalfBrick, "HalfBrick", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.Slope != actual.Slope, "Slope", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsActuated != actual.IsActuated, "Actuated", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsInactive != actual.IsInactive, "Inactive", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.TileColor != actual.TileColor, "TileColor", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.WallColor != actual.WallColor, "WallColor", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsInvisibleBlock != actual.IsInvisibleBlock, "InvisibleBlock",
    mismatchFieldCounts, sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsInvisibleWall != actual.IsInvisibleWall, "InvisibleWall",
    mismatchFieldCounts, sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsFullbrightBlock != actual.IsFullbrightBlock, "FullbrightBlock",
    mismatchFieldCounts, sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsFullbrightWall != actual.IsFullbrightWall, "FullbrightWall",
    mismatchFieldCounts, sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  return hasMismatch;
}

static Dictionary<string, int> CreateMismatchFieldCounts()
{
  return new Dictionary<string, int>(StringComparer.Ordinal)
  {
    ["Actuated"] = 0,
    ["FrameX"] = 0,
    ["FrameY"] = 0,
    ["FullbrightBlock"] = 0,
    ["FullbrightWall"] = 0,
    ["HalfBrick"] = 0,
    ["Inactive"] = 0,
    ["InvisibleBlock"] = 0,
    ["InvisibleWall"] = 0,
    ["IsActive"] = 0,
    ["LiquidAmount"] = 0,
    ["LiquidKind"] = 0,
    ["Slope"] = 0,
    ["TileColor"] = 0,
    ["TileType"] = 0,
    ["WallColor"] = 0,
    ["WallType"] = 0,
    ["Wire"] = 0,
    ["Wire2"] = 0,
    ["Wire3"] = 0,
    ["Wire4"] = 0
  };
}

static void IncrementMismatch(
  bool isMismatch,
  string fieldName,
  Dictionary<string, int> mismatchFieldCounts,
  Dictionary<string, int> sectionMismatchFieldCounts,
  Dictionary<string, int> regionMismatchFieldCounts,
  ref bool hasMismatch)
{
  if (!isMismatch)
  {
    return;
  }

  mismatchFieldCounts[fieldName]++;
  sectionMismatchFieldCounts[fieldName]++;
  regionMismatchFieldCounts[fieldName]++;
  hasMismatch = true;
}

static bool CountExtendedStateMismatches(
  LegacyTile expected,
  WorldTile actual,
  Dictionary<string, int> extendedStateMismatchFieldCounts)
{
  bool hasMismatch = false;
  IncrementExtendedStateMismatch(expected.WallType != actual.WallType, "WallType",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.TileColor != actual.TileColor, "TileColor",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.WallColor != actual.WallColor, "WallColor",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsInvisibleBlock != actual.IsInvisibleBlock,
    "InvisibleBlock", extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsInvisibleWall != actual.IsInvisibleWall,
    "InvisibleWall", extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsFullbrightBlock != actual.IsFullbrightBlock,
    "FullbrightBlock", extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsFullbrightWall != actual.IsFullbrightWall,
    "FullbrightWall", extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.HasWire != actual.HasWire, "Wire",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.HasWire2 != actual.HasWire2, "Wire2",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.HasWire3 != actual.HasWire3, "Wire3",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.HasWire4 != actual.HasWire4, "Wire4",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsHalfBrick != actual.IsHalfBrick, "HalfBrick",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.Slope != actual.Slope, "Slope",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsActuated != actual.IsActuated, "Actuated",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsInactive != actual.IsInactive, "Inactive",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  return hasMismatch;
}

static Dictionary<string, int> CreateExtendedStateMismatchFieldCounts()
{
  return new Dictionary<string, int>(StringComparer.Ordinal)
  {
    ["Actuated"] = 0,
    ["FullbrightBlock"] = 0,
    ["FullbrightWall"] = 0,
    ["HalfBrick"] = 0,
    ["Inactive"] = 0,
    ["InvisibleBlock"] = 0,
    ["InvisibleWall"] = 0,
    ["Slope"] = 0,
    ["TileColor"] = 0,
    ["WallColor"] = 0,
    ["WallType"] = 0,
    ["Wire"] = 0,
    ["Wire2"] = 0,
    ["Wire3"] = 0,
    ["Wire4"] = 0
  };
}

static void IncrementExtendedStateMismatch(
  bool isMismatch,
  string fieldName,
  Dictionary<string, int> extendedStateMismatchFieldCounts,
  ref bool hasMismatch)
{
  if (!isMismatch)
  {
    return;
  }

  extendedStateMismatchFieldCounts[fieldName]++;
  hasMismatch = true;
}

static string CreateWorldGridFingerprint(WorldGrid world)
{
  using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
  AppendInt32(hash, world.Width);
  AppendInt32(hash, world.Height);
  Span<byte> tileBuffer = stackalloc byte[16];
  for (int x = 0; x < world.Width; x++)
  {
    for (int y = 0; y < world.Height; y++)
    {
      WorldTile tile = world.GetTile(x, y);
      int offset = 0;
      tileBuffer[offset++] = tile.IsActive ? (byte)1 : (byte)0;
      BinaryPrimitives.WriteUInt16LittleEndian(tileBuffer[offset..], tile.Type);
      offset += sizeof(ushort);
      tileBuffer[offset++] = tile.LiquidAmount;
      tileBuffer[offset++] = tile.LiquidType;
      BinaryPrimitives.WriteInt16LittleEndian(tileBuffer[offset..], tile.FrameX);
      offset += sizeof(short);
      BinaryPrimitives.WriteInt16LittleEndian(tileBuffer[offset..], tile.FrameY);
      offset += sizeof(short);
      hash.AppendData(tileBuffer[..offset]);
    }
  }

  return Convert.ToHexString(hash.GetHashAndReset());
}

static void AppendInt32(IncrementalHash hash, int value)
{
  Span<byte> buffer = stackalloc byte[sizeof(int)];
  BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
  hash.AppendData(buffer);
}

static void AppendText(IncrementalHash hash, object? value)
{
  byte[] bytes = Encoding.UTF8.GetBytes(value?.ToString() ?? string.Empty);
  hash.AppendData(bytes);
}

static Dictionary<string, long> GetSectionVersions(WorldGridSnapshot snapshot)
{
  Dictionary<string, long> versions = new(StringComparer.Ordinal);
  for (int y = 0; y < snapshot.Metadata.Height / WorldGrid.SectionHeight; y++)
  {
    for (int x = 0; x < snapshot.Metadata.Width / WorldGrid.SectionWidth; x++)
    {
      WorldSectionCoordinates coordinates = new(x, y);
      versions[$"{x},{y}"] = snapshot.GetSectionVersion(coordinates);
    }
  }

  return versions;
}

static string CreateMethodMap(WorldgenInventory inventory)
{
  StringBuilder builder = new();
  builder.AppendLine("# WorldGen method map");
  builder.AppendLine();
  builder.AppendLine($"- Source: `{inventory.SourcePath}`");
  builder.AppendLine($"- SHA-256: `{inventory.Source.Sha256}`");
  builder.AppendLine();
  builder.AppendLine(
    "Entries remain `Unmapped` unless a source-line mapping records target scope and exclusions.");
  builder.AppendLine();
  builder.AppendLine("| Line | Visibility | Name | Candidate domain | Status | References |");
  builder.AppendLine("| ---: | --- | --- | --- | --- | --- |");
  foreach (MethodInventory method in inventory.Methods)
  {
    builder.AppendLine(
      $"| {method.Line} | {method.Visibility} | `{method.Name}` | {method.Domain} | " +
      $"{method.Status} | {string.Join(", ", method.References)} |");
  }

  return builder.ToString();
}

static void AssertThrows<TException>(Action action)
  where TException : Exception
{
  try
  {
    action();
  }
  catch (TException)
  {
    return;
  }

  throw new InvalidOperationException(
    $"Expected {typeof(TException).Name} was not thrown.");
}

static string FindRepositoryRoot()
{
  DirectoryInfo? directory = new(AppContext.BaseDirectory);
  while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
  {
    directory = directory.Parent;
  }

  return directory?.FullName ?? throw new InvalidOperationException(
    "Repository root was not found.");
}

internal sealed record WorldgenInventory(
  string SourceName,
  string SourcePath,
  FileEvidence Source,
  IReadOnlyList<MethodInventory> Methods,
  IReadOnlyList<FieldInventory> Fields,
  IReadOnlyList<ReferenceInventory> References,
  ReplayEvidence Replay);

internal sealed record FileEvidence(long Bytes, int Lines, string Sha256, string ScannerVersion);

internal sealed record MethodInventory(
  string Name,
  string Visibility,
  int Line,
  string Domain,
  string Status,
  IReadOnlyList<string> References,
  SourceMapping? Mapping);

internal sealed record SourceMapping(
  string Status,
  IReadOnlyList<string> TargetMembers,
  string ImplementedScope,
  IReadOnlyList<string> ExcludedLegacyResponsibilities,
  string Verification);

internal sealed record FieldInventory(
  string Name,
  string Visibility,
  int Line,
  bool IsStatic,
  string Domain,
  string Status,
  IReadOnlyList<string> References);

internal sealed record ReferenceInventory(string Name, int Count, string Status);

internal sealed record ReplayEvidence(
  int Width,
  int Height,
  int Seed,
  int SpawnX,
  int SurfaceY,
  string SnapshotFingerprint,
  IReadOnlyDictionary<string, long> SectionVersions);

internal sealed record LegacyOracleEvidence(
  string Path,
  long Bytes,
  string Sha256,
  int Version,
  string FormatVersion,
  LegacyWorldMetadata Metadata,
  int TileCount,
  int LiquidTileCount,
  string Fingerprint);

internal sealed record CursorRestartEvidence(
  string CheckpointStage,
  string ResumedStage,
  int SectionX,
  int SectionY,
  uint RandomState,
  string SnapshotFingerprint,
  long UninterruptedNextSequence,
  long RestartedNextSequence);

internal sealed record LegacyDifferentialEvidence(
  string LegacyPath,
  int Width,
  int Height,
  int SpawnX,
  int SurfaceY,
  string SpawnSource,
  string SurfaceYSource,
  int LegacyMetadataSpawnX,
  int LegacyMetadataSpawnY,
  int LegacyWorldSurfaceY,
  int LegacyRockLayerY,
  string? DirtWallOffsetArtifactPath,
  string? DirtWallOffsetArtifactSha256,
  int? DirtWallOffsetCount,
  int EcsInputRockLayerY,
  int ComparedTiles,
  int MismatchTiles,
  IReadOnlyDictionary<string, int> MismatchFieldCounts,
  int ExtendedStateMismatchTiles,
  IReadOnlyDictionary<string, int> ExtendedStateMismatchFieldCounts,
  int ActiveLegacyTiles,
  int ActiveGeneratedTiles,
  string LegacyFingerprint,
  string GeneratedFingerprint,
  IReadOnlyList<LegacySectionDifference> Sections,
  IReadOnlyList<LegacySpatialRegionDifference> Regions);

internal sealed record LegacySectionDifference(
  int SectionX,
  int SectionY,
  int MismatchTiles,
  int ExtendedStateMismatchTiles,
  IReadOnlyDictionary<string, int> MismatchFieldCounts);

internal sealed record LegacySpatialRegionDifference(
  string Name,
  int StartY,
  int EndExclusiveY,
  int ComparedTiles,
  int MismatchTiles,
  IReadOnlyDictionary<string, int> MismatchFieldCounts);

internal sealed class LegacySpatialRegionAccumulator
{
  public LegacySpatialRegionAccumulator(
    string name,
    int startY,
    int endExclusiveY,
    Dictionary<string, int> mismatchFieldCounts)
  {
    Name = name;
    StartY = startY;
    EndExclusiveY = endExclusiveY;
    MismatchFieldCounts = mismatchFieldCounts;
  }

  public int ComparedTiles { get; set; }

  public int EndExclusiveY { get; }

  public int MismatchTiles { get; set; }

  public Dictionary<string, int> MismatchFieldCounts { get; }

  public string Name { get; }

  public int StartY { get; }

  public LegacySpatialRegionDifference ToDifference()
  {
    return new LegacySpatialRegionDifference(
      Name,
      StartY,
      EndExclusiveY,
      ComparedTiles,
      MismatchTiles,
      MismatchFieldCounts);
  }
}
