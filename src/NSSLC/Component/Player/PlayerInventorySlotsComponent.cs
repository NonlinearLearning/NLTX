namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-712, P09-743, P09-744
// crossSubsystemOwner: item payload and transfer ordering remain integration-review
/// <summary>
/// 保存玩家主背包槽位、箱子堆叠标记和垃圾物品。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：trashItem（第 1021 行）； inventory（第 1083 行）； selectedItem（第 2960 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-execution.md。
/// </para>
/// <para>依据位置：第 33 行。</para>
/// </remarks>
public sealed class PlayerInventorySlotsComponent
{
  public const int MainInventorySlotCount = 59;

  public ItemEntityRef[] MainInventorySlots { get; } =
    new ItemEntityRef[MainInventorySlotCount];

  public bool[] InventoryChestStackMarkers { get; } =
    new bool[MainInventorySlotCount];

  public ItemEntityRef TrashItem { get; internal set; } = ItemEntityRef.None;

  public void ClearTrashItem()
  {
    TrashItem = ItemEntityRef.None;
  }
}
