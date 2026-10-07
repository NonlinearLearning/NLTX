using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the algorithm and source revision metadata for a generation run.
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成版本、提交标识和各步骤结果清单。</para>
/// <para>拆分来源：Terraria.WorldBuilding.WorldManifest。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/WorldManifest.cs。</para>
/// <para>主要源成员：Version（第 18 行）； GitSHA（第 20 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 509 行。</para>
/// </remarks>
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
