using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存方块实体在世界方块网格中的锚点。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.DataStructures.TileEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/TileEntity.cs。</para>
/// <para>主要源成员：Position（第 29 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P04-world-tiles-storage-component-design.md。
/// </para>
/// <para>依据位置：第 512 行。</para>
/// <para>
/// 用途：注册、查询和交互流程使用 Origin 将方块坐标关联到方块实体。
/// </para>
/// <para>
/// 当前实现：只保存坐标。位置合法性、结构覆盖范围、注册和移动处理由外部方块实体流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C176，L2（通用可选玩法）。分级理由：箱子、交互主体、方块锚点、身份、调度与结构占地是可选交互结构支持。普通固体地图不要求每格
/// 建实体。
/// </para>
/// </remarks>
public sealed class TileEntityAnchorComponent
{
  /// <summary>
  /// 方块实体的世界方块锚点坐标。
  /// </summary>
  public TileCoordinate Origin { get; internal set; }
}
