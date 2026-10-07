namespace Terraria.Player;

/// <summary>
/// 保存玩家生命和魔力的当前值、基础上限与生效上限。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：statLifeMax（第 1357 行）； statLifeMax2（第 1359 行）； statLife（第 1361 行）； statMana（第 1363 行）；
/// statManaMax（第 1365 行）； statManaMax2（第 1367 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 98 行。</para>
/// </remarks>
public sealed class PlayerVitalStateComponent
{
  public int StatLifeMax { get; internal set; } = 100;

  public int StatLifeMax2 { get; internal set; } = 100;

  public int StatLife { get; internal set; } = 100;

  public int StatMana { get; internal set; }

  public int StatManaMax { get; internal set; } = 20;

  public int StatManaMax2 { get; internal set; } = 20;

  internal void ResetEffects()
  {
    StatLifeMax2 = StatLifeMax;
    StatManaMax2 = StatManaMax;
  }

  internal void ResetForLifecycle()
  {
    StatLifeMax = 100;
    StatLifeMax2 = 100;
    StatLife = 100;
    StatMana = 0;
    StatManaMax = 20;
    StatManaMax2 = 20;
  }
}
