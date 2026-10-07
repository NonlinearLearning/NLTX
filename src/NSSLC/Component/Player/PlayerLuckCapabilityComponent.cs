namespace Terraria.Player;

/// <summary>
/// 保存玩家幸运类饰品能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：hasLuck_LuckyCoin（第 2046 行）； hasLuck_LuckyHorseshoe（第 2048 行）； hasLuck_LuckyClover（第
/// 2050 行）； hasLuck_WiltedClover（第 2052 行）； hasLuck_RavenFeather（第 2054 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1100 行。</para>
/// </remarks>
public sealed class PlayerLuckCapabilityComponent
{
  public bool HasLuckLuckyCoin { get; internal set; }

  public bool HasLuckLuckyHorseshoe { get; internal set; }

  public bool HasLuckLuckyClover { get; internal set; }

  public bool HasLuckWiltedClover { get; internal set; }

  public bool HasLuckRavenFeather { get; internal set; }

  internal void ResetEffects()
  {
    HasLuckLuckyCoin = false;
    HasLuckLuckyHorseshoe = false;
    HasLuckLuckyClover = false;
    HasLuckWiltedClover = false;
    HasLuckRavenFeather = false;
  }
}
