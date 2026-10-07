namespace Terraria.Player;

/// <summary>
/// 保存玩家生命、魔力及基础和生效上限与防御。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：statDefense（第 1355 行）； statLifeMax（第 1357 行）； statLifeMax2（第 1359 行）； statLife（第 1361
/// 行）； statMana（第 1363 行）； statManaMax（第 1365 行）； statManaMax2（第 1367 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-player-gameplay-component-code-draft.md。</para>
/// <para>依据位置：第 166 行。</para>
/// </remarks>
public sealed class PlayerVitalComponent
{
  public int Life { get; set; } = 100;

  public int BaseLifeMaximum { get; set; } = 100;

  public int EffectiveLifeMaximum { get; set; } = 100;

  public int Mana { get; set; }

  public int BaseManaMaximum { get; set; }

  public int EffectiveManaMaximum { get; set; }

  // Final ownership remains under review with CombatAndStatus.
  public int Defense { get; set; }
}
