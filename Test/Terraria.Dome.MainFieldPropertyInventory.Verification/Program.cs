using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

const string sourcePath = @"D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs";
const int expectedAcceptedNarrowMappings = 127;
const int expectedMemberCount = 696;
const int expectedMigrationScopeCount = 695;
const int expectedIdentityExcludedCount = 1;
const int expectedFieldDeclarationCount = 659;
const int expectedFieldVariableCount = 659;
const int expectedPropertyDeclarationCount = 37;
const int expectedDeferredBoundaryNameCount = 0;
const string expectedDeferredBoundaryNameSha256 =
  "01BA4719C80B6FE911B091A7C05124B64EEECE964E09C058EF8F9805DACA546B";
const string expectedAcceptedNameSha256 =
  "66D2475601026A25673BDD1A97F8BAD8FE9BD75D09C7684BDF97A7DCFE174E47";
const string expectedSourceSha256 =
  "844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D";
if (!File.Exists(sourcePath))
{
  Console.Error.WriteLine($"ERROR: source file is missing: {sourcePath}");
  return 2;
}

string repositoryRoot = FindRepositoryRoot();
string source = File.ReadAllText(sourcePath);
CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(
  source, path: sourcePath).GetCompilationUnitRoot();
string sourceHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(sourcePath)));
if (!string.Equals(sourceHash, expectedSourceSha256, StringComparison.Ordinal))
{
  Console.Error.WriteLine(
    $"ERROR: source hash changed: expected {expectedSourceSha256}, actual {sourceHash}");
  return 1;
}
HashSet<string> excluded = new(StringComparer.Ordinal)
{
  "NPC.netID",
  "Main.worldID",
  "Projectile.identity",
  "MessageBuffer.whoAmI"
};

List<MemberEvidence> members = new();
IReadOnlyList<IdentifierNameSyntax> identifiers =
  root.DescendantNodes().OfType<IdentifierNameSyntax>().ToList();
foreach (MemberDeclarationSyntax member in root.DescendantNodes().OfType<MemberDeclarationSyntax>())
{
  if (member is not FieldDeclarationSyntax and not PropertyDeclarationSyntax)
  {
    continue;
  }

  string containingType = member.Ancestors().OfType<TypeDeclarationSyntax>()
    .Select(type => type.Identifier.ValueText).FirstOrDefault() ?? "<global>";
  if (!string.Equals(containingType, "Main", StringComparison.Ordinal))
  {
    continue;
  }
  IEnumerable<(string name, string type, string defaultValue, string mutability)> declarations =
    ReadDeclarations(member);
  foreach ((string name, string type, string defaultValue, string mutability) in declarations)
  {
    string symbolId = $"{containingType}.{name}";
    bool isExcluded = excluded.Contains(symbolId);
    IReadOnlyList<IdentifierNameSyntax> references = identifiers.Where(identifier =>
      identifier.Identifier.ValueText == name &&
      !member.Span.Contains(identifier.Span)).ToList();
    int writeCount = references.Count(IsWriteReference);
    MemberMapping mapping = ResolveMapping(name, isExcluded);
    members.Add(new MemberEvidence(
      symbolId,
      containingType,
      name,
      member is FieldDeclarationSyntax ? "field" : "property",
      type,
      member.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
      member.Modifiers.Any(modifier => modifier.Text == "static"),
      references.Count - writeCount,
      writeCount,
      defaultValue,
      mutability,
      mapping.Status,
      mapping.Owner,
      mapping.Serialization,
      mapping.Verification));
  }
}

members = members.OrderBy(member => member.Line)
  .ThenBy(member => member.SymbolId, StringComparer.Ordinal)
  .ToList();
if (members.Select(member => member.SymbolId).Distinct(StringComparer.Ordinal).Count() !=
    members.Count)
{
  Console.Error.WriteLine("ERROR: inventory contains duplicate SymbolId values.");
  return 1;
}
if (members.Any(member => member.Status == "accepted-narrow" &&
    (member.Owner is "deferred" or "" || member.Serialization is "unverified" or "" ||
      member.Verification is "unverified" or "")) ||
    members.Any(member => member.Status == "unmapped" &&
      (member.Owner != "deferred" || member.Serialization != "unverified" ||
        member.Verification != "unverified")))
{
  Console.Error.WriteLine("ERROR: inventory mapping status and evidence are inconsistent.");
  return 1;
}
string[] deferredBoundaryNames =
[
];
if (deferredBoundaryNames.Length != expectedDeferredBoundaryNameCount)
{
  Console.Error.WriteLine(
    $"ERROR: deferred boundary list changed: expected {expectedDeferredBoundaryNameCount}, " +
    $"actual {deferredBoundaryNames.Length}");
  return 1;
}
string deferredBoundaryNameText = string.Join(
  "\n",
  deferredBoundaryNames.OrderBy(name => name, StringComparer.Ordinal)) + "\n";
string deferredBoundaryNameSha256 = Convert.ToHexString(
  SHA256.HashData(Encoding.UTF8.GetBytes(deferredBoundaryNameText)));
if (!string.Equals(
    deferredBoundaryNameSha256,
    expectedDeferredBoundaryNameSha256,
    StringComparison.Ordinal))
{
  Console.Error.WriteLine(
    $"ERROR: deferred boundary names changed: expected {expectedDeferredBoundaryNameSha256}, " +
    $"actual {deferredBoundaryNameSha256}");
  return 1;
}
if (members.Any(member => deferredBoundaryNames.Contains(
    member.Name, StringComparer.Ordinal) && member.Status != "unmapped"))
{
  Console.Error.WriteLine("ERROR: a deferred boundary field was promoted without re-audit.");
  return 1;
}
int deferredBoundaryCoverage = deferredBoundaryNames.Count(name =>
  members.Any(member => member.Name == name && member.Status == "unmapped"));
if (deferredBoundaryCoverage != deferredBoundaryNames.Length)
{
  Console.Error.WriteLine(
    $"ERROR: deferred boundary coverage changed: expected {deferredBoundaryNames.Length}, " +
    $"actual {deferredBoundaryCoverage}");
  return 1;
}
IEnumerable<string> acceptedVerificationProjects = members
  .Where(member => member.Status == "accepted-narrow")
  .Select(member => member.Verification)
  .Distinct(StringComparer.Ordinal);
foreach (string verificationProject in acceptedVerificationProjects)
{
  const string projectPrefix = "Terraria.Dome.";
  if (!verificationProject.StartsWith(projectPrefix, StringComparison.Ordinal))
  {
    Console.Error.WriteLine(
      $"ERROR: accepted mapping has invalid verification project: {verificationProject}");
    return 1;
  }

  string projectName = verificationProject[projectPrefix.Length..];
  string projectDirectory = Path.Combine(
    repositoryRoot, "Test", verificationProject);
  if (!Directory.Exists(projectDirectory) &&
      !Directory.Exists(Path.Combine(repositoryRoot, "Test", projectName)))
  {
    Console.Error.WriteLine(
      $"ERROR: accepted mapping verification project is missing: {verificationProject}");
    return 1;
  }
}
InventoryEvidence evidence = new(
  "main-field-property-v1",
  sourcePath,
  sourceHash,
  members.Count,
  members.Count(member => member.Status != "identity-excluded"),
  members.Count(member => member.Status == "identity-excluded"),
  root.DescendantNodes().OfType<FieldDeclarationSyntax>().Count(field =>
    field.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault()?.Identifier.ValueText ==
    "Main"),
  root.DescendantNodes().OfType<FieldDeclarationSyntax>().Where(field =>
    field.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault()?.Identifier.ValueText ==
    "Main")
    .Sum(field => field.Declaration.Variables.Count),
  root.DescendantNodes().OfType<PropertyDeclarationSyntax>().Count(property =>
    property.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault()?.Identifier.ValueText ==
    "Main"),
  685,
  "lexical-baseline-and-roslyn-symbol-baseline-are-distinct; use-symbols-for-deletion-gate",
  members,
  AuditIdentityDeclarations(sourcePath));
if (evidence.MemberCount != expectedMemberCount ||
    evidence.MigrationScopeCount != expectedMigrationScopeCount ||
    evidence.IdentityExcludedCount != expectedIdentityExcludedCount ||
    evidence.FieldDeclarationCount != expectedFieldDeclarationCount ||
    evidence.FieldVariableCount != expectedFieldVariableCount ||
    evidence.PropertyDeclarationCount != expectedPropertyDeclarationCount)
{
  Console.Error.WriteLine(
    $"ERROR: inventory shape changed: members={evidence.MemberCount}, " +
    $"scope={evidence.MigrationScopeCount}, identity={evidence.IdentityExcludedCount}, " +
    $"fields={evidence.FieldDeclarationCount}, properties={evidence.PropertyDeclarationCount}");
  return 1;
}
int acceptedNarrowMappings = members.Count(member => member.Status == "accepted-narrow");
if (acceptedNarrowMappings != expectedAcceptedNarrowMappings)
{
  Console.Error.WriteLine(
    $"ERROR: accepted-narrow mapping count changed: expected " +
    $"{expectedAcceptedNarrowMappings}, actual {acceptedNarrowMappings}");
  return 1;
}
string acceptedNameText = string.Join(
  "\n",
  members.Where(member => member.Status == "accepted-narrow")
    .Select(member => member.SymbolId)
    .OrderBy(symbolId => symbolId, StringComparer.Ordinal)) + "\n";
string acceptedNameSha256 = Convert.ToHexString(
  SHA256.HashData(Encoding.UTF8.GetBytes(acceptedNameText)));
if (!string.Equals(acceptedNameSha256, expectedAcceptedNameSha256, StringComparison.Ordinal))
{
  Console.Error.WriteLine(
    $"ERROR: accepted-narrow name set changed: expected {expectedAcceptedNameSha256}, " +
    $"actual {acceptedNameSha256}");
  return 1;
}
if (!AuditSeedRuntimeProjection(repositoryRoot))
{
  return 1;
}
string outputDirectory = Path.Combine(
  repositoryRoot, "Build", "diagnostics", "main-field-property");
Directory.CreateDirectory(outputDirectory);
string outputPath = Path.Combine(outputDirectory, "inventory.json");
JsonSerializerOptions options = new() { WriteIndented = true };
File.WriteAllText(outputPath, JsonSerializer.Serialize(evidence, options));
if (!ValidateArtifact(outputPath, evidence, acceptedNarrowMappings))
{
  return 1;
}
Console.WriteLine(
  $"PASS: members={evidence.MemberCount} migratedScope={evidence.MigrationScopeCount} " +
  $"identityExcluded={evidence.IdentityExcludedCount}");
Console.WriteLine(
  $"COUNTING: fieldDeclarations={evidence.FieldDeclarationCount} " +
  $"fieldVariables={evidence.FieldVariableCount} properties={evidence.PropertyDeclarationCount}");
Console.WriteLine($"IDENTITY-AUDIT: {evidence.IdentityAudit.Count(identity => identity.Found)}/" +
  $"{evidence.IdentityAudit.Count} declarations found");
Console.WriteLine($"MAPPING-AUDIT: accepted-narrow={acceptedNarrowMappings}");
Console.WriteLine(
  $"DEFERRED-GUARD-AUDIT: {deferredBoundaryCoverage}/{deferredBoundaryNames.Length}");
Console.WriteLine($"EVIDENCE: {Path.GetRelativePath(repositoryRoot, outputPath)}");
return evidence.IdentityAudit.All(identity => identity.Found) ? 0 : 1;

static bool AuditSeedRuntimeProjection(string repositoryRoot)
{
  string projectionPath = Path.Combine(
    repositoryRoot,
    "src",
    "Terraria.Dome.Simulation",
    "WorldGeneration",
    "SecretSeedRuntimeProjection.cs");
  if (!File.Exists(projectionPath))
  {
    Console.Error.WriteLine($"ERROR: seed projection source is missing: {projectionPath}");
    return false;
  }

  string source = File.ReadAllText(projectionPath);
  CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(source, path: projectionPath)
    .GetCompilationUnitRoot();
  RecordDeclarationSyntax? projection = root.DescendantNodes()
    .OfType<RecordDeclarationSyntax>()
    .FirstOrDefault(record => record.Identifier.ValueText == "SecretSeedRuntimeProjection");
  if (projection is null)
  {
    Console.Error.WriteLine("ERROR: SecretSeedRuntimeProjection declaration is missing.");
    return false;
  }

  string[] requiredMembers =
  [
    "VampireSeed",
    "InfectedSeed",
    "TeamBasedSpawnsSeed",
    "DualDungeonsSeed"
  ];
  bool membersPresent = requiredMembers.All(member =>
    projection.ParameterList?.Parameters.Any(parameter =>
      parameter.Identifier.ValueText == member) == true);
  string[] requiredVariants =
  [
    "vampirism",
    "world-is-infected",
    "team-based-spawns",
    "dual-dungeons"
  ];
  bool variantsPresent = requiredVariants.All(variant => source.Contains(
    $"\"{variant}\"",
    StringComparison.Ordinal));
  bool passed = membersPresent && variantsPresent;
  Console.WriteLine($"SEED-PROJECTION-AUDIT: {(passed ? "passed" : "failed")} " +
    $"members={membersPresent} variants={variantsPresent}");
  return passed;
}

static bool ValidateArtifact(
  string outputPath,
  InventoryEvidence expected,
  int expectedAcceptedNarrowMappings)
{
  InventoryEvidence? actual = JsonSerializer.Deserialize<InventoryEvidence>(
    File.ReadAllText(outputPath));
  int actualAcceptedNarrowMappings = actual?.Members.Count(member =>
    member.Status == "accepted-narrow") ?? -1;
  bool passed = actual is not null &&
    actual.SourceSha256 == expected.SourceSha256 &&
    actual.MemberCount == expected.MemberCount &&
    actual.MigrationScopeCount == expected.MigrationScopeCount &&
    actualAcceptedNarrowMappings == expectedAcceptedNarrowMappings;
  Console.WriteLine($"ARTIFACT-AUDIT: {(passed ? "passed" : "failed")} " +
    $"members={actual?.MemberCount ?? -1} accepted-narrow={actualAcceptedNarrowMappings}");
  return passed;
}

static IEnumerable<(string name, string type, string defaultValue, string mutability)>
  ReadDeclarations(
  MemberDeclarationSyntax member)
{
  if (member is FieldDeclarationSyntax field)
  {
    string type = field.Declaration.Type.ToString();
    string defaultValue = field.Declaration.Variables.Count == 1
      ? field.Declaration.Variables[0].Initializer?.Value.ToString() ?? "<none>"
      : "<multiple>";
    string mutability = field.Modifiers.Any(modifier => modifier.Text is "const" or "readonly")
      ? string.Join(" ", field.Modifiers.Select(modifier => modifier.Text))
      : "mutable";
    return field.Declaration.Variables.Select(variable =>
      (variable.Identifier.ValueText, type, variable.Initializer?.Value.ToString() ?? defaultValue,
        mutability));
  }

  PropertyDeclarationSyntax property = (PropertyDeclarationSyntax)member;
  bool hasSetter = property.AccessorList?.Accessors.Any(
    accessor => accessor.Keyword.Text == "set") == true;
  string accessorMutability = hasSetter
    ? "mutable"
    : "readonly";
  return [(property.Identifier.ValueText, property.Type.ToString(),
    property.Initializer?.Value.ToString() ?? "<none>", accessorMutability)];
}

static bool IsWriteReference(IdentifierNameSyntax identifier)
{
  SyntaxNode? parent = identifier.Parent;
  if (parent is AssignmentExpressionSyntax assignment &&
      assignment.Left.Span.Contains(identifier.Span))
  {
    return true;
  }

  if (parent is PrefixUnaryExpressionSyntax prefix)
  {
    return prefix.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PreIncrementExpression) ||
      prefix.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PreDecrementExpression);
  }

  if (parent is PostfixUnaryExpressionSyntax postfix)
  {
    return postfix.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PostIncrementExpression) ||
      postfix.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PostDecrementExpression);
  }

  return false;
}

static MemberMapping ResolveMapping(string name, bool isExcluded)
{
  if (isExcluded)
  {
    return new("identity-excluded", "identity", "identity", "identity");
  }

  return name switch
  {
    "worldName" => new(
      "accepted-narrow", "WorldMetadata.Name", "DomeStatePersistenceFormat.WorldMetadata",
      "Terraria.Dome.Persistence.Verification"),
    "maxTilesX" => new(
      "accepted-narrow", "WorldMetadata.Width", "DomeStatePersistenceFormat.WorldMetadata",
      "Terraria.Dome.Persistence.Verification"),
    "maxTilesY" => new(
      "accepted-narrow", "WorldMetadata.Height", "DomeStatePersistenceFormat.WorldMetadata",
      "Terraria.Dome.Persistence.Verification"),
    "leftWorld" => new(
      "accepted-narrow", "WorldMetadata.LeftWorld", "derived-from-WorldMetadata.Width",
      "Terraria.Dome.Persistence.Verification"),
    "rightWorld" => new(
      "accepted-narrow", "WorldMetadata.RightWorld", "derived-from-WorldMetadata.Width",
      "Terraria.Dome.Persistence.Verification"),
    "topWorld" => new(
      "accepted-narrow", "WorldMetadata.TopWorld", "derived-from-WorldMetadata.Height",
      "Terraria.Dome.Persistence.Verification"),
    "bottomWorld" => new(
      "accepted-narrow", "WorldMetadata.BottomWorld", "derived-from-WorldMetadata.Height",
      "Terraria.Dome.Persistence.Verification"),
    "maxSectionsX" => new(
      "accepted-narrow", "WorldMetadata.MaxSectionsX", "derived-from-WorldMetadata.Width",
      "Terraria.Dome.Persistence.Verification"),
    "maxSectionsY" => new(
      "accepted-narrow", "WorldMetadata.MaxSectionsY", "derived-from-WorldMetadata.Height",
      "Terraria.Dome.Persistence.Verification"),
    "sectionWidth" => new(
      "accepted-narrow", "WorldGrid.SectionWidth", "derived-constant",
      "Terraria.Dome.MainBoundary.Verification"),
    "sectionHeight" => new(
      "accepted-narrow", "WorldGrid.SectionHeight", "derived-constant",
      "Terraria.Dome.MainBoundary.Verification"),
    "spawnTileX" => new(
      "accepted-narrow", "WorldMetadata.SpawnX", "DomeStatePersistenceFormat.WorldMetadata",
      "Terraria.Dome.Persistence.Verification"),
    "spawnTileY" => new(
      "accepted-narrow", "WorldMetadata.SpawnY", "DomeStatePersistenceFormat.WorldMetadata",
      "Terraria.Dome.Persistence.Verification"),
    "worldSurface" => new(
      "accepted-narrow", "WorldMetadata.WorldSurface", "DomeStatePersistenceFormat.WorldSurface",
      "Terraria.Dome.WorldRules.Verification"),
    "WorldGeneratorVersion" => new(
      "accepted-narrow", "WorldMetadata.WorldGeneratorVersion",
      "DomeStatePersistenceFormat.WorldGeneratorVersion",
      "Terraria.Dome.Persistence.Verification"),
    "hardMode" => new(
      "accepted-narrow", "WorldProgressionState.IsHardMode",
      "DomeStatePersistenceFormat.WorldProgression", "Terraria.Dome.WorldRules.Verification"),
    "bloodMoon" => new(
      "accepted-narrow", "WorldProgressionState.IsBloodMoon",
      "DomeStatePersistenceFormat.WorldProgression", "Terraria.Dome.WorldRules.Verification"),
    "eclipse" => new(
      "accepted-narrow", "WorldProgressionState.IsEclipse",
      "DomeStatePersistenceFormat.WorldProgression", "Terraria.Dome.WorldRules.Verification"),
    "slimeRain" => new(
      "accepted-narrow", "WorldProgressionState.IsSlimeRaining",
      "DomeStatePersistenceFormat.WorldProgression", "Terraria.Dome.WorldRules.Verification"),
    "invasionType" => new(
      "accepted-narrow", "WorldProgressionState.InvasionType",
      "DomeStatePersistenceFormat.WorldProgression", "Terraria.Dome.WorldRules.Verification"),
    "invasionSize" => new(
      "accepted-narrow", "WorldProgressionState.InvasionSize",
      "DomeStatePersistenceFormat.WorldProgression", "Terraria.Dome.WorldRules.Verification"),
    "invasionSizeStart" => new(
      "accepted-narrow", "WorldProgressionState.InvasionSizeStart",
      "DomeStatePersistenceFormat.WorldProgression", "Terraria.Dome.WorldRules.Verification"),
    "invasionDelay" => new(
      "accepted-narrow", "WorldProgressionState.InvasionDelayTicks",
      "DomeStatePersistenceFormat.WorldProgression", "Terraria.Dome.WorldRules.Verification"),
    "invasionX" => new(
      "accepted-narrow", "WorldProgressionState.InvasionX",
      "DomeStatePersistenceFormat.WorldProgression", "Terraria.Dome.WorldRules.Verification"),
    "slimeRainTime" => new(
      "accepted-narrow", "WorldProgressionState.SlimeRainTimeTicks",
      "DomeStatePersistenceFormat.WorldProgression", "Terraria.Dome.WorldRules.Verification"),
    "raining" => new(
      "accepted-narrow", "WorldRuleState.IsRaining", "DomeStatePersistenceFormat.WorldRules",
      "Terraria.Dome.WorldRules.Verification"),
    "rainTime" => new(
      "accepted-narrow", "WorldRuleState.RainTimeTicks", "DomeStatePersistenceFormat.WorldRules",
      "Terraria.Dome.WorldRules.Verification"),
    "maxRaining" => new(
      "accepted-narrow", "WorldRuleState.MaximumRainStrength",
      "DomeStatePersistenceFormat.WorldRules", "Terraria.Dome.WorldRules.Verification"),
    "windSpeedCurrent" => new(
      "accepted-narrow", "WorldRuleState.WindSpeedCurrent",
      "DomeStatePersistenceFormat.WorldRules", "Terraria.Dome.WorldRules.Verification"),
    "windSpeedTarget" => new(
      "accepted-narrow", "WorldRuleState.WindSpeedTarget",
      "DomeStatePersistenceFormat.WorldRules", "Terraria.Dome.WorldRules.Verification"),
    "maxPlayers" => new(
      "accepted-narrow", "SimulationEntityLimits.MaximumPlayers",
      "WorldBootstrapRequest.EntityLimits", "Terraria.Dome.World.Server.Verification"),
    "maxNPCs" => new(
      "accepted-narrow", "SimulationEntityLimits.MaximumNpcs",
      "WorldBootstrapRequest.EntityLimits", "Terraria.Dome.World.Server.Verification"),
    "maxProjectiles" => new(
      "accepted-narrow", "SimulationEntityLimits.MaximumProjectiles",
      "WorldBootstrapRequest.EntityLimits", "Terraria.Dome.World.Server.Verification"),
    "maxItems" => new(
      "accepted-narrow", "SimulationEntityLimits.MaximumWorldItems",
      "WorldBootstrapRequest.EntityLimits", "Terraria.Dome.World.Server.Verification"),
    "maxChests" => new(
      "accepted-narrow", "SimulationEntityLimits.MaximumChests",
      "WorldBootstrapRequest.EntityLimits", "Terraria.Dome.World.Server.Verification"),
    "WorldRollingBackupsCountToKeep" => new(
      "accepted-narrow", "WorldSaveCoordinator.DefaultRollingBackupsCountToKeep",
      "WorldSaveCoordinator.Save rolling-backup policy", "Terraria.Dome.Persistence.Verification"),
    "maxNetPlayers" => new(
      "deferred-host", "legacy network player-slot limit",
      "server-transport-only", "Terraria.Dome.World.Server.Verification"),
    "dedServ" => new(
      "deferred-host", "DomeServer hosting mode", "server-host-only",
      "Terraria.Dome.World.Server.Verification"),
    "netMode" => new(
      "deferred-host", "DomeServer transport mode", "server-transport-only",
      "Terraria.Dome.World.Server.Verification"),
    "_targetNetMode" => new(
      "deferred-host", "DomeServer transport transition", "server-transport-only",
      "Terraria.Dome.World.Server.Verification"),
    "MaxTimeout" => new(
      "deferred-host", "DomeServer session timeout policy", "server-transport-only",
      "Terraria.Dome.World.Server.Verification"),
    "rockLayer" => new(
      "accepted-narrow", "WorldMetadata.RockLayer",
      "WorldPersistenceFormat + DomeStatePersistenceFormat + CompatibilityToDomeProjection",
      "Terraria.Dome.Persistence.Verification"),
    "remixWorld" => new(
      "accepted-narrow", "WorldMetadata.IsRemixWorld",
      "DomeStatePersistenceFormat.WorldMetadata", "Terraria.Dome.WorldRules.Verification"),
    "dayTime" => new(
      "accepted-narrow", "WorldClock.IsDayTime", "DomeStatePersistenceFormat.WorldClock",
      "Terraria.Dome.WorldRules.Verification"),
    "GlobalTimeWrappedHourly" => new(
      "accepted-narrow", "WorldClock.GlobalTimeWrappedHourly", "derived-monotonic-clock-modulo",
      "Terraria.Dome.WorldClock.Verification"),
    "time" => new(
      "accepted-narrow", "WorldClock.TimeOfDay", "DomeStatePersistenceFormat.WorldClock",
      "Terraria.Dome.WorldRules.Verification"),
    "moonPhase" => new(
      "accepted-narrow", "WorldClock.MoonPhase", "DomeStatePersistenceFormat.WorldClock",
      "Terraria.Dome.WorldRules.Verification"),
    "GlobalTimerPaused" => new(
      "accepted-narrow", "WorldClock.IsPaused", "DomeStatePersistenceFormat.WorldClock",
      "Terraria.Dome.WorldRules.Verification"),
    "dayLength" => new(
      "accepted-narrow", "WorldClock.DefaultDayLengthTicks", "derived-constant",
      "Terraria.Dome.WorldClock.Verification"),
    "nightLength" => new(
      "accepted-narrow", "WorldClock.DefaultNightLengthTicks", "derived-constant",
      "Terraria.Dome.WorldClock.Verification"),
    "dayRate" => new(
      "accepted-narrow", "WorldTimeRateSnapshot.Rate", "DomeStatePersistenceFormat.WorldTimeRate",
      "Terraria.Dome.WorldRules.Verification"),
    "GameUpdateCount" => new(
      "accepted-narrow", "WorldGameUpdateCountProjection.Value",
      "transient-instance-local; excluded from DomeStatePersistenceFormat",
      "Terraria.Dome.WorldClock.Verification"),
    "desiredWorldTilesUpdateRate" => new(
      "accepted-narrow", "WorldUpdateRatePolicy.GetRate",
      "derived-world-update-rate-policy", "Terraria.Dome.WorldGeneration.Verification"),
    "tileRope" => new(
      "accepted-narrow", "TileRopeQuery.RegisterDefaults",
      "derived-immutable-tile-rope-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileFrameImportant" => new(
      "accepted-narrow", "TileFrameImportantRegistry.RegisterDefaults",
      "derived-immutable-tile-frame-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileCut" => new(
      "accepted-narrow", "LegacyTileRunnerCandidateRegistry.IsTileCut",
      "derived-immutable-tile-cut-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileMoss" => new(
      "accepted-narrow", "MossTileTypeRegistry.TileTypes",
      "derived-immutable-tile-moss-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileStone" => new(
      "accepted-narrow", "LegacyTileRunnerTargetRegistry.IsStone",
      "derived-immutable-tile-stone-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileDungeon" => new(
      "accepted-narrow", "LegacyEvilReplacementDefinitions.CreateDefault().DungeonTileTypes",
      "derived-immutable-dungeon-tile-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileWaterDeath" => new(
      "accepted-narrow", "TileDefinition.WaterDestroysTile",
      "TileDefinitionRegistry.RegisterDefaults", "Terraria.Dome.WorldGeneration.Verification"),
    "tileLavaDeath" => new(
      "accepted-narrow", "TileDefinition.LavaDestroysTile",
      "TileDefinitionRegistry.RegisterDefaults", "Terraria.Dome.Liquid.Verification"),
    "tileSand" => new(
      "accepted-narrow", "ConversionSandTileRegistry.RegisterDefaults",
      "derived-immutable-conversion-sand-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileCracked" => new(
      "accepted-narrow", "TileSolidityOverrideQuery.RegisterCrackedBrickDefaults",
      "derived-immutable-cracked-brick-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "wallDungeon" => new(
      "accepted-narrow", "LegacyEvilReplacementDefinitions.CreateDefault().DungeonWallTypes",
      "derived-immutable-dungeon-wall-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileContainer" => new(
      "accepted-narrow", "LegacyTileContainerRegistry.RegisterDefaults()",
      "derived-immutable-legacy-container-registry", "Terraria.Dome.Wiring.Verification"),
    "tileSign" => new(
      "accepted-narrow", "LegacySignTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-sign-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileSolidTop" => new(
      "accepted-narrow", "LegacySolidTopTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-solid-top-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileSolid" => new(
      "accepted-narrow", "LegacySolidTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-solid-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileHammer" => new(
      "accepted-narrow", "LegacyHammerTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-hammer-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileAxe" => new(
      "accepted-narrow", "LegacyAxeTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-axe-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileTable" => new(
      "accepted-narrow", "LegacyTileTableRegistry.RegisterDefaults()",
      "derived-immutable-legacy-table-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileNoFail" => new(
      "accepted-narrow", "LegacyNoFailTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-no-fail-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileBouncy" => new(
      "accepted-narrow", "LegacyBouncyTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-bouncy-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileAlch" => new(
      "accepted-narrow", "LegacyAlchemicalTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-alchemical-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "wallLight" => new(
      "accepted-narrow", "LegacyWallLightRegistry.RegisterDefaults()",
      "derived-immutable-legacy-wall-light-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "wallHouse" => new(
      "accepted-narrow", "LegacyHouseWallRegistry.RegisterDefaults()",
      "derived-immutable-legacy-house-wall-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileLargeFrames" => new(
      "accepted-narrow", "LegacyLargeFrameTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-large-frame-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "wallLargeFrames" => new(
      "accepted-narrow", "LegacyLargeFrameWallRegistry.RegisterDefaults()",
      "derived-immutable-legacy-wall-large-frame-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "anglerQuestItemNetIDs" => new(
      "accepted-narrow", "LegacyAnglerQuestItemRegistry.RegisterDefaults()",
      "derived-immutable-legacy-angler-quest-item-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "projFrames" => new(
      "accepted-narrow", "LegacyProjectileFrameRegistry.RegisterDefaults()",
      "derived-immutable-legacy-projectile-frame-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "projHook" => new(
      "accepted-narrow", "LegacyProjectileHookRegistry.RegisterDefaults()",
      "derived-immutable-legacy-projectile-hook-registry",
      "Terraria.Dome.ProjectileHook.Verification"),
    "projHostile" => new(
      "accepted-narrow", "LegacyProjectileHostileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-projectile-hostile-registry",
      "Terraria.Dome.ProjectileHostile.Verification"),
    "buffNoSave" => new(
      "accepted-narrow", "LegacyBuffNoSaveRegistry.RegisterDefaults()",
      "derived-immutable-legacy-buff-no-save-registry",
      "Terraria.Dome.BuffNoSave.Verification"),
    "buffNoTimeDisplay" => new(
      "accepted-narrow", "LegacyBuffNoTimeDisplayRegistry.RegisterDefaults()",
      "derived-immutable-legacy-buff-no-time-display-registry",
      "Terraria.Dome.BuffNoTimeDisplay.Verification"),
    "offLimitBorderTiles" => new(
      "accepted-narrow", "LegacyWorldBorderPolicy.OffLimitBorderTiles",
      "derived-legacy-world-border-policy", "Terraria.Dome.WorldBorderPolicy.Verification"),
    "dungeonX" => new(
      "accepted-narrow", "DungeonSpawnPoint.X",
      "DungeonSpawnPoint runtime boundary", "Terraria.Dome.DungeonSpawnPoint.Verification"),
    "dungeonY" => new(
      "accepted-narrow", "DungeonSpawnPoint.Y",
      "DungeonSpawnPoint runtime boundary", "Terraria.Dome.DungeonSpawnPoint.Verification"),
    "tileNoSunLight" => new(
      "accepted-narrow", "LegacyNoSunLightTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-no-sun-light-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tileNoAttach" => new(
      "accepted-narrow", "TileDefinition.IsNoAttach",
      "derived-immutable-tile-no-attach-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tilePile" => new(
      "accepted-narrow", "LegacyPileTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-pile-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tileFlame" => new(
      "accepted-narrow", "LegacyFlameTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-flame-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tileLighted" => new(
      "accepted-narrow", "LegacyLightedTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-lighted-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "npcCatchable" => new(
      "accepted-narrow", "LegacyCatchableNpcRegistry.RegisterDefaults()",
      "derived-immutable-legacy-catchable-npc-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "npcFrameCount" => new(
      "accepted-narrow", "LegacyNpcFrameRegistry.RegisterDefaults()",
      "derived-immutable-legacy-npc-frame-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "slimeRainNPC" => new(
      "accepted-narrow", "LegacySlimeRainNpcRegistry.RegisterDefaults()",
      "derived-immutable-legacy-slime-rain-npc-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "projPet" => new(
      "accepted-narrow", "LegacyPetProjectileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-pet-projectile-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "meleeBuff" => new(
      "accepted-narrow", "LegacyMeleeBuffRegistry.RegisterDefaults()",
      "derived-immutable-legacy-melee-buff-registry",
      "Terraria.Dome.Combat.Verification"),
    "persistentBuff" => new(
      "accepted-narrow", "LegacyPersistentBuffRegistry.RegisterDefaults()",
      "derived-immutable-legacy-persistent-buff-registry",
      "Terraria.Dome.Combat.Verification"),
    "debuff" => new(
      "accepted-narrow", "LegacyDebuffRegistry.RegisterDefaults()",
      "derived-immutable-legacy-debuff-registry",
      "Terraria.Dome.Combat.Verification"),
    "lightPet" => new(
      "accepted-narrow", "LegacyLightPetBuffRegistry.RegisterDefaults()",
      "derived-immutable-legacy-light-pet-registry",
      "Terraria.Dome.Combat.Verification"),
    "vanityPet" => new(
      "accepted-narrow", "LegacyVanityPetBuffRegistry.RegisterDefaults()",
      "derived-immutable-legacy-vanity-pet-registry",
      "Terraria.Dome.Combat.Verification"),
    "pvpBuff" => new(
      "accepted-narrow", "LegacyPvpBuffRegistry.RegisterDefaults()",
      "derived-immutable-legacy-pvp-buff-registry",
      "Terraria.Dome.MainFieldPropertyPvpBuff.Verification"),
    "tileObsidianKill" => new(
      "accepted-narrow", "LegacyObsidianKillTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-obsidian-kill-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tileOreFinderPriority" => new(
      "accepted-narrow", "LegacyOreFinderPriorityRegistry.RegisterDefaults()",
      "derived-immutable-legacy-ore-finder-priority-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tileMergeDirt" => new(
      "accepted-narrow", "LegacyMergeDirtTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-merge-dirt-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tileMerge" => new(
      "accepted-narrow", "LegacyTileMergeRegistry.RegisterDefaults()",
      "derived-immutable-legacy-tile-merge-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tileBrick" => new(
      "accepted-narrow", "LegacyBrickTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-brick-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tileBlockLight" => new(
      "accepted-narrow", "LegacyBlockLightTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-block-light-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tileSpelunker" => new(
      "accepted-narrow", "LegacySpelunkerTileRegistry.RegisterDefaults()",
      "derived-immutable-legacy-spelunker-registry", "Terraria.Dome.WorldGeneration.Verification"),
    "tileBlendAll" => new(
      "accepted-narrow", "LegacyTileBlendAllRegistry.RegisterDefaults()",
      "derived-immutable-legacy-tile-blend-all-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tileShine" => new(
      "accepted-narrow", "LegacyTileShineRegistry.RegisterDefaults()",
      "derived-immutable-legacy-tile-shine-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tileShine2" => new(
      "accepted-narrow", "LegacyTileShine2Registry.RegisterDefaults()",
      "derived-immutable-legacy-tile-shine2-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "tileGlowMask" => new(
      "accepted-narrow", "LegacyTileGlowMaskRegistry.RegisterDefaults()",
      "derived-immutable-legacy-tile-glow-mask-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "maxLiquidTypes" => new(
      "accepted-narrow", "LegacyLiquidTypeCapacity.MaxLiquidTypes",
      "derived-legacy-liquid-type-capacity", "Terraria.Dome.Liquid.Verification"),
    "townNPCCanSpawn" => new(
      "accepted-narrow", "LegacyTownNpcSpawnCandidateRegistry.RegisterDefaults()",
      "derived-immutable-town-npc-spawn-candidate-registry",
      "Terraria.Dome.WorldGeneration.Verification"),
    "noTrapsWorld" => new(
      "accepted-narrow", "WorldMetadata.IsNoTrapsWorld",
      "WorldPersistenceFormat.NoTrapsWorldFormatVersion",
      "Terraria.Dome.WorldImport.Verification"),
    "skyblockWorld" => new(
      "accepted-narrow", "WorldMetadata.IsSkyblockWorld",
      "WorldPersistenceFormat.SkyblockWorldFormatVersion",
      "Terraria.Dome.WorldImport.Verification"),
    "GameMode" => new(
      "accepted-narrow", "WorldRuleState.GameMode", "DomeStatePersistenceFormat.GameMode",
      "Terraria.Dome.Persistence.Verification"),
    "IsJourneyMode" => new(
      "accepted-narrow", "WorldRuleState.IsJourneyMode", "derived-WorldRuleState.GameMode",
      "Terraria.Dome.WorldRules.Verification"),
    "Difficulty" => new(
      "accepted-narrow", "WorldRuleState.Difficulty", "DomeStatePersistenceFormat.WorldRules",
      "Terraria.Dome.Persistence.Verification"),
    "expertMode" => new(
      "accepted-narrow", "WorldRuleState.IsExpertMode", "derived-WorldRuleState.Difficulty",
      "Terraria.Dome.WorldRules.Verification"),
    "masterMode" => new(
      "accepted-narrow", "WorldRuleState.IsMasterMode", "derived-WorldRuleState.Difficulty",
      "Terraria.Dome.WorldRules.Verification"),
    "isThereAWorldSurface" => new(
      "accepted-narrow", "WorldMetadata.HasWorldSurface", "derived-world-surface-threshold",
      "Terraria.Dome.WorldRules.Verification"),
    "NoFunctionalSurface" => new(
      "accepted-narrow", "WorldMetadata.HasNoFunctionalSurface", "derived-world-surface-threshold",
      "Terraria.Dome.WorldRules.Verification"),
    "invasionWarn" => new(
      "accepted-narrow", "WorldInvasionWarningSystem.WarningTicks", "transient-event-state",
      "Terraria.Dome.WorldRules.Verification"),
    "slimeWarningTime" => new(
      "accepted-narrow", "WorldProgressionState.SlimeRainWarningTicks", "transient-event-state",
      "Terraria.Dome.WorldRules.Verification"),
    "slimeWarningDelay" => new(
      "accepted-narrow", "WorldProgressionSystem.DefaultSlimeRainWarningDelayTicks",
      "derived-constant", "Terraria.Dome.WorldRules.Verification"),
    "IsRainingForever" => new(
      "accepted-narrow", "WorldRuleState.IsRainingForever", "derived-rain-duration-threshold",
      "Terraria.Dome.WorldImport.Verification"),
    "invasionProgress" => new(
      "accepted-narrow", "WorldInvasionProgressResult.Progress", "transient-projection",
      "Terraria.Dome.WorldRules.Verification"),
    "invasionProgressMax" => new(
      "accepted-narrow", "WorldInvasionProgressResult.ProgressMax", "transient-projection",
      "Terraria.Dome.WorldRules.Verification"),
    "invasionProgressIcon" => new(
      "accepted-narrow", "WorldInvasionProgressResult.Icon", "transient-projection",
      "Terraria.Dome.WorldRules.Verification"),
    "invasionProgressWave" => new(
      "accepted-narrow", "WorldInvasionProgressResult.Wave", "transient-projection",
      "Terraria.Dome.WorldRules.Verification"),
    _ => new("unmapped", "deferred", "unverified", "unverified")
  };
}

static IReadOnlyList<IdentityEvidence> AuditIdentityDeclarations(string mainSourcePath)
{
  string sourceDirectory = Path.GetDirectoryName(mainSourcePath) ?? string.Empty;
  (string fileName, string typeName, string memberName)[] targets =
  [
    ("Main.cs", "Main", "worldID"),
    ("NPC.cs", "NPC", "netID"),
    ("Projectile.cs", "Projectile", "identity"),
    ("MessageBuffer.cs", "MessageBuffer", "whoAmI")
  ];
  List<IdentityEvidence> result = new();
  foreach ((string fileName, string typeName, string memberName) in targets)
  {
    string path = Path.Combine(sourceDirectory, fileName);
    if (!File.Exists(path))
    {
      result.Add(new(fileName, typeName, memberName, false, "source-missing"));
      continue;
    }

    CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(
      File.ReadAllText(path), path: path).GetCompilationUnitRoot();
    bool found = root.DescendantNodes().OfType<TypeDeclarationSyntax>()
      .Where(type => type.Identifier.ValueText == typeName)
      .SelectMany(type => type.Members)
      .Any(member => member switch
      {
        FieldDeclarationSyntax field => field.Declaration.Variables.Any(variable =>
          variable.Identifier.ValueText == memberName),
        PropertyDeclarationSyntax property => property.Identifier.ValueText == memberName,
        _ => false
      });
    result.Add(new(fileName, typeName, memberName, found, found ? "declared" : "not-found"));
  }

  return result;
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

internal sealed record InventoryEvidence(
  string ScannerVersion,
  string SourcePath,
  string SourceSha256,
  int MemberCount,
  int MigrationScopeCount,
  int IdentityExcludedCount,
  int FieldDeclarationCount,
  int FieldVariableCount,
  int PropertyDeclarationCount,
  int ExpectedMigrationScopeCount,
  string CountReconciliation,
  IReadOnlyList<MemberEvidence> Members,
  IReadOnlyList<IdentityEvidence> IdentityAudit);

internal sealed record IdentityEvidence(
  string FileName,
  string TypeName,
  string MemberName,
  bool Found,
  string Status);

internal sealed record MemberEvidence(
  string SymbolId,
  string ContainingType,
  string Name,
  string Kind,
  string Type,
  int Line,
  bool IsStatic,
  int ReadCount,
  int WriteCount,
  string DefaultValue,
  string Mutability,
  string Status,
  string Owner,
  string Serialization,
  string Verification);

internal sealed record MemberMapping(
  string Status,
  string Owner,
  string Serialization,
  string Verification);
