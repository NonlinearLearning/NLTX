namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存方块实体的网络同步身份。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.DataStructures.TileEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/TileEntity.cs。</para>
/// <para>主要源成员：ID（第 27 行）。</para>
/// <para>重组说明：原 TileEntity.ID 的身份职责按网络与持久用途分开表达，独立身份类型为新增设计。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-interaction-and-structures-component-code-draft.md。
/// </para>
/// <para>依据位置：第 555 行。</para>
/// </remarks>
public sealed class TileEntityNetworkIdentityComponent
{
  // TODO [BD-COMP-04]:
  // 替换为连接范围内确定的 NetworkId 类型。
  // 不得与 runtime ID 或 persistent ID 共用字段。
  public int? NetworkId { get; internal set; }
}
