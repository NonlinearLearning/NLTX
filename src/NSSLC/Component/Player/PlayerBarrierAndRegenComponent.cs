namespace Terraria.Player;

/// <summary>
/// 保存冰屏障外观计时及钯金恢复增益。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：iceBarrier（第 761 行）； iceBarrierFrame（第 789 行）； iceBarrierFrameCounter（第 791 行）；
/// palladiumRegen（第 796 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 76 行。</para>
/// </remarks>
public sealed class PlayerBarrierAndRegenComponent
{
  public bool IceBarrier { get; internal set; }

  public byte IceBarrierFrame { get; internal set; }

  public byte IceBarrierFrameCounter { get; internal set; }

  public bool PalladiumRegen { get; internal set; }
}
