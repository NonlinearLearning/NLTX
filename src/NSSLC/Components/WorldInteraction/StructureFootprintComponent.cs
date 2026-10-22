using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Structures;

/// <summary>
/// 保存结构在方块网格中的原点、尺寸和宿主方块类型。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 TileObjectData 的多方块结构尺寸与锚点模型重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.ObjectData/TileObjectData.cs。</para>
/// <para>重组说明：原点和宿主方块与结构定义尺寸组合为实例占地范围。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p20-world-generation-actions-shapes-component-design.md。
/// </para>
/// <para>依据位置：第 150 行。</para>
/// <para>
/// 用途：放置、破坏和交互流程根据 Origin、Width、Height 读取结构覆盖范围，例如多格箱子或家具。
/// </para>
/// <para>
/// 当前实现：宽高默认 1。Origin 与 TileEntityAnchorComponent.Origin 的一致性、尺寸有效性和占用冲突由外部组合与结构流程维护。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C175，L2（通用可选玩法）。分级理由：箱子、交互主体、方块锚点、身份、调度与结构占地是可选交互结构支持。普通固体地图不要求每格
/// 建实体。
/// </para>
/// </remarks>
public sealed class StructureFootprintComponent
{
  // 与 TileEntityAnchorComponent.Origin 的一致性是组合不变量。
  /// <summary>
  /// 结构覆盖范围的原点，应与关联方块实体锚点一致。
  /// </summary>
  public TileCoordinate Origin { get; internal set; }

  /// <summary>
  /// 结构横向覆盖的方块数，默认 1。
  /// </summary>
  public int Width { get; internal set; } = 1;

  /// <summary>
  /// 结构纵向覆盖的方块数，默认 1。
  /// </summary>
  public int Height { get; internal set; } = 1;

  /// <summary>
  /// 结构所依附的方块类型编号，可为空。
  /// </summary>
  public ushort? HostTileType { get; internal set; }
}
