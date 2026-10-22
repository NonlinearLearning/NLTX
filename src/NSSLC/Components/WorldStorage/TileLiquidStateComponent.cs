namespace Terraria.WorldStorage;

/// <summary>
/// 保存单个方块格中的液体数量和液体类型编号。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Tile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Tile.cs。</para>
/// <para>主要源成员：liquid（第 12 行）； bTileHeader（第 16 行）。</para>
/// <para>重组说明：Type 对应原 liquidType() 解码的液体种类，不是 Tile.type 的方块材质。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-liquid-simulation-component-design.md。</para>
/// <para>依据位置：第 89 行。</para>
/// <para>
/// 用途：液体求解和世界存储流程读取或修改格子的液体数量，再按类型应用流动和环境规则。
/// </para>
/// <para>
/// 当前实现：Amount 与 Type 都是字节字段，IsEmpty 只判断 Amount 是否为零。类型有效性和液体转移由外部流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C213，L2（通用可选玩法）。分级理由：液体缓冲、排序、工作项、脏区块、预算和流动状态只在启用液体传播时需要。
/// </para>
/// </remarks>
public struct TileLiquidStateComponent
{
  /// <summary>
  /// 此方块格的液体数量字节值，零表示无液体。
  /// </summary>
  public byte Amount;
  /// <summary>
  /// 液体类型编号，具体映射由液体规则定义。
  /// </summary>
  public byte Type;

  /// <summary>
  /// 液体数量为零时返回 true。
  /// </summary>
  public bool IsEmpty => Amount == 0;
}
