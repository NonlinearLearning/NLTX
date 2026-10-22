namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存方块实体是否需要进入更新调度的标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.DataStructures.TileEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/TileEntity.cs。</para>
/// <para>主要源成员：RequiresUpdates（第 33 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P04-world-tiles-storage-component-design.md。
/// </para>
/// <para>依据位置：第 507 行。</para>
/// <para>
/// 用途：调度流程读取 RequiresUpdates，选择需要周期更新的方块实体。
/// </para>
/// <para>
/// 当前实现：只保存调度资格，默认 false。更新频率、队列组织和实际调用由外部调度器负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C181，L2（通用可选玩法）。分级理由：箱子、交互主体、方块锚点、身份、调度与结构占地是可选交互结构支持。普通固体地图不要求每格
/// 建实体。
/// </para>
/// </remarks>
public sealed class TileEntityUpdateScheduleComponent
{
  /// <summary>
  /// 此方块实体是否需要执行更新。
  /// </summary>
  public bool RequiresUpdates { get; internal set; }
}
