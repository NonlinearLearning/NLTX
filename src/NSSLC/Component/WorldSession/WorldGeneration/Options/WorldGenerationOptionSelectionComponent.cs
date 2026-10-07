namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the selected state of one world-generation option for a run.
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

  public bool Enabled { get; private set; }

  public bool AutoGenEnabled { get; private set; }
}
