namespace Terraria.Player;

// Records the last persistence timestamp after an external save commit succeeds.
// It does not read clocks or perform file I/O itself.
/// <summary>
/// 保存玩家最近一次保存的时间戳。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：lastTimePlayerWasSaved（第 1148 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 366 行。</para>
/// </remarks>
public sealed class PlayerSaveCheckpointComponent
{
  public long LastSavedBinaryTimestamp { get; set; }
}
