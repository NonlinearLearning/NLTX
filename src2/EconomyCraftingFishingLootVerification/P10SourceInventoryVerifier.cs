using System.Text.RegularExpressions;

namespace NLTX.EconomyCraftingFishingLootVerification;

public static class P10SourceInventoryVerifier
{
  private const int ExpectedMemberCount = 321;
  private const int ExpectedFieldCount = 296;
  private const int ExpectedPropertyCount = 25;

  private static readonly Regex ReportMemberRowPattern = new(
    @"^\|\s*(\d+)\s*\|\s*(field|property)\s*\|",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

  private static readonly Regex MatrixMemberRowPattern = new(
    @"^\|\s*([0-9]+(?:\s*,\s*[0-9]+)*)\s*\|\s*`proposed\.",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

  private static readonly Regex MatrixTargetPathPattern = new(
    @"->\s*`(?<path>src2/[^`]+)`",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

  private static readonly Regex ForbiddenSourceReferencePattern = new(
    @"(?:D:\\TRbackup\\NLTX\\src(?!2)|D:\\TRbackup\\Version4|\busing\s+Terraria\.)",
    RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

  public static P10SourceInventoryVerificationResult Verify(
    string repositoryRoot)
  {
    if (string.IsNullOrWhiteSpace(repositoryRoot))
    {
      return Invalid("The repository root must contain a path.");
    }

    string reportPath = Path.Combine(
      repositoryRoot,
      "docs",
      "迁移参考表",
      "Version4非权威组件拆分分区",
      "10-economy-crafting-fishing-loot.md");
    string designPath = Path.Combine(
      repositoryRoot,
      "docs",
      "第二轮审查",
      "非权威",
      "2026-09-11-version4-non-authoritative-P10-economy-crafting-fishing-loot-component-design.md");
    string sourceRoot = Path.Combine(
      repositoryRoot,
      "src2",
      "EconomyCraftingFishingLoot");
    string projectPath = Path.Combine(
      sourceRoot,
      "Terraria.EconomyCraftingFishingLoot.csproj");

    if (!File.Exists(reportPath))
    {
      return Invalid($"P10 input report was not found: {reportPath}");
    }

    if (!File.Exists(designPath))
    {
      return Invalid($"P10 design document was not found: {designPath}");
    }

    if (!Directory.Exists(sourceRoot))
    {
      return Invalid($"P10 src2 source root was not found: {sourceRoot}");
    }

    if (!File.Exists(projectPath))
    {
      return Invalid($"P10 source project was not found: {projectPath}");
    }

    try
    {
      InventorySnapshot report = ReadReport(reportPath);
      if (!report.IsValid)
      {
        return Invalid(report.FailureMessage);
      }

      InventorySnapshot matrix = ReadMatrix(designPath);
      if (!matrix.IsValid)
      {
        return Invalid(matrix.FailureMessage);
      }

      if (report.MemberCount != ExpectedMemberCount ||
          report.FieldCount != ExpectedFieldCount ||
          report.PropertyCount != ExpectedPropertyCount)
      {
        return Invalid(
          $"P10 report counts were {report.MemberCount}/{report.FieldCount}/{report.PropertyCount}; " +
          $"expected {ExpectedMemberCount}/{ExpectedFieldCount}/{ExpectedPropertyCount}.");
      }

      if (matrix.MemberCount != ExpectedMemberCount)
      {
        return Invalid(
          $"P10 design matrix mapped {matrix.MemberCount} source sequences; " +
          $"expected {ExpectedMemberCount}.");
      }

      if (!report.Sequences.SetEquals(matrix.Sequences))
      {
        HashSet<int> missing = report.Sequences.Except(matrix.Sequences).ToHashSet();
        HashSet<int> extra = matrix.Sequences.Except(report.Sequences).ToHashSet();
        return Invalid(
          $"P10 design matrix sequence set differs from the report. " +
          $"Missing mappings: {FormatSequenceSet(missing)}; " +
          $"extra mappings: {FormatSequenceSet(extra)}.");
      }

      P10SourceInventoryVerificationResult targetPathResult =
        VerifyMatrixTargetPaths(repositoryRoot, sourceRoot, matrix.TargetPaths);
      if (!targetPathResult.IsValid)
      {
        return targetPathResult;
      }

      P10SourceInventoryVerificationResult boundaryResult =
        VerifySourceBoundary(sourceRoot, projectPath);
      if (!boundaryResult.IsValid)
      {
        return boundaryResult;
      }

      return new P10SourceInventoryVerificationResult(
        isValid: true,
        failureMessage: string.Empty);
    }
    catch (IOException exception)
    {
      return Invalid($"P10 inventory verification could not read a required file: {exception.Message}");
    }
    catch (UnauthorizedAccessException exception)
    {
      return Invalid($"P10 inventory verification could not access a required file: {exception.Message}");
    }
  }

  private static InventorySnapshot ReadReport(string reportPath)
  {
    HashSet<int> sequences = [];
    int fieldCount = 0;
    int propertyCount = 0;

    foreach (string line in File.ReadLines(reportPath))
    {
      Match match = ReportMemberRowPattern.Match(line);
      if (!match.Success)
      {
        continue;
      }

      int sequence = int.Parse(match.Groups[1].Value);
      if (!sequences.Add(sequence))
      {
        return InventorySnapshot.Invalid(
          $"P10 input report contains duplicate source sequence {sequence}.");
      }

      if (match.Groups[2].Value == "field")
      {
        fieldCount++;
      }
      else
      {
        propertyCount++;
      }
    }

    return InventorySnapshot.Valid(sequences, fieldCount, propertyCount);
  }

  private static InventorySnapshot ReadMatrix(string designPath)
  {
    string content = File.ReadAllText(designPath);
    const string matrixHeading = "## Complete Member Ownership Matrix";
    const string matrixEndHeading = "## State, Invariants, And Composition";
    int matrixStart = content.IndexOf(
      matrixHeading,
      StringComparison.Ordinal);
    int matrixEnd = content.IndexOf(
      matrixEndHeading,
      StringComparison.Ordinal);

    if (matrixStart < 0 || matrixEnd <= matrixStart)
    {
      return InventorySnapshot.Invalid(
        "P10 design document does not contain a bounded ownership matrix.");
    }

    string matrixContent = content.Substring(
      matrixStart,
      matrixEnd - matrixStart);
    HashSet<int> sequences = [];
    HashSet<string> targetPaths = new(StringComparer.Ordinal);
    int memberCount = 0;

    foreach (string line in matrixContent.Split('\n'))
    {
      Match match = MatrixMemberRowPattern.Match(line.TrimEnd('\r'));
      if (!match.Success)
      {
        continue;
      }

      Match targetPathMatch = MatrixTargetPathPattern.Match(line);
      if (!targetPathMatch.Success)
      {
        return InventorySnapshot.Invalid(
          $"P10 design matrix row does not map into src2: {line.Trim()}");
      }

      targetPaths.Add(targetPathMatch.Groups["path"].Value);

      string[] sequenceValues = match.Groups[1].Value.Split(',');
      foreach (string sequenceValue in sequenceValues)
      {
        int sequence = int.Parse(sequenceValue.Trim());
        if (!sequences.Add(sequence))
        {
          return InventorySnapshot.Invalid(
            $"P10 design matrix contains duplicate source sequence {sequence}.");
        }

        memberCount++;
      }
    }

    return InventorySnapshot.Valid(
      sequences,
      fieldCount: 0,
      propertyCount: 0,
      memberCount: memberCount,
      targetPaths: targetPaths);
  }

  private static P10SourceInventoryVerificationResult VerifyMatrixTargetPaths(
    string repositoryRoot,
    string sourceRoot,
    IEnumerable<string> targetPaths)
  {
    string sourceRootWithSeparator =
      Path.GetFullPath(sourceRoot).TrimEnd(Path.DirectorySeparatorChar) +
      Path.DirectorySeparatorChar;

    foreach (string targetPath in targetPaths)
    {
      string relativePath = targetPath.Replace('/', Path.DirectorySeparatorChar);
      string fullPath = Path.GetFullPath(Path.Combine(repositoryRoot, relativePath));
      if (!fullPath.StartsWith(sourceRootWithSeparator, StringComparison.OrdinalIgnoreCase) ||
          !File.Exists(fullPath))
      {
        return Invalid(
          $"P10 design matrix target path does not exist under src2: {targetPath}");
      }
    }

    return new P10SourceInventoryVerificationResult(
      isValid: true,
      failureMessage: string.Empty);
  }

  private static P10SourceInventoryVerificationResult VerifySourceBoundary(
    string sourceRoot,
    string projectPath)
  {
    string projectContent = File.ReadAllText(projectPath);
    if (projectContent.Contains("<ProjectReference", StringComparison.OrdinalIgnoreCase))
    {
      return Invalid(
        "P10 production src2 project must remain isolated and cannot declare a project reference.");
    }

    string[] sourceFiles = Directory.GetFiles(sourceRoot, "*.cs", SearchOption.AllDirectories);
    if (sourceFiles.Length == 0)
    {
      return Invalid("P10 src2 production project contains no C# source files.");
    }

    foreach (string sourceFile in sourceFiles)
    {
      string sourceContent = File.ReadAllText(sourceFile);
      Match forbiddenReference = ForbiddenSourceReferencePattern.Match(sourceContent);
      if (forbiddenReference.Success)
      {
        return Invalid(
          $"P10 src2 source contains a forbidden production/Version4 reference in " +
          $"{sourceFile}: {forbiddenReference.Value}");
      }
    }

    return new P10SourceInventoryVerificationResult(
      isValid: true,
      failureMessage: string.Empty);
  }

  private static P10SourceInventoryVerificationResult Invalid(string message)
  {
    return new P10SourceInventoryVerificationResult(
      isValid: false,
      failureMessage: message);
  }

  private static string FormatSequenceSet(IEnumerable<int> sequences)
  {
    string formatted = string.Join(", ", sequences.OrderBy(sequence => sequence));
    return string.IsNullOrEmpty(formatted) ? "none" : formatted;
  }

  private sealed class InventorySnapshot
  {
    private InventorySnapshot(
      HashSet<int> sequences,
      int fieldCount,
      int propertyCount,
      int memberCount,
      HashSet<string> targetPaths,
      bool isValid,
      string failureMessage)
    {
      Sequences = sequences;
      FieldCount = fieldCount;
      PropertyCount = propertyCount;
      MemberCount = memberCount;
      TargetPaths = targetPaths;
      IsValid = isValid;
      FailureMessage = failureMessage;
    }

    public HashSet<int> Sequences { get; }

    public int FieldCount { get; }

    public int PropertyCount { get; }

    public int MemberCount { get; }

    public HashSet<string> TargetPaths { get; }

    public bool IsValid { get; }

    public string FailureMessage { get; }

    public static InventorySnapshot Valid(
      HashSet<int> sequences,
      int fieldCount,
      int propertyCount,
      int? memberCount = null,
      HashSet<string>? targetPaths = null)
    {
      return new InventorySnapshot(
        sequences,
        fieldCount,
        propertyCount,
        memberCount ?? sequences.Count,
        targetPaths ?? new HashSet<string>(StringComparer.Ordinal),
        isValid: true,
        failureMessage: string.Empty);
    }

    public static InventorySnapshot Invalid(string failureMessage)
    {
      return new InventorySnapshot(
        [],
        fieldCount: 0,
        propertyCount: 0,
        memberCount: 0,
        targetPaths: new HashSet<string>(StringComparer.Ordinal),
        isValid: false,
        failureMessage);
    }
  }
}
