using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存按步骤编号组织的世界生成启用选择表。
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
/// <para>
/// 用途：执行器按生成步骤编号读取是否启用，选择本次需要执行的步骤。
/// </para>
/// <para>
/// 当前实现：构造时复制输入，使用区分大小写的 Ordinal 键比较，并要求编号非空白且唯一。未列出步骤的默认选择由外部执行流程定义。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C193，L2（通用可选玩法）。分级理由：生成器的运行阶段、步骤计划、随机流、选择与取消仅在服务端生成世界时需要，预置地图可省去。
/// </para>
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

  /// <summary>
  /// 生成步骤编号到启用标记的只读映射，构造时复制输入。
  /// </summary>
  public IReadOnlyDictionary<string, bool> PassEnabledById { get; }
}
