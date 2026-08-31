using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.Json;
using Terraria.Dome.Server;
using Terraria.Dome.Server.Import;
using Terraria.Dome.Server.Startup;
using Terraria.Dome.Protocol.V1456.Compatibility;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.WorldCompatibility.Model;
using Terraria.WorldCompatibility.Projection;
using Terraria.WorldFile.V319.Format;
using Terraria.WorldFile.V319.Model;

string? fullBaselineArgument = args.FirstOrDefault(
  argument => argument.StartsWith("--full-baseline=", StringComparison.Ordinal));
if (fullBaselineArgument is not null)
{
  string fullBaselinePath = fullBaselineArgument["--full-baseline=".Length..];
  if (!Path.IsPathFullyQualified(fullBaselinePath))
  {
    throw new ArgumentException("The full baseline path must be absolute.", nameof(args));
  }

  VerifyFullBaselineTileProjection(fullBaselinePath);
  Environment.Exit(0);
}

VerifyOptions();
VerifyImportFailureDoesNotStartServer();
VerifyDefaultBootstrapIsDeterministic();
VerifyCompatibilityProjectionPreservesLegacyTileState();
VerifyWorldSurfaceProjection();
VerifyWorldRemixProjection();
VerifyNoTrapsWorldProjection();
VerifySkyblockWorldProjection();
VerifyGoodWorldProjection();
VerifyWorldGeneratorVersionProjection();
VerifyWorldUniqueIdProjection();
VerifyWorldSeedTextProjection();
VerifyWorldWindProjection();
VerifyWorldRainProjection();
VerifyWorldTimeAndEndlessRainRepairProjection();
VerifyMoonPhaseProjection();
VerifyWorldEventFlagsProjection();
VerifyCrimsonWorldProjection();
VerifyHardModeProjection();
VerifyProgressionFactsProjection();
VerifyInvasionFactsProjection();
VerifyGameModeProjection();
VerifyMeteorScheduleProjection();
VerifyTemporaryPortLifecycle();
VerifyWorldStateContracts();
LocalWorldAcceptance.RunIfConfigured();
Console.WriteLine("PASS: strict world import options and no-listener failure lifecycle");

static void VerifyNoTrapsWorldProjection()
{
  LegacyWorldDocument legacy = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new LegacyWorldMetadata(
      "no-traps",
      1456,
      400,
      300,
      200,
      75,
      IsNoTrapsWorld: true),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  CompatibilityWorldSnapshot source = WldToCompatibilityProjection.Project(legacy);
  if (source.Metadata.IsNoTrapsWorld != true)
  {
    throw new InvalidOperationException(
      "WLD compatibility projection did not preserve the no-traps world flag.");
  }

  DomeSimulationSnapshot projected = CompatibilityToDomeProjection.Project(
    source,
    new WorldSeed(1456));
  if (projected.World.Metadata.IsNoTrapsWorld != true)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the no-traps world flag.");
  }

  Console.WriteLine("PASS: no-traps world metadata import projection");
}

static void VerifySkyblockWorldProjection()
{
  LegacyWorldDocument legacy = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new LegacyWorldMetadata(
      "skyblock",
      1457,
      400,
      300,
      200,
      75,
      IsSkyblockWorld: true),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  CompatibilityWorldSnapshot source = WldToCompatibilityProjection.Project(legacy);
  DomeSimulationSnapshot projected = CompatibilityToDomeProjection.Project(
    source,
    new WorldSeed(1457));
  WorldGenerationRequest request = new(
    projected.World.Metadata,
    spawnX: 200,
    surfaceY: 75);
  if (source.Metadata.IsSkyblockWorld != true ||
      projected.World.Metadata.IsSkyblockWorld != true || !request.IsSkyblockWorld)
  {
    throw new InvalidOperationException(
      "Skyblock world metadata did not reach the generation request.");
  }

  Console.WriteLine("PASS: skyblock world metadata and generation-request projection");
}

static void VerifyGoodWorldProjection()
{
  LegacyWorldDocument legacy = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new LegacyWorldMetadata(
      "good-world",
      1458,
      400,
      300,
      200,
      75,
      IsGoodWorld: true),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  CompatibilityWorldSnapshot source = WldToCompatibilityProjection.Project(legacy);
  DomeSimulationSnapshot projected = CompatibilityToDomeProjection.Project(
    source,
    new WorldSeed(1458));
  WorldGenerationRequest request = new(
    projected.World.Metadata,
    spawnX: 200,
    surfaceY: 75);
  if (source.Metadata.IsGoodWorld != true ||
      projected.World.Metadata.IsGoodWorld != true ||
      !request.IsGoodWorld)
  {
    throw new InvalidOperationException(
      "Good-world metadata did not reach the compatibility projection and generation request.");
  }

  Console.WriteLine("PASS: good-world metadata and generation-request projection");
}

static void VerifyWorldTimeAndEndlessRainRepairProjection()
{
  const int width = 400;
  const int height = 300;
  CompatibilityWorldSnapshot fractionalClock = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new CompatibilityWorldMetadata(
      "fractional-clock",
      1,
      width,
      height,
      200,
      75,
      IsRaining: false,
      RainTimeTicks: 120,
      MaximumRainStrength: 0.75f,
      TimeOfDay: 1.5d,
      IsDayTime: false),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    loadReport: new CompatibilityLoadReport([]));
  DomeSimulationSnapshot projected = CompatibilityToDomeProjection.Project(
    fractionalClock,
    new WorldSeed(1456));
  if (projected.Clock.TimeOfDay != 1.5d || projected.Clock.IsDayTime ||
      projected.WorldRules.IsRaining || projected.WorldRules.RainTimeTicks != 120 ||
      projected.WorldRules.MaximumRainStrength != 0.75f)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve fractional WLD time and raw weather facts.");
  }


  CompatibilityWorldSnapshot endlessRain = new(
    version: 317,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: fractionalClock.Metadata with
    {
      IsRaining = true,
      RainTimeTicks = 5184000,
      MaximumRainStrength = 0.5f
    },
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    loadReport: new CompatibilityLoadReport([]));
  ExpectProjectionFailure(() => CompatibilityToDomeProjection.Project(
    endlessRain,
    new WorldSeed(1456)));

  DomeSimulationSnapshot repaired = CompatibilityToDomeProjection.Project(
    endlessRain,
    new WorldSeed(1456),
    isRainsForAYearSecretSeedActive: false);
  if (repaired.WorldRules.IsRaining || repaired.WorldRules.RainTimeTicks != 0 ||
      repaired.WorldRules.MaximumRainStrength != 0.0f)
  {
    throw new InvalidOperationException(
      "The confirmed non-secret endless-rain repair did not clear all values.");
  }

  DomeSimulationSnapshot preserved = CompatibilityToDomeProjection.Project(
    endlessRain,
    new WorldSeed(1456),
    isRainsForAYearSecretSeedActive: true);
  if (!preserved.WorldRules.IsRaining || !preserved.WorldRules.IsRainingForever ||
      preserved.WorldRules.RainTimeTicks != WorldRuleState.EndlessRainThresholdTicks ||
      preserved.WorldRules.MaximumRainStrength != 0.5f)
  {
    throw new InvalidOperationException(
      "The active endless-rain secret seed did not preserve raw weather.");
  }

  CompatibilityWorldSnapshot invalidTime = new(
    fractionalClock.Version,
    fractionalClock.FormatVersion,
    fractionalClock.Metadata with { TimeOfDay = double.NaN },
    fractionalClock.Tiles,
    fractionalClock.Chests,
    fractionalClock.Signs,
    fractionalClock.Npcs,
    fractionalClock.TileEntities,
    fractionalClock.LoadReport);
  ExpectProjectionFailure(() => CompatibilityToDomeProjection.Project(
    invalidTime,
    new WorldSeed(1456)));
  Console.WriteLine(
    "PASS: WLD time/weather projection is lossless and endless-rain repair fails closed");
}

static void ExpectProjectionFailure(Action action)
{
  try
  {
    action();
    throw new InvalidOperationException("An unsupported WLD projection was accepted.");
  }
  catch (InvalidOperationException)
  {
  }
}

static void VerifyWorldStateContracts()
{
  WorldMetadata first = new(
    "contract-world",
    new WorldSeed(10),
    400,
    300,
    worldId: 44,
    spawnX: 180,
    spawnY: 75,
    seedVariant: "for-the-worthy",
    randomStreamVersion: 2);
  WorldMetadata equivalent = new(
    "contract-world",
    new WorldSeed(10),
    400,
    300,
    worldId: 44,
    spawnX: 180,
    spawnY: 75,
    seedVariant: "for-the-worthy",
    randomStreamVersion: 2);
  WorldMetadata differentSeed = new(
    "contract-world",
    new WorldSeed(11),
    400,
    300,
    worldId: 44,
    spawnX: 180,
    spawnY: 75,
    seedVariant: "for-the-worthy",
    randomStreamVersion: 2);
  if (first != equivalent || first == differentSeed ||
      first.SeedVariant != "for-the-worthy" || first.RandomStreamVersion != 2)
  {
    throw new InvalidOperationException("World metadata did not preserve deterministic seed inputs.");
  }

  ExpectInvalid(() => new WorldMetadata("invalid", new WorldSeed(1), 401, 300));
  ExpectInvalid(() => new WorldRuleState(difficulty: -1));
  ExpectInvalid(() => new WorldProgressionState(invasionType: -1));
}

static void ExpectInvalid(Action action)
{
  try
  {
    action();
    throw new InvalidOperationException("An invalid world-state value was accepted.");
  }
  catch (ArgumentException)
  {
  }
}

static void VerifyOptions()
{
  string path = Path.Combine(Path.GetTempPath(), "strict-world.wld");
  ServerLaunchOptions options = ServerLaunchOptions.Parse(["--world", path, "--port", "18081"]);
  if (!Path.IsPathFullyQualified(options.WorldPath) || options.Port != 18081)
  {
    throw new InvalidOperationException("Valid world import options were not accepted.");
  }

  string[][] invalidArguments =
  [
    ["--world", path],
    ["--port", "18081"],
    ["--world", path, "--world", path, "--port", "18081"],
    ["--world", path, "--port", "18081", "--port", "18082"],
    ["--world", path, "--port", "7778"],
    ["--world", path, "--port", "0"],
    ["--world", path, "--port", "65536"],
    ["--world", "relative.wld", "--port", "18081"],
    ["--unknown", "value", "--world", path, "--port", "18081"]
  ];
  for (int index = 0; index < invalidArguments.Length; index++)
  {
    try
    {
      _ = ServerLaunchOptions.Parse(invalidArguments[index]);
      throw new InvalidOperationException("An invalid import argument set was accepted.");
    }
    catch (ArgumentException)
    {
    }
  }
}

static void VerifyImportFailureDoesNotStartServer()
{
  string path = Path.Combine(Path.GetTempPath(), $"invalid-world-{Guid.NewGuid():N}.wld");
  File.WriteAllBytes(path, [0x40, 0x00, 0x00, 0x00]);
  try
  {
    try
    {
      _ = new DomeWorldImportApplier().Import(path);
      throw new InvalidOperationException("Malformed world input was accepted.");
    }
    catch (InvalidDataException)
    {
    }
  }
  finally
  {
    File.Delete(path);
  }
}

static void VerifyDefaultBootstrapIsDeterministic()
{
  WorldBootstrapResult first = WorldBootstrap.CreateDefault();
  WorldBootstrapResult second = WorldBootstrap.CreateDefault();
  if (!first.UsesDefaultWorld || !second.UsesDefaultWorld ||
      first.Snapshot.World.Metadata != second.Snapshot.World.Metadata ||
      first.Snapshot.Chests.Count != 23 ||
      second.Snapshot.Chests.Count != first.Snapshot.Chests.Count)
  {
    throw new InvalidOperationException(
      "Default world bootstrap was not deterministic or did not retain server-owned objects.");
  }

  WorldTile firstTile = first.Snapshot.World.GetTile(0, 0);
  WorldTile secondTile = second.Snapshot.World.GetTile(0, 0);
  if (firstTile != secondTile ||
      first.Snapshot.World.Metadata.SpawnX != 2100 ||
      first.Snapshot.World.Metadata.SpawnY != 300)
  {
    throw new InvalidOperationException(
      "Default world bootstrap did not preserve deterministic metadata and tile state.");
  }

  using DomeServer server = new();
  DomeSimulationSnapshot serverSnapshot = server.CreatePersistenceSnapshot(
    first.Snapshot.World.Metadata);
  if (serverSnapshot.Chests.Count != first.Snapshot.Chests.Count)
  {
    throw new InvalidOperationException(
      "DomeServer recreated default world objects after bootstrap.");
  }

  string missingPath = Path.Combine(Path.GetTempPath(), $"missing-world-{Guid.NewGuid():N}.wld");
  try
  {
    _ = WorldBootstrap.Load(missingPath);
    throw new InvalidOperationException("Missing world input was accepted by bootstrap.");
  }
  catch (FileNotFoundException)
  {
  }
}

static void VerifyCompatibilityProjectionPreservesLegacyTileState()
{
  const int height = 300;
  const int width = 400;
  const int tileX = 19;
  const int tileY = 23;
  CompatibilityTile[] tiles = new CompatibilityTile[width * height];
  for (int x = 0; x < width; x++)
  {
    for (int y = 0; y < height; y++)
    {
      tiles[(x * height) + y] = new CompatibilityTile(x, y, false, 0);
    }
  }

  tiles[(tileX * height) + tileY] = new CompatibilityTile(
    tileX,
    tileY,
    IsActive: true,
    TileType: 321,
    WallType: 257,
    LiquidAmount: 200,
    LiquidKind: 3,
    HasWire: true,
    HasWire2: true,
    HasWire3: true,
    HasWire4: true,
    IsHalfBrick: false,
    Slope: 4,
    IsActuated: true,
    IsInactive: true,
    FrameX: 18,
    FrameY: 36,
    TileColor: 7,
    WallColor: 8,
    IsInvisibleBlock: true,
    IsInvisibleWall: true,
    IsFullbrightBlock: true,
    IsFullbrightWall: true);
  CompatibilityWorldSnapshot compatibility = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new CompatibilityWorldMetadata("projection", 1, width, height, 200, 75),
    tiles: tiles,
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    loadReport: new CompatibilityLoadReport([]));

  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  WorldTile actual = snapshot.World.GetTile(tileX, tileY);
  if (!actual.IsActive ||
      actual.Type != 321 ||
      actual.WallType != 257 ||
      actual.LiquidAmount != 200 ||
      actual.LiquidType != 3 ||
      !actual.HasWire ||
      !actual.HasWire2 ||
      !actual.HasWire3 ||
      !actual.HasWire4 ||
      actual.IsHalfBrick ||
      actual.Slope != 4 ||
      !actual.IsActuated ||
      !actual.IsInactive ||
      actual.FrameX != 18 ||
      actual.FrameY != 36 ||
      actual.TileColor != 7 ||
      actual.WallColor != 8 ||
      !actual.IsInvisibleBlock ||
      !actual.IsInvisibleWall ||
      !actual.IsFullbrightBlock ||
      !actual.IsFullbrightWall)
  {
    throw new InvalidOperationException(
      "Compatibility projection discarded a legacy tile state field.");
  }
}

static void VerifyWorldSurfaceProjection()
{
  const double expectedWorldSurface = 123.5;
  LegacyWorldDocument document = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new LegacyWorldMetadata(
      "world-surface",
      1456,
      400,
      300,
      200,
      75,
      WorldSurface: expectedWorldSurface),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  CompatibilityWorldSnapshot compatibility = WldToCompatibilityProjection.Project(document);
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  if (snapshot.World.Metadata.WorldSurface != expectedWorldSurface)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the legacy world surface.");
  }
}

static void VerifyWorldWindProjection()
{
  const float expectedWind = -0.35f;
  CompatibilityWorldSnapshot compatibility = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new CompatibilityWorldMetadata(
      "wind",
      1,
      400,
      300,
      200,
      75,
      WindSpeedTarget: expectedWind),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    loadReport: new CompatibilityLoadReport([]));
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  if (snapshot.WorldRules.WindSpeedTarget != expectedWind ||
      snapshot.WorldRules.WindSpeedCurrent != expectedWind)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the modern WLD wind target/current relation.");
  }
}

static void VerifyWorldRainProjection()
{
  CompatibilityWorldSnapshot modernCompatibility = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new CompatibilityWorldMetadata(
      "rain",
      1,
      400,
      300,
      200,
      75,
      IsRaining: true,
      RainTimeTicks: 120,
      MaximumRainStrength: 0.75f),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    loadReport: new CompatibilityLoadReport([]));
  DomeSimulationSnapshot modernSnapshot = CompatibilityToDomeProjection.Project(
    modernCompatibility,
    new WorldSeed(1456));
  if (!modernSnapshot.WorldRules.IsRaining ||
      modernSnapshot.WorldRules.RainTimeTicks != 120 ||
      modernSnapshot.WorldRules.MaximumRainStrength != 0.75f ||
      modernSnapshot.WorldRules.RainStrength != 0.0f)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the raw WLD rain facts.");
  }

  CompatibilityWorldSnapshot unavailableRepairCompatibility = new(
    version: 317,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: modernCompatibility.Metadata with { RainTimeTicks = 5184000 },
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    loadReport: new CompatibilityLoadReport([]));
  ExpectImportRejected(
    unavailableRepairCompatibility,
    "legacy rain repair without secret-seed context");
}

static void ExpectImportRejected(CompatibilityWorldSnapshot compatibility, string message)
{
  try
  {
    _ = CompatibilityToDomeProjection.Project(compatibility, new WorldSeed(1456));
    throw new InvalidOperationException("An unsupported compatibility import was accepted.");
  }
  catch (InvalidOperationException exception) when (
    exception.Message.Contains(message, StringComparison.Ordinal))
  {
  }
}

static void VerifyWorldRemixProjection()
{
  CompatibilityWorldSnapshot compatibility = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new CompatibilityWorldMetadata(
      "remix",
      1,
      400,
      300,
      200,
      75,
      WorldSurface: 100,
      RockLayer: 210,
      IsRemixWorld: true),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    loadReport: new CompatibilityLoadReport([]));
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  if (snapshot.World.Metadata.IsRemixWorld != true ||
      snapshot.World.Metadata.WorldSurface != 100 ||
      snapshot.World.Metadata.RockLayer != 210)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the authoritative Remix world metadata.");
  }

  CompatibilityWorldSnapshot unknownCompatibility = new(
    compatibility.Version,
    compatibility.FormatVersion,
    new CompatibilityWorldMetadata(
      "remix",
      1,
      400,
      300,
      200,
      75,
      WorldSurface: 100),
    compatibility.Tiles,
    compatibility.Chests,
    compatibility.Signs,
    compatibility.Npcs,
    compatibility.TileEntities,
    compatibility.LoadReport);
  DomeSimulationSnapshot unknownSnapshot = CompatibilityToDomeProjection.Project(
    unknownCompatibility,
    new WorldSeed(1456));
  if (unknownSnapshot.World.Metadata.IsRemixWorld is not null)
  {
    throw new InvalidOperationException(
      "Compatibility projection fabricated a missing Remix world value.");
  }
}

static void VerifyWorldGeneratorVersionProjection()
{
  const ulong expectedGeneratorVersion = 1370094567425UL;
  CompatibilityWorldSnapshot compatibility = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new CompatibilityWorldMetadata(
      "generator-version",
      1,
      400,
      300,
      200,
      75,
      WorldGeneratorVersion: expectedGeneratorVersion),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    loadReport: new CompatibilityLoadReport([]));
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  if (snapshot.World.Metadata.WorldGeneratorVersion != expectedGeneratorVersion)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the WLD generator version.");
  }

  CompatibilityWorldSnapshot unknown = new(
    compatibility.Version,
    compatibility.FormatVersion,
    compatibility.Metadata with { WorldGeneratorVersion = null },
    compatibility.Tiles,
    compatibility.Chests,
    compatibility.Signs,
    compatibility.Npcs,
    compatibility.TileEntities,
    compatibility.LoadReport);
  DomeSimulationSnapshot unknownSnapshot = CompatibilityToDomeProjection.Project(
    unknown,
    new WorldSeed(1456));
  if (unknownSnapshot.World.Metadata.WorldGeneratorVersion is not null)
  {
    throw new InvalidOperationException(
      "Compatibility projection fabricated a missing WLD generator version.");
  }
}

static void VerifyWorldUniqueIdProjection()
{
  Guid expectedUniqueId = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff");
  CompatibilityWorldSnapshot compatibility = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new CompatibilityWorldMetadata(
      "unique-id",
      1,
      400,
      300,
      200,
      75,
      UniqueId: expectedUniqueId),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    loadReport: new CompatibilityLoadReport([]));
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  if (snapshot.World.Metadata.UniqueId != expectedUniqueId)
  {
    throw new InvalidOperationException("Compatibility projection did not preserve the WLD UUID.");
  }

  LegacyWorldDataContext context = LegacyWorldDataContext.FromWorldMetadata(snapshot.World.Metadata);
  if (context.UniqueId != expectedUniqueId)
  {
    throw new InvalidOperationException("Protocol projection did not preserve the WLD UUID.");
  }
}

static void VerifyWorldSeedTextProjection()
{
  const string expectedSeedText = "abc|rainsForAYear";
  CompatibilityWorldSnapshot compatibility = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new CompatibilityWorldMetadata(
      "seed-text",
      1,
      400,
      300,
      200,
      75,
      SeedText: expectedSeedText),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    loadReport: new CompatibilityLoadReport([]));
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  if (snapshot.World.Metadata.SeedText != expectedSeedText)
  {
    throw new InvalidOperationException("Compatibility projection did not preserve raw WLD seed text.");
  }
}

static void VerifyMoonPhaseProjection()
{
  const byte expectedMoonPhase = 6;
  LegacyWorldDocument document = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new LegacyWorldMetadata(
      "moon-phase",
      1456,
      400,
      300,
      200,
      75,
      MoonPhase: expectedMoonPhase),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  CompatibilityWorldSnapshot compatibility = WldToCompatibilityProjection.Project(document);
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  if (compatibility.Metadata.MoonPhase != expectedMoonPhase ||
      snapshot.Clock.MoonPhase != expectedMoonPhase)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the legacy moon phase.");
  }
}

static void VerifyWorldEventFlagsProjection()
{
  LegacyWorldDocument document = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new LegacyWorldMetadata(
      "event-flags",
      1456,
      400,
      300,
      200,
      75,
      IsBloodMoon: true,
      IsEclipse: true),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  CompatibilityWorldSnapshot compatibility = WldToCompatibilityProjection.Project(document);
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  if (!compatibility.Metadata.IsBloodMoon || !compatibility.Metadata.IsEclipse ||
      !snapshot.Progression.IsBloodMoon || !snapshot.Progression.IsEclipse)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the legacy world event flags.");
  }
}

static void VerifyCrimsonWorldProjection()
{
  LegacyWorldDocument document = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new LegacyWorldMetadata(
      "crimson-world",
      1456,
      400,
      300,
      200,
      75,
      IsCrimsonWorld: true),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  CompatibilityWorldSnapshot compatibility = WldToCompatibilityProjection.Project(document);
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  if (!compatibility.Metadata.IsCrimsonWorld || !snapshot.WorldRules.IsCrimsonWorld)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the legacy Crimson-world flag.");
  }
}

static void VerifyHardModeProjection()
{
  LegacyWorldDocument document = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new LegacyWorldMetadata(
      "hard-mode",
      1456,
      400,
      300,
      200,
      75,
      IsHardMode: true),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  CompatibilityWorldSnapshot compatibility = WldToCompatibilityProjection.Project(document);
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  if (!compatibility.Metadata.IsHardMode || !snapshot.Progression.IsHardMode)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the legacy hard-mode flag.");
  }
}

static void VerifyProgressionFactsProjection()
{
  LegacyWorldDocument document = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new LegacyWorldMetadata(
      "progression-facts",
      1456,
      400,
      300,
      200,
      75,
      DefeatedEyeOfCthulhu: true,
      DefeatedEaterOrBrain: true,
      DefeatedSkeletron: true,
      DefeatedMechanicalBoss: true,
      DefeatedPlantera: true,
      DefeatedGolem: true),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  CompatibilityWorldSnapshot compatibility = WldToCompatibilityProjection.Project(document);
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  WorldProgressionState progression = snapshot.Progression;
  if (!compatibility.Metadata.DefeatedEyeOfCthulhu ||
      !compatibility.Metadata.DefeatedEaterOrBrain ||
      !compatibility.Metadata.DefeatedSkeletron ||
      !compatibility.Metadata.DefeatedMechanicalBoss ||
      !compatibility.Metadata.DefeatedPlantera ||
      !compatibility.Metadata.DefeatedGolem ||
      !progression.DefeatedEyeOfCthulhu ||
      !progression.DefeatedEaterOrBrain ||
      !progression.DefeatedSkeletron ||
      !progression.DefeatedMechanicalBoss ||
      !progression.DefeatedPlantera ||
      !progression.DefeatedGolem)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the legacy progression facts.");
  }
}

static void VerifyInvasionFactsProjection()
{
  LegacyWorldDocument document = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new LegacyWorldMetadata(
      "invasion-facts",
      1456,
      400,
      300,
      200,
      75,
      InvasionType: 4,
      InvasionSize: 250,
      InvasionX: 123.5,
      DefeatedGoblins: true,
      DefeatedFrost: true,
      DefeatedPirates: true,
      DefeatedMartians: true),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  CompatibilityWorldSnapshot compatibility = WldToCompatibilityProjection.Project(document);
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  if (compatibility.Metadata.InvasionType != 4 ||
      compatibility.Metadata.InvasionSize != 250 ||
      compatibility.Metadata.InvasionX != 123.5 ||
      !compatibility.Metadata.DefeatedGoblins ||
      !compatibility.Metadata.DefeatedFrost ||
      !compatibility.Metadata.DefeatedPirates ||
      !compatibility.Metadata.DefeatedMartians ||
      snapshot.Progression.InvasionType != 4 ||
      snapshot.Progression.InvasionSize != 250 ||
      snapshot.Progression.InvasionX != 123.5 ||
      !snapshot.Progression.DefeatedGoblins ||
      !snapshot.Progression.DefeatedFrost ||
      !snapshot.Progression.DefeatedPirates ||
      !snapshot.Progression.DefeatedMartians)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the legacy invasion facts.");
  }

  LegacyWorldDocument invalidDocument = new(
    version: document.Version,
    formatVersion: document.FormatVersion,
    metadata: document.Metadata with { InvasionSize = -1 },
    tiles: document.Tiles,
    chests: document.Chests,
    signs: document.Signs,
    npcs: document.Npcs,
    tileEntities: document.TileEntities,
    diagnostics: document.Diagnostics);
  CompatibilityWorldSnapshot invalidCompatibility = WldToCompatibilityProjection.Project(
    invalidDocument);
  ExpectInvalid(() => _ = CompatibilityToDomeProjection.Project(
    invalidCompatibility,
    new WorldSeed(1456)));
}

static void VerifyGameModeProjection()
{
  foreach ((int gameMode, int difficulty, bool expert, bool master, bool journey) expected in
           new[]
           {
             (0, 0, false, false, false),
             (1, 1, true, false, false),
             (2, 2, true, true, false),
             (3, 0, false, false, true)
           })
  {
    LegacyWorldDocument document = new(
      version: 319,
      formatVersion: WldFormatVersion.PointerTableV88ToV319,
      metadata: new LegacyWorldMetadata(
        "game-mode",
        1456,
        400,
        300,
        200,
        75,
        GameMode: expected.gameMode),
      tiles: [],
      chests: [],
      signs: [],
      npcs: [],
      tileEntities: [],
      diagnostics: []);
    CompatibilityWorldSnapshot compatibility = WldToCompatibilityProjection.Project(document);
    DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
      compatibility,
      new WorldSeed(1456));
    WorldRuleState rules = snapshot.WorldRules;
    if (compatibility.Metadata.GameMode != expected.gameMode ||
        rules.Difficulty != expected.difficulty ||
        rules.IsExpertMode != expected.expert ||
        rules.IsMasterMode != expected.master ||
        rules.IsJourneyMode != expected.journey)
    {
      throw new InvalidOperationException(
        "Compatibility projection did not preserve the legacy game-mode semantics.");
    }
  }

  LegacyWorldDocument invalidDocument = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new LegacyWorldMetadata("invalid-game-mode", 1456, 400, 300, 200, 75, GameMode: 4),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  CompatibilityWorldSnapshot invalidCompatibility = WldToCompatibilityProjection.Project(
    invalidDocument);
  ExpectInvalid(() => _ = CompatibilityToDomeProjection.Project(
    invalidCompatibility,
    new WorldSeed(1456)));
}

static void VerifyMeteorScheduleProjection()
{
  LegacyWorldDocument document = new(
    version: 319,
    formatVersion: WldFormatVersion.PointerTableV88ToV319,
    metadata: new LegacyWorldMetadata(
      "meteor-schedule",
      1456,
      400,
      300,
      200,
      75,
      IsMeteorScheduled: true),
    tiles: [],
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  CompatibilityWorldSnapshot compatibility = WldToCompatibilityProjection.Project(document);
  DomeSimulationSnapshot snapshot = CompatibilityToDomeProjection.Project(
    compatibility,
    new WorldSeed(1456));
  if (!compatibility.Metadata.IsMeteorScheduled || !snapshot.Progression.IsMeteorScheduled)
  {
    throw new InvalidOperationException(
      "Compatibility projection did not preserve the pending meteor schedule.");
  }
}

static void VerifyFullBaselineTileProjection(string fullBaselinePath)
{
  DomeWorldImportResult result = new DomeWorldImportApplier().Import(
    fullBaselinePath,
    strictImport: false);
  LegacyWorldDocument document = result.Document;
  if (document.Tiles.Count != document.Metadata.Width * document.Metadata.Height ||
      result.Snapshot.World.Metadata.Width != document.Metadata.Width ||
      result.Snapshot.World.Metadata.Height != document.Metadata.Height)
  {
    throw new InvalidOperationException("The full baseline world dimensions were not preserved.");
  }

  int mismatchTiles = 0;
  for (int index = 0; index < document.Tiles.Count; index++)
  {
    int x = index / document.Metadata.Height;
    int y = index % document.Metadata.Height;
    LegacyTile expected = document.Tiles[index];
    WorldTile actual = result.Snapshot.World.GetTile(x, y);
    if (!LegacyTileMatchesWorldTile(expected, actual))
    {
      mismatchTiles++;
    }
  }

  if (mismatchTiles != 0)
  {
    throw new InvalidOperationException(
      $"The full baseline import lost {mismatchTiles} tile states during projection.");
  }

  string repositoryRoot = FindRepositoryRoot();
  string evidenceDirectory = Path.Combine(repositoryRoot, "docs", "worldgen");
  Directory.CreateDirectory(evidenceDirectory);
  LegacyImportProjectionEvidence evidence = new(
    fullBaselinePath,
    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fullBaselinePath))),
    document.Version,
    document.Metadata.Width,
    document.Metadata.Height,
    document.Tiles.Count,
    mismatchTiles,
    result.Snapshot.OpaqueCompatibilityRecords.Count);
  File.WriteAllText(
    Path.Combine(evidenceDirectory, "legacy-worldgen-import-parity.json"),
    JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
  Console.WriteLine(
    $"PASS: full baseline import preserves {document.Tiles.Count} tile states; " +
    $"opaque non-tile records {result.Snapshot.OpaqueCompatibilityRecords.Count}");
}

static string FindRepositoryRoot()
{
  DirectoryInfo? directory = new(AppContext.BaseDirectory);
  while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
  {
    directory = directory.Parent;
  }

  return directory?.FullName ?? throw new InvalidOperationException(
    "The repository root was not found.");
}

static bool LegacyTileMatchesWorldTile(LegacyTile expected, WorldTile actual)
{
  return expected.IsActive == actual.IsActive &&
    expected.TileType == actual.Type &&
    expected.WallType == actual.WallType &&
    expected.LiquidAmount == actual.LiquidAmount &&
    expected.LiquidKind == actual.LiquidType &&
    expected.HasWire == actual.HasWire &&
    expected.HasWire2 == actual.HasWire2 &&
    expected.HasWire3 == actual.HasWire3 &&
    expected.HasWire4 == actual.HasWire4 &&
    expected.IsHalfBrick == actual.IsHalfBrick &&
    expected.Slope == actual.Slope &&
    expected.IsActuated == actual.IsActuated &&
    expected.IsInactive == actual.IsInactive &&
    expected.FrameX == actual.FrameX &&
    expected.FrameY == actual.FrameY &&
    expected.TileColor == actual.TileColor &&
    expected.WallColor == actual.WallColor &&
    expected.IsInvisibleBlock == actual.IsInvisibleBlock &&
    expected.IsInvisibleWall == actual.IsInvisibleWall &&
    expected.IsFullbrightBlock == actual.IsFullbrightBlock &&
    expected.IsFullbrightWall == actual.IsFullbrightWall;
}

static void VerifyTemporaryPortLifecycle()
{
  using TcpListener probe = new(IPAddress.Loopback, 0);
  probe.Start();
  int port = ((IPEndPoint)probe.LocalEndpoint).Port;
  probe.Stop();
  if (port is 7777 or 7778)
  {
    throw new InvalidOperationException("The temporary verifier port is protected.");
  }

  using Terraria.Dome.Server.DomeServer server = new();
  server.Start(port);
  if (server.Port != port)
  {
    throw new InvalidOperationException(
      "The imported-server lifecycle did not bind the requested port.");
  }
}

internal sealed record LegacyImportProjectionEvidence(
  string LegacyPath,
  string LegacySha256,
  int Version,
  int Width,
  int Height,
  int ComparedTiles,
  int MismatchTiles,
  int OpaqueNonTileRecords);
