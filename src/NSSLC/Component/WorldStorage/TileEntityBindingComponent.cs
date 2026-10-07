namespace Terraria.WorldStorage;

/// <summary>
/// Binds a TileEntity's persistent ID and tile-space anchor to its EntityRuntime root.
/// The anchor remains in tile coordinates and does not imply pixel-space state.
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
/// </remarks>
public sealed record TileEntityBindingComponent(
  TileEntityId Id,
  TileEntityTypeId Type,
  TileCoordinate Anchor);
