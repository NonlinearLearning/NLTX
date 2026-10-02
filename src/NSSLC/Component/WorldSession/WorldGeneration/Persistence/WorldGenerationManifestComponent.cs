using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the algorithm and source revision metadata for a generation run.
/// </summary>
public sealed class WorldGenerationManifestComponent
{
  private readonly List<WorldGenerationPassResultComponent> _passResults = new();

  private readonly ReadOnlyCollection<WorldGenerationPassResultComponent> _passResultsView;

  public WorldGenerationManifestComponent(
    string version = "",
    string gitSha = "",
    IEnumerable<WorldGenerationPassResultComponent>? passResults = null)
  {
    ArgumentNullException.ThrowIfNull(version);
    ArgumentNullException.ThrowIfNull(gitSha);

    if (passResults is not null)
    {
      foreach (WorldGenerationPassResultComponent result in passResults)
      {
        ArgumentNullException.ThrowIfNull(result);
        AppendPassResult(result);
      }
    }

    _passResultsView = _passResults.AsReadOnly();
    Version = version;
    GitSHA = gitSha;
  }

  public string Version { get; private set; }

  public string GitSHA { get; private set; }

  public IReadOnlyList<WorldGenerationPassResultComponent> PassResults => _passResultsView;

  internal void AppendPassResult(WorldGenerationPassResultComponent result)
  {
    ArgumentNullException.ThrowIfNull(result);
    if (result.PassIndex != _passResults.Count)
    {
      throw new InvalidOperationException("Pass results must be appended in pass order.");
    }

    if (_passResults.Count > 0 && result.GenerationId != _passResults[0].GenerationId)
    {
      throw new InvalidOperationException("A manifest cannot mix generation runs.");
    }

    _passResults.Add(result);
  }
}
