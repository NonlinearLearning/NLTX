using Terraria.Relationships;

namespace Terraria.Items;

/// <summary>
/// 保存背包、弹药、金币、垃圾槽和当前选中槽位。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：trashItem（第 1021 行）； inventory（第 1083 行）； selectedItem（第 2960 行）。</para>
/// <para>重组说明：Revision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 55 行。</para>
/// </remarks>
public sealed class InventoryComponent
{
  private readonly List<EntityReference> _slots;
  private readonly List<EntityReference> _ammoSlots;
  private readonly List<EntityReference> _coinSlots;

  public InventoryComponent(
    IReadOnlyList<EntityReference> slots,
    int selectedSlot,
    EntityReference trashSlot = default,
    IReadOnlyList<EntityReference>? ammoSlots = null,
    IReadOnlyList<EntityReference>? coinSlots = null,
    long revision = 0)
  {
    _slots = new List<EntityReference>(slots);
    SelectedSlot = selectedSlot;
    TrashSlot = trashSlot;
    _ammoSlots = ammoSlots is null ? [] : new List<EntityReference>(ammoSlots);
    _coinSlots = coinSlots is null ? [] : new List<EntityReference>(coinSlots);
    Revision = revision;
  }

  public int SelectedSlot;
  public EntityReference TrashSlot;
  public long Revision;

  public IReadOnlyList<EntityReference> Slots => _slots;
  public IReadOnlyList<EntityReference> AmmoSlots => _ammoSlots;
  public IReadOnlyList<EntityReference> CoinSlots => _coinSlots;
  public bool HasSelectedSlot => SelectedSlot >= 0 && SelectedSlot < _slots.Count;
  public EntityReference SelectedItem => HasSelectedSlot ? _slots[SelectedSlot] : EntityReference.None;
}
