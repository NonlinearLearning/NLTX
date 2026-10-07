using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Terraria.Npc;

const string referenceIndexRelativePath =
  "docs/system-decomposition/2026-10-05-npc-ai-reference-index.md";
const string coverageLedgerRelativePath =
  "docs/migration/ledgers/2026-10-06-npc-ai-profile-coverage-ledger.md";

string repositoryRoot = GetArgument(args, "--repo-root") ?? FindRepositoryRoot();
bool requireComplete = args.Contains("--require-complete", StringComparer.Ordinal);
string reportPath = GetArgument(args, "--report") ?? Path.Combine(
  repositoryRoot,
  "Build",
  "diagnostics",
  "NpcAiCoverage",
  "coverage-gate-report.json");

string referenceIndexPath = Path.Combine(repositoryRoot, referenceIndexRelativePath);
string coverageLedgerPath = Path.Combine(repositoryRoot, coverageLedgerRelativePath);
int[] referenceStyles = ReadStyleIds(referenceIndexPath, "## 全部分派入口");
LedgerStyleCoverage[] ledgerStyles = ReadLedgerStyles(coverageLedgerPath);
RequireStyleInventory(referenceStyles, "reference index");
RequireStyleInventory(ledgerStyles.Select(static style => style.AiStyle).ToArray(), "coverage ledger");
Require(referenceStyles.SequenceEqual(ledgerStyles.Select(static style => style.AiStyle)),
  "The reference index and coverage ledger must contain the same ordered style inventory.");

NpcAiProfileCoverageRegistry registry = NpcAiProfileCoverageBaseline.CreateCurrentSnapshot(
  ledgerStyles.Select(static style => style.AiStyle));
int assertions = VerifyBaseline(registry);
CoverageReport report = CreateReport(registry, assertions, referenceIndexRelativePath,
  coverageLedgerRelativePath, ledgerStyles, requireComplete);

string fullReportPath = Path.GetFullPath(reportPath);
Directory.CreateDirectory(Path.GetDirectoryName(fullReportPath)!);
await File.WriteAllTextAsync(
  fullReportPath,
  JsonSerializer.Serialize(report, new JsonSerializerOptions
  {
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() },
  }));

Console.WriteLine($"NPC AI coverage inventory gate PASS");
Console.WriteLine($"Style entries: {report.Summary.StyleEntries}");
Console.WriteLine($"Indexed/mapped/implemented/verified styles: " +
  $"{report.Summary.IndexedStyleEntries}/{report.Summary.MappedStyleEntries}/" +
  $"{report.Summary.ImplementedStyleEntries}/{report.Summary.VerifiedStyleEntries}");
Console.WriteLine($"Open style entries: {report.Summary.OpenStyleEntries}");
Console.WriteLine($"Mapped profiles: {report.Summary.MappedProfiles}");
Console.WriteLine($"Finite handlers: {report.Summary.FiniteHandlers}");
Console.WriteLine($"Implemented profiles: {report.Summary.ImplementedProfiles}");
Console.WriteLine($"Verified profiles: {report.Summary.VerifiedProfiles}");
Console.WriteLine($"Open profiles: {report.Summary.OpenProfiles}");
Console.WriteLine($"Assertions: {report.AssertionsPassed}");
Console.WriteLine($"Coverage closure ready: {report.Completion.IsReady}");
Console.WriteLine($"Report: {fullReportPath}");
if (requireComplete && !report.Completion.IsReady)
{
  Console.Error.WriteLine(
    $"NPC AI coverage completion gate FAIL: " +
    $"{report.Completion.IncompleteStyles.Count} styles and " +
    $"{report.Completion.IncompleteProfiles.Count} registered profiles remain incomplete.");
  return 2;
}

return 0;

static int VerifyBaseline(NpcAiProfileCoverageRegistry registry)
{
  int assertions = 0;
  NpcAiProfileIdentity[] finiteProfiles =
  [
    new(1, 1, 1),
    new(2, 2, 2),
    new(3, 3, 3),
    new(16, 16, 1),
    new(4, 4, 4),
  ];

  foreach (NpcAiProfileIdentity identity in finiteProfiles)
  {
    NpcAiProfileCoverageRegistration registration = registry.RequireFiniteHandler(identity);
    Require(registration.Stage == NpcAiCoverageStage.Mapped && registration.HasOpenWork,
      $"Finite source profile {identity} must remain mapped/open until its full closure is verified.");
    assertions++;
  }

  Require(NpcBlueSlimeProfile.CanHandle(1, 1, 1), "Blue Slime finite handler identity must match.");
  Require(NpcFloatingEyeProfile.CanHandle(2, 2, 2), "Demon Eye finite handler identity must match.");
  Require(NpcFighterProfile.CanHandle(3, 3, 3), "Zombie finite handler identity must match.");
  Require(NpcMotherSlimeProfile.CanHandle(16, 16, 1),
    "Mother Slime finite handler identity must match.");
  Require(NpcEyeOfCthulhuProfile.CanHandle(4, 4, 4),
    "Eye of Cthulhu finite handler identity must match.");
  Require(!NpcBlueSlimeProfile.CanHandle(1, 16, 1),
    "Green Slime finite catalog id 16 must not alias the reference Blue Slime profile.");
  assertions += 6;

  NpcAiProfileIdentity negativeVariantIdentity = new(700, -3, 5);
  NpcAiProfileCoverageRegistry variantRegistry = new(registry.IndexedStyles);
  variantRegistry.Register(new NpcAiProfileCoverageRegistration(
    "negative netId verifier fixture",
    negativeVariantIdentity,
    NpcAiCoverageStage.Mapped,
    HasFiniteHandler: false,
    HasOpenWork: true));
  Require(variantRegistry.RequireRegistered(negativeVariantIdentity).Identity.NetId == -3,
    "An explicitly registered negative netId must retain its variant identity.");
  RequireFailure(variantRegistry, new NpcAiProfileIdentity(700, 3, 5),
    NpcAiProfileRegistrationFailure.UnregisteredProfile);
  RequireFailure(registry, new NpcAiProfileIdentity(1, -3, 1),
    NpcAiProfileRegistrationFailure.UnregisteredProfile);
  assertions += 3;

  RequireFailure(registry, new NpcAiProfileIdentity(1, 1, 2),
    NpcAiProfileRegistrationFailure.IdentityConflict);
  RequireFailure(registry, new NpcAiProfileIdentity(900, 1, 1),
    NpcAiProfileRegistrationFailure.IdentityConflict);
  RequireFailure(registry, new NpcAiProfileIdentity(2, 9, 2),
    NpcAiProfileRegistrationFailure.IdentityConflict);
  RequireFailure(registry, new NpcAiProfileIdentity(2, 2, 1),
    NpcAiProfileRegistrationFailure.IdentityConflict);
  RequireFailure(registry, new NpcAiProfileIdentity(1, 16, 1),
    NpcAiProfileRegistrationFailure.IdentityConflict);
  RequireFailure(registry, new NpcAiProfileIdentity(22, 22, 0),
    NpcAiProfileRegistrationFailure.IdentityConflict);
  RequireFailure(registry, new NpcAiProfileIdentity(488, 488, 0),
    NpcAiProfileRegistrationFailure.IdentityConflict);
  RequireFailure(registry, new NpcAiProfileIdentity(900, 901, 12),
    NpcAiProfileRegistrationFailure.UnregisteredProfile);
  RequireFailure(registry, new NpcAiProfileIdentity(900, 901, 128),
    NpcAiProfileRegistrationFailure.UnsupportedStyle);
  RequireFailure(registry, new NpcAiProfileIdentity(22, 22, 7),
    NpcAiProfileRegistrationFailure.FiniteHandlerMissing);
  RequireFailure(registry, new NpcAiProfileIdentity(1, 1, 1),
    NpcAiProfileRegistrationFailure.CoverageOpen);
  assertions += 11;

  NpcAiProfileCoverageRegistry duplicateRegistry =
    new(registry.IndexedStyles);
  NpcAiProfileCoverageRegistration duplicate = new(
    "test profile",
    new NpcAiProfileIdentity(700, -700, 5),
    NpcAiCoverageStage.Mapped,
    HasFiniteHandler: true,
    HasOpenWork: true);
  duplicateRegistry.Register(duplicate);
  RequireFailure(duplicateRegistry, duplicate.Identity,
    NpcAiProfileRegistrationFailure.DuplicateRegistration,
    duplicate);

  RequireRegistrationFailure(
    () => duplicateRegistry.Register(duplicate with
    {
      Identity = new NpcAiProfileIdentity(700, -700, 6),
    }),
    NpcAiProfileRegistrationFailure.IdentityConflict,
    "Conflicting profile registration must fail before entering the registry.");
  assertions += 2;

  RequireFailure(registry, new NpcAiProfileIdentity(37, 37, 0),
    NpcAiProfileRegistrationFailure.IdentityConflict);
  RequireFailure(registry, new NpcAiProfileIdentity(37, 37, 7),
    NpcAiProfileRegistrationFailure.FiniteHandlerMissing);
  RequireFailure(registry, new NpcAiProfileIdentity(488, 488, 92),
    NpcAiProfileRegistrationFailure.FiniteHandlerMissing);
  Require(!registry.Registrations.Any(static registration =>
      registration.Name.Contains("fallback", StringComparison.OrdinalIgnoreCase)),
    "Fallback behavior must not be entered as completed profile coverage.");
  assertions += 4;
  return assertions;
}

static void RequireFailure(
  NpcAiProfileCoverageRegistry registry,
  NpcAiProfileIdentity identity,
  NpcAiProfileRegistrationFailure expectedFailure,
  NpcAiProfileCoverageRegistration? duplicate = null)
{
  try
  {
    if (duplicate is not null)
    {
      registry.Register(duplicate);
      throw new InvalidOperationException("Duplicate registration unexpectedly succeeded.");
    }

    if (expectedFailure == NpcAiProfileRegistrationFailure.FiniteHandlerMissing)
    {
      registry.RequireFiniteHandler(identity);
    }
    else if (expectedFailure == NpcAiProfileRegistrationFailure.CoverageOpen)
    {
      registry.RequireVerified(identity);
    }
    else
    {
      registry.RequireRegistered(identity);
    }
  }
  catch (NpcAiProfileRegistrationException exception)
      when (exception.Failure == expectedFailure)
  {
    return;
  }

  throw new InvalidOperationException(
    $"Expected {expectedFailure} for NPC AI profile {identity}.");
}

static void RequireRegistrationFailure(
  Action registration,
  NpcAiProfileRegistrationFailure expectedFailure,
  string message)
{
  try
  {
    registration();
  }
  catch (NpcAiProfileRegistrationException exception)
      when (exception.Failure == expectedFailure)
  {
    return;
  }

  throw new InvalidOperationException(message);
}

static CoverageReport CreateReport(
  NpcAiProfileCoverageRegistry registry,
  int assertionsPassed,
  string referenceIndexPath,
  string coverageLedgerPath,
  IReadOnlyList<LedgerStyleCoverage> ledgerStyles,
  bool requireComplete)
{
  NpcAiProfileCoverageRegistration[] registrations = registry.Registrations.ToArray();
  int[] incompleteStyles = ledgerStyles.Where(static style =>
      style.Stage != NpcAiCoverageStage.Verified ||
      !style.Profiles.Equals("verified", StringComparison.OrdinalIgnoreCase) ||
      !style.Closure.Equals("closed", StringComparison.OrdinalIgnoreCase) ||
      !style.Verification.Equals("passed", StringComparison.OrdinalIgnoreCase))
    .Select(static style => style.AiStyle).ToArray();
  NpcAiProfileIdentity[] incompleteProfiles = registrations.Where(static registration =>
      registration.Stage != NpcAiCoverageStage.Verified ||
      !registration.HasFiniteHandler || registration.HasOpenWork)
    .Select(static registration => registration.Identity).ToArray();
  return new CoverageReport(
    DateTimeOffset.UtcNow,
    referenceIndexPath,
    coverageLedgerPath,
    new CoverageSummary(
      ledgerStyles.Count,
      ledgerStyles.Count(static style => style.Stage == NpcAiCoverageStage.Indexed),
      ledgerStyles.Count(static style => style.Stage == NpcAiCoverageStage.Mapped),
      ledgerStyles.Count(static style => style.Stage == NpcAiCoverageStage.Implemented),
      ledgerStyles.Count(static style => style.Stage == NpcAiCoverageStage.Verified),
      ledgerStyles.Count(static style => style.HasOpenWork),
      registrations.Count(static registration => registration.Stage == NpcAiCoverageStage.Mapped),
      registrations.Count(static registration => registration.HasFiniteHandler),
      registrations.Count(static registration => registration.Stage == NpcAiCoverageStage.Implemented),
      registrations.Count(static registration => registration.Stage == NpcAiCoverageStage.Verified),
      registrations.Count(static registration => registration.HasOpenWork)),
    assertionsPassed,
    ledgerStyles,
    registrations,
    new CoverageCompletion(
      requireComplete,
      incompleteStyles.Length == 0 && incompleteProfiles.Length == 0,
      incompleteStyles,
      incompleteProfiles));
}

static LedgerStyleCoverage[] ReadLedgerStyles(string path)
{
  const int entryColumn = 4;
  const int profilesColumn = 5;
  const int closureColumn = 6;
  const int verificationColumn = 7;
  int[] requiredColumns =
  [
    entryColumn,
    profilesColumn,
    closureColumn,
    verificationColumn,
  ];

  List<LedgerStyleCoverage> styles = [];
  bool hasStartedTable = false;
  foreach (string line in ReadSectionLines(path, "## Style entry inventory"))
  {
    if (!line.StartsWith('|'))
    {
      if (hasStartedTable && string.IsNullOrWhiteSpace(line))
      {
        break;
      }

      continue;
    }

    string[] columns = line.Trim('|').Split('|').Select(static column => column.Trim()).ToArray();
    if (columns.Length > 0 && columns[0].Equals("Style", StringComparison.OrdinalIgnoreCase))
    {
      hasStartedTable = true;
      continue;
    }

    if (!hasStartedTable)
    {
      continue;
    }

    if (columns[0].StartsWith("---", StringComparison.Ordinal))
    {
      continue;
    }

    if (columns.Length <= requiredColumns.Max() ||
        !int.TryParse(columns[0], out int aiStyle))
    {
      break;
    }

    string entry = columns[entryColumn];
    string closure = columns[closureColumn];
    styles.Add(new LedgerStyleCoverage(
      aiStyle,
      ResolveStage(entry),
      columns[profilesColumn],
      closure,
      columns[verificationColumn],
      closure.Contains("open", StringComparison.OrdinalIgnoreCase)));
  }

  return styles.ToArray();
}

static IEnumerable<string> ReadSectionLines(string path, string sectionHeading)
{
  if (!File.Exists(path))
  {
    throw new FileNotFoundException("Coverage source document was not found.", path);
  }

  bool inSection = false;
  foreach (string line in File.ReadLines(path))
  {
    if (line.Trim().Equals(sectionHeading, StringComparison.Ordinal))
    {
      inSection = true;
      continue;
    }

    if (inSection && line.StartsWith("## ", StringComparison.Ordinal))
    {
      yield break;
    }

    if (inSection)
    {
      yield return line;
    }
  }
}

static NpcAiCoverageStage ResolveStage(string entry)
{
  if (entry.StartsWith("verified", StringComparison.OrdinalIgnoreCase))
  {
    return NpcAiCoverageStage.Verified;
  }

  if (entry.StartsWith("implemented", StringComparison.OrdinalIgnoreCase))
  {
    return NpcAiCoverageStage.Implemented;
  }

  if (entry.StartsWith("mapped", StringComparison.OrdinalIgnoreCase) ||
      entry.StartsWith("partial", StringComparison.OrdinalIgnoreCase))
  {
    return NpcAiCoverageStage.Mapped;
  }

  return NpcAiCoverageStage.Indexed;
}

static int[] ReadStyleIds(string path, string sectionHeading)
{
  if (!File.Exists(path))
  {
    throw new FileNotFoundException("Coverage source document was not found.", path);
  }

  bool inSection = false;
  List<int> styleIds = [];
  Regex styleRow = new(@"^\|\s*(-?\d+)\s*\|", RegexOptions.Compiled);
  foreach (string line in File.ReadLines(path))
  {
    if (line.Trim().Equals(sectionHeading, StringComparison.Ordinal))
    {
      inSection = true;
      continue;
    }

    if (inSection && line.StartsWith("## ", StringComparison.Ordinal))
    {
      break;
    }

    if (!inSection)
    {
      continue;
    }

    Match match = styleRow.Match(line);
    if (match.Success && int.TryParse(match.Groups[1].Value, out int styleId))
    {
      styleIds.Add(styleId);
    }
  }

  return styleIds.ToArray();
}

static void RequireStyleInventory(IReadOnlyList<int> styleIds, string sourceName)
{
  int[] expected = Enumerable.Range(
    NpcAiProfileCoverageRegistry.MinimumIndexedStyle,
    NpcAiProfileCoverageRegistry.MaximumIndexedStyle + 1).ToArray();
  Require(styleIds.SequenceEqual(expected),
    $"The {sourceName} must contain every NPC AI style 0–127 exactly once and in order.");
}

static string? GetArgument(IReadOnlyList<string> arguments, string name)
{
  int index = -1;
  for (int argumentIndex = 0; argumentIndex < arguments.Count; argumentIndex++)
  {
    if (StringComparer.Ordinal.Equals(arguments[argumentIndex], name))
    {
      index = argumentIndex;
      break;
    }
  }

  if (index < 0)
  {
    return null;
  }

  if (index + 1 >= arguments.Count)
  {
    throw new ArgumentException($"Argument {name} requires a value.");
  }

  return arguments[index + 1];
}

static string FindRepositoryRoot()
{
  DirectoryInfo? directory = new(Environment.CurrentDirectory);
  while (directory is not null)
  {
    if (File.Exists(Path.Combine(directory.FullName, "global.json")) &&
        Directory.Exists(Path.Combine(directory.FullName, "docs")))
    {
      return directory.FullName;
    }

    directory = directory.Parent;
  }

  throw new DirectoryNotFoundException(
    "Could not locate repository root; pass --repo-root explicitly.");
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

sealed record LedgerStyleCoverage(
  int AiStyle,
  NpcAiCoverageStage Stage,
  string Profiles,
  string Closure,
  string Verification,
  bool HasOpenWork);

sealed record CoverageSummary(
  int StyleEntries,
  int IndexedStyleEntries,
  int MappedStyleEntries,
  int ImplementedStyleEntries,
  int VerifiedStyleEntries,
  int OpenStyleEntries,
  int MappedProfiles,
  int FiniteHandlers,
  int ImplementedProfiles,
  int VerifiedProfiles,
  int OpenProfiles);

sealed record CoverageReport(
  DateTimeOffset GeneratedAtUtc,
  string ReferenceIndex,
  string CoverageLedger,
  CoverageSummary Summary,
  int AssertionsPassed,
  IReadOnlyList<LedgerStyleCoverage> Styles,
  IReadOnlyList<NpcAiProfileCoverageRegistration> Profiles,
  CoverageCompletion Completion);

sealed record CoverageCompletion(
  bool Required,
  bool IsReady,
  IReadOnlyList<int> IncompleteStyles,
  IReadOnlyList<NpcAiProfileIdentity> IncompleteProfiles);
