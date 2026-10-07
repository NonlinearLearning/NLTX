using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the selected enabled state for each registered generation pass.
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成步骤的启用选择表。</para>
/// <para>拆分来源：Terraria.WorldBuilding.GenPass。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenPass.cs。</para>
/// <para>主要源成员：Enabled（第 11 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 281 行。</para>
/// </remarks>
public sealed class WorldGenerationPassSelectionComponent
{
  public WorldGenerationPassSelectionComponent(
    IReadOnlyDictionary<string, bool> passEnabledById)
  {
    ArgumentNullException.ThrowIfNull(passEnabledById);

    Dictionary<string, bool> copy = new(StringComparer.Ordinal);
    foreach (KeyValuePair<string, bool> selection in passEnabledById)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(selection.Key);
      if (!copy.TryAdd(selection.Key, selection.Value))
      {
        throw new ArgumentException(
          "Pass identifiers must be unique.",
          nameof(passEnabledById));
      }
    }

    PassEnabledById = new ReadOnlyDictionary<string, bool>(copy);
  }

  public IReadOnlyDictionary<string, bool> PassEnabledById { get; }
}
