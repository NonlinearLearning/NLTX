namespace Terraria.Player;

/// <summary>
/// 保存玩家元素伤害、视野和窒息等状态标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：archery（第 1712 行）； poisoned（第 1714 行）； venom（第 1716 行）； blind（第 1718 行）； blackout（第 1720
/// 行）； headcovered（第 1722 行）； frostBurn（第 1724 行）； onFrostBurn（第 1726 行）； onFrostBurn2（第 1728 行）；
/// burned（第 1730 行）； suffocating（第 1740 行）； onFire（第 1749 行）； onFire2（第 1751 行）； onFire3（第 1753
/// 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 704 行。</para>
/// </remarks>
public sealed class PlayerElementalStatusComponent
{
  public bool Archery { get; internal set; }

  public bool Poisoned { get; internal set; }

  public bool Venom { get; internal set; }

  public bool Blind { get; internal set; }

  public bool Blackout { get; internal set; }

  public bool Headcovered { get; internal set; }

  public bool FrostBurn { get; internal set; }

  public bool OnFrostBurn { get; internal set; }

  public bool OnFrostBurn2 { get; internal set; }

  public bool Burned { get; internal set; }

  public bool Suffocating { get; internal set; }

  public bool OnFire { get; internal set; }

  public bool OnFire2 { get; internal set; }

  public bool OnFire3 { get; internal set; }

  internal void ResetEffects()
  {
    Archery = false;
    Poisoned = false;
    Venom = false;
    Blind = false;
    Blackout = false;
    Headcovered = false;
    FrostBurn = false;
    OnFrostBurn = false;
    OnFrostBurn2 = false;
    Burned = false;
    Suffocating = false;
    OnFire = false;
    OnFire2 = false;
    OnFire3 = false;
  }
}
