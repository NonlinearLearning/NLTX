namespace Terraria.Player;

/// <summary>
/// 保存玩家便携凳的使用、增高和显示偏移状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.DataStructures.PortableStoolUsage。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/PortableStoolUsage.cs。</para>
/// <para>
/// 主要源成员：HasAStool（第 5 行）； IsInUse（第 7 行）； HeightBoost（第 9 行）； VisualYOffset（第 11 行）；
/// MapYOffset（第 13 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 480 行。</para>
/// </remarks>
public sealed class PlayerPortableStoolStateComponent
{
  public bool HasAStool { get; internal set; }

  public bool IsInUse { get; internal set; }

  public int HeightBoost { get; internal set; }

  public int VisualYOffset { get; internal set; }

  public int MapYOffset { get; internal set; }

  internal void ResetForLifecycle()
  {
    HasAStool = false;
    IsInUse = false;
    HeightBoost = 0;
    VisualYOffset = 0;
    MapYOffset = 0;
  }
}
