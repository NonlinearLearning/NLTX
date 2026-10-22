using System.Security.Cryptography;
using System.Text;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.WireCompiler;

internal static class Program
{
  private const string OutputDirectoryArgument = "--output-directory";
  private const string RepositoryRootArgument = "--repository-root";
  private const string GeneratedFactsFileName = "Version4GeneratedWireFacts.g.cs";

  private static int Main(string[] args)
  {
    if (!TryReadArguments(args, out string repositoryRoot, out string outputDirectory))
    {
      Console.Error.WriteLine(
        $"Usage: MinimalImplementation.WireCompiler {RepositoryRootArgument} <path> {OutputDirectoryArgument} <path>");
      return 2;
    }

    try
    {
      Generate(repositoryRoot, outputDirectory);
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void Generate(string repositoryRoot, string outputDirectory)
  {
    string root = Path.GetFullPath(repositoryRoot);
    string output = Path.GetFullPath(outputDirectory);
    string generatedRoot = Path.GetFullPath(Path.Combine(root, "Build", "generated"));
    if (!IsWithin(output, generatedRoot))
    {
      throw new InvalidOperationException(
        "Generated protocol source must stay under the repository Build/generated directory.");
    }

    InitializeCompilerOnlySchemaInputs();
    ProtocolManifest protocol = PacketAllDefinitions.CreateProtocol();
    CompilationResult result = new PacketDesignCompiler().Compile(protocol);
    if (!result.Success)
    {
      string diagnostics = string.Join(
        Environment.NewLine,
        result.Diagnostics.Select(static diagnostic =>
          $"{diagnostic.Severity} {diagnostic.Code} [{diagnostic.Phase}] {diagnostic.PacketId}: {diagnostic.Message}"));
      throw new InvalidOperationException("Version4 wire compilation failed:" + Environment.NewLine + diagnostics);
    }

    Directory.CreateDirectory(output);
    var generatedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (GeneratedArtifact artifact in result.Artifacts)
    {
      string fileName = Path.GetFileName(artifact.HintName);
      if (!string.Equals(fileName, artifact.HintName, StringComparison.Ordinal)
          || !fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
      {
        throw new InvalidOperationException($"Generated artifact name '{artifact.HintName}' is invalid.");
      }

      if (!generatedFileNames.Add(fileName))
      {
        throw new InvalidOperationException($"Generated artifact name '{fileName}' is duplicated.");
      }

      string targetPath = Path.GetFullPath(Path.Combine(output, fileName));
      if (!IsWithin(targetPath, output))
      {
        throw new InvalidOperationException($"Generated artifact '{fileName}' escapes its output directory.");
      }

      File.WriteAllText(targetPath, artifact.Source, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    foreach (string existingPath in Directory.EnumerateFiles(
      output,
      "*.g.cs",
      SearchOption.TopDirectoryOnly))
    {
      string fileName = Path.GetFileName(existingPath);
      if (!generatedFileNames.Contains(fileName))
      {
        File.Delete(existingPath);
      }
    }

    string fingerprintPath = Path.Combine(output, "version4-input.sha256");
    string fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(result.InputFingerprint)));
    if (!generatedFileNames.Add(GeneratedFactsFileName))
    {
      throw new InvalidOperationException(
        $"Generated artifact name '{GeneratedFactsFileName}' is duplicated.");
    }

    string generatedFacts = $$"""
      namespace MinimalImplementation.Server.Protocol;

      internal static class Version4GeneratedWireFacts
      {
        internal const string InputFingerprint = "{{fingerprint}}";
      }
      """;
    File.WriteAllText(
      Path.Combine(output, GeneratedFactsFileName),
      generatedFacts,
      new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    File.WriteAllText(fingerprintPath, fingerprint + Environment.NewLine, Encoding.ASCII);
    Console.WriteLine(
      $"Generated {result.Artifacts.Length} Version4 codec source file(s); input fingerprint {result.InputFingerprint}.");
  }

  private static void InitializeCompilerOnlySchemaInputs()
  {
    if (ProtocolInputs.IsInitialized)
    {
      return;
    }

    PacketTileEntityCodecs tileEntityCodecs = PacketTileEntityCodecsV4.Create();
    IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> moduleCodecs =
      Packet82KnownModuleCodecsV4.Create();
    if (ProtocolInputs.IsInitialized)
    {
      return;
    }

    // CPK181 requires a constructed model for schema binding. This process only emits source;
    // CSharpBackend writes runtime reads from ProtocolInputs.Instance and does not embed these
    // values. Production ProtocolInputs are created once by Version4ProtocolFactsFactory.
    _ = new ProtocolInputs(
      [],
      [],
      tileEntityCodecs,
      true,
      [],
      static (short npcType, byte? width, float? life) => 1,
      static (short projectileType) => false,
      40,
      moduleCodecs,
      0,
      static (short effectType) => false);
  }

  private static bool TryReadArguments(
    IReadOnlyList<string> args,
    out string repositoryRoot,
    out string outputDirectory)
  {
    repositoryRoot = string.Empty;
    outputDirectory = string.Empty;
    for (int index = 0; index + 1 < args.Count; index += 2)
    {
      string value = args[index + 1];
      if (args[index] == RepositoryRootArgument)
      {
        repositoryRoot = value;
      }
      else if (args[index] == OutputDirectoryArgument)
      {
        outputDirectory = value;
      }
      else
      {
        return false;
      }
    }

    return repositoryRoot.Length != 0 && outputDirectory.Length != 0 && args.Count % 2 == 0;
  }

  private static bool IsWithin(string path, string directory)
  {
    string fullDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
    string fullPath = Path.GetFullPath(path);
    return string.Equals(fullPath, fullDirectory, StringComparison.OrdinalIgnoreCase)
      || fullPath.StartsWith(fullDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
  }
}
