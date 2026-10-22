namespace Terraria.Combat;

/// <summary>
/// 保存主体生命恢复或持续伤害的速率、累积量和恢复进度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：lifeRegen（第 1369 行）； lifeRegenCount（第 1371 行）； lifeRegenTime（第 1373 行）。</para>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：lifeRegen（第 6111 行）； lifeRegenCount（第 6113 行）； lifeRegenExpectedLossPerSecond（第 6115 行）。
/// </para>
/// <para>
/// 重组说明：TimeSinceLastDamage 的原始来源 lifeRegenTime 是自然恢复进度，可被效果加速；ExpectedLossPerSecond 参与 NPC
/// 持续伤害的消费阈值和每次伤害量计算。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-06-version4-combat-and-status-component-design-report.md。</para>
/// <para>依据位置：第 113 行。</para>
/// <para>
/// 用途：外部状态系统根据 Rate 累积 Accumulator，再按规则结算生命变化；恢复进度和预期损失可供玩家与 NPC 的不同规则读取。
/// </para>
/// <para>
/// 当前实现：构造时直接保存参数。Rate 大于零表示恢复，小于零表示持续损失。TimeSinceLastDamage 的旧来源是可被效果加速的自然恢复进度，不能直接视为真
/// 实秒数。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C072，L2（通用可选玩法）。分级理由：比例减伤、击退抗性和持续恢复可在初版用固定规则替代。
/// </para>
/// </remarks>
public struct HealthRegenerationStateComponent
{
  public HealthRegenerationStateComponent(
    int rate,
    int accumulator,
    float timeSinceLastDamage = 0.0f,
    int? expectedLossPerSecond = null)
  {
    Rate = rate;
    Accumulator = accumulator;
    TimeSinceLastDamage = timeSinceLastDamage;
    ExpectedLossPerSecond = expectedLossPerSecond;
  }

  /// <summary>
  /// 生命变化速率输入；正值用于恢复，负值用于持续损失。
  /// </summary>
  public int Rate;
  /// <summary>
  /// 尚未换算为实际生命变化的累积量。
  /// </summary>
  public int Accumulator;
  /// <summary>
  /// 自然恢复进度，来源于 lifeRegenTime；增长规则由外部系统决定。
  /// </summary>
  public float TimeSinceLastDamage;
  /// <summary>
  /// NPC 持续伤害消费规则使用的预期每秒损失，可为空。
  /// </summary>
  public int? ExpectedLossPerSecond;

  /// <summary>
  /// Rate 大于零时返回 true。
  /// </summary>
  public bool IsRegenerating => Rate > 0;

  /// <summary>
  /// Rate 小于零时返回 true。
  /// </summary>
  public bool IsDegenerating => Rate < 0;
}
