using System;

namespace Terraria.Items;

/// <summary>
/// 保存方块实体的类型、锚点和身份绑定。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.DataStructures.TileEntity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/TileEntity.cs。</para>
/// <para>主要源成员：ID（第 27 行）； Position（第 29 行）； type（第 31 行）。</para>
/// <para>重组说明：持久身份字段是 NLTX 新增的绑定表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/baseline/。</para>
/// <para>拆分依据文件：Version4物品容器与经济事务组件设计.md。</para>
/// <para>依据位置：第 245 行。</para>
/// </remarks>
public sealed class TileEntityBindingComponent
{
  public TileEntityBindingComponent(
    int tileEntityKind,
    TileCoordinates tilePosition,
    Guid persistentTileEntityId = default)
  {
    TileEntityKind = tileEntityKind;
    TilePosition = tilePosition;
    PersistentTileEntityId = persistentTileEntityId;
  }

  public int TileEntityKind;
  public TileCoordinates TilePosition;
  public Guid PersistentTileEntityId;

  public bool IsBound => TileEntityKind > 0 && PersistentTileEntityId != Guid.Empty;
}
