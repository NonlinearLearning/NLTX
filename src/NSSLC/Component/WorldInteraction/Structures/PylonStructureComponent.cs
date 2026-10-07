namespace Terraria.WorldInteraction.Structures;

/// <summary>
/// 保存晶塔种类、方块类型和支撑要求。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 TETeleportationPylon 的方块种类与结构有效性检查重组。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.Tile_Entities/TETeleportationPylon.cs。
/// </para>
/// <para>重组说明：方块种类和支撑要求在结构组件中显式保存。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 1157 行。</para>
/// </remarks>
public sealed class PylonStructureComponent
{
  // Version4 style -> TeleportPylonType 的具体 NLTX 类型仍未确定。
  public byte PylonKind { get; internal set; }

  public ushort TileType { get; internal set; } = 597;

  public bool RequiresSolidSupport { get; internal set; } = true;
}
