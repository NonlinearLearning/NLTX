using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

string repositoryRoot = FindRepositoryRoot();
string simulationRoot = Path.Combine(repositoryRoot, "src", "Terraria.Dome.Simulation");
if (!Directory.Exists(simulationRoot))
{
  throw new DirectoryNotFoundException("Terraria.Dome.Simulation source root was not found.");
}

Regex[] forbiddenPatterns =
[
  new Regex(@"\bTerraria\.NPC\b", RegexOptions.CultureInvariant),
  new Regex(@"\bMain\.npc\b", RegexOptions.CultureInvariant),
  new Regex(@"\bNetMessage\b", RegexOptions.CultureInvariant),
  new Regex(@"\bMessageBuffer\b", RegexOptions.CultureInvariant),
  new Regex(@"\bai\s*\[", RegexOptions.CultureInvariant),
  new Regex(@"\blocalAI\s*\[", RegexOptions.CultureInvariant)
];
List<string> violations = new();
int fileCount = 0;
foreach (string file in Directory.EnumerateFiles(simulationRoot, "*.cs", SearchOption.AllDirectories))
{
  string relativePath = Path.GetRelativePath(repositoryRoot, file);
  if (relativePath.StartsWith("Build", StringComparison.OrdinalIgnoreCase))
  {
    continue;
  }

  fileCount++;
  string source = File.ReadAllText(file);
  for (int index = 0; index < forbiddenPatterns.Length; index++)
  {
    Match match = forbiddenPatterns[index].Match(source);
    if (match.Success)
    {
      violations.Add($"{relativePath}: {match.Value}");
    }
  }
}

if (violations.Count > 0)
{
  throw new InvalidOperationException(
    "Simulation contains forbidden legacy NPC dependencies:" + Environment.NewLine +
    string.Join(Environment.NewLine, violations));
}

Console.WriteLine($"PASS: checked {fileCount} Simulation source files; legacy NPC violations 0");

static string FindRepositoryRoot()
{
  DirectoryInfo? directory = new(Directory.GetCurrentDirectory());
  while (directory is not null)
  {
    if (File.Exists(Path.Combine(directory.FullName, "Terraria.Dome.sln")) &&
        Directory.Exists(Path.Combine(directory.FullName, "src")))
    {
      return directory.FullName;
    }

    directory = directory.Parent;
  }

  throw new DirectoryNotFoundException("Repository root was not found from the current directory.");
}
