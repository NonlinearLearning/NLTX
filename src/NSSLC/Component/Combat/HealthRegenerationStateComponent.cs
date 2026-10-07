namespace Terraria.Combat;

/// <summary>
/// 保存生命恢复或持续伤害的速率、累积量和恢复进度。
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

  public int Rate;
  public int Accumulator;
  public float TimeSinceLastDamage;
  public int? ExpectedLossPerSecond;

  public bool IsRegenerating => Rate > 0;

  public bool IsDegenerating => Rate < 0;
}
