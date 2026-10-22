namespace Terraria.Player;

// Records the last persistence timestamp after an external save commit succeeds.
// It does not read clocks or perform file I/O itself.
/// <summary>
/// 保存玩家最近一次成功存档后提交的时间记录。
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
/// <para>
/// 用途：外部存档流程成功提交后写入时间值，后续保存调度或显示流程读取最近保存时间。
/// </para>
/// <para>
/// 当前实现：只保存二进制时间戳。时间获取、编码方式和文件写入由外部存档流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C142，L2（通用可选玩法）。分级理由：需要持久化或周期保存时保留，单次内存会话可省去独立时间戳。
/// </para>
/// </remarks>
public sealed class PlayerSaveCheckpointComponent
{
  /// <summary>
  /// 最近一次成功保存的二进制时间表示，具体编码由存档流程约定。
  /// </summary>
  public long LastSavedBinaryTimestamp { get; set; }
}
