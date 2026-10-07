namespace Terraria.Player;

/// <summary>
/// 保存饰品提供的资源保护和免伤时间加成。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：longInvince（第 1823 行）； pStone（第 1825 行）； PhilosopherStoneDurationMultiplier（第 1827 行）；
/// manaFlower（第 1829 行）； moonLeech（第 1831 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 900 行。</para>
/// </remarks>
public sealed class PlayerAccessoryResourceProtectionComponent
{
  public static readonly float PhilosopherStoneDurationMultiplier = 0.75f;

  public bool LongInvince { get; internal set; }

  public bool PStone { get; internal set; }

  public bool ManaFlower { get; internal set; }

  public bool MoonLeech { get; internal set; }

  internal void ResetEffects()
  {
    LongInvince = false;
    PStone = false;
    ManaFlower = false;
    MoonLeech = false;
  }
}
