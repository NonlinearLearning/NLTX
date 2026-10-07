using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

const string referenceIndexRelativePath =
  "docs/system-decomposition/2026-10-05-npc-ai-reference-index.md";
const string coverageLedgerRelativePath =
  "docs/migration/ledgers/2026-10-06-npc-ai-profile-coverage-ledger.md";

try
{
  return Run(args);
}
catch (Exception exception)
{
  Console.Error.WriteLine($"NPC AI source identity gate FAIL: {exception.Message}");
  return 1;
}

static int Run(string[] arguments)
{
  string repositoryRoot = Path.GetFullPath(
    GetArgument(arguments, "--repo-root") ?? FindRepositoryRoot());
  string referenceIndexPath = Path.Combine(repositoryRoot, referenceIndexRelativePath);
  string coverageLedgerPath = Path.Combine(repositoryRoot, coverageLedgerRelativePath);
  string referenceIndex = File.ReadAllText(referenceIndexPath);
  string coverageLedger = File.ReadAllText(coverageLedgerPath);
  string referenceRoot = Path.GetFullPath(
    GetArgument(arguments, "--reference-root") ?? ReadReferenceRoot(coverageLedger));
  string reportPath = Path.GetFullPath(GetArgument(arguments, "--report") ?? Path.Combine(
    repositoryRoot,
    "Build",
    "diagnostics",
    "NpcAiSourceIdentity",
    "source-identity-report.json"));

  SourceFile[] sourceFiles =
  [
    new("Terraria/NPC.cs"),
    new("Terraria/Main.cs"),
    new("Terraria.ID/NPCID.cs"),
  ];
  List<SourceFileResult> results = [];
  List<string> failures = [];

  foreach (SourceFile sourceFile in sourceFiles)
  {
    string indexHash = ReadIndexHash(referenceIndex, sourceFile.RelativePath);
    string ledgerHash = ReadLedgerHash(coverageLedger, sourceFile.RelativePath);
    string sourcePath = Path.Combine(
      referenceRoot,
      sourceFile.RelativePath.Replace('/', Path.DirectorySeparatorChar));
    string actualHash = ComputeSha256(sourcePath);
    bool indexMatchesLedger = string.Equals(indexHash, ledgerHash, StringComparison.Ordinal);
    bool sourceMatchesIndex = string.Equals(indexHash, actualHash, StringComparison.Ordinal);

    if (!indexMatchesLedger)
    {
      failures.Add($"Reference index and coverage ledger disagree for {sourceFile.RelativePath}.");
    }

    if (!sourceMatchesIndex)
    {
      failures.Add($"Reference source hash differs from the pinned index for {sourceFile.RelativePath}.");
    }

    results.Add(new SourceFileResult(
      sourceFile.RelativePath,
      indexHash,
      ledgerHash,
      actualHash,
      indexMatchesLedger,
      sourceMatchesIndex));
  }

  int assertionsPassed = results.Sum(static result =>
    (result.IndexMatchesLedger ? 1 : 0) + (result.SourceMatchesIndex ? 1 : 0));
  SourceIdentityReport report = new(
    SchemaVersion: 1,
    Gate: "npc-ai-source-identity",
    Status: failures.Count == 0 ? "passed" : "failed",
    ReferenceRoot: referenceRoot,
    ReferenceIndex: referenceIndexRelativePath,
    CoverageLedger: coverageLedgerRelativePath,
    FilesChecked: results.Count,
    HashComparisonsPassed: assertionsPassed,
    Files: results,
    Failures: failures);

  string reportDirectory = Path.GetDirectoryName(reportPath)!;
  Directory.CreateDirectory(reportDirectory);
  File.WriteAllText(reportPath, JsonSerializer.Serialize(report, new JsonSerializerOptions
  {
    WriteIndented = true,
  }));

  Console.WriteLine($"NPC AI source identity gate {(failures.Count == 0 ? "PASS" : "FAIL")}");
  Console.WriteLine($"Reference files checked: {results.Count}");
  Console.WriteLine($"Index/ledger/source hash comparisons passed: {assertionsPassed}/6");
  Console.WriteLine($"Report: {reportPath}");

  return failures.Count == 0 ? 0 : 1;
}

static string ReadIndexHash(string referenceIndex, string relativePath)
{
  string pattern =
    "^\\|\\s*`" + Regex.Escape(relativePath) +
    "`\\s*\\|\\s*\\d+\\s*\\|\\s*`(?<hash>[a-fA-F0-9]{64})`\\s*\\|\\s*$";
  return ReadUniqueHash(referenceIndex, pattern, "reference index", relativePath);
}

static string ReadLedgerHash(string coverageLedger, string relativePath)
{
  string pattern =
    "^\\|\\s*" + Regex.Escape(relativePath) +
    "\\s*\\|\\s*(?<hash>[a-fA-F0-9]{64})\\s*\\|\\s*same\\s*\\|\\s*confirmed\\s*\\|\\s*$";
  return ReadUniqueHash(coverageLedger, pattern, "coverage ledger", relativePath);
}

static string ReadUniqueHash(
  string document,
  string pattern,
  string documentName,
  string relativePath)
{
  MatchCollection matches = Regex.Matches(
    document,
    pattern,
    RegexOptions.Multiline | RegexOptions.CultureInvariant);
  if (matches.Count != 1)
  {
    throw new InvalidOperationException(
      $"Expected one {documentName} fingerprint row for {relativePath}, found {matches.Count}.");
  }

  return matches[0].Groups["hash"].Value.ToLowerInvariant();
}

static string ReadReferenceRoot(string coverageLedger)
{
  Match match = Regex.Match(
    coverageLedger,
    "^Reference root:\\s*(?<path>.+?)\\s+\\(read-only\\)\\.",
    RegexOptions.Multiline | RegexOptions.CultureInvariant);
  if (!match.Success)
  {
    throw new InvalidOperationException(
      "The coverage ledger does not declare a read-only reference root; pass --reference-root.");
  }

  return match.Groups["path"].Value.Replace('/', Path.DirectorySeparatorChar);
}

static string ComputeSha256(string path)
{
  using FileStream stream = File.OpenRead(path);
  return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
}

static string? GetArgument(IReadOnlyList<string> arguments, string name)
{
  for (int index = 0; index < arguments.Count; index++)
  {
    if (!string.Equals(arguments[index], name, StringComparison.Ordinal))
    {
      continue;
    }

    if (index + 1 >= arguments.Count || arguments[index + 1].StartsWith("--", StringComparison.Ordinal))
    {
      throw new ArgumentException($"Argument {name} requires a value.");
    }

    return arguments[index + 1];
  }

  return null;
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
    "Could not find the repository root; pass --repo-root <path>.");
}

sealed record SourceFile(string RelativePath);

sealed record SourceFileResult(
  string Path,
  string IndexSha256,
  string LedgerSha256,
  string SourceSha256,
  bool IndexMatchesLedger,
  bool SourceMatchesIndex);

sealed record SourceIdentityReport(
  int SchemaVersion,
  string Gate,
  string Status,
  string ReferenceRoot,
  string ReferenceIndex,
  string CoverageLedger,
  int FilesChecked,
  int HashComparisonsPassed,
  IReadOnlyList<SourceFileResult> Files,
  IReadOnlyList<string> Failures);
