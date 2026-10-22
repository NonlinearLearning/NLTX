namespace Terraria.Combat;

/// <summary>
/// 保存主体的击退免疫标记和击退响应系数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：noKnockback（第 1383 行）。</para>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：knockBackResist（第 6359 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-06-version4-combat-and-status-component-design-report.md。</para>
/// <para>依据位置：第 201 行。</para>
/// <para>
/// 用途：战斗流程读取是否免疫及 Resistance 决定能否施加击退，并按自身公式调整击退响应。
/// </para>
/// <para>
/// 当前实现：构造时直接保存参数。CanReceiveKnockback 要求未免疫且 Resistance 大于零；实际速度修改由外部系统完成。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C073，L2（通用可选玩法）。分级理由：比例减伤、击退抗性和持续恢复可在初版用固定规则替代。
/// </para>
/// </remarks>
public struct KnockbackPolicyComponent
{
  public KnockbackPolicyComponent(
    bool isImmune,
    float resistance)
  {
    IsImmune = isImmune;
    Resistance = resistance;
  }

  /// <summary>
  /// 是否完全免疫击退。
  /// </summary>
  public bool IsImmune;
  /// <summary>
  /// 沿用原版 knockBackResist 的击退响应系数；具体倍率公式由外部战斗流程解释。
  /// </summary>
  public float Resistance;

  /// <summary>
  /// 未免疫且击退响应系数大于零时返回 true。
  /// </summary>
  public bool CanReceiveKnockback => !IsImmune && Resistance > 0.0f;
}
