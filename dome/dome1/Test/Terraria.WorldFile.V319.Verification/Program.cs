using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Terraria.WorldFile.V319;
using Terraria.WorldFile.V319.Format;
using Terraria.WorldFile.V319.Model;
using Terraria.WorldFile.V319.Verification.Fixtures;

const string WorldFileHash =
  "92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289";
const string TileHash =
  "65FF4F662640D1BBD2FD994631E3B71CA5C15D1AF9785EC3E6402F4B04F86595";
const string SignHash =
  "AA8B1D36046BF06E1D0ACCF21B75867A591BFE2D02AF3CEA9C58425796A1947A";

string repositoryRoot = FindRepositoryRoot();
string parserProjectPath = Path.Combine(
  repositoryRoot,
  "src",
  "Terraria.WorldFile.V319",
  "Terraria.WorldFile.V319.csproj");
string legacyReferenceDirectory = Path.Combine(
  repositoryRoot,
  "src",
  "Terraria.WorldFile.V319",
  "LegacyReference");
string manifestPath = Path.Combine(legacyReferenceDirectory, "SourceManifest.md");

VerifyFileExists(parserProjectPath, "The net10 parser project is missing.");
VerifyFileExists(manifestPath, "The legacy source manifest is missing.");

VerifyLegacySource(
  legacyReferenceDirectory,
  "WorldFile.cs",
  WorldFileHash,
  "D:\\TRbackup\\无任何删减通过编译\\Terraria.IO\\WorldFile.cs");
VerifyLegacySource(
  legacyReferenceDirectory,
  "Tile.cs",
  TileHash,
  "D:\\TRbackup\\无任何删减通过编译\\Terraria\\Tile.cs");
VerifyLegacySource(
  legacyReferenceDirectory,
  "Sign.cs",
  SignHash,
  "D:\\TRbackup\\无任何删减通过编译\\Terraria\\Sign.cs");
VerifyParserProject(parserProjectPath);
VerifyNonSeekableInputIsRejected();
VerifyTruncatedPrimitiveIsRejected();
VerifyOversizedStringIsRejected();
VerifyImpossibleDimensionsAreRejected();
VerifyDocumentCopiesCollections();
VerifyFormatDispatch(1, WldFormatVersion.LegacyV1ToV87);
VerifyFormatDispatch(87, WldFormatVersion.LegacyV1ToV87);
VerifyFormatDispatch(88, WldFormatVersion.PointerTableV88ToV319);
VerifyFormatDispatch(319, WldFormatVersion.PointerTableV88ToV319);
VerifyVersionMatrix();
VerifyFutureVersionIsRejected();
VerifyLegacyDocument(version: 1);
VerifyLegacyDocument(version: 87);
VerifyLegacyVersionMatrix();
VerifyLegacyWorldEventFlagVersionBoundary();
VerifyLegacyCrimsonVersionBoundary();
VerifyLegacyHardModeVersionBoundary();
VerifyLegacyInvasionFacts();
VerifyLegacyRleCannotCrossRowBoundary();
VerifyPointerVersionMatrix();
VerifyGoodWorldVersionBoundary();
VerifyRemixWorldVersionBoundary();
VerifyWorldGeneratorVersionBoundary();
VerifyWorldUniqueIdBoundary();
VerifyWorldSeedTextBoundary();
VerifyPointerWorldEventFlags();
VerifyPointerWorldCrimson();
VerifyPointerWorldHardMode();
VerifyPointerWorldProgressionFacts();
VerifyPointerWorldInvasionFacts();
VerifyPointerWorldGameMode();
VerifyWorldMeteorSchedule();
VerifyWorldWindVersionBoundary();
VerifyWorldRainVersionBoundary();
VerifyInvalidPointerWorldMoonPhaseIsRejected();
VerifyRecordedOracleArtifacts(repositoryRoot);
VerifyV88PointerWorld();
VerifyV88ObjectSections();
VerifyObjectSectionValidation();
VerifyV319PointerTable();
VerifyPointerTablePreservesTileImportance();
VerifyV319PointerWorld();
VerifyTileEntityFoodPlatter();
VerifyTileEntityDisplayJar();
VerifyTileEntityAnchors();
VerifyTileEntityDisplayDollEmptyState();
VerifyTileEntityHatRackEmptyState();
VerifyTileEntityPylon();
VerifyTileEntityVariableItemPayloads();
VerifyUnknownTileEntityIsOpaque();
VerifyInvalidPointerTables();
VerifyHeaderCannotPassItsNextPointer();
VerifyTileRleReader();

Console.WriteLine("PASS: legacy source evidence is excluded from the net10 runtime graph");
Console.WriteLine("PASS: historical version matrix and recorded differential oracle");

static string FindRepositoryRoot()
{
  DirectoryInfo? directory = new(AppContext.BaseDirectory);
  while (directory is not null)
  {
    if (File.Exists(Path.Combine(directory.FullName, "Terraria.Dome.sln")))
    {
      return directory.FullName;
    }

    directory = directory.Parent;
  }

  throw new InvalidOperationException("Could not locate the repository root.");
}

static void VerifyFileExists(string path, string message)
{
  if (!File.Exists(path))
  {
    throw new InvalidOperationException(message);
  }
}

static void VerifyLegacySource(
  string legacyReferenceDirectory,
  string fileName,
  string expectedHash,
  string expectedOriginPath)
{
  string sourcePath = Path.Combine(legacyReferenceDirectory, fileName);
  VerifyFileExists(sourcePath, $"The isolated {fileName} source copy is missing.");

  string actualHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(sourcePath)));
  if (!StringComparer.Ordinal.Equals(expectedHash, actualHash))
  {
    throw new InvalidOperationException(
      $"The isolated {fileName} source copy does not match its recorded SHA-256.");
  }

  string manifest = File.ReadAllText(Path.Combine(legacyReferenceDirectory, "SourceManifest.md"));
  if (!manifest.Contains(expectedOriginPath, StringComparison.Ordinal) ||
      !manifest.Contains(expectedHash, StringComparison.Ordinal))
  {
    throw new InvalidOperationException(
      $"The source manifest does not record the origin and SHA-256 for {fileName}.");
  }
}

static void VerifyParserProject(string parserProjectPath)
{
  XDocument project = XDocument.Load(parserProjectPath);
  XNamespace projectNamespace = project.Root?.Name.Namespace ?? XNamespace.None;
  string? targetFramework = project
    .Descendants(projectNamespace + "TargetFramework")
    .SingleOrDefault()
    ?.Value;
  if (!StringComparer.Ordinal.Equals(targetFramework, "net10.0"))
  {
    throw new InvalidOperationException("The parser project must target net10.0.");
  }

  string[] legacyFileNames = ["WorldFile.cs", "Tile.cs", "Sign.cs"];
  IEnumerable<XElement> runtimeItems = project.Descendants()
    .Where(element => element.Name.LocalName is "Compile" or "ProjectReference" or "Reference");
  foreach (XElement runtimeItem in runtimeItems)
  {
    string include = runtimeItem.Attribute("Include")?.Value ?? string.Empty;
    if (legacyFileNames.Any(fileName =>
      include.Contains($"LegacyReference\\{fileName}", StringComparison.Ordinal) ||
      include.Contains($"LegacyReference/{fileName}", StringComparison.Ordinal)))
    {
      throw new InvalidOperationException(
        "A legacy source copy appears in the net10 runtime dependency graph.");
    }
  }

  bool hasOldTerrariaPackage = project.Descendants()
    .Where(element => element.Name.LocalName == "PackageReference")
    .Select(element => element.Attribute("Include")?.Value ?? string.Empty)
    .Any(include => include.Contains("Terraria", StringComparison.OrdinalIgnoreCase));
  if (hasOldTerrariaPackage)
  {
    throw new InvalidOperationException(
      "The parser project must not reference an old Terraria package.");
  }

  HashSet<string> excludedFiles = project.Descendants()
    .Where(element => element.Name.LocalName == "Compile")
    .Where(element => StringComparer.Ordinal.Equals(element.Attribute("Remove")?.Value, "LegacyReference\\**\\*.cs"))
    .Select(element => element.Attribute("Remove")?.Value ?? string.Empty)
    .ToHashSet(StringComparer.Ordinal);
  if (excludedFiles.Count != 1)
  {
    throw new InvalidOperationException(
      "The parser project must explicitly exclude legacy source copies from compilation.");
  }

  bool hasEvidenceOnlyBuildAction = project.Descendants()
    .Where(element => element.Name.LocalName == "None")
    .Any(element => StringComparer.Ordinal.Equals(
      element.Attribute("Include")?.Value,
      "LegacyReference\\**\\*.cs"));
  if (!hasEvidenceOnlyBuildAction)
  {
    throw new InvalidOperationException(
      "The legacy source copies must have the None build action.");
  }
}

static void VerifyNonSeekableInputIsRejected()
{
  using NonSeekableReadStream input = new([1, 2, 3, 4]);
  AssertInvalidData(
    () => _ = WldWorldReader.Read(input),
    "Non-seekable input was accepted.");
}

static void VerifyTruncatedPrimitiveIsRejected()
{
  using MemoryStream input = new([1, 2, 3]);
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  AssertInvalidData(
    () => _ = reader.ReadInt32(),
    "A truncated Int32 primitive was accepted.");
}

static void VerifyOversizedStringIsRejected()
{
  WldReadLimits limits = WldReadLimits.Default with { MaxStringByteLength = 3 };
  using MemoryStream input = new([4, (byte)'t', (byte)'e', (byte)'s', (byte)'t']);
  using WldBinaryReader reader = new(input, limits, 0, input.Length);
  AssertInvalidData(
    () => _ = reader.ReadString(),
    "A string longer than the configured byte cap was accepted.");
}

static void VerifyImpossibleDimensionsAreRejected()
{
  WldReadLimits limits = WldReadLimits.Default with
  {
    MaxWorldWidth = 4,
    MaxWorldHeight = 3,
    MaxTileCount = 12
  };

  AssertInvalidData(
    () => limits.ValidateWorldDimensions(5, 3),
    "A world wider than the configured cap was accepted.");
  AssertInvalidData(
    () => limits.ValidateWorldDimensions(4, 4),
    "A world taller than the configured cap was accepted.");
  WldReadLimits tileCountLimits = limits with { MaxTileCount = 11 };
  AssertInvalidData(
    () => tileCountLimits.ValidateWorldDimensions(4, 3),
    "A world with an impossible tile count was accepted.");
}

static void VerifyDocumentCopiesCollections()
{
  List<LegacyTile> tiles = [new LegacyTile(IsActive: true, TileType: 7)];
  LegacyWorldDocument document = new(
    version: 1,
    formatVersion: WldFormatVersion.LegacyV1ToV87,
    metadata: new LegacyWorldMetadata("Immutable", 1, 1, 1, 0, 0),
    tiles: tiles,
    chests: [],
    signs: [],
    npcs: [],
    tileEntities: [],
    diagnostics: []);
  tiles[0] = new LegacyTile(IsActive: false, TileType: 9);

  if (document.Tiles[0] != new LegacyTile(IsActive: true, TileType: 7))
  {
    throw new InvalidOperationException("The document retained a mutable caller tile collection.");
  }

  if (document.Tiles is IList<LegacyTile>)
  {
    throw new InvalidOperationException("The document exposed a mutable tile collection.");
  }
}

static void AssertInvalidData(Action action, string failureMessage)
{
  try
  {
    action();
  }
  catch (InvalidDataException)
  {
    return;
  }

  throw new InvalidOperationException(failureMessage);
}

static void AssertInvalidDataContains(Action action, params string[] expectedMessageParts)
{
  try
  {
    action();
  }
  catch (InvalidDataException exception)
  {
    if (expectedMessageParts.All(part => exception.Message.Contains(part, StringComparison.Ordinal)))
    {
      return;
    }

    throw new InvalidOperationException(
      $"The WLD error context was incomplete: {exception.Message}");
  }

  throw new InvalidOperationException("The malformed WLD section was accepted.");
}

static void VerifyFormatDispatch(int version, WldFormatVersion expectedFormatVersion)
{
  if (WldWorldReader.SelectFormatVersion(version) != expectedFormatVersion)
  {
    throw new InvalidOperationException(
      $"WLD version {version} selected an unexpected parser layout.");
  }
}

static void VerifyVersionMatrix()
{
  ExpectedVersionLayout[] rows = ExpectedVersionLayouts.CreateAll();
  if (rows.Length != WldWorldReader.MaximumSupportedVersion)
  {
    throw new InvalidOperationException("The WLD version matrix is incomplete.");
  }

  int transitionCount = 0;
  for (int index = 0; index < rows.Length; index++)
  {
    ExpectedVersionLayout row = rows[index];
    if (row.Version != index + 1 || WldWorldReader.SelectFormatVersion(row.Version) !=
        row.FormatVersion)
    {
      throw new InvalidOperationException($"The WLD version matrix is invalid at v{row.Version}.");
    }

    if (row.FormatVersion == WldFormatVersion.PointerTableV88ToV319 &&
        WldSectionPointerTable.GetExpectedSectionCount(row.Version) != row.SectionCount)
    {
      throw new InvalidOperationException($"The WLD section matrix is invalid at v{row.Version}.");
    }

    if (row.IsLayoutTransition)
    {
      transitionCount++;
    }
  }

  if (transitionCount < 2 ||
      !rows[0].IsLayoutTransition ||
      rows[0].LayoutTransition != "legacy-base" ||
      !rows[87].IsLayoutTransition ||
      rows[87].LayoutTransition != "pointer-table-base")
  {
    throw new InvalidOperationException(
      "The WLD version matrix does not record the legacy and pointer-table boundaries.");
  }
}

static void VerifyLegacyDocument(int version)
{
  using MemoryStream input = LegacyV1ToV87FixtureWriter.Create(version);
  LegacyWorldDocument document = WldWorldReader.Read(input);
  if (document.Version != version ||
      document.FormatVersion != WldFormatVersion.LegacyV1ToV87 ||
      document.Metadata.Name != "Legacy fixture" ||
      document.Metadata.WorldId != 2468 ||
      document.Metadata.Width != 2 ||
      document.Metadata.Height != 2 ||
      document.Metadata.SpawnX != 1 ||
      document.Metadata.SpawnY != 1 ||
      document.Metadata.WorldSurface != 0 ||
      document.Metadata.RockLayer != 1 ||
      document.Tiles.Count != 4 ||
      document.Chests.Count != 1 ||
      document.Chests[0].Items.Count != (version < 58 ? 20 : 40) ||
      document.Signs.Count != 1 ||
      document.Npcs.Count != 0)
  {
    throw new InvalidOperationException($"The v{version} legacy fixture was not parsed.");
  }
}

static void VerifyLegacyVersionMatrix()
{
  for (int version = 1; version <= 87; version++)
  {
    VerifyLegacyDocument(version);
  }
}

static void VerifyLegacyRleCannotCrossRowBoundary()
{
  using MemoryStream input = LegacyV1ToV87FixtureWriter.CreateRleCrossingRowBoundary();
  AssertInvalidData(
    () => _ = WldWorldReader.Read(input),
    "A legacy tile RLE run crossed a row boundary.");
}

static void VerifyLegacyWorldEventFlagVersionBoundary()
{
  using MemoryStream beforeEclipse = LegacyV1ToV87FixtureWriter.Create(
    version: 69,
    isBloodMoon: true,
    isEclipse: false);
  LegacyWorldDocument v69 = WldWorldReader.Read(beforeEclipse);
  if (!v69.Metadata.IsBloodMoon || v69.Metadata.IsEclipse)
  {
    throw new InvalidOperationException("A pre-v70 WLD event flag layout was not preserved.");
  }

  using MemoryStream withEclipse = LegacyV1ToV87FixtureWriter.Create(
    version: 70,
    isBloodMoon: true,
    isEclipse: true);
  LegacyWorldDocument v70 = WldWorldReader.Read(withEclipse);
  if (!v70.Metadata.IsBloodMoon || !v70.Metadata.IsEclipse)
  {
    throw new InvalidOperationException("The v70 WLD eclipse flag was discarded.");
  }
}

static void VerifyLegacyCrimsonVersionBoundary()
{
  using MemoryStream beforeCrimson = LegacyV1ToV87FixtureWriter.Create(
    version: 55,
    isBloodMoon: false,
    isEclipse: false,
    isCrimsonWorld: false);
  LegacyWorldDocument v55 = WldWorldReader.Read(beforeCrimson);
  if (v55.Metadata.IsCrimsonWorld)
  {
    throw new InvalidOperationException("A pre-v56 WLD unexpectedly restored a Crimson world.");
  }

  using MemoryStream withCrimson = LegacyV1ToV87FixtureWriter.Create(
    version: 56,
    isBloodMoon: false,
    isEclipse: false,
    isCrimsonWorld: true);
  LegacyWorldDocument v56 = WldWorldReader.Read(withCrimson);
  if (!v56.Metadata.IsCrimsonWorld)
  {
    throw new InvalidOperationException("The v56 WLD Crimson-world flag was discarded.");
  }
}

static void VerifyLegacyHardModeVersionBoundary()
{
  using MemoryStream beforeHardMode = LegacyV1ToV87FixtureWriter.Create(
    version: 22,
    isBloodMoon: false,
    isEclipse: false,
    isHardMode: false);
  LegacyWorldDocument v22 = WldWorldReader.Read(beforeHardMode);
  if (v22.Metadata.IsHardMode)
  {
    throw new InvalidOperationException("A pre-v23 WLD unexpectedly restored hard mode.");
  }

  using MemoryStream withHardMode = LegacyV1ToV87FixtureWriter.Create(
    version: 23,
    isBloodMoon: false,
    isEclipse: false,
    isHardMode: true);
  LegacyWorldDocument v23 = WldWorldReader.Read(withHardMode);
  if (!v23.Metadata.IsHardMode)
  {
    throw new InvalidOperationException("The v23 WLD hard-mode flag was discarded.");
  }
}

static void VerifyLegacyInvasionFacts()
{
  using MemoryStream input = LegacyV1ToV87FixtureWriter.Create(
    version: 1,
    isBloodMoon: false,
    isEclipse: false,
    invasionType: 3,
    invasionSize: 125);
  LegacyWorldDocument document = WldWorldReader.Read(input);
  if (document.Metadata.InvasionType != 3 || document.Metadata.InvasionSize != 125)
  {
    throw new InvalidOperationException("The v1 WLD invasion facts were discarded.");
  }
}

static void VerifyPointerVersionMatrix()
{
  for (int version = 88; version <= 319; version++)
  {
    using MemoryStream input = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(version);
    LegacyWorldDocument document = WldWorldReader.Read(input);
    if (document.Version != version ||
        document.FormatVersion != WldFormatVersion.PointerTableV88ToV319 ||
        document.Metadata.Name != "Matrix fixture" ||
        document.Metadata.WorldId != version ||
       document.Metadata.Width != 2 ||
       document.Metadata.Height != 2 ||
       document.Metadata.MoonPhase != 6 ||
       document.Tiles.Count != 4 ||
        document.Chests.Count != 0 ||
        document.Signs.Count != 0 ||
        document.Npcs.Count != 0 ||
        document.TileEntities.Count != 0 ||
        document.Diagnostics.Count != 0)
    {
      throw new InvalidOperationException($"The v{version} pointer fixture was not parsed.");
    }
  }
}

static void VerifyRemixWorldVersionBoundary()
{
  LegacyWorldDocument beforeRemix = WldWorldReader.Read(
    WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(248));
  if (beforeRemix.Metadata.IsRemixWorld is not null)
  {
    throw new InvalidOperationException(
      "A pre-v249 WLD unexpectedly supplied a Remix world value.");
  }

  LegacyWorldDocument remix = WldWorldReader.Read(
    WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(249, isRemixWorld: true));
  if (remix.Metadata.IsRemixWorld != true)
  {
    throw new InvalidOperationException(
      "The v249 WLD Remix world bit was not restored from the header.");
  }
}

static void VerifyGoodWorldVersionBoundary()
{
  LegacyWorldDocument beforeGoodWorld = WldWorldReader.Read(
    WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(226, isGoodWorld: true));
  if (beforeGoodWorld.Metadata.IsGoodWorld is not null ||
      beforeGoodWorld.Metadata.IsRemixWorld is not null)
  {
    throw new InvalidOperationException(
      "A pre-v227 WLD unexpectedly supplied Good World or Remix metadata.");
  }

  LegacyWorldDocument goodWorld = WldWorldReader.Read(
    WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
      227,
      isGoodWorld: true,
      isRemixWorld: true));
  if (goodWorld.Metadata.IsGoodWorld != true ||
      goodWorld.Metadata.IsRemixWorld is not null)
  {
    throw new InvalidOperationException(
      "The v227 WLD Good World bit was not isolated from the later Remix bit.");
  }

  LegacyWorldDocument current = WldWorldReader.Read(
    WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
      319,
      isGoodWorld: true,
      isRemixWorld: true));
  if (current.Metadata.IsGoodWorld != true || current.Metadata.IsRemixWorld != true)
  {
    throw new InvalidOperationException(
      "The v319 WLD Good World and Remix bits were not preserved independently.");
  }

  Console.WriteLine("PASS: WLD v226/v227/v319 Good World boundary");
}

static void VerifyWorldGeneratorVersionBoundary()
{
  const ulong expectedGeneratorVersion = 1370094567425UL;
  LegacyWorldDocument document = WldWorldReader.Read(
    WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
      319,
      worldGeneratorVersion: expectedGeneratorVersion));
  if (document.Metadata.WorldGeneratorVersion != expectedGeneratorVersion)
  {
    throw new InvalidOperationException(
      "The v179+ WLD generator version was not restored from the header.");
  }

  LegacyWorldDocument beforeGeneratorVersion = WldWorldReader.Read(
    WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(178));
  if (beforeGeneratorVersion.Metadata.WorldGeneratorVersion is not null)
  {
    throw new InvalidOperationException(
      "A pre-v179 WLD unexpectedly supplied a generator version.");
  }
}

static void VerifyWorldUniqueIdBoundary()
{
  Guid expectedUniqueId = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff");
  LegacyWorldDocument document = WldWorldReader.Read(
    WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
      319,
      uniqueId: expectedUniqueId));
  if (document.Metadata.UniqueId != expectedUniqueId)
  {
    throw new InvalidOperationException("The v181+ WLD UUID was not restored from the header.");
  }

  LegacyWorldDocument beforeUniqueId = WldWorldReader.Read(
    WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(180));
  if (beforeUniqueId.Metadata.UniqueId is not null)
  {
    throw new InvalidOperationException(
      "A pre-v181 WLD unexpectedly supplied a UUID.");
  }
}

static void VerifyWorldSeedTextBoundary()
{
  const string expectedSeedText = "abc|rainsForAYear";
  LegacyWorldDocument document = WldWorldReader.Read(
    WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
      319,
      seedText: expectedSeedText));
  if (document.Metadata.SeedText != expectedSeedText)
  {
    throw new InvalidOperationException("The v180+ WLD seed text was not restored from the header.");
  }

  LegacyWorldDocument beforeSeedText = WldWorldReader.Read(
    WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(178));
  if (beforeSeedText.Metadata.SeedText is not null)
  {
    throw new InvalidOperationException("A pre-v179 WLD unexpectedly supplied seed text.");
  }
}

static void VerifyInvalidPointerWorldMoonPhaseIsRejected()
{
  foreach (int moonPhase in new[] { -1, 8 })
  {
    using MemoryStream input = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
      version: 319,
      moonPhase: moonPhase);
    AssertInvalidData(
      () => _ = WldWorldReader.Read(input),
      "A pointer-table WLD with an invalid moon phase was accepted.");
  }
}

static void VerifyPointerWorldEventFlags()
{
  using MemoryStream input = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
    version: 319,
    isBloodMoon: true,
    isEclipse: true);
  LegacyWorldDocument document = WldWorldReader.Read(input);
  if (!document.Metadata.IsBloodMoon || !document.Metadata.IsEclipse)
  {
    throw new InvalidOperationException("The pointer-table WLD event flags were discarded.");
  }
}

static void VerifyPointerWorldCrimson()
{
  using MemoryStream input = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
    version: 319,
    isCrimsonWorld: true);
  LegacyWorldDocument document = WldWorldReader.Read(input);
  if (!document.Metadata.IsCrimsonWorld)
  {
    throw new InvalidOperationException("The pointer-table WLD Crimson-world flag was discarded.");
  }
}

static void VerifyPointerWorldHardMode()
{
  using MemoryStream input = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
    version: 319,
    isHardMode: true);
  LegacyWorldDocument document = WldWorldReader.Read(input);
  if (!document.Metadata.IsHardMode)
  {
    throw new InvalidOperationException("The pointer-table WLD hard-mode flag was discarded.");
  }
}

static void VerifyPointerWorldProgressionFacts()
{
  using MemoryStream input = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
    version: 319,
    defeatedEyeOfCthulhu: true,
    defeatedEaterOrBrain: true,
    defeatedSkeletron: true,
    defeatedMechanicalBoss: true,
    defeatedPlantera: true,
    defeatedGolem: true);
  LegacyWorldDocument document = WldWorldReader.Read(input);
  if (!document.Metadata.DefeatedEyeOfCthulhu ||
      !document.Metadata.DefeatedEaterOrBrain ||
      !document.Metadata.DefeatedSkeletron ||
      !document.Metadata.DefeatedMechanicalBoss ||
      !document.Metadata.DefeatedPlantera ||
      !document.Metadata.DefeatedGolem)
  {
    throw new InvalidOperationException("The pointer-table WLD progression facts were discarded.");
  }
}

static void VerifyPointerWorldInvasionFacts()
{
  using MemoryStream input = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
    version: 319,
    invasionType: 4,
    invasionSize: 250);
  LegacyWorldDocument document = WldWorldReader.Read(input);
  if (document.Metadata.InvasionType != 4 || document.Metadata.InvasionSize != 250)
  {
    throw new InvalidOperationException("The pointer-table WLD invasion facts were discarded.");
  }
}

static void VerifyPointerWorldGameMode()
{
  using MemoryStream expertInput = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
    version: 112,
    gameMode: 1);
  LegacyWorldDocument expert = WldWorldReader.Read(expertInput);
  if (expert.Metadata.GameMode != 1)
  {
    throw new InvalidOperationException("The v112 expert-game-mode flag was discarded.");
  }

  using MemoryStream masterInput = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
    version: 208,
    gameMode: 2);
  LegacyWorldDocument master = WldWorldReader.Read(masterInput);
  if (master.Metadata.GameMode != 2)
  {
    throw new InvalidOperationException("The v208 master-game-mode flag was discarded.");
  }

  using MemoryStream journeyInput = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
    version: 319,
    gameMode: 3);
  LegacyWorldDocument journey = WldWorldReader.Read(journeyInput);
  if (journey.Metadata.GameMode != 3)
  {
    throw new InvalidOperationException("The v319 Journey game mode was discarded.");
  }
}

static void VerifyWorldMeteorSchedule()
{
  using MemoryStream legacyInput = LegacyV1ToV87FixtureWriter.Create(
    version: 87,
    isBloodMoon: false,
    isEclipse: false,
    isMeteorScheduled: true);
  LegacyWorldDocument legacy = WldWorldReader.Read(legacyInput);
  if (!legacy.Metadata.IsMeteorScheduled)
  {
    throw new InvalidOperationException("The legacy WLD meteor schedule was discarded.");
  }

  using MemoryStream pointerInput = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
    version: 319,
    isMeteorScheduled: true);
  LegacyWorldDocument pointer = WldWorldReader.Read(pointerInput);
  if (!pointer.Metadata.IsMeteorScheduled)
  {
    throw new InvalidOperationException("The pointer-table WLD meteor schedule was discarded.");
  }
}

static void VerifyWorldWindVersionBoundary()
{
  using MemoryStream legacyInput = LegacyV1ToV87FixtureWriter.Create(
    version: 61,
    isBloodMoon: false,
    isEclipse: false,
    isMeteorScheduled: false);
  LegacyWorldDocument legacy = WldWorldReader.Read(legacyInput);
  if (legacy.Metadata.WindSpeedTarget is not null)
  {
    throw new InvalidOperationException("A V61 WLD unexpectedly supplied a wind target.");
  }

  using MemoryStream modernInput = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
    version: 319,
    windSpeedTarget: -0.35f);
  LegacyWorldDocument modern = WldWorldReader.Read(modernInput);
  if (modern.Metadata.WindSpeedTarget != -0.35f)
  {
    throw new InvalidOperationException("A V319 WLD wind target was not restored exactly.");
  }
}

static void VerifyWorldRainVersionBoundary()
{
  using MemoryStream preRainInput = LegacyV1ToV87FixtureWriter.Create(version: 87);
  LegacyWorldDocument preRain = WldWorldReader.Read(preRainInput);
  if (preRain.Metadata.IsRaining != false ||
      preRain.Metadata.RainTimeTicks != 0 ||
      preRain.Metadata.MaximumRainStrength != 0.0f)
  {
    throw new InvalidOperationException("A V87 WLD rain fields were not restored.");
  }

  using MemoryStream rainInput = WldVersionMatrixFixtureWriter.CreateMinimalPointerWorld(
    version: 319,
    isRaining: true,
    rainTimeTicks: 120,
    maximumRainStrength: 0.75f);
  LegacyWorldDocument rain = WldWorldReader.Read(rainInput);
  if (rain.Metadata.IsRaining != true || rain.Metadata.RainTimeTicks != 120 ||
      rain.Metadata.MaximumRainStrength != 0.75f)
  {
    throw new InvalidOperationException("A V319 WLD rain triple was not restored exactly.");
  }
}

static void VerifyRecordedOracleArtifacts(string repositoryRoot)
{
  string sourceArtifactPath = Path.Combine(
    repositoryRoot,
    "Test",
    "Terraria.WorldFile.V319.Verification",
    "Oracle",
    "RecordedLegacyOracleSnapshots.json");
  string outputArtifactPath = Path.Combine(
    AppContext.BaseDirectory,
    "Oracle",
    "RecordedLegacyOracleSnapshots.json");
  VerifyFileExists(sourceArtifactPath, "The recorded legacy oracle artifact is missing.");
  VerifyFileExists(outputArtifactPath, "The copied legacy oracle artifact is missing.");

  using JsonDocument oracle = JsonDocument.Parse(File.ReadAllText(outputArtifactPath));
  JsonElement root = oracle.RootElement;
  if (OracleInt(root, "SchemaVersion") != 1 ||
      OracleString(root.GetProperty("Producer"), "SourceSha256") != WorldFileHash)
  {
    throw new InvalidOperationException("The recorded legacy oracle metadata is invalid.");
  }

  foreach (JsonElement oracleCase in root.GetProperty("Cases").EnumerateArray())
  {
    string fixture = OracleString(oracleCase, "Fixture");
    using MemoryStream input = fixture switch
    {
      "LegacyV87" => LegacyV1ToV87FixtureWriter.Create(87),
      "PointerV88" => CreateV88PointerWorld(),
      "PointerV319" => CreateV319PointerWorld(),
      _ => throw new InvalidOperationException($"The oracle fixture '{fixture}' is unsupported.")
    };
    LegacyWorldDocument document = WldWorldReader.Read(input);
    using JsonDocument normalized = JsonSerializer.SerializeToDocument(
      new
      {
        Fixture = fixture,
        Version = document.Version,
        FormatVersion = document.FormatVersion.ToString(),
        Metadata = new
        {
          document.Metadata.Name,
          document.Metadata.WorldId,
          document.Metadata.Width,
          document.Metadata.Height,
          document.Metadata.SpawnX,
          document.Metadata.SpawnY,
          document.Metadata.LeftWorld,
          document.Metadata.RightWorld,
          document.Metadata.TopWorld,
          document.Metadata.BottomWorld,
          document.Metadata.WorldSurface,
          document.Metadata.RockLayer,
          document.Metadata.MoonPhase,
          document.Metadata.IsBloodMoon,
          document.Metadata.IsEclipse,
          document.Metadata.IsCrimsonWorld,
          document.Metadata.IsHardMode,
          document.Metadata.DefeatedEyeOfCthulhu,
          document.Metadata.DefeatedEaterOrBrain,
          document.Metadata.DefeatedSkeletron,
          document.Metadata.DefeatedMechanicalBoss,
          document.Metadata.DefeatedPlantera,
          document.Metadata.DefeatedGolem,
          document.Metadata.InvasionType,
          document.Metadata.InvasionSize,
          document.Metadata.GameMode,
          document.Metadata.IsMeteorScheduled
        },
        TileCount = document.Tiles.Count,
        Tile = document.Tiles[0],
        Chests = document.Chests.Select(chest => new
        {
          chest.X,
          chest.Y,
          chest.Name,
          Items = chest.Items.Select(item => new { item.Stack, item.NetId, item.Prefix })
        }),
        Signs = document.Signs.Select(sign => new { sign.X, sign.Y, sign.Text }),
        Npcs = document.Npcs.Select(npc => new
        {
          npc.Type,
          npc.Name,
          npc.PositionX,
          npc.PositionY,
          npc.IsHomeless,
          npc.HomeX,
          npc.HomeY,
          npc.LegacyTypeName,
          npc.IsTownNpc,
          npc.TownVariationIndex,
          npc.HomelessDespawn
        }),
        TileEntities = document.TileEntities.Select(entity => new
        {
          entity.Id,
          entity.Type,
          entity.X,
          entity.Y,
          entity.IsOpaque,
          Payload = entity.Payload.Select(value => (int)value).ToArray()
        }),
        Footer = new { document.Metadata.Name, document.Metadata.WorldId }
      });
    AssertOracleJsonEqual(oracleCase, normalized.RootElement, $"oracle fixture {fixture}");
  }
}

static void AssertOracleJsonEqual(JsonElement expected, JsonElement actual, string context)
{
  if (expected.ValueKind != actual.ValueKind)
  {
    throw new InvalidOperationException($"{context} has an unexpected JSON value kind.");
  }

  if (expected.ValueKind == JsonValueKind.Object)
  {
    int expectedPropertyCount = expected.EnumerateObject().Count();
    int actualPropertyCount = actual.EnumerateObject().Count();
    if (expectedPropertyCount != actualPropertyCount)
    {
      throw new InvalidOperationException($"{context} has an unexpected property count.");
    }

    foreach (JsonProperty expectedProperty in expected.EnumerateObject())
    {
      if (!actual.TryGetProperty(expectedProperty.Name, out JsonElement actualProperty))
      {
        throw new InvalidOperationException(
          $"{context} is missing '{expectedProperty.Name}'.");
      }

      AssertOracleJsonEqual(
        expectedProperty.Value,
        actualProperty,
        $"{context}.{expectedProperty.Name}");
    }

    return;
  }

  if (expected.ValueKind == JsonValueKind.Array)
  {
    if (expected.GetArrayLength() != actual.GetArrayLength())
    {
      throw new InvalidOperationException($"{context} has an unexpected array length.");
    }

    for (int index = 0; index < expected.GetArrayLength(); index++)
    {
      AssertOracleJsonEqual(expected[index], actual[index], $"{context}[{index}]");
    }

    return;
  }

  if (!StringComparer.Ordinal.Equals(expected.GetRawText(), actual.GetRawText()))
  {
    throw new InvalidOperationException($"{context} has an unexpected value.");
  }
}

static int OracleInt(JsonElement element, string propertyName)
{
  return element.GetProperty(propertyName).GetInt32();
}

static string OracleString(JsonElement element, string propertyName)
{
  return element.GetProperty(propertyName).GetString() ??
    throw new InvalidOperationException($"The oracle property '{propertyName}' is null.");
}

static void VerifyHeaderCannotPassItsNextPointer()
{
  using MemoryStream input = CreateV88PointerWorld(headerLengthOverride: 1);
  AssertInvalidData(
    () => _ = WldWorldReader.Read(input),
    "A v88 header crossed its next section pointer.");
}

static void VerifyTileRleReader()
{
  using MemoryStream validInput = new([0x40, 0x01]);
  using WldBinaryReader validReader = new(validInput, WldReadLimits.Default, 0, validInput.Length);
  IReadOnlyList<LegacyTile> tiles = WldTileRleReader.Read(
    version: 319,
    reader: validReader,
    width: 1,
    height: 2,
    tileImportance: []);
  LegacyTile inactiveTile = new(false, 0, FrameX: -1, FrameY: -1);
  if (tiles.Count != 2 || tiles[0] != inactiveTile || tiles[1] != tiles[0])
  {
    throw new InvalidOperationException("A one-byte tile RLE run was not decoded.");
  }

  using MemoryStream activeInput = new([0x47, 0x01, 0x7F, 0x1E, 0x07, 0x03, 0x05, 0x04, 0x01, 0x01]);
  using WldBinaryReader activeReader = new(activeInput, WldReadLimits.Default, 0, activeInput.Length);
  IReadOnlyList<LegacyTile> activeTiles = WldTileRleReader.Read(319, activeReader, 1, 2, []);
  LegacyTile expectedTile = new(
    IsActive: true,
    TileType: 7,
    WallType: 261,
    HasWire4: true,
    IsActuated: true,
    IsInactive: true,
    FrameX: -1,
    FrameY: -1,
    TileColor: 3,
    WallColor: 4,
    IsInvisibleBlock: true,
    IsInvisibleWall: true,
    IsFullbrightBlock: true,
    IsFullbrightWall: true);
  if (activeTiles.Count != 2 || activeTiles[0] != expectedTile || activeTiles[1] != expectedTile)
  {
    throw new InvalidOperationException("The v319 tile flags were not decoded.");
  }

  using MemoryStream invalidInput = new([0x80, 0x02, 0x00]);
  using WldBinaryReader invalidReader = new(
    invalidInput,
    WldReadLimits.Default,
    0,
    invalidInput.Length);
  AssertInvalidData(
    () => _ = WldTileRleReader.Read(319, invalidReader, 1, 2, []),
    "A two-byte tile RLE run crossed the remaining column height.");

  VerifyLiquidWireAndSlopeTile();
  VerifyLiquidKinds();
  VerifySlopeTile();
  VerifyV88WideTileType();
  VerifyFrameImportantTile144();
}

static void VerifyLiquidWireAndSlopeTile()
{
  byte[] inputBytes = [0x09, 0x1F, 0x80, 0x99];
  using MemoryStream input = new(inputBytes);
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  IReadOnlyList<LegacyTile> tiles = WldTileRleReader.Read(319, reader, 1, 1, []);
  LegacyTile expectedTile = new(
    IsActive: false,
    TileType: 0,
    LiquidAmount: 0x99,
    LiquidKind: 3,
    HasWire: true,
    HasWire2: true,
    HasWire3: true,
    IsHalfBrick: true,
    FrameX: -1,
    FrameY: -1);
  if (tiles.Count != 1 || tiles[0] != expectedTile)
  {
    throw new InvalidOperationException(
      "Liquid, wiring, and half-brick tile flags were not decoded.");
  }
}

static void VerifyFrameImportantTile144()
{
  bool[] tileImportance = new bool[145];
  tileImportance[144] = true;
  byte[] inputBytes = [0x23, 0x00, 0x90, 0x00, 0x0C, 0x00, 0x22, 0x00];
  using MemoryStream input = new(inputBytes);
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  IReadOnlyList<LegacyTile> tiles = WldTileRleReader.Read(319, reader, 1, 1, tileImportance);
  LegacyTile expectedTile = new(
    IsActive: true,
    TileType: 144,
    FrameX: 12,
    FrameY: 0);
  if (tiles.Count != 1 || tiles[0] != expectedTile)
  {
    throw new InvalidOperationException("Frame-important tile 144 was not normalized.");
  }
}

static void VerifyLiquidKinds()
{
  VerifyLiquidKind([0x08, 0x11], expectedKind: 0, "water");
  VerifyLiquidKind([0x10, 0x22], expectedKind: 1, "lava");
  VerifyLiquidKind([0x18, 0x33], expectedKind: 2, "honey");
  VerifyLiquidKind([0x09, 0x01, 0x80, 0x44], expectedKind: 3, "shimmer");
}

static void VerifyLiquidKind(byte[] inputBytes, byte expectedKind, string name)
{
  using MemoryStream input = new(inputBytes);
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  IReadOnlyList<LegacyTile> tiles = WldTileRleReader.Read(319, reader, 1, 1, []);
  if (tiles.Count != 1 || tiles[0].LiquidKind != expectedKind || tiles[0].LiquidAmount == 0)
  {
    throw new InvalidOperationException($"The {name} liquid encoding was not decoded.");
  }
}

static void VerifySlopeTile()
{
  using MemoryStream input = new([0x01, 0x61, 0x00]);
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  IReadOnlyList<LegacyTile> tiles = WldTileRleReader.Read(319, reader, 1, 1, []);
  if (tiles.Count != 1 || tiles[0].IsHalfBrick || tiles[0].Slope != 5)
  {
    throw new InvalidOperationException("The WLD slope encoding was not decoded.");
  }
}

static void VerifyV88WideTileType()
{
  using MemoryStream input = new([0x23, 0x00, 0x2C, 0x01]);
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  IReadOnlyList<LegacyTile> tiles = WldTileRleReader.Read(88, reader, 1, 1, []);
  if (tiles.Count != 1 || !tiles[0].IsActive || tiles[0].TileType != 300)
  {
    throw new InvalidOperationException("The v88 16-bit tile type was not decoded.");
  }
}

static void VerifyInvalidPointerTables()
{
  AssertInvalidPointerTable(CreatePointerTable(version: 88, sectionCount: 5), "short count");
  AssertInvalidPointerTable(
    CreatePointerTable(version: 88, offsets: [-1, 40, 41, 42, 43, 44]),
    "negative offset");
  AssertInvalidPointerTable(
    CreatePointerTable(version: 88, offsets: [40, 41, 42, 43, 44, 100_000]),
    "out-of-file offset");
  AssertInvalidPointerTable(
    CreatePointerTable(version: 88, offsets: [40, 42, 41, 43, 44, 45]),
    "non-monotonic offsets");
  AssertInvalidPointerTable(
    CreatePointerTable(version: 88, offsets: [40, 41, 41, 43, 44, 45]),
    "overlapping sections");
}

static void VerifyV319PointerTable()
{
  using MemoryStream input = CreatePointerTable(version: 319);
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  int version = reader.ReadInt32();
  WldSectionPointerTable table = WldSectionPointerTable.Read(version, reader);
  if (table.SectionCount != 11 || table.Offsets.Count != 11)
  {
    throw new InvalidOperationException("The v319 section pointer table was not decoded.");
  }
}

static void VerifyPointerTablePreservesTileImportance()
{
  bool[] tileImportance = new bool[8];
  tileImportance[7] = true;
  using MemoryStream input = CreatePointerTable(version: 88, tileImportance: tileImportance);
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  int version = reader.ReadInt32();
  WldSectionPointerTable table = WldSectionPointerTable.Read(version, reader);
  if (table.TileImportance.Count != tileImportance.Length ||
      !table.TileImportance[7] ||
      table.TileImportance[0])
  {
    throw new InvalidOperationException("The WLD tile-importance bit map was not preserved.");
  }
}

static void VerifyV319PointerWorld()
{
  using MemoryStream input = CreateV319PointerWorld();
  LegacyWorldDocument document = WldWorldReader.Read(input);
  if (document.Version != 319 ||
      document.FormatVersion != WldFormatVersion.PointerTableV88ToV319 ||
      document.Metadata.Name != "V319 pointer fixture" ||
      document.Metadata.WorldId != 319 ||
      document.Metadata.Width != 2 ||
      document.Metadata.Height != 2 ||
      document.Metadata.WorldSurface != 0 ||
      document.Metadata.RockLayer != 0 ||
      document.Tiles.Count != 4 ||
      document.TileEntities.Count != 4 ||
      document.TileEntities[0].Id != 7 ||
      document.TileEntities[0].Type != 0 ||
      document.TileEntities[0].X != 0 ||
      document.TileEntities[0].Y != 1 ||
      !document.TileEntities[0].Payload.SequenceEqual([(byte)42, (byte)0]) ||
      document.TileEntities[1].Id != 8 ||
      document.TileEntities[1].Type != 1 ||
      document.TileEntities[1].X != 1 ||
      document.TileEntities[1].Y != 0 ||
      !document.TileEntities[1].Payload.SequenceEqual([(byte)70, (byte)0, (byte)4, (byte)2, (byte)0]) ||
      document.TileEntities[2].Id != 9 ||
      document.TileEntities[2].Type != 2 ||
      document.TileEntities[2].X != 1 ||
      document.TileEntities[2].Y != 1 ||
      !document.TileEntities[2].Payload.SequenceEqual([(byte)3, (byte)1]) ||
      document.TileEntities[3].Id != 10 ||
      document.TileEntities[3].Type != 4 ||
      document.TileEntities[3].X != 0 ||
      document.TileEntities[3].Y != 0 ||
      !document.TileEntities[3].Payload.SequenceEqual([(byte)71, (byte)0, (byte)5, (byte)1, (byte)0]) ||
      document.Diagnostics.Count != 4 ||
      document.Diagnostics[0].SectionIndex != 6 ||
      document.Diagnostics[1].SectionIndex != 7 ||
      document.Diagnostics[2].SectionIndex != 8 ||
      document.Diagnostics[3].SectionIndex != 9 ||
      !document.Diagnostics.All(diagnostic =>
        diagnostic.Message.Contains("preserved", StringComparison.OrdinalIgnoreCase)) ||
      document.Tiles.Any(tile => tile != new LegacyTile(false, 0, FrameX: -1, FrameY: -1)))
  {
    throw new InvalidOperationException("The v319 pointer-table fixture was not parsed.");
  }
}

static void VerifyV88PointerWorld()
{
  using MemoryStream input = CreateV88PointerWorld();
  LegacyWorldDocument document = WldWorldReader.Read(input);
  if (document.Version != 88 ||
      document.FormatVersion != WldFormatVersion.PointerTableV88ToV319 ||
      document.Metadata.Name != "Pointer fixture" ||
      document.Metadata.WorldId != 8642 ||
      document.Metadata.Width != 2 ||
      document.Metadata.Height != 2 ||
      document.Metadata.WorldSurface != 0 ||
      document.Metadata.RockLayer != 1 ||
      document.Tiles.Count != 4 ||
      document.Tiles.Any(tile => tile != new LegacyTile(false, 0, FrameX: -1, FrameY: -1)))
  {
    throw new InvalidOperationException("The v88 pointer-table fixture was not parsed.");
  }
}

static void VerifyTileEntityFoodPlatter()
{
  LegacyWorldMetadata metadata = new("Tile entity", 1, 2, 2, 0, 0);
  using MemoryStream input = new();
  using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(1);
    writer.Write((byte)6);
    writer.Write(11);
    writer.Write((short)0);
    writer.Write((short)1);
    writer.Write((short)72);
    writer.Write((byte)6);
    writer.Write((short)4);
  }

  input.Position = 0;
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  IReadOnlyList<LegacyTileEntity> entities = WldTileEntityReader.Read(
    319,
    reader,
    metadata,
    WldReadLimits.Default);
  if (entities.Count != 1 || entities[0].Type != 6 ||
      !entities[0].Payload.SequenceEqual([(byte)72, (byte)0, (byte)6, (byte)4, (byte)0]))
  {
    throw new InvalidOperationException("The food-platter tile entity was not parsed.");
  }
}

static void VerifyTileEntityDisplayJar()
{
  LegacyWorldMetadata metadata = new("Tile entity", 1, 2, 2, 0, 0);
  using MemoryStream input = new();
  using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(1);
    writer.Write((byte)8);
    writer.Write(12);
    writer.Write((short)1);
    writer.Write((short)0);
    writer.Write((short)73);
    writer.Write((byte)7);
    writer.Write((short)5);
  }

  input.Position = 0;
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  IReadOnlyList<LegacyTileEntity> entities = WldTileEntityReader.Read(
    319,
    reader,
    metadata,
    WldReadLimits.Default);
  if (entities.Count != 1 || entities[0].Type != 8 ||
      !entities[0].Payload.SequenceEqual([(byte)73, (byte)0, (byte)7, (byte)5, (byte)0]))
  {
    throw new InvalidOperationException("The display-jar tile entity was not parsed.");
  }
}

static void VerifyTileEntityAnchors()
{
  LegacyWorldMetadata metadata = new("Tile entity", 1, 2, 2, 0, 0);
  foreach ((byte type, short itemType) in new[] { ((byte)9, (short)81), ((byte)10, (short)82) })
  {
    using MemoryStream input = new();
    using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(1);
      writer.Write(type);
      writer.Write(13 + type);
      writer.Write((short)0);
      writer.Write((short)(type - 9));
      writer.Write(itemType);
    }

    input.Position = 0;
    using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
    IReadOnlyList<LegacyTileEntity> entities = WldTileEntityReader.Read(
      319,
      reader,
      metadata,
      WldReadLimits.Default);
    if (entities.Count != 1 || entities[0].Type != type ||
        !entities[0].Payload.SequenceEqual([(byte)itemType, (byte)(itemType >> 8)]))
    {
      throw new InvalidOperationException($"The type-{type} anchor entity was not parsed.");
    }
  }
}

static void VerifyTileEntityDisplayDollEmptyState()
{
  LegacyWorldMetadata metadata = new("Tile entity", 1, 2, 2, 0, 0);
  using MemoryStream input = new();
  using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(1);
    writer.Write((byte)3);
    writer.Write(14);
    writer.Write((short)0);
    writer.Write((short)0);
    writer.Write((byte)0);
    writer.Write((byte)0);
    writer.Write((byte)0);
    writer.Write((byte)0);
  }

  input.Position = 0;
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  IReadOnlyList<LegacyTileEntity> entities = WldTileEntityReader.Read(
    319,
    reader,
    metadata,
    WldReadLimits.Default);
  if (entities.Count != 1 || entities[0].Type != 3 ||
      !entities[0].Payload.SequenceEqual([(byte)0, (byte)0, (byte)0, (byte)0]))
  {
    throw new InvalidOperationException("The empty display-doll tile entity was not parsed.");
  }
}

static void VerifyTileEntityHatRackEmptyState()
{
  LegacyWorldMetadata metadata = new("Tile entity", 1, 2, 2, 0, 0);
  using MemoryStream input = new();
  using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(1);
    writer.Write((byte)5);
    writer.Write(15);
    writer.Write((short)1);
    writer.Write((short)1);
    writer.Write((byte)0);
  }

  input.Position = 0;
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  IReadOnlyList<LegacyTileEntity> entities = WldTileEntityReader.Read(
    319,
    reader,
    metadata,
    WldReadLimits.Default);
  if (entities.Count != 1 || entities[0].Type != 5 ||
      !entities[0].Payload.SequenceEqual([(byte)0]))
  {
    throw new InvalidOperationException("The empty hat-rack tile entity was not parsed.");
  }
}

static void VerifyTileEntityPylon()
{
  LegacyWorldMetadata metadata = new("Tile entity", 1, 2, 2, 0, 0);
  using MemoryStream input = new();
  using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(1);
    writer.Write((byte)7);
    writer.Write(16);
    writer.Write((short)0);
    writer.Write((short)1);
  }

  input.Position = 0;
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  IReadOnlyList<LegacyTileEntity> entities = WldTileEntityReader.Read(
    319,
    reader,
    metadata,
    WldReadLimits.Default);
  if (entities.Count != 1 || entities[0].Type != 7 || entities[0].Payload.Count != 0)
  {
    throw new InvalidOperationException("The pylon tile entity was not parsed.");
  }
}

static void VerifyTileEntityVariableItemPayloads()
{
  LegacyWorldMetadata metadata = new("Tile entity", 1, 2, 2, 0, 0);
  VerifyTileEntityPayload(
    metadata,
    type: 3,
    payload: [(byte)1, (byte)0, (byte)0, (byte)0, (byte)83, (byte)0, (byte)2, (byte)3, (byte)0]);
  VerifyTileEntityPayload(
    metadata,
    type: 5,
    payload: [(byte)4, (byte)84, (byte)0, (byte)3, (byte)4, (byte)0]);
}

static void VerifyUnknownTileEntityIsOpaque()
{
  LegacyWorldMetadata metadata = new("Tile entity", 1, 2, 2, 0, 0);
  using MemoryStream input = new();
  using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(1);
    writer.Write((byte)250);
    writer.Write(21);
    writer.Write((short)1);
    writer.Write((short)1);
    writer.Write((byte)0xAA);
    writer.Write((byte)0xBB);
  }

  input.Position = 0;
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  IReadOnlyList<LegacyTileEntity> entities = WldTileEntityReader.Read(
    319,
    reader,
    metadata,
    WldReadLimits.Default);
  if (entities.Count != 1 || !entities[0].IsOpaque ||
      !entities[0].Payload.SequenceEqual([(byte)0xAA, (byte)0xBB]))
  {
    throw new InvalidOperationException("The unknown tile entity was not retained as opaque data.");
  }
}

static void VerifyTileEntityPayload(
  LegacyWorldMetadata metadata,
  byte type,
  byte[] payload)
{
  using MemoryStream input = new();
  using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(1);
    writer.Write(type);
    writer.Write(20 + type);
    writer.Write((short)0);
    writer.Write((short)0);
    writer.Write(payload);
  }

  input.Position = 0;
  using WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length);
  IReadOnlyList<LegacyTileEntity> entities = WldTileEntityReader.Read(
    319,
    reader,
    metadata,
    WldReadLimits.Default);
  if (entities.Count != 1 || entities[0].Type != type ||
      !entities[0].Payload.SequenceEqual(payload))
  {
    throw new InvalidOperationException($"The type-{type} variable payload was not parsed.");
  }
}

static void VerifyV88ObjectSections()
{
  using MemoryStream input = CreateV88PointerWorld();
  LegacyWorldDocument document = WldWorldReader.Read(input);
  if (document.Chests.Count != 1 ||
      document.Chests[0].Name != "Pointer chest" ||
      document.Chests[0].Items.Count != 1 ||
      document.Chests[0].Items[0] != new LegacyChestItem(2, 73, 5) ||
      document.Signs.Count != 1 ||
      document.Signs[0] != new LegacySign(1, 1, "Pointer sign") ||
      document.Npcs.Count != 1 ||
      document.Npcs[0].Name != "Andrew" ||
      document.Npcs[0].LegacyTypeName != "Guide")
  {
    throw new InvalidOperationException("The v88 object sections were not parsed.");
  }
}

static void VerifyObjectSectionValidation()
{
  LegacyWorldMetadata metadata = new("Objects", 1, 2, 2, 0, 0);
  using MemoryStream chestInput = CreateChestSection(writer =>
  {
    writer.Write((short)1);
    writer.Write((short)1);
    writer.Write(2);
    writer.Write(0);
    writer.Write("Out of bounds");
    writer.Write((short)0);
  });
  using WldBinaryReader chestReader = new(
    chestInput,
    WldReadLimits.Default,
    0,
    chestInput.Length);
  AssertInvalidDataContains(
    () => _ = WldChestReader.Read(88, chestReader, metadata, WldReadLimits.Default),
    "version=88",
    "section=chests",
    "record=0");

  using MemoryStream negativeStackInput = CreateChestSection(writer =>
  {
    writer.Write((short)1);
    writer.Write((short)1);
    writer.Write(0);
    writer.Write(0);
    writer.Write("Negative stack");
    writer.Write((short)-1);
    writer.Write(1);
    writer.Write((byte)0);
  });
  using WldBinaryReader negativeStackReader = new(
    negativeStackInput,
    WldReadLimits.Default,
    0,
    negativeStackInput.Length);
  AssertInvalidData(
    () => _ = WldChestReader.Read(88, negativeStackReader, metadata, WldReadLimits.Default),
    "A negative chest stack was accepted.");

  using MemoryStream duplicateChestInput = CreateChestSection(writer =>
  {
    writer.Write((short)2);
    writer.Write((short)0);
    writer.Write(0);
    writer.Write(0);
    writer.Write("First");
    writer.Write(0);
    writer.Write(0);
    writer.Write("Duplicate");
  });
  using WldBinaryReader duplicateChestReader = new(
    duplicateChestInput,
    WldReadLimits.Default,
    0,
    duplicateChestInput.Length);
  AssertInvalidData(
    () => _ = WldChestReader.Read(88, duplicateChestReader, metadata, WldReadLimits.Default),
    "A duplicate chest coordinate was accepted.");

  using MemoryStream v319ChestInput = CreateChestSection(writer =>
  {
    writer.Write((short)1);
    writer.Write(0);
    writer.Write(0);
    writer.Write("V319 chest");
    writer.Write(1);
    writer.Write((short)3);
    writer.Write(22);
    writer.Write((byte)4);
  });
  using WldBinaryReader v319ChestReader = new(
    v319ChestInput,
    WldReadLimits.Default,
    0,
    v319ChestInput.Length);
  IReadOnlyList<LegacyChest> v319Chests = WldChestReader.Read(
    319,
    v319ChestReader,
    metadata,
    WldReadLimits.Default);
  if (v319Chests.Count != 1 || v319Chests[0].Items.Count != 1 ||
      v319Chests[0].Items[0] != new LegacyChestItem(3, 22, 4))
  {
    throw new InvalidOperationException("The v319 chest slot layout was not parsed.");
  }

  using MemoryStream signInput = new();
  using (BinaryWriter writer = new(signInput, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write((short)1);
    writer.Write("Too long");
    writer.Write(0);
    writer.Write(0);
  }

  signInput.Position = 0;
  WldReadLimits shortStringLimits = WldReadLimits.Default with { MaxStringByteLength = 3 };
  using WldBinaryReader signReader = new(signInput, shortStringLimits, 0, signInput.Length);
  AssertInvalidData(
    () => _ = WldSignReader.Read(88, signReader, metadata, shortStringLimits),
    "An overlong sign string was accepted.");

  using MemoryStream npcInput = new();
  using (BinaryWriter writer = new(npcInput, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(0);
    writer.Write(true);
    writer.Write(17);
    writer.Write("Guide");
    writer.Write(1.0f);
    writer.Write(2.0f);
    writer.Write(false);
    writer.Write(0);
    writer.Write(0);
    writer.Write((byte)1);
    writer.Write(2);
    writer.Write(false);
    writer.Write(false);
    writer.Write(false);
  }

  npcInput.Position = 0;
  using WldBinaryReader npcReader = new(npcInput, WldReadLimits.Default, 0, npcInput.Length);
  IReadOnlyList<LegacyNpc> npcs = WldNpcReader.Read(319, npcReader, WldReadLimits.Default);
  if (npcs.Count != 1 || npcs[0].Type != 17 || npcs[0].Name != "Guide" ||
      npcs[0].TownVariationIndex != 2 || npcs[0].HomelessDespawn)
  {
    throw new InvalidOperationException("The active v319 NPC record was not parsed.");
  }

  using MemoryStream footerInput = new();
  using (BinaryWriter writer = new(footerInput, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(true);
    writer.Write("Objects");
    writer.Write(1);
  }

  footerInput.Position = 0;
  using WldBinaryReader footerReader = new(footerInput, WldReadLimits.Default, 0, footerInput.Length);
  WldFooterReader.Read(88, footerReader, metadata);

  using MemoryStream mismatchedFooterInput = new();
  using (BinaryWriter writer = new(
    mismatchedFooterInput,
    System.Text.Encoding.UTF8,
    leaveOpen: true))
  {
    writer.Write(true);
    writer.Write("Other world");
    writer.Write(1);
  }

  mismatchedFooterInput.Position = 0;
  using WldBinaryReader mismatchedFooterReader = new(
    mismatchedFooterInput,
    WldReadLimits.Default,
    0,
    mismatchedFooterInput.Length);
  AssertInvalidData(
    () => WldFooterReader.Read(88, mismatchedFooterReader, metadata),
    "A mismatched footer identity was accepted.");
}

static MemoryStream CreateChestSection(Action<BinaryWriter> writeSection)
{
  MemoryStream input = new();
  using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writeSection(writer);
  }

  input.Position = 0;
  return input;
}

static void AssertInvalidPointerTable(MemoryStream input, string caseName)
{
  using (input)
  using (WldBinaryReader reader = new(input, WldReadLimits.Default, 0, input.Length))
  {
    int version = reader.ReadInt32();
    AssertInvalidData(
      () => _ = WldSectionPointerTable.Read(version, reader),
      $"The {caseName} pointer table was accepted.");
  }
}

static MemoryStream CreatePointerTable(
  int version,
  short? sectionCount = null,
  int[]? offsets = null,
  bool[]? tileImportance = null)
{
  int expectedSectionCount = version >= 220 ? 11 : 6;
  short actualSectionCount = sectionCount ?? (short)expectedSectionCount;
  MemoryStream input = new();
  using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(version);
    if (version >= 135)
    {
      writer.Write(0x026369676F6C6572UL);
      writer.Write(0U);
      writer.Write(0UL);
    }

    writer.Write(actualSectionCount);
    long offsetsPosition = input.Position;
    for (int index = 0; index < actualSectionCount; index++)
    {
      writer.Write(0);
    }

    bool[] importance = tileImportance ?? [];
    writer.Write(checked((ushort)importance.Length));
    for (int index = 0; index < importance.Length; index += 8)
    {
      byte packedImportance = 0;
      for (int bit = 0; bit < 8 && index + bit < importance.Length; bit++)
      {
        if (importance[index + bit])
        {
          packedImportance |= (byte)(1 << bit);
        }
      }

      writer.Write(packedImportance);
    }
    int firstSectionOffset = checked((int)input.Position);
    int[] actualOffsets = offsets ?? Enumerable.Range(0, actualSectionCount)
      .Select(index => firstSectionOffset + index)
      .ToArray();
    for (int index = 0; index < actualSectionCount; index++)
    {
      writer.Write((byte)0);
    }

    input.Position = offsetsPosition;
    foreach (int offset in actualOffsets)
    {
      writer.Write(offset);
    }
  }

  input.Position = 0;
  return input;
}

static MemoryStream CreateV88PointerWorld(int? headerLengthOverride = null)
{
  const short sectionCount = 6;
  MemoryStream input = new();
  using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(88);
    writer.Write(sectionCount);
    long offsetsPosition = input.Position;
    for (int index = 0; index < sectionCount; index++)
    {
      writer.Write(0);
    }

    writer.Write((ushort)0);
    int[] offsets = new int[sectionCount];
    offsets[0] = checked((int)input.Position);
    WriteV88Header(writer);
    int fullHeaderEnd = checked((int)input.Position);
    offsets[1] = headerLengthOverride is int length
      ? offsets[0] + length
      : fullHeaderEnd;
    if (headerLengthOverride is null)
    {
      offsets[1] = checked((int)input.Position);
      WriteDefaultTileSection(writer);
      offsets[2] = checked((int)input.Position);
      WriteV88Chests(writer);
      offsets[3] = checked((int)input.Position);
      WriteV88Signs(writer);
      offsets[4] = checked((int)input.Position);
      WriteV88Npcs(writer);
      offsets[5] = checked((int)input.Position);
      WriteV88Footer(writer);
    }
    else
    {
      for (int index = 1; index < sectionCount; index++)
      {
        if (index != 1)
        {
          offsets[index] = checked((int)input.Position);
        }

        writer.Write((byte)0);
      }
    }

    input.Position = offsetsPosition;
    foreach (int offset in offsets)
    {
      writer.Write(offset);
    }
  }

  input.Position = 0;
  return input;
}

static MemoryStream CreateV319PointerWorld()
{
  const short sectionCount = 11;
  MemoryStream input = new();
  using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(319);
    writer.Write(0x026369676F6C6572UL);
    writer.Write(0U);
    writer.Write(0UL);
    writer.Write(sectionCount);
    long offsetsPosition = input.Position;
    for (int index = 0; index < sectionCount; index++)
    {
      writer.Write(0);
    }

    writer.Write((ushort)0);
    int[] offsets = new int[sectionCount];
    offsets[0] = checked((int)input.Position);
    WriteV319Header(writer);
    for (int index = 1; index < sectionCount; index++)
    {
      offsets[index] = checked((int)input.Position);
      switch (index)
      {
        case 1:
          WriteDefaultTileSection(writer);
          break;
        case 2:
          writer.Write((short)0);
          break;
        case 3:
          writer.Write((short)0);
          break;
        case 4:
          writer.Write(0);
          writer.Write(false);
          writer.Write(false);
          break;
        case 5:
          writer.Write(4);
          writer.Write((byte)0);
          writer.Write(7);
          writer.Write((short)0);
          writer.Write((short)1);
          writer.Write((short)42);
          writer.Write((byte)1);
          writer.Write(8);
          writer.Write((short)1);
          writer.Write((short)0);
          writer.Write((short)70);
          writer.Write((byte)4);
          writer.Write((short)2);
          writer.Write((byte)2);
          writer.Write(9);
          writer.Write((short)1);
          writer.Write((short)1);
          writer.Write((byte)3);
          writer.Write(true);
          writer.Write((byte)4);
          writer.Write(10);
          writer.Write((short)0);
          writer.Write((short)0);
          writer.Write((short)71);
          writer.Write((byte)5);
          writer.Write((short)1);
          break;
        case 6:
          writer.Write(1);
          writer.Write(1);
          writer.Write(1);
          break;
        case 7:
          writer.Write(1);
          writer.Write(17);
          writer.Write(0);
          writer.Write(0);
          break;
        case 8:
          writer.Write(1);
          writer.Write("NPC.Guide");
          writer.Write(3);
          writer.Write(1);
          writer.Write("NPC.Guide");
          writer.Write(0);
          break;
        case 9:
          writer.Write(true);
          writer.Write((ushort)0);
          writer.Write((byte)1);
          writer.Write((byte)2);
          break;
        case 10:
          writer.Write(true);
          writer.Write("V319 pointer fixture");
          writer.Write(319);
          break;
        default:
          writer.Write((byte)0);
          break;
      }
    }

    input.Position = offsetsPosition;
    foreach (int offset in offsets)
    {
      writer.Write(offset);
    }
  }

  input.Position = 0;
  return input;
}

static void WriteDefaultTileSection(BinaryWriter writer)
{
  writer.Write((byte)0x40);
  writer.Write((byte)1);
  writer.Write((byte)0x40);
  writer.Write((byte)1);
}

static void WriteV88Chests(BinaryWriter writer)
{
  writer.Write((short)1);
  writer.Write((short)1);
  writer.Write(0);
  writer.Write(0);
  writer.Write("Pointer chest");
  writer.Write((short)2);
  writer.Write(73);
  writer.Write((byte)5);
}

static void WriteV88Signs(BinaryWriter writer)
{
  writer.Write((short)1);
  writer.Write("Pointer sign");
  writer.Write(1);
  writer.Write(1);
}

static void WriteV88Npcs(BinaryWriter writer)
{
  writer.Write(true);
  writer.Write("Guide");
  writer.Write("Andrew");
  writer.Write(1.0f);
  writer.Write(1.0f);
  writer.Write(false);
  writer.Write(0);
  writer.Write(0);
  writer.Write(false);
}

static void WriteV88Footer(BinaryWriter writer)
{
  writer.Write(true);
  writer.Write("Pointer fixture");
  writer.Write(8642);
}

static void WriteV88Header(BinaryWriter writer)
{
  writer.Write("Pointer fixture");
  writer.Write(8642);
  writer.Write(-84);
  writer.Write(84);
  writer.Write(-60);
  writer.Write(60);
  writer.Write(2);
  writer.Write(2);
  writer.Write((byte)0);
  for (int index = 0; index < 17; index++)
  {
    writer.Write(0);
  }

  writer.Write(1);
  writer.Write(1);
  writer.Write(0.0d);
  writer.Write(1.0d);
  writer.Write(2.0d);
  writer.Write(true);
  writer.Write(0);
  writer.Write(false);
  writer.Write(false);
  writer.Write(0);
  writer.Write(0);
  writer.Write(false);
  for (int index = 0; index < 3 + 1 + 4 + 2 + 4 + 3; index++)
  {
    writer.Write(false);
  }

  writer.Write(false);
  writer.Write(false);
  writer.Write((byte)0);
  writer.Write(0);
  writer.Write(false);
  writer.Write(0);
  writer.Write(0);
  writer.Write(0);
  writer.Write(0.0d);
  writer.Write(false);
  writer.Write(0);
  writer.Write(0.0f);
  for (int index = 0; index < 3; index++)
  {
    writer.Write(0);
  }

  for (int index = 0; index < 8; index++)
  {
    writer.Write((byte)0);
  }

  writer.Write(0);
  writer.Write((short)0);
  writer.Write(0.0f);
}

static void WriteV319Header(BinaryWriter writer)
{
  writer.Write("V319 pointer fixture");
  writer.Write("seed");
  writer.Write(0UL);
  writer.Write(new byte[16]);
  writer.Write(319);
  writer.Write(-84);
  writer.Write(84);
  writer.Write(-60);
  writer.Write(60);
  writer.Write(2);
  writer.Write(2);
  writer.Write(0);
  WriteBooleans(writer, 9);
  writer.Write(0L);
  writer.Write(0L);
  writer.Write((byte)0);
  WriteInt32Values(writer, 17);
  writer.Write(1);
  writer.Write(1);
  WriteDoubleValues(writer, 3);
  writer.Write(true);
  writer.Write(0);
  WriteBooleans(writer, 2);
  WriteInt32Values(writer, 2);
  WriteBooleans(writer, 1 + 3 + 1 + 4 + 2 + 1 + 4 + 3);
  writer.Write(false);
  writer.Write(false);
  writer.Write((byte)0);
  writer.Write(0);
  writer.Write(false);
  writer.Write(false);
  WriteInt32Values(writer, 3);
  WriteDoubleValues(writer, 2);
  writer.Write((byte)0);
  writer.Write(false);
  writer.Write(0);
  writer.Write(0.0f);
  WriteInt32Values(writer, 3);
  WriteByteValues(writer, 8);
  writer.Write(0);
  writer.Write((short)0);
  writer.Write(0.0f);
  writer.Write(0);
  writer.Write(false);
  writer.Write(0);
  WriteBooleans(writer, 3);
  WriteInt32Values(writer, 2);
  writer.Write((short)0);
  writer.Write((short)0);
  writer.Write(false);
  WriteBooleans(writer, 9);
  WriteBooleans(writer, 9);
  WriteBooleans(writer, 2);
  writer.Write(0);
  writer.Write(0);
  writer.Write(false);
  writer.Write(0);
  writer.Write(0.0f);
  writer.Write(0.0f);
  WriteBooleans(writer, 4);
  WriteByteValues(writer, 5);
  writer.Write(false);
  writer.Write(0);
  WriteBooleans(writer, 3);
  writer.Write(0);
  WriteBooleans(writer, 2);
  WriteInt32Values(writer, 4);
  WriteBooleans(writer, 3);
  WriteBooleans(writer, 2);
  WriteBooleans(writer, 1 + 1 + 8 + 1 + 1 + 7);
  writer.Write(false);
  writer.Write((byte)0);
  WriteBooleans(writer, 2 + 1 + 1);
  WriteInt32Values(writer, 2);
  writer.Write(false);
  writer.Write((byte)0);
  writer.Write(false);
  writer.Write("{}");
}

static void WriteBooleans(BinaryWriter writer, int count)
{
  for (int index = 0; index < count; index++)
  {
    writer.Write(false);
  }
}

static void WriteByteValues(BinaryWriter writer, int count)
{
  for (int index = 0; index < count; index++)
  {
    writer.Write((byte)0);
  }
}

static void WriteDoubleValues(BinaryWriter writer, int count)
{
  for (int index = 0; index < count; index++)
  {
    writer.Write(0.0d);
  }
}

static void WriteInt32Values(BinaryWriter writer, int count)
{
  for (int index = 0; index < count; index++)
  {
    writer.Write(0);
  }
}

static void VerifyFutureVersionIsRejected()
{
  using MemoryStream input = CreateHeaderOnlyInput(320);
  try
  {
    _ = WldWorldReader.Read(input);
  }
  catch (InvalidDataException exception) when (
    exception.Message.Contains("320", StringComparison.Ordinal) &&
    exception.Message.Contains("319", StringComparison.Ordinal))
  {
    return;
  }

  throw new InvalidOperationException(
    "WLD version 320 was not rejected with its version and the supported maximum.");
}

static MemoryStream CreateHeaderOnlyInput(int version)
{
  MemoryStream input = new();
  using (BinaryWriter writer = new(input, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(version);
  }

  input.Position = 0;
  return input;
}

sealed class NonSeekableReadStream : MemoryStream
{
  public NonSeekableReadStream(byte[] buffer)
    : base(buffer, writable: false)
  {
  }

  public override bool CanSeek => false;

  public override long Length => throw new NotSupportedException();

  public override long Position
  {
    get => throw new NotSupportedException();
    set => throw new NotSupportedException();
  }

  public override long Seek(long offset, SeekOrigin origin)
  {
    throw new NotSupportedException();
  }
}
