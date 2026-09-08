using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

internal static class Program
{
  private const string SchemaVersion = "1.0";

  public static async Task<int> Main(string[] args)
  {
    try
    {
      ScannerOptions options = ScannerOptions.Parse(args);
      Snapshot snapshot = await ScanAsync(options);
      WriteSnapshot(snapshot, options.OutputPath);
      return snapshot.Diagnostics.Count == 0 && snapshot.Completeness == "complete" ? 0 : 2;
    }
    catch (ArgumentException exception)
    {
      Console.Error.WriteLine($"error: {exception.Message}");
      Console.Error.WriteLine(ScannerOptions.Usage);
      return 64;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine($"error: {exception.Message}");
      return 1;
    }
  }

  private static async Task<Snapshot> ScanAsync(ScannerOptions options)
  {
    TargetManifestLoadResult? targetManifestData = options.Mode == "target" ? LoadTargetManifest(options.TargetManifestPath!, options.TargetRoot, options.ProjectOrAssembly) : null;
    List<TargetManifestEntry> targetManifest = targetManifestData?.Entries ?? new List<TargetManifestEntry>();
    List<string> memberScope = options.Mode == "source" && options.MemberScopePath is not null ? LoadMemberScope(options.MemberScopePath) : new List<string>();
    List<string> files = options.Mode == "source"
      ? EnumerateSourceFiles(options.RootPath, options.IncludeGenerated, options.CoverageFilePath)
      : EnumerateTargetFiles(targetManifest, options.RootPath);
    files = files.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(path => path, StringComparer.Ordinal).ToList();

    CSharpParseOptions parseOptions = new(LanguageVersion.Preview, DocumentationMode.Parse, SourceCodeKind.Regular, options.PreprocessorSymbols);
    Encoding encoding = new UTF8Encoding(false);
    FileScanResult[] scanResults = await ReadAndParseFilesAsync(files, parseOptions, encoding, options);
    List<DiagnosticRecord> diagnostics = scanResults.SelectMany(result => result.Diagnostics).ToList();
    List<FileRecord> fileRecords = scanResults.Select(result => result.File).ToList();
    int parsedFileCount = scanResults.Count(result => result.Tree is not null);
    List<SyntaxTree> trees = scanResults.Where(result => result.Tree is not null).Select(result => result.Tree!).ToList();

    List<MetadataReference> references = LoadMetadataReferences();
    CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(
      OutputKind.DynamicallyLinkedLibrary,
      nullableContextOptions: NullableContextOptions.Enable).WithConcurrentBuild(true);
    CSharpCompilation compilation = CSharpCompilation.Create(
      "Version4MemberMigrationScannerInput",
      trees,
      references,
      compilationOptions);
    List<MemberRecord>?[] membersByScanResult = new List<MemberRecord>?[scanResults.Length];
    Parallel.ForEach(
      Enumerable.Range(0, scanResults.Length).Where(index => scanResults[index].Tree is not null),
      new ParallelOptions { MaxDegreeOfParallelism = options.MaxDegreeOfParallelism },
      index =>
    {
      try
      {
        SyntaxTree tree = scanResults[index].Tree!;
        SemanticModel model = compilation.GetSemanticModel(tree, ignoreAccessibility: true);
        SyntaxNode root = tree.GetRoot();
        List<MemberRecord> members = new();
        foreach (BaseTypeDeclarationSyntax typeDeclaration in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
        {
          INamedTypeSymbol? typeSymbol = model.GetDeclaredSymbol(typeDeclaration);
          string declaringType = typeSymbol is null ? GetSyntaxTypeName(typeDeclaration) : Display(typeSymbol);
          if (typeDeclaration is not TypeDeclarationSyntax typeSyntax)
          {
            continue;
          }
          foreach (MemberDeclarationSyntax memberDeclaration in typeSyntax.Members)
          {
            AddMembers(memberDeclaration, model, declaringType, tree, options, members);
          }
        }
        membersByScanResult[index] = members;
      }
      catch (Exception exception)
      {
        string relativePath = NormalizePath(Path.GetRelativePath(options.RootPath, files[index]));
        membersByScanResult[index] = new List<MemberRecord>();
        scanResults[index].Diagnostics.Add(new DiagnosticRecord("SEMANTIC_EXTRACTION_FAILED", "error", exception.Message, relativePath, 1, 1));
      }
    });
    diagnostics = scanResults.SelectMany(result => result.Diagnostics).ToList();
    List<MemberRecord> allMembers = membersByScanResult.SelectMany(item => item ?? new List<MemberRecord>()).ToList();

    List<MemberRecord> merged = allMembers
      .GroupBy(member => options.Mode == "source" ? member.SourceMemberId! : member.TargetMemberId!, StringComparer.Ordinal)
      .Select(group => Merge(group.ToList(), options.Mode))
      .OrderBy(member => options.Mode == "source" ? member.SourceMemberId : member.TargetMemberId, StringComparer.Ordinal)
      .ToList();
    if (memberScope.Count > 0)
    {
      HashSet<string> scopeSet = memberScope.ToHashSet(StringComparer.Ordinal);
      HashSet<string> discovered = merged.Select(member => member.SourceMemberId!).ToHashSet(StringComparer.Ordinal);
      foreach (string missing in scopeSet.Except(discovered, StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal))
      {
        diagnostics.Add(new DiagnosticRecord("SCOPE_MEMBER_NOT_FOUND", "error", $"Scoped source member was not found: {missing}", "member-scope", 1, 1));
      }
      merged = merged.Where(member => member.SourceMemberId is not null && scopeSet.Contains(member.SourceMemberId)).ToList();
    }
    if (options.Mode == "target")
    {
      Dictionary<string, TargetManifestEntry> expected = targetManifest.ToDictionary(EntryKey, StringComparer.Ordinal);
      List<MemberRecord> selected = new();
      foreach (MemberRecord member in merged)
      {
        string key = EntryKey(member);
        if (!expected.TryGetValue(key, out TargetManifestEntry? entry))
        {
          continue;
        }
        selected.Add(member with
        {
          TargetMemberId = entry.TargetMemberId,
          TargetRoot = entry.TargetRoot,
          ProjectOrAssembly = entry.ProjectOrAssembly,
        });
      }
      foreach (TargetManifestEntry entry in targetManifest)
      {
        if (!selected.Any(member => member.TargetMemberId == entry.TargetMemberId))
        {
          diagnostics.Add(new DiagnosticRecord("TARGET_MEMBER_NOT_FOUND", "error", $"Target member was not found: {entry.TargetMemberId}", entry.ComponentPath, 1, 1));
        }
      }
      merged = selected.OrderBy(member => member.TargetMemberId, StringComparer.Ordinal).ToList();
    }
    List<string> skippedFiles = scanResults.Where(result => result.Tree is null).Select(result => result.File.Path).OrderBy(path => path, StringComparer.Ordinal).ToList();
    return new Snapshot(
      SchemaVersion,
      options.Mode,
      options.RootPathDisplay,
      options.TargetRoot,
      options.ProjectOrAssembly,
      merged,
      fileRecords,
      diagnostics,
      diagnostics.Count == 0 ? "complete" : "scan-failed",
      new CompletenessRecord(files.Count, parsedFileCount, skippedFiles.Count, merged.Count),
      new ConfigurationRecord(
        "preview",
        "enable",
        "parse",
        options.PreprocessorSymbols.OrderBy(item => item, StringComparer.Ordinal).ToList(),
        "utf-8",
        "Microsoft.CodeAnalysis.CSharp 4.14.0",
        "parallel-async",
        options.MaxDegreeOfParallelism,
        compilationOptions.ConcurrentBuild),
      references.Select(reference => new ReferenceRecord(Path.GetFileName(reference.Display ?? ""), TryHashReference(reference))).OrderBy(reference => reference.Path, StringComparer.Ordinal).ToList(),
      memberScope.Count == 0 ? null : HashMemberScope(memberScope),
      targetManifestData?.CanonicalHash,
      targetManifestData?.RawFileHash);
  }

  private static async Task<FileScanResult[]> ReadAndParseFilesAsync(IReadOnlyList<string> files, CSharpParseOptions parseOptions, Encoding encoding, ScannerOptions options)
  {
    FileScanResult[] results = new FileScanResult[files.Count];
    ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = options.MaxDegreeOfParallelism };
    await Parallel.ForEachAsync(Enumerable.Range(0, files.Count), parallelOptions, async (index, cancellationToken) =>
    {
      results[index] = await ReadAndParseFileAsync(files[index], parseOptions, encoding, options, cancellationToken);
    });
    return results;
  }

  private static async Task<FileScanResult> ReadAndParseFileAsync(
    string file,
    CSharpParseOptions parseOptions,
    Encoding encoding,
    ScannerOptions options,
    CancellationToken cancellationToken)
  {
    string relativePath = NormalizePath(Path.GetRelativePath(options.RootPath, file));
    try
    {
      byte[] bytes = await File.ReadAllBytesAsync(file, cancellationToken);
      string text = Decode(bytes, encoding);
      SyntaxTree tree = CSharpSyntaxTree.ParseText(SourceText.From(text, encoding), parseOptions, file);
      ImmutableArray<Diagnostic> treeDiagnostics = tree.GetDiagnostics().ToImmutableArray();
      List<DiagnosticRecord> diagnostics = treeDiagnostics.Select(diagnostic => DiagnosticRecord.From(diagnostic, relativePath)).ToList();
      FileRecord fileRecord = new(relativePath, HashBytes(bytes), treeDiagnostics.Length == 0 ? "parsed" : "diagnostic");
      return new FileScanResult(tree, diagnostics, fileRecord);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (Exception exception)
    {
      return new FileScanResult(
        null,
        new List<DiagnosticRecord> { new("FILE_READ_OR_PARSE_FAILED", "error", exception.Message, relativePath, 1, 1) },
        new FileRecord(relativePath, null, "failed"));
    }
  }

  private static string Decode(byte[] bytes, Encoding encoding)
  {
    byte[] preamble = encoding.GetPreamble();
    int offset = preamble.Length > 0 && bytes.AsSpan().StartsWith(preamble) ? preamble.Length : 0;
    return encoding.GetString(bytes, offset, bytes.Length - offset);
  }

  private static void AddMembers(MemberDeclarationSyntax declaration, SemanticModel model, string declaringType, SyntaxTree tree, ScannerOptions options, List<MemberRecord> members)
  {
    if (declaration is FieldDeclarationSyntax field)
    {
      foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
      {
        IFieldSymbol? symbol = model.GetDeclaredSymbol(variable) as IFieldSymbol;
        string member = symbol?.Name ?? variable.Identifier.ValueText;
        string type = symbol is null ? field.Declaration.Type.ToString() : Display(symbol.Type);
        members.Add(CreateRecord("field", member, member, type, symbol, field, declaringType, tree, options));
      }
      return;
    }
    if (declaration is PropertyDeclarationSyntax property)
    {
      IPropertySymbol? symbol = model.GetDeclaredSymbol(property);
      string member = property.Identifier.ValueText;
      string type = symbol is null ? property.Type.ToString() : Display(symbol.Type);
      string signature = property.ExplicitInterfaceSpecifier is null
        ? member
        : $"{property.ExplicitInterfaceSpecifier.Name}.{member}";
      members.Add(CreateRecord("property", member, signature, type, symbol, property, declaringType, tree, options));
      return;
    }
    if (declaration is IndexerDeclarationSyntax indexer)
    {
      IPropertySymbol? symbol = model.GetDeclaredSymbol(indexer);
      string signature = symbol is null ? $"this[{indexer.ParameterList}]" : symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
      string member = symbol?.Name ?? "this";
      string type = symbol is null ? indexer.Type.ToString() : Display(symbol.Type);
      members.Add(CreateRecord("property", member, signature, type, symbol, indexer, declaringType, tree, options));
    }
  }

  private static MemberRecord CreateRecord(string kind, string member, string signature, string type, ISymbol? symbol, MemberDeclarationSyntax declaration, string declaringType, SyntaxTree tree, ScannerOptions options)
  {
    string relativePath = NormalizePath(Path.GetRelativePath(options.RootPath, tree.FilePath));
    string canonicalSignature = CanonicalSignature(signature, type, kind);
    string sourceId = $"Version4::{declaringType}::{canonicalSignature}";
    string targetId = $"{options.TargetRoot}::{options.ProjectOrAssembly}::{declaringType}::{canonicalSignature}";
    LinePositionSpan lineSpan = declaration.GetLocation().GetLineSpan().Span;
    string declarationText = declaration.ToFullString().Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
    string fingerprintInput = $"{kind}\n{declaringType}\n{canonicalSignature}\n{type}\n{Accessibility(symbol)}\n{string.Join(',', Modifiers(declaration))}\n{declarationText}";
    return new MemberRecord(
      options.Mode == "source" ? sourceId : null,
      options.Mode == "target" ? targetId : null,
      relativePath,
      declaringType,
      member,
      canonicalSignature,
      kind,
      type,
      Accessibility(symbol),
      Modifiers(declaration),
      new List<LocationRecord> { new(relativePath, lineSpan.Start.Line + 1, lineSpan.Start.Character + 1) },
      HashText(fingerprintInput),
      options.Mode == "target" ? options.TargetRoot : null,
      options.Mode == "target" ? options.ProjectOrAssembly : null,
      options.Mode == "target" ? relativePath : null,
      options.Mode == "target" ? declaringType : null);
  }

  private static MemberRecord Merge(List<MemberRecord> members, string mode)
  {
    MemberRecord first = members[0];
    List<LocationRecord> locations = members.SelectMany(member => member.Locations).OrderBy(location => location.Path, StringComparer.Ordinal).ThenBy(location => location.Line).ToList();
    return first with { Locations = locations };
  }

  private static List<string> LoadMemberScope(string path)
  {
    using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
    JsonElement value = document.RootElement;
    if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty("sourceMemberIds", out JsonElement memberIds))
    {
      value = memberIds;
    }
    if (value.ValueKind != JsonValueKind.Array)
    {
      throw new ArgumentException("member scope must be an array or an object with sourceMemberIds");
    }
    List<string> sourceMemberIds = value.EnumerateArray().Select(item => item.GetString() ?? throw new ArgumentException("member scope contains a non-string sourceMemberId")).ToList();
    if (sourceMemberIds.Count != sourceMemberIds.Distinct(StringComparer.Ordinal).Count())
    {
      throw new ArgumentException("member scope contains duplicate sourceMemberId entries");
    }
    if (sourceMemberIds.Count == 0)
    {
      throw new ArgumentException("member scope must contain at least one sourceMemberId");
    }
    return sourceMemberIds.OrderBy(item => item, StringComparer.Ordinal).ToList();
  }

  private static List<string> EnumerateSourceFiles(string rootPath, bool includeGenerated, string? coverageFilePath)
  {
    IEnumerable<string> files;
    if (coverageFilePath is null)
    {
      files = Directory.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories);
    }
    else
    {
      string[] lines = File.ReadAllLines(coverageFilePath, Encoding.UTF8);
      if (lines.Length == 0 || !lines[0].StartsWith("relative_path\t", StringComparison.Ordinal))
      {
        throw new ArgumentException("coverage file must start with the relative_path TSV column");
      }
      IEnumerable<string> relativePaths = lines.Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split('\t')[0].Trim());
      List<string> resolvedFiles = relativePaths.Select(relativePath => ResolveContainedPath(rootPath, relativePath, "coverage file")).ToList();
      if (resolvedFiles.Count != resolvedFiles.Distinct(StringComparer.OrdinalIgnoreCase).Count())
      {
        throw new ArgumentException("coverage file contains duplicate source paths");
      }
      files = resolvedFiles;
      string[] missing = files.Where(file => !File.Exists(file)).ToArray();
      if (missing.Length > 0)
      {
        throw new ArgumentException($"coverage file contains missing source files: {string.Join(", ", missing.Take(5))}");
      }
    }
    if (!includeGenerated)
    {
      files = files.Where(file => !IsGeneratedPath(file) && !LooksGenerated(file));
    }
    return files.ToList();
  }

  private static TargetManifestLoadResult LoadTargetManifest(string manifestPath, string targetRoot, string projectOrAssembly)
  {
    byte[] manifestBytes = File.ReadAllBytes(manifestPath);
    using JsonDocument document = JsonDocument.Parse(manifestBytes);
    if (!document.RootElement.TryGetProperty("targetMembers", out JsonElement members) || members.ValueKind != JsonValueKind.Array)
    {
      throw new ArgumentException("target manifest must contain an explicit targetMembers array");
    }
    List<TargetManifestEntry> result = new();
    List<JsonElement> rawEntries = new();
    HashSet<string> targetIds = new(StringComparer.Ordinal);
    foreach (JsonElement member in members.EnumerateArray())
    {
      TargetManifestEntry entry = TargetManifestEntry.Parse(member);
      if (!string.Equals(entry.TargetRoot, targetRoot, StringComparison.Ordinal) || !string.Equals(entry.ProjectOrAssembly, projectOrAssembly, StringComparison.Ordinal))
      {
        throw new ArgumentException($"target manifest identity root/assembly does not match scanner options: {entry.TargetMemberId}");
      }
      if (!targetIds.Add(entry.TargetMemberId))
      {
        throw new ArgumentException($"target manifest contains duplicate targetMemberId: {entry.TargetMemberId}");
      }
      result.Add(entry);
      rawEntries.Add(member.Clone());
    }
    if (result.Count == 0)
    {
      throw new ArgumentException("target manifest must contain at least one target member");
    }
    return new TargetManifestLoadResult(
      result,
      CanonicalManifestHash(rawEntries),
      HashBytes(manifestBytes));
  }

  private static List<string> EnumerateTargetFiles(List<TargetManifestEntry> manifest, string rootPath)
  {
    List<string> result = new();
    foreach (string relative in manifest.Select(item => item.ComponentPath).Distinct(StringComparer.Ordinal))
    {
      string path = ResolveContainedPath(rootPath, relative, "target manifest");
      if (!File.Exists(path))
      {
        throw new ArgumentException($"target manifest file does not exist: {path}");
      }
      result.Add(path);
    }
    return result;
  }

  private static string ResolveContainedPath(string rootPath, string relativePath, string sourceLabel)
  {
    if (Path.IsPathRooted(relativePath))
    {
      throw new ArgumentException($"{sourceLabel} path must be relative to root: {relativePath}");
    }
    string fullPath = Path.GetFullPath(Path.Combine(rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));
    string rootWithSeparator = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
    if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
    {
      throw new ArgumentException($"{sourceLabel} path is outside root: {relativePath}");
    }
    return fullPath;
  }

  private static string EntryKey(TargetManifestEntry entry) => $"{entry.ComponentPath}|{entry.ComponentType}|{entry.Member}|{entry.Signature}|{entry.Kind}";

  private static string EntryKey(MemberRecord member) => $"{member.Path}|{member.DeclaringType}|{member.Member}|{member.Signature}|{member.Kind}";

  private static List<MetadataReference> LoadMetadataReferences()
  {
    string? trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
    if (string.IsNullOrWhiteSpace(trustedAssemblies))
    {
      return new List<MetadataReference>();
    }
    return trustedAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
      .Distinct(StringComparer.OrdinalIgnoreCase)
      .OrderBy(path => path, StringComparer.Ordinal)
      .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
      .ToList();
  }

  private static string? TryHashReference(MetadataReference reference)
  {
    if (reference is not PortableExecutableReference pe || string.IsNullOrWhiteSpace(pe.FilePath) || !File.Exists(pe.FilePath))
    {
      return null;
    }
    return HashBytes(File.ReadAllBytes(pe.FilePath));
  }

  private static bool IsGeneratedPath(string path)
  {
    string normalized = NormalizePath(path).ToLowerInvariant();
    return normalized.Contains("/bin/") || normalized.Contains("/obj/") || normalized.Contains("/build/generated/");
  }

  private static bool LooksGenerated(string path)
  {
    using StreamReader reader = new(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
    char[] buffer = new char[2048];
    int count = reader.ReadBlock(buffer, 0, buffer.Length);
    string prefix = new(buffer, 0, count);
    return prefix.Contains("<auto-generated", StringComparison.OrdinalIgnoreCase);
  }

  private static string GetSyntaxTypeName(BaseTypeDeclarationSyntax declaration)
  {
    IEnumerable<string> names = declaration.Ancestors().OfType<BaseTypeDeclarationSyntax>().Reverse().Select(item => item.Identifier.ValueText).Append(declaration.Identifier.ValueText);
    IEnumerable<string> namespaces = declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(item => item.Name.ToString());
    string prefix = string.Join('.', namespaces.Append(string.Empty));
    return prefix + string.Join('.', names);
  }

  private static string CanonicalSignature(string signature, string type, string kind)
  {
    return signature.Trim().Replace("\r", "", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal).Replace("  ", " ", StringComparison.Ordinal);
  }

  private static string Display(ITypeSymbol symbol) => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "", StringComparison.Ordinal);

  private static string Display(INamedTypeSymbol symbol) => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "", StringComparison.Ordinal);

  private static string Accessibility(ISymbol? symbol) => symbol?.DeclaredAccessibility switch
  {
    Microsoft.CodeAnalysis.Accessibility.Public => "public",
    Microsoft.CodeAnalysis.Accessibility.Private => "private",
    Microsoft.CodeAnalysis.Accessibility.Protected => "protected",
    Microsoft.CodeAnalysis.Accessibility.Internal => "internal",
    Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal => "protected-internal",
    Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal => "private-protected",
    _ => "none",
  };

  private static List<string> Modifiers(MemberDeclarationSyntax declaration) => declaration.Modifiers.Select(modifier => modifier.Text).OrderBy(item => item, StringComparer.Ordinal).ToList();

  private static string NormalizePath(string path) => path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');

  private static string HashText(string value) => HashBytes(Encoding.UTF8.GetBytes(value));

  private static string HashMemberScope(IReadOnlyCollection<string> memberScope)
  {
    JsonSerializerOptions options = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    string canonical = JsonSerializer.Serialize(memberScope.OrderBy(item => item, StringComparer.Ordinal).ToList(), options) + "\n";
    return HashText(canonical);
  }

  private static string CanonicalManifestHash(IEnumerable<JsonElement> entries)
  {
    using MemoryStream stream = new();
    using (Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
    {
      writer.WriteStartArray();
      foreach (JsonElement entry in entries.OrderBy(item => item.GetProperty("targetMemberId").GetString(), StringComparer.Ordinal))
      {
        WriteCanonicalJson(entry, writer);
      }
      writer.WriteEndArray();
      writer.Flush();
    }
    stream.WriteByte((byte)'\n');
    return HashBytes(stream.ToArray());
  }

  private static void WriteCanonicalJson(JsonElement value, Utf8JsonWriter writer)
  {
    switch (value.ValueKind)
    {
      case JsonValueKind.Object:
        writer.WriteStartObject();
        foreach (JsonProperty property in value.EnumerateObject().OrderBy(item => item.Name, StringComparer.Ordinal))
        {
          writer.WritePropertyName(property.Name);
          WriteCanonicalJson(property.Value, writer);
        }
        writer.WriteEndObject();
        break;
      case JsonValueKind.Array:
        writer.WriteStartArray();
        foreach (JsonElement item in value.EnumerateArray())
        {
          WriteCanonicalJson(item, writer);
        }
        writer.WriteEndArray();
        break;
      case JsonValueKind.String:
        writer.WriteStringValue(value.GetString());
        break;
      case JsonValueKind.Number:
        writer.WriteRawValue(value.GetRawText(), skipInputValidation: true);
        break;
      case JsonValueKind.True:
        writer.WriteBooleanValue(true);
        break;
      case JsonValueKind.False:
        writer.WriteBooleanValue(false);
        break;
      case JsonValueKind.Null:
        writer.WriteNullValue();
        break;
      default:
        throw new ArgumentException($"Unsupported JSON value kind: {value.ValueKind}");
    }
  }

  private static string HashBytes(byte[] bytes) => $"sha256:{Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()}";

  private static void WriteSnapshot(Snapshot snapshot, string outputPath)
  {
    JsonSerializerOptions serializerOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    string json = JsonSerializer.Serialize(snapshot, serializerOptions) + "\n";
    string fullOutputPath = Path.GetFullPath(outputPath);
    string outputDirectory = Path.GetDirectoryName(fullOutputPath)!;
    Directory.CreateDirectory(outputDirectory);
    string temporaryPath = Path.Combine(outputDirectory, $".{Path.GetFileName(fullOutputPath)}.{Guid.NewGuid():N}.tmp");
    try
    {
      File.WriteAllText(temporaryPath, json, new UTF8Encoding(false));
      File.Move(temporaryPath, fullOutputPath, overwrite: true);
    }
    finally
    {
      if (File.Exists(temporaryPath))
      {
        File.Delete(temporaryPath);
      }
    }
  }
}

internal sealed record ScannerOptions(string Mode, string RootPath, string RootPathDisplay, string OutputPath, string? TargetManifestPath, string TargetRoot, string ProjectOrAssembly, bool IncludeGenerated, List<string> PreprocessorSymbols, string? MemberScopePath, string? CoverageFilePath, string? MemberScopeHash, int MaxDegreeOfParallelism)
{
  private const int DefaultMaxDegreeOfParallelism = 4;

  public const string Usage = "Usage: Version4MemberMigrationScanner --mode source|target --root <path> --output <path> [--coverage-file <path>] [--member-scope <path>] [--target-manifest <path>] [--target-root <id>] [--project-or-assembly <id>] [--max-degree-of-parallelism <count>] [--include-generated] [--define <symbol>]";

  public static ScannerOptions Parse(string[] args)
  {
    string? mode = null;
    string? root = null;
    string? output = null;
    string? manifest = null;
    string? memberScope = null;
    string? coverageFile = null;
    string targetRoot = "src";
    string assembly = "unknown";
    int maxDegreeOfParallelism = DefaultMaxDegreeOfParallelism;
    bool includeGenerated = false;
    List<string> symbols = new();
    for (int index = 0; index < args.Length; index++)
    {
      string argument = args[index];
      string Next() => index + 1 < args.Length ? args[++index] : throw new ArgumentException($"missing value for {argument}");
      switch (argument)
      {
        case "--mode": mode = Next(); break;
        case "--root": root = Next(); break;
        case "--output": output = Next(); break;
        case "--target-manifest": manifest = Next(); break;
        case "--member-scope": memberScope = Next(); break;
        case "--coverage-file": coverageFile = Next(); break;
        case "--target-root": targetRoot = Next(); break;
        case "--project-or-assembly": assembly = Next(); break;
        case "--max-degree-of-parallelism": maxDegreeOfParallelism = ParseMaxDegreeOfParallelism(Next()); break;
        case "--include-generated": includeGenerated = true; break;
        case "--define": symbols.Add(Next()); break;
        case "--help": throw new ArgumentException(Usage);
        default: throw new ArgumentException($"unknown argument: {argument}");
      }
    }
    if (mode is not ("source" or "target")) throw new ArgumentException("--mode must be source or target");
    if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("--root is required");
    if (string.IsNullOrWhiteSpace(output)) throw new ArgumentException("--output is required");
    if (mode == "target" && string.IsNullOrWhiteSpace(manifest)) throw new ArgumentException("--target-manifest is required for target mode");
    string fullRoot = Path.GetFullPath(root);
    if (!Directory.Exists(fullRoot)) throw new ArgumentException($"root does not exist: {fullRoot}");
    string? fullMemberScope = memberScope is null ? null : Path.GetFullPath(memberScope);
    string? fullCoverageFile = coverageFile is null ? null : Path.GetFullPath(coverageFile);
    if (fullMemberScope is not null && !File.Exists(fullMemberScope)) throw new ArgumentException($"member scope does not exist: {fullMemberScope}");
    if (fullCoverageFile is not null && !File.Exists(fullCoverageFile)) throw new ArgumentException($"coverage file does not exist: {fullCoverageFile}");
    return new ScannerOptions(mode, fullRoot, NormalizePath(fullRoot), Path.GetFullPath(output), manifest is null ? null : Path.GetFullPath(manifest), targetRoot, assembly, includeGenerated, symbols, fullMemberScope, fullCoverageFile, null, maxDegreeOfParallelism);
  }

  private static int ParseMaxDegreeOfParallelism(string value)
  {
    if (!int.TryParse(value, out int result) || result is < 1 or > 64)
    {
      throw new ArgumentException("--max-degree-of-parallelism must be between 1 and 64");
    }
    return result;
  }

  private static string NormalizePath(string path) => path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');

  private static string HashBytes(byte[] bytes) => $"sha256:{Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()}";
}

internal sealed record Snapshot(string SchemaVersion, string SnapshotKind, string Root, string TargetRoot, string ProjectOrAssembly, List<MemberRecord> Members, List<FileRecord> Files, List<DiagnosticRecord> Diagnostics, string Completeness, CompletenessRecord CompletenessDetails, ConfigurationRecord Configuration, List<ReferenceRecord> ReferenceManifest, string? MemberScopeSha256, string? TargetManifestSha256, string? TargetManifestFileSha256);

internal sealed record MemberRecord(string? SourceMemberId, string? TargetMemberId, string Path, string DeclaringType, string Member, string Signature, string Kind, string Type, string Accessibility, List<string> Modifiers, List<LocationRecord> Locations, string SourceFingerprint, string? TargetRoot, string? ProjectOrAssembly, string? ComponentPath, string? ComponentType);

internal sealed record FileRecord(string Path, string? Sha256, string ParseStatus);

internal sealed record FileScanResult(SyntaxTree? Tree, List<DiagnosticRecord> Diagnostics, FileRecord File);

internal sealed record LocationRecord(string Path, int Line, int Column);

internal sealed record DiagnosticRecord(string Id, string Severity, string Message, string Path, int Line, int Column)
{
  public static DiagnosticRecord From(Diagnostic diagnostic, string path)
  {
    FileLinePositionSpan span = diagnostic.Location.GetLineSpan();
    return new DiagnosticRecord(diagnostic.Id, diagnostic.Severity.ToString().ToLowerInvariant(), diagnostic.GetMessage(), path, span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1);
  }
}

internal sealed record CompletenessRecord(int RequestedFiles, int ParsedFiles, int SkippedFiles, int MemberCount);

internal sealed record ConfigurationRecord(string LanguageVersion, string NullableContext, string DocumentationMode, List<string> PreprocessorSymbols, string Encoding, string RoslynVersion, string FileReadStrategy, int MaxDegreeOfParallelism, bool RoslynConcurrentBuild);

internal sealed record ReferenceRecord(string Path, string? Sha256);

internal sealed record TargetManifestLoadResult(List<TargetManifestEntry> Entries, string CanonicalHash, string RawFileHash);

internal sealed record TargetManifestEntry(string TargetMemberId, string ComponentPath, string ComponentType, string Member, string Signature, string Kind, string TargetRoot, string ProjectOrAssembly)
{
  public static TargetManifestEntry Parse(JsonElement element)
  {
    string targetMemberId = Required(element, "targetMemberId");
    string componentPath = Required(element, "componentPath");
    string componentType = Required(element, "componentType");
    string member = Required(element, "member");
    string signature = Required(element, "signature");
    string kind = Required(element, "kind");
    string[] segments = targetMemberId.Split("::", StringSplitOptions.None);
    if (segments.Length != 4 || segments.Any(segment => string.IsNullOrWhiteSpace(segment) || segment.Contains('/') || segment.Contains('\\')))
    {
      throw new ArgumentException($"targetMemberId must contain exactly four identity segments: {targetMemberId}");
    }
    if (element.TryGetProperty("projectOrAssembly", out JsonElement declaredAssembly) &&
        (declaredAssembly.ValueKind != JsonValueKind.String ||
         !string.Equals(declaredAssembly.GetString(), segments[1], StringComparison.Ordinal)))
    {
      throw new ArgumentException($"target manifest projectOrAssembly does not match targetMemberId: {targetMemberId}");
    }
    string expectedId = $"{segments[0]}::{segments[1]}::{componentType}::{signature}";
    if (!string.Equals(targetMemberId, expectedId, StringComparison.Ordinal))
    {
      throw new ArgumentException($"targetMemberId does not match componentType and signature: {targetMemberId}");
    }
    return new TargetManifestEntry(targetMemberId, Normalize(componentPath), componentType, member, signature, kind, segments[0], segments[1]);
  }

  private static string Required(JsonElement element, string name)
  {
    if (!element.TryGetProperty(name, out JsonElement value) || value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
    {
      throw new ArgumentException($"target manifest member requires {name}");
    }
    return value.GetString()!;
  }

  private static string Normalize(string value) => value.Replace('\\', '/');
}
