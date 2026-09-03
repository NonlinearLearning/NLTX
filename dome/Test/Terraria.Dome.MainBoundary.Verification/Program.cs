using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

internal static class MainBoundaryVerifier
{
  private static readonly (string Name, string Pattern)[] ForbiddenPatterns =
  [
    ("Terraria.Main", "Terraria.Main"),
    ("Main.player", "Main.player"),
    ("Main.npc", "Main.npc"),
    ("Main.tile", "Main.tile"),
    ("Main.rand", "Main.rand"),
    ("Main.time", "Main.time"),
    ("Main.projectile", "Main.projectile"),
    ("Main.item", "Main.item"),
    ("Main.GameUpdateCount", "Main.GameUpdateCount"),
    ("Microsoft.Xna", "Microsoft.Xna"),
    ("System.Windows.Forms", "System.Windows.Forms"),
    ("GraphicsDevice", "GraphicsDevice"),
    ("UserInterface", "UserInterface"),
    ("socket type", "System.Net.Sockets"),
    ("TcpClient", "TcpClient"),
    ("TcpListener", "TcpListener"),
    ("UdpClient", "UdpClient"),
    ("NetworkStream", "NetworkStream"),
    ("Socket", "Socket"),
    ("WebSocket", "WebSocket"),
    ("HttpClient", "HttpClient"),
    ("HttpListener", "HttpListener"),
    ("Protocol reference", "Terraria.Dome.Protocol"),
    ("Server reference", "Terraria.Dome.Server")
  ];

  private static int Main()
  {
    string? repositoryRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
    if (repositoryRoot is null)
    {
      Console.Error.WriteLine(
        "ERROR: repository root was not found from the current directory.");
      return 2;
    }

    string simulationRoot = Path.Combine(repositoryRoot, "src", "Terraria.Dome.Simulation");
    string simulationProject = Path.Combine(
      simulationRoot,
      "Terraria.Dome.Simulation.csproj");
    if (!Directory.Exists(simulationRoot))
    {
      Console.Error.WriteLine($"ERROR: Simulation source directory is missing: {simulationRoot}");
      return 2;
    }

    if (!File.Exists(simulationProject))
    {
      Console.Error.WriteLine($"ERROR: Simulation project is missing: {simulationProject}");
      return 2;
    }

    List<string> violations = ScanSource(simulationRoot);
    violations.AddRange(ScanProjectReferences(simulationProject));
    violations.AddRange(ScanCoverageMatrix(repositoryRoot));
    violations = violations.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    Console.WriteLine($"Simulation source: {simulationRoot}");
    Console.WriteLine($"Checked source files: {CountSourceFiles(simulationRoot)}");
    Console.WriteLine($"Violations: {violations.Count}");
    foreach (string violation in violations.OrderBy(
      value => value,
      StringComparer.OrdinalIgnoreCase))
    {
      Console.WriteLine($"VIOLATION: {violation}");
    }

    if (violations.Count > 0)
    {
      return 1;
    }

    Console.WriteLine(
      "PASS: Simulation has no forbidden Main, client, transport, Protocol, or Server dependency.");
    return 0;
  }

  private static List<string> ScanSource(string simulationRoot)
  {
    List<string> violations = new();
    foreach (string path in EnumerateSourceFiles(simulationRoot))
    {
      string[] lines = File.ReadAllLines(path);
      for (int index = 0; index < lines.Length; index++)
      {
        string line = lines[index];
        foreach ((string name, string pattern) in ForbiddenPatterns)
        {
          if (line.Contains(pattern, StringComparison.Ordinal))
          {
            violations.Add($"{GetRelativePath(path)}:{index + 1}: {name}: {line.Trim()}");
          }
        }
      }
    }

    return violations;
  }

  private static List<string> ScanProjectReferences(string simulationProject)
  {
    List<string> violations = new();
    XDocument document = XDocument.Load(simulationProject);
    foreach (XElement reference in document.Descendants().Where(element =>
      element.Name.LocalName.Equals("ProjectReference", StringComparison.OrdinalIgnoreCase)))
    {
      string? include = reference.Attribute("Include")?.Value;
      if (include is null)
      {
        violations.Add(
          $"{GetRelativePath(simulationProject)}: ProjectReference has no Include attribute");
        continue;
      }

      if (include.Contains("Protocol", StringComparison.OrdinalIgnoreCase) ||
          include.Contains("Server", StringComparison.OrdinalIgnoreCase))
      {
        violations.Add(
          $"{GetRelativePath(simulationProject)}: forbidden ProjectReference: {include}");
      }
    }

    return violations;
  }

  private static List<string> ScanCoverageMatrix(string repositoryRoot)
  {
    string path = Path.Combine(
      repositoryRoot,
      "docs",
      "research",
      "2026-08-20-main-member-coverage-matrix.md");
    List<string> violations = new();
    if (!File.Exists(path))
    {
      violations.Add("coverage matrix is missing");
      return violations;
    }

    string[] lines = File.ReadAllLines(path);
    int headerIndex = Array.FindIndex(lines, line =>
      line.Contains("| Id | Source anchor | Responsibility domain | Classification |", StringComparison.Ordinal));
    if (headerIndex < 0)
    {
      violations.Add("coverage matrix header is missing");
      return violations;
    }

    HashSet<string> classifications = ["accepted", "deferred", "excluded"];
    HashSet<string> domains = new(StringComparer.OrdinalIgnoreCase);
    int rowCount = 0;
    for (int index = headerIndex + 2; index < lines.Length; index++)
    {
      string line = lines[index];
      if (!line.StartsWith("| M-", StringComparison.Ordinal))
      {
        continue;
      }

      string[] cells = line.Split('|', StringSplitOptions.TrimEntries);
      if (cells.Length < 7)
      {
        violations.Add($"coverage matrix row {index + 1} has too few columns");
        continue;
      }

      rowCount++;
      if (!classifications.Contains(cells[4]))
      {
        violations.Add($"coverage matrix row {index + 1} has invalid classification '{cells[4]}'");
      }

      if (cells[2].Length == 0 || cells[3].Length == 0 || cells[5].Length == 0)
      {
        violations.Add($"coverage matrix row {index + 1} has an empty ownership field");
      }

      domains.Add(cells[3]);
    }

    if (rowCount == 0)
    {
      violations.Add("coverage matrix has no member rows");
    }

    string[] requiredDomains =
    ["world clock/rules", "entities", "events", "server bootstrap", "persistence", "randomness"];
    foreach (string domain in requiredDomains)
    {
      if (!domains.Contains(domain))
      {
        violations.Add($"coverage matrix is missing responsibility domain '{domain}'");
      }
    }

    return violations;
  }

  private static IEnumerable<string> EnumerateSourceFiles(string simulationRoot)
  {
    EnumerationOptions options = new()
    {
      IgnoreInaccessible = false,
      RecurseSubdirectories = true,
      ReturnSpecialDirectories = false
    };

    return Directory.EnumerateFiles(simulationRoot, "*.cs", options)
      .Where(path => !IsGeneratedPath(path));
  }

  private static int CountSourceFiles(string simulationRoot)
  {
    return EnumerateSourceFiles(simulationRoot).Count();
  }

  private static bool IsGeneratedPath(string path)
  {
    return path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
      .Any(segment => segment.Equals("Build", StringComparison.OrdinalIgnoreCase) ||
                     segment.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                     segment.Equals("obj", StringComparison.OrdinalIgnoreCase));
  }

  private static string? FindRepositoryRoot(string startPath)
  {
    DirectoryInfo? directory = new(startPath);
    while (directory is not null)
    {
      if (File.Exists(Path.Combine(directory.FullName, "Terraria.Dome.sln")) &&
          File.Exists(Path.Combine(
            directory.FullName,
            "src",
            "Terraria.Dome.Simulation",
            "Terraria.Dome.Simulation.csproj")))
      {
        return directory.FullName;
      }

      directory = directory.Parent;
    }

    return null;
  }

  private static string GetRelativePath(string path)
  {
    string? repositoryRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
    return repositoryRoot is null
      ? path
      : Path.GetRelativePath(repositoryRoot, path);
  }
}
