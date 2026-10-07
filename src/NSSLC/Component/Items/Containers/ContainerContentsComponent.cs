using System.Collections.Generic;

namespace Terraria.Items;

/// <summary>
/// 保存容器槽位中的物品引用和内容变更版本。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Chest。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Chest.cs。</para>
/// <para>主要源成员：item（第 42 行）。</para>
/// <para>重组说明：Revision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 55 行。</para>
/// </remarks>
public sealed class ContainerContentsComponent
{
  private readonly List<RuntimeEntityId?> _slots;

  public ContainerContentsComponent(
    IReadOnlyList<RuntimeEntityId?> slots,
    long revision = 0,
    long lastMutationTick = 0)
  {
    _slots = new List<RuntimeEntityId?>(slots);
    Revision = revision;
    LastMutationTick = lastMutationTick;
  }

  public long Revision;
  public long LastMutationTick;

  public IReadOnlyList<RuntimeEntityId?> Slots => _slots;

  public int OccupiedSlotCount
  {
    get
    {
      int occupiedSlotCount = 0;

      foreach (RuntimeEntityId? slot in _slots)
      {
        if (slot.HasValue && !slot.Value.IsEmpty)
        {
          occupiedSlotCount++;
        }
      }

      return occupiedSlotCount;
    }
  }

  public bool IsEmpty => OccupiedSlotCount == 0;

  public bool IsFull =>
    _slots.Count > 0 &&
    OccupiedSlotCount == _slots.Count;
}
