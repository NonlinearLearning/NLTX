namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存方块实体在当前运行时注册体系中的身份。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.DataStructures.TileEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/TileEntity.cs。</para>
/// <para>主要源成员：ID（第 27 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-interaction-and-structures-component-design.md。
/// </para>
/// <para>依据位置：第 482 行。</para>
/// <para>
/// 用途：方块实体注册与查询流程通过 Id 定位本次运行中的对象。
/// </para>
/// <para>
/// 当前实现：只保存 TileEntityRuntimeId。运行时身份分配、释放以及其他身份与它的映射由外部注册流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C180，L2（通用可选玩法）。分级理由：箱子、交互主体、方块锚点、身份、调度与结构占地是可选交互结构支持。普通固体地图不要求每格
/// 建实体。
/// </para>
/// </remarks>
public sealed class TileEntityRuntimeIdComponent
{
  /// <summary>
  /// 当前运行时注册使用的方块实体身份。
  /// </summary>
  public TileEntityRuntimeId Id { get; internal set; }
}
