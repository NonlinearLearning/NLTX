namespace Terraria.WorldStorage;

/// <summary>
/// 保存方块的液体数量和液体种类。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Tile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Tile.cs。</para>
/// <para>主要源成员：liquid（第 12 行）； bTileHeader（第 16 行）。</para>
/// <para>重组说明：Type 对应原 liquidType() 解码的液体种类，不是 Tile.type 的方块材质。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-liquid-simulation-component-design.md。</para>
/// <para>依据位置：第 89 行。</para>
/// </remarks>
public struct TileLiquidStateComponent
{
  public byte Amount;
  public byte Type;

  public bool IsEmpty => Amount == 0;
}
