namespace Terraria.Player;

/// <summary>
/// 保存玩家狼人、人鱼变身及其外观控制能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：wereWolf（第 1767 行）； wolfAcc（第 1769 行）； hideMerman（第 1771 行）； hideWolf（第 1773 行）；
/// forceMerman（第 1775 行）； forceWerewolf（第 1777 行）； accMerman（第 1789 行）； merman（第 1791 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 755 行。</para>
/// </remarks>
public sealed class PlayerTransformationCapabilityComponent
{
  public bool WereWolf { get; internal set; }

  public bool WolfAcc { get; internal set; }

  public bool HideMerman { get; internal set; }

  public bool HideWolf { get; internal set; }

  public bool ForceMerman { get; internal set; }

  public bool ForceWerewolf { get; internal set; }

  public bool AccMerman { get; internal set; }

  public bool Merman { get; internal set; }

  internal void ResetEffects()
  {
    WereWolf = false;
    WolfAcc = false;
    HideMerman = false;
    HideWolf = false;
    ForceMerman = false;
    ForceWerewolf = false;
    AccMerman = false;
    Merman = false;
  }
}
