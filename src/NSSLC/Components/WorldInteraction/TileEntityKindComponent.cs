namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存方块实体所属的种类标识。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.DataStructures.TileEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/TileEntity.cs。</para>
/// <para>主要源成员：type（第 31 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P04-world-tiles-storage-component-design.md。
/// </para>
/// <para>依据位置：第 513 行。</para>
/// <para>
/// 用途：方块实体流程根据 Kind 选择相应交互、更新、存储或网络处理规则。
/// </para>
/// <para>
/// 当前实现：只保存种类值。种类注册、对应实现和宿主方块匹配由外部注册与适配流程维护。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C177，L2（通用可选玩法）。分级理由：箱子、交互主体、方块锚点、身份、调度与结构占地是可选交互结构支持。普通固体地图不要求每格
/// 建实体。
/// </para>
/// </remarks>
public sealed class TileEntityKindComponent
{
  /// <summary>
  /// 方块实体种类标识，具体值由 TileEntityKindId 定义。
  /// </summary>
  public TileEntityKindId Kind { get; internal set; }
}
