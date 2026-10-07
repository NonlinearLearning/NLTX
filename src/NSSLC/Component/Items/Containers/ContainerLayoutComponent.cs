using System.Collections.Generic;

namespace Terraria.Items;

/// <summary>
/// 保存容器槽位用途、当前选择和布局版本。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Player 的背包、弹药、金币和装备槽位布局重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>重组说明：槽位用途表与布局版本是对旧数组约定的显式表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-version4-item-container-and-economy-component-code-draft.md。
/// </para>
/// <para>依据位置：第 338 行。</para>
/// </remarks>
public sealed class ContainerLayoutComponent
{
  private readonly List<ContainerSlotRole> _slotRoles;

  public ContainerLayoutComponent(
    IReadOnlyList<ContainerSlotRole> slotRoles,
    SlotIndex? selectedSlot = null,
    long layoutRevision = 0)
  {
    _slotRoles = new List<ContainerSlotRole>(slotRoles);
    SelectedSlot = selectedSlot;
    LayoutRevision = layoutRevision;
  }

  public SlotIndex? SelectedSlot;
  public long LayoutRevision;

  public IReadOnlyList<ContainerSlotRole> SlotRoles => _slotRoles;

  public int SlotCount => _slotRoles.Count;

  public bool HasSelectedSlot =>
    SelectedSlot.HasValue &&
    SelectedSlot.Value.Value >= 0 &&
    SelectedSlot.Value.Value < _slotRoles.Count;
}
