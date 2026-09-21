using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the algorithm and source revision metadata for a generation run.
/// </summary>
public sealed class WorldGenerationManifestComponent
{
  public WorldGenerationManifestComponent(
    string version = "",
    string gitSha = "")
  {
    ArgumentNullException.ThrowIfNull(version);
    ArgumentNullException.ThrowIfNull(gitSha);

    Version = version;
    GitSHA = gitSha;
  }

  public string Version { get; private set; }

  public string GitSHA { get; private set; }
}
