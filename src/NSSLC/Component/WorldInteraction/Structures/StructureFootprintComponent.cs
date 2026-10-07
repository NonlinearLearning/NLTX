using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Structures;

/// <summary>
/// 保存多方块结构的原点、尺寸和宿主方块类型。
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
/// </remarks>
public sealed class StructureFootprintComponent
{
  // 与 TileEntityAnchorComponent.Origin 的一致性是组合不变量。
  public TileCoordinate Origin { get; internal set; }

  public int Width { get; internal set; } = 1;

  public int Height { get; internal set; } = 1;

  public ushort? HostTileType { get; internal set; }
}
