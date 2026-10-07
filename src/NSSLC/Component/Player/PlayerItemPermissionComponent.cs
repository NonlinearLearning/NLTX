namespace Terraria.Player;

/// <summary>
/// 保存玩家持有物品、自动涂漆和自动致动权限。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：IsAllowedToHoldItems（第 1849 行）； autoPaint（第 1891 行）； autoActuator（第 1893 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1051 行。</para>
/// </remarks>
public sealed class PlayerItemPermissionComponent
{
  public bool IsAllowedToHoldItems { get; internal set; } = true;

  public bool AutoPaint { get; internal set; }

  public bool AutoActuator { get; internal set; }

  internal void ResetEffects()
  {
    IsAllowedToHoldItems = true;
    AutoPaint = false;
    AutoActuator = false;
  }
}
