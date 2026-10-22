namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存方块实体用于存档恢复的持久化身份编号。
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
/// <para>依据位置：第 532 行。</para>
/// <para>
/// 用途：存储适配层将存档中的标识关联到恢复后的方块实体，供保存和加载流程读取。
/// </para>
/// <para>
/// 当前实现：以可空 int 暂存 PersistentId，最终身份类型仍待已有整合裁决。持久身份分配、编码和文件读写由外部存储流程处理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C179，L2（通用可选玩法）。分级理由：箱子、交互主体、方块锚点、身份、调度与结构占地是可选交互结构支持。普通固体地图不要求每格
/// 建实体。
/// </para>
/// </remarks>
public sealed class TileEntityPersistenceIdentityComponent
{
  // TODO [BD-COMP-04]:
  // 替换为整合裁决后的持久化 ID 类型。
  // 不得直接复用 TileEntityRuntimeIdComponent.Id。
  /// <summary>
  /// 方块实体的持久化身份编号，尚未确认或分配时为空。
  /// </summary>
  public int? PersistentId { get; internal set; }
}
