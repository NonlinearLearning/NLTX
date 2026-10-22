namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存方块实体在网络同步范围内的身份编号。
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
/// <para>
/// 用途：网络适配层分配或解析同步身份，再将网络消息关联到对应方块实体。
/// </para>
/// <para>
/// 当前实现：以可空 int 暂存 NetworkId，未分配时为空。编号范围和最终类型仍由已有 TODO 记录，网络身份分配与连接生命周期由外部流程维护。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C178，L2（通用可选玩法）。分级理由：箱子、交互主体、方块锚点、身份、调度与结构占地是可选交互结构支持。普通固体地图不要求每格
/// 建实体。
/// </para>
/// </remarks>
public sealed class TileEntityNetworkIdentityComponent
{
  // TODO [BD-COMP-04]:
  // 替换为连接范围内确定的 NetworkId 类型。
  // 不得与 runtime ID 或 persistent ID 共用字段。
  /// <summary>
  /// 方块实体的网络身份编号，可为空；应按网络同步范围解释。
  /// </summary>
  public int? NetworkId { get; internal set; }
}
