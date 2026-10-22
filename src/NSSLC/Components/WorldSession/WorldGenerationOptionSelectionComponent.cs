namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成选项是否启用及是否允许自动生成。
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成选项的启用和自动生成选择。</para>
/// <para>拆分来源：Terraria.WorldBuilding.AWorldGenerationOption。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/AWorldGenerationOption.cs。</para>
/// <para>主要源成员：AutoGenEnabled（第 14 行）； Enabled（第 16 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 19 行。</para>
/// <para>
/// 用途：选项选择和生成控制流程读取 Enabled 与 AutoGenEnabled，决定是否把相应选项纳入执行。
/// </para>
/// <para>
/// 当前实现：两个值只在构造时保存，默认 false。选项内容、适用条件和生成器调用由外部流程处理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C192，L2（通用可选玩法）。分级理由：生成器的运行阶段、步骤计划、随机流、选择与取消仅在服务端生成世界时需要，预置地图可省去。
/// </para>
/// </remarks>
public sealed class WorldGenerationOptionSelectionComponent
{
  public WorldGenerationOptionSelectionComponent(
    bool enabled = false,
    bool autoGenEnabled = false)
  {
    Enabled = enabled;
    AutoGenEnabled = autoGenEnabled;
  }

  /// <summary>
  /// 此世界生成选项是否启用。
  /// </summary>
  public bool Enabled { get; private set; }

  /// <summary>
  /// 此选项的自动生成选择是否启用。
  /// </summary>
  public bool AutoGenEnabled { get; private set; }
}
