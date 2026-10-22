namespace Terraria.WorldStorage;

/// <summary>
/// 保存方块实体的存储身份、类型与方块锚点绑定。
/// </summary>
/// <remarks>
/// <para>职责：保存方块实体的类型、锚点和身份绑定。</para>
/// <para>拆分来源：Terraria.DataStructures.TileEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/TileEntity.cs。</para>
/// <para>主要源成员：ID（第 27 行）； Position（第 29 行）； type（第 31 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P04-world-tiles-storage-component-design.md。
/// </para>
/// <para>依据位置：第 512 行。</para>
/// <para>
/// 用途：世界存储流程把已确认的方块实体编号、类型和坐标保存为一条绑定记录，供恢复或查询读取。
/// </para>
/// <para>
/// 当前实现：三个值通过位置参数构造记录。本文件只承载绑定事实，身份分配、类型注册、坐标检查和文件读写由外部流程完成。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C212，L2（通用可选玩法）。分级理由：现 TileEntityStore 使用此命名空间的绑定类型。仅在保留方块实体及其恢复路
/// 径时需要。
/// </para>
/// </remarks>
/// <param name="Id">此绑定使用的方块实体身份。</param>
/// <param name="Type">此绑定记录的方块实体类型。</param>
/// <param name="Anchor">此绑定对应的世界方块锚点。</param>
public sealed record TileEntityBindingComponent(
  TileEntityId Id,
  TileEntityTypeId Type,
  TileCoordinate Anchor);
