namespace Terraria.Player;

/// <summary>
/// 保存玩家背包、银行、垃圾槽和快捷栏选择状态。
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
public sealed class PlayerInventoryComponent
{
  public const int MainInventorySlotCount = 59;

  public ItemEntityRef[] MainInventory { get; } =
    new ItemEntityRef[MainInventorySlotCount];

  public bool[] InventoryChestStackEligibility { get; } =
    new bool[MainInventorySlotCount];

  public PlayerContainerRef Bank { get; set; }

  public PlayerContainerRef Bank2 { get; set; }

  public PlayerContainerRef Bank3 { get; set; }

  public PlayerContainerRef Bank4 { get; set; }

  public VoidVaultState VoidVaultState { get; set; }

  public ItemEntityRef TrashItem { get; set; }

  public int SelectedSlotIndex { get; set; }

  public int LastHotbarSlotIndex { get; set; }

  public int? BufferedSelectedSlotIndex { get; set; }

  public int? OverriddenSelectedSlotIndex { get; set; }
}
