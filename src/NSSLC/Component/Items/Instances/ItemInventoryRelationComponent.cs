using Terraria.Relationships;

namespace Terraria.Items;

public enum ItemInventorySlotKind : byte
{
  MainInventory,
  Trash,
}

/// <summary>
/// 保存物品实例与玩家背包槽位之间的归属关系。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Player.inventory 的槽位归属模型重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>重组说明：玩家实体引用、槽位种类和槽位索引组成拆分时新增的显式关系。</para>
/// </remarks>
public readonly record struct ItemInventoryRelationComponent
{
  public ItemInventoryRelationComponent(
    EntityReference playerReference,
    ItemInventorySlotKind slotKind,
    int slotIndex)
  {
    if (playerReference.IsEmpty || playerReference.Scope != EntityReferenceScope.Player)
    {
      throw new ArgumentException(
        "An item inventory relation requires a Player-scoped reference.",
        nameof(playerReference));
    }

    if (slotKind == ItemInventorySlotKind.MainInventory)
    {
      ArgumentOutOfRangeException.ThrowIfNegative(slotIndex);
    }
    else if (slotKind == ItemInventorySlotKind.Trash && slotIndex != 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(slotIndex),
        "The Trash relation uses slot index zero.");
    }
    else if (!Enum.IsDefined(slotKind))
    {
      throw new ArgumentOutOfRangeException(nameof(slotKind));
    }

    PlayerReference = playerReference;
    SlotKind = slotKind;
    SlotIndex = slotIndex;
  }

  public EntityReference PlayerReference { get; }

  public ItemInventorySlotKind SlotKind { get; }

  public int SlotIndex { get; }
}
