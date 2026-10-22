namespace Terraria.WorldInteraction.Interaction;

/// <summary>
/// 保存发起世界交互的原版玩家槽位上下文。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Wiring。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Wiring.cs。</para>
/// <para>主要源成员：CurrentUser（第 67 行）。</para>
/// <para>重组说明：将原静态 CurrentUser 表达为单次交互的发起者上下文。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 71 行。</para>
/// <para>
/// 用途：方块、箱子或 TileEntity 操作需要来源玩家时，适配层提交 InitiatingPlayerIndex，领域流程据此读取调用者归属。
/// </para>
/// <para>
/// 当前实现：使用可空 byte 保存索引，null 表示没有可确认的玩家。原版无调用者值 255 应在适配边界转换为空，属性本身未执行该转换。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C171，L2（通用可选玩法）。分级理由：箱子、交互主体、方块锚点、身份、调度与结构占地是可选交互结构支持。普通固体地图不要求每格
/// 建实体。
/// </para>
/// </remarks>
public sealed class InteractionActorContextComponent
{
  // null 表示没有可确认的 Version4 玩家索引。
  // Version4 的兼容无调用者值为 255，但不把 255 暴露为有效玩家。
  /// <summary>
  /// 可确认的原版发起玩家索引；未知或无玩家时为空。
  /// </summary>
  public byte? InitiatingPlayerIndex { get; internal set; }
}
