namespace Terraria.Combat;

/// <summary>
/// 保存伤害结算使用的防御值。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：statDefense（第 1355 行）。</para>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：defense（第 6325 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 50 行。</para>
/// </remarks>
public struct DefenseComponent
{
  public DefenseComponent(int value)
  {
    Value = value;
  }

  public int Value;

  public bool HasValue => Value != 0;
}
