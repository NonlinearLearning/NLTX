namespace Terraria.Combat;

/// <summary>
/// 保存击退免疫和击退抗性。
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

  public bool IsImmune;
  public float Resistance;

  public bool CanReceiveKnockback => !IsImmune && Resistance > 0.0f;
}
